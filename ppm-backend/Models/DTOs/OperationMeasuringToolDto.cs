using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    // Для создания связи (добавления инструмента к операции)
    public record CreateOperationMeasuringToolDto(
        [Required] Guid OperationId,
        [Required] Guid MeasuringToolId,
        bool? IsMandatory = true,
        int? Sequence = 0
    );

    // Для обновления (например, изменить порядок sequence)
    public record UpdateOperationMeasuringToolDto(
        bool? IsMandatory,
        int? Sequence
    );

    // Для ответа (с подробностями)
    public record OperationMeasuringToolResponseDto(
        Guid Id,
        Guid OperationId,
        Guid MeasuringToolId,
        string ToolCode,    // Подтягиваем из MeasuringTool
        string ToolName,    // Подтягиваем из MeasuringTool
        bool? IsMandatory,
        int? Sequence
    );
}
