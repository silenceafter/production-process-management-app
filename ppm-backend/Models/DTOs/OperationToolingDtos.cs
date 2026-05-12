using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    public record CreateOperationToolingDto(
        [Required] Guid OperationId,
        [Required] Guid ToolingId,
        bool? IsPrimary = true,
        int? Sequence = 0,
        int? Quantity = 1
    );

    public record UpdateOperationToolingDto(
        bool? IsPrimary,
        int? Sequence,
        int? Quantity
    );

    public record OperationToolingResponseDto(
        Guid Id,
        Guid OperationId,
        Guid ToolingId,
        string? ToolingCode,
        string? ToolingName,
        bool? IsPrimary,
        int? Sequence,
        int? Quantity
    );
}
