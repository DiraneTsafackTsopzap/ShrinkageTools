using BlazorLayout.Enums;
using BlazorLayout.Extensions;
using BlazorLayout.Modeles;
using BlazorLayout.StateManagement;

namespace BlazorLayout.Stores;

[AutoSubscribe]
public sealed partial class UserDailySummaryStore : StoreBase
{
    [AutoSubscribe]
    public partial IReadOnlyList<UserDailySummaryDto> Summaries { get; private set; }

    private static readonly DateOnly displayStartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
    private static readonly DateOnly displayEndDate = DateOnly.FromDateTime(DateTime.UtcNow);
    public void InitializeSummary(IReadOnlyList<UserDailySummaryDto> userSummaries)
    {
        if (__Summaries.Any()) throw new InvalidOperationException("Summary was already initialized");

        var userDailySummaryItems = userSummaries.ToList();


        //1- Stocke le resultat de notre API en triant par date décroissante ds la liste Summaries
        Summaries = userDailySummaryItems.OrderByDescending(x => x.Date).ToList();
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


}

