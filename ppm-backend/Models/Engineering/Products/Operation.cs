using PpmBackend.Models.Engineering.Dictionaries;
using PpmBackend.Models.Engineering.Resources;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Products
{
    [Table("operations", Schema = "engineering")]
    public class Operation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required, MaxLength(50)]
        [Column("code")]
        public string Code { get; set; } = "";  // "005", "010"

        [Required, MaxLength(100)]
        [Column("name")] 
        public string Name { get; set; } = "";  // "Токарная черновая"

        [MaxLength(100)]
        [Column("document")]
        public string? Document { get; set; }  // "МК-БЦЖИ.656123.001"

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("default_equipment_id")]
        public Guid? DefaultEquipmentId { get; set; }

        [ForeignKey(nameof(DefaultEquipmentId))]
        public Equipment DefaultEquipment { get; set; } = null!;

        [Column("default_tooling_id")]
        public Guid? DefaultToolingId { get; set; }

        [ForeignKey(nameof(DefaultToolingId))]
        public Tooling DefaultTooling { get; set; }

        [Column("default_job_id")]
        public Guid? DefaultJobId { get; set; }

        [ForeignKey(nameof(DefaultJobId))]
        public Job DefaultJob { get; set; }

        [Column("default_grade")]
        public int? DefaultGrade { get; set; }

        [Column("default_setup_minutes", TypeName = "numeric(10,2)")]
        public decimal? DefaultSetupMinutes { get; set; }

        [Column("default_unit_minutes_per_piece", TypeName = "numeric(10,2)")]
        public decimal? DefaultUnitMinutesPerPiece { get; set; }

        [Column("default_measuring_tool_id")]
        public Guid? DefaultMeasuringToolId { get; set; }

        [ForeignKey(nameof(DefaultMeasuringToolId))]
        public MeasuringTool DefaultMeasuringTool { get; set; }
    }
}
