using BlazorLayout.Enums;
using BlazorLayout.Modeles;
using BlazorLayout.Shared;
using BlazorLayout.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

namespace BlazorLayout.Validators
{
    public static class ActivityValidator
    {

        public static IStringLocalizer Localizer {  get; set; } = null!;

        private static readonly DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        private static readonly TimeZoneInfo germanTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        public static string GetFormattedDuration(this ActivityDto formActivityDto)
        {
            if (formActivityDto.StoppedAt is not null)
            {
                var duration = formActivityDto.StoppedAt.Value - formActivityDto.StartedAt;
                return $"{(int)duration.TotalHours:D2}:{duration.Minutes:D2}";
            }

            return TimeSpan.Zero.FormatTimeSpanToHhMm();
        }
        public static string? ValidateAll(ActivityDto activity, TimeSpan userPaidTime, TimeSpan userOvertime, TimeSpan userVacationTime,
       TimeSpan userPaidTimeOff, IReadOnlyList<ActivityDto> activities, DateOnly shrinkageDate)
        {
            var requiredFieldsWarningMessage = ValidateRequiredFields(activity);

            if (requiredFieldsWarningMessage is not null)
            {
                return requiredFieldsWarningMessage;
            }

            var futureDateWarningMessage = ValidateFutureDate(activity, shrinkageDate);
            if (futureDateWarningMessage is not null)
            {
                return futureDateWarningMessage;
            }

            var dateTimeLogicWarningMessage = ValidateTimeLogic(activity);
            if (dateTimeLogicWarningMessage is not null)
            {
                return dateTimeLogicWarningMessage;
            }

            var hasOverlapWarningMessage = ValidateOverlap(activities, activity);
            if (hasOverlapWarningMessage is not null)
            {
                return hasOverlapWarningMessage;
            }

            var hasConsumedAllHoursWarningMessage = CheckIfAllPaidTimeIsConsumed(activity, userPaidTime, userOvertime, userVacationTime, userPaidTimeOff, activities);
            if (hasConsumedAllHoursWarningMessage is not null)
            {
                return hasConsumedAllHoursWarningMessage;
            }

            return null;
        }

        private static string? ValidateRequiredFields(ActivityDto activity)
        {
            if (activity.ActivityType == ActivityTypeDto.Unspecified)
            {
                return Localizer["shrinkage_warning_activity_required_field"];
            }

            if (activity.TeamId == Guid.Empty)
            {
                return Localizer["shrinkage_warning_select_team"];
            }

            if (activity.StoppedAt is null)
            {
                return Localizer["shrinkage_warning_select_end_time"];
            }

            return null;
        }

        private static string? ValidateTimeLogic(ActivityDto activity)
        {
            if (activity.StoppedAt is null)
            {
                return null;
            }

            return (activity.StartedAt >= activity.StoppedAt ? Localizer["shrinkage_error_message_from_should_be_smaller_to"] : null)!;
        }

