using PpmBackend.Models.Engineering.Dictionaries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Resources
{
    [Table("tooling", Schema = "engineering")]
    public class Tooling
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = "";  // "РТ-001", "ОФ-002"

        [Required, MaxLength(100)]
        [Column("name")]  // 🔑 "name" в кавычках в DDL → явный маппинг
        public string Name { get; set; } = "";  // "Резец токарный проходной"

        [MaxLength(255)]
        [Column("description")]
        public string? Description { get; set; }  // 🔑 ДОБАВЛЕНО

        // 🔑 ЗАМЕНА ENUM НА ВНЕШНИЙ КЛЮЧ (как в БД)
        [Required]
        [Column("tooling_type_id")]
        public int ToolingTypeId { get; set; }

        [ForeignKey(nameof(ToolingTypeId))]
        public ToolingType Type { get; set; } = null!; // Навигация на справочник типов

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
