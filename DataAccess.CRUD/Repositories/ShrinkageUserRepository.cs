using Dapper;
using DataAccess.CRUD.DapperContext;
using DataAccess.CRUD.Modeles;
using Npgsql;
using System.Data;

namespace DataAccess.CRUD.Repositories
{
    public class ShrinkageUserRepository : IShrinkageUserRepository
    {
        private readonly DapperDbContext dapperDbContext;

        public ShrinkageUserRepository(DapperDbContext dapper)
        {
            dapperDbContext = dapper;
        }

        private async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken token)
        {
            var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);

            await connection.OpenAsync(token);
            return connection;
        }
        public async Task<ShrinkageUserDataModel> Create( ShrinkageUserDataModel user,CancellationToken token)

        {
            await using var connection = await GetOpenConnectionAsync(token);

            await using var transaction = await connection.BeginTransactionAsync(token);

            try
            {
                // 1️⃣ Insert user (user_role supprimé → OK)
                var insertUserSql = $@"
INSERT INTO shrinkage_users(
    id,
    user_email,
    team_id,
    created_at
)
VALUES(
    @{nameof(ShrinkageUserDataModel.Id)},
    @{nameof(ShrinkageUserDataModel.Email)},
    @{nameof(ShrinkageUserDataModel.TeamId)},
    @{nameof(ShrinkageUserDataModel.UserCreatedAt)}
)
RETURNING
    id          AS {nameof(ShrinkageUserDataModel.Id)},
    user_email AS {nameof(ShrinkageUserDataModel.Email)},
    team_id    AS {nameof(ShrinkageUserDataModel.TeamId)},
    created_at AS {nameof(ShrinkageUserDataModel.UserCreatedAt)};
";

                var newUser = await connection.QueryFirstAsync<ShrinkageUserDataModel>( insertUserSql,user,transaction);

                // 2️⃣ Insert paid time
                var insertPaidTimeSql = $@"
INSERT INTO shrinkage_user_paid_time(
    id,
    user_id,
    valid_from,
    created_at,
    created_by
)
VALUES (
    @id,
    @user_id,
    @valid_from,
    @created_at,
    @created_by
)

RETURNING
    id                  AS {nameof(ShrinkageUserDataModel.PaidTimeId)},
    paid_time_monday    AS {nameof(ShrinkageUserDataModel.PaidTimeMonday)},
    paid_time_tuesday   AS {nameof(ShrinkageUserDataModel.PaidTimeTuesday)},
    paid_time_wednesday AS {nameof(ShrinkageUserDataModel.PaidTimeWednesday)},
    paid_time_thursday  AS {nameof(ShrinkageUserDataModel.PaidTimeThursday)},
    paid_time_friday    AS {nameof(ShrinkageUserDataModel.PaidTimeFriday)},
    paid_time_saturday  AS {nameof(ShrinkageUserDataModel.PaidTimeSaturday)},
    valid_from          AS {nameof(ShrinkageUserDataModel.ValidFrom)},
    created_at          AS {nameof(ShrinkageUserDataModel.PaidTimeCreatedAt)};
";

                var paidTimeParams = new
                {
                    id = Guid.NewGuid(),
                    user_id = newUser.Id,
                    valid_from = user.ValidFrom,
                    created_at = user.UserCreatedAt,
                    created_by = user.CreatedBy

                };

                var paidTime = await connection.QueryFirstAsync<ShrinkageUserDataModel>( insertPaidTimeSql, paidTimeParams,transaction);

                // 3️⃣ Merge , Retourne l'objet Shrinkage
                newUser.PaidTimeId = paidTime.PaidTimeId;
                newUser.PaidTimeMonday = paidTime.PaidTimeMonday;
                newUser.PaidTimeTuesday = paidTime.PaidTimeTuesday;
                newUser.PaidTimeWednesday = paidTime.PaidTimeWednesday;
                newUser.PaidTimeThursday = paidTime.PaidTimeThursday;
                newUser.PaidTimeFriday = paidTime.PaidTimeFriday;
                newUser.PaidTimeSaturday = paidTime.PaidTimeSaturday;
                newUser.ValidFrom = paidTime.ValidFrom;
                newUser.PaidTimeCreatedAt = paidTime.PaidTimeCreatedAt;
                newUser.PaidTimeCreatedByUserEmail = user.Email;

                await transaction.CommitAsync(token);

                return newUser;
            }
            catch
            {
                await transaction.RollbackAsync(token);
                throw;
            }
        }

        public async Task<ShrinkageUserDataModel?> GetUserByEmail( string email,CancellationToken token)
        {
            var sql = $@"
SELECT DISTINCT ON (su.id)
    su.id                  AS {nameof(ShrinkageUserDataModel.Id)},
    su.user_email          AS {nameof(ShrinkageUserDataModel.Email)},
    su.team_id             AS {nameof(ShrinkageUserDataModel.TeamId)},

    ph.id                  AS {nameof(ShrinkageUserDataModel.PaidTimeId)},
    ph.paid_time_monday    AS {nameof(ShrinkageUserDataModel.PaidTimeMonday)},
    ph.paid_time_tuesday   AS {nameof(ShrinkageUserDataModel.PaidTimeTuesday)},
    ph.paid_time_wednesday AS {nameof(ShrinkageUserDataModel.PaidTimeWednesday)},
    ph.paid_time_thursday  AS {nameof(ShrinkageUserDataModel.PaidTimeThursday)},
    ph.paid_time_friday    AS {nameof(ShrinkageUserDataModel.PaidTimeFriday)},
    ph.paid_time_saturday  AS {nameof(ShrinkageUserDataModel.PaidTimeSaturday)},

    ph.valid_from          AS {nameof(ShrinkageUserDataModel.ValidFrom)},
    ph.created_at          AS {nameof(ShrinkageUserDataModel.PaidTimeCreatedAt)},
    u1.user_email          AS {nameof(ShrinkageUserDataModel.PaidTimeCreatedByUserEmail)}
FROM shrinkage_users su
JOIN shrinkage_user_paid_time ph
    ON su.id = ph.user_id
LEFT JOIN shrinkage_users u1
    ON ph.created_by = u1.id
WHERE lower(su.user_email) = @email
  AND ph.valid_from <= NOW()
  AND ph.deleted_at IS NULL
ORDER BY su.id, ph.valid_from DESC;
";
            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.QueryFirstOrDefaultAsync<ShrinkageUserDataModel>(
                sql,
                new { email = email.ToLower() });
        }

        public async Task<Guid> GetUserIdByEmail(string email, CancellationToken token)
        {
            const string sql = @"
SELECT su.id
FROM shrinkage_users su
WHERE lower(su.user_email) = @email
  AND su.deleted_at IS NULL;
";

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.QueryFirstOrDefaultAsync<Guid>(
                sql,
                new { email = email.ToLower() }
            );
        }

        public async Task<ShrinkageUserDailyValuesDataModel?> GetActiveUserDailyValuesByUserIdAndDate(
    Guid userId,
    DateOnly shrinkageDate,
    CancellationToken token)
        {
            const string sql = $@"
SELECT 
    udv.id                  AS {nameof(ShrinkageUserDailyValuesDataModel.Id)},
    udv.user_id             AS {nameof(ShrinkageUserDailyValuesDataModel.UserId)},
    udv.team_id             AS {nameof(ShrinkageUserDailyValuesDataModel.TeamId)},
    udv.paid_time           AS {nameof(ShrinkageUserDailyValuesDataModel.PaidTime)},
    udv.paid_time_off       AS {nameof(ShrinkageUserDailyValuesDataModel.PaidTimeOff)},
    udv.overtime            AS {nameof(ShrinkageUserDailyValuesDataModel.Overtime)},
    udv.vacation_time       AS {nameof(ShrinkageUserDailyValuesDataModel.VacationTime)},
    udv.status              AS {nameof(ShrinkageUserDailyValuesDataModel.Status)},
    udv.comment             AS {nameof(ShrinkageUserDailyValuesDataModel.Comment)},
    udv.created_at          AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedAt)},
    udv.created_by          AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedBy)},
    u1.user_email           AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedByUserEmail)},
    udv.updated_at          AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedAt)},
    udv.updated_by          AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedBy)},
    u2.user_email           AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedByUserEmail)},
    udv.shrinkage_date      AS {nameof(ShrinkageUserDailyValuesDataModel.ShrinkageDate)}
