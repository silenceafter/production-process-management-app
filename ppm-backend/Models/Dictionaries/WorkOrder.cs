using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Dictionaries
{
    public class WorkOrder
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)] public string OrderNumber { get; set; } = "";
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        [Range(1, int.MaxValue)] public int Quantity { get; set; }
        public DateTimeOffset PlannedStart { get; set; }
        public DateTimeOffset? DueDate { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Draft;
        [MaxLength(500)] public string? Notes { get; set; }

        // Аудит
        public string? CreatedById { get; set; }
        public ApplicationUser? CreatedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        public ICollection<OrderOperation> Operations { get; set; } = new List<OrderOperation>();
    }
}
