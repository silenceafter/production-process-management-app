using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Security.Claims;

namespace PpmBackend.Controllers
{
    [AllowAnonymous]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IConfiguration config, ILogger<ProductsController> logger)
        {
            _config = config;
            _logger = logger;
        }

        // 🔑 Вспомогательный метод для создания соединения
        private NpgsqlConnection GetConn() =>
            new NpgsqlConnection(_config.GetConnectionString("DefaultConnection"));

        // ==========================================
        // GET: api/products
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> GetList(
            string? search = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 100)
        {
            try
            {
                await using var conn = GetConn();

                var offset = (page - 1) * pageSize;
                var searchPattern = string.IsNullOrEmpty(search) ? null : $"%{search}%";

                var sql = @"
                    SELECT 
                        id,
                        name,
                        number,
                        revision,
                        is_active,
                        modification,
                        code,
                        technology_id
                    FROM engineering.products
                    WHERE (@Search IS NULL OR name ILIKE @SearchPattern OR number ILIKE @SearchPattern)
                      AND (@IsActive IS NULL OR is_active = @IsActive)
                    ORDER BY name ASC
                    LIMIT @PageSize OFFSET @Offset";

                var products = await conn.QueryAsync(sql, new
                {
                    Search = search,
                    SearchPattern = searchPattern,
                    IsActive = isActive,
                    PageSize = pageSize,
                    Offset = offset
                });

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // GET: api/products/{id}
        // ==========================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                await using var conn = GetConn();

                var sql = @"
                    SELECT 
                        id,
                        name,
                        number,
                        revision,
                        is_active,
                        modification,
                        code,
                        technology_id
                    FROM engineering.products
                    WHERE id = @Id";

                var product = await conn.QueryFirstOrDefaultAsync(sql, new { Id = id });

                if (product == null) return NotFound();

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product {ProductId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // POST: api/products
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest dto)
        {
            try
            {
                await using var conn = GetConn();

                var sql = @"
                    INSERT INTO engineering.products 
                        (id, name, number, revision, is_active, modification, code, technology_id)
                    VALUES 
                        (gen_random_uuid(), @Name, @Number, @Revision, @IsActive, @Modification, @Code, @TechnologyId)
                    RETURNING id, name, number, is_active";

                var created = await conn.QueryFirstAsync(sql, new
                {
                    dto.Name,
                    dto.Number,
                    dto.Revision,
                    IsActive = dto.IsActive ?? true,
                    dto.ModificationCode,
                    dto.BaseDesignation,
                    dto.TechnologyId
                });

                return CreatedAtAction(nameof(GetById), new { id = created.id }, created);
            }
            catch (Exception ex)
            {
                // Проверяем уникальность имени или номера чертежа
                if (ex.Message.Contains("duplicate key"))
                {
                    if (ex.Message.Contains("products_name_key"))
                        return Conflict(new { error = "Изделие с таким именем уже существует" });

                    if (ex.Message.Contains("products_number_key"))
                        return Conflict(new { error = "Изделие с таким номером чертежа уже существует" });
                }

                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // PUT: api/products/{id}
        // ==========================================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest dto)
        {
            try
            {
                await using var conn = GetConn();

                var sql = @"
                    UPDATE engineering.products
                    SET 
                        name = @Name,
                        number = @Number,
                        revision = @Revision,
                        is_active = @IsActive,
                        modification = @Modification,
                        code = @Code,
                        technology_id = @TechnologyId
                    WHERE id = @Id
                    RETURNING id, name, number, is_active";

                var updated = await conn.QueryFirstOrDefaultAsync(sql, new
                {
                    Id = id,
                    dto.Name,
                    dto.Number,
                    dto.Revision,
                    dto.IsActive,
                    dto.ModificationCode,
                    dto.BaseDesignation,
                    dto.TechnologyId
                });

                if (updated == null) return NotFound();

                return Ok(updated);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("duplicate key"))
                {
                    if (ex.Message.Contains("products_name_key"))
                        return Conflict(new { error = "Изделие с таким именем уже существует" });

                    if (ex.Message.Contains("products_number_key"))
                        return Conflict(new { error = "Изделие с таким номером чертежа уже существует" });
                }

                _logger.LogError(ex, "Error updating product {ProductId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // ==========================================
        // DELETE: api/products/{id}
        // ==========================================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await using var conn = GetConn();

                // Проверяем, не используется ли изделие в заказах
                var checkSql = "SELECT EXISTS(SELECT 1 FROM planning.work_orders WHERE product_id = @Id)";
                var isInUse = await conn.ExecuteScalarAsync<bool>(checkSql, new { Id = id });

                if (isInUse)
                    return BadRequest(new { error = "Нельзя удалить изделие, которое используется в заказах" });

                var sql = "DELETE FROM engineering.products WHERE id = @Id";
                var rows = await conn.ExecuteAsync(sql, new { Id = id });

                if (rows == 0) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product {ProductId}", id);
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    // 🔹 Классы для входящих данных
    public class CreateProductRequest
    {
        public string Name { get; set; } = "";
        public string Number { get; set; } = "";
        public string? Revision { get; set; }
        public bool? IsActive { get; set; } = true;
        public string? ModificationCode { get; set; }
        public string? BaseDesignation { get; set; }
        public Guid? TechnologyId { get; set; }
    }

    public class UpdateProductRequest : CreateProductRequest
    {
        // Наследуем все поля из CreateProductRequest
    }
}