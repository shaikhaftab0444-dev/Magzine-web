using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital_Magzine.Models
{
    public class Article
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(MagazineIssue))]
        public int MagazineIssueId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string ContentHtml { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Author { get; set; }

        public bool IsPremium { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual MagazineIssue? MagazineIssue { get; set; }
    }
}
