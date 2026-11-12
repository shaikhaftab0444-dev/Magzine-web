using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    [Authorize]
    public class MyAccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyAccountController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /MyAccount/Index or /MyAccount
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var currentUserId = user.Id;

            // 1. Purchased Library: Query _context.Purchases.Include(p => p.MagazineIssue).Where(p => p.UserId == currentUserId).ToList()
            var purchases = await _context.Purchases
                .Include(p => p.MagazineIssue)
                    .ThenInclude(i => i!.Magazine)
                        .ThenInclude(m => m!.Category)
                .Include(p => p.MagazineIssue)
                    .ThenInclude(i => i!.Category)
                .Where(p => p.UserId == currentUserId && p.MagazineIssueId != null)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            // 2. Active Subscription Check
            var activeSubscription = await _context.UserSubscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.UserId == currentUserId && s.Status == "Active" && s.EndDate >= DateTime.UtcNow)
                .OrderByDescending(s => s.EndDate)
                .FirstOrDefaultAsync();

            bool isVipSubscriber = activeSubscription != null || await _userManager.IsInRoleAsync(user, "Subscriber");

            // 3. Reading History: Fallback to last 3 published MagazineIssues as Suggested Reading
            var suggestedReading = await _context.MagazineIssues
                .Include(i => i.Magazine)
                    .ThenInclude(m => m!.Category)
                .Include(i => i.Category)
                .Where(i => i.IsPublished)
                .OrderByDescending(i => i.PublishDate)
                .Take(3)
                .ToListAsync();

            var model = new MyAccountViewModel
            {
                UserName = user.UserName ?? user.Email ?? "Member",
                UserEmail = user.Email ?? string.Empty,
                IsVipSubscriber = isVipSubscriber,
                IsAdmin = await _userManager.IsInRoleAsync(user, "Admin"),
                ActiveSubscription = activeSubscription,
                Purchases = purchases,
                SuggestedReading = suggestedReading,
                RecentlyViewed = suggestedReading
            };

            return View(model);
        }

        // GET: /MyAccount/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var history = await _context.ReadingHistories
                .Include(h => h.MagazineIssue)
                    .ThenInclude(i => i!.Magazine)
                        .ThenInclude(m => m!.Category)
                .Include(h => h.MagazineIssue)
                    .ThenInclude(i => i!.Category)
                .Where(h => h.UserId == user.Id && h.MagazineIssue != null)
                .OrderByDescending(h => h.LastReadDate)
                .ToListAsync();

            return View(history);
        }
    }
}
