using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Engineering.Products
{
    public class Component
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(100)] public string Name { get; set; } = "";
        [MaxLength(50)] public string? Code { get; set; }
        [MaxLength(100)] public string? Supplier { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
