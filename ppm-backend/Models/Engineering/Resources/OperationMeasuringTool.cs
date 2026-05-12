using PpmBackend.Models.Engineering.Products;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PpmBackend.Models.Engineering.Resources
{
    [Table("operation_measuring_tools", Schema = "engineering")]
    public class OperationMeasuringTool
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        [Column("operation_id")]
        public Guid OperationId { get; set; }

        // Навигация на операцию
        [ForeignKey(nameof(OperationId))]
        public Operation? Operation { get; set; }

        [Required]
        [Column("measuring_tool_id")]
        public Guid MeasuringToolId { get; set; }

        // Навигация на средство измерения
        [ForeignKey(nameof(MeasuringToolId))]
        public MeasuringTool? MeasuringTool { get; set; }

        [Column("is_mandatory")]
        public bool? IsMandatory { get; set; } = true; // По умолчанию true

        [Column("sequence")]
        public int? Sequence { get; set; } = 0; // Порядок использования
    }
}
