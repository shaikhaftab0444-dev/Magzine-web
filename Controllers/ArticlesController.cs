using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ArticlesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Articles (Public read-only)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var articles = await _context.Articles
                .Include(a => a.MagazineIssue)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(articles);
        }

        // GET: Articles/Details/5 (Public read-only with premium check)
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.MagazineIssue)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            // Check if article is premium and if user is authorized (Admin or Subscriber)
            bool isPrivileged = User.IsInRole("Admin") || User.IsInRole("Subscriber");
            ViewBag.CanReadFullContent = !article.IsPremium || isPrivileged;

            return View(article);
        }

        // GET: Articles/Create (Admin only)
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            PopulateMagazineIssuesDropDownList();
            var article = new Article
            {
                CreatedAt = DateTime.UtcNow,
                IsPremium = true
            };
            return View(article);
        }

        // POST: Articles/Create (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MagazineIssueId,Title,ContentHtml,Author,IsPremium,CreatedAt")] Article article)
        {
            // Remove navigation property from model validation if present
            ModelState.Remove(nameof(Article.MagazineIssue));

            if (ModelState.IsValid)
            {
                if (article.CreatedAt == default)
                {
                    article.CreatedAt = DateTime.UtcNow;
                }

                _context.Add(article);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Article created successfully.";
                return RedirectToAction(nameof(Index));
            }

            PopulateMagazineIssuesDropDownList(article.MagazineIssueId);
            return View(article);
        }

        // GET: Articles/Edit/5 (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.FindAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            PopulateMagazineIssuesDropDownList(article.MagazineIssueId);
            return View(article);
        }

        // POST: Articles/Edit/5 (Admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MagazineIssueId,Title,ContentHtml,Author,IsPremium,CreatedAt")] Article article)
        {
            if (id != article.Id)
            {
                return NotFound();
            }

            ModelState.Remove(nameof(Article.MagazineIssue));

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(article);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Article updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticleExists(article.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateMagazineIssuesDropDownList(article.MagazineIssueId);
            return View(article);
        }

        // GET: Articles/Delete/5 (Admin only)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.MagazineIssue)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

        // POST: Articles/Delete/5 (Admin only)
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Articles.FindAsync(id);
            if (article != null)
            {
                _context.Articles.Remove(article);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Article deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ArticleExists(int id)
        {
            return _context.Articles.Any(e => e.Id == id);
        }

        private void PopulateMagazineIssuesDropDownList(object? selectedIssue = null)
        {
            var issuesQuery = _context.MagazineIssues
                .OrderBy(m => m.Volume)
                .ThenBy(m => m.Title)
                .Select(m => new
                {
                    m.Id,
                    DisplayText = $"Vol {m.Volume}: {m.Title}"
                });

            ViewBag.MagazineIssueId = new SelectList(issuesQuery, "Id", "DisplayText", selectedIssue);
        }
    }
}
