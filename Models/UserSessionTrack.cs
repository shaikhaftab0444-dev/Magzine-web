using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital_Magzine.Models
{
    public class UserSessionTrack
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(256)]
        public string UserEmail { get; set; } = string.Empty;

        public DateTime LoginTime { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string DeviceName { get; set; } = string.Empty;

        [StringLength(100)]
        public string IPAddress { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Active"; // "Active" or "Logged Out"
    }
}
