using Dapper;
using DataAccess.CRUD.DapperContext;
using DataAccess.CRUD.Modeles;
using Npgsql;

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
        public async Task<ShrinkageUserDataModel> Create(ShrinkageUserDataModel user, CancellationToken token)

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

                var newUser = await connection.QueryFirstAsync<ShrinkageUserDataModel>(insertUserSql, user, transaction);

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

                var paidTime = await connection.QueryFirstAsync<ShrinkageUserDataModel>(insertPaidTimeSql, paidTimeParams, transaction);

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

        public async Task<ShrinkageUserDataModel?> GetUserByEmail(string email, CancellationToken token)
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

            try
            {
                await using var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);
                await connection.OpenAsync(token);
                return await connection.QueryFirstAsync<ShrinkageActivityDataModel>(sql, activity);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"❌ Erreur dans CreateActivity: {ex.Message}");
                throw;
            }

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

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.ExecuteAsync(sql, activity);
        }

        public async Task<ShrinkageUserDataModel?> GetUserById(Guid id, CancellationToken token)
        {
            const string sql = @$"
SELECT 
    su.id            AS {nameof(ShrinkageUserDataModel.Id)},
    su.user_email    AS {nameof(ShrinkageUserDataModel.Email)},
    su.team_id       AS {nameof(ShrinkageUserDataModel.TeamId)},
    su.created_at    AS {nameof(ShrinkageUserDataModel.UserCreatedAt)}
FROM shrinkage_users su
WHERE su.id = @Id
  AND su.deleted_at IS NULL;
";

            var parameters = new { Id = id };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.QueryFirstOrDefaultAsync<ShrinkageUserDataModel>(sql, parameters);
        }

        public async Task<List<ShrinkageUserDailyValuesDataModel>> GetUserDailyValuesByUserId(
    Guid id,
    DateOnly startDate,
    DateOnly endDate,
    CancellationToken token)
        {
            const string sql = @$"
SELECT 
    id               AS {nameof(ShrinkageUserDailyValuesDataModel.Id)},
    status           AS {nameof(ShrinkageUserDailyValuesDataModel.Status)},
    comment          AS {nameof(ShrinkageUserDailyValuesDataModel.Comment)},
    shrinkage_date   AS {nameof(ShrinkageUserDailyValuesDataModel.ShrinkageDate)}
FROM shrinkage_user_daily_values
WHERE user_id = @Id
  AND shrinkage_date >= @StartDate
  AND shrinkage_date <= @EndDate
  AND deleted_at IS NULL;
";

            var parameters = new
            {
                Id = id,
                StartDate = startDate,
                EndDate = endDate
            };

            await using var connection = await GetOpenConnectionAsync(token);

            var result = await connection.QueryAsync<ShrinkageUserDailyValuesDataModel>(sql, parameters);

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

        public async Task<double> GetPaidTimeByUserIdAndDate(Guid userId, DateOnly shrinkageDate, CancellationToken token)
        {
            var dayOfWeek = shrinkageDate.DayOfWeek;

            var paidTimeColumn = dayOfWeek switch
            {
                DayOfWeek.Monday => "paid_time_monday",
                DayOfWeek.Tuesday => "paid_time_tuesday",
                DayOfWeek.Wednesday => "paid_time_wednesday",
                DayOfWeek.Thursday => "paid_time_thursday",
                DayOfWeek.Friday => "paid_time_friday",
                DayOfWeek.Saturday => "paid_time_saturday",
                DayOfWeek.Sunday => "paid_time_saturday", // ✅ fallback logique
                _ => throw new ArgumentOutOfRangeException(nameof(shrinkageDate), "Invalid day of week")
            };

            var sql = $@"
SELECT {paidTimeColumn}
FROM shrinkage_user_paid_time
WHERE user_id = @Id
  AND valid_from <= @Date
  AND deleted_at IS NULL
ORDER BY valid_from DESC, created_at DESC
LIMIT 1;
";

            var parameters = new { Id = userId, Date = shrinkageDate };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.ExecuteScalarAsync<double>(sql, parameters);
        }

        public async Task<ShrinkageUserDailyValuesDataModel?> GetDeletedUserDailyValuesByUserIdAndDate(Guid id, DateOnly date, CancellationToken token)
        {
            const string sql = $@"
SELECT DISTINCT ON (udv.id)
       udv.id                 AS {nameof(ShrinkageUserDailyValuesDataModel.Id)}, 
       udv.user_id            AS {nameof(ShrinkageUserDailyValuesDataModel.UserId)},
       udv.team_id            AS {nameof(ShrinkageUserDailyValuesDataModel.TeamId)},
       udv.paid_time          AS {nameof(ShrinkageUserDailyValuesDataModel.PaidTime)},
       udv.paid_time_off      AS {nameof(ShrinkageUserDailyValuesDataModel.PaidTimeOff)},
       udv.overtime           AS {nameof(ShrinkageUserDailyValuesDataModel.Overtime)},
       udv.vacation_time      AS {nameof(ShrinkageUserDailyValuesDataModel.VacationTime)},
       udv.status             AS {nameof(ShrinkageUserDailyValuesDataModel.Status)},
       udv.comment            AS {nameof(ShrinkageUserDailyValuesDataModel.Comment)},
       udv.created_at         AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedAt)},
       udv.created_by         AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedBy)},
       u1.user_email          AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedByUserEmail)},
       udv.updated_at         AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedAt)},
       udv.updated_by         AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedBy)},
       u2.user_email          AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedByUserEmail)},
       udv.deleted_at         AS {nameof(ShrinkageUserDailyValuesDataModel.DeletedAt)}
