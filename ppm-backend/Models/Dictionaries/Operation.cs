using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.Dictionaries
{
    public class Operation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Sequence { get; set; }

        // operationCode → плоские поля
        [MaxLength(50)] public string Code { get; set; } = "";
        [Required, MaxLength(100)] public string Name { get; set; } = "";

        [MaxLength(100)] public string? Document { get; set; } // op_document
        [MaxLength(1000)] public string? Description { get; set; } // operationDescription

        // Ресурсы
        public Guid EquipmentId { get; set; }
        public Equipment Equipment { get; set; } = null!;
        public Guid? ToolingId { get; set; }
        public Tooling? Tooling { get; set; }
        public Guid? MeasuringToolId { get; set; }
        public MeasuringTool? MeasuringTool { get; set; }
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;

        // Нормы и условия (из legacy)
        public int Grade { get; set; } = 1;
        [MaxLength(200)] public string? WorkingConditions { get; set; }
        public int NumberOfWorkers { get; set; } = 1;
        public int NumberOfProcessedParts { get; set; } = 1;

        // Нормы времени (минуты) — для PERT/планирования
        [Range(0, double.MaxValue)] public decimal SetupMinutes { get; set; } = 0;          // наладка
        [Range(0.01, double.MaxValue)] public decimal UnitMinutesPerPiece { get; set; } = 0; // на 1 деталь

        // public decimal LaborEffort { get; set; } = 0;

        // PERT-оценки (опционально)
        public decimal? OptimisticMinutes { get; set; }
        public decimal? MostLikelyMinutes { get; set; }
        public decimal? PessimisticMinutes { get; set; }
    }
}
