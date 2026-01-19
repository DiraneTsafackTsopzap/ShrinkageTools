using BlazorLayout.Enums;

namespace BlazorLayout.ModelRequest;
    public class SaveUserDailyValuesRequest_M
    {
    public Guid CorrelationId { get; init; }
    public Guid UserId { get; init; }
    public Guid TeamId { get; init; }
    public DateOnly ShrinkageDate { get; init; }
    public TimeSpan? PaidTime { get; init; }
    public TimeSpan? Overtime { get; init; }
    public TimeSpan? PaidTimeOff { get; init; } // Freizeitausgleich
    public TimeSpan? VacationTime { get; init; } // Urlaubstunden
    public StatusDto Status { get; init; }

}

