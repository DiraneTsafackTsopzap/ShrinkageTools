using DataAccess.CRUD.Modeles;

namespace DataAccess.CRUD.Repositories
{
    public interface IShrinkageUserRepository
    {

        Task<ShrinkageUserDataModel?> GetUserByEmail(string email, CancellationToken token);

        Task<ShrinkageUserDataModel> Create(ShrinkageUserDataModel user, CancellationToken token);
        Task<int> UpdateActivityById(ShrinkageActivityDataModel activity, CancellationToken token);
        Task<ShrinkageActivityDataModel> CreateActivity(ShrinkageActivityDataModel activity, CancellationToken token);
        Task<Guid> GetUserIdByEmail(string email, CancellationToken token);

        Task<ShrinkageUserDailyValuesDataModel?> GetActiveUserDailyValuesByUserIdAndDate(Guid id, DateOnly date, CancellationToken token);

        Task<ShrinkageActivityDataModel?> GetActivityById(Guid id, CancellationToken token);


    }
}
