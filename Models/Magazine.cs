using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital_Magzine.Models
{
    public class Magazine
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [ForeignKey(nameof(Category))]
        public int CategoryId { get; set; } = 1;

        public virtual Category? Category { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SubscriptionPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<MagazineIssue> MagazineIssues { get; set; } = new List<MagazineIssue>();
    }
}
