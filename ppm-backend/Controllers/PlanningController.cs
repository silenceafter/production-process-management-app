using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PpmBackend.Data;
using PpmBackend.Models.Dictionaries;
using PpmBackend.Models.DTOs;
using PpmBackend.Services;
using System.Security.Claims;

namespace PpmBackend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PlanningController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly WorkOrderService _orderService;
        private readonly PertCalculator _pertCalc;

        public PlanningController(ApplicationDbContext db, WorkOrderService orderService, PertCalculator pertCalc)
        {
            _db = db;
            _orderService = orderService;
            _pertCalc = pertCalc;
        }

        [HttpPost("workorders")]
        public async Task<IActionResult> CreateWorkOrder([FromBody] CreateWorkOrderDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var order = await _orderService.CreateAsync(userId, dto);
            return CreatedAtAction(nameof(GetWorkOrder), new { id = order.Id }, order);
        }

        [HttpGet("workorders/{id}")]
        public async Task<IActionResult> GetWorkOrder(Guid id)
        {
            var order = await _db.WorkOrders
                .Include(w => w.Operations)
                .FirstOrDefaultAsync(w => w.Id == id);
            if (order == null) return NotFound();
            return Ok(order);
        }

        [HttpPost("workorders/{id}/dependencies")]
        public async Task<IActionResult> SetDependencies(Guid id, [FromBody] List<SetOperationDependencyDto> deps)
        {
            var existing = _db.OperationDependencies.Where(d => d.Predecessor.WorkOrderId == id);
            _db.OperationDependencies.RemoveRange(existing);

            var newDeps = deps.Select(d => new OperationDependency
            {
                PredecessorId = d.PredecessorId,
                SuccessorId = d.SuccessorId,
                Type = d.Type,
                LagHours = d.LagHours
            });
            _db.OperationDependencies.AddRange(newDeps);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("workorders/{id}/calculate-pert")]
        public async Task<IActionResult> CalculatePert(Guid id)
        {
            await _pertCalc.CalculateAsync(id);
            return Ok(new { message = "PERT расчёт завершён" });
        }
    }
}
