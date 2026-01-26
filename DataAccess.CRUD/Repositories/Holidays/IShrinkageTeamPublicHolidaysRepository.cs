using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.Modeles;

namespace DataAccess.CRUD.Repositories.Holidays;
    public interface IShrinkageTeamPublicHolidaysRepository
    {
    Task<List<ShrinkageTeamsPublicHolidaysDataModel>> GetTeamsPublicHolidaysByTeamId(Guid teamId, CancellationToken token);
    Task<IReadOnlyList<ShrinkageTeamsPublicHolidaysDataModel>> GetPublicHolidaysByTeamLeadId(Guid teamLeadId, CancellationToken cancellationToken);


    }

