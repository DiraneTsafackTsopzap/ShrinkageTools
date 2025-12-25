using BlazorLayout.Extensions;
using BlazorLayout.Gateways;
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
        private const int Timeout = 60_000;

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

        [Inject]
        private ShrinkageApi ShrinkageApi { get; init; } = null!;

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
            try
            {
                await base.OnInitializedAsync();

                var authState = await AuthCtx;

                userEmailAddress = authState.User.GetId();

                await ShrinkageApi.EnsureGetUserByEmail(userEmailAddress, forceRefresh: false, TimeoutToken(Timeout));

                isUserLoaded = true;

                if (State.CurrentUser!.TeamId is null)
                {
                    errorMessage = Localizer["shrinkage_error_no_team_assigned"];
                }
                else
                {
                   // Afficher GetPublicHolidays();
                }
            }
            catch (GetUserByEmailException ex)
            {
                errorMessage = Localizer["shrinkage_error_get_user_email"];
                if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                    errorMessage += " " + reason;
            }








        }
    }
}


// Flux correct de l'application :
// API → Store → BuildState → State → UI
//
// 1️⃣ L’API appelle le backend et récupère les données (UserDto)  :  var user = await HttpClient.GetFromJsonAsyncNotNull<UserDto>(url, token);
// 2️⃣ Les données sont stockées dans un Store (source de vérité)  : userByEmailStore.InitializeUser(user);
// 3️⃣ BuildState lit le Store et construit un State immutable     :  CurrentUser = UserStore.User, Placer ds le BuildState
// 4️⃣ L’UI affiche uniquement le State                            : @State.Current.Email pour Afficher

