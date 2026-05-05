using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Dictionaries
{
    public class MeasuringTool
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)] public string Code { get; set; } = "";
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        public MeasuringToolType Type { get; set; } = MeasuringToolType.Caliper;
        public DateTime? LastCalibrationDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
