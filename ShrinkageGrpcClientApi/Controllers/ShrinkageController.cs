using DataAccess.CRUD.Mapper;
using DataAccess.CRUD.ModeleDto;
using DataAccess.CRUD.Modeles;
using DataAccess.CRUD.ModelesRequests;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcShrinkageServiceTraining.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace ShrinkageGrpcClientApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShrinkageController : ControllerBase
    {
        private readonly ShrinkageProtoService.ShrinkageProtoServiceClient _grpcClient;
        private readonly ILogger<ShrinkageController> logger;

        public ShrinkageController(ILogger<ShrinkageController> logger, ShrinkageProtoService.ShrinkageProtoServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
            this.logger = logger;
        }


        // http://localhost:5000/api/shrinkage/emailAddress?emailAddress=diraneserges@gmail.com&correlationId=8d3f2a61-3c9a-4b8e-b9e1-6c8c9cda1111

        [HttpGet("emailAddress")]
        public async Task<UserDto> GetByEmail([FromQuery] string emailAddress, [FromQuery] Guid correlationId, CancellationToken cancellationToken)
        {


            using var _ = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["Email"] = emailAddress
            });

            try
            {
                var response = await _grpcClient.GetUserByEmailAsync

                  (
                       new GetUserByEmailRequest
                       {
                           CorrelationId = correlationId,
                           Email = emailAddress
                       }, cancellationToken: cancellationToken
                  );



                var mappedUser = GrpcMapper.MapToUserDto(response.User);

                return mappedUser;
            }

            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.InvalidArgument)
            {
                logger.LogError(ex, "Failed to get user");
                throw new BadHttpRequestException($"Failed to get user", StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get user");
                throw new BadHttpRequestException("Failed to get user", StatusCodes.Status500InternalServerError, ex);
            }
        }


        // Get Teams
        //http://localhost:5000/api/shrinkage/teams?correlationId=b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66
        [HttpGet("teams")]
        public async Task<IReadOnlyList<TeamDto>> GetTeams([FromQuery] Guid correlationId, CancellationToken cancellationToken)
        {
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
            });

            try
            {
                var request = new GetTeamsRequest
                {
                    CorrelationId = correlationId
                };

                // ✅ ASYNC gRPC CALL
                var teamsResponse = await _grpcClient.GetTeamsAsync(
                    request,
                    cancellationToken: cancellationToken);

                if (teamsResponse == null)
                {
                    logger.LogError("No teams found");
                    throw new ArgumentNullException(nameof(teamsResponse));
                }

                var result = GrpcMapper.MapToTeamDtoList(teamsResponse);

                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get teams");
                throw new BadHttpRequestException(
                    "Failed to get teams",
                    StatusCodes.Status500InternalServerError,
                    ex);
            }
        }


        /// <summary>
        /// Enregistre une nouvelle activité utilisateur dans le système shrinkage.
        /// 
        /// 🔒 Nécessite :
        /// - Un `userId` valide existant dans la base.
        /// - Un `shrinkage_user_daily_values` existant pour la date de `startedAt`.
        /// - `startedAt` et `stoppedAt` doivent être en **format UTC**.
        /// 
        /// 📝 Exemple de payload JSON :
        ///
        /// {
        ///   "correlationId": "c111d111-3b1a-4a1b-9d1a-1b1b1b1b1b1b",
        ///   "activity": {
        ///     "id": "a1234567-89ab-4cde-f012-3456789abcde",
        ///     "userId": "b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66",
        ///     "teamId": "c1f2b9d4-0c64-4c89-9d7b-8e91fcb6e7b2",
        ///     "activityType": 2,
        ///     "activityTrackType": 2,
        ///     "startedAt": "2025-12-26T08:00:00Z",
        ///     "stoppedAt": "2025-12-26T09:30:00Z",
        ///     "createdBy": "diraneserges@gmail.com",
        ///     "updatedBy": "diraneserges@gmail.com"
        ///   }
        /// }
        /// </summary>

        // Save Activity
        //http://localhost:5000/api/shrinkage/save-user-activity
        [HttpPost("save-user-activity")]
        public async Task SaveActivity([FromBody] SaveActivityRequest_M input, CancellationToken cancellationToken)
        {
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["@Request"] = input,
            });
            try
            {
                var grpcRequest = GrpcMapper.MapToSaveActivityRequest(input);
                await _grpcClient.SaveActivityAsync(grpcRequest, cancellationToken: cancellationToken);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.InvalidArgument)
            {
                logger.LogError(ex, "Failed to save activity");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
            {
                logger.LogError(ex, "Failed to save activity");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save activity");
                throw new BadHttpRequestException("Failed to save activity", StatusCodes.Status500InternalServerError, ex);
            }
        }



        // Get Users Daily Summary
        // http://localhost:5000/api/shrinkage/get-user-daily-summary : POST
        //{
        //  "correlationId": "b9f2190f-d38c-426e-bbaa-512a5ff74f8e",
        //  "userId": "b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66"
        //}

        // S'assurer ici que le userId existe bien dans la base, sinon 404 NotFound
        [HttpPost("get-user-daily-summary")]
        public async Task<IReadOnlyList<UserDailySummaryDto>> GetUserDailySummary([FromBody] GetUserDailySummaryRequest_M input, CancellationToken cancellationToken)
        {
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = input.CorrelationId,
                ["UserId"] = input.UserId,
            });
            try
            {
                var grpcRequest = new GetUserDailySummaryRequest
                {
                    CorrelationId = input.CorrelationId,
                    UserId = input.UserId,
                };

                var grpcResponse = await _grpcClient
                    .GetUserDailySummaryAsync(grpcRequest, cancellationToken: cancellationToken);

                var vm = GrpcMapper.MapFromGrpcToViewModel(grpcResponse);

                return vm;
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.InvalidArgument)
            {
                logger.LogError(ex, "Failed to get user daily summary");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
            {
                logger.LogError(ex, "Failed to get user daily summary");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get user daily summary");
                throw new BadHttpRequestException("Failed to get user daily summary", StatusCodes.Status500InternalServerError, ex);
            }
        }

        // Get user Shrinkage
        // http://localhost:5000/api/shrinkage/get-user-shrinkage : POST
        // {
        //  "correlationId": "8a6f73c2-6f3f-4c92-8c41-4c1b682a1be3",
        //  "userId": "b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66",
        //  "shrinkageDate": "2025-12-29"
        // }


        [HttpPost("get-user-shrinkage")]
        public async Task<UserShrinkageDto> GetUserShrinkage(GetUserShrinkageRequest_M input, CancellationToken cancellationToken)
        {
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = input.CorrelationId,
                ["UserId"] = input.UserId,
                ["ShrinkageDate"] = input.ShrinkageDate,
            });
            try
            {
                var grpcrequest = new GetUserShrinkageRequest
                {
                    CorrelationId =  input.CorrelationId ,
                    UserId = input.UserId,
                    ShrinkageDate = input.ShrinkageDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc).ToTimestamp()
                };

                var grpcResponse = await _grpcClient.GetUserShrinkageAsync(grpcrequest, cancellationToken: cancellationToken);

                var dto = GrpcMapper.MapFromGrpc(grpcResponse);

                return dto;
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.InvalidArgument)
            {
                logger.LogError(ex, "Failed to get user shrinkage");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
            {
                logger.LogError(ex, "Failed to get user shrinkage");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get user shrinkage");
                throw new BadHttpRequestException("Failed to get user shrinkage", StatusCodes.Status500InternalServerError, ex);
            }
        }


        // Delete Activity
        // http://localhost:5000/api/shrinkage/activities : DELETE
        //
        //        {
        //  "correlationId": "b9f2190f-d38c-426e-bbaa-512a5ff74f8e",
        //  "activityId": "d6a9fc9f-3b77-4a92-aacc-9e3a2a6cfa7f",  // Activity id doit exister ds la Base de donnes
        //  "deletedBy": "b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66"
        //}

        [HttpDelete("activities")]
        public async Task DeleteActivityById([FromBody] DeleteActivityRequest_M input, CancellationToken cancellationToken)
        {
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["@Request"] = input,
            });
            try
            {
                var request = new DeleteActivityByIdRequest
                {
                    CorrelationId = input.CorrelationId,
                    Id = input.ActivityId,
                    DeletedBy = input.DeletedBy,
                };

                await _grpcClient.DeleteActivityByIdAsync(request, cancellationToken: cancellationToken);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.InvalidArgument)
            {
                logger.LogError(ex, "Failed to delete activity by id");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
            {
                logger.LogError(ex, "Failed to delete activity by id");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete activity by id");
                throw new BadHttpRequestException("Failed to delete activity by id", StatusCodes.Status500InternalServerError, ex);
            }
        }


        // Save Absence
        // http://localhost:5000/api/shrinkage/absence : POST
        //        {
        //  "correlationId": "e4d1a00f-3f56-44cf-85b9-5f5f4148a301",
        //  "Absence": {
        //    "id": "cd50c6eb-1d5a-4f6b-9df4-b4e58e96d234",
        //    "userId": "b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66", UserId doit exister dans la base
        //    "userEmail": "diraneserges@gmail.com",            Email doit exister dans la base
        //    "teamId": "c1f2b9d4-0c64-4c89-9d7b-8e91fcb6e7b2",  TeamId doit exister dans la base
        //    "absenceType":1,                                , AbsenceType : 1 = Vacation , 2 = Sickness , 3 = Unspecified
        //    "startDate": "2026-01-03",
        //    "endDate": "2026-01-05",
        //    "createdAt": "2026-01-02T10:00:00Z",
        //    "createdBy": "diraneserges@gmail.com",
        //    "updatedAt": null,
        //    "updatedBy": null                                     // Lors du Save ,UpdatedBy est vide , Lors de Update ,updatedBy est rempli
        //  }
        //}
        [HttpPost("absence")]
        public async Task SaveAbsence([FromBody] SaveUserAbsenceRequest_M absence, CancellationToken cancellationToken)
        {
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["@Request"] = absence,
            });

            try
            {
                var request = GrpcMapper.MapToSaveAbsenceRequest(absence);
                await _grpcClient.SaveAbsenceAsync(request, cancellationToken: cancellationToken);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.InvalidArgument)
            {
                logger.LogError(ex, "Failed to save absence");
                throw new BadHttpRequestException("Failed to save absence", StatusCodes.Status400BadRequest);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
            {
                logger.LogError(ex, "Failed to save absence");
                throw new BadHttpRequestException("Failed to save absence", StatusCodes.Status404NotFound);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.AlreadyExists)
            {
                logger.LogError(ex, "Failed to save absence");
                throw new BadHttpRequestException("Failed to save absence", StatusCodes.Status409Conflict);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save absence");
                throw new BadHttpRequestException("Failed to save absence", StatusCodes.Status500InternalServerError, ex);
            }
        }


        // Get Absences By User Ids
        // POST http://localhost:5000/api/shrinkage/absences/by-user-ids
