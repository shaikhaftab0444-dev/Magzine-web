using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    [Authorize]
    public class ReaderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReaderController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Reader/Issue/5
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Issue(int id)
        {
            // If user is unauthenticated, redirect to login
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Reader/Issue/{id}" });
            }

            var issue = await _context.MagazineIssues
                .Include(i => i.Magazine)
                    .ThenInclude(m => m!.Category)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (issue == null)
            {
                return NotFound("Magazine issue not found.");
            }

            // Role & Purchase Fallback (Paywall Check):
            // 1. If issue.SingleIssuePrice == 0, allow any authenticated user to read it.
            // 2. If it has a price, ensure they are Admin, Subscriber, OR have purchased this issue.
            if (issue.SingleIssuePrice > 0)
            {
                bool isAuthorized = User.IsInRole("Admin") || User.IsInRole("Subscriber");
                if (!isAuthorized)
                {
                    var userId = _userManager.GetUserId(User);
                    if (!string.IsNullOrEmpty(userId))
                    {
                        isAuthorized = await _context.Purchases.AnyAsync(p => p.UserId == userId && p.MagazineIssueId == id);
                    }
                }

                if (!isAuthorized)
                {
                    TempData["ErrorMessage"] = "This digital edition requires an active Subscriber membership or single-issue purchase.";
                    return RedirectToAction("Details", "Magazines", new { id = id });
                }
            }

            // Track Reading History for the authenticated user
            var currentUserId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(currentUserId))
            {
                var historyRecord = await _context.ReadingHistories
                    .FirstOrDefaultAsync(h => h.UserId == currentUserId && h.MagazineIssueId == id);

                if (historyRecord != null)
                {
                    historyRecord.LastReadDate = DateTime.UtcNow;
                }
                else
                {
                    _context.ReadingHistories.Add(new ReadingHistory
                    {
                        UserId = currentUserId,
                        MagazineIssueId = id,
                        LastReadDate = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            return View(issue);
        }

        // POST: /Reader/TrackReading
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> TrackReading(int issueId)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId))
            {
                return Unauthorized();
            }

            var historyRecord = await _context.ReadingHistories
                .FirstOrDefaultAsync(h => h.UserId == currentUserId && h.MagazineIssueId == issueId);

            if (historyRecord != null)
            {
                historyRecord.LastReadDate = DateTime.UtcNow;
            }
            else
            {
                _context.ReadingHistories.Add(new ReadingHistory
                {
                    UserId = currentUserId,
                    MagazineIssueId = issueId,
                    LastReadDate = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }
    }
}
