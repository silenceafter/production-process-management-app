using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Dictionaries
{
    [Table("jobs", Schema = "engineering")]
    public class Job
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

        [Column("min_grade")]
        public int MinGrade { get; set; } = 1;

        [Column("hourly_rate", TypeName = "numeric(10,2)")]
        public decimal HourlyRate { get; set; } = 0.00m;

        [Column("is_active")]
        public bool IsActive { get; set; } = true;
    }
}
