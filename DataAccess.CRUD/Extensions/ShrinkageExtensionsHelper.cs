
using System.ComponentModel;
using DataAccess.CRUD.EnumsModels;
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

    // Conversion de ActivityTrackType (gRPC) vers ActivityTrackTypeDto
    public static ActivityTrackTypeDto FromGrpcActivityTrackType(ActivityTrackType trackType)
    {
        return trackType switch
        {
            ActivityTrackType.Manual => ActivityTrackTypeDto.Manual,
            ActivityTrackType.Timer => ActivityTrackTypeDto.Timer,
            ActivityTrackType.ManualStopped => ActivityTrackTypeDto.ManualStopped,
            _ => ActivityTrackTypeDto.Unspecified,
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


    // Attention ici :  Conversion de Status (gRPC) vers StatusDto
    public static StatusDto FromGrpcStatus(Status status)
    {
        return status switch
        {
            Status.Pending => StatusDto.Open,
            Status.Transferred => StatusDto.Transferred,
            Status.Approved => StatusDto.Approved,
            Status.Rejected => StatusDto.Rejected,
            Status.Missing => StatusDto.Missing,
            _ => StatusDto.Unspecified,
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

    public static ActivityTypeDto FromGrpcActivityType(this ActivityType activityType)
    {
        return activityType switch
        {
            ActivityType.Meeting => ActivityTypeDto.Meeting,
            ActivityType.Projects => ActivityTypeDto.Projects,
            ActivityType.BusinessInterruption => ActivityTypeDto.BusinessInterruption,
            ActivityType.Others => ActivityTypeDto.Others,
            ActivityType.TrainingOrCoaching => ActivityTypeDto.TrainingOrCoaching,
            ActivityType.ProductiveNotMeasurable => ActivityTypeDto.ProductiveNotMeasurable,
            _ => throw new InvalidEnumArgumentException(nameof(activityType)),
        };
    }

    public static AbsenceType ToGrpcAbsenceType(this AbsenceTypeDto absenceType)
    {
        return absenceType switch
        {
            AbsenceTypeDto.Vacation => AbsenceType.Vacation,
            AbsenceTypeDto.Sickness => AbsenceType.Sickness,
            _ => throw new InvalidEnumArgumentException(nameof(absenceType)),
        };
    }

    public static AbsenceTypeDto FromGrpcAbsenceType(this AbsenceType absenceType)
    {
        return absenceType switch
        {
            AbsenceType.Vacation => AbsenceTypeDto.Vacation,
            AbsenceType.Sickness => AbsenceTypeDto.Sickness,
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
