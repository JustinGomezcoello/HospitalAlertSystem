using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Domain;
using System.Text.Json;

namespace EmergenciaConsumer
{
    class Program
    {
        private const string ConnectionString = "ABCDEFGHI";
        private const string QueueName = "sendAlertEvent";
        private static ServiceBusClient? _client;
        private static ServiceBusProcessor? _processor;

        static async Task Main(string[] args)
        {
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Console.WriteLine("🚨 Hospital Alert System - Consumidor de Emergencias");
            Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            try
            {
                await SetupServiceBus();
                Console.WriteLine("\n⏳ Esperando alertas de emergencia. Presione Ctrl+C para salir.\n");
                
                // Mantener la aplicación en ejecución hasta que se cancele
                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (s, e) =>
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                };

                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("\nCerrando el consumidor de emergencias...");
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
            _processor = _client.CreateProcessor(QueueName);

            _processor.ProcessMessageAsync += HandleMessage;
            _processor.ProcessErrorAsync += HandleError;

            await _processor.StartProcessingAsync();
        }

        private static async Task HandleMessage(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"\n📨 Mensaje recibido: {body}");
            
            try
            {
                var alert = JsonSerializer.Deserialize<AlertEvent>(body);
                
                if (alert != null)
                {
                    HandleAlert(alert);
                }

                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error al procesar mensaje: {ex.Message}");
                Console.ResetColor();
            }
        }

        private static Task HandleError(ProcessErrorEventArgs args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error en el procesamiento: {args.Exception.Message}");
            Console.ResetColor();
            return Task.CompletedTask;
        }        private static void HandleAlert(AlertEvent alert)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n🚨 ALERTA DE EMERGENCIA RECIBIDA:");
            Console.WriteLine($"ID: {alert.Id}");
            Console.WriteLine($"Tipo: {alert.Type}");
            Console.WriteLine($"Ubicación: {alert.Location}");
            Console.WriteLine($"Mensaje: {alert.Message}");
            Console.WriteLine($"Severidad: {alert.Severity}");
            Console.WriteLine($"Timestamp: {alert.Timestamp}");
            Console.WriteLine($"Creado por: {alert.CreatedBy}");
            Console.ResetColor();
        }

        private static async Task CleanupServiceBus()
        {
            if (_processor != null)
            {
                await _processor.StopProcessingAsync();
                await _processor.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
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
                queue: Domain.Constants.EmergenciaQueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            // Vincular la cola al exchange directo con la routing key específica
            _channel.QueueBind(
                queue: Domain.Constants.EmergenciaQueueName,
                exchange: Domain.Constants.DirectExchangeName,
                routingKey: Domain.Constants.EmergenciaRoutingKey);

            // Vincular la cola al exchange fanout para recibir todas las alertas críticas
            _channel.QueueBind(
                queue: Domain.Constants.EmergenciaQueueName,
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
                queue: Domain.Constants.EmergenciaQueueName,
                autoAck: false,
                consumer: consumer);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Conectado a RabbitMQ exitosamente");
            Console.WriteLine($"✓ Consumiendo mensajes de la cola: {Domain.Constants.EmergenciaQueueName}");
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