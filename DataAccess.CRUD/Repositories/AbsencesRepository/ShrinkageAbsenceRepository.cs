using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataAccess.CRUD.DapperContext;
using DataAccess.CRUD.Modeles;
using Npgsql;

namespace DataAccess.CRUD.Repositories.AbsencesRepository;
public class ShrinkageAbsenceRepository : IShrinkageAbsenceRepository
{
    private readonly DapperDbContext dapperDbContext;

    public ShrinkageAbsenceRepository(DapperDbContext dapper)
    {
        dapperDbContext = dapper;
    }

    private async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken token)
    {
        var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);

        await connection.OpenAsync(token);
        return connection;
    }
    public async Task<ShrinkageAbsenceDataModel> CreateAbsence(ShrinkageAbsenceDataModel absence, CancellationToken token)
    {
        const string sql = $@"
INSERT INTO shrinkage_user_absences (
    id,
    created_at,
    created_by,
    user_id,
    team_id,
    absence_type,
    start_date,
    end_date
)
VALUES (
    @{nameof(ShrinkageAbsenceDataModel.Id)},
    @{nameof(ShrinkageAbsenceDataModel.CreatedAt)},
    @{nameof(ShrinkageAbsenceDataModel.CreatedBy)},
    @{nameof(ShrinkageAbsenceDataModel.UserId)},
    @{nameof(ShrinkageAbsenceDataModel.TeamId)},
    @{nameof(ShrinkageAbsenceDataModel.AbsenceType)},
    @{nameof(ShrinkageAbsenceDataModel.StartDate)},
    @{nameof(ShrinkageAbsenceDataModel.EndDate)}
)
RETURNING
    id            AS {nameof(ShrinkageAbsenceDataModel.Id)},
    created_at    AS {nameof(ShrinkageAbsenceDataModel.CreatedAt)},
    created_by    AS {nameof(ShrinkageAbsenceDataModel.CreatedBy)},
    user_id       AS {nameof(ShrinkageAbsenceDataModel.UserId)},
    team_id       AS {nameof(ShrinkageAbsenceDataModel.TeamId)},
    absence_type  AS {nameof(ShrinkageAbsenceDataModel.AbsenceType)},
    start_date    AS {nameof(ShrinkageAbsenceDataModel.StartDate)},
    end_date      AS {nameof(ShrinkageAbsenceDataModel.EndDate)};
";

        await using var connection = await GetOpenConnectionAsync(token);

        var result = await connection.QueryFirstAsync<ShrinkageAbsenceDataModel>(sql, absence);

        return result;
    }

    public async Task<bool> DeleteAbsenceById(Guid id, Guid deletedBy, CancellationToken token)
    {
        const string sql = $@"
UPDATE shrinkage_user_absences
SET
    deleted_at = CURRENT_TIMESTAMP,
    deleted_by = @DeletedBy
WHERE id = @Id;
";

        var parameters = new { Id = id, DeletedBy = deletedBy };

        await using var connection = await GetOpenConnectionAsync(token);

        var affectedRows = await connection.ExecuteAsync(sql, parameters);
        return affectedRows > 0;
    }

    public async Task<int> UpdateAbsence(ShrinkageAbsenceDataModel absence, CancellationToken token)
    {
        const string sql = $@"
UPDATE shrinkage_user_absences
SET 
    updated_at    = @{nameof(ShrinkageAbsenceDataModel.UpdatedAt)},
    updated_by    = @{nameof(ShrinkageAbsenceDataModel.UpdatedBy)},
    user_id       = @{nameof(ShrinkageAbsenceDataModel.UserId)},
    team_id       = @{nameof(ShrinkageAbsenceDataModel.TeamId)},
    absence_type  = @{nameof(ShrinkageAbsenceDataModel.AbsenceType)},
    start_date    = @{nameof(ShrinkageAbsenceDataModel.StartDate)},
    end_date      = @{nameof(ShrinkageAbsenceDataModel.EndDate)}
WHERE id = @{nameof(ShrinkageAbsenceDataModel.Id)};
";

        await using var connection = await GetOpenConnectionAsync(token);

        return await connection.ExecuteAsync(sql, absence);
    }


    public async Task<List<ShrinkageAbsenceDataModel>> GetAbsenceByUserIds(List<Guid> userIds, CancellationToken token)
    {
        await using var connection = await GetOpenConnectionAsync(token);

        const string sql = $@"
SELECT 
    a.id                  AS {nameof(ShrinkageAbsenceDataModel.Id)},
    a.created_at          AS {nameof(ShrinkageAbsenceDataModel.CreatedAt)},
    u1.user_email         AS {nameof(ShrinkageAbsenceDataModel.CreatedByUserEmail)},
    a.updated_at          AS {nameof(ShrinkageAbsenceDataModel.UpdatedAt)},
    u2.user_email         AS {nameof(ShrinkageAbsenceDataModel.UpdatedByUserEmail)},
    a.user_id             AS {nameof(ShrinkageAbsenceDataModel.UserId)},
    a.team_id             AS {nameof(ShrinkageAbsenceDataModel.TeamId)},
    a.absence_type        AS {nameof(ShrinkageAbsenceDataModel.AbsenceType)},
    a.start_date          AS {nameof(ShrinkageAbsenceDataModel.StartDate)},
    a.end_date            AS {nameof(ShrinkageAbsenceDataModel.EndDate)}
FROM shrinkage_user_absences a
LEFT JOIN shrinkage_users u1 ON a.created_by = u1.id
LEFT JOIN shrinkage_users u2 ON a.updated_by = u2.id
WHERE a.user_id = ANY(:UserIds)
  AND a.deleted_at IS NULL
  AND a.start_date >= (CURRENT_DATE - INTERVAL '3 months');
";

        var parameters = new { UserIds = userIds };

        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<ShrinkageAbsenceDataModel>(command);

        return result.ToList();
    }

    public async Task<List<ShrinkageAbsenceDataModel>> GetAbsencesByUserId(Guid id, CancellationToken token)
    {
        const string sql = @$"
SELECT 
    a.id             AS {nameof(ShrinkageAbsenceDataModel.Id)},
    a.absence_type   AS {nameof(ShrinkageAbsenceDataModel.AbsenceType)},
    a.start_date     AS {nameof(ShrinkageAbsenceDataModel.StartDate)},
    a.end_date       AS {nameof(ShrinkageAbsenceDataModel.EndDate)}
FROM shrinkage_user_absences a
WHERE a.user_id = @Id
  AND a.deleted_at IS NULL;
";

        var parameters = new { Id = id };

        await using var connection = await GetOpenConnectionAsync(token);

        var result = await connection.QueryAsync<ShrinkageAbsenceDataModel>(sql, parameters);

        return result.ToList();
    }

    public async Task<ShrinkageAbsenceDataModel?> GetAbsenceById(Guid id, CancellationToken token)
    {
        const string sql = $@"
SELECT 
    id           AS {nameof(ShrinkageAbsenceDataModel.Id)},
    created_at   AS {nameof(ShrinkageAbsenceDataModel.CreatedAt)},
    created_by   AS {nameof(ShrinkageAbsenceDataModel.CreatedBy)},
    updated_at   AS {nameof(ShrinkageAbsenceDataModel.UpdatedAt)},
    updated_by   AS {nameof(ShrinkageAbsenceDataModel.UpdatedBy)},
    user_id      AS {nameof(ShrinkageAbsenceDataModel.UserId)},
    team_id      AS {nameof(ShrinkageAbsenceDataModel.TeamId)},
    absence_type AS {nameof(ShrinkageAbsenceDataModel.AbsenceType)},
    start_date   AS {nameof(ShrinkageAbsenceDataModel.StartDate)},
    end_date     AS {nameof(ShrinkageAbsenceDataModel.EndDate)}
FROM shrinkage_user_absences
WHERE id = @Id
  AND deleted_at IS NULL;
";

        var parameters = new { Id = id };

        await using var connection = await GetOpenConnectionAsync(token);

        return await connection.QuerySingleOrDefaultAsync<ShrinkageAbsenceDataModel>(sql, parameters);
    }

    public async Task<List<ShrinkageAbsenceDataModel>> GetAllAbsenceWithinDateRange(Guid userId, DateOnly startDate, DateOnly endDate, CancellationToken token)
    {
        const string sql = @$"
SELECT 
    a.id               AS {nameof(ShrinkageAbsenceDataModel.Id)},
    a.absence_type     AS {nameof(ShrinkageAbsenceDataModel.AbsenceType)},
    a.start_date       AS {nameof(ShrinkageAbsenceDataModel.StartDate)},
    a.end_date         AS {nameof(ShrinkageAbsenceDataModel.EndDate)}
FROM shrinkage_user_absences a
WHERE a.user_id = @UserId
  AND a.deleted_at IS NULL
  AND (
      a.start_date BETWEEN @StartDate AND @EndDate
      OR a.end_date BETWEEN @StartDate AND @EndDate
  );
";

        var parameters = new { UserId = userId, StartDate = startDate, EndDate = endDate };

        await using var connection = await GetOpenConnectionAsync(token);

        var result = await connection.QueryAsync<ShrinkageAbsenceDataModel>(sql, parameters);

        return result.ToList();
    }

}

