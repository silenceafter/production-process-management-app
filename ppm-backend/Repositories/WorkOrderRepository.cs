using Dapper;
using Npgsql;
using PpmBackend.Interfaces;
using PpmBackend.Models.DTOs;

namespace PpmBackend.Repositories
{
    public class WorkOrderDapperRepository : IWorkOrderRepository
    {
        private readonly IConfiguration _config;

        public WorkOrderDapperRepository(IConfiguration config)
        {
            _config = config;
        }

        //  Вспомогательный метод: создаёт и возвращает соединение
        private NpgsqlConnection GetConnection() =>
            new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        public async Task<List<WorkOrderResponseDto>> GetAllAsync(WorkOrderFilterDto filter, CancellationToken ct = default)
        {
            await using var conn = GetConnection();
            var sql = @"
                SELECT 
                    wo.id AS Id,                    
                    wo.order_number AS OrderNumber, 
                    wo.product_id AS ProductId,     
                    p.name AS ProductName,
                    p.drawing_number AS ProductDrawingNumber,
                    wo.quantity AS Quantity,
                    wo.planned_start AS PlannedStart,
                    wo.due_date AS DueDate,
                    wo.status AS Status,
                    COALESCE(wo.notes, '') AS Notes,
                    wo.created_by_id AS CreatedById,
                    COALESCE(u.""UserName"", '') AS CreatedByName,
                    wo.created_at AS CreatedAt,
                    wo.updated_at AS UpdatedAt,
                    0 AS TotalOperations, 
                    0 AS CompletedOperations, 
                    0.0 AS CompletionPercent
                FROM planning.work_orders wo
                LEFT JOIN engineering.products p ON wo.product_id = p.id
                LEFT JOIN identity.""AspNetUsers"" u ON wo.created_by_id = u.""Id""
                WHERE (@Status IS NULL OR wo.status = @Status)
                  AND (@ProductId IS NULL OR wo.product_id = @ProductId)
                ORDER BY wo.created_at DESC
                LIMIT @PageSize OFFSET @Offset";

            try
            {
                var result = await conn.QueryAsync<WorkOrderResponseDto>(sql, new
                {
                    filter.Status,
                    filter.ProductId,
                    PageSize = filter.ClampedPageSize,
                    Offset = filter.SkipCount
                });
                return result.ToList();
            }
            catch (Exception ex)
            {
                // 🔥 Логируем полную ошибку для отладки
                Console.WriteLine($"❌ SQL Error: {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner: {ex.InnerException.Message}");
                throw;
            }
        }

        public async Task<WorkOrderResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            await using var conn = GetConnection();

            var sql = @"
                SELECT 
                    wo.id, wo.order_number, wo.product_id,
                    p.name AS ProductName,
                    p.drawing_number AS ProductDrawingNumber,
                    wo.quantity, wo.planned_start, wo.due_date, wo.status, wo.notes,
                    wo.created_by_id, u.""UserName"" AS CreatedByName,
                    wo.created_at, wo.updated_at,
                    (SELECT COUNT(*) FROM planning.order_operations oo WHERE oo.work_order_id = wo.id) AS TotalOperations,
                    (SELECT COUNT(*) FROM planning.order_operations oo WHERE oo.work_order_id = wo.id AND oo.status = 'Completed') AS CompletedOperations,
                    100.0 * (SELECT COUNT(*) FROM planning.order_operations oo WHERE oo.work_order_id = wo.id AND oo.status = 'Completed') 
                        / NULLIF((SELECT COUNT(*) FROM planning.order_operations oo WHERE oo.work_order_id = wo.id), 0) AS CompletionPercent
                FROM planning.work_orders wo
                LEFT JOIN engineering.products p ON wo.product_id = p.id
                LEFT JOIN identity.""AspNetUsers"" u ON wo.created_by_id = u.""Id""
                WHERE wo.id = @Id";

            return await conn.QueryFirstOrDefaultAsync<WorkOrderResponseDto>(sql, new { Id = id });
        }

        public async Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto dto, string userId, CancellationToken ct = default)
        {
            await using var conn = GetConnection();

            // RETURNING сразу возвращает созданную запись (экономит один запрос к БД)
            var sql = @"
                INSERT INTO planning.work_orders 
                    (id, order_number, product_id, quantity, planned_start, due_date, status, notes, created_by_id, created_at)
                VALUES 
                    (gen_random_uuid(), @OrderNumber, @ProductId, @Quantity, @PlannedStart, @DueDate, @Status, @Notes, @UserId, NOW())
                RETURNING 
                    id, order_number, product_id, quantity, planned_start, due_date, status, notes, created_by_id, created_at, updated_at,
                    '' AS ProductName, '' AS ProductDrawingNumber, '' AS CreatedByName, 
                    0 AS TotalOperations, 0 AS CompletedOperations, 0.0 AS CompletionPercent";

            return await conn.QueryFirstAsync<WorkOrderResponseDto>(sql, new
            {
                dto.OrderNumber,
                dto.ProductId,
                dto.Quantity,
                dto.PlannedStart,
                dto.DueDate,
                dto.Status,
                dto.Notes,
                UserId = userId
            });
        }

        public async Task<WorkOrderResponseDto> UpdateAsync(Guid id, UpdateWorkOrderDto dto, CancellationToken ct = default)
        {
            await using var conn = GetConnection();

            // COALESCE обновляет только переданные поля, остальные оставляет без изменений
            var sql = @"
                UPDATE planning.work_orders
                SET 
                    order_number = COALESCE(@OrderNumber, order_number),
                    product_id = COALESCE(@ProductId, product_id),
                    quantity = COALESCE(@Quantity, quantity),
                    planned_start = COALESCE(@PlannedStart, planned_start),
                    due_date = COALESCE(@DueDate, due_date),
                    status = COALESCE(@Status, status),
                    notes = COALESCE(@Notes, notes),
                    updated_at = NOW()
                WHERE id = @Id
                RETURNING 
                    id, order_number, product_id, quantity, planned_start, due_date, status, notes, created_by_id, created_at, updated_at,
                    '' AS ProductName, '' AS ProductDrawingNumber, '' AS CreatedByName, 
                    0 AS TotalOperations, 0 AS CompletedOperations, 0.0 AS CompletionPercent";

            return await conn.QueryFirstAsync<WorkOrderResponseDto>(sql, new
            {
                Id = id,
                dto.OrderNumber,
                dto.ProductId,
                dto.Quantity,
                dto.PlannedStart,
                dto.DueDate,
                dto.Status,
                dto.Notes
            });
        }

        public async Task UpdateStatusAsync(Guid id, string newStatus, CancellationToken ct = default)
        {
            await using var conn = GetConnection();
            var sql = "UPDATE planning.work_orders SET status = @Status, updated_at = NOW() WHERE id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id, Status = newStatus });
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await using var conn = GetConnection();
            var sql = "DELETE FROM planning.work_orders WHERE id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<bool> ExistsByOrderNumberAsync(string orderNumber, Guid? excludeId = null, CancellationToken ct = default)
        {
            await using var conn = GetConnection();
            var sql = @"
                SELECT EXISTS(
                    SELECT 1 FROM planning.work_orders 
                    WHERE order_number = @OrderNumber AND (@ExcludeId IS NULL OR id <> @ExcludeId)
                )";
            return await conn.ExecuteScalarAsync<bool>(sql, new { OrderNumber = orderNumber, ExcludeId = excludeId });
        }
    }
}
