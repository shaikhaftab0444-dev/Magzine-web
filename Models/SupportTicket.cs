namespace Digital_Magzine.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string UserEmail { get; set; } = string.Empty;

        public string Subject { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
