using BlazorLayout.Extensions;
using BlazorLayout.Modeles;
using BlazorLayout.StateManagement;

namespace BlazorLayout.Stores;
[AutoSubscribe]
public sealed partial class UserAbsencesStore : StoreBase
{
    [AutoSubscribe]
    public partial IReadOnlyDictionary<Guid, IReadOnlyList<AbsenceDto>> Absences { get; private set; }

    public void InitializeUserAbsences(Guid userId, List<AbsenceDto> absences)
    {
        Absences = new Dictionary<Guid, IReadOnlyList<AbsenceDto>>(__Absences)
        {
            [userId] = absences,
        };
    }

    public void Update(AbsenceDto absence)
    {
        var index = __Absences[absence.UserId].FindIndex(x => x.Id == absence.Id);
        var list = __Absences[absence.UserId];

        list = index is null ? [.. list, absence] : list.WithAt(index.Value, absence);

        Absences = new Dictionary<Guid, IReadOnlyList<AbsenceDto>>(__Absences)
        {
            [absence.UserId] = list.OrderByDescending(x => x.StartDate).ToArray(),
        };
    }

    public void Remove(Guid userId, Guid id)
    {
        var index = __Absences[userId].FindIndex(x => x.Id == id) ?? Utils.Unreachable<int>();
        Absences = new Dictionary<Guid, IReadOnlyList<AbsenceDto>>(__Absences)
        {
            [userId] = __Absences[userId].ExceptAt(index),
        };
    }

    public void Reset()
    {
        Absences = new Dictionary<Guid, IReadOnlyList<AbsenceDto>>();
    }
}


