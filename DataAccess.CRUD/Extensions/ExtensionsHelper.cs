using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.Extensions;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcShrinkageServiceTraining.Protobuf;

namespace GrpcShrinkageServiceTraining.Protobuf;
public static class ExtensionsHelper
{
    public static string ConvertFromApiStatus(this Status status)
    {
        return status switch
        {
            Status.Approved => ShrinkageConstants.Approved,
            Status.Rejected => ShrinkageConstants.Rejected,
            Status.Pending => ShrinkageConstants.Pending,
            Status.Transferred => ShrinkageConstants.Transferred,
            _ => ShrinkageConstants.Pending
        };
    }

    public static Status ConvertToApiStatus(this string status)
    {
        return status switch
        {
            "approved" => Status.Approved,
            "rejected" => Status.Rejected,
            "pending" => Status.Pending,
            "transferred" => Status.Transferred,
            "missing" => Status.Missing,
            _ => Status.Unspecified
        };
    }

    public static ActivityTrackType ConvertToApiActivityTrackType(this string activityTrackType)
    {
        return activityTrackType.ToLower() switch
        {
            "manual" => ActivityTrackType.Manual,
            "manual_stopped" => ActivityTrackType.ManualStopped,
            "timer" => ActivityTrackType.Timer,
            _ => ActivityTrackType.Unspecified
        };
    }

    public static string ConvertFromApiActivityTrackType(this ActivityTrackType activityTrackType)
    {
        return activityTrackType switch
        {
            ActivityTrackType.Manual => ShrinkageConstants.Manual,
            ActivityTrackType.ManualStopped => ShrinkageConstants.ManualStopped,
            ActivityTrackType.Timer => ShrinkageConstants.Timer,
            _ => string.Empty
        };
    }

    public static ActivityType ConvertToApiActivityType(this string activityType)
    {
        return activityType.ToLower() switch
        {
            "meeting" => ActivityType.Meeting,
            "projects" => ActivityType.Projects,
            "business_interruption" => ActivityType.BusinessInterruption,
            "others" => ActivityType.Others,
            "productive_not_measurable" => ActivityType.ProductiveNotMeasurable,
            "training_or_coaching" => ActivityType.TrainingOrCoaching,
            _ => throw new InvalidEnumArgumentException(nameof(activityType))
        };
    }

    public static string ConvertFromApiActivityType(this ActivityType activityType)
    {
        return activityType switch
        {
            ActivityType.Meeting => ShrinkageConstants.Meeting,
            ActivityType.Projects => ShrinkageConstants.Projects,
            ActivityType.BusinessInterruption => ShrinkageConstants.BusinessInterruption,
            ActivityType.Others => ShrinkageConstants.Others,
            ActivityType.ProductiveNotMeasurable => ShrinkageConstants.ProductiveNotMeasurable,
            ActivityType.TrainingOrCoaching => ShrinkageConstants.TrainingOrCoaching,
            _ => throw new InvalidEnumArgumentException(nameof(activityType))
        };
    }

    public static AbsenceType ConvertToApiAbsenceType(this string absenceType)
    {
        return absenceType.ToLower() switch
        {
            "sickness" => AbsenceType.Sickness,
            "vacation" => AbsenceType.Vacation,
            _ => throw new InvalidEnumArgumentException(nameof(absenceType))
        };
    }

    public static string ConvertFromApiAbsenceType(this AbsenceType absenceType)
    {
        return absenceType switch
        {
            AbsenceType.Sickness => ShrinkageConstants.Sickness,
            AbsenceType.Vacation => ShrinkageConstants.Vacation,
            _ => throw new InvalidEnumArgumentException(nameof(absenceType))
        };
    }

    public static DateTime ConvertUtcToGermanDateTime(this DateTime utcDateTime)
    {
        DateTime germanDateTime = utcDateTime;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            germanDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcDateTime, "W. Europe Standard Time");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            germanDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcDateTime, "Europe/Berlin");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            throw new NotImplementedException("unable to lookup time zones on a Mac Pc.");
        }

        return germanDateTime;
    }

    public static DateOnly FromTimeStampToDate(this Timestamp timestamp)
    {
        return DateOnly.FromDateTime(timestamp.ToDateTime().ConvertUtcToGermanDateTime());
    }

    public static DateOnly FromDateTimeOffsetToDate(this DateTimeOffset datetime)
    {
        return DateOnly.FromDateTime(datetime.DateTime);
    }
}

