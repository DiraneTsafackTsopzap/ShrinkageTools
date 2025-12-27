using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrpcShrinkageServiceTraining.Protobuf;

namespace DataAccess.CRUD.Extensions;

public static class ShrinkageExtensionsHelper
{
    public static ActivityTrackType ToGrpcActivityTrackType(this ActivityTrackTypeDto type)
      => type switch
      {
          ActivityTrackTypeDto.Timer => ActivityTrackType.Timer,
          ActivityTrackTypeDto.Manual => ActivityTrackType.Manual,
          ActivityTrackTypeDto.ManualStopped => ActivityTrackType.ManualStopped,
          _ => ActivityTrackType.Unspecified
      };

    public static ActivityTrackType FromGrpcActivityTrackType(ActivityTrackTypeDto trackType)
    {
        return trackType switch
        {
            ActivityTrackTypeDto.Manual => ActivityTrackType.Manual,
            ActivityTrackTypeDto.Timer => ActivityTrackType.Timer,
            ActivityTrackTypeDto.ManualStopped => ActivityTrackType.ManualStopped,
            _ => ActivityTrackType.Unspecified,
        };
    }

    public static Status ToGrpcStatus(this Status status)
    {
        return status switch
        {
            Status.Pending => Status.Pending,
            Status.Transferred => Status.Transferred,
            Status.Approved => Status.Approved,
            Status.Rejected => Status.Rejected,
            _ => Status.Unspecified,
        };
    }

    public static Status FromGrpcStatus(Status status)
    {
        return status switch
        {
            Status.Pending => Status.Pending,
            Status.Transferred => Status.Transferred,
            Status.Approved => Status.Approved,
            Status.Rejected => Status.Rejected,
            Status.Missing => Status.Missing,
            _ => Status.Unspecified,
        };
    }


    // Conversion de ActivityTypeDto vers ActivityType (gRPC)
    public static ActivityType ToGrpcActivityType(this ActivityTypeDto type)
         => type switch
         {
             ActivityTypeDto.Meeting => ActivityType.Meeting,
             ActivityTypeDto.Projects => ActivityType.Projects,
             ActivityTypeDto.BusinessInterruption => ActivityType.BusinessInterruption,
             ActivityTypeDto.TrainingOrCoaching => ActivityType.TrainingOrCoaching,
             ActivityTypeDto.ProductiveNotMeasurable => ActivityType.ProductiveNotMeasurable,
             ActivityTypeDto.Others => ActivityType.Others,
             _ => ActivityType.Unspecified
         };

    public static ActivityType FromGrpcActivityType(this ActivityType activityType)
    {
        return activityType switch
        {
            ActivityType.Meeting => ActivityType.Meeting,
            ActivityType.Projects => ActivityType.Projects,
            ActivityType.BusinessInterruption => ActivityType.BusinessInterruption,
            ActivityType.Others => ActivityType.Others,
            ActivityType.TrainingOrCoaching => ActivityType.TrainingOrCoaching,
            ActivityType.ProductiveNotMeasurable => ActivityType.ProductiveNotMeasurable,
            _ => throw new InvalidEnumArgumentException(nameof(activityType)),
        };
    }

    public static AbsenceType ToGrpcAbsenceType(this AbsenceType absenceType)
    {
        return absenceType switch
        {
            AbsenceType.Vacation => AbsenceType.Vacation,
            AbsenceType.Sickness => AbsenceType.Sickness,
            _ => throw new InvalidEnumArgumentException(nameof(absenceType)),
        };
    }

    public static AbsenceType FromGrpcAbsenceType(this AbsenceType absenceType)
    {
        return absenceType switch
        {
            AbsenceType.Vacation => AbsenceType.Vacation,
            AbsenceType.Sickness => AbsenceType.Sickness,
            _ => throw new InvalidEnumArgumentException(nameof(absenceType)),
        };
    }

    public static string FormatAsTime(string input)
    {
        var digits = new string((input ?? "").Where(char.IsDigit).ToArray());
        if (string.IsNullOrEmpty(digits))
            return "00:00";

        if (!int.TryParse(digits, out var number))
            return "00:00";

        if (digits.Length <= 3)
        {
            var ts = TimeSpan.FromMinutes(number);
            return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}";
        }

        var hours = number / 100;
        var minutes = number % 100;
        if (minutes >= 60)
        {
            hours += minutes / 60;
            minutes %= 60;
        }

        return $"{hours:D2}:{minutes:D2}";
    }

}
