using BlazorLayout.Extensions;
using BlazorLayout.Modeles;
using BlazorLayout.StateManagement;

namespace BlazorLayout.Stores;

[AutoSubscribe]
public sealed partial class UserDailySummaryStore : StoreBase
{
    [AutoSubscribe]
    public partial IReadOnlyList<UserDailySummaryDto> Summaries { get; private set; }

    public void InitializeSummary(IReadOnlyList<UserDailySummaryDto> userSummaries)
    {
        if (__Summaries.Any()) throw new InvalidOperationException("Summary was already initialized");

        var userDailySummaryItems = userSummaries.ToList();


        //1- Stocke le resultat de notre API en triant par date décroissante ds la liste Summaries
        Summaries = userDailySummaryItems.OrderByDescending(x => x.Date).ToList();
    }

    public void Reset()
    {
        Summaries = new List<UserDailySummaryDto>();
    }


}

