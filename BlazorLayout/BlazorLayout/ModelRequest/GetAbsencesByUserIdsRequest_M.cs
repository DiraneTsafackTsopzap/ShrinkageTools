namespace BlazorLayout.ModelRequest;
   
public class GetAbsencesByUserIdsRequest_M
{
    public Guid CorrelationId { get; init; }
    public IReadOnlyList<Guid> UserIds { get; init; } = null!;
}

