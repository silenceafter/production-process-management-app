using PpmBackend.Models.Engineering.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Technologies
{
    [Table("technology_operations", Schema = "engineering")]
    public class TechnologyOperation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // 🔑 Foreign Key: Технология (шаблон)
        [Required]
        [Column("technology_id")]
        public Guid TechnologyId { get; set; }

        [ForeignKey(nameof(TechnologyId))]
        public Technology? Technology { get; set; }

        // 🔑 Foreign Key: Операция из справочника
        [Required]
        [Column("operation_id")]
        public Guid OperationId { get; set; }

        [ForeignKey(nameof(OperationId))]
        public Operation? Operation { get; set; }

        // 🔑 Порядок выполнения в технологии
        [Required]
        [Column("sequence")]
        public int Sequence { get; set; }

        // 🔑 Переопределение времени наладки (если отличается от стандарта в operations)
        [Column("default_setup_minutes", TypeName = "numeric(10,2)")]
        public decimal? DefaultSetupMinutes { get; set; }

        // 🔑 Переопределение штучного времени (ОБРАТИТЕ ВНИМАНИЕ: без "_per_piece"!)
        [Column("default_unit_minutes", TypeName = "numeric(10,2)")]
        public decimal? DefaultUnitMinutes { get; set; }

        // 🔑 PERT: трёхточечная оценка длительности операции
        [Column("duration_opt", TypeName = "numeric(10,2)")]
        public decimal? DurationOptimistic { get; set; }

        [Column("duration_most_likely", TypeName = "numeric(10,2)")]
        public decimal? DurationMostLikely { get; set; }

        [Column("duration_pessimistic", TypeName = "numeric(10,2)")]
        public decimal? DurationPessimistic { get; set; }

        // 🔑 Вычисляемое свойство: эффективное время наладки (переопределение или стандарт)
        [NotMapped]
        public decimal EffectiveSetupMinutes =>
            DefaultSetupMinutes ?? Operation?.DefaultSetupMinutes ?? 0;

        // 🔑 Вычисляемое свойство: эффективное штучное время
        [NotMapped]
        public decimal EffectiveUnitMinutes =>
            DefaultUnitMinutes ?? Operation?.DefaultUnitMinutesPerPiece ?? 0;

        // 🔑 Вычисляемое свойство: ожидаемая длительность по PERT (формула: (O + 4M + P) / 6)
        [NotMapped]
        public decimal? ExpectedDuration =>
            DurationOptimistic.HasValue && DurationMostLikely.HasValue && DurationPessimistic.HasValue
                ? (DurationOptimistic.Value + 4 * DurationMostLikely.Value + DurationPessimistic.Value) / 6
                : (decimal?)null;
    }
}
