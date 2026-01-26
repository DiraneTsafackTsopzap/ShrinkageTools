using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.Modeles;

namespace DataAccess.CRUD.Repositories.UserDailyRepository
{
    public interface IShrinkageUserDailyValuesRepository
    {
        Task<int> Create(ShrinkageUserDailyValuesDataModel dailyValue, CancellationToken token);
        Task<int> DeleteById(ShrinkageUserDailyValuesDataModel model, CancellationToken token);

        Task<int> UpdateById(ShrinkageUserDailyValuesDataModel model, CancellationToken token);
        Task<List<ShrinkageUserDailyValuesDataModel>> GetUserDailyValuesByUserIdAndDateRange(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken token);
        Task<ShrinkageUserDailyValuesDataModel?> GetUserDailyValuesByUserIdAndDate(Guid id, DateOnly date, CancellationToken token);
    }
}
