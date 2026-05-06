using Microsoft.EntityFrameworkCore;
using PpmBackend.Data;
using PpmBackend.Models;
using PpmBackend.Models.Dictionaries;
using PpmBackend.Models.DTOs;

namespace PpmBackend.Services
{
    public class WorkOrderService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<WorkOrderService> _logger;

        public WorkOrderService(ApplicationDbContext db, ILogger<WorkOrderService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<WorkOrder> CreateAsync(string userId, CreateWorkOrderDto dto)
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // 1. Создаём заказ
                var order = new WorkOrder
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    PlannedStart = dto.PlannedStart,
                    DueDate = dto.DueDate,
                    CreatedById = userId,
                    Status = OrderStatus.Draft
                };
                _db.WorkOrders.Add(order);
                await _db.SaveChangesAsync();

                // 2. Клонируем операции из шаблона (engineering) в план (planning)
                var templateOps = await _db.Operations
                    .AsNoTracking()
                    .Where(o => o.ProductId == dto.ProductId)
                    .OrderBy(o => o.Sequence)
                    .Select(o => new { o.Id, o.Sequence, o.Code }) // Только нужные поля
                    .ToListAsync();

                var planOps = templateOps.Select(t => new OrderOperation
                {
                    WorkOrderId = order.Id,
                    OperationId = t.Id, // ссылка на шаблон
                    Sequence = t.Sequence,
                    OperationCode = t.Code,
                    Status = OperationStatus.Pending
                }).ToList();

                _db.OrderOperations.AddRange(planOps);
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
                return order;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }
    }
}
