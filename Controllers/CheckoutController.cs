using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IWebHostEnvironment _environment;

        public CheckoutController(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            SignInManager<IdentityUser> signInManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _environment = environment;
        }

        // GET: /Checkout/Payment?planId=1 or /Checkout/Payment?issueId=5
        [HttpGet]
        public async Task<IActionResult> Payment(int? planId, int? issueId)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Payment", "Checkout", new { planId, issueId }) });
            }

            var model = new PaymentViewModel();

            // Single Issue Flow
            if (issueId.HasValue && issueId.Value > 0)
            {
                var issue = await _context.MagazineIssues
                    .Include(i => i.Magazine)
                    .FirstOrDefaultAsync(i => i.Id == issueId.Value);

                if (issue != null)
                {
                    model.IssueId = issue.Id;
                    model.IssueTitle = issue.Title;
                    model.IssueCoverImage = !string.IsNullOrEmpty(issue.CoverImagePath) ? issue.CoverImagePath : issue.CoverImageUrl;
                    model.Price = issue.SingleIssuePrice;
                    model.PlanName = $"Single Issue: {issue.Title}";
                    return View(model);
                }
            }

            // VIP Subscription Plan Flow
            SubscriptionPlan? plan = null;
            if (planId.HasValue && planId.Value > 0)
            {
                plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Id == planId.Value);
            }

            if (plan == null)
            {
                plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.IsActive);
            }

            if (plan == null)
            {
                // Fallback virtual default plan
                plan = new SubscriptionPlan
                {
                    Id = 1,
                    Name = "Culture Club All-Access VIP",
                    Price = 499.00m,
                    DurationInMonths = 1,
                    IsActive = true
                };
            }

            model.PlanId = plan.Id;
            model.PlanName = plan.Name;
            model.Price = plan.Price;
            model.DurationInMonths = plan.DurationInMonths > 0 ? plan.DurationInMonths : 1;

            return View(model);
        }

        // POST: /Checkout/ProcessPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model, IFormFile? receiptImage)
        {
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Payment", "Checkout", new { planId = model.PlanId, issueId = model.IssueId }) });
            }

            if (string.IsNullOrWhiteSpace(model.TransactionReference))
            {
                ModelState.AddModelError("TransactionReference", "Transaction Reference (UPI ID or UTR Number) is required.");
            }

            if (!ModelState.IsValid)
            {
                return View("Payment", model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Save uploaded screenshot to wwwroot/images/receipts/
            string receiptImagePath = string.Empty;
            if (receiptImage != null && receiptImage.Length > 0)
            {
                var webRoot = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var receiptsFolder = Path.Combine(webRoot, "images", "receipts");
                if (!Directory.Exists(receiptsFolder))
                {
                    Directory.CreateDirectory(receiptsFolder);
                }

                var extension = Path.GetExtension(receiptImage.FileName);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".jpg";
                }
                var uniqueFileName = $"{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}{extension}";
                var fullPath = Path.Combine(receiptsFolder, uniqueFileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await receiptImage.CopyToAsync(stream);
                }
                receiptImagePath = $"/images/receipts/{uniqueFileName}";
            }
            else
            {
                receiptImagePath = "/images/receipts/default_receipt.png";
            }

            // Determine RequestedItemType, ItemId, and AmountPaid
            string requestedItemType;
            int itemId;
            decimal amountPaid = model.Price;

            if (model.IssueId.HasValue && model.IssueId.Value > 0)
            {
                requestedItemType = "Issue";
                itemId = model.IssueId.Value;
                var issue = await _context.MagazineIssues.FindAsync(itemId);
                if (issue != null)
                {
                    amountPaid = issue.SingleIssuePrice;
                }
            }
            else
            {
                requestedItemType = "Plan";
                var plan = model.PlanId.HasValue ? await _context.SubscriptionPlans.FindAsync(model.PlanId.Value) : null;
                if (plan == null)
                {
                    plan = await _context.SubscriptionPlans.FirstOrDefaultAsync(p => p.IsActive);
                }
                itemId = plan?.Id ?? 1;
                amountPaid = plan?.Price ?? model.Price;
            }

            // Create new PaymentRequest record with Status "Pending"
            var paymentRequest = new PaymentRequest
            {
                UserId = user.Id,
                RequestedItemType = requestedItemType,
                ItemId = itemId,
                AmountPaid = amountPaid,
                TransactionReference = model.TransactionReference.Trim(),
                ReceiptImagePath = receiptImagePath,
                Status = "Pending",
                RequestDate = DateTime.UtcNow
            };

            _context.PaymentRequests.Add(paymentRequest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment submitted. Awaiting Admin verification.";
            return RedirectToAction("Index", "MyAccount");
        }
    }
}
