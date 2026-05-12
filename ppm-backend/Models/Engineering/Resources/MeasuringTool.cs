using PpmBackend.Models.Engineering.Dictionaries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Resources
{
    [Table("measuring_tools", Schema = "engineering")]
    public class MeasuringTool
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

        [Column("type_id")]
        public int? TypeId { get; set; }

        [ForeignKey(nameof(TypeId))]
        public MeasuringToolType Type { get; set; } = null;

        [Column("calibration_date")]
        public DateTime? CalibrationDate { get; set; }

        [Column("next_calibration_date")]
        public DateTime? NextCalibrationDate { get; set; }

        [MaxLength(50)]
        [Column("serial_number")]
        public string? SerialNumber { get; set; }

        [MaxLength(50)]
        [Column("measurement_range")]
        public string? MeasurementRange { get; set; }

        [MaxLength(50)]
        [Column("accuracy")]
        public string? Accuracy { get; set; }

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
