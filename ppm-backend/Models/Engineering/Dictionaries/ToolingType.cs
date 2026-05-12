using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Dictionaries
{
    [Table("tooling_types", Schema = "engineering")]
    public class ToolingType
    {
        // 🔑 serial4 → int (автоинкремент)
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        [Column("name")]  // 🔑 Кавычки в DDL → явный маппинг
        public string Name { get; set; } = "";  // "Режущий инструмент", "Оснастка"

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        // 🔑 Nullable bool с дефолтом true (как в БД)
        [Column("is_active")]
        public bool? IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }  // Заполняется БД (now())
    }
}
