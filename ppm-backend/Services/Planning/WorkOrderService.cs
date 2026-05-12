using Microsoft.EntityFrameworkCore;
using PpmBackend.Data;
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
        private readonly ApplicationDbContext _db;

        public WorkOrderService(ApplicationDbContext db) => _db = db;

        public async Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto dto, CancellationToken ct = default)
        {
            // 🔑 Проверка уникальности номера
            if (await _db.WorkOrders.AnyAsync(wo => wo.OrderNumber == dto.OrderNumber, ct))
                throw new InvalidOperationException($"Заказ с номером '{dto.OrderNumber}' уже существует");

            // 🔑 Проверка существования продукта
            var product = await _db.Products.FindAsync(new object[] { dto.ProductId }, ct);
            if (product == null)
                throw new InvalidOperationException($"Продукт с ID {dto.ProductId} не найден");

            var order = new WorkOrder
            {
                OrderNumber = dto.OrderNumber,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                PlannedStart = dto.PlannedStart,
                DueDate = dto.DueDate,
                Status = dto.Status,
                Notes = dto.Notes,
                CreatedById = dto.CreatedById,
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.WorkOrders.Add(order);
            await _db.SaveChangesAsync(ct);

            return await MapToResponseAsync(order, ct);
        }

        public async Task<WorkOrderResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var order = await _db.WorkOrders
                .Include(o => o.Product)
                .Include(o => o.CreatedBy)
                .FirstOrDefaultAsync(o => o.Id == id, ct);

            return order == null ? null : await MapToResponseAsync(order, ct);
        }

        public async Task<List<WorkOrderResponseDto>> GetListAsync(WorkOrderFilterDto filter, CancellationToken ct = default)
        {
            var query = _db.WorkOrders
                .Include(o => o.Product)
                .Include(o => o.CreatedBy)
                .AsNoTracking()
                .AsQueryable();

            // 🔍 Применение фильтров
            if (!string.IsNullOrWhiteSpace(filter.Status))
                query = query.Where(w => w.Status == filter.Status);

            if (filter.ProductId.HasValue)
                query = query.Where(w => w.ProductId == filter.ProductId.Value);

            if (filter.StartDate.HasValue)
                query = query.Where(w => w.PlannedStart.Date >= filter.StartDate.Value.Date);

            if (filter.EndDate.HasValue)
                query = query.Where(w => w.PlannedStart.Date <= filter.EndDate.Value.AddDays(1).Date);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                query = query.Where(w =>
                    w.OrderNumber.ToLower().Contains(search) ||
                    (w.Product != null && w.Product.Name.ToLower().Contains(search)) ||
                    (w.Product != null && w.Product.DrawingNumber.ToLower().Contains(search)));
            }

            // 🔀 Сортировка
            query = filter.SortBy?.ToLower() switch
            {
                "plannedstart" => filter.SortDescending
                    ? query.OrderByDescending(w => w.PlannedStart)
                    : query.OrderBy(w => w.PlannedStart),

                "ordernumber" => filter.SortDescending
                    ? query.OrderByDescending(w => w.OrderNumber)
                    : query.OrderBy(w => w.OrderNumber),

                "status" => filter.SortDescending
                    ? query.OrderByDescending(w => w.Status)
                    : query.OrderBy(w => w.Status),

                _ => filter.SortDescending
                    ? query.OrderByDescending(w => w.CreatedAt)
                    : query.OrderBy(w => w.CreatedAt)
            };
            try
            {
                // 📄 Пагинация
                var items = await query
                    .Skip(filter.SkipCount)
                    .Take(filter.ClampedPageSize)
                    .ToListAsync(ct);

                var result = new List<WorkOrderResponseDto>(items.Count);
                foreach (var order in items)
                    result.Add(await MapToResponseAsync(order, ct));
                return result;
            } catch(Exception ex)
            {
                var t = 5;
                return new List<WorkOrderResponseDto>();
            }
            
        }

        public async Task<WorkOrderResponseDto> UpdateAsync(Guid id, UpdateWorkOrderDto dto, CancellationToken ct = default)
        {
            var order = await _db.WorkOrders.FindAsync(new object[] { id }, ct);
            if (order == null)
                throw new InvalidOperationException($"Заказ с ID {id} не найден");

            // 🔑 Проверка уникальности номера (если меняем)
            if (!string.IsNullOrWhiteSpace(dto.OrderNumber) && dto.OrderNumber != order.OrderNumber)
            {
                if (await _db.WorkOrders.AnyAsync(wo => wo.OrderNumber == dto.OrderNumber && wo.Id != id, ct))
                    throw new InvalidOperationException($"Заказ с номером '{dto.OrderNumber}' уже существует");

                order.OrderNumber = dto.OrderNumber;
            }

            if (dto.ProductId.HasValue)
            {
                var product = await _db.Products.FindAsync(new object[] { dto.ProductId.Value }, ct);
                if (product == null)
                    throw new InvalidOperationException($"Продукт с ID {dto.ProductId} не найден");

                order.ProductId = dto.ProductId.Value;
            }

            if (dto.Quantity.HasValue) order.Quantity = dto.Quantity.Value;
            if (dto.PlannedStart.HasValue) order.PlannedStart = dto.PlannedStart.Value;
            if (dto.DueDate.HasValue) order.DueDate = dto.DueDate.Value;
            if (!string.IsNullOrWhiteSpace(dto.Status)) order.Status = dto.Status;
            if (dto.Notes != null) order.Notes = dto.Notes;
            if (dto.UpdatedAt.HasValue) order.UpdatedAt = dto.UpdatedAt.Value;
            else order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);

            return await MapToResponseAsync(order, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var order = await _db.WorkOrders.FindAsync(new object[] { id }, ct);
            if (order == null)
                throw new InvalidOperationException($"Заказ с ID {id} не найден");

            // 🔑 Мягкое удаление через статус
            order.Status = "Cancelled";
            order.UpdatedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);
        }

        public async Task<WorkOrderResponseDto> UpdateStatusAsync(Guid id, string newStatus, CancellationToken ct = default)
        {
            var order = await _db.WorkOrders.FindAsync(new object[] { id }, ct);
            if (order == null)
                throw new InvalidOperationException($"Заказ с ID {id} не найден");

            // 🔑 Машина состояний
            var allowedTransitions = new Dictionary<string, string[]>
            {
                ["Draft"] = new[] { "Released", "Cancelled" },
                ["Released"] = new[] { "InProgress", "Cancelled" },
                ["InProgress"] = new[] { "Completed", "Cancelled" },
                ["Completed"] = Array.Empty<string>(),
                ["Cancelled"] = Array.Empty<string>()
            };

            if (!allowedTransitions.TryGetValue(order.Status, out var allowed) || !allowed.Contains(newStatus))
                throw new InvalidOperationException(
                    $"Недопустимый переход статуса: {order.Status} → {newStatus}. " +
                    $"Разрешены: {string.Join(", ", allowed)}");

            order.Status = newStatus;
            order.UpdatedAt = DateTimeOffset.UtcNow;

            // Если завершён — фиксируем дату окончания
            if (newStatus == "Completed" && !order.DueDate.HasValue)
                order.DueDate = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);

            return await MapToResponseAsync(order, ct);
        }

        // 🔹 Приватный метод маппинга
        private async Task<WorkOrderResponseDto> MapToResponseAsync(WorkOrder order, CancellationToken ct)
        {
            // Подгружаем операции для расчёта прогресса
            var operations = await _db.OrderOperations
                .Where(o => o.WorkOrderId == order.Id)
                .ToListAsync(ct);

            var completed = operations.Count(o => o.Status == "Completed");

            return new WorkOrderResponseDto(
                Id: order.Id,
                OrderNumber: order.OrderNumber,
                ProductId: order.ProductId,
                ProductName: order.Product?.Name ?? "Unknown",
                ProductDrawingNumber: order.Product?.DrawingNumber ?? "",
                Quantity: order.Quantity,
                PlannedStart: order.PlannedStart,
                DueDate: order.DueDate,
                Status: order.Status,
                Notes: order.Notes,
                CreatedById: order.CreatedById,
                CreatedByName: order.CreatedBy?.UserName,
                CreatedAt: order.CreatedAt,
                UpdatedAt: order.UpdatedAt,
                CompletionPercent: operations.Count > 0 ? 100.0 * completed / operations.Count : 0,
                TotalOperations: operations.Count,
                CompletedOperations: completed
            );
        }
    }
}
