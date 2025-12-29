namespace BlazorLayout.ModelRequest;
public class GetUserShrinkageRequest_M
{
    public Guid CorrelationId { get; init; }
    public Guid UserId { get; init; }
    public DateOnly ShrinkageDate { get; init; }
}