//        {
//  "correlationId": "f25332cd-dde2-4e90-a0db-d80a5ef1ea5c",
//  "userIds": [
//    "b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66", UserId Doit Exister Dans la Base
//   
//  ]
//}


        [HttpPost("absences/by-user-ids")]
        public async Task<IReadOnlyList<AbsenceDto>> GetAbsencesByUserIds([FromBody] GetAbsencesByUserIdsRequest_M input, CancellationToken cancellationToken)
        {
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = input.CorrelationId,
                ["UserIds"] = string.Join(", ", input.UserIds),
            });
            try
            {
                var request = new GetAbsencesByUserIdsRequest
                {
                    CorrelationId = input.CorrelationId,
                    UserIds = { input.UserIds.Select(AppUuid.FromGuid).ToList() },
                };

                var response = await _grpcClient.GetAbsencesByUserIdsAsync(request, cancellationToken: cancellationToken);

                var dtoList = GrpcMapper.MapToAbsencesDtoList(response);

                return dtoList;
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.InvalidArgument)
            {
                logger.LogError(ex, "Failed to get absence");
                throw new BadHttpRequestException("Failed to get absence", StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to get absence");
                throw new BadHttpRequestException("Failed to get absence", StatusCodes.Status500InternalServerError, ex);
            }
        }

    }
}


// Exemple de Post : Save Activity ici 
// Methode: POST : POST 

//startedAt et stoppedAt doivent impérativement 
//    être en format UTC → suffixe Z (ex : "2025-12-26T08:00:00Z")

//✅ S'assurer qu'une ligne correspondante existe déjà dans 
//    shrinkage_user_daily_values pour la date 2025-12-26 et 
//    le userId, sinon l’API retournera une erreur 404 NotFound.

//⚠️ Cette ligne de test doit être préparée dans ton fichier init.sql ou test.yaml