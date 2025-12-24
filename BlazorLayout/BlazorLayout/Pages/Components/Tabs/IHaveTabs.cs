namespace BlazorLayout.Pages.Components.Tabs
{
    public interface IHaveTabs
    {
        void Register(Tab tab);
        void Unregister(Tab tab);
    }
}
