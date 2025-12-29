namespace BlazorLayout.ModelRequest;

    public class DeleteActivityRequest_M
    {
    public Guid CorrelationId { get; init; }
    public Guid ActivityId { get; init; }
    public Guid DeletedBy { get; init; }
}

