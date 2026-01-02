using BlazorLayout.Enums;
using BlazorLayout.Extensions;
using BlazorLayout.Gateways;
using BlazorLayout.Modeles;
using BlazorLayout.Shared;
using BlazorLayout.Stores;
using BlazorLayout.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System.Xml.Linq;
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
        public IReadOnlyDictionary<Guid, IReadOnlyDictionary<SystemDateOnly, UserShrinkageDto>> UserShrinkages { get; init; } = null!;


        [Inject]
        private UserDailySummaryStore UserDailySummaryStore { get; init; } = null!;

        [Inject]
        private UserShrinkageStore UserShrinkageStore { get; init; } = null!;

        [Parameter, EditorRequired]
        public UserDailySummaryDto? SelectedDailySummaryRow { get; set; }


        private string? errorMessage;
        private string? warningMessage;
        private bool isLoading;
        public UserShrinkageDto? UserShrinkage { get; init; }
        private UserShrinkageDto? userShrinkage;
        private static StatusDto displayStatus = StatusDto.Open;
        private Guid? currentDailyValuesId;
        private SystemDateOnly ShrinkageDate { get; set; } = SystemDateOnly.FromDateTime(DateTime.UtcNow);
        private string DayNameDe => ShrinkageDate.GetDayName();

        private TimeSpan userPaidTime = TimeSpan.Zero;
        private TimeSpan userOvertime = TimeSpan.Zero;
        private TimeSpan userVacationTime = TimeSpan.Zero;
        private TimeSpan userPaidTimeOff = TimeSpan.Zero;
        private string? comment;
        private const int Timeout = 30_000;
        public sealed record StateT
        {
            public UserDto? CurrentUser { get; init; }
            public IReadOnlyList<TeamDto> Teams { get; init; } = [];
            public IReadOnlyList<UserDailySummaryDto> Summaries { get; init; } = null!;

            public IReadOnlyDictionary<Guid, IReadOnlyDictionary<SystemDateOnly, UserShrinkageDto>> UserShrinkages { get; init; } = null!;
        }

        protected override StateT BuildState()
        {
            return new StateT
            {
                CurrentUser = UserByEmailStore.User,
                Teams = TeamsStore.Teams ?? [],
                Summaries = UserDailySummaryStore.Summaries,

                UserShrinkages = UserShrinkageStore.UsersShrinkages,
            };
        }


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            await GetUserDailySummary(false);
            ExtensionsHelper.Localizer = Localizer;

            SelectedDailySummaryRow = State.Summaries.FirstOrDefault(x => x.Date == ShrinkageDate);

            if (State.Summaries.Any(x => x.Date == ShrinkageDate && x.Status != StatusDto.Unspecified))
            {
                await LoadUserShrinkageForDateAsync(ShrinkageDate, false);

                var paidTimeForDay = userShrinkage?.PaidTime ?? TimeSpan.Zero;
                var overTimeForDay = userShrinkage?.Overtime ?? TimeSpan.Zero;
                if (paidTimeForDay == TimeSpan.Zero && overTimeForDay == TimeSpan.Zero)
                {
                    warningMessage = Localizer["shrinkage_warning_paid_time_is_zero_D", ShrinkageDate.GetDayName()];
                    return;
                }
            }
        }

        private void HandleWarning(string? message)
        {
            warningMessage = message;
            StateHasChanged();
        }

        private async Task LoadUserShrinkageForDateAsync(SystemDateOnly date, bool forceRefresh)
        {
            try
            {
                errorMessage = null;
                warningMessage = null;
                var user = State.CurrentUser;
                ShrinkageDate = date;

                await ShrinkageApi.EnsureGetUserShrinkage(date, user!.UserId, forceRefresh, TimeoutToken(Timeout));
                State.UserShrinkages[State.CurrentUser!.UserId].TryGetValue(ShrinkageDate, out userShrinkage);
                displayStatus = userShrinkage is { UserDailyValues: not null } ? userShrinkage.UserDailyValues.Status : StatusDto.Open;
                currentDailyValuesId = userShrinkage is { UserDailyValues: not null } ? userShrinkage.UserDailyValues.Id : Guid.Empty;
                userPaidTime = userShrinkage?.PaidTime ?? TimeSpan.Zero;
                userOvertime = userShrinkage?.Overtime ?? TimeSpan.Zero;
                userVacationTime = userShrinkage?.VacationTime ?? TimeSpan.Zero;
                userPaidTimeOff = userShrinkage?.PaidTimeOff ?? TimeSpan.Zero;
                comment = userShrinkage is { UserDailyValues: not null } ? userShrinkage.UserDailyValues.Comment : null;
                var usedTime = TimeSpan.Zero;
                foreach (var a in UserActivities)
                {
                    if (a.StoppedAt.HasValue)
                    {
                        usedTime += a.StoppedAt.Value - a.StartedAt;
                    }
                }

                StateHasChanged();
            }
            catch (Exception ex)
            {
                errorMessage = Localizer["shrinkage_error_get_user_shrinkage"];
                if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                    errorMessage += " " + reason;
            }
        }

        private void HandleError(string? message)
        {
            errorMessage = message;
            StateHasChanged();
        }

        private async Task GetUserDailySummary(bool forceRefresh)
        {
            try
            {
                await ShrinkageApi.EnsureGetUserDailySummary(State.CurrentUser!.UserId, forceRefresh, TimeoutToken(Timeout));
            }
            catch (Exception ex)
            {
                errorMessage = Localizer["shrinkage_error_get_user_daily_summary"];
                if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                    errorMessage += " " + reason;
            }
        }
    }
}
