import express from 'express';
import { createServer } from 'http';
import { Server } from 'socket.io';
import amqp from 'amqplib';
import cors from 'cors';

const app = express();
app.use(cors());

// Middleware para logging
app.use((req, res, next) => {
  console.log(`${new Date().toISOString()} - ${req.method} ${req.url}`);
  next();
});

// Error handling middleware
app.use((err: any, req: any, res: any, next: any) => {
  console.error('Error:', err);
  res.status(500).json({ error: 'Internal Server Error' });
});

const httpServer = createServer(app);
const io = new Server(httpServer, {
  cors: {
    origin: "http://localhost:5178",
    methods: ["GET", "POST"],
    credentials: true
  },
  transports: ['websocket', 'polling'],
  pingTimeout: 60000,
  pingInterval: 25000,
  connectTimeout: 60000,
  allowEIO3: true
});

const RABBITMQ_URL = 'amqp://guest:guest@localhost';
const QUEUES = ['emergencia.alerts', 'enfermeria.alerts', 'mantenimiento.alerts'];

const AlertType = {
  Emergencia: 0,
  Enfermeria: 1,
  Mantenimiento: 2
};

const AlertSeverity = {
  Baja: 0,
  Media: 1,
  Alta: 2,
  Critica: 3
};

const getAlertType = (type: number | string): string => {
  if (typeof type === 'number') {
    return Object.keys(AlertType)[type] || 'Desconocido';
  }
  return type;
};

const getAlertSeverity = (severity: number | string): string => {
  if (typeof severity === 'number') {
    return Object.keys(AlertSeverity)[severity] || 'Desconocido';
  }
  return severity;
};

async function connectToRabbitMQ() {
  try {
    const connection = await amqp.connect(RABBITMQ_URL);
    const channel = await connection.createChannel();

    // Asegurar que las colas existen
    for (const queue of QUEUES) {
      await channel.assertQueue(queue, { durable: true });
    }

    // Consumir mensajes de todas las colas
    for (const queue of QUEUES) {      channel.consume(queue, (msg) => {
        if (msg) {
          try {            const alertData = JSON.parse(msg.content.toString());
            console.log('Received alert data:', alertData);
            
            // Asegurar que la fecha sea válida
            const now = new Date().toISOString();
            let timestamp = now;
            
            try {
              if (alertData.Timestamp) {
                // Si es un objeto Date de C#, vendrá en este formato "/Date(1621123456789)/"
                if (typeof alertData.Timestamp === 'string' && alertData.Timestamp.includes('/Date(')) {
                  const match = alertData.Timestamp.match(/\/Date\((\d+)\)\//);
                  if (match) {
                    timestamp = new Date(parseInt(match[1])).toISOString();
                  }
                } else {
                  timestamp = new Date(alertData.Timestamp).toISOString();
                }
              }
            } catch (error) {
              console.error('Error parsing timestamp:', error);
              timestamp = now;
            }

            // Normalizar los campos
            const normalizedAlertData = {
              id: alertData.Id || alertData.id,
              type: getAlertType(alertData.Type ?? alertData.type),
              severity: getAlertSeverity(alertData.Severity ?? alertData.severity),
              message: alertData.Message || alertData.message || 'Sin mensaje',
              location: alertData.Location || alertData.location || 'Sin ubicación',
              createdBy: alertData.CreatedBy || alertData.createdBy || 'Sistema',
              timestamp: timestamp
            };
            io.emit('alert', normalizedAlertData);
            console.log(`[${new Date().toISOString()}] Alerta emitida:`, normalizedAlertData);
          } catch (error) {
            console.error(`Error procesando mensaje de ${queue}:`, error);
          }
          channel.ack(msg);
        }
      });
    }

    console.log('🚀 Conectado a RabbitMQ y escuchando mensajes');
  } catch (error) {
    console.error('Error conectando a RabbitMQ:', error);
    process.exit(1);
  }
}

// Manejo de errores global
process.on('unhandledRejection', (error) => {
  console.error('Error no manejado:', error);
});

// Endpoints de diagnóstico
app.get('/health', (req, res) => {
  res.json({ status: 'ok', timestamp: new Date().toISOString() });
});

app.get('/status', (req, res) => {
  res.json({
    server: 'running',
    connections: io.engine.clientsCount,
    uptime: process.uptime(),
    timestamp: new Date().toISOString()
  });
});

// Socket.IO connection logging middleware
io.use((socket, next) => {
  console.log(`[${new Date().toISOString()}] Intento de conexión - ID: ${socket.id}`);
  next();
});

// Socket.IO event handlers
io.on('connection', (socket) => {
  console.log(`[${new Date().toISOString()}] Cliente conectado - ID: ${socket.id}`);

  socket.emit('welcome', { message: 'Conexión establecida con éxito' });

  socket.on('disconnect', (reason) => {
    console.log(`[${new Date().toISOString()}] Cliente desconectado - ID: ${socket.id}, Razón: ${reason}`);
  });

  socket.on('error', (error) => {
    console.error(`[${new Date().toISOString()}] Error en socket ${socket.id}:`, error);
  });
});

// RabbitMQ event handlers
process.on('SIGINT', () => {
  console.log('Cerrando servidor...');
  process.exit();
});

// Puerto y inicio del servidor
const PORT = process.env.PORT || 3001;
httpServer.listen(PORT, () => {
  console.log(`[${new Date().toISOString()}] 🚀 Servidor escuchando en puerto ${PORT}`);
  connectToRabbitMQ();
});
