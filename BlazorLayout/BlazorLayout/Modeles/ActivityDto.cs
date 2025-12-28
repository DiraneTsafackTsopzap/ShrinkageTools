using BlazorLayout.Enums;

namespace BlazorLayout.Modeles;

public record ActivityDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid TeamId { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? StoppedAt { get; init; }
    public ActivityTrackTypeDto ActivityTrackType { get; init; }
    public ActivityTypeDto ActivityType { get; init; }
    public string CreatedBy { get; init; } = null!;
    public string? UpdatedBy { get; init; }
}