        public static string? ValidateOverlap(IReadOnlyList<ActivityDto> activities, ActivityDto activity)
        {
            if (activity.StoppedAt is null)
            {
                foreach (var existingActivity in activities)
                {
                    bool overlaps = activity.StartedAt >= existingActivity.StartedAt.ConvertToGermanLocalTime() && activity.StartedAt < existingActivity.StoppedAt!.Value.ConvertToGermanLocalTime();
                    if (overlaps)
                    {
                        return Localizer["shrinkage_warning_activity_overlap_S", existingActivity.ActivityType.ConvertToActivityTypeString()];
                    }
                }
            }

            foreach (var existing in activities)
            {
                if (existing.Id == activity.Id || existing.StoppedAt == null)
                    continue;

                bool overlaps =
                    (activity.StartedAt >= existing.StartedAt.ConvertToGermanLocalTime() && activity.StartedAt < existing.StoppedAt.Value.ConvertToGermanLocalTime()) ||
                    (activity.StoppedAt > existing.StartedAt.ConvertToGermanLocalTime() && activity.StoppedAt <= existing.StoppedAt.Value.ConvertToGermanLocalTime()) ||
                    (activity.StartedAt < existing.StartedAt.ConvertToGermanLocalTime() && activity.StoppedAt > existing.StoppedAt.Value.ConvertToGermanLocalTime());

                if (overlaps)
                {
                    return Localizer["shrinkage_warning_activity_overlap_S", existing.ActivityType.ConvertToActivityTypeString()];
                }
            }

            return null;
        }
        private static string? CheckIfAllPaidTimeIsConsumed(ActivityDto activity, TimeSpan userPaidTime, TimeSpan userOvertime, TimeSpan userVacationTime,
        TimeSpan userPaidTimeOff, IReadOnlyList<ActivityDto> activities)
        {
            if (activity.StoppedAt is null)
                return null;

            var remainingTime = TimeCalculator.GetRemainingTime(userPaidTime, userOvertime, userVacationTime, userPaidTimeOff, activities);

            var newDuration = TimeSpan.Parse(TimeCalculator.GetDuration(activity.StartedAt, activity.StoppedAt.Value));
            var availableTime = remainingTime;

            if (activities.Any(x => x.Id == activity.Id))
            {
                var existing = activities.First(x => x.Id == activity.Id);
                if (existing.StoppedAt.HasValue)
                {
                    var oldDuration = TimeSpan.Parse(TimeCalculator.GetDuration(existing.StartedAt, existing.StoppedAt.Value));
                    availableTime += oldDuration;
                }
            }

            if (availableTime == TimeSpan.Zero || newDuration > availableTime)
            {
                var formattedAvailableTime = FormatTimeSpanHuman(availableTime);
                return Localizer["shrinkage_warning_time_consumed_S", formattedAvailableTime];
            }

            return null;
        }
        private static string? ValidateFutureDate(ActivityDto activity, DateOnly shrinkageDate)
        {
            if (shrinkageDate == today)
            {
                var now = DateTimeOffset.Now;

                if ((activity.StartedAt.TimeOfDay > now.TimeOfDay) ||
                    (activity.StoppedAt != null && activity.StoppedAt?.TimeOfDay > now.TimeOfDay))
                {
                    return @Localizer["shrinkage_warning_future_date"];
                }
            }

            return null;
        }

        private static string FormatTimeSpanHuman(TimeSpan time)
        {
            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours}h {time.Minutes:D2} min";
            else if (time.TotalMinutes >= 1)
                return $"{(int)time.TotalMinutes} min";
            else
                return $"{(int)time.TotalSeconds} s";
        }


        public static void ChangeStartedAt(ActivityDto activity, TimeOnly newStartedAt, out TimeOnly newStoppedAt, out ActivityDto updatedActivity, DateOnly shrinkageDate)
        {
            activity = activity with
            {
                StartedAt = MergeShrinkageDateAndActivityTime(newStartedAt, shrinkageDate),
            };
            if (activity.StoppedAt is not null)
            {
                if (activity.StoppedAt < activity.StartedAt)
                {
                    activity = activity with
                    {
                        StoppedAt = activity.StartedAt.AddMinutes(1),
                    };
                }

                newStoppedAt = TimeOnly.FromDateTime(activity.StoppedAt.Value.DateTime);
            }
            else
            {
                newStoppedAt = TimeOnly.FromTimeSpan(TimeSpan.Zero);
            }

            updatedActivity = activity;
        }

        public static void ChangeStoppedAt(ActivityDto activity, TimeOnly newStoppedAt, out TimeOnly newStartedAt, out ActivityDto updatedActivity, DateOnly shrinkageDate)
        {
            activity = activity with
            {
                StoppedAt = MergeShrinkageDateAndActivityTime(newStoppedAt, shrinkageDate),
            };

            if (activity.StartedAt > activity.StoppedAt)
            {
                activity = activity with
                {
                    StartedAt = activity.StoppedAt.Value.AddMinutes(-1),
                };
            }

            newStartedAt = TimeOnly.FromDateTime(activity.StartedAt.DateTime);

            updatedActivity = activity;
        }


        private static DateTimeOffset MergeShrinkageDateAndActivityTime(TimeOnly value, DateOnly shrinkageDate)
        {
            var mergedDateTime = shrinkageDate.ToDateTime(value, DateTimeKind.Local);
            var offset = germanTimeZone.GetUtcOffset(mergedDateTime);
            return new DateTimeOffset(mergedDateTime, offset).LocalDateTime;
        }
    }
}
