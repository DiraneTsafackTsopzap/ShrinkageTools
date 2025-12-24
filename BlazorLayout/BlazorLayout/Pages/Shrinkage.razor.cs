using BlazorLayout.Modeles;
using BlazorLayout.Stores;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Localization;

namespace BlazorLayout.Pages
{
    public partial class Shrinkage
    {
        private static string? errorMessage;
        private bool isUserLoaded;
        private string userEmailAddress = null!;

        [CascadingParameter]
        private Task<AuthenticationState> AuthCtx { get; init; } = null!;

        [Inject]
        private IStringLocalizer Localizer { get; init; } = null!;

        [Parameter, EditorRequired]
        public string? ActiveTab { get; set; }
        [Inject]
        private NavigationManager NavigationManager { get; init; } = null!;

        [Inject]
        public UserByEmailStore UserStore { get; set; } = null!;

        private void SwitchTab(string tabName)
        {
            NavigationManager.NavigateRelativeToBaseUri($"shrinkage/{tabName}", forceLoad: false, replace: ActiveTab is null);
        }
        public sealed record StateT
        {
            public UserDto? CurrentUser { get; init; }
        }

        protected override StateT BuildState()
        {
            return new StateT
            {
                CurrentUser = UserStore.User,
            };
        }


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            var authState = await AuthCtx;

            userEmailAddress = authState.User.GetId();

            // await ShrinkageApi.EnsureGetUserByEmail(userEmailAddress, forceRefresh: false, TimeoutToken(Timeout));
            // 🔴 FAKE USER (remplace l'appel API)
            UserStore.InitializeUser(new UserDto
            {
                Email = userEmailAddress,
                TeamId = Guid.NewGuid(), // IMPORTANT : sinon erreur "no team"
                PaidTimeList = []
            });


            isUserLoaded = true;


        }
    }
}
