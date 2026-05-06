namespace PpmBackend.Models.DTOs
{
    public record CreateWorkOrderDto(
        Guid ProductId,
        int Quantity,
        DateTimeOffset PlannedStart,
        DateTimeOffset? DueDate);
}
