using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    public record CreateToolingTypeDto(
        [Required, MaxLength(100)] string Name,
        [MaxLength(500)] string? Description,
        bool? IsActive = true
    );

    public record UpdateToolingTypeDto(
        [MaxLength(100)] string? Name,
        [MaxLength(500)] string? Description,
        bool? IsActive
    );

    public record ToolingTypeResponseDto(
        int Id,
        string Name,
        string? Description,
        bool? IsActive,
        DateTime? CreatedAt,
        int ToolingsCount = 0  // Опционально: сколько оснастки этого типа
    );
}
