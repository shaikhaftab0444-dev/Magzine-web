using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    [Authorize]
    public class SupportController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SupportController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Support or /Support/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);
            var tickets = await _context.SupportTickets
                .Where(t => t.UserId == currentUserId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return View(tickets);
        }

        // POST: /Support/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(subject) || string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Please provide both a subject and message for your support ticket.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var ticket = new SupportTicket
            {
                UserId = user.Id,
                UserEmail = user.Email ?? "Customer",
                Subject = subject.Trim(),
                Message = message.Trim(),
                Status = "Open",
                CreatedAt = DateTime.UtcNow
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            // Notify Administrator
            var adminNotification = new Notification
            {
                UserId = null, // For all Admins
                Title = "New Support Ticket",
                Message = $"Support ticket #{ticket.Id} from {ticket.UserEmail}: {ticket.Subject}",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };
            _context.Notifications.Add(adminNotification);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Support ticket #{ticket.Id} submitted successfully. Our editorial team will review it shortly.";
            return RedirectToAction(nameof(Index));
        }
    }
}
