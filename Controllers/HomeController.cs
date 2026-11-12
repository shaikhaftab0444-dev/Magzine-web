using System.Diagnostics;
using Digital_Magzine.Data;
using Digital_Magzine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Digital_Magzine.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            // Fetch real categories and published magazine issues from the live SQL database
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();

            var issues = await _context.MagazineIssues
                .Include(m => m.Category)
                .Include(m => m.Magazine)
                    .ThenInclude(mag => mag!.Category)
                .Where(m => m.Status == "Published" || m.IsPublished)
                .OrderByDescending(m => m.PublishDate)
                .ToListAsync();

            var magazines = await _context.Magazines
                .Include(m => m.Category)
                .ToListAsync();

            // Group published MagazineIssues by their actual, database-connected Category.Name
            var issuesByCategory = issues
                .GroupBy(i => i.Category?.Name ?? i.Magazine?.Category?.Name ?? "General")
                .ToDictionary(g => g.Key, g => g.ToList());

            var viewModel = new StorefrontViewModel
            {
                MostPopular = issues.Take(8).ToList(),
                NewArrivals = issues.Skip(1).Take(8).ToList(),
                Categories = categories,
                FeaturedMagazines = magazines.Take(6).ToList(),
                IssuesByCategory = issuesByCategory
            };

            if (viewModel.NewArrivals.Count == 0)
            {
                viewModel.NewArrivals = issues.ToList();
            }

            var heroMagazines = await _context.MagazineIssues
                .Include(m => m.Category)
                .Where(m => m.Status == "Published" || m.IsPublished)
                .OrderByDescending(m => m.PublishDate)
                .Take(3)
                .ToListAsync();

            if (heroMagazines.Count < 3 && issues.Any())
            {
                heroMagazines = issues.Take(3).ToList();
            }

            ViewBag.HeroMagazines = heroMagazines;

            var allPlans = await _context.SubscriptionPlans.ToListAsync();
            var vipPlan = allPlans.FirstOrDefault(p => p.Name.Contains("VIP", StringComparison.OrdinalIgnoreCase) || p.DurationMonths == 12 || p.DurationInMonths == 12);
            ViewBag.VipPrice = vipPlan != null ? vipPlan.Price.ToString("0") : "999";

            ViewBag.Categories = categories;
            ViewBag.Issues = issues;

            return View(viewModel);
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
