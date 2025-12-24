using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataAccess.CRUD.Modeles;

namespace DataAccess.CRUD.Repositories.TeamsRepository
{
    public interface IShrinkageTeamsRepository
    {
        Task<ShrinkageTeamsDataModel> Create(ShrinkageTeamsDataModel team, CancellationToken token);
        Task<List<ShrinkageTeamsDataModel>> GetTeams(CancellationToken token);
        Task<ShrinkageTeamsDataModel?> GetTeamById(Guid id, CancellationToken token);
        Task<bool> DeleteById(Guid teamId, CancellationToken token);
        Task<int> Truncate(CancellationToken token);
    }
}
