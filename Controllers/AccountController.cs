using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

namespace Digital_Magzine.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Live Session Tracking Hook
                    try
                    {
                        string userAgent = Request.Headers["User-Agent"].ToString();
                        if (string.IsNullOrWhiteSpace(userAgent))
                        {
                            userAgent = "Unknown Browser / Client";
                        }
                        else if (userAgent.Length > 490)
                        {
                            userAgent = userAgent.Substring(0, 490);
                        }

                        string ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                        if (Request.Headers.ContainsKey("X-Forwarded-For"))
                        {
                            var forwarded = Request.Headers["X-Forwarded-For"].ToString();
                            if (!string.IsNullOrWhiteSpace(forwarded))
                            {
                                ip = forwarded.Split(',')[0].Trim();
                            }
                        }

                        var sessionTrack = new UserSessionTrack
                        {
                            UserEmail = model.Email.Trim(),
                            LoginTime = DateTime.UtcNow,
                            DeviceName = userAgent,
                            IPAddress = ip,
                            Status = "Active"
                        };

                        _context.UserSessionTracks.Add(sessionTrack);
                        await _context.SaveChangesAsync();
                    }
                    catch
                    {
                        // Session tracking should never block user login
                    }

                    var user = await _userManager.FindByEmailAsync(model.Email);
                    if (user != null)
                    {
                        if (await _userManager.IsInRoleAsync(user, "Admin"))
                        {
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/" && !returnUrl.Equals(Url.Action("Index", "Home"), StringComparison.OrdinalIgnoreCase))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Dashboard", "Admin");
                        }
                        if (await _userManager.IsInRoleAsync(user, "Subscriber"))
                        {
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/" && !returnUrl.Equals(Url.Action("Index", "Home"), StringComparison.OrdinalIgnoreCase))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Dashboard", "Customer");
                        }
                        if (await _userManager.IsInRoleAsync(user, "FreeUser"))
                        {
                            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/" && !returnUrl.Equals(Url.Action("Index", "Home"), StringComparison.OrdinalIgnoreCase))
                            {
                                return Redirect(returnUrl);
                            }
                            return RedirectToAction("Index", "MyAccount");
                        }
                    }

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/" && !returnUrl.Equals(Url.Action("Index", "Home"), StringComparison.OrdinalIgnoreCase))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "MyAccount");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: /Account/Register
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Assign default 3-tier role FreeUser
                    if (await _roleManager.RoleExistsAsync("FreeUser"))
                    {
                        await _userManager.AddToRoleAsync(user, "FreeUser");
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/" && !returnUrl.Equals(Url.Action("Index", "Home"), StringComparison.OrdinalIgnoreCase))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "MyAccount");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity?.Name;
            if (!string.IsNullOrEmpty(userEmail))
            {
                try
                {
                    var activeSession = await _context.UserSessionTracks
                        .Where(s => s.UserEmail == userEmail && s.Status == "Active")
                        .OrderByDescending(s => s.LoginTime)
                        .FirstOrDefaultAsync();

                    if (activeSession != null)
                    {
                        activeSession.Status = "Logged Out";
                        await _context.SaveChangesAsync();
                    }
                }
                catch
                {
                    // Ignore session update error on logout
                }
            }

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // GET: /Account/Manage
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Manage()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            return View(user);
        }
    }
}
