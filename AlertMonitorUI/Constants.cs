namespace AlertMonitorUI
{
    public static class Constants
    {
        // Exchange names
        public const string DirectExchangeName = "hospital.alerts.direct";
        public const string FanoutExchangeName = "hospital.alerts.fanout";
        
        // Queue names
        public const string EmergenciaQueueName = "emergencia.alerts";
        public const string EnfermeriaQueueName = "enfermeria.alerts";
        public const string MantenimientoQueueName = "mantenimiento.alerts";
        
        // Routing keys for direct exchange
        public const string EmergenciaRoutingKey = "emergencia";
        public const string EnfermeriaRoutingKey = "enfermeria";
        public const string MantenimientoRoutingKey = "mantenimiento";
    }
}