using DataAccess.CRUD.Extensions;
using DataAccess.CRUD.ModeleDto;
using DataAccess.CRUD.Modeles;
using Google.Protobuf.WellKnownTypes;
using GrpcShrinkageServiceTraining.Protobuf;
using PaidTime = DataAccess.CRUD.Modeles.PaidTime;

namespace DataAccess.CRUD.Mapper
{
    public class GrpcMapper
    {
        public static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId.ToGuid(),
                TeamId = user.TeamId == null || user.TeamId.ToGuid() == Guid.Empty ? null : user.TeamId.ToGuid(),
                Email = user.Email,
                PaidTimeList = user.PaidTime.Select(x => new PaidTime
                {
                    Id = x.Id.ToGuid(),
                    PaidTimeMonday = x.PaidTimeMonday.ToTimeSpan(),
                    PaidTimeTuesday = x.PaidTimeTuesday.ToTimeSpan(),
                    PaidTimeWednesday = x.PaidTimeWednesday.ToTimeSpan(),
                    PaidTimeThursday = x.PaidTimeThursday.ToTimeSpan(),
                    PaidTimeFriday = x.PaidTimeFriday.ToTimeSpan(),
                    PaidTimeSaturday = x.PaidTimeSaturday.ToTimeSpan(),
                    ValidFrom = DateOnly.FromDateTime(x.ValidFrom.ToDateTime()),
                    CreatedAt = x.CreatedAt.ToDateTime(),
                    CreatedBy = x.CreatedBy,
                }).ToList(),
            };
        }


        public static IReadOnlyList<TeamDto> MapToTeamDtoList(GetTeamsResponse response)
        {
            return response.Teams.Select(team => new TeamDto
            {
                Id = team.Id.ToGuid(),
                Name = team.Name,
                TeamLeadIds = team.TeamLeadIds.Select(id => id.ToGuid()).ToList(),
            }).ToList();
        }

        public static SaveActivityRequest MapToSaveActivityRequest(SaveActivityDto activity)
        {
            var apiRequest = new SaveActivityRequest
            {
                CorrelationId = activity.CorrelationId,
                UserId = activity.Activity.UserId,
                Activity = new Activity
                {
                    Id = activity.Activity.Id,
                    TeamId = activity.Activity.TeamId,
                    ActivityType = activity.Activity.ActivityType.ToGrpcActivityType(),
                    ActivityTrackType = activity.Activity.ActivityTrackType.ToGrpcActivityTrackType(),
                    DateTimeRange = new DateTimeRange
                    {
                        StartedAt = activity.Activity.StartedAt.ToTimestamp(),
                        StoppedAt = activity.Activity.StoppedAt?.ToTimestamp(),
                    },
                    CreatedBy = activity.Activity.CreatedBy,
                    UpdatedBy = activity.Activity.UpdatedBy ?? string.Empty,
                },
            };
            return apiRequest;
        }

    }
}


// https://dev.azure.com/lowell-dach/api/_git/api-cloud?path=%2Fprotos%2Flowell%2Fworkforce%2Fgrpc%2Fv1%2Fshrinkage_service.proto&_a=contents&version=GBmaster

// 1- remarque :  le ToGuid
// Achten au ToGuid ici , Lowell est : Guid.Protobuf.Uuid.ToGuid() , le mien est AppUuid.ToGuid()
