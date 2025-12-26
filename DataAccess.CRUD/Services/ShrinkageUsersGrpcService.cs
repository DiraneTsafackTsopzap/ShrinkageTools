using System;
using System.Runtime.InteropServices;
using DataAccess.CRUD.Extensions;
using DataAccess.CRUD.Modeles;
using DataAccess.CRUD.Repositories;
using DataAccess.CRUD.Repositories.TeamsRepository;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcShrinkageServiceTraining.Protobuf;


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
        public async Task<SaveActivityResponse> SaveActivity(SaveActivityRequest request, ServerCallContext context)
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
                StartedAt = userActivity.DateTimeRange.StartedAt!.ToDateTime().ConvertUtcToGermanDateTime(),
                StoppedAt = userActivity.DateTimeRange?.StoppedAt?.ToDateTime().ConvertUtcToGermanDateTime(),
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




    }
}
   




// Tjrs ajouter Override aux methodes qui viennent de la classe mere