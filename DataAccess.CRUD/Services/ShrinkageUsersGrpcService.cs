using System.Diagnostics;
using System.Text;
using DataAccess.CRUD.Extensions;
using DataAccess.CRUD.Modeles;
using DataAccess.CRUD.Repositories;
using DataAccess.CRUD.Repositories.TeamsRepository;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcShrinkageServiceTraining.Protobuf;
using Status = GrpcShrinkageServiceTraining.Protobuf.Status;


namespace DataAccess.CRUD.Services
{
    public class ShrinkageUsersGrpcService : ShrinkageProtoService.ShrinkageProtoServiceBase
    {
        private readonly IShrinkageUserRepository _shrinkageUsersRepository;
        private readonly IShrinkageTeamsRepository _shrinkageTeamsRepository;

        public ShrinkageUsersGrpcService(IShrinkageTeamsRepository shrinkageTeamsRepository, IShrinkageUserRepository shrinkageUsersRepository)
        {
            _shrinkageUsersRepository = shrinkageUsersRepository;
            _shrinkageTeamsRepository = shrinkageTeamsRepository;
        }


        public override async Task<GetUserByEmailResponse> GetUserByEmail(GetUserByEmailRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation id {request.CorrelationId} email is required");
            }

            try
            {
                var user = await _shrinkageUsersRepository.GetUserByEmail(request.Email, context.CancellationToken);

                if (user == null)
                {
                    var userId = Guid.NewGuid();

                    var newUser = new ShrinkageUserDataModel
                    {
                        Id = userId,
                        Email = request.Email,
                        UserCreatedAt = DateTime.UtcNow,  // Createdat ici est en UTC
                        CreatedBy = userId,
                        ValidFrom = DateOnly.FromDateTime(DateTime.Now),  // // ValidFrom ici est en UTC


                        // Apres cherche comment enlever le TeamId ci , car on va l'ajouter personnellement plus tard
                        TeamId = Guid.NewGuid()
                    };

                    user = await _shrinkageUsersRepository.Create(newUser, context.CancellationToken);
                }

                return new GetUserByEmailResponse
                {
                    User = new User
                    {
                        UserId = new AppUuid { Value = user.Id.ToString() },

                        TeamId = user.TeamId.HasValue ? new AppUuid { Value = user.TeamId.Value.ToString() } : null,

                        Email = user.Email ?? request.Email,

                        PaidTime =                         {
                            new GrpcShrinkageServiceTraining.Protobuf.PaidTime
                            {
                                 PaidTimeMonday = Duration.FromTimeSpan(TimeSpan.FromMinutes(user.PaidTimeMonday)),
                        PaidTimeTuesday = Duration.FromTimeSpan(TimeSpan.FromMinutes(user.PaidTimeTuesday)),
                        PaidTimeWednesday = Duration.FromTimeSpan(TimeSpan.FromMinutes(user.PaidTimeWednesday)),
                        PaidTimeThursday = Duration.FromTimeSpan(TimeSpan.FromMinutes(user.PaidTimeThursday)),
                        PaidTimeFriday = Duration.FromTimeSpan(TimeSpan.FromMinutes(user.PaidTimeFriday)),
                        PaidTimeSaturday = Duration.FromTimeSpan(TimeSpan.FromMinutes(user.PaidTimeSaturday)),
                        ValidFrom = NormalizeUtc(user.ValidFrom.ToDateTime(TimeOnly.MinValue)).ToTimestamp(),
                        CreatedAt = NormalizeUtc(user.PaidTimeCreatedAt.ToUniversalTime()).ToTimestamp(),
                        CreatedBy = user.PaidTimeCreatedByUserEmail,
                        Id = user.PaidTimeId,
                            }
                        }
                    }
                };
            }
            catch (Exception ex)
            {

                throw new RpcException(new Grpc.Core.Status(StatusCode.Internal, ex.Message));
            }
        }

