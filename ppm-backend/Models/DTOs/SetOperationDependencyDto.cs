namespace PpmBackend.Models.DTOs
{
    public record SetOperationDependencyDto(
        Guid PredecessorId,
        Guid SuccessorId,
        DependencyType Type = DependencyType.FinishToStart,
        double LagHours = 0);
}
