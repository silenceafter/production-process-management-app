using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Dictionaries
{
    public class Equipment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [MaxLength(50)] public string Code { get; set; } = "";
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(20)] public string? ShopNumber { get; set; }
        [MaxLength(20)] public string? AreaNumber { get; set; }
        public decimal CapacityHoursPerShift { get; set; } = 8;
        public bool IsActive { get; set; } = true;
    }
}
