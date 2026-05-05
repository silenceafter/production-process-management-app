using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Dictionaries
{
    public class Tooling
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)] public string Code { get; set; } = "";
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        public ToolingType Type { get; set; } = ToolingType.Fixture;
        public int? LifespanHours { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
