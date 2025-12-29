using DataAccess.CRUD.Modeles;

namespace DataAccess.CRUD.Repositories
{
    public interface IShrinkageUserRepository
    {

        Task<ShrinkageUserDataModel?> GetUserByEmail(string email, CancellationToken token);

        Task<bool> DeleteById(Guid id, Guid deletedBy, CancellationToken token);
        Task<List<ShrinkageActivityDataModel>> GetActivitiesByUserId(Guid userId, DateOnly shrinkageDate, CancellationToken token);
        Task<ShrinkageUserDataModel> Create(ShrinkageUserDataModel user, CancellationToken token);
        Task<int> UpdateActivityById(ShrinkageActivityDataModel activity, CancellationToken token);
        Task<ShrinkageActivityDataModel> CreateActivity(ShrinkageActivityDataModel activity, CancellationToken token);
        Task<Guid> GetUserIdByEmail(string email, CancellationToken token);

        Task<ShrinkageUserDailyValuesDataModel?> GetActiveUserDailyValuesByUserIdAndDate(Guid id, DateOnly date, CancellationToken token);

        Task<ShrinkageActivityDataModel?> GetActivityById(Guid id, CancellationToken token);


        // GetUserById
        Task<ShrinkageUserDataModel?> GetUserById(Guid id, CancellationToken token);

        Task<double> GetPaidTimeByUserIdAndDate(Guid userId, DateOnly shrinkageDate, CancellationToken token);

        Task<List<ShrinkageTeamsPublicHolidaysDataModel>> GetTeamsPublicHolidaysByTeamId(Guid teamId, CancellationToken token);
        Task<List<ShrinkageAbsenceDataModel>> GetAbsencesByUserId(Guid id, CancellationToken token);

        Task<List<ShrinkageUserDailyValuesDataModel>> GetUserDailyValuesByUserId(Guid id, DateOnly startDate, DateOnly endDate, CancellationToken token);

        Task<ShrinkageUserDailyValuesDataModel?> GetDeletedUserDailyValuesByUserIdAndDate(Guid id, DateOnly date, CancellationToken token);

        Task<int> UpdateById(ShrinkageUserDailyValuesDataModel model, CancellationToken token);

        Task<int> Create(ShrinkageUserDailyValuesDataModel dailyValue, CancellationToken token);

    }
}
