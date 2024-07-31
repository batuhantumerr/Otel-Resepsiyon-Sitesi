namespace OtelResepsiyon.Models
{
    public class Notification
    {
        public string Message { get; set; }
        public string Type { get; set; } // "success", "error", "info", "warning"
    }
}
