using PpmBackend.Models.Engineering.Products;
using PpmBackend.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Planning.Orders
{
    [Table("work_orders", Schema = "planning")]
    public class WorkOrder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]  // 🔑 NOT NULL в БД
        [Column("order_number")]
        public string OrderNumber { get; set; } = "";

        [Required]
        [Column("product_id")]
        public Guid ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        [Range(1, int.MaxValue)]
        [Column("quantity")]
        public int Quantity { get; set; } = 1;

        [Required]
        [Column("planned_start")]
        public DateTimeOffset PlannedStart { get; set; }

        [Column("due_date")]
        public DateTimeOffset? DueDate { get; set; }

        // 🔑 Status как string (или enum + конвертация в DbContext)
        [Required, MaxLength(50)]
        [Column("status")]
        public string Status { get; set; } = "Draft";  // "Draft", "Released", "InProgress", "Completed", "Cancelled"

        [MaxLength(500)]
        [Column("notes")]
        public string? Notes { get; set; }

        [Column("created_by_id")]
        public string? CreatedById { get; set; }

        [ForeignKey(nameof(CreatedById))]
        public ApplicationUser? CreatedBy { get; set; }

        // 🔑 ИСПРАВЛЕНО: Computed вместо Identity для DEFAULT now()
        [Column("created_at")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTimeOffset? UpdatedAt { get; set; }

        // 🔑 Навигация
        public ICollection<OrderOperation> Operations { get; set; } = new List<OrderOperation>();

        // 🔑 Вычисляемое свойство: прогресс выполнения
        [NotMapped]
        public double CompletionPercent =>
            Operations.Count > 0
                ? 100.0 * Operations.Count(o => o.Status == "Completed") / Operations.Count
                : 0;
    }
}
