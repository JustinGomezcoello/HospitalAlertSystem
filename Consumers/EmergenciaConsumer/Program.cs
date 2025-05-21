using System;
using System.Text;
using System.Threading;
using Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EmergenciaConsumer
{
    class Program
    {
        private static IConnection? _connection;
        private static IModel? _channel;

        static void Main(string[] args)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🚨 Hospital Alert System - Consumidor de Emergencias");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            try
            {
                SetupRabbitMQ();
                Console.WriteLine("\n⏳ Esperando alertas de emergencia. Presione Ctrl+C para salir.\n");
                
                // Mantener la aplicación en ejecución
                while (true)
                {
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                CleanupRabbitMQ();
            }
        }

        private static void SetupRabbitMQ()
        {
            Console.WriteLine("Conectando a RabbitMQ...");
            
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Configurar la cola de emergencias
            _channel.QueueDeclare(
                queue: Constants.EmergenciaQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Vincular la cola al exchange directo con la routing key específica
            _channel.QueueBind(
                queue: Constants.EmergenciaQueueName,
                exchange: Constants.DirectExchangeName,
                routingKey: Constants.EmergenciaRoutingKey);

            // Vincular la cola al exchange fanout para recibir todas las alertas críticas
            _channel.QueueBind(
                queue: Constants.EmergenciaQueueName,
                exchange: Constants.FanoutExchangeName,
                routingKey: string.Empty);  // No importa para fanout

            // Configurar el consumer
            var consumer = new EventingBasicConsumer(_channel);
            
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                
                try
                {
                    var alert = AlertEvent.Deserialize(message);
                    
                    if (alert != null)
                    {
                        HandleAlert(alert, ea.RoutingKey, ea.Exchange);
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error al procesar mensaje: {ex.Message}");
                    Console.ResetColor();
                }
                
                // Confirmar que se procesó el mensaje
                _channel.BasicAck(ea.DeliveryTag, false);
            };
            
            // Comenzar a consumir de la cola
            _channel.BasicConsume(
                queue: Constants.EmergenciaQueueName,
                autoAck: false,
                consumer: consumer);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Conectado a RabbitMQ exitosamente");
            Console.WriteLine($"✓ Consumiendo mensajes de la cola: {Constants.EmergenciaQueueName}");
            Console.ResetColor();
        }

        private static void HandleAlert(AlertEvent alert, string routingKey, string exchange)
        {
            bool isCritical = alert.Severity == AlertSeverity.Critica;
            bool isFromFanout = exchange == Constants.FanoutExchangeName;
            
            // Colorear según severidad
            ConsoleColor color = alert.Severity switch
            {
                AlertSeverity.Baja => ConsoleColor.Gray,
                AlertSeverity.Media => ConsoleColor.Yellow,
                AlertSeverity.Alta => ConsoleColor.Magenta,
                AlertSeverity.Critica => ConsoleColor.Red,
                _ => ConsoleColor.White
            };

            Console.ForegroundColor = color;
            
            if (isCritical && isFromFanout)
            {
                // Alerta crítica recibida via fanout
                Console.WriteLine("\n⚠️ ALERTA CRÍTICA GLOBAL RECIBIDA ⚠️");
                Console.WriteLine($"📟 {alert.ToString()}");
                Console.WriteLine($"   → Recibida via broadcast (Fanout Exchange)");
                
                // Simulación de activación de código azul
                if (alert.Message.Contains("CÓDIGO AZUL"))
                {
                    Console.WriteLine("\n🚨 INICIANDO PROTOCOLO DE CÓDIGO AZUL 🚨");
                    Console.WriteLine("   → Activando equipo de respuesta rápida");
                    Console.WriteLine("   → Enviando notificación a médicos de guardia");
                    Console.WriteLine("   → Preparando desfibrilador y carro de paro");
                    Console.WriteLine("   → Tiempo estimado de respuesta: 30 segundos");
                }
            }
            else if (alert.Type == AlertType.Emergencia)
            {
                // Alerta normal específica para este servicio
                Console.WriteLine($"\n📥 NUEVA ALERTA DE EMERGENCIA RECIBIDA");
                Console.WriteLine($"📟 {alert.ToString()}");
                
                // Simulación de respuesta
                SimulateEmergencyResponse(alert);
            }
            else
            {
                // Alerta de otro tipo que ha llegado por fanout porque es crítica
                Console.WriteLine($"\n⚠️ ALERTA CRÍTICA DE {alert.Type} RECIBIDA");
                Console.WriteLine($"📟 {alert.ToString()}");
                Console.WriteLine($"   → Notificado al departamento de {alert.Type}");
            }
            
            Console.ResetColor();
        }

        private static void SimulateEmergencyResponse(AlertEvent alert)
        {
            Console.WriteLine("🏃 RESPUESTA A EMERGENCIA:");
            
            switch (alert.Severity)
            {
                case AlertSeverity.Baja:
                    Console.WriteLine("   → Asignando enfermero para evaluación");
                    Console.WriteLine("   → Prioridad: Normal");
                    break;
                case AlertSeverity.Media:
                    Console.WriteLine("   → Asignando médico para atención");
                    Console.WriteLine("   → Preparando medicación para dolor");
                    Console.WriteLine("   → Prioridad: Elevada");
                    break;
                case AlertSeverity.Alta:
                    Console.WriteLine("   → Enviando equipo médico de urgencia");
                    Console.WriteLine("   → Preparando kit de estabilización");
                    Console.WriteLine("   → Notificando a especialista de guardia");
                    Console.WriteLine("   → Prioridad: Alta");
                    break;
                case AlertSeverity.Critica:
                    Console.WriteLine("   → ACTIVANDO PROTOCOLO DE EMERGENCIA");
                    Console.WriteLine("   → Enviando equipo completo de resucitación");
                    Console.WriteLine("   → Notificando a UCI para posible ingreso");
                    Console.WriteLine("   → Despejando acceso para traslado rápido");
                    Console.WriteLine("   → Prioridad: MÁXIMA");
                    break;
            }
        }

        private static void CleanupRabbitMQ()
        {
            _channel?.Close();
            _connection?.Close();
            
            Console.WriteLine("Conexión a RabbitMQ cerrada.");
        }
    }
}