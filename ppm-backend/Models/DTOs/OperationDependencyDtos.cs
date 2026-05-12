using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    /*public record CreateOperationDependencyDto(
        [Required] Guid PredecessorId,
        [Required] Guid SuccessorId,
        DependencyType DependencyType = DependencyType.FinishToStart,
        [Range(0, 99999)] int LagMinutes = 0
    );

    public record UpdateOperationDependencyDto(
        DependencyType? DependencyType,
        [Range(0, 99999)] int? LagMinutes
    );

    public record OperationDependencyResponseDto(
        Guid Id,
        Guid PredecessorId,
        string PredecessorCode,  // Из связанной операции
        Guid SuccessorId,
        string SuccessorCode,
        string DependencyType,   // Строковое представление для фронтенда
        int LagMinutes
    );*/
}
