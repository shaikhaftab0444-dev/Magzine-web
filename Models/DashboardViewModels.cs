namespace Digital_Magzine.Models
{
    public class AdminDashboardViewModel
    {
        public int TotalMagazines { get; set; }
        public int TotalIssues { get; set; }
        public int TotalSubscribers { get; set; }
        public int PendingRequestsCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<SubscriptionRequestItemViewModel> PendingRequests { get; set; } = new();
        public List<SubscriberItemViewModel> RecentSubscribers { get; set; } = new();
    }

    public class SubscriptionRequestItemViewModel
    {
        public int RequestId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public int DurationInMonths { get; set; }
        public DateTime RequestDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class SubscriberItemViewModel
    {
        public int SubscriptionId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public decimal PlanPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class AdminSubscribersViewModel
    {
        public List<SubscriptionRequestItemViewModel> PendingRequests { get; set; } = new();
        public List<SubscriberItemViewModel> ActiveSubscribers { get; set; } = new();
        public int TotalSubscribersCount { get; set; }
        public int ActiveCount { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
    }

    public class AdminAnalyticsViewModel
    {
        public int TotalArticles { get; set; }
        public int TotalIssues { get; set; }
        public int TotalSubscribers { get; set; }
        public int TotalPlans { get; set; }
        public int TotalPendingRequests { get; set; }
        public decimal EstimatedMonthlyRevenue { get; set; }
        public int TotalViewsEstimate { get; set; }

        public List<string> MonthlyLabels { get; set; } = new();
        public List<int> MonthlySubscriberCounts { get; set; } = new();
        public List<string> CategoryLabels { get; set; } = new();
        public List<int> CategoryArticleCounts { get; set; } = new();
    }

    public class AdminUserItemViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
        public bool EmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTimeOffset? LockoutEnd { get; set; }
    }

    public class AddUserViewModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "FreeUser";
        public bool EmailConfirmed { get; set; } = true;
    }

    public class AdminPaymentRequestItemViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string RequestedItemType { get; set; } = string.Empty; // "Plan" or "Issue"
        public int ItemId { get; set; }
        public string ItemTitle { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string TransactionReference { get; set; } = string.Empty;
        public string ReceiptImagePath { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public DateTime RequestDate { get; set; }
    }

    public class AdminSettingsViewModel
    {
        public string SiteName { get; set; } = "Culture Club";
        public string Tagline { get; set; } = "ALL THINGS THAT MATTER.";
        public string ContactEmail { get; set; } = "editorial@cultureclub.com";
        public string CurrencySymbol { get; set; } = "INR";
        public bool DefaultDarkMode { get; set; } = true;
        public bool EnableNewsletter { get; set; } = true;
        public string TimeZone { get; set; } = "UTC+05:30";
        public string LogoPath { get; set; } = "/images/system/logo.png";
        public IFormFile? LogoImage { get; set; }
    }

    public class CustomerDashboardViewModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public UserSubscription? ActiveSubscription { get; set; }
        public List<SubscriptionRequest> RecentRequests { get; set; } = new();
        public List<MagazineIssue> AvailableIssues { get; set; } = new();
    }

    public class MyAccountViewModel
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public bool IsVipSubscriber { get; set; }
        public bool IsAdmin { get; set; }
        public UserSubscription? ActiveSubscription { get; set; }
        public List<Purchase> Purchases { get; set; } = new();
        public List<MagazineIssue> SuggestedReading { get; set; } = new();
        public List<MagazineIssue> RecentlyViewed { get; set; } = new();
        public List<MagazineIssue> VipAccessIssues { get; set; } = new();
    }
}
