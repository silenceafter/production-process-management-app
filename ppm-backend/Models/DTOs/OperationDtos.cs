using System.ComponentModel.DataAnnotations;

namespace PpmBackend.Models.DTOs
{
    public record CreateOperationDto(
        [Required, MaxLength(50)] string Code,
        [Required, MaxLength(100)] string Name,
        [MaxLength(100)] string? Document,
        [MaxLength(1000)] string? Description,
        Guid? DefaultEquipmentId,
        Guid? DefaultToolingId,
        Guid? DefaultJobId,
        [Range(1, 10)] int? DefaultGrade,
        [Range(0, 99999.99)] decimal? DefaultSetupMinutes,
        [Range(0, 99999.99)] decimal? DefaultUnitMinutesPerPiece,
        Guid? DefaultMeasuringToolId
    );

    public record UpdateOperationDto(
        [MaxLength(50)] string? Code,
        [MaxLength(100)] string? Name,
        [MaxLength(100)] string? Document,
        [MaxLength(1000)] string? Description,
        Guid? DefaultEquipmentId,
        Guid? DefaultToolingId,
        Guid? DefaultJobId,
        [Range(1, 10)] int? DefaultGrade,
        [Range(0, 99999.99)] decimal? DefaultSetupMinutes,
        [Range(0, 99999.99)] decimal? DefaultUnitMinutesPerPiece,
        Guid? DefaultMeasuringToolId
    );

    public record OperationResponseDto(
        Guid Id,
        string Code,
        string Name,
        string? Document,
        string? Description,
        Guid? DefaultEquipmentId,
        string? DefaultEquipmentName,
        Guid? DefaultToolingId,
        string? DefaultToolingName,
        Guid? DefaultJobId,
        string? DefaultJobName,
        int? DefaultGrade,
        decimal? DefaultSetupMinutes,
        decimal? DefaultUnitMinutesPerPiece,
        Guid? DefaultMeasuringToolId,
        string? DefaultMeasuringToolName
    );
}
