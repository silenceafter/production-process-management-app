using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания переопределения
    public record CreateProductTechnologyOverrideDto(
        [Required] Guid ProductId,
        [Required] Guid OperationId,
        [Required][Range(1, 9999)] int Sequence,
        [Range(0, 99999.99)] decimal? CustomSetupMinutes,
        [Range(0, 99999.99)] decimal? CustomUnitMinutes,
        Guid? CustomEquipmentId
    );

    // Для обновления
    public record UpdateProductTechnologyOverrideDto(
        [Range(1, 9999)] int? Sequence,
        [Range(0, 99999.99)] decimal? CustomSetupMinutes,
        [Range(0, 99999.99)] decimal? CustomUnitMinutes,
        Guid? CustomEquipmentId
    );

    // Для ответа (с подтянутыми данными)
    public record ProductTechnologyOverrideResponseDto(
        Guid Id,
        Guid ProductId,
        Guid OperationId,
        string OperationCode,
        string OperationName,
        int Sequence,
        decimal? CustomSetupMinutes,
        decimal? CustomUnitMinutes,
        Guid? CustomEquipmentId,
        string? CustomEquipmentName,
        // Стандартные значения для сравнения
        decimal? DefaultSetupMinutes,
        decimal? DefaultUnitMinutesPerPiece,
        // Итоговые эффективные значения
        decimal EffectiveSetupMinutes,
        decimal EffectiveUnitMinutes
    );
}
