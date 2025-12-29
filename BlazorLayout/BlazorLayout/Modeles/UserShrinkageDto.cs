using BlazorLayout.Enums;

namespace BlazorLayout.Modeles;
public record UserShrinkageDto
{
    public TimeSpan PaidTime { get; init; }
    public TimeSpan PaidTimeOff { get; init; }
    public TimeSpan Overtime { get; init; }
    public TimeSpan VacationTime { get; init; }
    public UserDailyValuesDto? UserDailyValues { get; init; }
    public IReadOnlyList<ActivityDto> Activities { get; init; } = null!;
}
public record UserDailyValuesDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid TeamId { get; init; }
    public StatusDto Status { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = null!;
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
    public DateOnly ShrinkageDate { get; init; }
}
