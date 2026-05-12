using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания
    public record CreateTechnologyDto(
        [Required, MaxLength(20)] string Code,
        [Required, MaxLength(150)] string Name,
        [MaxLength(1000)] string? Description,
        bool? IsActive = true
    // CreatedAt заполняется БД автоматически
    );

    // Для обновления
    public record UpdateTechnologyDto(
        [MaxLength(20)] string? Code,
        [MaxLength(150)] string? Name,
        [MaxLength(1000)] string? Description,
        bool? IsActive
    );

    // Для ответа
    public record TechnologyResponseDto(
        Guid Id,
        string Code,
        string Name,
        string? Description,
        bool? IsActive,
        DateTime? CreatedAt,
        int OperationsCount  // Количество операций в технологии (опционально)
    );
}
