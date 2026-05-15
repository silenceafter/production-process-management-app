using Microsoft.EntityFrameworkCore;
using PpmBackend.Data;
using PpmBackend.Interfaces;
using PpmBackend.Models;
using PpmBackend.Models.DTOs;
using PpmBackend.Models.Engineering.Products;
using PpmBackend.Models.Planning.Orders;
using PpmBackend.Models.Planning.Scheduling;
using System.Data;

namespace PpmBackend.Services.Planning
{
    public class WorkOrderService : IWorkOrderService
    {
        private readonly IWorkOrderRepository _repo;
        private readonly ILogger<WorkOrderService> _logger;

        public WorkOrderService(IWorkOrderRepository repo, ILogger<WorkOrderService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<List<WorkOrderResponseDto>> GetListAsync(WorkOrderFilterDto filter, CancellationToken ct = default)
        {
            _logger.LogDebug("Fetching work orders | Status: {Status}, Product: {ProductId}, Page: {Page}",
                filter.Status, filter.ProductId, filter.Page);

            return await _repo.GetAllAsync(filter, ct);
        }

        public async Task<WorkOrderResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _repo.GetByIdAsync(id, ct);
        }

        public async Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto dto, string userId, CancellationToken ct = default)
        {
            // 1. Бизнес-валидация: уникальность номера
            if (await _repo.ExistsByOrderNumberAsync(dto.OrderNumber, ct: ct))
                throw new InvalidOperationException($"Заказ с номером '{dto.OrderNumber}' уже существует.");

            _logger.LogInformation("Creating work order '{OrderNumber}' by user {UserId}", dto.OrderNumber, userId);
            return await _repo.CreateAsync(dto, userId, ct);
        }

        public async Task<WorkOrderResponseDto> UpdateAsync(Guid id, UpdateWorkOrderDto dto, CancellationToken ct = default)
        {
            // Если меняем номер → проверяем уникальность (исключая текущий заказ)
            if (!string.IsNullOrWhiteSpace(dto.OrderNumber))
            {
                if (await _repo.ExistsByOrderNumberAsync(dto.OrderNumber, excludeId: id, ct: ct))
                    throw new InvalidOperationException($"Заказ с номером '{dto.OrderNumber}' уже существует.");
            }

            return await _repo.UpdateAsync(id, dto, ct);
        }

        public async Task UpdateStatusAsync(Guid id, string newStatus, CancellationToken ct = default)
        {
            var current = await _repo.GetByIdAsync(id, ct);
            if (current == null)
                throw new KeyNotFoundException($"Заказ #{id} не найден.");

            // 🔒 Машина состояний
            bool isValid = (current.Status, newStatus) switch
            {
                ("Draft", "Released") => true,
                ("Draft", "Cancelled") => true,
                ("Released", "InProgress") => true,
                ("Released", "Cancelled") => true,
                ("InProgress", "Completed") => true,
                ("InProgress", "Cancelled") => true,
                _ => false
            };

            if (!isValid)
                throw new InvalidOperationException($"Недопустимый переход статуса: {current.Status} → {newStatus}");

            await _repo.UpdateStatusAsync(id, newStatus, ct);
            _logger.LogInformation("Status changed for order #{OrderId}: {Old} → {New}", id, current.Status, newStatus);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var exists = await _repo.GetByIdAsync(id, ct) != null;
            if (!exists) throw new KeyNotFoundException($"Заказ #{id} не найден.");

            await _repo.DeleteAsync(id, ct);
            _logger.LogWarning("Work order #{OrderId} deleted", id);
        }

        public Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto dto, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        Task<WorkOrderResponseDto> IWorkOrderService.UpdateStatusAsync(Guid id, string newStatus, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}