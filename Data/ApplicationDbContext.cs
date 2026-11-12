using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Digital_Magzine.Models;

namespace Digital_Magzine.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<MagazineIssue> MagazineIssues { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<SubscriptionRequest> SubscriptionRequests { get; set; }

        // Digital Newsstand Platform Architecture
        public DbSet<Magazine> Magazines { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PaymentRequest> PaymentRequests { get; set; }
        public DbSet<ReadingHistory> ReadingHistories { get; set; }
        public DbSet<PlatformSetting> PlatformSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure the 7 ASP.NET Core Identity tables with DM_ prefix
            builder.Entity<IdentityUser>().ToTable("DM_Users");
            builder.Entity<IdentityRole>().ToTable("DM_Roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("DM_UserRoles");
            builder.Entity<IdentityUserClaim<string>>().ToTable("DM_UserClaims");
            builder.Entity<IdentityUserLogin<string>>().ToTable("DM_UserLogins");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("DM_RoleClaims");
            builder.Entity<IdentityUserToken<string>>().ToTable("DM_UserTokens");

            // Configure existing custom tables with DM_ prefix
            builder.Entity<SubscriptionPlan>().ToTable("DM_SubscriptionPlans");
            builder.Entity<UserSubscription>().ToTable("DM_UserSubscriptions");
            builder.Entity<MagazineIssue>().ToTable("DM_MagazineIssues");
            builder.Entity<Article>().ToTable("DM_Articles");
            builder.Entity<SubscriptionRequest>().ToTable("DM_SubscriptionRequests");

            // Configure Digital Newsstand tables with DM_ prefix
            builder.Entity<Magazine>().ToTable("DM_Magazines");
            builder.Entity<Category>().ToTable("DM_Categories");
            builder.Entity<Purchase>().ToTable("DM_Purchases");
            builder.Entity<PaymentRequest>().ToTable("DM_PaymentRequests");
            builder.Entity<ReadingHistory>().ToTable("DM_ReadingHistories");
            builder.Entity<PlatformSetting>().ToTable("DM_PlatformSettings");

            builder.Entity<PlatformSetting>().HasData(
                new PlatformSetting { Id = 1, SettingKey = "BrandName", SettingValue = "Culture Club" },
                new PlatformSetting { Id = 2, SettingKey = "Tagline", SettingValue = "ALL THINGS THAT MATTER." },
                new PlatformSetting { Id = 3, SettingKey = "Currency", SettingValue = "INR" },
                new PlatformSetting { Id = 4, SettingKey = "Timezone", SettingValue = "UTC+05:30" },
                new PlatformSetting { Id = 5, SettingKey = "LogoPath", SettingValue = "/images/system/logo.png" }
            );

            builder.Entity<ReadingHistory>()
                .HasOne(r => r.MagazineIssue)
                .WithMany()
                .HasForeignKey(r => r.MagazineIssueId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure delete behaviors to avoid multiple cascade paths / cycles in SQL Server
            builder.Entity<Purchase>()
                .HasOne(p => p.Magazine)
                .WithMany()
                .HasForeignKey(p => p.MagazineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Purchase>()
                .HasOne(p => p.MagazineIssue)
                .WithMany()
                .HasForeignKey(p => p.MagazineIssueId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MagazineIssue>()
                .Property(m => m.Status)
                .HasDefaultValue("Published");

            builder.Entity<MagazineIssue>()
                .HasOne(m => m.Magazine)
                .WithMany(m => m.MagazineIssues)
                .HasForeignKey(m => m.MagazineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MagazineIssue>()
                .HasOne(m => m.Category)
                .WithMany(c => c.MagazineIssues)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Magazine>()
                .HasOne(m => m.Category)
                .WithMany(m => m.Magazines)
                .HasForeignKey(m => m.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
