using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PpmBackend.Models.Planning;
using PpmBackend.Models.DTOs;
using System.Security.Claims;
using PpmBackend.Services.Planning;

namespace PpmBackend.Controllers
{
    [AllowAnonymous]
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WorkOrdersController : ControllerBase
    {
        private readonly IWorkOrderService _service;
        private readonly ILogger<WorkOrdersController> _logger;

        public WorkOrdersController(IWorkOrderService service, ILogger<WorkOrdersController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Получить список заказов с фильтрацией и пагинацией
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<List<WorkOrderResponseDto>>> GetList(
            [FromQuery] WorkOrderFilterDto filter)
        {
            try
            {
                var items = await _service.GetListAsync(filter);
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении списка заказов");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Получить заказ по ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkOrderResponseDto>> GetById(Guid id)
        {
            try
            {
                var order = await _service.GetByIdAsync(id);
                if (order == null)
                    return NotFound($"Заказ с ID {id} не найден");

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении заказа {OrderId}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Создать новый заказ
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<WorkOrderResponseDto>> Create([FromBody] CreateWorkOrderDto dto)
        {
            try
            {
                // 🔑 Берем ID текущего пользователя из токена
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var dtoWithUser = dto with { CreatedById = userId };

                var order = await _service.CreateAsync(dtoWithUser);

                return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при создании заказа");
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Обновить заказ
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<WorkOrderResponseDto>> Update(Guid id, [FromBody] UpdateWorkOrderDto dto)
        {
            try
            {
                var order = await _service.UpdateAsync(id, dto);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("не найден"))
                    return NotFound(ex.Message);
                return Conflict(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении заказа {OrderId}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Удалить заказ (мягкое удаление через статус Cancelled)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _service.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении заказа {OrderId}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }

        /// <summary>
        /// Изменить статус заказа
        /// </summary>
        [HttpPatch("{id}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WorkOrderResponseDto>> UpdateStatus(
            Guid id,
            [FromBody] UpdateStatusDto dto)
        {
            try
            {
                var order = await _service.UpdateStatusAsync(id, dto.Status);
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("не найден"))
                    return NotFound(ex.Message);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении статуса заказа {OrderId}", id);
                return StatusCode(500, "Внутренняя ошибка сервера");
            }
        }
    }

    // 🔹 Вспомогательный DTO для изменения статуса
    public record UpdateStatusDto(string Status);
}
