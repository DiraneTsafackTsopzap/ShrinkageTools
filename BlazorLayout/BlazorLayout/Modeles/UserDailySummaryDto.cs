using BlazorLayout.Enums;

namespace BlazorLayout.Modeles;

    public class UserDailySummaryDto 
    {
    public Guid Id { get; init; }
    public DateOnly Date { get; init; }
    public StatusDto Status { get; init; }
    public AbsenceTypeDto AbsenceType { get; init; }
    public PublicHolidayDto? PublicHoliday { get; init; }
    public WeekendDto? Weekend { get; init; }

}

