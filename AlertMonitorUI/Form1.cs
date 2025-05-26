using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AlertMonitorUI
{
    public class Alert
    {
        public string Type { get; set; }
        public string Severity { get; set; }
        public string Message { get; set; }
        public string Location { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Timestamp { get; set; }
    }


    public class Form1 : Form
    {
        private DataGridView dataGrid;
        private Button btnStart;
        private DataTable table;

        public Form1()
        {
            this.Text = "Hospital Alert Monitor";
            this.Width = 900;
            this.Height = 500;

            // Botón de monitoreo
            btnStart = new Button()
            {
                Text = "Iniciar Monitoreo",
                Top = 20,
                Left = 20,
                Width = 150
            };
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            // Tabla de datos
            dataGrid = new DataGridView()
            {
                Top = 60,
                Left = 20,
                Width = 840,
                Height = 380,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            // Columnas de la tabla
            table = new DataTable();
            table.Columns.Add("Tipo");
            table.Columns.Add("Severidad");
            table.Columns.Add("Mensaje");
            table.Columns.Add("Ubicación");
            table.Columns.Add("Responsable");
            table.Columns.Add("Hora");


            dataGrid.DataSource = table;
            dataGrid.CellFormatting += DataGrid_CellFormatting;

            this.Controls.Add(dataGrid);
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            string[] colas = { "emergencia.alerts", "enfermeria.alerts", "mantenimiento.alerts" };

            foreach (var cola in colas)
            {
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);

                    try
                    {
                        // Intenta como objeto único
                        var alert = JsonSerializer.Deserialize<Alert>(json);
                        if (alert != null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                table.Rows.Add(alert.Type, alert.Severity, alert.Message, alert.Location, alert.CreatedBy, alert.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

                            });
                            return;
                        }

                        // Si no funcionó, intenta como lista
                        var alerts = JsonSerializer.Deserialize<List<Alert>>(json);
                        if (alerts != null)
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                foreach (var a in alerts)
                                    table.Rows.Add(a.Type, a.Severity, a.Message, a.Location, a.CreatedBy, a.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"));

                            });
                            return;
                        }

                        // Si llegó aquí, no se pudo deserializar
                        throw new Exception("Formato desconocido.");
                    }
                    catch
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            table.Rows.Add("ERROR", cola, "", json, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        });
                    }
                };

                channel.BasicConsume(queue: cola, autoAck: true, consumer: consumer);
            }

            btnStart.Enabled = false;
            MessageBox.Show("Monitoreo iniciado.");
        }

        private void DataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGrid.Columns[e.ColumnIndex].Name == "Severidad")
            {
                string? severity = e.Value?.ToString();
                if (severity == "Crítica")
                    dataGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightCoral;
                else if (severity == "Media")
                    dataGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.Khaki;
                else if (severity == "Baja")
                    dataGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;
            }
        }
    }
}


