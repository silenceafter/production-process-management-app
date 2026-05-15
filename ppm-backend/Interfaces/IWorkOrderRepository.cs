using PpmBackend.Models.DTOs;

namespace PpmBackend.Interfaces
{
    public interface IWorkOrderRepository
    {
        /// <summary>
        /// Получить список заказов с фильтрацией, сортировкой и пагинацией
        /// </summary>
        Task<List<WorkOrderResponseDto>> GetAllAsync(WorkOrderFilterDto filter, CancellationToken ct = default);

        /// <summary>
        /// Получить заказ по ID с данными продукта и статистикой операций
        /// </summary>
        Task<WorkOrderResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Создать новый заказ
        /// </summary>
        /// <returns>Созданный заказ с заполненным Id и CreatedAt</returns>
        Task<WorkOrderResponseDto> CreateAsync(CreateWorkOrderDto dto, string userId, CancellationToken ct = default);

        /// <summary>
        /// Полностью обновить заказ (замена всех полей)
        /// </summary>
        Task<WorkOrderResponseDto> UpdateAsync(Guid id, UpdateWorkOrderDto dto, CancellationToken ct = default);

        /// <summary>
        /// Частичное обновление: только статус
        /// </summary>
        Task UpdateStatusAsync(Guid id, string newStatus, CancellationToken ct = default);

        /// <summary>
        /// Удалить заказ (физическое удаление из БД)
        /// </summary>
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Проверить существование заказа по номеру (для валидации уникальности)
        /// </summary>
        Task<bool> ExistsByOrderNumberAsync(string orderNumber, Guid? excludeId = null, CancellationToken ct = default);
    }
}
