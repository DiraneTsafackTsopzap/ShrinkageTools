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

        public async Task<ShrinkageUserDataModel> Create( ShrinkageUserDataModel user,CancellationToken token)

        {
            await using var connection = new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);
            await connection.OpenAsync(token);

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



























        public async Task<ShrinkageUserDataModel?> GetUserByEmail(
            string email,
            CancellationToken token)
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

            await using var connection =
                new NpgsqlConnection(dapperDbContext.Connection.ConnectionString);

            await connection.OpenAsync(token);

            return await connection.QueryFirstOrDefaultAsync<ShrinkageUserDataModel>(
                sql,
                new { email = email.ToLower() });
        }
    }
}
