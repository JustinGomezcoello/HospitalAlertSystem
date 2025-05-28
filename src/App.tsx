import React, { useState, useEffect } from 'react';
import { ThemeProvider, createTheme, CssBaseline, Container, Box, Alert, Snackbar } from '@mui/material';
import Header from './components/Header';
import AlertTable from './components/AlertTable';
import io from 'socket.io-client';

interface AlertData {
  id: string | number;
  type: string | number;
  severity: string | number;
  message: string;
  location: string;
  createdBy: string;
  timestamp: string;
}

const theme = createTheme({
  palette: {
    primary: {
      main: '#1976d2',
    },
    background: {
      default: '#f5f5f5',
    },
  },
});

function App() {
  const [alerts, setAlerts] = useState<AlertData[]>([]);
  const [isMonitoring, setIsMonitoring] = useState(false);
  const [isConnecting, setIsConnecting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [socket, setSocket] = useState<any>(null);
  const [lastAlert, setLastAlert] = useState<{ severity: string; timestamp: string; }>();
  const [alertStats, setAlertStats] = useState<{ Critica: number; Media: number; Baja: number; }>({
    Critica: 0,
    Media: 0,
    Baja: 0
  });

  const getSeverityLabel = (severity: string | number) => {
    const severities = ['Baja', 'Media', 'Alta', 'Critica'];
    if (typeof severity === 'number' && severity >= 0 && severity < severities.length) {
      return severities[severity];
    }
    return String(severity);
  };

  const startMonitoring = () => {
    console.log('[React] Iniciando monitoreo...');
    setIsConnecting(true);
    setError(null);
    
    const newSocket = io('http://localhost:3001', {
      reconnection: true,
      reconnectionAttempts: Infinity,
      reconnectionDelay: 1000,
      timeout: 20000,
      transports: ['websocket', 'polling']
    });

    console.log('[React] Socket configurado, intentando conectar...');

    newSocket.on('welcome', (data) => {
      console.log('[React] Mensaje de bienvenida:', data.message);
    });

    newSocket.on('connect', () => {
      console.log('[React] Conexión establecida');
      setIsMonitoring(true);
      setIsConnecting(false);
      setError(null);
    });

    newSocket.on('disconnect', (reason) => {
      console.log('[React] Desconectado:', reason);
      setIsMonitoring(false);
      setError(`Desconectado: ${reason}`);
    });

    newSocket.on('connect_error', (error) => {
      console.error('[React] Error de conexión:', error.message);
      setError(`Error de conexión: ${error.message}`);
      setIsConnecting(false);
    });

    newSocket.on('error', (error) => {
      console.error('[React] Error de Socket.IO:', error);
      setError(`Error: ${error.message}`);
      setIsConnecting(false);
    });

    newSocket.on('alert', (alert: AlertData) => {
      console.log('[React] Alerta recibida:', alert);
      const newId = Date.now();
      setAlerts(prev => [...prev, { ...alert, id: newId }]);
      
      const severityLabel = getSeverityLabel(alert.severity);
      setAlertStats(prev => ({
        ...prev,
        [severityLabel]: (prev[severityLabel as keyof typeof prev] || 0) + 1
      }));
      
      setLastAlert({
        severity: severityLabel,
        timestamp: new Date().toLocaleTimeString()
      });
    });

    newSocket.connect();
    setSocket(newSocket);
  };

  const stopMonitoring = () => {
    if (socket) {
      socket.disconnect();
      setSocket(null);
      setIsMonitoring(false);
    }
  };

  useEffect(() => {
    return () => {
      if (socket) {
        socket.disconnect();
      }
    };
  }, [socket]);

  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Container maxWidth="xl">
        <Box sx={{ my: 4 }}>
          <Header 
            isMonitoring={isMonitoring}
            isConnecting={isConnecting}
            onStartMonitoring={startMonitoring}
            lastAlert={lastAlert}
            alertStats={alertStats}
          />
          
          {error && (
            <Snackbar
              open={!!error}
              autoHideDuration={6000}
              onClose={() => setError(null)}
              anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
            >
              <Alert severity="error" onClose={() => setError(null)}>
                {error}
              </Alert>
            </Snackbar>
          )}

          <Box sx={{ mt: 4 }}>
            <AlertTable alerts={alerts} />
          </Box>
        </Box>
      </Container>
    </ThemeProvider>
  );
}

export default App;
