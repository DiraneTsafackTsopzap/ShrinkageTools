namespace BlazorLayout.Modeles;

    public class SaveActivityDto
    {
    public Guid CorrelationId { get; init; }
    public ActivityDto Activity { get; init; } = null!;
}

