using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Engineering.Products
{
    public class Material
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(50)] public string? Code { get; set; }
        [Required, MaxLength(10)] public string Unit { get; set; } = "шт";
        public bool IsActive { get; set; } = true;
    }
}
