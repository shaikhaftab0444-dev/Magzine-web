namespace Digital_Magzine.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        // Nullable: if null, the notification is addressed to all Admins
        public string? UserId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
