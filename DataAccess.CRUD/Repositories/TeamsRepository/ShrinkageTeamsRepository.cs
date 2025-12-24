using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataAccess.CRUD.DapperContext;
using DataAccess.CRUD.Modeles;
using Npgsql;

namespace DataAccess.CRUD.Repositories.TeamsRepository
{
    public class ShrinkageTeamsRepository : IShrinkageTeamsRepository
    {

        private readonly DapperDbContext dapperDbContext;

        public ShrinkageTeamsRepository(DapperDbContext dapper)
        {
            dapperDbContext = dapper;
        }

        private async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken token)
        {
            var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);

            await connection.OpenAsync(token);
            return connection;
        }




        public async Task<ShrinkageTeamsDataModel> Create( ShrinkageTeamsDataModel team,CancellationToken token)
        {
            await using var connection = await GetOpenConnectionAsync(token);

            var sql = $@"
INSERT INTO shrinkage_teams
(
    id,
    created_at,
    deleted_at,
    team_name,
    team_lead_ids,
    team_reference
)
VALUES
(
    @{nameof(ShrinkageTeamsDataModel.Id)},
    @{nameof(ShrinkageTeamsDataModel.CreatedAt)},
    @{nameof(ShrinkageTeamsDataModel.DeletedAt)},
    @{nameof(ShrinkageTeamsDataModel.TeamName)},
    @{nameof(ShrinkageTeamsDataModel.TeamLeadIds)},
    @{nameof(ShrinkageTeamsDataModel.TeamReference)}
);
";

            await connection.ExecuteAsync(sql, team);
            return team;
        }



       

        public async Task<List<ShrinkageTeamsDataModel>> GetTeams(CancellationToken token)
        {
            await using var connection = await GetOpenConnectionAsync(token);

            var sql = $@"
SELECT
    id             AS {nameof(ShrinkageTeamsDataModel.Id)},
    created_at     AS {nameof(ShrinkageTeamsDataModel.CreatedAt)},
    deleted_at     AS {nameof(ShrinkageTeamsDataModel.DeletedAt)},
    team_name      AS {nameof(ShrinkageTeamsDataModel.TeamName)},
    team_lead_ids  AS {nameof(ShrinkageTeamsDataModel.TeamLeadIds)},
    team_reference AS {nameof(ShrinkageTeamsDataModel.TeamReference)}
FROM shrinkage_teams
WHERE deleted_at IS NULL;
";

            var teams = await connection.QueryAsync<ShrinkageTeamsDataModel>(sql);
            return teams.ToList();
        }


        public async Task<bool> DeleteById(Guid teamId, CancellationToken token)
        {
            await using var connection = await GetOpenConnectionAsync(token);

            var sql = @"DELETE FROM shrinkage_teams WHERE id = @TeamId;";

            var affectedRows = await connection.ExecuteAsync(
                sql,
                new { TeamId = teamId });

            return affectedRows >= 0;
        }

        public async Task<ShrinkageTeamsDataModel?> GetTeamById(Guid id, CancellationToken token)
        {
            await using var connection = await GetOpenConnectionAsync(token);

            var sql = $@"
SELECT
    id             AS {nameof(ShrinkageTeamsDataModel.Id)},
    created_at     AS {nameof(ShrinkageTeamsDataModel.CreatedAt)},
    deleted_at     AS {nameof(ShrinkageTeamsDataModel.DeletedAt)},
    team_name      AS {nameof(ShrinkageTeamsDataModel.TeamName)},
    team_lead_ids  AS {nameof(ShrinkageTeamsDataModel.TeamLeadIds)},
    team_reference AS {nameof(ShrinkageTeamsDataModel.TeamReference)}
FROM shrinkage_teams
WHERE id = @Id
  AND deleted_at IS NULL;
";

            return await connection.QuerySingleOrDefaultAsync<ShrinkageTeamsDataModel>(
                sql,
                new { Id = id });
        }

        public async Task<int> Truncate(CancellationToken token)
        {
            await using var connection = await GetOpenConnectionAsync(token);

            var sql = @"DELETE FROM shrinkage_teams;";

            return await connection.ExecuteAsync(sql);
        }



    }
}
