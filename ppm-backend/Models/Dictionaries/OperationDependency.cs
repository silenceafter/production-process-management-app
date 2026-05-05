namespace PpmBackend.Models.Dictionaries
{
    public class OperationDependency
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PredecessorId { get; set; }
        public Guid SuccessorId { get; set; }
        public DependencyType Type { get; set; } = DependencyType.FinishToStart;
        public double LagHours { get; set; } = 0;

        public OrderOperation Predecessor { get; set; } = null!;
        public OrderOperation Successor { get; set; } = null!;
    }
}
