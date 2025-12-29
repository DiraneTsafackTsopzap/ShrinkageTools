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

}


