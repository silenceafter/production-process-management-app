using PpmBackend.Models.Engineering.Dictionaries;
using PpmBackend.Models.Engineering.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Planning.Orders
{
    [Table("order_operations", Schema = "planning")]
    public class OrderOperation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Column("work_order_id")]
        public Guid WorkOrderId { get; set; }

        [ForeignKey(nameof(WorkOrderId))]
        public WorkOrder? WorkOrder { get; set; }

        [Required]
        [Column("operation_id")]
        public Guid OperationId { get; set; }

        [ForeignKey(nameof(OperationId))]
        public Operation? TemplateOperation { get; set; }

        // 🔑 ИСПРАВЛЕНО: 50 вместо 20
        [MaxLength(50)]
        [Column("operation_code")]
        public string? OperationCode { get; set; }  // Nullable, как в БД

        [Column("sequence")]
        public int Sequence { get; set; } = 0;  // 🔑 Дефолт 0

        // 🔑 Поля времени (PERT) — ДОБАВЛЕНО
        [Column("setup_minutes", TypeName = "numeric(10,2)")]
        public decimal? SetupMinutes { get; set; }

        [Column("unit_minutes_per_piece", TypeName = "numeric(10,2)")]
        public decimal? UnitMinutesPerPiece { get; set; }

        [Column("duration_opt", TypeName = "numeric(10,2)")]
        public decimal? DurationOptimistic { get; set; }

        [Column("duration_most_likely", TypeName = "numeric(10,2)")]
        public decimal? DurationMostLikely { get; set; }

        [Column("duration_pessimistic", TypeName = "numeric(10,2)")]
        public decimal? DurationPessimistic { get; set; }

        [Column("duration_expected", TypeName = "numeric(10,2)")]
        public decimal? DurationExpected { get; set; }

        [Column("variance", TypeName = "numeric(10,4)")]
        public decimal? Variance { get; set; }

        // 🔑 Вычисляемое свойство: общая длительность для заказа
        public decimal? GetTotalDuration(int quantity) =>
            SetupMinutes + (UnitMinutesPerPiece * quantity);

        // 🔑 Вычисляемое: ожидаемая длительность по формуле PERT
        [NotMapped]
        public decimal? CalculatedExpectedDuration =>
            DurationOptimistic.HasValue && DurationMostLikely.HasValue && DurationPessimistic.HasValue
                ? (DurationOptimistic.Value + 4 * DurationMostLikely.Value + DurationPessimistic.Value) / 6
                : DurationExpected;

        // Планируемые даты
        [Column("scheduled_start")]
        public DateTimeOffset? ScheduledStart { get; set; }

        [Column("scheduled_end")]
        public DateTimeOffset? ScheduledEnd { get; set; }

        // Фактические даты
        [Column("actual_start")]
        public DateTimeOffset? ActualStart { get; set; }

        [Column("actual_end")]
        public DateTimeOffset? ActualEnd { get; set; }

        // PERT-расчёты
        [Column("earliest_start")]
        public double? EarliestStart { get; set; }

        [Column("latest_start")]
        public double? LatestStart { get; set; }

        [Column("is_critical_path")]
        public bool IsCriticalPath { get; set; } = false;  // 🔑 Дефолт false

        // 🔑 Status как string с конвертацией (или оставьте enum + конвертация в DbContext)
        [Column("status", TypeName = "varchar(50)")]
        public string Status { get; set; } = "Pending";  // "Pending", "InProgress", "Completed"

        // Оборудование
        [Column("assigned_equipment_id")]
        public Guid? AssignedEquipmentId { get; set; }

        [ForeignKey(nameof(AssignedEquipmentId))]
        public Equipment? AssignedEquipment { get; set; } = null!;

        // Выполнение
        [Column("qty_completed")]
        public int QtyCompleted { get; set; } = 0;  // 🔑 Не nullable, дефолт 0
    }
}
