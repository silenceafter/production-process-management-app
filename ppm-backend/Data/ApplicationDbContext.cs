using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PpmBackend.Models.Engineering.Dictionaries;
using PpmBackend.Models.Engineering.Products;
using PpmBackend.Models.Engineering.Resources;
using PpmBackend.Models.Engineering.Technologies;
using PpmBackend.Models.Identity;
using PpmBackend.Models.Planning.Orders;
using PpmBackend.Models.Planning.Scheduling;
using MeasuringToolType = PpmBackend.Models.Engineering.Dictionaries.MeasuringToolType;
using ToolingType = PpmBackend.Models.Engineering.Dictionaries.ToolingType;

namespace PpmBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔑 Устанавливаем схему по умолчанию
            modelBuilder.HasDefaultSchema("engineering");        

            // 🔑 Настройка Identity таблиц с правильными именами колонок
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable("AspNetUsers", "identity");

                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.UserName).HasColumnName("UserName").HasMaxLength(256);
                entity.Property(e => e.NormalizedUserName).HasColumnName("NormalizedUserName").HasMaxLength(256);
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(256);
                entity.Property(e => e.NormalizedEmail).HasColumnName("NormalizedEmail").HasMaxLength(256);
                entity.Property(e => e.EmailConfirmed).HasColumnName("EmailConfirmed");
                entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash");
                entity.Property(e => e.SecurityStamp).HasColumnName("SecurityStamp");
                entity.Property(e => e.ConcurrencyStamp).HasColumnName("ConcurrencyStamp");
                entity.Property(e => e.PhoneNumber).HasColumnName("PhoneNumber");
                entity.Property(e => e.PhoneNumberConfirmed).HasColumnName("PhoneNumberConfirmed");
                entity.Property(e => e.TwoFactorEnabled).HasColumnName("TwoFactorEnabled");
                entity.Property(e => e.LockoutEnd).HasColumnName("LockoutEnd");
                entity.Property(e => e.LockoutEnabled).HasColumnName("LockoutEnabled");
                entity.Property(e => e.AccessFailedCount).HasColumnName("AccessFailedCount");

                // Ваши кастомные поля
                entity.Property(e => e.FirstName).HasColumnName("FirstName");
                entity.Property(e => e.LastName).HasColumnName("LastName");
            });

            modelBuilder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable("AspNetRoles", "identity");
                entity.Property(e => e.Id).HasColumnName("Id");
                entity.Property(e => e.Name).HasColumnName("Name").HasMaxLength(256);
                entity.Property(e => e.NormalizedName).HasColumnName("NormalizedName").HasMaxLength(256);
                entity.Property(e => e.ConcurrencyStamp).HasColumnName("ConcurrencyStamp");
            });

            // Остальные Identity таблицы
            modelBuilder.Entity<IdentityUserRole<string>>(e =>
            {
                e.ToTable("AspNetUserRoles", "identity");
                e.Property(e => e.UserId).HasColumnName("UserId");
                e.Property(e => e.RoleId).HasColumnName("RoleId");
                e.HasKey(r => new { r.UserId, r.RoleId });
            });

            modelBuilder.Entity<IdentityUserClaim<string>>(e =>
            {
                e.ToTable("AspNetUserClaims", "identity");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.UserId).HasColumnName("UserId");
                e.Property(e => e.ClaimType).HasColumnName("ClaimType");
                e.Property(e => e.ClaimValue).HasColumnName("ClaimValue");
            });

            modelBuilder.Entity<IdentityUserLogin<string>>(e =>
            {
                e.ToTable("AspNetUserLogins", "identity");
                e.Property(e => e.LoginProvider).HasColumnName("LoginProvider");
                e.Property(e => e.ProviderKey).HasColumnName("ProviderKey");
                e.Property(e => e.ProviderDisplayName).HasColumnName("ProviderDisplayName");
                e.Property(e => e.UserId).HasColumnName("UserId");
                e.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            });

            modelBuilder.Entity<IdentityRoleClaim<string>>(e =>
            {
                e.ToTable("AspNetRoleClaims", "identity");
                e.Property(e => e.Id).HasColumnName("Id");
                e.Property(e => e.RoleId).HasColumnName("RoleId");
                e.Property(e => e.ClaimType).HasColumnName("ClaimType");
                e.Property(e => e.ClaimValue).HasColumnName("ClaimValue");
            });

            modelBuilder.Entity<IdentityUserToken<string>>(e =>
            {
                e.ToTable("AspNetUserTokens", "identity");
                e.Property(e => e.UserId).HasColumnName("UserId");
                e.Property(e => e.LoginProvider).HasColumnName("LoginProvider");
                e.Property(e => e.Name).HasColumnName("Name");
                e.Property(e => e.Value).HasColumnName("Value");
                e.HasKey(t => new { t.UserId, t.LoginProvider, t.Name });
            });
        }    
    }
}