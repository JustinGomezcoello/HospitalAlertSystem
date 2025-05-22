using System;
using System.Text;
using System.Threading;
using Domain;
using RabbitMQ.Client;

namespace Producer
{
    class Program
    {
        private static IConnection? _connection;
        private static IModel? _channel;
        private static readonly Random _random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🏥 Hospital Alert System - Productor de Alertas");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            try
            {
                SetupRabbitMQ();
                RunAlertProducer();
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

            // Declarar el exchange directo para enrutamiento específico
            _channel.ExchangeDeclare(
                exchange: Domain.Constants.DirectExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false);

            // Declarar el exchange fanout para broadcast de alertas críticas
            _channel.ExchangeDeclare(
                exchange: Domain.Constants.FanoutExchangeName,
                type: ExchangeType.Fanout,
                durable: true,
                autoDelete: false);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Conectado a RabbitMQ exitosamente");
            Console.WriteLine("✓ Exchanges declarados correctamente");
            Console.ResetColor();
        }

        private static void RunAlertProducer()
        {
            Console.WriteLine("\n🔔 Generador de alertas iniciado. Presione Ctrl+C para detener.\n");

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var alertType = GetRandomAlertType();
                    var alertSeverity = GetRandomSeverity();
                    var alert = CreateAlert(alertType, alertSeverity);
                    
                    PublishAlert(alert);
                    
                    // Esperamos entre 2 y 5 segundos entre cada alerta
                    Thread.Sleep(_random.Next(2000, 5000));
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error al generar alerta: {ex.Message}");
                    Console.ResetColor();
                }
            }
        }

        private static AlertType GetRandomAlertType()
        {
            var values = Enum.GetValues<AlertType>();
            return values[_random.Next(values.Length)];
        }

        private static AlertSeverity GetRandomSeverity()
        {
            var values = Enum.GetValues<AlertSeverity>();
            return values[_random.Next(values.Length)];
        }

        private static AlertEvent CreateAlert(AlertType type, AlertSeverity severity)
        {
            var locations = new[] { "Piso 1", "Piso 2", "Urgencias", "Quirófano", "UCI", "Pediatría", "Laboratorio" };
            var creators = new[] { "Sistema", "Dr. Martínez", "Enfermera López", "Técnico Rodríguez", "Supervisor Gómez" };
            
            var alert = new AlertEvent
            {
                Type = type,
                Severity = severity,
                Location = locations[_random.Next(locations.Length)],
                CreatedBy = creators[_random.Next(creators.Length)],
                Timestamp = DateTime.Now
            };

            // Definir mensaje según tipo de alerta
            switch (type)
            {
                case AlertType.Emergencia:
                    alert.Message = GetEmergencyMessage(severity);
                    break;
                case AlertType.Enfermeria:
                    alert.Message = GetNursingMessage(severity);
                    break;
                case AlertType.Mantenimiento:
                    alert.Message = GetMaintenanceMessage(severity);
                    break;
            }

            return alert;
        }

        private static string GetEmergencyMessage(AlertSeverity severity)
        {
            return severity switch
            {
                AlertSeverity.Baja => "Paciente con síntomas leves requiere evaluación.",
                AlertSeverity.Media => "Paciente con dolores agudos requiere atención.",
                AlertSeverity.Alta => "Paciente con signos vitales inestables requiere atención inmediata.",
                AlertSeverity.Critica => "CÓDIGO AZUL - Paciente en paro cardiorrespiratorio.",
                _ => "Alerta de emergencia sin especificar."
            };
        }

        private static string GetNursingMessage(AlertSeverity severity)
        {
            return severity switch
            {
                AlertSeverity.Baja => "Paciente solicita asistencia para comodidad.",
                AlertSeverity.Media => "Paciente requiere medicación programada.",
                AlertSeverity.Alta => "Paciente requiere cambio de vendajes y evaluación.",
                AlertSeverity.Critica => "Paciente muestra reacción adversa a medicación.",
                _ => "Alerta de enfermería sin especificar."
            };
        }

        private static string GetMaintenanceMessage(AlertSeverity severity)
        {
            return severity switch
            {
                AlertSeverity.Baja => "Solicitud de limpieza de habitación.",
                AlertSeverity.Media => "Equipo médico requiere calibración.",
                AlertSeverity.Alta => "Fuga de agua detectada.",
                AlertSeverity.Critica => "Fallo en sistema eléctrico de respaldo.",
                _ => "Alerta de mantenimiento sin especificar."
            };
        }

        private static void PublishAlert(AlertEvent alert)
        {
            if (_channel == null)
            {
                throw new InvalidOperationException("El canal de RabbitMQ no está inicializado.");
            }

            var body = Encoding.UTF8.GetBytes(alert.Serialize());
            var routingKey = GetRoutingKeyForAlertType(alert.Type);

            // Publicar en el exchange directo para enrutamiento específico
            _channel.BasicPublish(
                exchange: Domain.Constants.DirectExchangeName,
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            // Si la alerta es crítica, también publicarla en el exchange fanout
            if (alert.Severity == AlertSeverity.Critica)
            {
                _channel.BasicPublish(
                    exchange: Domain.Constants.FanoutExchangeName,
                    routingKey: string.Empty,  // Fanout ignora la routing key
                    basicProperties: null,
                    body: body);
                
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"⚠️ ALERTA CRÍTICA ENVIADA: {alert}");
                Console.WriteLine($"   → Broadcast a todos los servicios via fanout exchange");
                Console.ResetColor();
            }
            else
            {
                ConsoleColor color = alert.Type switch
                {
                    AlertType.Emergencia => ConsoleColor.Red,
                    AlertType.Enfermeria => ConsoleColor.Green,
                    AlertType.Mantenimiento => ConsoleColor.Blue,
                    _ => ConsoleColor.White
                };

                Console.ForegroundColor = color;
                Console.WriteLine($"📨 Alerta enviada: {alert}");
                Console.WriteLine($"   → Routing key: {routingKey}");
                Console.ResetColor();
            }
        }

        private static string GetRoutingKeyForAlertType(AlertType type)
        {
            return type switch
            {
                AlertType.Emergencia => Domain.Constants.EmergenciaRoutingKey,
                AlertType.Enfermeria => Domain.Constants.EnfermeriaRoutingKey,
                AlertType.Mantenimiento => Domain.Constants.MantenimientoRoutingKey,
                _ => throw new ArgumentException($"Tipo de alerta no soportado: {type}")
            };
        }

        private static void CleanupRabbitMQ()
        {
            _channel?.Close();
            _connection?.Close();
            
            Console.WriteLine("Conexión a RabbitMQ cerrada.");
        }
    }
}