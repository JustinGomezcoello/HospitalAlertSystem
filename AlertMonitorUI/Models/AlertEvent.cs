using System.Text.Json;

namespace AlertMonitorUI.Models
{
    public enum AlertType
    {
        Emergencia,
        Enfermeria,
        Mantenimiento
    }

    public enum AlertSeverity
    {
        Baja,
        Media,
        Alta,
        Critica
    }

    public class AlertEvent
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public AlertType Type { get; set; }
        public AlertSeverity Severity { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Type} - {Severity} - {Location}: {Message}";
        }

        public string Serialize()
        {
            return JsonSerializer.Serialize(this);
        }

        public static AlertEvent? Deserialize(string json)
        {
            return JsonSerializer.Deserialize<AlertEvent>(json);
        }
    }
}