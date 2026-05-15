using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PpmBackend.Helpers.PpmBackend.Helpers;
using PpmBackend.Models.DTOs;
using PpmBackend.Models.Planning;
using PpmBackend.Services.Planning;
using System.Security.Claims;

namespace PpmBackend.Controllers
{
    [AllowAnonymous]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrdersController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<WorkOrdersController> _logger;

        public WorkOrdersController(IConfiguration config, ILogger<WorkOrdersController> logger)
        {
            _config = config;
            _logger = logger;
        }

        // 🔑 Вспомогательный метод для создания соединения
        private NpgsqlConnection GetConn() =>
            new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        // ==========================================
        // GET: api/workorders
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetList(
            string? status = null,
            Guid? productId = null,
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                await using var conn = GetConn();

                var offset = (page - 1) * pageSize;

                var sql = @"
                    SELECT 
                        row_number() over(order by wo.created_at desc) as row_num,
                        wo.id, 
                        wo.order_number, 
                        wo.quantity,
                        to_char(wo.planned_start AT TIME ZONE 'UTC', 'DD.MM.YYYY HH24:MI:SS') as planned_start,
                        to_char(wo.due_date AT TIME ZONE 'UTC', 'DD.MM.YYYY HH24:MI:SS') as due_date,
                        case 
                            when wo.status = 'Draft' then 'Черновик'
                            when wo.status = 'Released' then 'Выпущен'
                            when wo.status = 'InProgress' then 'В работе'
                            when wo.status = 'Completed' then 'Завершен'
                            when wo.status = 'Cancelled' then 'Отменен'
                        end as status, 
                        to_char(wo.created_at AT TIME ZONE 'UTC', 'DD.MM.YYYY HH24:MI:SS') as created_at,
                        p.name AS product_name,      
                        p.drawing_number
                    FROM planning.work_orders wo
                    LEFT JOIN engineering.products p ON wo.product_id = p.id
                    WHERE (@Status IS NULL OR wo.status = @Status)
                      AND (@ProductId IS NULL OR wo.product_id = @ProductId)
                    ORDER BY wo.created_at DESC
                    LIMIT @PageSize OFFSET @Offset";

                var items = await conn.QueryAsync(sql, new
                {
                    Status = status,
                    ProductId = productId,
                    PageSize = pageSize,
                    Offset = offset
                });

                return Ok(items); // Возвращаем динамические объекты (Dapper)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching work orders");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // GET: api/workorders/{id}
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                await using var conn = GetConn();

                var sql = @"
                    SELECT 
                        id, order_number, product_id, quantity,
                        planned_start, due_date, status, notes, created_at
                    FROM planning.work_orders
                    WHERE id = @Id";

                var item = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });

                if (item == null) return NotFound();

                return Ok(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching work order {OrderId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // POST: api/workorders
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequest dto)
        {
            try
            {
                await using var conn = GetConn();

                // Берем пользователя из токена
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";

                var sql = @"
                    INSERT INTO planning.work_orders 
                        (id, order_number, product_id, quantity, planned_start, status, created_by_id, created_at)
                    VALUES 
                        (gen_random_uuid(), @OrderNumber, @ProductId, @Quantity, @PlannedStart, @Status, @UserId, NOW())
                    RETURNING id, order_number, created_at";

                var created = await conn.QueryFirstAsync(sql, new
                {
                    dto.OrderNumber,
                    dto.ProductId,
                    dto.Quantity,
                    dto.PlannedStart,
                    Status = dto.Status ?? "Draft",
                    UserId = userId
                });

                return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
            }
            catch (Exception ex)
            {
                // Конфликт, если дублируется номер заказа (уникальный индекс)
                if (ex.Message.Contains("duplicate key"))
                    return Conflict(new { error = "Заказ с таким номером уже существует" });

                _logger.LogError(ex, "Error creating work order");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // PATCH: api/workorders/{id}/status
        // ==========================================
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] StatusUpdate dto)
        {
            try
            {
                await using var conn = GetConn();

                var sql = "UPDATE planning.work_orders SET status = @Status, updated_at = NOW() WHERE id = @Id";

                var rows = await conn.ExecuteAsync(sql, new { Id = id, Status = dto.Status });

                if (rows == 0) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for order {OrderId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // DELETE: api/workorders/{id}
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await using var conn = GetConn();

                var sql = "DELETE FROM planning.work_orders WHERE id = @Id";
                var rows = await conn.ExecuteAsync(sql, new { Id = id });

                if (rows == 0) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting work order {OrderId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // POST: api/workorders/{id}/calculate-pert
        // ==========================================
        [HttpPost("{id}/calculate-pert")]
        public async Task<IActionResult> CalculatePert(Guid id)
        {
            try
            {
                await using var conn = GetConn();

                // 1. Загружаем операции заказа с оценками длительности
                var operationsSql = @"
            SELECT 
                id, 
                COALESCE(duration_opt, 0) AS DurationOptimistic,
                COALESCE(duration_most_likely, 0) AS DurationMostLikely,
                COALESCE(duration_pessimistic, 0) AS DurationPessimistic,
                scheduled_start,
                scheduled_end
            FROM planning.order_operations
            WHERE work_order_id = @WorkOrderId";

                var operations = (await conn.QueryAsync<PertNode>(operationsSql, new { WorkOrderId = id }))
                    .ToList();

                if (operations.Count == 0)
                    return NotFound("Операции для этого заказа не найдены");

                // 2. Загружаем зависимости между операциями
                var depsSql = @"
            SELECT predecessor_id AS PredecessorId, successor_id AS SuccessorId, 
                   type AS Type, COALESCE(lag_minutes, 0) AS LagMinutes
            FROM planning.operation_dependencies
            WHERE predecessor_id IN @Ids OR successor_id IN @Ids";

                var dependencies = (await conn.QueryAsync<Dependency>(depsSql, new
                {
                    Ids = operations.Select(o => o.Id)
                })).ToList();

                // 3. Запускаем расчёт PERT
                var results = PertCalculator.CalculateCriticalPath(operations, dependencies);

                // 4. Обновляем рассчитанные поля в БД
                var updateSql = @"
            UPDATE planning.order_operations
            SET 
                duration_expected = @DurationExpected,
                variance = @Variance,
                earliest_start = @EarliestStart,
                latest_start = @LatestStart,
                is_critical_path = @IsCriticalPath,
                updated_at = NOW()
            WHERE id = @Id";

                foreach (var node in results)
                {
                    await conn.ExecuteAsync(updateSql, new
                    {
                        node.Id,
                        node.DurationExpected,
                        node.Variance,
                        node.EarliestStart,
                        node.LatestStart,
                        node.IsCriticalPath
                    });
                }

                // 5. Возвращаем результат
                return Ok(new
                {
                    workOrderId = id,
                    totalDuration = results.Max(r => r.EarliestFinish),
                    criticalPathOperations = results.Where(r => r.IsCriticalPath).Select(r => r.Id),
                    operations = results.Select(r => new
                    {
                        r.Id,
                        r.DurationExpected,
                        r.EarliestStart,
                        r.LatestStart,
                        r.Slack,
                        r.IsCriticalPath
                    })
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating PERT for order {OrderId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // 🔹 Минимальные классы для входящих данных (можно даже без них, но так удобнее)
    public class CreateRequest
    {
        public string OrderNumber { get; set; } = "";
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTimeOffset PlannedStart { get; set; }
        public string? Status { get; set; }
    }

    public class StatusUpdate
    {
        public string Status { get; set; } = "";
    }
}
