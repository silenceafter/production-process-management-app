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
                        wo.planned_start,
                        wo.due_date,
                        wo.product_id,
                        wo.status, 
                        wo.created_at,
                        p.name AS product_name,      
                        p.number as drawing_number
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRequest dto)
        {
            try
            {
                await using var conn = GetConn();

                // 🔹 ВРЕМЕННО: null для created_by_id (разрешите NULL в БД или создайте тестового пользователя)
                Guid? createdById = null;

                // ==========================================
                // 1. Создаём заказ
                // ==========================================
                var createOrderSql = @"
            INSERT INTO planning.work_orders 
                (id, order_number, product_id, quantity, planned_start, due_date, status, created_by_id, created_at)
            VALUES 
                (gen_random_uuid(), (select planning.generate_work_order_number()), @ProductId, @Quantity, @PlannedStart, @DueDate, @Status, @CreatedById, NOW())
            RETURNING id, order_number";

                var order = await conn.QueryFirstAsync(createOrderSql, new
                {
                    dto.ProductId,
                    dto.Quantity,
                    PlannedStart = dto.PlannedStart.ToUniversalTime(),
                    DueDate = dto.DueDate.ToUniversalTime(),
                    Status = "Draft",
                    CreatedById = createdById
                });

                // ==========================================
                // 2. Получаем technology_id из продукта
                // ==========================================
                var techSql = @"
            SELECT technology_id 
            FROM engineering.products 
            WHERE id = @ProductId";

                var technologyId = await conn.ExecuteScalarAsync<Guid?>(techSql, new { dto.ProductId });

                if (!technologyId.HasValue)
                    return BadRequest(new { error = "Для изделия не задан технологический процесс" });

                // ==========================================
                // 3. Копируем операции из engineering.operations
                // ==========================================
                // 🔹 Без алиасов — реальные имена колонок из БД
                var sourceOpsSql = @"
            SELECT 
                op.id,
                op.code,
                op.name,
                op.default_setup_minutes,
                op.default_unit_minutes_per_piece,
                op.default_equipment_id
            FROM engineering.technology_operations t_op
            JOIN engineering.operations op ON t_op.operation_id = op.id
            WHERE t_op.technology_id = @TechnologyId
            ORDER BY t_op.sequence ASC";

                var sourceOperations = await conn.QueryAsync<dynamic>(sourceOpsSql, new { TechnologyId = technologyId.Value });
                var sourceOpsList = sourceOperations.ToList();

                if (sourceOpsList.Count == 0)
                    return BadRequest(new { error = "Техпроцесс не содержит операций" });

                // ==========================================
                // 4. Вставляем операции в planning.order_operations
                // ==========================================
                var insertOpSql = @"
            INSERT INTO planning.order_operations 
                (id, work_order_id, operation_id, sequence, operation_code, status, 
                 setup_minutes, unit_minutes_per_piece, 
                 duration_opt, duration_most_likely, duration_pessimistic,
                 assigned_equipment_id)
            VALUES 
                (gen_random_uuid(), @WorkOrderId, @OperationId, @Sequence, @OperationCode, 'Pending',
                 @SetupMinutes, @UnitMinutes,
                 @DurationOpt, @DurationMostLikely, @DurationPessimistic,
                 @EquipmentId)
            RETURNING id";

                var oldToNewIds = new Dictionary<Guid, Guid>();
                int sequence = 0;

                foreach (var op in sourceOpsList)
                {
                    var setupMin = op.default_setup_minutes != null ? (double)op.default_setup_minutes : 0;
                    var unitMin = op.default_unit_minutes_per_piece != null ? (double)op.default_unit_minutes_per_piece : 0;
                    var totalMin = setupMin + (unitMin * dto.Quantity);

                    var newId = await conn.ExecuteScalarAsync<Guid>(insertOpSql, new
                    {
                        WorkOrderId = order.id,
                        OperationId = op.id,
                        Sequence = sequence++,
                        OperationCode = op.code,
                        SetupMinutes = setupMin,
                        UnitMinutes = unitMin,
                        DurationOpt = totalMin * 0.8,
                        DurationMostLikely = totalMin,
                        DurationPessimistic = totalMin * 1.3,
                        EquipmentId = op.default_equipment_id
                    });

                    oldToNewIds[op.id] = newId;
                }

                // ==========================================
                // 5. Копируем зависимости между операциями
                // ==========================================
                /*var copyDepsSql = @"
            SELECT predecessor_id, successor_id, type, lag_minutes
            FROM planning.operation_dependencies
            WHERE predecessor_id = ANY(@OldIds) OR successor_id = ANY(@OldIds)";

                var dependencies = await conn.QueryAsync<dynamic>(copyDepsSql, new
                {
                    OldIds = oldToNewIds.Keys.ToList() // 🔹 Материализуем для Dapper
                });

                var insertDepSql = @"
            INSERT INTO planning.operation_dependencies 
                (predecessor_id, successor_id, type, lag_minutes)
            VALUES 
                (@PredecessorId, @SuccessorId, @Type, @LagMinutes)";

                foreach (var dep in dependencies)
                {
                    await conn.ExecuteAsync(insertDepSql, new
                    {
                        PredecessorId = oldToNewIds[dep.predecessor_id],
                        SuccessorId = oldToNewIds[dep.successor_id],
                        Type = dep.type ?? "FinishToStart",
                        LagMinutes = dep.lag_minutes ?? 0
                    });
                }*/

                var sortedOps = sourceOpsList.OrderBy(o => o.sequence ?? 0).ToList();
                var newOpIds = sortedOps.Select(o => oldToNewIds[o.id]).ToList();

                if (newOpIds.Count > 1)
                {
                    var insertDepSql = @"
        INSERT INTO planning.operation_dependencies 
            (predecessor_id, successor_id, type, lag_minutes)
        VALUES 
            (@PredecessorId, @SuccessorId, 'FinishToStart', 0)";

                    // Создаём связь: op[0] → op[1], op[1] → op[2], и т.д.
                    for (int i = 0; i < newOpIds.Count - 1; i++)
                    {
                        await conn.ExecuteAsync(insertDepSql, new
                        {
                            PredecessorId = newOpIds[i],
                            SuccessorId = newOpIds[i + 1]
                        });
                    }
                }

                // ==========================================
                // 6. Возвращаем результат
                // ==========================================
                return CreatedAtAction(nameof(GetById), new { id = order.id }, new
                {
                    order.id,
                    order.order_number,
                    operationsCount = oldToNewIds.Count,
                    message = $"Заказ создан. Операций: {oldToNewIds.Count}, Зависимостей: {Math.Max(0, oldToNewIds.Count - 1)}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating work order with operations");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // PATCH: api/workorders/{id} (Полное обновление)
        // ==========================================
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkOrderRequest dto)
        {
            try
            {
                await using var conn = GetConn();

                // Обновляем ТОЛЬКО переданные поля (остальные не трогаем)
                // COALESCE(@Param, column) — если параметр null, оставляем старое значение
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
                id, order_number, product_id, quantity, 
                planned_start, due_date, status, notes, updated_at";

                var updated = await conn.QueryFirstOrDefaultAsync(sql, new
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

                if (updated == null)
                    return NotFound($"Заказ #{id} не найден");

                return Ok(updated);
            }
            catch (Exception ex)
            {
                // Обработка конфликта уникальности номера заказа
                if (ex.Message.Contains("duplicate key") && ex.Message.Contains("work_orders_order_number_key"))
                    return Conflict(new { error = "Заказ с таким номером уже существует" });

                _logger.LogError(ex, "Error updating work order {OrderId}", id);
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

        [HttpPost("{id}/calculate-pert")]
        public async Task<IActionResult> CalculatePert(Guid id, [FromQuery] string? scenario = null)
        {
            try
            {
                await using var conn = GetConn();

                // 1. Нормализуем сценарий (по умолчанию: expected)
                var scenarioType = string.IsNullOrWhiteSpace(scenario) ? "expected" : scenario.ToLowerInvariant();
                if (!new[] { "expected", "optimistic", "pessimistic" }.Contains(scenarioType))
                    scenarioType = "expected";

                // 2. Загружаем операции
                var opsSql = @"
            SELECT 
                id, sequence, operation_code,
                COALESCE(duration_opt, 1) AS duration_opt,
                COALESCE(duration_most_likely, 2) AS duration_most_likely,
                COALESCE(duration_pessimistic, 3) AS duration_pessimistic
            FROM planning.order_operations
            WHERE work_order_id = @WorkOrderId
            ORDER BY sequence";

                var operations = (await conn.QueryAsync<dynamic>(opsSql, new { WorkOrderId = id })).ToList();
                if (operations.Count == 0) return NotFound("Операции не найдены");

                // 3. Загружаем зависимости
                var depsSql = @"
            SELECT predecessor_id, successor_id, 
                   COALESCE(type, 'FinishToStart') AS type, 
                   COALESCE(lag_minutes, 0) AS lag_minutes
            FROM planning.operation_dependencies
            WHERE predecessor_id IS NOT NULL 
              AND successor_id IS NOT NULL
              AND (predecessor_id = ANY(@Ids) OR successor_id = ANY(@Ids))";

                var operationIds = operations.Select(o => (Guid)o.id).ToList();
                var dependencies = (await conn.QueryAsync<dynamic>(depsSql, new { Ids = operationIds })).ToList();

                // 4. Маппинг в PertNode с учётом сценария
                var nodes = operations.Select(o =>
                {
                    double opt = Convert.ToDouble(o.duration_opt);
                    double most = Convert.ToDouble(o.duration_most_likely);
                    double pess = Convert.ToDouble(o.duration_pessimistic);

                    // Для детерминированных сценариев выравниваем все три оценки
                    if (scenarioType == "optimistic") opt = most = pess = opt;
                    else if (scenarioType == "pessimistic") opt = most = pess = pess;

                    return new PertNode
                    {
                        Id = o.id,
                        DurationOptimistic = opt,
                        DurationMostLikely = most,
                        DurationPessimistic = pess
                    };
                }).ToList();

                var deps = dependencies.Select(d => new Dependency
                {
                    PredecessorId = d.predecessor_id,
                    SuccessorId = d.successor_id,
                    Type = d.type,
                    LagMinutes = Convert.ToDouble(d.lag_minutes)
                }).ToList();

                // 5. Считаем критический путь
                var results = PertCalculator.CalculateCriticalPath(nodes, deps);

                // 6. Сохраняем в БД ТОЛЬКО базовый сценарий (чтобы не перезаписывать план)
                if (scenarioType == "expected")
                {
                    var updateSql = @"
                UPDATE planning.order_operations
                SET 
                    duration_expected = @DurationExpected,
                    variance = @Variance,
                    earliest_start = @EarliestStart,
                    latest_start = @LatestStart,
                    is_critical_path = @IsCriticalPath
                WHERE id = @Id";

                    foreach (var r in results)
                    {
                        await conn.ExecuteAsync(updateSql, new
                        {
                            Id = r.Id,
                            DurationExpected = r.DurationExpected,
                            Variance = r.Variance,
                            EarliestStart = r.EarliestStart,
                            LatestStart = r.LatestStart,
                            IsCriticalPath = r.IsCriticalPath
                        });
                    }
                }

                // 7. Формируем ответ (сортируем по sequence из исходных данных)
                var totalDuration = results.Max(r => r.EarliestFinish);

                var sortedOperations = results
                    .Join(operations, r => r.Id, o => o.id, (r, o) => new
                    {
                        Result = r,
                        Sequence = o.sequence,
                        Code = o.operation_code
                    })
                    .OrderBy(x => x.Sequence ?? 0)
                    .Select(x => new
                    {
                        Id = x.Result.Id,
                        OperationCode = x.Code,
                        DurationExpected = x.Result.DurationExpected,
                        EarliestStart = x.Result.EarliestStart,
                        LatestStart = x.Result.LatestStart,
                        Slack = x.Result.Slack,
                        IsCriticalPath = x.Result.IsCriticalPath
                    });

                return Ok(new
                {
                    workOrderId = id,
                    scenario = scenarioType,
                    totalDuration = totalDuration,
                    criticalCount = results.Count(r => r.IsCriticalPath),
                    operations = sortedOperations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PERT calculation failed for order {OrderId}", id);
                return StatusCode(500, new { error = "Ошибка расчёта PERT: " + ex.Message });
            }
        }

    }

    // 🔹 Минимальные классы для входящих данных
    public class CreateRequest
    {
        public string OrderNumber { get; set; } = "";
        public Guid ProductId { get; set; }
        public int Quantity { get; set; } = 1;
        public DateTimeOffset PlannedStart { get; set; }
        public string? Status { get; set; }
        public DateTimeOffset DueDate { get; set; }
    }

    public class UpdateWorkOrderRequest
    {
        public string? OrderNumber { get; set; }
        public Guid? ProductId { get; set; }
        public int? Quantity { get; set; }
        public DateTimeOffset? PlannedStart { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}
