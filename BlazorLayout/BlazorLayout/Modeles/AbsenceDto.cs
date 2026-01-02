using BlazorLayout.Enums;

namespace BlazorLayout.Modeles;
    public record AbsenceDto
    {
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string? UserEmail { get; init; }
    public Guid TeamId { get; init; }
    public AbsenceTypeDto AbsenceType { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = null!;
    public DateTime? UpdatedAt { get; init; }
    public string? UpdatedBy { get; init; }
}

