using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания
    public record CreateProductDto(
        [Required, MaxLength(150)] string Name,
        [Required, MaxLength(50)] string DrawingNumber,
        [MaxLength(20)] string? BaseDesignation,
        [MaxLength(4)] string? ModificationCode,  // 🔑 4 символа!
        [MaxLength(20)] string? Revision,
        Guid? TechnologyId,
        bool IsActive = true
    );

    // Для обновления
    public record UpdateProductDto(
        [MaxLength(150)] string? Name,
        [MaxLength(50)] string? DrawingNumber,
        [MaxLength(20)] string? BaseDesignation,
        [MaxLength(4)] string? ModificationCode,
        [MaxLength(20)] string? Revision,
        Guid? TechnologyId,
        bool? IsActive
    );

    // Для ответа
    public record ProductResponseDto(
        Guid Id,
        string Name,
        string DrawingNumber,
        string? BaseDesignation,
        string? ModificationCode,
        string? Revision,
        bool IsActive,
        Guid? TechnologyId,
        string? TechnologyName,
        string FullDesignation  // Вычисляемое
    );
}
