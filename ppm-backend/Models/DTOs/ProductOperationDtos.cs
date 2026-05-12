using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания связи (добавление операции в маршрут изделия)
    public record CreateProductOperationDto(
        [Required] Guid ProductId,
        [Required] Guid OperationId,
        [Required][Range(1, 9999)] int Sequence,
        Guid? CustomEquipmentId,
        [Range(0, 99999.99)] decimal? CustomSetupMinutes,
        [Range(0, 99999.99)] decimal? CustomUnitMinutesPerPiece
    );

    // Для обновления параметров связи
    public record UpdateProductOperationDto(
        [Range(1, 9999)] int? Sequence,
        Guid? CustomEquipmentId,
        [Range(0, 99999.99)] decimal? CustomSetupMinutes,
        [Range(0, 99999.99)] decimal? CustomUnitMinutesPerPiece
    );

    // Для ответа (с подтянутыми данными из связанных таблиц)
    public record ProductOperationResponseDto(
        Guid Id,
        Guid ProductId,
        Guid OperationId,
        string OperationCode,
        string OperationName,
        int Sequence,
        Guid? CustomEquipmentId,
        string? CustomEquipmentName,
        decimal? CustomSetupMinutes,
        decimal? CustomUnitMinutesPerPiece,
        // Стандартные значения из справочника (для сравнения)
        decimal? DefaultSetupMinutes,
        decimal? DefaultUnitMinutesPerPiece,
        // Итоговые эффективные значения
        decimal EffectiveSetupMinutes,
        decimal EffectiveUnitMinutes
    );
}
