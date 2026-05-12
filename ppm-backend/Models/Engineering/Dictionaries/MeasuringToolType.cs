using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Dictionaries
{
    [Table("measuring_tool_types", Schema = "engineering")]
    public class MeasuringToolType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(10)]
        [Column("code")]
        public string Code { get; set; } = "";

        [Required, MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = "";

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool? IsActive { get; set; } = true;
    }
}