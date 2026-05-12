using PpmBackend.Models.DTOs;

namespace PpmBackend.Services.Planning
{
    public interface IWorkOrderService
    {
        Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto dto, CancellationToken ct = default);
        Task<WorkOrderResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<List<WorkOrderResponseDto>> GetListAsync(WorkOrderFilterDto filter, CancellationToken ct = default);
        Task<WorkOrderResponseDto> UpdateAsync(Guid id, UpdateWorkOrderDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
        Task<WorkOrderResponseDto> UpdateStatusAsync(Guid id, string newStatus, CancellationToken ct = default);
    }
}
