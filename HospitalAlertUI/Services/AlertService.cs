using Domain;
using System.Collections.Concurrent;

namespace HospitalAlertUI.Services
{
    public class AlertService
    {
        private readonly ConcurrentQueue<AlertEvent> _alerts = new();

        public void AddAlert(AlertEvent alert)
        {
            _alerts.Enqueue(alert);
            while (_alerts.Count > 100)
                _alerts.TryDequeue(out _);
        }

        public IEnumerable<AlertEvent> GetAlerts()
        {
            return _alerts.Reverse();
        }
    }
}
