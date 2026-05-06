using Microsoft.EntityFrameworkCore;
using PpmBackend.Data;
using PpmBackend.Models.Dictionaries;

namespace PpmBackend.Services
{
    public class PertCalculator
    {
        private readonly ApplicationDbContext _db;

        public PertCalculator(ApplicationDbContext db) => _db = db;

        public async Task CalculateAsync(Guid workOrderId)
        {
            var ops = await _db.OrderOperations
                .Where(o => o.WorkOrderId == workOrderId)
                .Include(o => o.TemplateOperation)
                .Include(o => o.Predecessors)
                .Include(o => o.Successors)
                .ToListAsync();

            if (!ops.Any()) return;

            // 1. Длительность каждой операции (мин → часы для удобства)
            var durations = new Dictionary<Guid, double>();
            foreach (var op in ops)
            {
                var t = op.TemplateOperation;

                // 🔑 Формула: наладка + (время на деталь × количество в заказе)
                var totalMinutes = t.SetupMinutes + (t.UnitMinutesPerPiece * op.WorkOrder.Quantity);

                durations[op.Id] = (double)(totalMinutes / 60m); // перевод в часы
            }

            // 2. Forward Pass (Earliest Start/Finish)
            var forward = new Dictionary<Guid, double>();
            foreach (var op in TopologicalSort(ops))
            {
                double es = 0;
                foreach (var dep in op.Predecessors)
                {
                    var predDuration = durations[dep.PredecessorId];
                    var predEF = forward[dep.PredecessorId] + predDuration + dep.LagHours;
                    if (predEF > es) es = predEF;
                }
                forward[op.Id] = es;
            }

            // 3. Backward Pass (Latest Start/Finish)
            var projectEnd = forward.Values.Max() + durations[forward.Keys.Last()];
            var backward = new Dictionary<Guid, double>();
            foreach (var op in TopologicalSort(ops).Reverse())
            {
                double lf = projectEnd;
                foreach (var dep in op.Successors)
                {
                    var succLS = backward[dep.SuccessorId] - durations[dep.SuccessorId] - dep.LagHours;
                    if (succLS < lf) lf = succLS;
                }
                backward[op.Id] = lf;
            }

            // 4. Записываем результаты в БД
            foreach (var op in ops)
            {
                op.EarliestStart = forward[op.Id];
                op.LatestStart = backward[op.Id] - durations[op.Id];
                op.IsCriticalPath = Math.Abs(op.LatestStart.Value - op.EarliestStart.Value) < 0.01; // допуск на float
            }

            await _db.SaveChangesAsync();
        }

        // Топологическая сортировка (защита от циклов + порядок расчёта)
        private IEnumerable<OrderOperation> TopologicalSort(List<OrderOperation> ops)
        {
            var inDegree = ops.ToDictionary(o => o.Id, o => o.Predecessors.Count);
            var queue = new Queue<Guid>(inDegree.Where(k => k.Value == 0).Select(k => k.Key));
            var sorted = new List<OrderOperation>();

            while (queue.Any())
            {
                var currentId = queue.Dequeue();
                var current = ops.First(o => o.Id == currentId);
                sorted.Add(current);

                foreach (var dep in current.Successors)
                {
                    inDegree[dep.SuccessorId]--;
                    if (inDegree[dep.SuccessorId] == 0) queue.Enqueue(dep.SuccessorId);
                }
            }

            if (sorted.Count != ops.Count)
                throw new InvalidOperationException("Обнаружен цикл в зависимостях операций.");

            return sorted;
        }
    }
}
