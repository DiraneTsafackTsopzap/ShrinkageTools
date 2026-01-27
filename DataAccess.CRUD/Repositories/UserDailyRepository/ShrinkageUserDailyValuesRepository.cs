using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using DataAccess.CRUD.DapperContext;
using DataAccess.CRUD.Modeles;
using Npgsql;

namespace DataAccess.CRUD.Repositories.UserDailyRepository
{
    public class ShrinkageUserDailyValuesRepository : IShrinkageUserDailyValuesRepository
    {
        private readonly DapperDbContext dapperDbContext;

        public ShrinkageUserDailyValuesRepository(DapperDbContext dapper)
        {
            dapperDbContext = dapper;
        }

        public async Task<int> DeleteById(ShrinkageUserDailyValuesDataModel model, CancellationToken token)
        {
            const string sql = @$"
UPDATE shrinkage_user_daily_values
SET
    deleted_at = @DeletedAt,
    deleted_by = @DeletedBy
WHERE id = @Id;
";

            var parameters = new
            {
                model.Id,
                model.DeletedAt,
                model.DeletedBy
            };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.ExecuteAsync(sql, parameters);
        }

        public async Task<int> Create(ShrinkageUserDailyValuesDataModel dailyValue, CancellationToken token)
        {
            const string sql = @$"
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
    @Id,
    @UserId,
    @TeamId,
    @PaidTime,
    @PaidTimeOff,
    @Overtime,
    @VacationTime,
    @Status,
    @CreatedAt,
    @CreatedBy,
    @ShrinkageDate
);
";

            var parameters = new
            {
                dailyValue.Id,
                dailyValue.UserId,
                dailyValue.TeamId,
                dailyValue.PaidTime,
                dailyValue.PaidTimeOff,
                dailyValue.Overtime,
                dailyValue.VacationTime,
                dailyValue.Status,
                dailyValue.CreatedAt,
                dailyValue.CreatedBy,
                dailyValue.ShrinkageDate
            };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.ExecuteAsync(sql, parameters);
        }

        private async Task<NpgsqlConnection> GetOpenConnectionAsync(CancellationToken token)
        {
            var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);

            await connection.OpenAsync(token);
            return connection;
        }


        public async Task<ShrinkageUserDailyValuesDataModel?> GetUserDailyValuesByUserIdAndDate(
    Guid id,
    DateOnly date,
    CancellationToken token)
        {
            const string sql = @$"
SELECT DISTINCT ON (udv.id)
       udv.id            AS {nameof(ShrinkageUserDailyValuesDataModel.Id)}, 
       udv.user_id       AS {nameof(ShrinkageUserDailyValuesDataModel.UserId)},
       udv.team_id       AS {nameof(ShrinkageUserDailyValuesDataModel.TeamId)},
       udv.paid_time     AS {nameof(ShrinkageUserDailyValuesDataModel.PaidTime)},
       udv.paid_time_off AS {nameof(ShrinkageUserDailyValuesDataModel.PaidTimeOff)},
       udv.overtime      AS {nameof(ShrinkageUserDailyValuesDataModel.Overtime)},
       udv.vacation_time AS {nameof(ShrinkageUserDailyValuesDataModel.VacationTime)},
       udv.status        AS {nameof(ShrinkageUserDailyValuesDataModel.Status)},
       udv.comment       AS {nameof(ShrinkageUserDailyValuesDataModel.Comment)},
       udv.created_at    AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedAt)},
       udv.created_by    AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedBy)},
       u1.user_email     AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedByUserEmail)},
       udv.updated_at    AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedAt)},
       udv.updated_by    AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedBy)},
       u2.user_email     AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedByUserEmail)},
       udv.deleted_at    AS {nameof(ShrinkageUserDailyValuesDataModel.DeletedAt)}
FROM shrinkage_user_daily_values udv
LEFT JOIN shrinkage_users u1 ON u1.id = udv.created_by
LEFT JOIN shrinkage_users u2 ON u2.id = udv.updated_by
WHERE udv.user_id = @UserId
  AND udv.shrinkage_date = @ShrinkageDate
  AND udv.deleted_at IS NULL
