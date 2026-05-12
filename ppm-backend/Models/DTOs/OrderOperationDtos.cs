using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    /*public record CreateOrderOperationDto(
        [Required] Guid WorkOrderId,
        [Required] Guid OperationId,
        [MaxLength(50)] string? OperationCode,
        int Sequence = 0,
        [Range(0, 99999.99)] decimal? SetupMinutes,
        [Range(0, 99999.99)] decimal? UnitMinutesPerPiece,
        Guid? AssignedEquipmentId,
        string Status = "Pending"
    );

    public record UpdateOrderOperationDto(
        [MaxLength(50)] string? OperationCode,
        int? Sequence,
        [Range(0, 99999.99)] decimal? SetupMinutes,
        [Range(0, 99999.99)] decimal? UnitMinutesPerPiece,
        DateTimeOffset? ScheduledStart,
        DateTimeOffset? ScheduledEnd,
        DateTimeOffset? ActualStart,
        DateTimeOffset? ActualEnd,
        string? Status,
        Guid? AssignedEquipmentId,
        int? QtyCompleted,
        // PERT
        [Range(0, 99999.99)] decimal? DurationOptimistic,
        [Range(0, 99999.99)] decimal? DurationMostLikely,
        [Range(0, 99999.99)] decimal? DurationPessimistic,
        double? EarliestStart,
        double? LatestStart,
        bool? IsCriticalPath
    );

    public record OrderOperationResponseDto(
        Guid Id,
        Guid WorkOrderId,
        Guid OperationId,
        string? OperationCode,
        string? OperationName,  // Из связанной операции
        int Sequence,
        decimal? SetupMinutes,
        decimal? UnitMinutesPerPiece,
        decimal? TotalDuration,  // Вычисленное: Setup + Unit*Qty
        DateTimeOffset? ScheduledStart,
        DateTimeOffset? ScheduledEnd,
        DateTimeOffset? ActualStart,
        DateTimeOffset? ActualEnd,
        string Status,
        Guid? AssignedEquipmentId,
        string? EquipmentName,
        int QtyCompleted,
        // PERT
        decimal? DurationOptimistic,
        decimal? DurationMostLikely,
        decimal? DurationPessimistic,
        decimal? DurationExpected,
        decimal? Variance,
        double? EarliestStart,
        double? LatestStart,
        bool IsCriticalPath
    );*/
}
