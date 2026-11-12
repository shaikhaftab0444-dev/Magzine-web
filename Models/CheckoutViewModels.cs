using System.ComponentModel.DataAnnotations;

namespace Digital_Magzine.Models
{
    public class PaymentViewModel
    {
        public int? PlanId { get; set; }
        public string PlanName { get; set; } = "Culture Club All-Access VIP";
        public int? IssueId { get; set; }
        public string? IssueTitle { get; set; }
        public string? IssueCoverImage { get; set; }
        public bool IsSingleIssue => IssueId.HasValue && IssueId.Value > 0;
        public decimal Price { get; set; } = 499.00m;
        public int DurationInMonths { get; set; } = 1;

        [Required(ErrorMessage = "Transaction Reference / UTR ID is required.")]
        [Display(Name = "UPI / Bank Transaction Reference")]
        [StringLength(100, ErrorMessage = "Reference must be less than 100 characters")]
        public string TransactionReference { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
