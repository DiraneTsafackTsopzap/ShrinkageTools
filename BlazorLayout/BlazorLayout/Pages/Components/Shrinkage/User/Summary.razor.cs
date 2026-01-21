using BlazorLayout.Enums;
using BlazorLayout.Exceptions;
using BlazorLayout.Extensions;
using BlazorLayout.Gateways;
using BlazorLayout.Modeles;
using BlazorLayout.ModelRequest;
using BlazorLayout.Shared;
using BlazorLayout.Stores;
using BlazorLayout.Validators;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Microsoft.JSInterop;
using SystemDateOnly = System.DateOnly;



namespace BlazorLayout.Pages.Components.Shrinkage.User;

    public sealed partial class Summary
    {

    [Parameter, EditorRequired]
    public TimeSpan PaidTime { get; set; }

    [Parameter, EditorRequired]
    public TimeSpan CurrentOvertime { get; set; }

    [Parameter, EditorRequired]
    public TimeSpan CurrentVacationTime { get; set; }

    [Parameter, EditorRequired]
    public TimeSpan CurrentPaidTimeOff { get; set; }

    [Parameter, EditorRequired]
    public SystemDateOnly ShrinkageDate { get; set; }

    [Parameter, EditorRequired]
    public IReadOnlyList<ActivityDto> Activities { get; set; } = null!;


    [Inject]
    private ActivityTimerService TimerService { get; set; } = null!;

    [Inject]
    private TeamsStore TeamsStore { get; init; } = null!;

    [Inject]
    private UserByEmailStore UserByEmailStore { get; init; } = null!;

    [Inject]
    private IStringLocalizer Localizer { get; init; } = null!;



    [Inject]
    private IJSRuntime JsRuntime { get; init; } = null!;


    [Parameter, EditorRequired]
    public UserShrinkageDto? UserShrinkage { get; set; }

    [Parameter, EditorRequired]
    public SystemDateOnly TargetDate { get; set; }



    private string paidTimeOffInput = string.Empty;
    private string overtimeInput = string.Empty;
    private string vacationTimeInput = string.Empty;

    private bool isEditingPaidTimeOff;
    private bool isEditingOvertime;
    private bool isEditingVacationTime;
    private bool popoverInitialized;
    private ActivityTypeDto activeActivityType;
    private ActivityDto? newActivity;

    [Parameter, EditorRequired]
    public Action<bool> OnGlobalEditChanged { get; set; }

    [Parameter, EditorRequired]
    public Func<Task> OnAdjustmentsChanged { get; set; }



    [Parameter, EditorRequired]
    public Action<string?> OnWarning { get; set; }

    [Parameter, EditorRequired]
    public Func<ActivityDto, Task> OnSave { get; set; }

    private string? errorMessage;


    /// <summary>
    /// Grace a ce AnySummaryEditing on va desactiver tous les Autres Boutons ds le Razor
    /// Si L'user clique sur le Bearbeiten de Freizeitausgleich et bien je desactive tous les autres
    /// </summary>
    private bool AnySummaryEditing => isEditingPaidTimeOff || isEditingOvertime || isEditingVacationTime;

    private bool IsTimerActive => TimerService.IsRunning || Activities.Any(a => a.StoppedAt == null);

    [Parameter, EditorRequired]
    public bool IsReadOnly { get; set; }

    private Guid? selectedTeamId;




    [Parameter, EditorRequired]
    public Action<bool> OnTimerStateChanged { get; set; }
    public sealed record StateT
    {
        public UserDto? CurrentUser { get; init; }
        public IReadOnlyList<TeamDto> Teams { get; init; } = null!;
    }

    protected override StateT BuildState() => new()
    {
        CurrentUser = UserByEmailStore.User,
        Teams = TeamsStore.Teams,
    };

    private Dictionary<string, object> StylesAttributes { get; set; } = new()
    {
        ["style"] = "min-width: 90px; text-align: center;"
    };

    protected override void OnInitialized()
    {
        TimerService.OnTick += OnTimerTick;
        selectedTeamId = State.CurrentUser?.TeamId;
    }

    private void OnTimerTick()
    {
     
        _ = InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        TimerService.OnTick -= OnTimerTick;
    }


    private string PaidTimeOffInput
    {
        get => paidTimeOffInput;
        set
        {
            var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length > 4) digits = digits[..4];

            // format as HH:mm while typing
            paidTimeOffInput = digits.Length switch
            {
                >= 3 => $"{digits[..2]}:{digits[2..]}",
                2 => digits,
                1 => digits,
                _ => ""
            };
        }
    }

    private string OvertimeInput
    {
        get => overtimeInput;
        set
        {
            var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length > 4) digits = digits[..4];

            overtimeInput = digits.Length switch
            {
                >= 3 => $"{digits[..2]}:{digits[2..]}",
                2 => digits,
                1 => digits,
                _ => ""
            };
        }
    }

    private string VacationTimeInput
    {
        get => vacationTimeInput;
        set
        {
            var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
            if (digits.Length > 4) digits = digits[..4];

            // format as HH:mm while typing
            vacationTimeInput = digits.Length switch
            {
                >= 3 => $"{digits[..2]}:{digits[2..]}",
                2 => digits,
                1 => digits,
                _ => ""
            };
        }
    }

    private void FormatPaidTimeOffOnBlur()
    {
        if (!isEditingPaidTimeOff || IsReadOnly)
            return;

        var formatted = ShrinkageExtensionsHelper.FormatAsTime(PaidTimeOffInput);
        paidTimeOffInput = formatted;
    }

    private void FormatOvertimeOnBlur()
    {
        if (!isEditingOvertime || IsReadOnly)
            return;

        var formatted = ShrinkageExtensionsHelper.FormatAsTime(overtimeInput);
        overtimeInput = formatted;
    }

    private void FormatVacationTimeOnBlur()
    {
        if (  !isEditingVacationTime || IsReadOnly)
            return;

        var formatted = ShrinkageExtensionsHelper.FormatAsTime(vacationTimeInput);
        vacationTimeInput = formatted;
    }

    private void StartEditPaid()
    {
        OnWarning(null);
        if ( IsReadOnly) return;
        if (UnfinishedActivityExist()) return;
        isEditingPaidTimeOff = true;
        isEditingOvertime = false;
        isEditingVacationTime = false;
        OnGlobalEditChanged(true);
    }

    private void StartEditOvertime()
    {
        OnWarning(null);
        if (IsReadOnly) return;
        if (UnfinishedActivityExist()) return;
        isEditingPaidTimeOff = false;
        isEditingOvertime = true;
        isEditingVacationTime = false;
        OnGlobalEditChanged(true);
    }

    private void StartEditVacationTime()
    {
        OnWarning(null);
        if (IsReadOnly) return;
        if (UnfinishedActivityExist()) return;
        isEditingPaidTimeOff = false;
        isEditingOvertime = false;
        isEditingVacationTime = true;
        OnGlobalEditChanged(true);
    }
    private async Task StartTimer(ActivityTypeDto activityType)
    {
      
        OnWarning(null);

        if (State?.CurrentUser is null)
        {
            OnWarning("Current user not loaded yet.");
            return;
        }

        if (!selectedTeamId.HasValue || selectedTeamId.Value == Guid.Empty)
        {
            OnWarning(Localizer["shrinkage_message_select_team"]);
            return;
        }

        if (Activities.Any(x => x.StoppedAt == null))
        {
            errorMessage = Localizer["shrinkage_error_activity_running"];
            OnWarning(errorMessage);
            return;
        }

        var remainingTime = GetAdjustedRemainingTime();
        if (remainingTime == TimeSpan.Zero)
        {
            errorMessage = Localizer["shrinkage_warning_zero_remaining_time"];
            OnWarning(errorMessage);
            return;
        }

        TimerService.Reset();
        activeActivityType = activityType;
        TimerService.Start();

        if (TimerService.StartTime is null)
        {
            OnWarning("TimerService.StartTime is null after Start().");
            TimerService.Reset();
            return;
        }

        OnTimerStateChanged(true);

        newActivity = new ActivityDto
        {
            Id = Guid.NewGuid(),
            UserId = State.CurrentUser.UserId,
            TeamId = selectedTeamId.Value,
            StartedAt = new DateTimeOffset(
                ShrinkageDate.ToDateTime(TimerService.StartTime.Value),
                DateTimeOffset.Now.Offset),
            ActivityTrackType = ActivityTrackTypeDto.Timer,
            ActivityType = activeActivityType,
            CreatedBy = State.CurrentUser.Email,
        };

        var overlapMessage = ActivityValidator.ValidateOverlap(Activities, newActivity);
        if (overlapMessage != null)
        {
            TimerService.Reset();
            OnTimerStateChanged(false);
            OnWarning(overlapMessage);
            return;
        }

        TimerService.StartActivity(activityType, newActivity);
         await OnSave(newActivity);
        StateHasChanged();
    }


    private async Task StopTimer()
    {
        TimerService.Stop();
        TimerService.StopActivity();
        OnTimerStateChanged(false);
        if (newActivity != null)
        {
            if (Activities.Any(x => x.StoppedAt == null && x.Id == newActivity.Id))
            {
                newActivity = newActivity with { UpdatedBy = State.CurrentUser!.Email };
            }

            var duration = TimerService.StopTime - TimeOnly.FromDateTime(newActivity.StartedAt.DateTime);
            var remainingTime = GetAdjustedRemainingTime();

            if (remainingTime < duration)
                newActivity = newActivity with
                {
                    StartedAt = new DateTimeOffset(newActivity.StartedAt.DateTime, DateTimeOffset.Now.Offset),
                    StoppedAt = new DateTimeOffset(newActivity.StartedAt.DateTime.Add(remainingTime), DateTimeOffset.Now.Offset),
                };
            else
                newActivity = newActivity with
                {
                    StartedAt = new DateTimeOffset(newActivity.StartedAt.DateTime, DateTimeOffset.Now.Offset),
                    StoppedAt = new DateTimeOffset(ShrinkageDate.ToDateTime(TimerService.StopTime!.Value), DateTimeOffset.Now.Offset),
                };

             await OnSave(newActivity);
            activeActivityType = ActivityTypeDto.Unspecified;
            newActivity = null;
        }

        selectedTeamId = State.CurrentUser?.TeamId;
        StateHasChanged();
    }
    private TimeSpan GetAdjustedRemainingTime()
    {
        return TimeCalculator.GetRemainingTime(PaidTime, CurrentOvertime, CurrentVacationTime, CurrentPaidTimeOff, Activities);
    }
    private async Task SavePaidTimeOffAsync()
    {
        errorMessage = string.Empty;
        StateHasChanged();

        if (!TimeSpan.TryParse(paidTimeOffInput, out var newPaidTimeOff))
        {
            errorMessage = Localizer["shrinkage_warning_invalid_time_format"];
            OnWarning(errorMessage);
            OnGlobalEditChanged(false);
            return;
        }

        var remaining = GetAdjustedRemainingTime();

        if (!AdditionalTimeValidator.CheckIfVacationTimeOrPaidTimeOffCanBeModified(CurrentPaidTimeOff, remaining, newPaidTimeOff, @Localizer["shrinkage_label_paid_time_off"], out var err))
        {
            errorMessage = err;
            OnWarning(errorMessage);
            OnGlobalEditChanged(false);
            return;
        }

        var request = new SaveUserDailyValuesRequest_M
        {
            CorrelationId = Guid.NewGuid(),
            UserId = State.CurrentUser!.UserId,
            TeamId = State.CurrentUser!.TeamId!.Value,
            ShrinkageDate = TargetDate,
            PaidTimeOff = newPaidTimeOff,
        };
        try
        {
            //await ShrinkageApi.SaveUserDailyValuesForUserAsync(request, TimeoutToken(Timeout));
            CurrentPaidTimeOff = newPaidTimeOff;
            isEditingPaidTimeOff = false;
            paidTimeOffInput = CurrentPaidTimeOff.FormatTimeSpanToHhMm();
            await OnAdjustmentsChanged();
            StateHasChanged();
            OnGlobalEditChanged(false);
        }
        catch (ConflictException ex)
        {
            errorMessage = Localizer["shrinkage_error_save_user_daily_value_conflict"];
            if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                errorMessage += " " + reason;
            OnWarning(errorMessage);
        }
        catch (OperationCanceledException) when (IsDisposing) { }
        catch (Exception ex)
        {
            errorMessage = Localizer["shrinkage_error_save_user_daily_value"];
            if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                errorMessage += " " + reason;
            OnWarning(errorMessage);
        }
    }

    private async Task SaveOvertimeAsync()
    {
        errorMessage = string.Empty;
        StateHasChanged();
        if (!TimeSpan.TryParse(overtimeInput, out var newOvertime))
        {
            errorMessage = Localizer["shrinkage_warning_invalid_time_format"];
            OnWarning(errorMessage);
            OnGlobalEditChanged(false);
            StateHasChanged();
            return;
        }

        if (!AdditionalTimeValidator.CheckIfOverTimeBeModified(PaidTime, newOvertime, out var err))
        {
            errorMessage = err;
            OnWarning(errorMessage);
            OnGlobalEditChanged(false);
            return;
        }

        var request = new SaveUserDailyValuesRequest_M
        {
            CorrelationId = Guid.NewGuid(),
            UserId = State.CurrentUser!.UserId,
            TeamId = State.CurrentUser!.TeamId!.Value,
            ShrinkageDate = TargetDate,
            Overtime = newOvertime,
        };
        try
        {
           // await ShrinkageApi.SaveUserDailyValuesForUserAsync(request, TimeoutToken(Timeout));
            CurrentOvertime = newOvertime;
            isEditingOvertime = false;
            overtimeInput = CurrentOvertime.FormatTimeSpanToHhMm();
            await OnAdjustmentsChanged();
            OnGlobalEditChanged(false);
            StateHasChanged();
        }
        catch (ConflictException ex)
        {
            errorMessage = Localizer["shrinkage_error_save_user_daily_value_conflict"];
            if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                errorMessage += " " + reason;
            OnWarning(errorMessage);
        }
        catch (OperationCanceledException) when (IsDisposing) { }

        catch (Exception ex)
        {
            errorMessage = Localizer["shrinkage_error_save_user_daily_value"];
            if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                errorMessage += " " + reason;
            OnWarning(errorMessage);
        }
    }
    private bool UnfinishedActivityExist()
    {
        if (Activities.Any(x => x.StoppedAt == null))
        {
            errorMessage = Localizer["shrinkage_error_activity_running"];
            OnWarning(errorMessage);
            return true;
        }

        return false;
    }

    private async Task SaveVacationTimeAsync()
    {
        errorMessage = string.Empty;
        StateHasChanged();
        if (!TimeSpan.TryParse(vacationTimeInput, out var newVacationTime))
        {
            errorMessage = Localizer["shrinkage_warning_invalid_time_format"];
            OnWarning(errorMessage);
            OnGlobalEditChanged(false);
            return;
        }

        var remaining = GetAdjustedRemainingTime();
        if (!AdditionalTimeValidator.CheckIfVacationTimeOrPaidTimeOffCanBeModified(CurrentVacationTime, remaining, newVacationTime, Localizer["shrinkage_label_vacation_time"], out var err))
        {
            errorMessage = err;
            OnWarning(errorMessage);
            OnGlobalEditChanged(false);
            return;
        }

        var request = new SaveUserDailyValuesRequest_M
        {
            CorrelationId = Guid.NewGuid(),
            UserId = State.CurrentUser!.UserId,
            TeamId = State.CurrentUser!.TeamId!.Value,
            ShrinkageDate = TargetDate,
            VacationTime = newVacationTime,
        };
        try
        {
            //await ShrinkageApi.SaveUserDailyValuesForUserAsync(request, TimeoutToken(Timeout));
            CurrentVacationTime = newVacationTime;
            isEditingVacationTime = false;
            vacationTimeInput = CurrentVacationTime.FormatTimeSpanToHhMm();
            await OnAdjustmentsChanged();
            StateHasChanged();
            OnGlobalEditChanged(false);
        }
        catch (ConflictException ex)
        {
            errorMessage = Localizer["shrinkage_error_save_user_daily_value_conflict"];
            if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                errorMessage += " " + reason;
            OnWarning(errorMessage);
        }
        catch (OperationCanceledException) when (IsDisposing) { }

        catch (Exception ex)
        {
            errorMessage = Localizer["shrinkage_error_save_user_daily_value"];
            if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                errorMessage += " " + reason;
            OnWarning(errorMessage);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JsRuntime.InvokeVoidAsync("eval", @"
            const triggers = document.querySelectorAll('[data-bs-toggle=""popover""]');

triggers.forEach(el => {
    const contentId = el.getAttribute('data-content-id');
    const contentEl = contentId ? document.getElementById(contentId) : null;
    const content = contentEl ? contentEl.innerHTML : '';
    const title = el.getAttribute('title') || '';

    new bootstrap.Popover(el, {
        trigger: 'hover',
        html: true,
        container: 'body',
        title: title,
        content: content
    });
});
");
        }
    }


}

