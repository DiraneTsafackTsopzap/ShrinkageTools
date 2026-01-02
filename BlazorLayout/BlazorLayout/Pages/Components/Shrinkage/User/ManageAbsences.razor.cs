using BlazorLayout.Enums;
using BlazorLayout.Exceptions;
using BlazorLayout.Extensions;
using BlazorLayout.Gateways;
using BlazorLayout.Modeles;
using BlazorLayout.Stores;
using BlazorLayout.Validators;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;
using SystemDateOnly = System.DateOnly;
namespace BlazorLayout.Pages.Components.Shrinkage.User
{
    public sealed partial class ManageAbsences
    {
        [Inject]
        private IStringLocalizer Localizer { get; set; } = null!;

        [Inject]
        private ShrinkageApi ShrinkageApi { get; set; } = null!;

        [Inject]
        private UserByEmailStore UserByEmailStore { get; set; } = null!;

        [Inject]
        private UserAbsencesStore UserAbsencesStore { get; set; } = null!;

        private string? errorMessage;
        private string? warningMessage;
        private bool isLoading;
        private AbsenceDto? addOrEditAbsence;
        private bool showAddRow;
        private static readonly SystemDateOnly tomorrow = SystemDateOnly.FromDateTime(DateTime.Today.AddDays(1));
        private static readonly SystemDateOnly today = SystemDateOnly.FromDateTime(DateTime.Today);
        private IReadOnlyList<AbsenceDto>? userAbsence;
        private AbsenceDto? selectedAbsence;

        private EditContext formContext = new(new AbsenceDto());
        private SystemDateOnly newStartDate = tomorrow;
        private SystemDateOnly newEndDate = tomorrow;
        private const int Timeout = 30_000;
        private string GetRowCreatedBy(string? createdBy) => string.IsNullOrWhiteSpace(createdBy) ? "-" : createdBy;

        private IReadOnlyList<AbsenceDto> GetAbsences =>
        userAbsence == null
            ? []
            : showAddRow && addOrEditAbsence is not null
                ? new[] { addOrEditAbsence }.Concat(userAbsence).ToList()
                : userAbsence;

        private AbsenceTypeDto newAbsenceType;
        public sealed record StateT
        {
            public UserDto User { get; init; } = null!;
            public IReadOnlyDictionary<Guid, IReadOnlyList<AbsenceDto>> UsersAbsences { get; init; } = null!;
        }

        protected override StateT BuildState() => new()
        {
            User = UserByEmailStore.User!,
            UsersAbsences = UserAbsencesStore.Absences,
        };


        protected override async Task OnInitializedAsync()
        {
           
            await base.OnInitializedAsync();

            await LoadUserAbsencesAsync(false);

        }

        private async Task Refresh()
        {
            await LoadUserAbsencesAsync(true);
        }

        private async Task LoadUserAbsencesAsync(bool forceRefresh)
        {
            try
            {
                errorMessage = null;
                isLoading = true;

                await ShrinkageApi.EnsureGetAbsencesByUser([State.User.UserId], forceRefresh, TimeoutToken(Timeout));

                userAbsence = State.UsersAbsences.TryGetValue(State.User.UserId, out var absence) ? absence : [];

                // reset add/edit state
                showAddRow = false;
                addOrEditAbsence = null;
                formContext = new EditContext(new AbsenceDto());
            }
            catch (Exception ex) when (ex is BadRequestException or NotFoundException or GetAbsencesByUserIdsException)
            {
                errorMessage = Localizer["shrinkage_error_get_absences_user_ids"];
                if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                    errorMessage += " " + reason;
            }
            catch (OperationCanceledException) when (IsDisposing) { }
            catch (Exception ex)
            {
                errorMessage = Localizer["shrinkage_error_unexpected"];
                if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                    errorMessage += " " + reason;
            }
            finally
            {
                isLoading = false;
                StateHasChanged();
            }
        }
        private void StartEdit(AbsenceDto row)
        {
            if (showAddRow) return;
            addOrEditAbsence = new AbsenceDto
            {
                Id = row.Id,
                UserId = row.UserId,
                TeamId = row.TeamId,
                AbsenceType = row.AbsenceType,
                StartDate = row.StartDate,
                EndDate = row.EndDate,
                CreatedBy = row.CreatedBy,
                CreatedAt = row.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = State.User.Email,
            };
            newAbsenceType = row.AbsenceType;
            newStartDate = row.StartDate;
            newEndDate = row.EndDate;
            formContext = new EditContext(addOrEditAbsence);
            StateHasChanged();
        }
        private bool IsAbsenceInPast(AbsenceDto absence)
        {
            return absence.EndDate < today;
        }
        private static int CompareAbsences(AbsenceDto a, AbsenceDto b, uint columnIndex)
        {
            return columnIndex switch
            {
                0 => string.Compare(a.AbsenceType.ToString(), b.AbsenceType.ToString(), StringComparison.Ordinal),
                1 => a.StartDate.CompareTo(b.StartDate),
                2 => a.EndDate.CompareTo(b.EndDate),
                4 => DateTime.Compare(a.CreatedAt, b.CreatedAt),
                _ => 0
            };
        }

