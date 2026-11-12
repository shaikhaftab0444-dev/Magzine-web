using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    public class SubscriptionPlansController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SubscriptionPlansController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: SubscriptionPlans
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var plans = await _context.SubscriptionPlans
                .Include(p => p.UserSubscriptions)
                .OrderBy(p => p.DurationInMonths)
                .ThenBy(p => p.Price)
                .ToListAsync();

            UserSubscription? activeSub = null;
            var pendingRequests = new List<PaymentRequest>();
            var pendingPlanIds = new List<int>();

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUserId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(currentUserId))
                {
                    activeSub = await _context.UserSubscriptions
                        .Include(s => s.SubscriptionPlan)
                        .Where(s => s.UserId == currentUserId && s.Status == "Active" && s.EndDate >= DateTime.UtcNow)
                        .OrderByDescending(s => s.EndDate)
                        .FirstOrDefaultAsync();

                    pendingRequests = await _context.PaymentRequests
                        .Where(pr => pr.UserId == currentUserId && pr.Status == "Pending")
                        .ToListAsync();

                    pendingPlanIds = pendingRequests
                        .Where(pr => pr.RequestedItemType == "Plan")
                        .Select(pr => pr.ItemId)
                        .ToList();
                }
            }

            ViewBag.ActiveSubscription = activeSub;
            ViewBag.PendingRequests = pendingRequests;
            ViewBag.PendingPlanIds = pendingPlanIds;

            return View(plans);
        }

        // GET: SubscriptionPlans/Details/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan = await _context.SubscriptionPlans
                .Include(p => p.UserSubscriptions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        // GET: SubscriptionPlans/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var plan = new SubscriptionPlan
            {
                IsActive = true,
                DurationInMonths = 1,
                Price = 499.00m
            };
            return View(plan);
        }

        // POST: SubscriptionPlans/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Price,DurationInMonths,DurationMonths,IsActive")] SubscriptionPlan plan, [FromForm] int? DurationMonths = null)
        {
            if (DurationMonths.HasValue && DurationMonths.Value > 0)
            {
                plan.DurationInMonths = DurationMonths.Value;
            }
            if (plan.DurationInMonths <= 0)
            {
                plan.DurationInMonths = 1;
            }
            ModelState.Remove("DurationInMonths");
            ModelState.Remove("DurationMonths");

            if (ModelState.IsValid)
            {
                _context.Add(plan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Subscription Plan created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(plan);
        }

        // GET: SubscriptionPlans/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan == null)
            {
                return NotFound();
            }
            return View(plan);
        }

        // POST: SubscriptionPlans/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Price,DurationInMonths,DurationMonths,IsActive")] SubscriptionPlan plan, [FromForm] int? DurationMonths = null)
        {
            if (id != plan.Id)
            {
                return NotFound();
            }

            if (DurationMonths.HasValue && DurationMonths.Value > 0)
            {
                plan.DurationInMonths = DurationMonths.Value;
            }
            if (plan.DurationInMonths <= 0)
            {
                plan.DurationInMonths = 1;
            }
            ModelState.Remove("DurationInMonths");
            ModelState.Remove("DurationMonths");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(plan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Subscription Plan updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SubscriptionPlanExists(plan.Id))
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
            return View(plan);
        }

        // GET: SubscriptionPlans/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan = await _context.SubscriptionPlans
                .Include(p => p.UserSubscriptions)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        // POST: SubscriptionPlans/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan != null)
            {
                _context.SubscriptionPlans.Remove(plan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Subscription Plan deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SubscriptionPlanExists(int id)
        {
            return _context.SubscriptionPlans.Any(e => e.Id == id);
        }
    }
}
