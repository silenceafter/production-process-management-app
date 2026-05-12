using PpmBackend.Models.Planning.Orders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Planning.Scheduling
{
    [Table("operation_dependencies", Schema = "planning")]
    public class OperationDependency
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Column("predecessor_id")]  // 🔑 Явный маппинг на snake_case
        public Guid PredecessorId { get; set; }

        [ForeignKey(nameof(PredecessorId))]
        public OrderOperation? Predecessor { get; set; }

        [Required]
        [Column("successor_id")]
        public Guid SuccessorId { get; set; }

        [ForeignKey(nameof(SuccessorId))]
        public OrderOperation? Successor { get; set; }

        // 🔑 enum → string конвертация настраивается в DbContext
        [Column("type")]
        public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;

        [Column("lag_minutes")]
        public int LagMinutes { get; set; } = 0;
    }

    public enum DependencyType
    {
        FinishToStart = 0,  // FS: конец предшественника → начало преемника
        StartToStart = 1,   // SS: начало предшественника → начало преемника
        FinishToFinish = 2, // FF: конец предшественника → конец преемника
        StartToFinish = 3   // SF: начало предшественника → конец преемника (редко)
    }
}
