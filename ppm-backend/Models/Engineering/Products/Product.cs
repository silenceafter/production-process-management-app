using PpmBackend.Models.Engineering.Technologies;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Products
{
    [Table("products", Schema = "engineering")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, MaxLength(150)]
        [Column("name")]  // 🔑 "name" — зарезервированное слово
        public string Name { get; set; } = "";

        // 🔑 ИСПРАВЛЕНО: nullable + MaxLength(20) как в БД
        [MaxLength(20)]
        [Column("base_designation")]
        public string? BaseDesignation { get; set; }  // "БЦЖИ.656123.001"

        // 🔑 ИСПРАВЛЕНО: varchar(4) в БД, не 20!
        [MaxLength(4)]
        [Column("modification_code")]
        public string? ModificationCode { get; set; }  // "ПЗ", "У1", "ХЛ1"

        // 🔑 ИСПРАВЛЕНО: MaxLength(50) как в БД
        [Required, MaxLength(50)]
        [Column("drawing_number")]
        public string DrawingNumber { get; set; } = "";  // "БЦЖИ.656123.001 ПЗ"

        [MaxLength(20)]
        [Column("revision")]
        public string? Revision { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        // 🔑 ДОБАВЛЕНО: связь с технологией (шаблоном)
        [Column("technology_id")]
        public Guid? TechnologyId { get; set; }

        [ForeignKey(nameof(TechnologyId))]
        public Technology? Technology { get; set; }

        // 🔑 Навигации (исправленные)
        public ICollection<BomItem> BomItems { get; set; } = new List<BomItem>();

        // 🔑 Через связующую таблицу product_operations (не напрямую!)
        public ICollection<ProductOperation> ProductOperations { get; set; } = new List<ProductOperation>();

        // 🔑 Через переопределения технологии
        public ICollection<ProductTechnologyOverride> TechnologyOverrides { get; set; } = new List<ProductTechnologyOverride>();

        // 🔑 Вычисляемое свойство (полное обозначение)
        [NotMapped]
        public string FullDesignation =>
            !string.IsNullOrEmpty(ModificationCode)
                ? $"{BaseDesignation} {ModificationCode}"
                : BaseDesignation ?? DrawingNumber;
    }
}
