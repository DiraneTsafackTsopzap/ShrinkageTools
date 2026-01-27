using BlazorLayout.Extensions;
using BlazorLayout.Modeles;
using BlazorLayout.ModelRequest;
using BlazorLayout.StateManagement;

namespace BlazorLayout.Stores;

[AutoSubscribe]
public sealed partial class UserShrinkageStore : StoreBase
{
    [AutoSubscribe]
    public partial IReadOnlyDictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>> UsersShrinkages { get; set; }


    public void UpdateUserShrinkage(ActivityDto modifiedActivity)
    {
        var shrinkageDate = DateOnly.FromDateTime(modifiedActivity.StartedAt.DateTime);

        if (!__UsersShrinkages.TryGetValue(modifiedActivity.UserId, out var userShrinkages))
            throw new InvalidOperationException("User shrinkage for this user were not initialized");

        var userShrinkage = userShrinkages[shrinkageDate] ?? throw new InvalidOperationException("User shrinkage for this user for this shrinkage date were not initialized");

        var existingActivityIndex = userShrinkage.Activities.FindIndex(x => x.Id == modifiedActivity.Id);

        if (existingActivityIndex is not null)
        {
            userShrinkage = userShrinkage with
            {
                Activities = userShrinkage.Activities.WithAt(existingActivityIndex.Value, modifiedActivity),
            };
        }
        else
        {
            userShrinkage = userShrinkage with
            {
                Activities = [.. userShrinkage.Activities, modifiedActivity],
            };
        }

        UsersShrinkages = new Dictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>>(__UsersShrinkages)
        {
            [modifiedActivity.UserId] = new Dictionary<DateOnly, UserShrinkageDto>(userShrinkages)
            {
                [shrinkageDate] = userShrinkage,
            },
        };
    }

    public void InitializeShrinkage(Guid userId, DateOnly shrinkageDate, UserShrinkageDto userShrinkage)
    {
        if (__UsersShrinkages.Any(x => x.Key == userId))
        {
            if (__UsersShrinkages[userId].Values.Any(x => x.UserDailyValues?.ShrinkageDate == shrinkageDate))
                throw new InvalidOperationException("Shrinkages for this user for this date has be already initialized");

            var updatedUserShrinkage = __UsersShrinkages[userId];
            UsersShrinkages = new Dictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>>(__UsersShrinkages)
            {
                [userId] = new Dictionary<DateOnly, UserShrinkageDto>(updatedUserShrinkage)
                {
                    [shrinkageDate] = userShrinkage,
                },
            };
        }
        else
        {
            UsersShrinkages = new Dictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>>(__UsersShrinkages)
            {
                [userId] = new Dictionary<DateOnly, UserShrinkageDto>
                {
                    [shrinkageDate] = userShrinkage,
                },
            };
        }
    }

    public void Reset()
    {
        UsersShrinkages = new Dictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>>();
    }


    public void DeleteActivityFromUserShrinkage(Guid userId, Guid id, DateOnly activityDate)
    {
        if (!__UsersShrinkages.TryGetValue(userId, out var userShrinkages))
            throw new InvalidOperationException("User shrinkage for this user were not initialized");

        var userShrinkage = userShrinkages[activityDate] ?? throw new InvalidOperationException("User shrinkage for this user for this shrinkage date were not initialized");

        var existingActivityIndex = userShrinkage.Activities.FindIndex(x => x.Id == id) ?? Utils.Unreachable<int>();

        userShrinkage = userShrinkage with
        {
            Activities = userShrinkage.Activities.ExceptAt(existingActivityIndex),
        };

        UsersShrinkages = new Dictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>>(__UsersShrinkages)
        {
            [userId] = new Dictionary<DateOnly, UserShrinkageDto>(userShrinkages)
            {
                [activityDate] = userShrinkage,
            },
        };
    }

    public void RemoveUserShrinkage(Guid userId, DateOnly startInclusive, DateOnly endInclusive)
    {
        var start = startInclusive;
        var end = endInclusive;
        if (end < start) (start, end) = (end, start);

        if (!__UsersShrinkages.TryGetValue(userId, out var userShrinkages))
            throw new InvalidOperationException("User shrinkage for this user were not initialized");

        for (var day = start; day <= end; day = day.AddDays(1))
        {
            if (userShrinkages.TryGetValue(day, out UserShrinkageDto _))
            {
                var copy = new Dictionary<DateOnly, UserShrinkageDto>(userShrinkages);
                copy.Remove(day);
                UsersShrinkages = new Dictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>>(__UsersShrinkages)
                {
                    [userId] = new Dictionary<DateOnly, UserShrinkageDto>(copy),
                };
            }
        }


    }

    public void UpdateUserDailyValueStatus(IReadOnlyList<UserDailyValueStatus_M> dailyValueStatuses)
    {
        foreach (var dailyValueStatus in dailyValueStatuses)
        {
            if (!__UsersShrinkages.TryGetValue(dailyValueStatus.UserId, out var userShrinkages))
                throw new InvalidOperationException("User shrinkage for this user were not initialized");

            var shrinkage = userShrinkages.Values
                .FirstOrDefault(v => v.UserDailyValues?.Id == dailyValueStatus.DailyValuesId);

            if (shrinkage == null)
                throw new InvalidOperationException("User shrinkage for this shrinkage date were not initialized");

            if (shrinkage.UserDailyValues is null)
                throw new InvalidOperationException("UserDailyValues not loaded for this date.");

            shrinkage = shrinkage with
            {
                UserDailyValues = shrinkage.UserDailyValues with { Status = dailyValueStatus.Status, Comment = dailyValueStatus.Comment },
            };
            UsersShrinkages = new Dictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>>(__UsersShrinkages)
            {
                [dailyValueStatus.UserId] = new Dictionary<DateOnly, UserShrinkageDto>(userShrinkages)
                {
                    [shrinkage.UserDailyValues.ShrinkageDate] = shrinkage,
                },
            };
        }
    }
    public void UpdateUserDailyValue(SaveUserDailyValuesRequest_M dailyValue)
    {
        if (!__UsersShrinkages.TryGetValue(dailyValue.UserId, out var userShrinkages))
            throw new InvalidOperationException("User shrinkage for this user were not initialized");

        if (!userShrinkages.TryGetValue(dailyValue.ShrinkageDate, out var userShrinkage))
            throw new InvalidOperationException("User shrinkage for this shrinkage date were not initialized");

        if (dailyValue.PaidTime.HasValue)
        {
            userShrinkage = userShrinkage with
            {
                PaidTime = dailyValue.PaidTime ?? TimeSpan.Zero,
            };
        }

        if (dailyValue.PaidTimeOff.HasValue)
        {
            userShrinkage = userShrinkage with
            {
                PaidTimeOff = dailyValue.PaidTimeOff ?? TimeSpan.Zero,
            };
        }

        if (dailyValue.Overtime.HasValue)
        {
            userShrinkage = userShrinkage with
            {
                Overtime = dailyValue.Overtime ?? TimeSpan.Zero,
            };
        }

        if (dailyValue.VacationTime.HasValue)
        {
            userShrinkage = userShrinkage with
            {
                VacationTime = dailyValue.VacationTime ?? TimeSpan.Zero,
            };
        }

        UsersShrinkages = new Dictionary<Guid, IReadOnlyDictionary<DateOnly, UserShrinkageDto>>(__UsersShrinkages)
        {
            [dailyValue.UserId] = new Dictionary<DateOnly, UserShrinkageDto>(userShrinkages)
            {
                [dailyValue.ShrinkageDate] = userShrinkage,
            },
        };
    }
}


