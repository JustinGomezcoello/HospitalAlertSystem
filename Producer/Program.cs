using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Domain;

namespace Producer
{
    class Program
    {        private const string ConnectionString = "XXXXX";
        private const string QueueName = "alerts";
        private static ServiceBusClient? _client;
        private static ServiceBusSender? _sender;
        private static readonly Random _random = new Random();

        static async Task Main(string[] args)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🏥 Hospital Alert System - Productor de Alertas");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            try
            {
                await SetupServiceBus();
                await RunAlertProducer();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                await CleanupServiceBus();
            }
        }

        private static async Task SetupServiceBus()
        {
            Console.WriteLine("Conectando a Azure Service Bus...");
            
            _client = new ServiceBusClient(ConnectionString);
            _sender = _client.CreateSender(QueueName);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Conectado a Azure Service Bus exitosamente");
            Console.ResetColor();
        }

        private static async Task RunAlertProducer()
        {
            Console.WriteLine("\n🔔 Generador de alertas iniciado. Presione Ctrl+C para detener.\n");

            for (int i = 0; i < 10; i++)
            {
                try
                {
                    var alertType = GetRandomAlertType();
                    var alertSeverity = GetRandomSeverity();
                    var alert = CreateAlert(alertType, alertSeverity);
                    
                    await PublishAlert(alert);
                    
                    // Esperamos entre 2 y 5 segundos entre cada alerta
                    await Task.Delay(_random.Next(2000, 5000));
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

        private static string GetRandomArea(AlertType type)
        {
            return type switch
            {
                AlertType.Emergencia => _random.Next(2) == 0 ? "Sala de Emergencias" : "Trauma",
                AlertType.Enfermeria => _random.Next(2) == 0 ? "Habitación 101" : "Habitación 202",
                AlertType.Mantenimiento => _random.Next(2) == 0 ? "Ascensor Principal" : "Sistema Eléctrico",
                _ => throw new ArgumentException("Tipo de alerta no válido")
            };
        }

        private static string GetRandomMessage(AlertType type, AlertSeverity severity)
        {
            return (type, severity) switch
            {
                (AlertType.Emergencia, AlertSeverity.Critica) => "Código Azul - Paciente en paro cardíaco.",
                (AlertType.Emergencia, AlertSeverity.Alta) => "Paciente con trauma severo requiere atención inmediata.",
                (AlertType.Emergencia, _) => "Paciente requiere evaluación de emergencia.",

                (AlertType.Enfermeria, AlertSeverity.Critica) => "Paciente presenta signos vitales críticos.",
                (AlertType.Enfermeria, AlertSeverity.Alta) => "Paciente requiere cambio de vendajes y evaluación.",
                (AlertType.Enfermeria, _) => "Paciente solicita asistencia de enfermería.",

                (AlertType.Mantenimiento, AlertSeverity.Critica) => "Fallo crítico en sistema de respaldo eléctrico.",
                (AlertType.Mantenimiento, AlertSeverity.Alta) => "Fallo en sistema de climatización.",
                (AlertType.Mantenimiento, _) => "Mantenimiento preventivo requerido.",

                _ => throw new ArgumentException("Combinación de tipo y severidad no válida")
            };
        }

        private static AlertEvent CreateAlert(AlertType type, AlertSeverity severity)
        {
            return new AlertEvent
            {
                Type = type,
                Severity = severity,
                Location = GetRandomArea(type),
                Message = GetRandomMessage(type, severity),
                CreatedBy = "Sistema de Generación de Alertas"
            };
        }

        private static async Task PublishAlert(AlertEvent alert)
        {
            try
            {
                var message = new ServiceBusMessage(alert.Serialize());
                
                // Agregamos algunas propiedades personalizadas que podrían ser útiles para filtrado
                message.ApplicationProperties.Add("AlertType", alert.Type.ToString());
                message.ApplicationProperties.Add("Severity", alert.Severity.ToString());
                message.ApplicationProperties.Add("Location", alert.Location);

                // Enviamos el mensaje al Service Bus
                await _sender!.SendMessageAsync(message);

                // Mostramos un mensaje en consola con color según el tipo de alerta
                ConsoleColor color = alert.Type switch
                {
                    AlertType.Emergencia => ConsoleColor.Red,
                    AlertType.Enfermeria => ConsoleColor.Yellow,
                    AlertType.Mantenimiento => ConsoleColor.Blue,
                    _ => ConsoleColor.White
                };

                Console.ForegroundColor = color;
                Console.WriteLine($"📨 Alerta enviada: {alert}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al enviar el mensaje: {ex.Message}");
                Console.ResetColor();
                throw;
            }
        }

        private static async Task CleanupServiceBus()
        {
            if (_sender != null)
            {
                await _sender.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }
    }
}