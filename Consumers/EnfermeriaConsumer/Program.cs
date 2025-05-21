using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace EnfermeriaConsumer
{
    class Program
    {
        private static IConnection? _connection;
        private static IModel? _channel;
        // Simulación de estado de enfermeras
        private static readonly List<string> _availableNurses = new List<string>
        {
            "Enfermera García", "Enfermero Pérez", "Enfermera Rodríguez", 
            "Enfermero López", "Enfermera Martínez"
        };
        private static readonly Random _random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("👩‍⚕️ Hospital Alert System - Consumidor de Enfermería");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            try
            {
                SetupRabbitMQ();
                Console.WriteLine("\n⏳ Esperando alertas de enfermería. Presione Ctrl+C para salir.\n");
                
                // Mostrar estado inicial del personal
                DisplayNurseStatus();
                
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

            // Configurar la cola de enfermería
            _channel.QueueDeclare(
                queue: Constants.EnfermeriaQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Vincular la cola al exchange directo con la routing key específica
            _channel.QueueBind(
                queue: Constants.EnfermeriaQueueName,
                exchange: Constants.DirectExchangeName,
                routingKey: Constants.EnfermeriaRoutingKey);

            // Vincular la cola al exchange fanout para recibir todas las alertas críticas
            _channel.QueueBind(
                queue: Constants.EnfermeriaQueueName,
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
                queue: Constants.EnfermeriaQueueName,
                autoAck: false,
                consumer: consumer);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Conectado a RabbitMQ exitosamente");
            Console.WriteLine($"✓ Consumiendo mensajes de la cola: {Constants.EnfermeriaQueueName}");
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
                AlertSeverity.Alta => ConsoleColor.DarkGreen,
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
                
                // Alertar a todo el personal de enfermería disponible
                AssignAllNursesToEmergency(alert);
            }
            else if (alert.Type == AlertType.Enfermeria)
            {
                // Alerta normal específica para enfermería
                Console.WriteLine($"\n📥 NUEVA SOLICITUD DE ENFERMERÍA RECIBIDA");
                Console.WriteLine($"📟 {alert.ToString()}");
                
                // Asignar personal de enfermería según la severidad
                AssignNurseToPatient(alert);
            }
            else if (isCritical)
            {
                // Alerta crítica de otro tipo
                Console.WriteLine($"\n⚠️ ALERTA CRÍTICA DE {alert.Type} RECIBIDA");
                Console.WriteLine($"📟 {alert.ToString()}");
                Console.WriteLine($"   → Notificando a personal de enfermería para apoyo");
                
                // Para alertas críticas de otras áreas, asignar enfermeras de apoyo
                AssignNurseForSupport(alert);
            }
            
            Console.ResetColor();
        }

        private static void AssignNurseToPatient(AlertEvent alert)
        {
            if (_availableNurses.Count == 0)
            {
                Console.WriteLine("❌ No hay personal de enfermería disponible actualmente");
                Console.WriteLine("   → Solicitud puesta en cola de espera");
                return;
            }

            int nursesNeeded = alert.Severity switch
            {
                AlertSeverity.Baja => 1,
                AlertSeverity.Media => 1,
                AlertSeverity.Alta => 2,
                AlertSeverity.Critica => 3,
                _ => 1
            };

            nursesNeeded = Math.Min(nursesNeeded, _availableNurses.Count);
            
            Console.WriteLine($"👩‍⚕️ ASIGNACIÓN DE PERSONAL:");
            
            var assignedNurses = new List<string>();
            for (int i = 0; i < nursesNeeded; i++)
            {
                int index = _random.Next(_availableNurses.Count);
                string nurse = _availableNurses[index];
                assignedNurses.Add(nurse);
                _availableNurses.RemoveAt(index);
                
                Console.WriteLine($"   → {nurse} asignado/a a {alert.Location}");
            }
            
            // Simulación de respuesta según severidad
            switch (alert.Severity)
            {
                case AlertSeverity.Baja:
                    Console.WriteLine("   → Tiempo estimado de respuesta: 15 minutos");
                    break;
                case AlertSeverity.Media:
                    Console.WriteLine("   → Tiempo estimado de respuesta: 10 minutos");
                    Console.WriteLine("   → Preparando medicación solicitada");
                    break;
                case AlertSeverity.Alta:
                    Console.WriteLine("   → Tiempo estimado de respuesta: 5 minutos");
                    Console.WriteLine("   → Notificando a doctor de guardia");
                    break;
                case AlertSeverity.Critica:
                    Console.WriteLine("   → RESPUESTA INMEDIATA");
                    Console.WriteLine("   → Activando protocolo de emergencia");
                    break;
            }
            
            // Simular que las enfermeras vuelven a estar disponibles después de un tiempo
            Task.Run(async () =>
            {
                int delayTime = alert.Severity switch
                {
                    AlertSeverity.Baja => 5000,
                    AlertSeverity.Media => 8000,
                    AlertSeverity.Alta => 12000,
                    AlertSeverity.Critica => 15000,
                    _ => 5000
                };
                
                await Task.Delay(delayTime);
                
                lock (_availableNurses)
                {
                    foreach (var nurse in assignedNurses)
                    {
                        _availableNurses.Add(nurse);
                    }
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Personal vuelve a estar disponible tras atender alerta en {alert.Location}");
                    DisplayNurseStatus();
                    Console.ResetColor();
                }
            });
            
            DisplayNurseStatus();
        }

        private static void AssignNurseForSupport(AlertEvent alert)
        {
            if (_availableNurses.Count == 0)
            {
                Console.WriteLine("❌ No hay personal de enfermería disponible para apoyo");
                return;
            }

            int nursesToAssign = Math.Min(1, _availableNurses.Count);
            
            var assignedNurses = new List<string>();
            for (int i = 0; i < nursesToAssign; i++)
            {
                int index = _random.Next(_availableNurses.Count);
                string nurse = _availableNurses[index];
                assignedNurses.Add(nurse);
                _availableNurses.RemoveAt(index);
                
                Console.WriteLine($"   → {nurse} enviado/a para apoyo a {alert.Location}");
            }
            
            // Simular que las enfermeras vuelven a estar disponibles después de un tiempo
            Task.Run(async () =>
            {
                await Task.Delay(10000);
                
                lock (_availableNurses)
                {
                    foreach (var nurse in assignedNurses)
                    {
                        _availableNurses.Add(nurse);
                    }
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Personal de apoyo vuelve a estar disponible");
                    DisplayNurseStatus();
                    Console.ResetColor();
                }
            });
            
            DisplayNurseStatus();
        }

        private static void AssignAllNursesToEmergency(AlertEvent alert)
        {
            if (_availableNurses.Count == 0)
            {
                Console.WriteLine("❌ No hay personal de enfermería disponible para la emergencia crítica");
                return;
            }

            var assignedNurses = new List<string>(_availableNurses);
            Console.WriteLine("🚨 ASIGNANDO TODO EL PERSONAL DISPONIBLE A EMERGENCIA CRÍTICA:");
            
            foreach (var nurse in assignedNurses)
            {
                Console.WriteLine($"   → {nurse} asignado/a a emergencia en {alert.Location}");
                _availableNurses.Remove(nurse);
            }
            
            Console.WriteLine("   → Se ha activado llamada a personal de guardia");
            
            // Simular que las enfermeras vuelven a estar disponibles después de un tiempo
            Task.Run(async () =>
            {
                await Task.Delay(20000);
                
                lock (_availableNurses)
                {
                    foreach (var nurse in assignedNurses)
                    {
                        _availableNurses.Add(nurse);
                    }
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ Personal vuelve a estar disponible tras atender emergencia crítica");
                    DisplayNurseStatus();
                    Console.ResetColor();
                }
            });
            
            DisplayNurseStatus();
        }

        private static void DisplayNurseStatus()
        {
            Console.WriteLine("\n👩‍⚕️ ESTADO DEL PERSONAL DE ENFERMERÍA:");
            Console.WriteLine($"   → {_availableNurses.Count} enfermeros/as disponibles");
            
            if (_availableNurses.Count > 0)
            {
                Console.WriteLine("   → Personal: " + string.Join(", ", _availableNurses));
            }
            
            Console.WriteLine();
        }

        private static void CleanupRabbitMQ()
        {
            _channel?.Close();
            _connection?.Close();
            
            Console.WriteLine("Conexión a RabbitMQ cerrada.");
        }
    }
}