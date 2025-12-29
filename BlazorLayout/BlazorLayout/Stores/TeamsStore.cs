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
        if (__Teams.Any()) throw new InvalidOperationException("Teams were already been initialized.");

        Teams = teams;
    }

    public void Reset()
    {
        Teams = new List<TeamDto>();
    }
}

// Readme Important : Le AutoSubscribe generer ici provient du BlazorAnalyer et dans le StoreAutoSubscribeGenerator.cs
// S'asssurr que le Nom est BlazorLayout.StateManagement.AutoSubscribeAttribute 
