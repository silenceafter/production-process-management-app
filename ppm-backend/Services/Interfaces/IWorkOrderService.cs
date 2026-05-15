using PpmBackend.Models.DTOs;

namespace PpmBackend.Services.Interfaces
{
    public interface IWorkOrderService
    {
        Task<List<WorkOrderResponseDto>> GetListAsync(WorkOrderFilterDto filter, CancellationToken ct = default);
        Task<WorkOrderResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto dto, string userId, CancellationToken ct = default);
        Task<WorkOrderResponseDto> UpdateAsync(Guid id, UpdateWorkOrderDto dto, CancellationToken ct = default);
        Task UpdateStatusAsync(Guid id, string newStatus, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
