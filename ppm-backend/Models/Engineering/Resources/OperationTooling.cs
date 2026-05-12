using PpmBackend.Models.Engineering.Products;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Resources
{
    [Table("operation_tooling", Schema = "engineering")]
    public class OperationTooling
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Column("operation_id")]
        public Guid OperationId { get; set; }

        [ForeignKey(nameof(OperationId))]
        public Operation? Operation { get; set; }

        [Required]
        [Column("tooling_id")]
        public Guid ToolingId { get; set; }

        [ForeignKey(nameof(ToolingId))]
        public Tooling? Tooling { get; set; }

        [Column("is_primary")]
        public bool? IsPrimary { get; set; } = true; // По умолчанию true

        [Column("sequence")]
        public int? Sequence { get; set; } = 0; // Порядок использования

        [Column("quantity")]
        public int? Quantity { get; set; } = 1; // Требуемое количество
    }
}