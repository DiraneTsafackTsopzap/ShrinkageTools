using BlazorLayout.Enums;
using BlazorLayout.Exceptions;
using BlazorLayout.Extensions;
using BlazorLayout.Gateways;
using BlazorLayout.Modeles;
using BlazorLayout.Stores;
using BlazorLayout.Utilities;
using BlazorLayout.Validators;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
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

        [Inject]
        private UserDailySummaryStore UserDailySummaryStore { get; set; } = null!;

        private AbsenceTypeDto newAbsenceType;

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
        userAbsence == null ? []
                 : showAddRow && addOrEditAbsence is not null ? new[] { addOrEditAbsence }.Concat(userAbsence).ToList()
                 : userAbsence;

        /// <summary>
        ///    Cas 1 : userAbsence == null ? [] , Si il Y'a aucune Absence on retourne une liste vide
        /// </summary>

        /// <summary>
        /// Cas 2 : showAddRow == true ET addOrEditAbsence n’est pas null.
        /// 
        /// Cela correspond au mode AJOUT d’une absence.
        /// Une absence est en cours de création (mais pas encore sauvegardée).
        /// 
        /// Dans ce cas, on construit une nouvelle liste en plaçant
        /// l’absence en cours d’ajout au-dessus des absences existantes (userAbsence),
        /// uniquement pour l’affichage dans l’UI, sans modifier le Store.
        /// </summary>

        /// <summary>
        /// Cas 3 :  : userAbsence; Affichage Nornal
        /// 
        /// Les absences sont déjà chargées (userAbsence n’est pas null)
        /// et aucun ajout n’est en cours (showAddRow == false).
        /// 
        /// Dans ce cas, on retourne simplement la liste existante des absences
        /// de l’utilisateur, sans modification ni ajout temporaire.
        /// </summary>

        private Dictionary<string, object> InputAttributes { get; set; } =
        new()
        {
            { "min", DateTime.Today.AddDays(1).ToString("yyyy-MM-dd") },
        };
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
            // Initialisation du Localizer pour les validators statiques
            AdditionalTimeValidator.Localizer ??= Localizer;

            await base.OnInitializedAsync();

            await LoadUserAbsencesAsync(false);

        }

        private async Task Refresh()
        {
            await LoadUserAbsencesAsync(true);
        }


        private void OnStartDateBlurChanged(FocusEventArgs e)
        {
            if (addOrEditAbsence != null)
            {
                addOrEditAbsence = addOrEditAbsence with
                {
                    StartDate = newStartDate,
                };
                if (addOrEditAbsence!.EndDate < newStartDate)
                {
                    newEndDate = newStartDate;
                    addOrEditAbsence = addOrEditAbsence with
                    {
                        EndDate = newStartDate,
                    };
                }

                ValidateOverlap(addOrEditAbsence);
            }
        }
        private void OnStartDateChanged(ChangeEventArgs e)
        {
            errorMessage = null;
            warningMessage = null;
            if (SystemDateOnly.TryParse(e.ToString(), out var selectedStartDate))
            {
                if (selectedStartDate < tomorrow)
                {
                    newStartDate = addOrEditAbsence!.StartDate;
                }
            }
        }

        private void CancelEdit()
        {
            formContext = new EditContext(new AbsenceDto());
            Reset();
        }
        private async Task LoadUserAbsencesAsync(bool forceRefresh)
        {
            try
            {
                errorMessage = null;
                isLoading = true;

                /// <summary>
                /// les crochets [ ... ] equivalent de new List<Guid> { State.User.UserId }
                /// </summary>  
                await ShrinkageApi.EnsureGetAbsencesByUser([State.User.UserId], forceRefresh, TimeoutToken(Timeout));

                /// <summary>
                /// /// 
                /// userAbsence ici permet de manipuler les Absences sans toucher au Store
                /// </summary>
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

        private void OnEndDateChanged(ChangeEventArgs e)
        {
            errorMessage = null;
            warningMessage = null;
            if (SystemDateOnly.TryParse(e.ToString(), out var selectedEndDate))
            {
                if (selectedEndDate < tomorrow)
                {
                    newEndDate = addOrEditAbsence!.EndDate;
                }
            }
        }

        private void OnEndDateBlurChanged(FocusEventArgs e)
        {
            if (addOrEditAbsence != null)
            {
                addOrEditAbsence = addOrEditAbsence with
                {
                    EndDate = newEndDate,
                };
                if (addOrEditAbsence.StartDate > newEndDate)
                {
                    newStartDate = newEndDate;
                    addOrEditAbsence = addOrEditAbsence with
                    {
                        StartDate = newEndDate,
                    };
                }

                ValidateAbsenceRequest(addOrEditAbsence);
            }
        }
        private void OnClick(AbsenceDto row)
        {
            selectedAbsence = row;
            StateHasChanged();
        }

        private void OnAbsenceTypeChanged(ChangeEventArgs e)
        {
            errorMessage = null;
            warningMessage = null;

            if (e.Value != null && e.Value.ToString() != nameof(AbsenceTypeDto.Unspecified))
            {
                if (addOrEditAbsence != null)
                {
                    /// <summary>
                    /// Le mot-clé <c>with</c> permet de créer une NOUVELLE copie d'un objet
                    /// en modifiant uniquement certaines propriétés, sans toucher à l'objet original.
                    ///
                    /// 🧠 Image mentale :
                    /// - Ancienne absence → une feuille déjà remplie
                    /// - <c>with</c>       → une photocopie de cette feuille
                    /// - Modification     → on corrige juste une ligne sur la copie
                    /// - Résultat         → l'original reste intact
                    ///
                    /// Exemple concret :
                    ///
                    /// <code>
                    /// var a1 = new AbsenceDto
                    /// {
                    ///     Id = 1,
                    ///     AbsenceType = AbsenceTypeDto.Vacation,
                    ///     StartDate = new DateOnly(2026, 01, 10),
                    ///     EndDate = new DateOnly(2026, 01, 12),
                    ///     CreatedBy = "admin"
                    /// };
                    ///
                    /// var a2 = a1 with
                    /// {
                    ///     AbsenceType = AbsenceTypeDto.Sick
                    /// };
                    /// </code>
                    ///
                    /// Résultat :
                    /// - a1.AbsenceType == Vacation (inchangé)
                    /// - a2.AbsenceType == Sick     (nouvelle copie)
                    ///
                    /// Cette approche immuable est utilisée afin que Blazor détecte
                    /// le changement de référence et mette correctement à jour
                    /// l'interface utilisateur.
                    /// </summary>


                    addOrEditAbsence = addOrEditAbsence with
                    {
                        AbsenceType = e.Value.ToString()!.ConvertAbsenceTypeToEnum(),
                    };
                }
            }
        }
        private async Task SubmitAddOrEditAsync()
        {
            /// <summary>
            /// Ajout d'une Absence
            /// </summary>
            
            if (showAddRow && addOrEditAbsence is not null)
            {
                ValidateAbsenceRequest(addOrEditAbsence);
                if (errorMessage is not null || warningMessage is not null)
                    return;

                await SaveAbsence(addOrEditAbsence);
                return;
            }

            /// <summary>
            /// Edition d'une Absence car showaddrow == false ici
            /// </summary>

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
                await ShrinkageApi.SaveAbsenceAsync(updatedAbsence, TimeoutToken(Timeout));

                UserDailySummaryStore.AddAbsenceRange(updatedAbsence.Id, updatedAbsence.AbsenceType,
                updatedAbsence.StartDate, updatedAbsence.EndDate);
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
                StartDate = tomorrow, // Les Absences ne peuvent pas Commencer Aujourdhui , Mais Plut tot Demain
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
                ///<summary>
                /// L'erreur ci est declenche si l'user ne choisit aucune Absence
                /// </summary>
 
                warningMessage = Localizer["shrinkage_select_absence_type"];
            }

            else if (absence.StartDate > absence.EndDate)
            {
                ///<summary>
                /// L'erreur ci est declenche si J'entre Par Exemple StartDate : 15.01.2026 et EndDate : 10.01.2026
                /// </summary>
                
                warningMessage = Localizer["shrinkage_absence_date_to_before_from"];
            }
            else
            {
                ValidateOverlap(absence);
            }
        }

        private async Task DeleteSelectedAbsence()
        {
            errorMessage = null;
            warningMessage = null;
            if (selectedAbsence is null) return;

            try
            {
                await ShrinkageApi.DeleteAbsenceByUserAsync(selectedAbsence.Id, State.User.UserId, State.User.UserId, State.User.TeamId!.Value, selectedAbsence.StartDate, selectedAbsence.EndDate, TimeoutToken(Timeout));

                if (IsTodayWithin(selectedAbsence.StartDate, selectedAbsence.EndDate))
                {
                    ShrinkageApi.RemoveIdempotencyRequest(State.User.UserId, SystemDateOnly.FromDateTime(DateTime.Today));
                }

                selectedAbsence = null;
                Reset();
            }
            catch (Exception ex) when (ex is BadRequestException or NotFoundException or DeleteAbsenceException)
            {
                errorMessage = Localizer["shrinkage_error_delete_absence"];
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
        }

        private static bool IsTodayWithin(SystemDateOnly startDate, SystemDateOnly endDate)
        {
            var start = startDate;
            var end = endDate;
            if (end < start) (start, end) = (end, start);
            return today >= start && today <= end;
        }

    }
}

// La clé à comprendre : les crochets [ ... ] dans le code C# sont utilisés pour définir des attributs.
// [State.User.UserId] est equivalent de  new List<Guid> { State.User.UserId }

