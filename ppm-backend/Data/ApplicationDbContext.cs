using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PpmBackend.Models;
using PpmBackend.Models.Dictionaries;

namespace PpmBackend.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Справочники
        public DbSet<Job> Jobs { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Tooling> Tooling { get; set; }
        public DbSet<MeasuringTool> MeasuringTools { get; set; }
        public DbSet<Material> Materials { get; set; }
        public DbSet<Component> Components { get; set; }

        // Изделия и маршруты
        public DbSet<Product> Products { get; set; }
        public DbSet<BomItem> BomItems { get; set; }
        public DbSet<Operation> Operations { get; set; }

        // Планирование
        public DbSet<WorkOrder> WorkOrders { get; set; }
        public DbSet<OrderOperation> OrderOperations { get; set; }
        public DbSet<OperationDependency> OperationDependencies { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 🔐 Identity
            builder.Entity<ApplicationUser>().ToTable("AspNetUsers", "identity");
            builder.Entity<IdentityRole>().ToTable("AspNetRoles", "identity");
            builder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles", "identity");
            builder.Entity<IdentityUserClaim<string>>().ToTable("AspNetUserClaims", "identity");
            builder.Entity<IdentityUserLogin<string>>().ToTable("AspNetUserLogins", "identity");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("AspNetRoleClaims", "identity");
            builder.Entity<IdentityUserToken<string>>().ToTable("AspNetUserTokens", "identity");

            // 📐 Engineering: справочники + техподготовка
            builder.Entity<Job>().ToTable("jobs", "engineering");
            builder.Entity<Equipment>().ToTable("equipment", "engineering");
            builder.Entity<Tooling>().ToTable("tooling", "engineering");
            builder.Entity<MeasuringTool>().ToTable("measuring_tools", "engineering");
            builder.Entity<Material>().ToTable("materials", "engineering");
            builder.Entity<Component>().ToTable("components", "engineering");
            builder.Entity<Product>().ToTable("products", "engineering");
            builder.Entity<BomItem>().ToTable("bom_items", "engineering");
            builder.Entity<Operation>().ToTable("operations", "engineering");

            // 📅 Planning: оперативные данные и PERT
            builder.Entity<WorkOrder>().ToTable("work_orders", "planning");
            builder.Entity<OrderOperation>().ToTable("order_operations", "planning");
            builder.Entity<OperationDependency>().ToTable("operation_dependencies", "planning");

            // 🔗 Связи (cross-schema FK работают штатно)
            builder.Entity<Operation>()
                .HasOne(o => o.Equipment).WithMany().HasForeignKey(o => o.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Operation>()
                .HasOne(o => o.Job).WithMany().HasForeignKey(o => o.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderOperation>()
                .HasOne(o => o.TemplateOperation).WithMany()
                .HasForeignKey(o => o.OperationId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<WorkOrder>()
                .HasOne(w => w.CreatedBy).WithMany().HasForeignKey(w => w.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OrderOperation>()
                .HasOne(o => o.AssignedEquipment).WithMany()
                .HasForeignKey(o => o.AssignedEquipmentId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<OperationDependency>()
                .HasOne(d => d.Predecessor).WithMany(o => o.Successors)
                .HasForeignKey(d => d.PredecessorId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<OperationDependency>()
                .HasOne(d => d.Successor).WithMany(o => o.Predecessors)
                .HasForeignKey(d => d.SuccessorId).OnDelete(DeleteBehavior.Restrict);

            // 🔍 Индексы
            builder.Entity<WorkOrder>().HasIndex(w => w.OrderNumber).IsUnique().HasFilter("order_number IS NOT NULL");
            builder.Entity<OrderOperation>().HasIndex(o => new { o.AssignedEquipmentId, o.ScheduledStart });
            builder.Entity<Product>().HasIndex(p => p.DrawingNumber).IsUnique().HasFilter("drawing_number IS NOT NULL");
        }
    }
}