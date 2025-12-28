using BlazorLayout.Gateways;
using BlazorLayout.Modeles;
using BlazorLayout.Shared;
using BlazorLayout.Stores;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using SystemDateOnly = System.DateOnly;
namespace BlazorLayout.Pages.Components.Shrinkage.User
{
    public sealed partial class ManageActivities
    {
        [Inject]
        private IStringLocalizer Localizer { get; init; } = null!;

        [Inject]
        private UserByEmailStore UserByEmailStore { get; init; } = null!;

        [Inject]
        private ShrinkageApi ShrinkageApi { get; init; } = null!;

        [Inject]
        private TeamsStore TeamsStore { get; init; } = null!;

        private IReadOnlyList<ActivityDto> UserActivities => userShrinkage?.Activities.ToList() ?? [];


        private string? errorMessage;
        private string? warningMessage;
        private bool isLoading;
        private UserShrinkageDto? userShrinkage;
        private SystemDateOnly ShrinkageDate { get; set; } = SystemDateOnly.FromDateTime(DateTime.UtcNow);
        private string DayNameDe => ShrinkageDate.GetDayName();

        private TimeSpan userPaidTime = TimeSpan.Zero;
        private TimeSpan userOvertime = TimeSpan.Zero;
        private TimeSpan userVacationTime = TimeSpan.Zero;
        private TimeSpan userPaidTimeOff = TimeSpan.Zero;
        public sealed record StateT
        {
            public UserDto? CurrentUser { get; init; }
            public IReadOnlyList<TeamDto> Teams { get; init; } = [];
            //public IReadOnlyList<ActivityType> ActivityTypes { get; init; } = [];
        }

        protected override StateT BuildState()
        {
            return new StateT
            {
                CurrentUser = UserByEmailStore.User,
                Teams = TeamsStore.Teams ?? [],
            };
        }


        private void HandleWarning(string? message)
        {
            warningMessage = message;
            StateHasChanged();
        }
    }
}
