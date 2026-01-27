namespace BlazorLayout.ModelRequest;
    public class SaveUsersShrinkageStatusRequest_M
    {
    public Guid CorrelationId { get; init; }
    public IReadOnlyList<UserDailyValueStatus_M> DailyValueStatuses { get; init; } = null!;
}