        private void OnClick(AbsenceDto row)
        {
            selectedAbsence = row;
            StateHasChanged();
        }
        private async Task SubmitAddOrEditAsync()
        {
            if (showAddRow && addOrEditAbsence is not null)
            {
                ValidateAbsenceRequest(addOrEditAbsence);
                if (errorMessage is not null || warningMessage is not null)
                    return;

                await SaveAbsence(addOrEditAbsence);
                return;
            }

            if (addOrEditAbsence is not null)
            {
                ValidateAbsenceRequest(addOrEditAbsence);
                if (errorMessage is not null || warningMessage is not null)
                    return;

                await SaveAbsence(addOrEditAbsence);
            }
        }
        private void Reset()
        {
            userAbsence = State.UsersAbsences.TryGetValue(State.User.UserId, out var absence) ? absence : [];
            showAddRow = false;
            addOrEditAbsence = null;
            newAbsenceType = AbsenceTypeDto.Unspecified;
            ClearMessages();
            newStartDate = tomorrow;
            newEndDate = tomorrow;
            StateHasChanged();
        }

        private void ClearMessages()
        {
            errorMessage = null;
            warningMessage = null;
        }
        private async Task SaveAbsence(AbsenceDto updatedAbsence)
        {
            try
            {
                isLoading = true;
                //await ShrinkageApi.SaveAbsenceAsync(updatedAbsence, TimeoutToken(Timeout));

                //UserDailySummaryStore.AddAbsenceRange(updatedAbsence.Id, updatedAbsence.AbsenceType,
                //    updatedAbsence.StartDate, updatedAbsence.EndDate);
                Reset();
            }
            catch (ConflictException ex)
            {
                errorMessage = Localizer["shrinkage_error_save_absence_conflict"];
                if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                    errorMessage += " " + reason;
            }
            catch (Exception ex) when (ex is BadRequestException or NotFoundException or SaveAbsenceException)
            {
                errorMessage = Localizer["shrinkage_error_save_absence"];
                if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                    errorMessage += " " + reason;
            }
            catch (OperationCanceledException) when (IsDisposing) { }

            catch (Exception ex)
            {
                errorMessage = Localizer["shrinkage_error_unexpected"];
                if (ex.InnerException is HttpRequestException ex2 && ex2.GetReasonMessage(ex) is { } reason)
                    errorMessage += " " + reason;
            }
            finally { isLoading = false; }
        }

        private void AddAbsence()
        {
            addOrEditAbsence = new AbsenceDto
            {
                Id = Guid.NewGuid(),
                UserId = State.User.UserId,
                TeamId = State.User.TeamId!.Value,
                AbsenceType = AbsenceTypeDto.Unspecified,
                StartDate = tomorrow,
                EndDate = tomorrow,
                CreatedBy = State.User.Email,
                CreatedAt = DateTime.Now,
            };
            newAbsenceType = AbsenceTypeDto.Unspecified;
            showAddRow = true;
            formContext = new EditContext(addOrEditAbsence);
            StateHasChanged();
        }


        private void ValidateOverlap(AbsenceDto absence)
        {
            warningMessage = AdditionalTimeValidator.CheckOverlapForAbsence(absence.StartDate, absence.EndDate, userAbsence!, absence.Id);
        }


        private void ValidateAbsenceRequest(AbsenceDto absence)
        {
            if (absence.AbsenceType == AbsenceTypeDto.Unspecified)
            {
                warningMessage = Localizer["shrinkage_select_absence_type"];
            }

            else if (absence.StartDate > absence.EndDate)
            {
                warningMessage = Localizer["shrinkage_absence_date_to_before_from"];
            }
            else
            {
                ValidateOverlap(absence);
            }
        }

    }
}
