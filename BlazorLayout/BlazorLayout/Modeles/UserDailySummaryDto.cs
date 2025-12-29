using BlazorLayout.Enums;

namespace BlazorLayout.Modeles;

    public record UserDailySummaryDto 
    {
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public StatusDto Status { get; init; }
    public AbsenceTypeDto AbsenceType { get; init; }
    public PublicHolidayDto? PublicHoliday { get; init; }
    public WeekendDto? Weekend { get; init; }

}

