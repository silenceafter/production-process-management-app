using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Dictionaries
{
    public class Product
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required, MaxLength(150)] public string Name { get; set; } = "";
        [Required, MaxLength(50)] public string DrawingNumber { get; set; } = "";
        [MaxLength(20)] public string? Revision { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<BomItem> BomItems { get; set; } = new List<BomItem>();
        public ICollection<Operation> Operations { get; set; } = new List<Operation>();
    }
}
