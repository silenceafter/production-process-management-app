using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    public record CreateToolingDto(
        [Required, MaxLength(50)] string Code,
        [Required, MaxLength(100)] string Name,
        [MaxLength(255)] string? Description,
        [Required] int ToolingTypeId,
        bool IsActive = true
    );

    public record UpdateToolingDto(
        [MaxLength(50)] string? Code,
        [MaxLength(100)] string? Name,
        [MaxLength(255)] string? Description,
        int? ToolingTypeId,
        bool? IsActive
    );

    public record ToolingResponseDto(
        Guid Id,
        string Code,
        string Name,
        string? Description,
        int ToolingTypeId,
        string? ToolingTypeName,  // Подтягивается из ToolingType
        bool IsActive
    );
}
