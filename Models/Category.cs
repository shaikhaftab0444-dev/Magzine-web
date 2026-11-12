using System.ComponentModel.DataAnnotations;

namespace Digital_Magzine.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<Magazine> Magazines { get; set; } = new List<Magazine>();
        public virtual ICollection<MagazineIssue> MagazineIssues { get; set; } = new List<MagazineIssue>();
    }
}
