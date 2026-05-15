namespace PpmBackend.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Dapper;

    namespace PpmBackend.Helpers
    {
        /// <summary>
        /// Простые данные операции для расчёта PERT (без привязки к БД)
        /// </summary>
        public class PertNode
        {
            public Guid Id { get; set; }
            public Guid? PredecessorId { get; set; }  // Для простоты: одна зависимость
            public double DurationOptimistic { get; set; }
            public double DurationMostLikely { get; set; }
            public double DurationPessimistic { get; set; }

            // Рассчитанные поля
            public double DurationExpected { get; set; }
            public double Variance { get; set; }
            public double EarliestStart { get; set; }
            public double EarliestFinish { get; set; }
            public double LatestStart { get; set; }
            public double LatestFinish { get; set; }
            public double Slack { get; set; }
            public bool IsCriticalPath { get; set; }
        }

        public static class PertCalculator
        {
            /// <summary>
            /// Рассчитать ожидаемую длительность по формуле PERT
            /// </summary>
            public static double CalculateExpectedDuration(double optimistic, double mostLikely, double pessimistic) =>
                (optimistic + 4 * mostLikely + pessimistic) / 6;

            /// <summary>
            /// Рассчитать дисперсию (неопределённость)
            /// </summary>
            public static double CalculateVariance(double optimistic, double pessimistic) =>
                Math.Pow((pessimistic - optimistic) / 6, 2);

            /// <summary>
            /// Основной алгоритм: прямой и обратный проход + критический путь
            /// </summary>
            public static List<PertNode> CalculateCriticalPath(List<PertNode> nodes, List<Dependency> dependencies)
            {
                // 1. Рассчитываем DurationExpected и Variance для каждой операции
                foreach (var node in nodes)
                {
                    node.DurationExpected = CalculateExpectedDuration(
                        node.DurationOptimistic, node.DurationMostLikely, node.DurationPessimistic);
                    node.Variance = CalculateVariance(node.DurationOptimistic, node.DurationPessimistic);
                }

                // 2. Строим граф зависимостей
                var graph = new Dictionary<Guid, List<Guid>>();
                var inDegree = new Dictionary<Guid, int>();

                foreach (var node in nodes)
                {
                    graph[node.Id] = new List<Guid>();
                    inDegree[node.Id] = 0;
                }

                foreach (var dep in dependencies)
                {
                    if (graph.ContainsKey(dep.PredecessorId) && graph.ContainsKey(dep.SuccessorId))
                    {
                        graph[dep.PredecessorId].Add(dep.SuccessorId);
                        inDegree[dep.SuccessorId]++;
                    }
                }

                // 3. Прямой проход: Earliest Start/Finish
                var queue = new Queue<Guid>(nodes.Where(n => inDegree[n.Id] == 0).Select(n => n.Id));

                while (queue.Count > 0)
                {
                    var currentId = queue.Dequeue();
                    var current = nodes.First(n => n.Id == currentId);

                    current.EarliestFinish = current.EarliestStart + current.DurationExpected;

                    foreach (var successorId in graph[currentId])
                    {
                        var successor = nodes.First(n => n.Id == successorId);
                        successor.EarliestStart = Math.Max(successor.EarliestStart, current.EarliestFinish);

                        inDegree[successorId]--;
                        if (inDegree[successorId] == 0)
                            queue.Enqueue(successorId);
                    }
                }

                // 4. Обратный проход: Latest Start/Finish
                var projectDuration = nodes.Max(n => n.EarliestFinish);

                foreach (var node in nodes)
                {
                    node.LatestFinish = projectDuration;
                }

                // Топологическая сортировка в обратном порядке
                var reversed = nodes.OrderByDescending(n => n.EarliestFinish).Select(n => n.Id);

                foreach (var currentId in reversed)
                {
                    var current = nodes.First(n => n.Id == currentId);
                    current.LatestStart = current.LatestFinish - current.DurationExpected;

                    foreach (var dep in dependencies.Where(d => d.SuccessorId == currentId))
                    {
                        var predecessor = nodes.First(n => n.Id == dep.PredecessorId);
                        predecessor.LatestFinish = Math.Min(predecessor.LatestFinish, current.LatestStart);
                    }
                }

                // 5. Рассчитываем Slack и отмечаем критический путь
                foreach (var node in nodes)
                {
                    node.Slack = node.LatestStart - node.EarliestStart;
                    node.IsCriticalPath = Math.Abs(node.Slack) < 0.01; // Допуск на погрешность
                }

                return nodes;
            }
        }

        /// <summary>
        /// Простая зависимость между операциями
        /// </summary>
        public class Dependency
        {
            public Guid PredecessorId { get; set; }
            public Guid SuccessorId { get; set; }
            public string Type { get; set; } = "FS"; // Finish-to-Start по умолчанию
            public double LagMinutes { get; set; } = 0;
        }
    }
}
