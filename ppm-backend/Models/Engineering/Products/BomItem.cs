using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Engineering.Products
{
    public class BomItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public BomItemTypeEnum ItemType { get; set; }
        public Guid ItemId { get; set; } // ID Material или Component

        [Range(0.0001, double.MaxValue)] public decimal QuantityPerUnit { get; set; } = 1;
        [Range(0, 100)] public decimal ScrapPercent { get; set; } = 0;
    }
}