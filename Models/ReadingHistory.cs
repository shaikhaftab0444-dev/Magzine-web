using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital_Magzine.Models
{
    public class ReadingHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int MagazineIssueId { get; set; }

        public DateTime LastReadDate { get; set; } = DateTime.UtcNow;

        // Navigation property
        [ForeignKey("MagazineIssueId")]
        public virtual MagazineIssue? MagazineIssue { get; set; }
    }
}
