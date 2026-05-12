using PpmBackend.Models.Engineering.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Technologies
{
    [Table("technologies", Schema = "engineering")]
    public class Technology
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, MaxLength(20)]
        [Column("code")]
        public string Code { get; set; } = "";  // "ТП-ЭС", "ТП-Г", "ТП-ПЧ"

        [Required, MaxLength(150)]
        [Column("name")]  // 🔑 "name" — зарезервированное слово в PostgreSQL
        public string Name { get; set; } = "";  // "Электростанции бензиновые"

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; } = true;  // Nullable bool с дефолтом

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }  // Заполняется БД автоматически

        // 🔑 Навигации
        public ICollection<TechnologyOperation> TechnologyOperations { get; set; } = new List<TechnologyOperation>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
