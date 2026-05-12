using PpmBackend.Models.Engineering.Dictionaries;
using PpmBackend.Models.Engineering.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Technologies
{
    [Table("product_technology_override", Schema = "engineering")]
    public class ProductTechnologyOverride
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

        // 🔑 Порядок выполнения (должен совпадать с последовательностью в технологии)
        [Required]
        [Column("sequence")]
        public int Sequence { get; set; }

        // 🔑 Переопределение времени наладки (если отличается от шаблона)
        [Column("custom_setup_minutes", TypeName = "numeric(10,2)")]
        public decimal? CustomSetupMinutes { get; set; }

        // 🔑 Переопределение штучного времени (ОБРАТИТЕ ВНИМАНИЕ: поле называется custom_unit_minutes, а не custom_unit_minutes_per_piece)
        [Column("custom_unit_minutes", TypeName = "numeric(10,2)")]
        public decimal? CustomUnitMinutes { get; set; }

        // 🔑 Переопределение оборудования
        [Column("custom_equipment_id")]
        public Guid? CustomEquipmentId { get; set; }

        [ForeignKey(nameof(CustomEquipmentId))]
        public Equipment? CustomEquipment { get; set; } = null!;

        // 🔑 Вычисляемые свойства для удобного использования
        [NotMapped]
        public decimal EffectiveSetupMinutes =>
            CustomSetupMinutes ?? Operation?.DefaultSetupMinutes ?? 0;

        [NotMapped]
        public decimal EffectiveUnitMinutes =>
            CustomUnitMinutes ?? Operation?.DefaultUnitMinutesPerPiece ?? 0;
    }
}
