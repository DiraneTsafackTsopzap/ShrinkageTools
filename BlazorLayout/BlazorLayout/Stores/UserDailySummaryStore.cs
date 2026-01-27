using BlazorLayout.Enums;
using BlazorLayout.Extensions;
using BlazorLayout.Modeles;
using BlazorLayout.Pages.Components.Shrinkage.ReusableComponents.MeinModal;
using BlazorLayout.StateManagement;

namespace BlazorLayout.Stores;

[AutoSubscribe]
public sealed partial class UserDailySummaryStore : StoreBase
{
    [AutoSubscribe]
    public partial IReadOnlyList<UserDailySummaryDto> Summaries { get; private set; }

    private static readonly DateOnly displayStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
    private static readonly DateOnly displayEndDate = DateOnly.FromDateTime(DateTime.UtcNow);
    private static readonly DateOnly today = DateOnly.FromDateTime(DateTime.Today);
    public void InitializeSummary(IReadOnlyList<UserDailySummaryDto> userSummaries)
    {
        if (__Summaries.Any()) throw new InvalidOperationException("Summary was already initialized");

        var userDailySummaryItems = userSummaries.ToList();


        //1- Stocke le resultat de notre API en triant par date décroissante ds la liste Summaries
        Summaries = userDailySummaryItems.OrderByDescending(x => x.Date).ToList();
    }


    public void UpdateStatusBasedOnDate(DateOnly date, IReadOnlyList<PublicHolidayDto> publicHolidays)
    {
        var index = __Summaries.FindIndex(x => x.Date == date) ?? Utils.Unreachable<int>();

        var list = __Summaries.ToArray();

#if DEBUG
        Utils.Assert(!list[(index + 1)..].Any(x => x.Date == date));
#endif

        var item = __Summaries[index];

        if ((item.Weekend != null && item.Status == StatusDto.Unspecified) || (item.PublicHoliday != null && item.Status == StatusDto.Unspecified))
        {
            item = item with
            {
                Status = StatusDto.Open,
            };
            list[index] = item;
        }
        else if (item.AbsenceType != AbsenceTypeDto.Unspecified)
        {
            var deletedAbsences = list.Where(x => x.Date >= item.Date && x.Id == item.Id).OrderBy(x => x.Date).ToList();

            foreach (var deletedAbsence in deletedAbsences)
            {
                var indexOfDeletedAbsence = __Summaries.FindIndex(x => x.Date == deletedAbsence.Date) ?? Utils.Unreachable<int>();
                var deletedItem = __Summaries[indexOfDeletedAbsence];

                if (publicHolidays != null && publicHolidays.Any(x => x.AffectedDate == deletedAbsence.Date))
                {
                    var publicHoliday = publicHolidays.Single(x => x.AffectedDate == deletedAbsence.Date);
                    if (deletedAbsences.First().Date == deletedAbsence.Date)
                    {
                        deletedItem = deletedAbsence with
                        {
                            PublicHoliday = new PublicHolidayDto
                            {
                                Id = publicHoliday.Id,
                                Title = publicHoliday.Title,
                                AffectedDate = publicHoliday.AffectedDate,
                            },
                            AbsenceType = AbsenceTypeDto.Unspecified,
                            Status = StatusDto.Open,
                        };
                    }
                    else
                    {
                        deletedItem = deletedItem with
                        {
                            AbsenceType = AbsenceTypeDto.Unspecified,
                            PublicHoliday = new PublicHolidayDto
                            {
                                Id = publicHoliday.Id,
                                Title = publicHoliday.Title,
                                AffectedDate = publicHoliday.AffectedDate,
                            },
                            Status = StatusDto.Unspecified,
                        };
                    }
                }
                else if (deletedAbsence.Date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    if (deletedAbsences.First().Date == deletedAbsence.Date)
                    {
                        deletedItem = deletedItem with
                        {
                            AbsenceType = AbsenceTypeDto.Unspecified,
                            Weekend = new WeekendDto(),
                            Status = StatusDto.Open,
                        };
                    }
                    else
                    {
                        deletedItem = deletedItem with
                        {
                            AbsenceType = AbsenceTypeDto.Unspecified,
                            Weekend = new WeekendDto(),
                            Status = StatusDto.Unspecified,
                        };
                    }
                }
                else
                {
                    deletedItem = deletedItem with
                    {
                        AbsenceType = AbsenceTypeDto.Unspecified,
                        Status = StatusDto.Open,
                    };
                }

                list[indexOfDeletedAbsence] = deletedItem;
            }
        }

        Summaries = list.OrderByDescending(x => x.Date).ToArray();
    }

