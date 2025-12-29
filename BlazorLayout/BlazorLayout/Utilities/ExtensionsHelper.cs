using BlazorLayout.Enums;
using BlazorLayout.Modeles;
using Microsoft.Extensions.Localization;
using System.ComponentModel;

namespace BlazorLayout.Utilities;

    public static  class ExtensionsHelper
    {
    public static IStringLocalizer Localizer {  get; set; } = null!;


    public static string GetActivityTrackType(this ActivityTrackTypeDto type) =>
        type switch
        {
            ActivityTrackTypeDto.Timer => Localizer["shrinkage_activity_track_type_timer"],
            ActivityTrackTypeDto.Manual => Localizer["shrinkage_activity_track_type_manual"],
            ActivityTrackTypeDto.ManualStopped => Localizer["shrinkage_activity_track_type_manual_stopped"],
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    public static string GetStatus(this ActivityDto a)
    {
        if (a.StoppedAt != null)
            return Localizer["shrinkage_status_booked"];

        if (a.StoppedAt == null)
            return Localizer["shrinkage_status_running"];

        return Localizer["shrinkage_activity_unknown_status"];
    }

    public static string ToDisplayString(this StatusDto status)
    {
        return status switch
        {
            StatusDto.Open or StatusDto.Missing => Localizer["shrinkage_status_open"],
            StatusDto.Transferred => Localizer["shrinkage_button_transferred"],
            StatusDto.Rejected => Localizer["shrinkage_button_rejected"],
            StatusDto.Approved => Localizer["shrinkage_button_approved"],
            StatusDto.All => Localizer["shrinkage_status_all"],
            _ => throw new InvalidEnumArgumentException(nameof(status)),
        };
    }

    public static string ConvertToActivityTypeString(this Enum activityType)
    {
        if (Localizer == null)
            return activityType.ToString();

        var result = activityType switch
        {
            ActivityTypeDto.Meeting => Localizer["shrinkage_activity_type_meeting"],
            ActivityTypeDto.Projects => Localizer["shrinkage_activity_type_projects"],
            ActivityTypeDto.BusinessInterruption => Localizer["shrinkage_activity_type_business_interruption"],
            ActivityTypeDto.TrainingOrCoaching => Localizer["shrinkage_activity_type_training_or_coaching"],
            ActivityTypeDto.Others => Localizer["shrinkage_activity_type_others"],
            ActivityTypeDto.ProductiveNotMeasurable => Localizer["shrinkage_activity_type_productive_not_measurable"],
            _ => throw new InvalidEnumArgumentException(nameof(activityType)),
        };

        return result;
    }
    public static ActivityTypeDto ConvertActivityTypeToEnum(this string activityType)
    {
        var result = activityType switch
        {
            nameof(ActivityTypeDto.Meeting) => ActivityTypeDto.Meeting,
            nameof(ActivityTypeDto.Projects) => ActivityTypeDto.Projects,
            nameof(ActivityTypeDto.BusinessInterruption) => ActivityTypeDto.BusinessInterruption,
            nameof(ActivityTypeDto.TrainingOrCoaching) => ActivityTypeDto.TrainingOrCoaching,
            nameof(ActivityTypeDto.Others) => ActivityTypeDto.Others,
            nameof(ActivityTypeDto.ProductiveNotMeasurable) => ActivityTypeDto.ProductiveNotMeasurable,
            _ => throw new InvalidEnumArgumentException(nameof(activityType)),
        };

        return result;
    }



    public static string ConvertToAbsenceTypeString(this AbsenceTypeDto absenceType)
    {
        var result = absenceType switch
        {
            AbsenceTypeDto.Sickness => Localizer["shrinkage_absence_type_sickness"],
            AbsenceTypeDto.Vacation => Localizer["shrinkage_absence_type_vacation"],
            _ => throw new InvalidEnumArgumentException(nameof(absenceType)),
        };

        return result;
    }
}

