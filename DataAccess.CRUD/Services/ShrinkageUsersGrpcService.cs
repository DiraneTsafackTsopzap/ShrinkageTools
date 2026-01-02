
using System.Diagnostics;
using System.Text;
using System.Transactions;
using DataAccess.CRUD.Extensions;
using DataAccess.CRUD.Modeles;
using DataAccess.CRUD.Repositories;
using DataAccess.CRUD.Repositories.AbsencesRepository;
using DataAccess.CRUD.Repositories.TeamsRepository;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcShrinkageServiceTraining.Protobuf;
using Activity = GrpcShrinkageServiceTraining.Protobuf.Activity;
using Status = GrpcShrinkageServiceTraining.Protobuf.Status;


namespace DataAccess.CRUD.Services
{
    public class ShrinkageUsersGrpcService : ShrinkageProtoService.ShrinkageProtoServiceBase
    {
        private readonly IShrinkageUserRepository _shrinkageUsersRepository;
        private readonly IShrinkageTeamsRepository _shrinkageTeamsRepository;
        private readonly IShrinkageAbsenceRepository _shrinkageAbsenceRepository;
        public ShrinkageUsersGrpcService(IShrinkageTeamsRepository shrinkageTeamsRepository, 
                                         IShrinkageUserRepository shrinkageUsersRepository ,
                                         IShrinkageAbsenceRepository shrinkageAbsenceRepository)
        {
            _shrinkageUsersRepository = shrinkageUsersRepository;
            _shrinkageTeamsRepository = shrinkageTeamsRepository;
            _shrinkageAbsenceRepository = shrinkageAbsenceRepository;
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


        public override async Task<GetUserShrinkageResponse> GetUserShrinkage(GetUserShrinkageRequest request, ServerCallContext context)
        {
            if (!request.UserId.TryParseToGuidNotNullOrEmpty(out var userId))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation id {request.CorrelationId} invalid UserId: {userId}");
            }

            if (request.ShrinkageDate is null)
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation id {request.CorrelationId} invalid shrinkage date: {request.ShrinkageDate}");
            }

            var user = await _shrinkageUsersRepository.GetUserById(userId, context.CancellationToken);
            if (user is null)
            {
                throw RpcExceptions.NotFound($"Error with correlation id {request.CorrelationId} user with Id {request.UserId} not found");
            }

            var shrinkageDate = request.ShrinkageDate.FromTimeStampToDate();
            var dailyUserValues = await _shrinkageUsersRepository.GetActiveUserDailyValuesByUserIdAndDate(userId, shrinkageDate, context.CancellationToken);

            var activities = await _shrinkageUsersRepository.GetActivitiesByUserId(userId, shrinkageDate, context.CancellationToken);

            var response = new GetUserShrinkageResponse
            {
                Shrinkage = new Shrinkage
                {
                    UserDailyValues = new UserDailyValues
                    {
                        Id = dailyUserValues.Id,
                        UserId = userId,
                        TeamId = dailyUserValues.TeamId,
                        PaidTime = Duration.FromTimeSpan(TimeSpan.FromMinutes(dailyUserValues.PaidTime)),
                        PaidTimeOff = Duration.FromTimeSpan(TimeSpan.FromMinutes(dailyUserValues.PaidTimeOff)),
                        Overtime = Duration.FromTimeSpan(TimeSpan.FromMinutes(dailyUserValues.Overtime)),
                        VacationTime = Duration.FromTimeSpan(TimeSpan.FromMinutes(dailyUserValues.VacationTime)),
                        Status = dailyUserValues.Status.ConvertToApiStatus(),
                        Comment = dailyUserValues.Comment ?? string.Empty,
                        CreatedOn = NormalizeUtc(dailyUserValues.CreatedAt).ToTimestamp(),
                        CreatedBy = dailyUserValues.CreatedByUserEmail,
                        ShrinkageDate = request.ShrinkageDate,
                    },
                    Activities =
                {
                    activities
                        .Select(a => new Activity
                        {
                            Id = a.Id,
                            TeamId = a.TeamId,
                            DateTimeRange = new DateTimeRange { StartedAt = a.StartedAt.ToTimestamp(), StoppedAt = a.StoppedAt?.ToTimestamp() },
                            ActivityTrackType = a.ActivityTrackType.ConvertToApiActivityTrackType(),
                            ActivityType = a.ActivityType.ConvertToApiActivityType(),
                        }).ToList()
                }
                }
            };
            if (dailyUserValues.Status == ShrinkageConstants.Pending || dailyUserValues.Status == ShrinkageConstants.Transferred)
            {
                return response;
            }

