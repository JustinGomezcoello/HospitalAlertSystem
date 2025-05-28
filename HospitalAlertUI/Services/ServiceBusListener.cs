using Azure.Messaging.ServiceBus;
using Domain;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace HospitalAlertUI.Services
{    public class ServiceBusListener : BackgroundService
    {        private const string ConnectionString = "ABCDEFGHI";
        private const string QueueName = "alerts";
        private readonly AlertService _alertService;
        private ServiceBusClient? _client;
        private ServiceBusProcessor? _processor;

        public ServiceBusListener(AlertService alertService)
        {
            _alertService = alertService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client = new ServiceBusClient(ConnectionString);
            _processor = _client.CreateProcessor(QueueName);

            _processor.ProcessMessageAsync += HandleMessage;
            _processor.ProcessErrorAsync += HandleError;

            await _processor.StartProcessingAsync(stoppingToken);

            try
            {
                // Mantener el servicio en ejecución hasta que se cancele
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            finally
            {
                // Asegurarnos de limpiar los recursos
                await _processor.StopProcessingAsync(stoppingToken);
                await _processor.DisposeAsync();
                await _client.DisposeAsync();
            }
        }

        private async Task HandleMessage(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            
            try
            {
                var alert = JsonSerializer.Deserialize<AlertEvent>(body);
                
                if (alert != null)
                {
                    _alertService.AddAlert(alert);
                }

                // Completar el mensaje para que se elimine de la cola
                await args.CompleteMessageAsync(args.Message);
            }
            catch (Exception)
            {
                // En caso de error, abandonar el mensaje para que se pueda reintentar
                await args.AbandonMessageAsync(args.Message);
            }
        }

        private Task HandleError(ProcessErrorEventArgs args)
        {
            // Aquí podrías agregar logging del error
            Console.WriteLine($"Error procesando mensaje: {args.Exception.Message}");
            return Task.CompletedTask;
        }
    }
}
