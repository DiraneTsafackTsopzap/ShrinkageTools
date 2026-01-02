using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.Modeles;

namespace DataAccess.CRUD.Repositories.AbsencesRepository;

    public interface IShrinkageAbsenceRepository
    {
        Task<ShrinkageAbsenceDataModel> CreateAbsence(ShrinkageAbsenceDataModel absence, CancellationToken token);
        Task<int> UpdateAbsence(ShrinkageAbsenceDataModel absence, CancellationToken token);
        Task<List<ShrinkageAbsenceDataModel>> GetAbsenceByUserIds(List<Guid> userIds, CancellationToken token);

        Task<ShrinkageAbsenceDataModel?> GetAbsenceById(Guid id, CancellationToken token);

        Task<List<ShrinkageAbsenceDataModel>> GetAllAbsenceWithinDateRange(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken token);

    Task<bool> DeleteAbsenceById(Guid id, Guid deletedBy, CancellationToken token);

}

