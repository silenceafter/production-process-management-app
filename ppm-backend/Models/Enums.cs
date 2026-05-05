namespace PpmBackend.Models
{
    public enum OrderStatus { Draft, Released, InProgress, Paused, Completed, Cancelled }
    public enum OperationStatus { Pending, Scheduled, InProgress, Done, Skipped }
    public enum BomItemType { Material, Component }
    public enum ToolingType { Fixture, Cutting, Consumable, Other }
    public enum MeasuringToolType { Caliper, Micrometer, Gauge, Sensor, Other }
    public enum DependencyType { FinishToStart, StartToStart, FinishToFinish, StartToFinish }
}
