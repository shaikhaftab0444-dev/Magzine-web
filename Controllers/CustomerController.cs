using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    [Authorize(Roles = "Subscriber")]
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
                .Where(m => m.IsPublished)
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

            return View("Index", model);
        }

        // GET: /Customer/Dashboard
        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            return await Index();
        }

        // GET: /Customer/History
        [HttpGet]
        public IActionResult History()
        {
            return RedirectToAction("History", "MyAccount");
        }
    }
}