FROM shrinkage_user_daily_values udv
LEFT JOIN shrinkage_users u1 ON u1.id = udv.created_by
LEFT JOIN shrinkage_users u2 ON u2.id = udv.updated_by
WHERE udv.user_id = @userId
  AND DATE(udv.shrinkage_date) = @shrinkageDate
  AND udv.deleted_at IS NULL;
";

            var parameters = new
            {
                userId,
                shrinkageDate
            };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.QueryFirstOrDefaultAsync<ShrinkageUserDailyValuesDataModel>(sql, parameters);
        }


        public async Task<ShrinkageActivityDataModel?> GetActivityById(Guid id, CancellationToken token)
        {
            const string sql = $@"
SELECT 
    id                   AS {nameof(ShrinkageActivityDataModel.Id)},
    created_at           AS {nameof(ShrinkageActivityDataModel.CreatedAt)},
    created_by           AS {nameof(ShrinkageActivityDataModel.CreatedBy)},
    updated_at           AS {nameof(ShrinkageActivityDataModel.UpdatedAt)},
    updated_by           AS {nameof(ShrinkageActivityDataModel.UpdatedBy)},
    user_id              AS {nameof(ShrinkageActivityDataModel.UserId)},
    team_id              AS {nameof(ShrinkageActivityDataModel.TeamId)},
    started_at           AS {nameof(ShrinkageActivityDataModel.StartedAt)},
    stopped_at           AS {nameof(ShrinkageActivityDataModel.StoppedAt)},
    activity_type        AS {nameof(ShrinkageActivityDataModel.ActivityType)},
    activity_track_type  AS {nameof(ShrinkageActivityDataModel.ActivityTrackType)}
FROM shrinkage_user_activities
WHERE id = @id
  AND deleted_at IS NULL;
";

            var parameters = new { id };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.QuerySingleOrDefaultAsync<ShrinkageActivityDataModel>(sql, parameters);
        }

        public async Task<ShrinkageActivityDataModel> CreateActivity(ShrinkageActivityDataModel activity, CancellationToken token)
        {
            const string sql = $@"
INSERT INTO shrinkage_user_activities (
    id,
    created_at,
    created_by,
    user_id,
    team_id,
    started_at,
    stopped_at,
    activity_type,
    activity_track_type
)
VALUES (
    @{nameof(ShrinkageActivityDataModel.Id)},
    @{nameof(ShrinkageActivityDataModel.CreatedAt)},
    @{nameof(ShrinkageActivityDataModel.CreatedBy)},
    @{nameof(ShrinkageActivityDataModel.UserId)},
    @{nameof(ShrinkageActivityDataModel.TeamId)},
    @{nameof(ShrinkageActivityDataModel.StartedAt)},
    @{nameof(ShrinkageActivityDataModel.StoppedAt)},
    @{nameof(ShrinkageActivityDataModel.ActivityType)},
    @{nameof(ShrinkageActivityDataModel.ActivityTrackType)}
)
RETURNING
    id                      AS {nameof(ShrinkageActivityDataModel.Id)},
    created_at              AS {nameof(ShrinkageActivityDataModel.CreatedAt)},
    created_by              AS {nameof(ShrinkageActivityDataModel.CreatedBy)},
    user_id                 AS {nameof(ShrinkageActivityDataModel.UserId)},
    team_id                 AS {nameof(ShrinkageActivityDataModel.TeamId)},
    started_at              AS {nameof(ShrinkageActivityDataModel.StartedAt)},
    stopped_at              AS {nameof(ShrinkageActivityDataModel.StoppedAt)},
    activity_type           AS {nameof(ShrinkageActivityDataModel.ActivityType)},
    activity_track_type     AS {nameof(ShrinkageActivityDataModel.ActivityTrackType)};
";

            await using var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);
            await connection.OpenAsync(token);

            return await connection.QueryFirstAsync<ShrinkageActivityDataModel>(sql, activity);
        }

        public async Task<int> UpdateActivityById(ShrinkageActivityDataModel activity, CancellationToken token)
        {
            const string sql = $@"
UPDATE shrinkage_user_activities
SET
    updated_at          = @{nameof(ShrinkageActivityDataModel.UpdatedAt)},
    updated_by          = @{nameof(ShrinkageActivityDataModel.UpdatedBy)},
    team_id             = @{nameof(ShrinkageActivityDataModel.TeamId)},
    started_at          = @{nameof(ShrinkageActivityDataModel.StartedAt)},
    stopped_at          = @{nameof(ShrinkageActivityDataModel.StoppedAt)},
    activity_type       = @{nameof(ShrinkageActivityDataModel.ActivityType)},
    activity_track_type = @{nameof(ShrinkageActivityDataModel.ActivityTrackType)}
WHERE id = @{nameof(ShrinkageActivityDataModel.Id)};
";

            await using var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);
            await connection.OpenAsync(token);

            return await connection.ExecuteAsync(sql, activity);
        }

    }
}
