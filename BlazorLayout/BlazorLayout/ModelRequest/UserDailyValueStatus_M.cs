using BlazorLayout.Enums;

namespace BlazorLayout.ModelRequest;
    public class UserDailyValueStatus_M
    {
    public Guid DailyValuesId { get; init; }
    public Guid UserId { get; init; }
    public StatusDto Status { get; init; }
    public string? Comment { get; init; }
}