        private static DateTime NormalizeUtc(DateTime dt) =>
        dt.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dt, DateTimeKind.Utc),
            DateTimeKind.Local => dt.ToUniversalTime(),
            _ => dt
        };


        // Get Teams
        public override async Task<GetTeamsResponse> GetTeams(GetTeamsRequest request, ServerCallContext context)
        {
            try
            {
                var teams = await _shrinkageTeamsRepository.GetTeams(context.CancellationToken);

                return new GetTeamsResponse
                {
                    Teams =
            {
                teams.Select(x => new Team
                {
                    Id = AppUuid.FromGuid(x.Id),
                    Name = x.TeamName,
                    TeamLeadIds =
                    {
                        (x.TeamLeadIds ?? Array.Empty<Guid>())
                            .Select(AppUuid.FromGuid)
                    }
                })
            }
                };
            }
            catch (Exception ex)
            {
                throw new RpcException(
                    new Grpc.Core.Status(StatusCode.Internal, ex.Message)
                );
            }
        }


        // Save Activity
        public override async Task<SaveActivityResponse> SaveActivity(SaveActivityRequest request, ServerCallContext context)
        {
            if (!request.UserId.TryParseToGuidNotNullOrEmpty(out var userId))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} userid is required");
            }

            if (request.Activity == null || !request.Activity.Id.TryParseToGuidNotNullOrEmpty(out var activityId))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} either activity or activity id is not specified");
            }

            if (request.Activity.ActivityType == ActivityType.Unspecified)
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} activity type id is not specified");
            }

            if (!request.Activity.TeamId.TryParseToGuidNotNullOrEmpty(out var teamId))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} team id is required");
            }

            if (request.Activity.DateTimeRange?.StartedAt is null)
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} either Activity DateTime range or activity started at is not specified");
            }

            if (request.Activity.ActivityTrackType == ActivityTrackType.Unspecified)
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} activity track type is required");
            }

            if (request.Activity.DateTimeRange.StartedAt is not null && request.Activity.DateTimeRange.StoppedAt is not null)
            {
                if (request.Activity.DateTimeRange.StartedAt >= request.Activity.DateTimeRange.StoppedAt)
                    throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} activity started at cannot be greater than stopped at");
            }

            var cancellationToken = context.CancellationToken;
            var userActivity = request.Activity;
            var shrinkageDate = request.Activity.DateTimeRange.StartedAt!.FromTimeStampToDate();

            var dailyValue = await _shrinkageUsersRepository.GetActiveUserDailyValuesByUserIdAndDate(userId, shrinkageDate, cancellationToken);

            if (dailyValue == null)
            {
                throw RpcExceptions.NotFound($"Error with correlation Id {request.CorrelationId} daily user values for user id {userId} not found");
            }

            var teamDetails = await _shrinkageTeamsRepository.GetTeamById(teamId, cancellationToken);
            if (teamDetails is null)
                throw RpcExceptions.NotFound($"Error with correlation Id {request.CorrelationId} Team Id {teamId} does not exist");

            var activity = new ShrinkageActivityDataModel
            {
                Id = activityId,
                UserId = userId,
                TeamId = teamId,
                StartedAt = userActivity.DateTimeRange.StartedAt!.ToDateTime().ToUniversalTime(),
                StoppedAt = userActivity.DateTimeRange?.StoppedAt?.ToDateTime().ToUniversalTime(),

                //StartedAt = userActivity.DateTimeRange.StartedAt!.ToDateTime().ConvertUtcToGermanDateTime(),
                //StoppedAt = userActivity.DateTimeRange?.StoppedAt?.ToDateTime().ConvertUtcToGermanDateTime(),
                ActivityType = userActivity.ActivityType.ConvertFromApiActivityType(),
                ActivityTrackType = userActivity.ActivityTrackType.ConvertFromApiActivityTrackType(),
            };
            var existingActivity = await _shrinkageUsersRepository.GetActivityById(activityId, cancellationToken);

            if (existingActivity is null)
            {
                var createdBy = userActivity.CreatedBy;
                var createdByUserId = await _shrinkageUsersRepository.GetUserIdByEmail(createdBy, cancellationToken);
                if (createdByUserId == Guid.Empty)
                    throw RpcExceptions.NotFound($"Error with correlation id {request.CorrelationId} user with email {createdBy} does not exist");
                activity.CreatedAt = DateTime.UtcNow;
                activity.CreatedBy = createdByUserId;
                await _shrinkageUsersRepository.CreateActivity(activity, cancellationToken);
            }
            else
            {
                var updatedBy = userActivity.UpdatedBy;
                var updatedByUserId = await _shrinkageUsersRepository.GetUserIdByEmail(updatedBy, cancellationToken);
                if (updatedByUserId == Guid.Empty)
                    throw RpcExceptions.NotFound($"Error with correlation id {request.CorrelationId} user with email {updatedBy} does not exist");
                activity.UpdatedAt = DateTime.UtcNow;
                activity.UpdatedBy = updatedByUserId;
                await _shrinkageUsersRepository.UpdateActivityById(activity, cancellationToken);
            }

            return new SaveActivityResponse();
        }


        // Get User Daily Summary
        //Cette méthode gRPC reçoit une requête contenant CorrelationId et UserId, 
        //et retourne un résumé des activités journalières de l'utilisateur (UserDailySummaryResponse).
        public override async Task<GetUserDailySummaryResponse> GetUserDailySummary(GetUserDailySummaryRequest request, ServerCallContext context)
        {
            if (!request.UserId.TryParseToGuidNotNullOrEmpty(out var userId))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation id {request.CorrelationId} invalid UserId: {userId}");
            }

            var cancellationToken = context.CancellationToken;

            //1-  Vérifier si l'utilisateur existe
            var user = await _shrinkageUsersRepository.GetUserById(userId, cancellationToken);

            if (user is null)
            {
                throw RpcExceptions.NotFound($"Error with correlation id {request.CorrelationId} user with Id {userId} not found");
            }

            //2- Vérifier si l'utilisateur a un TeamId valide
            if (user.TeamId is null || user.TeamId.Value == Guid.Empty)
            {
                throw RpcExceptions.NotFound($"Error with correlation id {request.CorrelationId} TeamId is not set for the user with Id {userId}");
            }

            var teamId = user.TeamId.Value;
            var userCreatedDate = DateOnly.FromDateTime(user.UserCreatedAt);
            var startDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-30));
            var endDate = DateOnly.FromDateTime(DateTime.UtcNow);
            if (endDate < userCreatedDate) endDate = userCreatedDate;


            //3- Récupérer les données nécessaires en parallèle : Jours de Travail de l'utilisateur, Absences et Jours Fériés
            var userDailyValuesTask = _shrinkageUsersRepository.GetUserDailyValuesByUserId(userId, userCreatedDate, endDate, cancellationToken);
            var absencesTask = _shrinkageUsersRepository.GetAbsencesByUserId(userId, cancellationToken);
            var publicHolidaysTask = _shrinkageUsersRepository.GetTeamsPublicHolidaysByTeamId(teamId, cancellationToken);

            await Task.WhenAll(userDailyValuesTask, absencesTask, publicHolidaysTask);

            var userDailyValues = await userDailyValuesTask;
            var absences = await absencesTask;
            var publicHolidays = await publicHolidaysTask;

            var response = new GetUserDailySummaryResponse();
            var summaries = response.UserDailySummaries;

            var existingDates = new HashSet<DateOnly>();

            foreach (var daily in userDailyValues)
            {
                var date = daily.ShrinkageDate;
                existingDates.Add(date);
                summaries.Add(new GetUserDailySummaryResponse.Types.UserDailySummary { Id = daily.Id, Date = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToTimestamp(), WorkingDay = new GetUserDailySummaryResponse.Types.UserDailySummary.Types.WorkingDay { Status = daily.Status.ConvertToApiStatus() } });
            }

            foreach (var absence in absences)
            {
                var start = absence.StartDate;
                var end = absence.EndDate;
                if (end < start) continue;

                for (var date = start; date <= end; date = date.AddDays(1))
                {
                    existingDates.Add(date);
                    summaries.Add(new GetUserDailySummaryResponse.Types.UserDailySummary { Id = absence.Id, Date = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToUniversalTime().ToTimestamp(), Absence = new GetUserDailySummaryResponse.Types.UserDailySummary.Types.Absence { Type = absence.AbsenceType.ConvertToApiAbsenceType() } });
                }
            }

            foreach (var publicHoliday in publicHolidays)
            {
                var date = publicHoliday.AffectedDay;
                var existingAbsence = response.UserDailySummaries.FirstOrDefault(x => DateOnly.FromDateTime(x.Date.ToDateTime()) == date && x.AttendanceTypeCase == GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.Absence);
                var existingDailyValue = response.UserDailySummaries.FirstOrDefault(x => DateOnly.FromDateTime(x.Date.ToDateTime()) == date && x.AttendanceTypeCase == GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.WorkingDay);

                if (existingAbsence != null)
                {
                    continue;
                }

                if (existingDailyValue != null)
                {
                    response.UserDailySummaries.Remove(existingDailyValue);
                    response.UserDailySummaries.Add(new GetUserDailySummaryResponse.Types.UserDailySummary { Id = existingDailyValue.Id, Date = AsUtcDateTime(date).ToTimestamp(), PublicHoliday = new GetUserDailySummaryResponse.Types.UserDailySummary.Types.PublicHoliday { Status = existingDailyValue.WorkingDay.Status, } });
                }
                else
                {
                    response.UserDailySummaries.Add(new GetUserDailySummaryResponse.Types.UserDailySummary { Id = publicHoliday.Id, Date = AsUtcDateTime(date).ToTimestamp(), PublicHoliday = new GetUserDailySummaryResponse.Types.UserDailySummary.Types.PublicHoliday() });
                    existingDates.Add(date);
                }
            }

            for (var date = endDate; date >= userCreatedDate; date = date.AddDays(-1))
            {
                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    var existingWorkingDay = summaries.FirstOrDefault(x => DateOnly.FromDateTime(x.Date.ToDateTime()) == date &&
                                                                           x.AttendanceTypeCase == GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.WorkingDay);

                    if (existingWorkingDay != null)
                    {
                        summaries.Remove(existingWorkingDay);
                        summaries.Add(new GetUserDailySummaryResponse.Types.UserDailySummary { Id = existingWorkingDay.Id, Date = AsUtcDateTime(date).ToTimestamp(), Weekend = new GetUserDailySummaryResponse.Types.UserDailySummary.Types.Weekend { Status = existingWorkingDay.WorkingDay.Status } });
                    }
                    else if (!existingDates.Contains(date))
                    {
                        summaries.Add(new GetUserDailySummaryResponse.Types.UserDailySummary { Id = Guid.NewGuid(), Date = AsUtcDateTime(date).ToTimestamp(), Weekend = new GetUserDailySummaryResponse.Types.UserDailySummary.Types.Weekend() });
                    }

                    existingDates.Add(date);
                    continue;
                }

                if (!existingDates.Contains(date))
                {
                    var paidTime = await _shrinkageUsersRepository.GetPaidTimeByUserIdAndDate(userId, date, cancellationToken);

                    var existingDeletedUserDailyValue = await _shrinkageUsersRepository.GetDeletedUserDailyValuesByUserIdAndDate(userId, date, cancellationToken);
                    if (existingDeletedUserDailyValue is not null)
                    {
                        existingDeletedUserDailyValue.Status = ShrinkageConstants.Pending;
                        existingDeletedUserDailyValue.UpdatedAt = DateTime.UtcNow;
                        existingDeletedUserDailyValue.UpdatedBy = userId;

                        await _shrinkageUsersRepository.UpdateById(existingDeletedUserDailyValue, cancellationToken);
                        summaries.Add(new GetUserDailySummaryResponse.Types.UserDailySummary { Id = existingDeletedUserDailyValue.Id, Date = AsUtcDateTime(date).ToTimestamp(), WorkingDay = new GetUserDailySummaryResponse.Types.UserDailySummary.Types.WorkingDay { Status = Status.Pending } });
                    }
                    else
                    {
                        var newId = Guid.NewGuid();
                        await _shrinkageUsersRepository.Create(new ShrinkageUserDailyValuesDataModel
                        {
                            Id = newId,
                            UserId = userId,
                            TeamId = teamId,
                            PaidTime = paidTime,
                            Status = ShrinkageConstants.Pending,
                            ShrinkageDate = date,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = userId
                        }, cancellationToken);
                        summaries.Add(new GetUserDailySummaryResponse.Types.UserDailySummary { Id = newId, Date = AsUtcDateTime(date).ToTimestamp(), WorkingDay = new GetUserDailySummaryResponse.Types.UserDailySummary.Types.WorkingDay { Status = Status.Pending } });
                    }

                    existingDates.Add(date);
                }
            }

            var itemsToBeRemoved = summaries
                .Where(x =>
                    DateOnly.FromDateTime(x.Date.ToDateTime()) > endDate ||
                    DateOnly.FromDateTime(x.Date.ToDateTime()) < userCreatedDate ||
                    (DateOnly.FromDateTime(x.Date.ToDateTime()) <= startDate &&
                     (
                         x.AttendanceTypeCase == GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.PublicHoliday ||
                         x.AttendanceTypeCase == GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.Absence ||
                         (x.AttendanceTypeCase == GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.WorkingDay &&
                          x.WorkingDay.Status != Status.Pending &&
                          x.WorkingDay.Status != Status.Rejected) ||
                         (x.AttendanceTypeCase == GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.Weekend &&
                          x.Weekend.Status != Status.Pending &&
                          x.Weekend.Status != Status.Rejected &&
                          x.Weekend.Status != Status.Unspecified)
                     )))
                .ToList();

            foreach (var item in itemsToBeRemoved)
            {
                summaries.Remove(item);
            }

            return response;
        }

        private static DateTime AsUtcDateTime(DateOnly date) =>
            date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
    }
}





// Tjrs ajouter Override aux methodes qui viennent de la classe mere