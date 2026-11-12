using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    [Authorize(Roles = "Subscriber,Admin")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CustomerController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Customer
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return await Dashboard();
        }

        // GET: /Customer/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var activeSub = await _context.UserSubscriptions
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == "Active" && s.EndDate >= DateTime.UtcNow);

            var recentRequests = await _context.SubscriptionRequests
                .Include(r => r.SubscriptionPlan)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .ToListAsync();

            var availableIssues = await _context.MagazineIssues
                .Include(m => m.Magazine)
                    .ThenInclude(mag => mag!.Category)
                .Include(m => m.Category)
                .Where(m => m.Status == "Published" || m.IsPublished)
                .OrderByDescending(m => m.PublishDate)
                .Take(8)
                .ToListAsync();

            var model = new CustomerDashboardViewModel
            {
                UserEmail = User.Identity?.Name ?? "Subscriber",
                ActiveSubscription = activeSub,
                RecentRequests = recentRequests,
                AvailableIssues = availableIssues
            };

            return View("Dashboard", model);
        }

        // GET: /Customer/Library
        [HttpGet]
        public async Task<IActionResult> Library()
        {
            var catalog = await _context.MagazineIssues
                .Include(m => m.Category)
                .Include(m => m.Magazine)
                .Where(m => m.Status == "Published" || m.IsPublished)
                .OrderByDescending(m => m.PublishDate)
                .ToListAsync();

            return View(catalog);
        }

        // GET: /Customer/History
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var history = await _context.ReadingHistories
                .Include(h => h.MagazineIssue)
                    .ThenInclude(i => i!.Category)
                .Include(h => h.MagazineIssue)
                    .ThenInclude(i => i!.Magazine)
                .Where(h => h.UserId == userId && h.MagazineIssue != null)
                .OrderByDescending(h => h.LastReadDate)
                .ToListAsync();

            return View(history);
        }
    }
}
