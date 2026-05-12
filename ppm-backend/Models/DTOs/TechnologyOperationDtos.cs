using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания связи (добавление операции в технологию)
    public record CreateTechnologyOperationDto(
        [Required] Guid TechnologyId,
        [Required] Guid OperationId,
        [Required][Range(1, 9999)] int Sequence,
        [Range(0, 99999.99)] decimal? DefaultSetupMinutes,
        [Range(0, 99999.99)] decimal? DefaultUnitMinutes,
        [Range(0, 99999.99)] decimal? DurationOptimistic,
        [Range(0, 99999.99)] decimal? DurationMostLikely,
        [Range(0, 99999.99)] decimal? DurationPessimistic
    );

    // Для обновления параметров
    public record UpdateTechnologyOperationDto(
        [Range(1, 9999)] int? Sequence,
        [Range(0, 99999.99)] decimal? DefaultSetupMinutes,
        [Range(0, 99999.99)] decimal? DefaultUnitMinutes,
        [Range(0, 99999.99)] decimal? DurationOptimistic,
        [Range(0, 99999.99)] decimal? DurationMostLikely,
        [Range(0, 99999.99)] decimal? DurationPessimistic
    );

    // Для ответа (с подтянутыми данными)
    public record TechnologyOperationResponseDto(
        Guid Id,
        Guid TechnologyId,
        Guid OperationId,
        string OperationCode,
        string OperationName,
        int Sequence,
        decimal? DefaultSetupMinutes,
        decimal? DefaultUnitMinutes,
        decimal? DurationOptimistic,
        decimal? DurationMostLikely,
        decimal? DurationPessimistic,
        // Стандартные значения из справочника для сравнения
        decimal? StdSetupMinutes,
        decimal? StdUnitMinutes,
        // Итоговые эффективные значения
        decimal EffectiveSetupMinutes,
        decimal EffectiveUnitMinutes,
        // Ожидаемая длительность по PERT
        decimal? ExpectedDuration
    );
}
