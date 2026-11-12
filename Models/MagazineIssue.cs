using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital_Magzine.Models
{
    public class MagazineIssue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [ForeignKey(nameof(Magazine))]
        public int MagazineId { get; set; } = 1;

        public virtual Magazine? Magazine { get; set; }

        [ForeignKey(nameof(Category))]
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public int Volume { get; set; }

        [Required]
        public DateTime PublishDate { get; set; }

        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [StringLength(500)]
        public string? CoverImagePath { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SingleIssuePrice { get; set; }

        [StringLength(500)]
        public string? PdfFilePath { get; set; }

        public bool IsPublished { get; set; } = false;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Published";

        // Navigation property
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
    }
}
