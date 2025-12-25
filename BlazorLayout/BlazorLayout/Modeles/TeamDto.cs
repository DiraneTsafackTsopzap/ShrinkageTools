namespace BlazorLayout.Modeles;

public record TeamDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public IReadOnlyList<Guid> TeamLeadIds { get; init; } = null!;
}

