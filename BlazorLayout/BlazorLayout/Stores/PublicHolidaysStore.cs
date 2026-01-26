using BlazorLayout.Modeles;
using BlazorLayout.StateManagement;

namespace BlazorLayout.Stores;

[AutoSubscribe]
public sealed partial class PublicHolidaysStore : StoreBase
{
    [AutoSubscribe]
    private partial IReadOnlyDictionary<Guid, IReadOnlyList<PublicHolidayDto>> PublicHolidays { get; set; }

    public IReadOnlyList<PublicHolidayDto> GetPublicHolidaysForTeamId(Guid teamId)
    {
        if (!__PublicHolidays.TryGetValue(teamId, out var publicHolidays))
            throw new InvalidOperationException("Public holidays for this teamId were not initialized");

        return publicHolidays;
    }

    public void InitializePublicHolidays(Guid teamId, IReadOnlyList<PublicHolidayDto> publicHolidays)
    {
        if (__PublicHolidays.ContainsKey(teamId)) throw new InvalidOperationException("Public holidays for this teamId were already initialized");

        PublicHolidays = new Dictionary<Guid, IReadOnlyList<PublicHolidayDto>>(__PublicHolidays)
        {
            [teamId] = publicHolidays,
        };
    }

    public void Reset()
    {
        PublicHolidays = new Dictionary<Guid, IReadOnlyList<PublicHolidayDto>>();
    }
}
