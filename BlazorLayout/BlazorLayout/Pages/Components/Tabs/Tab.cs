using BlazorLayout.StateManagement;
using Microsoft.AspNetCore.Components;

namespace BlazorLayout.Pages.Components.Tabs
{
    public class Tab : AppComponentBase
    {
        [CascadingParameter]
        private IHaveTabs Parent { get; init; } = null!;

        [Parameter, EditorRequired]
        public RenderFragment ChildContent { get; init; } = null!;

        [Parameter, EditorRequired]
        public string Label { get; init; } = null!;

        [Parameter]
        public string? Key { get; init; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Parent.Register(this);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            Parent.Unregister(this);
        }
    }
}
