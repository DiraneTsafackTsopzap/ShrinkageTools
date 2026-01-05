using BlazorLayout.Modeles;
using BlazorLayout.StateManagement;

namespace BlazorLayout.Stores;

[AutoSubscribe]
public sealed partial class TeamsStore : StoreBase
{
    [AutoSubscribe]
    public partial IReadOnlyList<TeamDto> Teams { get; private set; } 

    public void InitializeTeams(IReadOnlyList<TeamDto> teams)
    {
        // Console.WriteLine($"__Teams count Before Initialisation = {__Teams.Count}"); // visible dans la console navigateur

        if (__Teams.Any()) throw new InvalidOperationException("Teams were already been initialized.");

        Teams = teams;

       //  Console.WriteLine($"__Teams count after Initialisation = {__Teams.Count}");
    }

    public void Reset()
    {
        Teams = new List<TeamDto>();
    }
}

// Readme Important : Le AutoSubscribe generer ici provient du BlazorAnalyer et dans le StoreAutoSubscribeGenerator.cs
// S'asssurr que le Nom est BlazorLayout.StateManagement.AutoSubscribeAttribute 

// Teams.Any() : La liste contient t'elle des elements ?  
//  List<int> numbers = new List<int> { 1, 2, 3 };
//  bool hasElements = numbers.Any(); // true
//  List<int> emptyNumbers = new List<int>();
//  bool hasNoElements = emptyNumbers.Any(); // false

// Ce Store permet de Stocker la liste des equipes (Teams) dans l'application BlazorLayout.

