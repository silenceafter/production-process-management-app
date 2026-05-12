using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания
    public record CreateJobDto(
        [Required, MaxLength(50)] string Code,
        [Required, MaxLength(100)] string Name,
        [Range(1, 10)] int MinGrade = 1,
        [Range(0, 999999.99)] decimal HourlyRate = 0.00m,
        bool IsActive = true
    );

    // Для обновления
    public record UpdateJobDto(
        [MaxLength(50)] string? Code,
        [MaxLength(100)] string? Name,
        [Range(1, 10)] int? MinGrade,
        [Range(0, 999999.99)] decimal? HourlyRate,
        bool? IsActive
    );

    // Для ответа
    public record JobResponseDto(
        Guid Id,
        string Code,
        string Name,
        int MinGrade,
        decimal HourlyRate,
        bool IsActive
    );
}