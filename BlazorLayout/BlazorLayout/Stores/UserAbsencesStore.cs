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
        // Avant 
        //Console.WriteLine("=== Avant InitializeUserAbsences ===");
        //foreach (var kv in __Absences)
        //{
        //    Console.WriteLine($"User {kv.Key} → {kv.Value.Count} absences");
        //}

        // Initialization : Pour chque UserId , Place la Liste de ses Absences d'ou le UserId = absences
        /// <summary>
        /// Associe un utilisateur à sa liste d’absences dans le Store
        /// (userId → absences) en recréant le dictionnaire pour notifier l’UI.
        /// </summary>


        Absences = new Dictionary<Guid, IReadOnlyList<AbsenceDto>>(__Absences)
        {
            [userId] = absences,
        };

        // APRES
        //Console.WriteLine("=== APRES InitializeUserAbsences ===");
        //foreach (var kv in __Absences)
        //{
        //    Console.WriteLine($"User {kv.Key} → {kv.Value.Count} absences");
        //}
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


//1- Question a Se Poser ici la et Tres Importante : Pour Un User donnee , Quelle sont ses Absences ?

// Chaque User a une Liste d'Absences associees a lui-meme .
