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

    [Parameter, EditorRequired]
    public bool IsEditing { get; set; }


    [Parameter, EditorRequired]
    public UserShrinkageDto? UserShrinkage { get; set; }

    [Parameter, EditorRequired]
    public SystemDateOnly TargetDate { get; set; }

    [Parameter, EditorRequired]
    public bool IsEditingDisabled { get; set; }

    private string paidTimeOffInput = string.Empty;
    private string overtimeInput = string.Empty;
    private string vacationTimeInput = string.Empty;

    private bool isEditingPaidTimeOff;
    private bool isEditingOvertime;
    private bool isEditingVacationTime;

    [Parameter, EditorRequired]
    public Action<bool> OnGlobalEditChanged { get; set; }

    [Parameter, EditorRequired]
    public Func<Task> OnAdjustmentsChanged { get; set; }



    [Parameter, EditorRequired]
    public Action<string?> OnWarning { get; set; }

    private string? errorMessage;
    private bool AnySummaryEditing => isEditingPaidTimeOff || isEditingOvertime || isEditingVacationTime;

    private bool IsTimerActive => TimerService.IsRunning || Activities.Any(a => a.StoppedAt == null);

    [Parameter, EditorRequired]
    public bool IsReadOnly { get; set; }

    private Guid? selectedTeamId;

    [Parameter, EditorRequired]
    public bool IsUiLocked { get; set; }



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
        if (IsEditingDisabled || !isEditingPaidTimeOff || IsReadOnly)
            return;

        var formatted = ShrinkageExtensionsHelper.FormatAsTime(PaidTimeOffInput);
        paidTimeOffInput = formatted;
    }

    private void FormatOvertimeOnBlur()
    {
        if (IsEditingDisabled || !isEditingOvertime || IsReadOnly)
            return;

        var formatted = ShrinkageExtensionsHelper.FormatAsTime(overtimeInput);
        overtimeInput = formatted;
    }

    private void FormatVacationTimeOnBlur()
    {
        if (IsEditingDisabled || !isEditingVacationTime || IsReadOnly)
            return;

        var formatted = ShrinkageExtensionsHelper.FormatAsTime(vacationTimeInput);
        vacationTimeInput = formatted;
    }

    private void StartEditPaid()
    {
        OnWarning(null);
        if (IsEditingDisabled || IsReadOnly) return;
        if (UnfinishedActivityExist()) return;
        isEditingPaidTimeOff = true;
        isEditingOvertime = false;
        isEditingVacationTime = false;
        OnGlobalEditChanged(true);
    }

    private void StartEditOvertime()
    {
        OnWarning(null);
        if (IsEditingDisabled || IsReadOnly) return;
        if (UnfinishedActivityExist()) return;
        isEditingPaidTimeOff = false;
        isEditingOvertime = true;
        isEditingVacationTime = false;
        OnGlobalEditChanged(true);
    }

    private void StartEditVacationTime()
    {
        OnWarning(null);
        if (IsEditingDisabled || IsReadOnly) return;
        if (UnfinishedActivityExist()) return;
        isEditingPaidTimeOff = false;
        isEditingOvertime = false;
        isEditingVacationTime = true;
        OnGlobalEditChanged(true);
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




}

