using System;
using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    public record CreateWorkOrderDto(
        // 🔥 1. Сначала ВСЕ обязательные параметры
        [Required, MaxLength(50)] string OrderNumber,
        [Required] Guid ProductId,
        [Required] DateTimeOffset PlannedStart,

        // ❓ 2. Потом nullable параметры (без значений по умолчанию)
        DateTimeOffset? DueDate,
        [MaxLength(500)] string? Notes,
        string? CreatedById,

        // 🔘 3. В КОНЦЕ параметры с дефолтными значениями
        [MaxLength(50)] string Status = "Draft",
        [Range(1, 999999)] int Quantity = 1
    );

    public record UpdateWorkOrderDto(
        [MaxLength(50)] string? OrderNumber,
        Guid? ProductId,
        [Range(1, 999999)] int? Quantity,
        DateTimeOffset? PlannedStart,
        DateTimeOffset? DueDate,
        [MaxLength(50)] string? Status,
        [MaxLength(500)] string? Notes,
        DateTimeOffset? UpdatedAt
    );

    public class WorkOrderResponseDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = "";
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = "";
        public string ProductDrawingNumber { get; set; } = "";
        public int Quantity { get; set; }
        public DateTimeOffset PlannedStart { get; set; }
        public DateTimeOffset? DueDate { get; set; }
        public string Status { get; set; } = "";
        public string? Notes { get; set; }
        public string? CreatedById { get; set; }
        public string? CreatedByName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public double CompletionPercent { get; set; }
        public int TotalOperations { get; set; }
        public int CompletedOperations { get; set; }
    }

    public record WorkOrderFilterDto(
        // 🔍 Фильтрация
        string? Status,                    // Фильтр по статусу: "Draft", "Released", "InProgress", "Completed", "Cancelled"
        Guid? ProductId,                   // Фильтр по изделию
        DateTime? StartDate,              // PlannedStart >= (диапазон "от")
        DateTime? EndDate,                // PlannedStart <= (диапазон "до")
        string? Search,                   // Поиск по OrderNumber или ProductName (частичное совпадение)

        // 🔀 Сортировка
        string? SortBy = "CreatedAt",     // Поле для сортировки: "CreatedAt", "PlannedStart", "OrderNumber", "Status"
        bool SortDescending = true,       // По убыванию (true) или возрастанию (false)

        // 📄 Пагинация
        int Page = 1,                     // Номер страницы (1-based)
        int PageSize = 50                 // Записей на страницу (макс. 200)
    )
    {
        // 🔑 Валидация пагинации (опционально, можно вынести в сервис)
        public int ClampedPageSize => Math.Clamp(PageSize, 1, 200);
        public int SkipCount => (Page - 1) * ClampedPageSize;
    }
}