    public void UpdateStatus(Guid id, StatusDto newStatus)
    {
        var index = __Summaries.FindIndex(x => x.Id == id) ?? Utils.Unreachable<int>();

        var list = __Summaries.ToArray();

#if DEBUG
        Utils.Assert(!list[(index + 1)..].Any(x => x.Id == id));
#endif

        var item = list[index];

        if (item is not { AbsenceType: AbsenceTypeDto.Unspecified })
        {
            return;
        }

        if (item.Date <= displayStartDate && item.Status == StatusDto.Transferred)
        {
            Summaries = __Summaries.ExceptAt(index);
        }
        else
        {
            item = item with { Status = newStatus };
            list[index] = item;
            Summaries = list;
        }
    }
    public void UpdateIdBasedOnDate(Guid id, DateOnly date)
    {
        var index = __Summaries.FindIndex(x => x.Date == date) ?? Utils.Unreachable<int>();

        var list = __Summaries.ToArray();

#if DEBUG
        Utils.Assert(!list[(index + 1)..].Any(x => x.Date == date));
#endif

        var updatedSummary = list[index];
        updatedSummary = updatedSummary with { Id = id };
        list[index] = updatedSummary;
        Summaries = list;
    }
    public void Reset()
    {
        Summaries = new List<UserDailySummaryDto>();
    }

    public void AddAbsenceRange(Guid absenceId, AbsenceTypeDto absenceType, DateOnly startInclusive, DateOnly endInclusive)
    {
        var start = startInclusive;
        var end = endInclusive;
        if (end < start) (start, end) = (end, start);

        var list = __Summaries.ToArray();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            bool inWindow = date >= displayStartDate && date <= displayEndDate;
            if (inWindow)
            {
                var index = __Summaries.FindIndex(x => x.Date == date) ?? Utils.Unreachable<int>();

#if DEBUG
                Utils.Assert(!list[(index + 1)..].Any(x => x.Date == date));
#endif

                var item = __Summaries[index];

                item = item with
                {
                    Id = absenceId,
                    Date = date,
                    Status = StatusDto.Unspecified,
                    AbsenceType = absenceType,
                };
                list[index] = item;
            }
        }

        Summaries = list.OrderByDescending(x => x.Date).ToArray();
    }

    public void RemoveAbsence(DateOnly startDate, DateOnly endDate, IReadOnlyList<PublicHolidayDto> publicHolidays)
    {
        var start = startDate;
        var end = endDate;
        if (end < start) (start, end) = (end, start);
        var newIdForDeletedAbsence = Guid.NewGuid();

        var list = __Summaries.ToArray();
        for (var date = start; date <= end; date = date.AddDays(1))
        {
            if (date > today)
            {
                continue;
            }

            var index = __Summaries.FindIndex(x => x.Date == date) ?? Utils.Unreachable<int>();
#if DEBUG
            Utils.Assert(!list[(index + 1)..].Any(x => x.Date == date));
#endif
            var item = __Summaries[index];

            if (date == today)
            {
                item = item with
                {
                    Id = newIdForDeletedAbsence,
                    Date = date,
                    Status = StatusDto.Open,
                };
            }
            else
            {
                var publicHoliday = publicHolidays.SingleOrDefault(x => x.AffectedDate == date);
                if (publicHoliday is not null)
                {
                    item = item with
                    {
                        Id = newIdForDeletedAbsence,
                        Date = date,
                        PublicHoliday = new PublicHolidayDto
                        {
                            Id = publicHoliday.Id,
                            Title = publicHoliday.Title,
                            AffectedDate = publicHoliday.AffectedDate,
                        },
                        Status = StatusDto.Unspecified,
                    };
                }
                else if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    item = item with
                    {
                        Id = newIdForDeletedAbsence,
                        Date = date,
                        Weekend = new WeekendDto(),
                        Status = StatusDto.Unspecified,
                    };
                }
                else
                {
                    item = item with
                    {
                        Id = newIdForDeletedAbsence,
                        Date = date,
                        Status = StatusDto.Open,
                    };
                }
            }

            list[index] = item;
            Summaries = list.OrderByDescending(x => x.Date).ToArray();
        }
    }


}

