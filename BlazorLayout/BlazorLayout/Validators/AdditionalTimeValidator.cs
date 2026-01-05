using BlazorLayout.Modeles;
using Microsoft.Extensions.Localization;

namespace BlazorLayout.Validators;

public static class AdditionalTimeValidator
{
    private static readonly TimeSpan maximumAllowedOvertime = TimeSpan.FromHours(10);

    public static IStringLocalizer Localizer {  get; set; } = null!;

    public static bool CheckIfVacationTimeOrPaidTimeOffCanBeModified(TimeSpan currentValue, TimeSpan currentRemainingTime, TimeSpan newValue, string changedValueType, out string? errorMessage)
    {
        errorMessage = null;

        var newRemainingTime = currentRemainingTime + currentValue;
        if (newValue > newRemainingTime)
        {
            var formattedRemainingTime = newRemainingTime.ToString(@"hh\:mm");
            errorMessage = Localizer["shrinkage_warning_not_valid_time_span_D", changedValueType, formattedRemainingTime];
            return false;
        }

        return true;
    }

    public static bool CheckIfOverTimeBeModified(TimeSpan currentPaidTime, TimeSpan newOvertime, out string? errorMessage)
    {
        errorMessage = null;
        var remainingAvailableOvertime = (maximumAllowedOvertime - currentPaidTime).ToString(@"hh\:mm");
        if (currentPaidTime + newOvertime > maximumAllowedOvertime)
        {
            errorMessage = Localizer["shrinkage_warning_not_valid_time_span_for_overtime_D", remainingAvailableOvertime];
            return false;
        }

        return true;
    }

    public static string? CheckOverlapForAbsence(DateOnly startDate, DateOnly stopDate, IEnumerable<AbsenceDto> existingAbsence, Guid? currentAbsenceId)
    {
        foreach (var absence in existingAbsence)
        {
            if (currentAbsenceId is not null && absence.Id == currentAbsenceId)
                continue;

            bool overlaps =
                (startDate >= absence.StartDate && startDate <= absence.EndDate) ||
                (stopDate <= absence.EndDate && stopDate >= absence.StartDate) ||
                (startDate <= absence.StartDate && stopDate >= absence.EndDate);

            if (overlaps)
            {
                return Localizer["shrinkage_warning_absence_overlap"];
            }
        }

        return null;

        /// <summary>
        /// Vérifie si une absence en cours de création ou de modification
        /// chevauche une absence existante de l'utilisateur.
        ///
        /// Exemple d'absence existante déjà enregistrée :
        /// - StartDate = 10/01/2026
        /// - EndDate   = 12/01/2026
        ///
        /// Le warning "shrinkage_warning_absence_overlap" est déclenché
        /// dans les cas suivants :
        ///
        /// Cas 1 : Chevauchement par le début
        /// La date de début de la nouvelle absence se situe à l'intérieur
        /// d'une absence existante.
        /// Exemple :
        /// - Nouvelle absence : 11/01/2026 → 14/01/2026
        ///
        /// Cas 2 : Chevauchement par la fin
        /// La date de fin de la nouvelle absence se situe à l'intérieur
        /// d'une absence existante.
        /// Exemple :
        /// - Nouvelle absence : 08/01/2026 → 11/01/2026
        ///
        /// Cas 3 : Chevauchement total
        /// La nouvelle absence englobe complètement une absence existante.
        /// Exemple :
        /// - Nouvelle absence : 09/01/2026 → 13/01/2026
        ///
        /// Dans tous ces cas, une absence a déjà été réservée pour la période
        /// spécifiée et la création ou la modification doit être bloquée.
        /// </summary>


        ///</summary>
    }
}

