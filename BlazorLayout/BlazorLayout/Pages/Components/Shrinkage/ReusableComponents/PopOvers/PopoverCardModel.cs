namespace BlazorLayout.Pages.Components.Shrinkage.ReusableComponents.PopOvers;

    public class PopoverCardModel
{
    public string Id { get; init; } = null!;
    public string Title { get; init; } = null!;
    public string Description { get; init; } = null!;
    public List<string>? Items { get; init; }
    public string? Warning { get; init; }
}

