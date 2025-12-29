using DataAccess.CRUD.EnumsModels;
using DataAccess.CRUD.Extensions;
using DataAccess.CRUD.ModeleDto;
using DataAccess.CRUD.Modeles;
using DataAccess.CRUD.ModelesRequests;
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

        public static SaveActivityRequest MapToSaveActivityRequest(SaveActivityRequest_M activity)
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


        public static IReadOnlyList<UserDailySummaryDto> MapFromGrpcToViewModel(GetUserDailySummaryResponse summary)
        {
            var response = new List<UserDailySummaryDto>();

            foreach (var item in summary.UserDailySummaries)
            {
                var userDailySummary = new UserDailySummaryDto
                {
                    Id = item.Id.ToGuid(),
                    Date = DateOnly.FromDateTime(item.Date.ToDateTime()),
                };

                switch (item.AttendanceTypeCase)
                {
                    case GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.WorkingDay:
                        userDailySummary = userDailySummary with
                        {
                            Status = ShrinkageExtensionsHelper.FromGrpcStatus(item.WorkingDay.Status),
                        };
                        break;

                    case GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.Absence:
                        if (item.Absence.Type != AbsenceType.Unspecified)
                        {
                            userDailySummary = userDailySummary with
                            {
                                AbsenceType = item.Absence.Type.FromGrpcAbsenceType(),
                            };
                        }

                        break;
                    case GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.PublicHoliday:
                        userDailySummary = userDailySummary with
                        {
                            Status = ShrinkageExtensionsHelper.FromGrpcStatus(item.PublicHoliday.Status),
                            PublicHoliday = new PublicHolidayDto(),
                        };
                        break;
                    case GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.Weekend:
                        userDailySummary = userDailySummary with
                        {
                            Status = ShrinkageExtensionsHelper.FromGrpcStatus(item.Weekend.Status),
                            Weekend = new WeekendDto(),
                        };
                        break;
                    case GetUserDailySummaryResponse.Types.UserDailySummary.AttendanceTypeOneofCase.None:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(item.AttendanceTypeCase), item.AttendanceTypeCase, null);
                }

                response.Add(userDailySummary);
            }

            return response;
        }


        public static UserShrinkageDto MapFromGrpc(GetUserShrinkageResponse grpcResponse)
        {
            var udv = grpcResponse.Shrinkage.UserDailyValues;

            return new UserShrinkageDto
            {
                PaidTime = udv?.PaidTime?.ToTimeSpan() ?? TimeSpan.Zero,
                PaidTimeOff = udv?.PaidTimeOff?.ToTimeSpan() ?? TimeSpan.Zero,
                Overtime = udv?.Overtime?.ToTimeSpan() ?? TimeSpan.Zero,
                VacationTime = udv?.VacationTime?.ToTimeSpan() ?? TimeSpan.Zero,

                UserDailyValues = udv == null
                    ? new UserDailyValuesDto()
                    : new UserDailyValuesDto
                    {
                        Id = udv.Id.ToGuid(),
                        UserId = udv.UserId.ToGuid(),
                        TeamId = udv.TeamId.ToGuid(),
                        Status = ShrinkageExtensionsHelper.FromGrpcStatus(udv.Status),
                        Comment = udv.Comment.NullIfEmpty(),
                        CreatedAt = udv.CreatedOn.ToDateTime(),
                        CreatedBy = udv.CreatedBy,
                        UpdatedAt = udv.UpdatedOn?.ToDateTime(),
                        UpdatedBy = udv.UpdatedBy.NullIfEmpty(),
                        ShrinkageDate = DateOnly.FromDateTime(udv.ShrinkageDate.ToDateTime()),
                    },

                Activities = grpcResponse.Shrinkage.Activities.Select(a => new ActivityDto
                {
                    Id = a.Id.ToGuid(),
                    UserId = udv!.UserId.ToGuid(),
                    TeamId = a.TeamId.ToGuid(),
                    StartedAt = a.DateTimeRange?.StartedAt.ToDateTimeOffset() ?? throw new ArgumentNullException(nameof(a.DateTimeRange.StartedAt), "Activity started at cannot be null"),
                    StoppedAt = a.DateTimeRange?.StoppedAt?.ToDateTimeOffset(),
                    ActivityTrackType = ShrinkageExtensionsHelper.FromGrpcActivityTrackType(a.ActivityTrackType),
                    ActivityType = a.ActivityType.FromGrpcActivityType(),
                    CreatedBy = a.CreatedBy,
                    UpdatedBy = a.UpdatedBy.NullIfEmpty(),
                }).ToList(),
            };
        }


    }
}


// https://dev.azure.com/lowell-dach/api/_git/api-cloud?path=%2Fprotos%2Flowell%2Fworkforce%2Fgrpc%2Fv1%2Fshrinkage_service.proto&_a=contents&version=GBmaster

// 1- remarque :  le ToGuid
// Achten au ToGuid ici , Lowell est : Guid.Protobuf.Uuid.ToGuid() , le mien est AppUuid.ToGuid()
