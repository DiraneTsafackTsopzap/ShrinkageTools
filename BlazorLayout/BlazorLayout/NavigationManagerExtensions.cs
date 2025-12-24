using Microsoft.AspNetCore.Components;

namespace BlazorLayout
{
    public static class NavigationManagerExtensions
    {
        public static void NavigateRelativeToBaseUri(this NavigationManager navigationManager, string uri, bool forceLoad = false, bool replace = false)
        {
            ArgumentNullException.ThrowIfNull(navigationManager);
            ArgumentNullException.ThrowIfNull(uri);
            var absoluteUri = new Uri(new(navigationManager.BaseUri), uri.StartsWith('/') ? uri[1..] : uri).ToString();
            if (!navigationManager.Uri.Equals(absoluteUri, StringComparison.Ordinal))
                navigationManager.NavigateTo(absoluteUri, forceLoad, replace);
        }
    }

}
