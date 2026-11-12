using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital_Magzine.Models
{
    public class UserSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty; // Note: Links to ASP.NET Core Identity (IdentityUser.Id)

        [Required]
        [ForeignKey(nameof(SubscriptionPlan))]
        public int SubscriptionPlanId { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = string.Empty; // e.g., Active, Expired, Canceled

        // Navigation property
        public virtual SubscriptionPlan? SubscriptionPlan { get; set; }
    }
}
