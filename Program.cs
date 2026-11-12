using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Data;
using Digital_Magzine.Models;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.MigrationsHistoryTable("__DM_EFMigrationsHistory");
    }));

// Add ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie login paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<Digital_Magzine.Services.ISettingsService, Digital_Magzine.Services.SettingsService>();
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100 MB
});

var app = builder.Build();

// Synchronously seed roles and default admin user during startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    SeedDatabase(services);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Database seeding method
static void SeedDatabase(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // Create roles if they don't exist: 3-tier system (Admin, Subscriber, FreeUser)
    string[] roleNames = { "Admin", "Subscriber", "FreeUser" };
    foreach (var roleName in roleNames)
    {
        var roleExists = roleManager.RoleExistsAsync(roleName).GetAwaiter().GetResult();
        if (!roleExists)
        {
            roleManager.CreateAsync(new IdentityRole(roleName)).GetAwaiter().GetResult();
        }
    }

    // Create default Admin account if no admin exists
    var adminEmail = "admin@magazine.com";
    var adminUser = userManager.FindByEmailAsync(adminEmail).GetAwaiter().GetResult();
    if (adminUser == null)
    {
        var newAdmin = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = userManager.CreateAsync(newAdmin, "Admin@123").GetAwaiter().GetResult();
        if (result.Succeeded)
        {
            userManager.AddToRoleAsync(newAdmin, "Admin").GetAwaiter().GetResult();
        }
        adminUser = newAdmin;
    }

    var adminId = adminUser?.Id ?? "admin-user";

    // Seed Marketplace Test Data (Categories, Publishers, Magazines, MagazineIssues)
    var context = services.GetRequiredService<ApplicationDbContext>();

    // Seed Platform Settings if missing
    var defaultSettings = new Dictionary<string, string>
    {
        { "BrandName", "Culture Club" },
        { "Tagline", "ALL THINGS THAT MATTER." },
        { "Currency", "INR" },
        { "Timezone", "UTC+05:30" },
        { "LogoPath", "/images/system/logo.png" }
    };

    bool settingsChanged = false;
    foreach (var kvp in defaultSettings)
    {
        if (!context.PlatformSettings.Any(s => s.SettingKey == kvp.Key))
        {
            context.PlatformSettings.Add(new PlatformSetting
            {
                SettingKey = kvp.Key,
                SettingValue = kvp.Value
            });
            settingsChanged = true;
        }
    }
    if (settingsChanged)
    {
        context.SaveChanges();
    }

    // Seed Subscription Plans if none exist
    if (!context.SubscriptionPlans.Any())
    {
        context.SubscriptionPlans.AddRange(
            new SubscriptionPlan { Name = "Monthly VIP All-Access", Price = 499.00m, DurationInMonths = 1, IsActive = true },
            new SubscriptionPlan { Name = "Annual VIP All-Access", Price = 3999.00m, DurationInMonths = 12, IsActive = true }
        );
        context.SaveChanges();
    }

    if (!context.Magazines.Any(m => m.Title == "Tech Weekly"))
    {
        // 1. Categories (Technology, Fashion, Business)
        var techCat = context.Categories.FirstOrDefault(c => c.Name == "Technology");
        if (techCat == null)
        {
            techCat = new Category { Name = "Technology", Description = "Computing, AI, Gadgets & Digital Culture" };
            context.Categories.Add(techCat);
        }

        var fashionCat = context.Categories.FirstOrDefault(c => c.Name == "Fashion");
        if (fashionCat == null)
        {
            fashionCat = new Category { Name = "Fashion", Description = "Haute Couture, Streetwear & Modern Style" };
            context.Categories.Add(fashionCat);
        }

        var businessCat = context.Categories.FirstOrDefault(c => c.Name == "Business");
        if (businessCat == null)
        {
            businessCat = new Category { Name = "Business", Description = "Global Markets, Tech Venture & Leadership" };
            context.Categories.Add(businessCat);
        }

        context.SaveChanges();

        // 2. 5-6 In-House Culture Club Magazines
        var mag1 = new Magazine
        {
            Title = "Tech Weekly",
            CategoryId = techCat.Id,
            SubscriptionPrice = 199.00m,
            CreatedAt = DateTime.UtcNow
        };
        var mag2 = new Magazine
        {
            Title = "Modern Vogue",
            CategoryId = fashionCat.Id,
            SubscriptionPrice = 249.00m,
            CreatedAt = DateTime.UtcNow
        };
        var mag3 = new Magazine
        {
            Title = "Wired Culture",
            CategoryId = techCat.Id,
            SubscriptionPrice = 179.00m,
            CreatedAt = DateTime.UtcNow
        };
        var mag4 = new Magazine
        {
            Title = "Business Insider Daily",
            CategoryId = businessCat.Id,
            SubscriptionPrice = 299.00m,
            CreatedAt = DateTime.UtcNow
        };
        var mag5 = new Magazine
        {
            Title = "Harper's Trend",
            CategoryId = fashionCat.Id,
            SubscriptionPrice = 219.00m,
            CreatedAt = DateTime.UtcNow
        };
        var mag6 = new Magazine
        {
            Title = "Forbes Executive",
            CategoryId = businessCat.Id,
            SubscriptionPrice = 349.00m,
            CreatedAt = DateTime.UtcNow
        };

        context.Magazines.AddRange(mag1, mag2, mag3, mag4, mag5, mag6);
        context.SaveChanges();

        // 4. MagazineIssues with SingleIssuePrice, CoverImageUrl and CategoryId
        var issues = new List<MagazineIssue>
        {
            new MagazineIssue
            {
                MagazineId = mag1.Id,
                CategoryId = techCat.Id,
                Title = "The AI Supercycle & Next-Gen Computing",
                Volume = 48,
                PublishDate = DateTime.Today.AddDays(-2),
                SingleIssuePrice = 99.00m,
                CoverImageUrl = "https://images.unsplash.com/photo-1518770660439-4636190af475?auto=format&fit=crop&w=800&q=80",
                IsPublished = true
            },
            new MagazineIssue
            {
                MagazineId = mag2.Id,
                CategoryId = fashionCat.Id,
                Title = "The Autumn Haute Couture & Runway Edit",
                Volume = 112,
                PublishDate = DateTime.Today.AddDays(-5),
                SingleIssuePrice = 149.00m,
                CoverImageUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=800&q=80",
                IsPublished = true
            },
            new MagazineIssue
            {
                MagazineId = mag4.Id,
                CategoryId = businessCat.Id,
                Title = "Global Market Dynamics & Venture In 2026",
                Volume = 24,
                PublishDate = DateTime.Today.AddDays(-7),
                SingleIssuePrice = 99.00m,
                CoverImageUrl = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?auto=format&fit=crop&w=800&q=80",
                IsPublished = true
            },
            new MagazineIssue
            {
                MagazineId = mag3.Id,
                CategoryId = techCat.Id,
                Title = "Quantum Architecture & Silicon Revolution",
                Volume = 91,
                PublishDate = DateTime.Today.AddDays(-10),
                SingleIssuePrice = 0.00m,
                CoverImageUrl = "https://images.unsplash.com/photo-1526374965328-7f61d4dc18c5?auto=format&fit=crop&w=800&q=80",
                IsPublished = true,
                Status = "Published"
            },
            new MagazineIssue
            {
                MagazineId = mag5.Id,
                CategoryId = fashionCat.Id,
                Title = "Minimalist Elegance & Contemporary Styling",
                Volume = 67,
                PublishDate = DateTime.Today.AddDays(-12),
                SingleIssuePrice = 149.00m,
                CoverImageUrl = "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?auto=format&fit=crop&w=800&q=80",
                IsPublished = true
            },
            new MagazineIssue
            {
                MagazineId = mag6.Id,
                CategoryId = businessCat.Id,
                Title = "The Wealth Report: Billion-Dollar Founders",
                Volume = 55,
                PublishDate = DateTime.Today.AddDays(-15),
                SingleIssuePrice = 199.00m,
                CoverImageUrl = "https://images.unsplash.com/photo-1507679799987-c73779587ccf?auto=format&fit=crop&w=800&q=80",
                IsPublished = true
            },
            new MagazineIssue
            {
                MagazineId = mag1.Id,
                CategoryId = techCat.Id,
                Title = "Autonomous Robotics & Cyber Frontiers",
                Volume = 47,
                PublishDate = DateTime.Today.AddDays(-20),
                SingleIssuePrice = 99.00m,
                CoverImageUrl = "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?auto=format&fit=crop&w=800&q=80",
                IsPublished = true
            },
            new MagazineIssue
            {
                MagazineId = mag2.Id,
                CategoryId = fashionCat.Id,
                Title = "Paris & Milan Fashion Week Exclusive",
                Volume = 111,
                PublishDate = DateTime.Today.AddDays(-25),
                SingleIssuePrice = 149.00m,
                CoverImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=800&q=80",
                IsPublished = true
            }
        };

        context.MagazineIssues.AddRange(issues);
        context.SaveChanges();
    }
}
