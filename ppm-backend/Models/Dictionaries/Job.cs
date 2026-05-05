using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Dictionaries
{
    public class Job
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)] public string Code { get; set; } = "";
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        public int MinGrade { get; set; } = 1;
        public decimal HourlyRate { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
