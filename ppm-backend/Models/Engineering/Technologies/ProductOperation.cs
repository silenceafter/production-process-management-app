using PpmBackend.Models.Engineering.Dictionaries;
using PpmBackend.Models.Engineering.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Technologies
{
    [Table("product_operations", Schema = "engineering")]
    public class ProductOperation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        // 🔑 Foreign Key: Продукт
        [Required]
        [Column("product_id")]
        public Guid ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public Product? Product { get; set; }

        // 🔑 Foreign Key: Операция из справочника
        [Required]
        [Column("operation_id")]
        public Guid OperationId { get; set; }

        [ForeignKey(nameof(OperationId))]
        public Operation? Operation { get; set; }

        // 🔑 Порядок выполнения в маршруте
        [Required]
        [Column("sequence")]
        public int Sequence { get; set; }

        // 🔑 Переопределение оборудования (если отличается от стандарта)
        [Column("custom_equipment_id")]
        public Guid? CustomEquipmentId { get; set; }

        [ForeignKey(nameof(CustomEquipmentId))]
        public Equipment? CustomEquipment { get; set; } = null!;

        // 🔑 Переопределение времени наладки
        [Column("custom_setup_minutes", TypeName = "numeric(10,2)")]
        public decimal? CustomSetupMinutes { get; set; }

        // 🔑 Переопределение штучного времени
        [Column("custom_unit_minutes_per_piece", TypeName = "numeric(10,2)")]
        public decimal? CustomUnitMinutesPerPiece { get; set; }

        // 🔑 Вычисляемое свойство: итоговое время наладки (кастомное или стандартное)
        [NotMapped]
        public decimal EffectiveSetupMinutes =>
            CustomSetupMinutes ?? Operation?.DefaultSetupMinutes ?? 0;

        // 🔑 Вычисляемое свойство: итоговое штучное время
        [NotMapped]
        public decimal EffectiveUnitMinutes =>
            CustomUnitMinutesPerPiece ?? Operation?.DefaultUnitMinutesPerPiece ?? 0;
    }
}
