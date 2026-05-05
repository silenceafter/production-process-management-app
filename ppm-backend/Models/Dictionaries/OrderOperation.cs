using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Dictionaries
{
    public class OrderOperation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; } = null!;

        // ⬇️ Чёткое разделение: ссылка на шаблон vs плановая операция
        public Guid OperationId { get; set; }
        public Operation TemplateOperation { get; set; } = null!; // ⬅️ понятное имя

        public int Sequence { get; set; }
        [MaxLength(50)] public string? OperationCode { get; set; } // денормализация для быстрых фильтров

        public DateTimeOffset? ScheduledStart { get; set; }
        public DateTimeOffset? ScheduledEnd { get; set; }
        public DateTimeOffset? ActualStart { get; set; }
        public DateTimeOffset? ActualEnd { get; set; }
        public int QtyCompleted { get; set; }
        public OperationStatus Status { get; set; } = OperationStatus.Pending;

        public Guid? AssignedEquipmentId { get; set; }
        public Equipment? AssignedEquipment { get; set; }

        // PERT/CPM
        public double? EarliestStart { get; set; }
        public double? LatestStart { get; set; }
        public bool IsCriticalPath { get; set; }

        public ICollection<OperationDependency> Predecessors { get; set; } = new List<OperationDependency>();
        public ICollection<OperationDependency> Successors { get; set; } = new List<OperationDependency>();
    }
}
