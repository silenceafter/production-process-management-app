using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания
    public record CreateEquipmentDto(
        [Required, MaxLength(50)] string Code,
        [Required, MaxLength(100)] string Name,
        [MaxLength(20)] string? ShopNumber,
        [MaxLength(50)] string? AreaNumber,
        [Range(0.01, 999.99)] decimal CapacityHoursPerShift = 8.00m,
        bool IsActive = true
    );

    // Для обновления (все поля опциональны)
    public record UpdateEquipmentDto(
        [MaxLength(50)] string? Code,
        [MaxLength(100)] string? Name,
        [MaxLength(20)] string? ShopNumber,
        [MaxLength(50)] string? AreaNumber,
        [Range(0.01, 999.99)] decimal? CapacityHoursPerShift,
        bool? IsActive
    );

    // Для ответа (полный объект)
    public record EquipmentResponseDto(
        Guid Id,
        string Code,
        string Name,
        string? ShopNumber,
        string? AreaNumber,
        decimal CapacityHoursPerShift,
        bool IsActive
    );
}