using System;
using System.Text;
using System.Windows.Forms;
using System.Collections.Generic;
using Domain;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AlertMonitorUI
{
    public partial class MainForm : Form
    {
        private IConnection? _connection;
        private IModel? _channel;
        private readonly Dictionary<AlertType, ListBox> _alertListBoxes;
        private readonly Dictionary<AlertSeverity, Color> _severityColors;

        public MainForm()
        {
            InitializeComponent();
            
            // Inicializar diccionarios
            _alertListBoxes = new Dictionary<AlertType, ListBox>
            {
                { AlertType.Emergencia, listBoxEmergencias },
                { AlertType.Enfermeria, listBoxEnfermeria },
                { AlertType.Mantenimiento, listBoxMantenimiento }
            };

            _severityColors = new Dictionary<AlertSeverity, Color>
            {
                { AlertSeverity.Baja, Color.Gray },
                { AlertSeverity.Media, Color.Orange },
                { AlertSeverity.Alta, Color.Red },
                { AlertSeverity.Critica, Color.DarkRed }
            };

            // Configurar la UI
            SetupUI();
            
            // Conectar a RabbitMQ
            ConnectToRabbitMQ();
        }

        private void SetupUI()
        {
            // Configurar temporizador para limpiar alertas antiguas
            var timer = new System.Windows.Forms.Timer
            {
                Interval = 60000 // 1 minuto
            };
            timer.Tick += CleanOldAlerts;
            timer.Start();

            // Suscribirse al evento de cierre del formulario
            FormClosing += (s, e) => CleanupRabbitMQ();
        }

        private void ConnectToRabbitMQ()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = "localhost",
                    UserName = "guest",
                    Password = "guest"
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar exchanges
                _channel.ExchangeDeclare(
                    exchange: Domain.Constants.DirectExchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);

                _channel.ExchangeDeclare(
                    exchange: Domain.Constants.FanoutExchangeName,
                    type: ExchangeType.Fanout,
                    durable: true,
                    autoDelete: false);

                // Declarar y vincular cola temporal para el monitor
                var queueName = _channel.QueueDeclare().QueueName;

                // Vincular a todas las routing keys en el exchange directo
                _channel.QueueBind(
                    queue: queueName,
                    exchange: Domain.Constants.DirectExchangeName,
                    routingKey: Domain.Constants.EmergenciaRoutingKey);

                _channel.QueueBind(
                    queue: queueName,
                    exchange: Domain.Constants.DirectExchangeName,
                    routingKey: Domain.Constants.EnfermeriaRoutingKey);

                _channel.QueueBind(
                    queue: queueName,
                    exchange: Domain.Constants.DirectExchangeName,
                    routingKey: Domain.Constants.MantenimientoRoutingKey);

                // Vincular al exchange fanout para alertas críticas
                _channel.QueueBind(
                    queue: queueName,
                    exchange: Domain.Constants.FanoutExchangeName,
                    routingKey: string.Empty);

                // Configurar el consumidor
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
                            Invoke(new Action(() => HandleAlert(alert, ea.RoutingKey, ea.Exchange)));
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al procesar mensaje: {ex.Message}", "Error", 
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                _channel.BasicConsume(
                    queue: queueName,
                    autoAck: true,
                    consumer: consumer);

                UpdateStatus("Conectado a RabbitMQ", Color.Green);
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error de conexión: {ex.Message}", Color.Red);
                MessageBox.Show(ex.Message, "Error de Conexión", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleAlert(AlertEvent alert, string routingKey, string exchange)
        {
            bool isCritical = alert.Severity == AlertSeverity.Critica;
            bool isFromFanout = exchange == Domain.Constants.FanoutExchangeName;

            // Crear el item para la lista
            var item = new AlertListItem(alert);
            item.BackColor = _severityColors[alert.Severity];
            item.ForeColor = (alert.Severity == AlertSeverity.Baja) ? Color.Black : Color.White;

            // Si es una alerta crítica del fanout, mostrarla en todas las listas
            if (isCritical && isFromFanout)
            {
                foreach (var listBox in _alertListBoxes.Values)
                {
                    listBox.Items.Insert(0, item);
                    FlashListBox(listBox);
                }

                // Reproducir sonido de alerta
                System.Media.SystemSounds.Exclamation.Play();
            }
            else
            {
                // Mostrar en la lista correspondiente
                var listBox = _alertListBoxes[alert.Type];
                listBox.Items.Insert(0, item);

                if (alert.Severity >= AlertSeverity.Alta)
                {
                    FlashListBox(listBox);
                }
            }

            // Actualizar contador
            UpdateAlertCount();
        }

        private void FlashListBox(ListBox listBox)
        {
            var originalColor = listBox.BackColor;
            listBox.BackColor = Color.Yellow;

            var timer = new System.Windows.Forms.Timer
            {
                Interval = 500
            };

            timer.Tick += (s, e) =>
            {
                listBox.BackColor = originalColor;
                timer.Stop();
                timer.Dispose();
            };

            timer.Start();
        }

        private void UpdateAlertCount()
        {
            labelEmergencias.Text = $"Emergencias ({listBoxEmergencias.Items.Count})";
            labelEnfermeria.Text = $"Enfermería ({listBoxEnfermeria.Items.Count})";
            labelMantenimiento.Text = $"Mantenimiento ({listBoxMantenimiento.Items.Count})";
        }

        private void CleanOldAlerts(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var maxAge = TimeSpan.FromMinutes(30); // Mantener alertas por 30 minutos

            foreach (var listBox in _alertListBoxes.Values)
            {
                for (int i = listBox.Items.Count - 1; i >= 0; i--)
                {
                    if (listBox.Items[i] is AlertListItem item)
                    {
                        if (now - item.Alert.Timestamp > maxAge)
                        {
                            listBox.Items.RemoveAt(i);
                        }
                    }
                }
            }

            UpdateAlertCount();
        }

        private void UpdateStatus(string message, Color color)
        {
            labelStatus.Text = message;
            labelStatus.ForeColor = color;
        }

        private void CleanupRabbitMQ()
        {
            try
            {
                _channel?.Close();
                _connection?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cerrar conexión: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CleanupRabbitMQ();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class AlertListItem
    {
        public AlertEvent Alert { get; }

        public AlertListItem(AlertEvent alert)
        {
            Alert = alert;
        }

        public override string ToString()
        {
            return $"[{Alert.Timestamp:HH:mm:ss}] {Alert.Severity} - {Alert.Location}: {Alert.Message}";
        }
    }
}