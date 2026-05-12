using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Dictionaries
{
    [Table("equipment", Schema = "engineering")]
    public class Equipment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = "";

        [Required, MaxLength(100)]
        [Column("name")]
        public string Name { get; set; } = "";

        [MaxLength(20)]
        [Column("shop_number")]
        public string? ShopNumber { get; set; }

        [MaxLength(50)]
        [Column("area_number")]
        public string? AreaNumber { get; set; }

        [Column(TypeName = "numeric(5,2)")]
        public decimal CapacityHoursPerShift { get; set; } = 8.00m;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
