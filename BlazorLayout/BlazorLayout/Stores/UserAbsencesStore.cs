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
        /// <summary>
        /// Rechercher La Position (index) De L'absence Dans La Liste de L'User
        ///  si index = -1 ou null , Absence non Trouve : Ajout
        ///  si index = 0  , Absence Trouve a la Premiere Position
        /// </summary>
        var index = __Absences[absence.UserId].FindIndex(x => x.Id == absence.Id);


         var list = __Absences[absence.UserId];


        /// <summary>
        ///   si index == 0 ou null alors ceci sera execute  list = index is null ? [.. list, absence]
        ///   si non  list.WithAt(index.Value, absence);
        /// </summary>
        /// 

        ///<summary>
        /// WithAt fonctionne comme ceci 
        /// Image Mentale :  Avant [ A , B , C ]
        /// list.WithAt (1, X);  veut dire que index 1 qui correspond a B sera remplace par le nouvel element X
        /// ma liste sera [ A , X , C ]
        /// </summary>

        list = index is null ? [.. list, absence] : list.WithAt(index.Value, absence);

        ///<summary>
        /// Mettre à jour le Store (IMMUTABLEMENT)
        /// On remplace seulement la liste du user concerné  [userId] = absences ds le Initialise devient [absence.userId] = list.OrderBydescending
        /// </summary>
        
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
