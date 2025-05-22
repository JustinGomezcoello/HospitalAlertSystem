using RabbitMQ.Client.Events;
using Domain;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using HospitalAlertUI.Services;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace HospitalAlertUI.Rabbit
{
    public class RabbitListener : BackgroundService
    {
        private readonly AlertService _alertService;

        public RabbitListener(AlertService alertService)
        {
            _alertService = alertService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "alertas-hospital",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var alerta = JsonSerializer.Deserialize<AlertEvent>(json);

                if (alerta != null)
                {
                    _alertService.AddAlert(alerta);
                }
            };

            channel.BasicConsume(queue: "alertas-hospital",
                                 autoAck: true,
                                 consumer: consumer);

            // Mantener activo el servicio mientras no se cancele
            return Task.Delay(-1, stoppingToken);
        }
    }
}