            response.Shrinkage.UserDailyValues.UpdatedBy = dailyUserValues.UpdatedByUserEmail ?? string.Empty;
            response.Shrinkage.UserDailyValues.UpdatedOn = dailyUserValues.UpdatedAt == null ? null : NormalizeUtc(dailyUserValues.UpdatedAt.Value).ToTimestamp();

            return response;
        }

        //Delete Activity By Id
        public override async Task<DeleteActivityByIdResponse> DeleteActivityById(DeleteActivityByIdRequest request, ServerCallContext context)
        {
            if (!request.Id.TryParseToGuidNotNullOrEmpty(out var id))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} activity id is required");
            }

            if (!request.DeletedBy.TryParseToGuidNotNullOrEmpty(out var deletedBy))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} deleted by is required");
            }

            var activityDeleted = await _shrinkageUsersRepository.DeleteById(id, deletedBy, context.CancellationToken);

            if (!activityDeleted)
                throw RpcExceptions.NotFound($"Error with correlation Id {request.CorrelationId} activity with id: {request.Id} not found");

            return new DeleteActivityByIdResponse();
        }


        // Save Absence
        public override async Task<SaveAbsenceResponse> SaveAbsence(SaveAbsenceRequest request, ServerCallContext context)
        {
            if (request.Absence is null || !request.Absence.Id.TryParseToGuidNotNullOrEmpty(out var absenceId))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} absence and absence id are required");
            }

            if (request.Absence.AbsenceType == AbsenceType.Unspecified)
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} absence type is not specified");
            }

            if (!request.Absence.UserId.TryParseToGuidNotNullOrEmpty(out var userId))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} userId is required");
            }

            if (!request.Absence.TeamId.TryParseToGuidNotNullOrEmpty(out var teamId))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} teamId is required");
            }

            if (request.Absence.StartDate is null || request.Absence.EndDate is null)
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} absence started at and stopped at are required");
            }

            if (request.Absence.StartDate > request.Absence.EndDate)
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} absence started at cannot be greater than stopped at");
            }

            var cancellationToken = context.CancellationToken;
            var absence = request.Absence;

            var startedAt = absence.StartDate.FromTimeStampToDate();
            var stoppedAt = absence.EndDate.FromTimeStampToDate();
            var absenceDataModel = new ShrinkageAbsenceDataModel
            {
                Id = absenceId,
                UserId = userId,
                TeamId = teamId,
                AbsenceType = absence.AbsenceType.ConvertFromApiAbsenceType(),
                StartDate = startedAt,
                EndDate = stoppedAt,
            };
            var publicHolidays = await _shrinkageUsersRepository.GetTeamsPublicHolidaysByTeamId(teamId, cancellationToken);
            var publicHolidayDates = publicHolidays.Select(x => x.AffectedDay).ToList();

            var existingAbsence = await _shrinkageAbsenceRepository.GetAbsenceById(absenceId, cancellationToken);
            var absenceExistForDateRange = await _shrinkageAbsenceRepository.GetAllAbsenceWithinDateRange(userId, startedAt, stoppedAt, cancellationToken);

            if (existingAbsence == null)
            {
                if (absenceExistForDateRange.Count != 0)
                {
                    throw RpcExceptions.AlreadyExists($"Error with correlation Id {request.CorrelationId} absence with requested date range {startedAt} - {stoppedAt} already exist for user {userId}");
                }

                var createdBy = absence.CreatedBy;
                var createdByUserId = await _shrinkageUsersRepository.GetUserIdByEmail(createdBy, cancellationToken);
                if (createdByUserId == Guid.Empty)
                    throw RpcExceptions.NotFound($"Error with correlation id {request.CorrelationId} user with email {createdBy} does not exist");
                absenceDataModel.CreatedBy = createdByUserId;
                absenceDataModel.CreatedAt = DateTime.UtcNow;

                using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromSeconds(100) }, TransactionScopeAsyncFlowOption.Enabled);

                await _shrinkageAbsenceRepository.CreateAbsence(absenceDataModel, cancellationToken);
                for (var date = startedAt; date <= stoppedAt; date = date.AddDays(1))
                {
                    var existingUserDailyValue = await _shrinkageUsersRepository.GetActiveUserDailyValuesByUserIdAndDate(userId, date, cancellationToken);
                    if (existingUserDailyValue != null)
                    {
                        await _shrinkageUsersRepository.DeleteById(new ShrinkageUserDailyValuesDataModel { Id = existingUserDailyValue.Id, DeletedAt = DateTime.UtcNow, DeletedBy = createdByUserId, }, context.CancellationToken);
                    }
                }

                scope.Complete();
            }
            else
            {
                if (absenceExistForDateRange.Any(x => x.Id != absenceId))
                {
                    throw RpcExceptions.AlreadyExists($"Error with correlation Id {request.CorrelationId} absence with requested date range {startedAt} - {stoppedAt} already exist for user {userId}");
                }

                if (string.IsNullOrWhiteSpace(absence.UpdatedBy))
                    throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} while SaveAbsence for UserId {absence.UserId}, updated by is required");

                var updatedBy = absence.UpdatedBy;
                var updatedByUserId = await _shrinkageUsersRepository.GetUserIdByEmail(updatedBy, cancellationToken);
                if (updatedByUserId == Guid.Empty)
                    throw RpcExceptions.NotFound($"Error with correlation id {request.CorrelationId} user with email {updatedBy} does not exist");
                absenceDataModel.UpdatedBy = updatedByUserId;
                absenceDataModel.UpdatedAt = DateTime.UtcNow;

                using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromSeconds(100) }, TransactionScopeAsyncFlowOption.Enabled);

                await _shrinkageAbsenceRepository.UpdateAbsence(absenceDataModel, cancellationToken);

                //Updating daily values for the existing absence
                var userDailyValuesListForNewAbsenceDates = await _shrinkageUsersRepository.GetUserDailyValuesByUserIdAndDateRange(userId, startedAt, stoppedAt, cancellationToken);
                var userDailyValuesListForOldAbsenceDates = await _shrinkageUsersRepository.GetUserDailyValuesByUserIdAndDateRange(userId, existingAbsence.StartDate, existingAbsence.EndDate, cancellationToken);

                Dictionary<DateOnly, ShrinkageUserDailyValuesDataModel> userDailyValuesForNewAbsenceDays = userDailyValuesListForNewAbsenceDates.ToDictionary(x => x.ShrinkageDate, x => x);
                Dictionary<DateOnly, ShrinkageUserDailyValuesDataModel> userDailyValuesForOldAbsenceDays = userDailyValuesListForOldAbsenceDates.ToDictionary(x => x.ShrinkageDate, x => x);

                //Dates in old but not in new — reopen
                var datesToBeReopened = userDailyValuesForOldAbsenceDays.Keys.Except(userDailyValuesForNewAbsenceDays.Keys);
                foreach (var date in datesToBeReopened)
                {
                    if (publicHolidayDates.Contains(date) || date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                        continue;

                    var dailyValueToReopen = userDailyValuesForOldAbsenceDays[date];
                    await _shrinkageUsersRepository.ReActivateUserDailyValueById(dailyValueToReopen.Id, cancellationToken);
                }

                //Dates in new but not in old — delete
                var datesToDelete = userDailyValuesForNewAbsenceDays.Keys.Except(userDailyValuesForOldAbsenceDays.Keys);
                foreach (var date in datesToDelete)
                {
                    var valueToDelete = userDailyValuesForNewAbsenceDays[date];
                    await _shrinkageUsersRepository.DeleteById(new ShrinkageUserDailyValuesDataModel { Id = valueToDelete.Id, DeletedAt = DateTime.UtcNow, DeletedBy = updatedByUserId }, cancellationToken);
                }

                scope.Complete();
            }

            return new SaveAbsenceResponse();
        }

        // Get Absences By User Ids
        public override async Task<GetAbsencesByUserIdsResponse> GetAbsencesByUserIds(GetAbsencesByUserIdsRequest request, ServerCallContext context)
        {
            if (!request.UserIds.Any())
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation id {request.CorrelationId} user ids are required");
            }

            var dbResponse = await _shrinkageAbsenceRepository.GetAbsenceByUserIds(request.UserIds.Select(x => x.ToGuid()).ToList(), context.CancellationToken);
            var response = new GetAbsencesByUserIdsResponse
            {
                Absences =
            {
                dbResponse.Select(x => new Absence
                {
                    Id = x.Id,
                    UserId = x.UserId,
                    TeamId = x.TeamId,
                    StartDate = NormalizeUtc(x.StartDate.ToDateTime(TimeOnly.MinValue)).ToTimestamp(),
                    EndDate = NormalizeUtc(x.EndDate.ToDateTime(TimeOnly.MinValue)).ToTimestamp(),
                    CreatedAt = NormalizeUtc(x.CreatedAt).ToTimestamp(),
                    CreatedBy = x.CreatedByUserEmail ?? string.Empty,
                    UpdatedAt = x.UpdatedAt == null ? null : NormalizeUtc(x.UpdatedAt.Value).ToTimestamp(),
                    UpdatedBy = x.UpdatedByUserEmail ?? string.Empty,
                    AbsenceType = x.AbsenceType.ConvertToApiAbsenceType(),
                })
            }
            };
            return response;
        }

        // Delete Absences By Id
        public override async Task<DeleteAbsenceByIdResponse> DeleteAbsenceById(DeleteAbsenceByIdRequest request, ServerCallContext context)
        {
            if (!request.Id.TryParseToGuidNotNullOrEmpty(out var id))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} absence id is required");
            }

            if (!request.DeletedBy.TryParseToGuidNotNullOrEmpty(out var deletedBy))
            {
                throw RpcExceptions.InvalidArgument($"Error with correlation Id {request.CorrelationId} deleted by is required");
            }

            var cancellationToken = context.CancellationToken;
            var deletedAbsence = await _shrinkageAbsenceRepository.GetAbsenceById(id, cancellationToken);
            if (deletedAbsence == null)
            {
                throw RpcExceptions.NotFound($"Error with correlation Id {request.CorrelationId} absence with id: {id} not found");
            }

            var user = await _shrinkageUsersRepository.GetUserById(deletedAbsence.UserId, cancellationToken);

            if (user == null)
            {
                throw RpcExceptions.NotFound($"Error with correlation Id {request.CorrelationId} absence for user with id : {deletedAbsence.UserId} not found");
            }

            var publicHolidays = await _shrinkageUsersRepository.GetTeamsPublicHolidaysByTeamId(user.TeamId.Value, cancellationToken);
            var publicHolidayDates = new List<DateOnly>();
            if (publicHolidays.Count > 0)
                publicHolidayDates = publicHolidays.Select(x => x.AffectedDay).ToList();

            using var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted, Timeout = TimeSpan.FromSeconds(100) }, TransactionScopeAsyncFlowOption.Enabled);
            var absenceDeleted = await _shrinkageAbsenceRepository.DeleteAbsenceById(id, deletedBy, context.CancellationToken);

            for (var date = deletedAbsence!.StartDate; date <= deletedAbsence.EndDate; date = date.AddDays(+1))
            {
                if (publicHolidayDates.Count != 0 && publicHolidayDates.Contains(date))
                {
                    continue;
                }

                if (date > DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    continue;
                }

                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                {
                    continue;
                }

                var paidTime = await _shrinkageUsersRepository.GetPaidTimeByUserIdAndDate(deletedAbsence.UserId, date, cancellationToken);

                await _shrinkageUsersRepository.Create(new ShrinkageUserDailyValuesDataModel
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    TeamId = user.TeamId.Value,
                    PaidTime = paidTime,
                    Status = ShrinkageConstants.Pending,
                    ShrinkageDate = date,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = deletedBy,
                }, context.CancellationToken);
            }

            scope.Complete();

            if (!absenceDeleted)
                throw RpcExceptions.NotFound($"Error with correlation Id {request.CorrelationId} absence with id: {request.Id} not found");

            return new DeleteAbsenceByIdResponse();
        }
    }
}





// Tjrs ajouter Override aux methodes qui viennent de la classe mere