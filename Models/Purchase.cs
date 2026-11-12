using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital_Magzine.Models
{
    public class Purchase
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(MagazineIssue))]
        public int? MagazineIssueId { get; set; }

        public virtual MagazineIssue? MagazineIssue { get; set; }

        [ForeignKey(nameof(Magazine))]
        public int? MagazineId { get; set; }

        public virtual Magazine? Magazine { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AmountPaid { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AdminCut { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal PublisherCut { get; set; }

        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    }
}