FROM shrinkage_user_daily_values udv
LEFT JOIN shrinkage_users u1 ON u1.id = udv.created_by
LEFT JOIN shrinkage_users u2 ON u2.id = udv.updated_by
WHERE udv.user_id = @Id
  AND DATE(udv.shrinkage_date) = @ShrinkageDate
  AND udv.deleted_at IS NOT NULL
ORDER BY udv.id, udv.created_at DESC;
";

            var parameters = new { Id = id, ShrinkageDate = date };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.QueryFirstOrDefaultAsync<ShrinkageUserDailyValuesDataModel>(sql, parameters);
        }



        public async Task<int> UpdateById(ShrinkageUserDailyValuesDataModel model, CancellationToken token)
        {
            const string sql = $@"
UPDATE shrinkage_user_daily_values
SET
    status         = @{nameof(ShrinkageUserDailyValuesDataModel.Status)},
    paid_time      = @{nameof(ShrinkageUserDailyValuesDataModel.PaidTime)},
    paid_time_off  = @{nameof(ShrinkageUserDailyValuesDataModel.PaidTimeOff)},
    overtime       = @{nameof(ShrinkageUserDailyValuesDataModel.Overtime)},
    vacation_time  = @{nameof(ShrinkageUserDailyValuesDataModel.VacationTime)},
    updated_at     = @{nameof(ShrinkageUserDailyValuesDataModel.UpdatedAt)},
    updated_by     = @{nameof(ShrinkageUserDailyValuesDataModel.UpdatedBy)},
    deleted_at     = NULL,
    deleted_by     = NULL
WHERE id = @{nameof(ShrinkageUserDailyValuesDataModel.Id)};
";

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.ExecuteAsync(sql, model);
        }

        public async Task<int> Create(ShrinkageUserDailyValuesDataModel dailyValue, CancellationToken token)
        {
            const string sql = $@"
INSERT INTO shrinkage_user_daily_values (
    id,
    user_id,
    team_id,
    paid_time,                                     
    paid_time_off,
    overtime,
    vacation_time,
    status,
    created_at,
    created_by,
    shrinkage_date
) 
VALUES (
    @{nameof(ShrinkageUserDailyValuesDataModel.Id)},
    @{nameof(ShrinkageUserDailyValuesDataModel.UserId)},
    @{nameof(ShrinkageUserDailyValuesDataModel.TeamId)},
    @{nameof(ShrinkageUserDailyValuesDataModel.PaidTime)},
    @{nameof(ShrinkageUserDailyValuesDataModel.PaidTimeOff)},
    @{nameof(ShrinkageUserDailyValuesDataModel.Overtime)},
    @{nameof(ShrinkageUserDailyValuesDataModel.VacationTime)},
    @{nameof(ShrinkageUserDailyValuesDataModel.Status)},
    @{nameof(ShrinkageUserDailyValuesDataModel.CreatedAt)},
    @{nameof(ShrinkageUserDailyValuesDataModel.CreatedBy)},
    @{nameof(ShrinkageUserDailyValuesDataModel.ShrinkageDate)}
);";

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.ExecuteAsync(sql, dailyValue);
        }


    }


}
