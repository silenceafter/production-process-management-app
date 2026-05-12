using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания
    public record CreateMeasuringToolTypeDto(
        [Required, MaxLength(10)] string Code,
        [Required, MaxLength(100)] string Name,
        [MaxLength(500)] string? Description,
        bool? IsActive = true
    );

    // Для обновления
    public record UpdateMeasuringToolTypeDto(
        [MaxLength(10)] string? Code,
        [MaxLength(100)] string? Name,
        [MaxLength(500)] string? Description,
        bool? IsActive
    );

    // Для ответа
    public record MeasuringToolTypeResponseDto(
        int Id,
        string Code,
        string Name,
        string? Description,
        bool? IsActive
    );
}