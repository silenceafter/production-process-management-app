namespace PpmBackend.Models
{
    public enum BomItemTypeEnum { Material, Component }
    public enum ToolingTypeEnum { Fixture, Cutting, Consumable, Other }
    public enum MeasuringToolTypeEnum { Caliper, Micrometer, Gauge, Sensor, Other }
    public enum DependencyTypeEnum { FinishToStart, StartToStart, FinishToFinish, StartToFinish }
}
