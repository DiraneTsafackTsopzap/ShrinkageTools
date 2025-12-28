using BlazorLayout.Enums;
using BlazorLayout.Modeles;

namespace BlazorLayout.Shared;
public static class TimeCalculator
{
    public static string GetDuration(DateTimeOffset? start, DateTimeOffset? stop)
    {
        if (!start.HasValue || !stop.HasValue) return string.Empty;
        var duration = stop.Value - start.Value;
        if (duration > TimeSpan.Zero)
        {
            duration = TimeSpan.FromMinutes(Math.Ceiling(duration.TotalMinutes));
        }

        return duration.FormatTimeSpanToHhMm();
    }

    public static TimeSpan GetRemainingTime(TimeSpan paidTime, TimeSpan overtime, TimeSpan vacationTime, TimeSpan paidTimeOff, IReadOnlyList<ActivityDto>? activities)
    {
        var paidEffective = paidTime + overtime - vacationTime - paidTimeOff;
        if (paidEffective < TimeSpan.Zero) paidEffective = TimeSpan.Zero;

        var used = TimeSpan.Zero;
        if (activities != null)
        {
            foreach (var a in activities)
            {
                if (a.StoppedAt.HasValue)
                {
                    var duration = a.StoppedAt.Value - a.StartedAt;
                    used += TimeSpan.FromMinutes(Math.Ceiling(duration.TotalMinutes));
                }
            }
        }

        var remaining = paidEffective - used;
        return remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
    }

    public static string GetTotalDurationOfActivity(IReadOnlyList<ActivityDto> activities, ActivityTypeDto activityType, Guid userTeamId)
        => GetTotalDurationForActivityTimeSpan(activities, activityType, userTeamId).FormatTimeSpanToHhMm();

    public static TimeSpan GetTotalDurationForActivityTimeSpan(IReadOnlyList<ActivityDto> activities, ActivityTypeDto activityType, Guid userTeamId)
    {
        var total = TimeSpan.Zero;
        foreach (var a in activities)
        {
            if (a.ActivityType == activityType &&
                a.StoppedAt.HasValue && a.TeamId == userTeamId)
            {
                var duration = a.StoppedAt.Value - a.StartedAt;
                total += TimeSpan.FromMinutes(Math.Ceiling(duration.TotalMinutes));
            }
        }

        return total;
    }

    public static string GetProductiveTime(TimeSpan paidTime, TimeSpan overtime, TimeSpan vacation, TimeSpan paidTimeOff, IReadOnlyList<ActivityDto> activities)
        => GetProductiveTimeSpan(paidTime, overtime, vacation, paidTimeOff, activities).FormatTimeSpanToHhMm();

    private static TimeSpan GetProductiveTimeSpan(TimeSpan paidTime, TimeSpan overtime, TimeSpan vacationTime, TimeSpan paidTimeOff, IReadOnlyList<ActivityDto> activities)
    {
        var paidEffective = paidTime + overtime - vacationTime - paidTimeOff;
        if (paidEffective < TimeSpan.Zero) paidEffective = TimeSpan.Zero;

        var nonProductiveTime = TimeSpan.Zero;
        var additionalProductiveTime = TimeSpan.Zero;

        foreach (var a in activities)
        {
            if (a.ActivityType == ActivityTypeDto.Unspecified || !a.StoppedAt.HasValue)
                continue;

            var duration = a.StoppedAt.Value - a.StartedAt;
            var roundedDuration = TimeSpan.FromMinutes(Math.Ceiling(duration.TotalMinutes));

            if (a.ActivityType == ActivityTypeDto.Unspecified)
                additionalProductiveTime += roundedDuration;
            else
                nonProductiveTime += roundedDuration;
        }

        var productiveTime = (paidEffective - nonProductiveTime) + additionalProductiveTime;
        return productiveTime < TimeSpan.Zero ? TimeSpan.Zero : productiveTime;
    }

    public static string GetLoanTime(IReadOnlyList<ActivityDto> activities, Guid userTeamId) => GetLoanTimeSpan(activities, userTeamId).FormatTimeSpanToHhMm();

    private static TimeSpan GetLoanTimeSpan(IReadOnlyList<ActivityDto> activities, Guid userTeamId)
    {
        var total = TimeSpan.Zero;
        if (userTeamId == Guid.Empty) return total;

        foreach (var a in activities)
        {
            if (a.StoppedAt.HasValue && a.TeamId != userTeamId)
            {
                var duration = a.StoppedAt.Value - a.StartedAt;
                total += TimeSpan.FromMinutes(Math.Ceiling(duration.TotalMinutes));
            }
        }

        return total;
    }
}