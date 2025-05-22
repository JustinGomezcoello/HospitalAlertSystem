using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MantenimientoConsumer
{
    class Program
    {
        private static IConnection? _connection;
        private static IModel? _channel;
        // Simulación de tareas de mantenimiento
        private static readonly List<string> _pendingTasks = new List<string>();
        private static readonly Random _random = new Random();

        static void Main(string[] args)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🔧 Hospital Alert System - Consumidor de Mantenimiento");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            try
            {
                SetupRabbitMQ();
                Console.WriteLine("\n⏳ Esperando alertas de mantenimiento. Presione Ctrl+C para salir.\n");
                
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

            // Configurar la cola de mantenimiento
            _channel.QueueDeclare(
                queue: Domain.Constants.MantenimientoQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Vincular la cola al exchange directo con la routing key específica
            _channel.QueueBind(
                queue: Domain.Constants.MantenimientoQueueName,
                exchange: Domain.Constants.DirectExchangeName,
                routingKey: Domain.Constants.MantenimientoRoutingKey);

            // Vincular la cola al exchange fanout para recibir todas las alertas críticas
            _channel.QueueBind(
                queue: Domain.Constants.MantenimientoQueueName,
                exchange: Domain.Constants.FanoutExchangeName,
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
                queue: Domain.Constants.MantenimientoQueueName,
                autoAck: false,
                consumer: consumer);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Conectado a RabbitMQ exitosamente");
            Console.WriteLine($"✓ Consumiendo mensajes de la cola: {Domain.Constants.MantenimientoQueueName}");
            Console.ResetColor();
        }

        private static void HandleAlert(AlertEvent alert, string routingKey, string exchange)
        {
            bool isCritical = alert.Severity == AlertSeverity.Critica;
            bool isFromFanout = exchange == Domain.Constants.FanoutExchangeName;
            
            // Colorear según severidad
            ConsoleColor color = alert.Severity switch
            {
                AlertSeverity.Baja => ConsoleColor.Gray,
                AlertSeverity.Media => ConsoleColor.Yellow,
                AlertSeverity.Alta => ConsoleColor.Blue,
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
                
                if (alert.Type == AlertType.Mantenimiento)
                {
                    // Es una alerta crítica de mantenimiento
                    HandleCriticalMaintenanceTask(alert);
                }
                else
                {
                    // Es una alerta crítica de otro departamento
                    Console.WriteLine("   → Preparando personal de apoyo para emergencia");
                    Console.WriteLine("   → Asegurando sistemas críticos de respaldo");
                }
            }
            else if (alert.Type == AlertType.Mantenimiento)
            {
                // Alerta normal de mantenimiento
                Console.WriteLine($"\n📥 NUEVA SOLICITUD DE MANTENIMIENTO RECIBIDA");
                Console.WriteLine($"📟 {alert.ToString()}");
                
                // Procesar la tarea de mantenimiento
                ProcessMaintenanceTask(alert);
            }
            
            Console.ResetColor();
        }

        private static void ProcessMaintenanceTask(AlertEvent alert)
        {
            // Añadir la tarea a la lista de pendientes
            string taskId = Guid.NewGuid().ToString().Substring(0, 8);
            string taskDesc = $"TAREA-{taskId}: {alert.Message} en {alert.Location}";
            _pendingTasks.Add(taskDesc);
            
            Console.WriteLine($"🔧 NUEVA TAREA DE MANTENIMIENTO:");
            Console.WriteLine($"   → ID: TAREA-{taskId}");
            Console.WriteLine($"   → Descripción: {alert.Message}");
            Console.WriteLine($"   → Ubicación: {alert.Location}");
            
            // Estimar tiempo de respuesta según severidad
            string estimatedTime = alert.Severity switch
            {
                AlertSeverity.Baja => "24 horas",
                AlertSeverity.Media => "6 horas",
                AlertSeverity.Alta => "1 hora",
                AlertSeverity.Critica => "INMEDIATA",
                _ => "N/A"
            };
            
            Console.WriteLine($"   → Prioridad: {alert.Severity}");
            Console.WriteLine($"   → Tiempo estimado de respuesta: {estimatedTime}");
            
            // Mostrar tareas pendientes
            DisplayPendingTasks();
            
            // Simular que la tarea se completa después de un tiempo
            SimulateTaskCompletion(taskDesc, alert.Severity);
        }

        private static void HandleCriticalMaintenanceTask(AlertEvent alert)
        {
            string taskId = Guid.NewGuid().ToString().Substring(0, 8);
            
            Console.WriteLine($"\n🚨 ALERTA CRÍTICA DE MANTENIMIENTO:");
            Console.WriteLine($"   → ID: EMERGENCIA-{taskId}");
            Console.WriteLine($"   → Descripción: {alert.Message}");
            Console.WriteLine($"   → Ubicación: {alert.Location}");
            Console.WriteLine($"   → RESPUESTA INMEDIATA REQUERIDA");
            
            Console.WriteLine("\n⚡ ACCIONES INMEDIATAS:");
            Console.WriteLine("   → Activando protocolo de emergencia de mantenimiento");
            Console.WriteLine("   → Enviando todos los técnicos disponibles");
            Console.WriteLine("   → Notificando a dirección de infraestructura");
            
            if (alert.Message.Contains("eléctrico"))
            {
                Console.WriteLine("   → Activando generadores de emergencia");
                Console.WriteLine("   → Verificando sistemas críticos de soporte vital");
            }
            else if (alert.Message.Contains("agua"))
            {
                Console.WriteLine("   → Activando sistemas de contención");
                Console.WriteLine("   → Cerrando válvulas principales del sector");
            }
            
            // No añadimos a tareas pendientes ya que es una emergencia que se atiende inmediatamente
            
            // Simular resolución rápida
            Task.Run(async () =>
            {
                await Task.Delay(8000);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n✓ EMERGENCIA DE MANTENIMIENTO RESUELTA:");
                Console.WriteLine($"   → ID: EMERGENCIA-{taskId}");
                Console.WriteLine($"   → Tiempo de respuesta: INMEDIATO");
                Console.WriteLine($"   → Estado: RESUELTO");
                Console.WriteLine($"   → Informe post-incidente en proceso");
                Console.ResetColor();
            });
        }

        private static void SimulateTaskCompletion(string taskDesc, AlertSeverity severity)
        {
            // Determinar tiempo de resolución basado en severidad
            int delayTime = severity switch
            {
                AlertSeverity.Baja => _random.Next(10000, 15000),
                AlertSeverity.Media => _random.Next(8000, 10000),
                AlertSeverity.Alta => _random.Next(5000, 8000),
                AlertSeverity.Critica => _random.Next(3000, 5000),
                _ => 10000
            };
            
            Task.Run(async () =>
            {
                await Task.Delay(delayTime);
                
                lock (_pendingTasks)
                {
                    _pendingTasks.Remove(taskDesc);
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\n✓ TAREA DE MANTENIMIENTO COMPLETADA:");
                    Console.WriteLine($"   → {taskDesc}");
                    Console.WriteLine($"   → Estado: RESUELTO");
                    
                    // Mostrar tareas pendientes actualizadas
                    DisplayPendingTasks();
                    Console.ResetColor();
                }
            });
        }

        private static void DisplayPendingTasks()
        {
            Console.WriteLine("\n📋 TAREAS PENDIENTES:");
            
            if (_pendingTasks.Count == 0)
            {
                Console.WriteLine("   → No hay tareas pendientes");
            }
            else
            {
                foreach (var task in _pendingTasks)
                {
                    Console.WriteLine($"   → {task}");
                }
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