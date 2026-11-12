using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Notifications or /Notifications/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");

            List<Notification> notifications;
            if (isAdmin)
            {
                // Admins see broadcasts/system alerts (UserId == null) as well as any directly addressed to them
                notifications = await _context.Notifications
                    .Where(n => n.UserId == null || n.UserId == currentUserId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }
            else
            {
                notifications = await _context.Notifications
                    .Where(n => n.UserId == currentUserId)
                    .OrderByDescending(n => n.CreatedAt)
                    .ToListAsync();
            }

            return View(notifications);
        }

        // POST: /Notifications/MarkAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");

            var notification = await _context.Notifications.FindAsync(id);
            if (notification != null)
            {
                if (isAdmin && (notification.UserId == null || notification.UserId == currentUserId))
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }
                else if (notification.UserId == currentUserId)
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Notifications/MarkAllAsRead
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var currentUserId = _userManager.GetUserId(User);
            bool isAdmin = User.IsInRole("Admin");

            var notifications = isAdmin
                ? await _context.Notifications.Where(n => (n.UserId == null || n.UserId == currentUserId) && !n.IsRead).ToListAsync()
                : await _context.Notifications.Where(n => n.UserId == currentUserId && !n.IsRead).ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "All notifications marked as read.";
            return RedirectToAction(nameof(Index));
        }
    }
}
