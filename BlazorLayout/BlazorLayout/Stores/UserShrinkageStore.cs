using BlazorLayout.Extensions;
using BlazorLayout.Modeles;
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




}


