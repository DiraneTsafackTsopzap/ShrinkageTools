using System;
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

                throw new RpcException(new Status(StatusCode.Internal, ex.Message));
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
        public override async Task<GetTeamsResponse> GetTeams( GetTeamsRequest request,ServerCallContext context)
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
                    new Status(StatusCode.Internal, ex.Message)
                );
            }
        }



    }
}




// Tjrs ajouter Override aux methodes qui viennent de la classe mere