using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataAccess.CRUD.DapperContext;
using DataAccess.CRUD.Modeles;
using Npgsql;

namespace DataAccess.CRUD.Repositories.Holidays
{
    public class ShrinkageTeamPublicHolidaysRepository : IShrinkageTeamPublicHolidaysRepository
    {
        private readonly DapperDbContext dapperDbContext;

        public ShrinkageTeamPublicHolidaysRepository(DapperDbContext dapper)
        {
            dapperDbContext = dapper;
        }

        private async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken token)
        {
            var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);

            await connection.OpenAsync(token);
            return connection;
        }
        public async Task<IReadOnlyList<ShrinkageTeamsPublicHolidaysDataModel>> GetPublicHolidaysByTeamLeadId(
    Guid teamLeadId,
    CancellationToken cancellationToken)
        {
            const string sql = @$"
SELECT 
    ph.id           AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.Id)},
    ph.affected_day AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.AffectedDay)},
    ph.title        AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.Title)},
    ph.team_ids     AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.TeamIds)}
FROM shrinkage_team_public_holidays ph
WHERE ph.deleted_at IS NULL
  AND EXISTS (
        SELECT 1 
        FROM shrinkage_teams t
        WHERE t.team_lead_ids @> ARRAY[@TeamLeadId]::uuid[]
          AND t.deleted_at IS NULL
          AND t.id = ANY(ph.team_ids)
  );
";

            var parameters = new { TeamLeadId = teamLeadId };

            await using var connection = await GetOpenConnectionAsync(cancellationToken);

            var result = await connection.QueryAsync<ShrinkageTeamsPublicHolidaysDataModel>(sql, parameters);

            return result.ToList();
        }


        public async Task<List<ShrinkageTeamsPublicHolidaysDataModel>> GetTeamsPublicHolidaysByTeamId(Guid teamId, CancellationToken token)
        {
            const string sql = @$"
SELECT 
    id            AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.Id)},
    created_at    AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.CreatedAt)},
    created_by    AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.CreatedBy)},
    deleted_at    AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.DeletedAt)},
    deleted_by    AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.DeletedBy)},
    title         AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.Title)},
    affected_day  AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.AffectedDay)},
    team_ids      AS {nameof(ShrinkageTeamsPublicHolidaysDataModel.TeamIds)}
FROM shrinkage_team_public_holidays
WHERE team_ids @> ARRAY[@TeamId]::uuid[]
  AND deleted_at IS NULL;
";

            var parameters = new { TeamId = teamId };

            await using var connection = await GetOpenConnectionAsync(token);

            var result = await connection.QueryAsync<ShrinkageTeamsPublicHolidaysDataModel>(sql, parameters);

            return result.ToList();
        }

    }
}
