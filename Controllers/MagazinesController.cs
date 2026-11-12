using Digital_Magzine.Data;
using Digital_Magzine.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Digital_Magzine.Controllers
{
    [AllowAnonymous]
    public class MagazinesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MagazinesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Magazines
        public async Task<IActionResult> Index(int? categoryId)
        {
            var query = _context.MagazineIssues
                .Include(m => m.Magazine)
                .Include(m => m.Category)
                .Where(m => m.Status == "Published" || m.IsPublished);

            if (categoryId.HasValue)
            {
                query = query.Where(m => m.CategoryId == categoryId || (m.Magazine != null && m.Magazine.CategoryId == categoryId));
            }

            var issues = await query
                .OrderByDescending(m => m.PublishDate)
                .ToListAsync();

            ViewBag.Categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            ViewBag.SelectedCategoryId = categoryId;

            return View(issues);
        }

        // GET: Magazines/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var issue = await _context.MagazineIssues
                .Include(m => m.Magazine)
                .Include(m => m.Category)
                .Include(m => m.Articles)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (issue == null)
            {
                return NotFound();
            }

            // Check if current authenticated user has purchased this issue
            bool hasPurchased = false;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    hasPurchased = await _context.Purchases.AnyAsync(p => p.UserId == userId && p.MagazineIssueId == issue.Id);
                }
            }
            ViewBag.HasPurchased = hasPurchased;

            // Also load related issues in the same category or magazine
            ViewBag.RelatedIssues = await _context.MagazineIssues
                .Include(m => m.Category)
                .Where(m => m.Id != issue.Id && (m.CategoryId == issue.CategoryId || m.MagazineId == issue.MagazineId))
                .Where(m => m.Status == "Published" || m.IsPublished)
                .Take(4)
                .ToListAsync();

            return View(issue);
        }

        // GET: Magazines/Search
        public async Task<IActionResult> Search(string? query)
        {
            ViewBag.Query = query ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                return View(new List<MagazineIssue>());
            }

            var cleanQuery = query.Trim();
            var results = await _context.MagazineIssues
                .Include(m => m.Category)
                .Include(m => m.Magazine)
                .Where(m => (m.Status == "Published" || m.IsPublished) &&
                            (m.Title.Contains(cleanQuery) ||
                             (m.Description != null && m.Description.Contains(cleanQuery)) ||
                             (m.Category != null && m.Category.Name.Contains(cleanQuery))))
                .OrderByDescending(m => m.PublishDate)
                .ToListAsync();

            return View(results);
        }
    }
}