ORDER BY udv.id, udv.created_at DESC;
";

            var parameters = new
            {
                UserId = id,
                ShrinkageDate = date
            };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.QueryFirstOrDefaultAsync<ShrinkageUserDailyValuesDataModel>(sql, parameters);
        }


        public async Task<List<ShrinkageUserDailyValuesDataModel>> GetUserDailyValuesByUserIdAndDateRange(
    Guid userId,
    DateOnly startDate,
    DateOnly endDate,
    CancellationToken token)
        {
            const string sql = @$"
WITH RankedDailyValues AS (
    SELECT 
        udv.id            AS {nameof(ShrinkageUserDailyValuesDataModel.Id)},
        udv.user_id       AS {nameof(ShrinkageUserDailyValuesDataModel.UserId)},
        udv.team_id       AS {nameof(ShrinkageUserDailyValuesDataModel.TeamId)},
        udv.paid_time     AS {nameof(ShrinkageUserDailyValuesDataModel.PaidTime)},
        udv.paid_time_off AS {nameof(ShrinkageUserDailyValuesDataModel.PaidTimeOff)},
        udv.overtime      AS {nameof(ShrinkageUserDailyValuesDataModel.Overtime)},
        udv.vacation_time AS {nameof(ShrinkageUserDailyValuesDataModel.VacationTime)},
        udv.status        AS {nameof(ShrinkageUserDailyValuesDataModel.Status)},
        udv.comment       AS {nameof(ShrinkageUserDailyValuesDataModel.Comment)},
        udv.created_at    AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedAt)},
        udv.created_by    AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedBy)},
        u1.user_email     AS {nameof(ShrinkageUserDailyValuesDataModel.CreatedByUserEmail)},
        udv.updated_at    AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedAt)},
        udv.updated_by    AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedBy)},
        u2.user_email     AS {nameof(ShrinkageUserDailyValuesDataModel.UpdatedByUserEmail)},
        udv.deleted_at    AS {nameof(ShrinkageUserDailyValuesDataModel.DeletedAt)},
        udv.shrinkage_date AS ShrinkageDate,
        ROW_NUMBER() OVER (
            PARTITION BY DATE(udv.shrinkage_date)
            ORDER BY udv.created_at DESC
        ) AS rn
    FROM shrinkage_user_daily_values udv
    LEFT JOIN shrinkage_users u1 ON u1.id = udv.created_by
    LEFT JOIN shrinkage_users u2 ON u2.id = udv.updated_by
    WHERE udv.user_id = @UserId
      AND udv.shrinkage_date >= @StartDate
      AND udv.shrinkage_date <= @EndDate
      AND udv.deleted_at IS NULL
)
SELECT *
FROM RankedDailyValues
WHERE rn = 1;
";

            var parameters = new
            {
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate
            };

            await using var connection = await GetOpenConnectionAsync(token);

            var response = await connection.QueryAsync<ShrinkageUserDailyValuesDataModel>(sql, parameters);

            return response.ToList();
        }



        public async Task<int> UpdateById(ShrinkageUserDailyValuesDataModel model, CancellationToken token)
        {
            const string sql = @$"
UPDATE shrinkage_user_daily_values
SET
    status         = @Status,
    paid_time      = @PaidTime,
    paid_time_off  = @PaidTimeOff,
    overtime       = @Overtime,
    vacation_time  = @VacationTime,
    updated_at     = @UpdatedAt,
    updated_by     = @UpdatedBy,
    deleted_at     = NULL,
    deleted_by     = NULL
WHERE id = @Id;
";

            var parameters = new
            {
                model.Id,
                model.Status,
                model.PaidTime,
                model.PaidTimeOff,
                model.Overtime,
                model.VacationTime,
                model.UpdatedAt,
                model.UpdatedBy
            };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.ExecuteAsync(sql, parameters);
        }

        public async Task<int> UpdateStatusAndCommentById(ShrinkageUserDailyValuesDataModel model, CancellationToken token)
        {
            const string sql = @$"
UPDATE shrinkage_user_daily_values
SET
    status     = @Status,
    comment    = @Comment,
    updated_at = @UpdatedAt,
    updated_by = @UpdatedBy
WHERE id = @Id;
";

            var parameters = new
            {
                model.Id,
                model.Status,
                model.Comment,
                model.UpdatedAt,
                model.UpdatedBy
            };

            await using var connection = await GetOpenConnectionAsync(token);

            return await connection.ExecuteAsync(sql, parameters);
        }


    }
}
