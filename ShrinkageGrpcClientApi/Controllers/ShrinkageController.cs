using DataAccess.CRUD.Mapper;
using DataAccess.CRUD.ModeleDto;
using DataAccess.CRUD.Modeles;
using DataAccess.CRUD.ModelesRequests;
using Grpc.Core;
using GrpcShrinkageServiceTraining.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ShrinkageGrpcClientApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShrinkageController : ControllerBase
    {
        private readonly ShrinkageProtoService.ShrinkageProtoServiceClient _grpcClient;
        private readonly ILogger<ShrinkageController> _logger;

        public ShrinkageController(ILogger<ShrinkageController> logger, ShrinkageProtoService.ShrinkageProtoServiceClient grpcClient)
        {
            _grpcClient = grpcClient;
            _logger = logger;
        }


        // http://localhost:5000/api/shrinkage/emailAddress?emailAddress=diraneserges@gmail.com&correlationId=8d3f2a61-3c9a-4b8e-b9e1-6c8c9cda1111

        [HttpGet("emailAddress")]
        public async Task<UserDto> GetByEmail([FromQuery] string emailAddress, [FromQuery] Guid correlationId, CancellationToken cancellationToken)
        {


            using var _ = _logger.BeginScope(new Dictionary<string, object>
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
                _logger.LogError(ex, "Failed to get user");
                throw new BadHttpRequestException($"Failed to get user", StatusCodes.Status400BadRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user");
                throw new BadHttpRequestException("Failed to get user", StatusCodes.Status500InternalServerError, ex);
            }
        }


        // Get Teams
        //http://localhost:5000/api/shrinkage/teams?correlationId=b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66
        [HttpGet("teams")]
        public async Task<IReadOnlyList<TeamDto>> GetTeams([FromQuery] Guid correlationId, CancellationToken cancellationToken)
        {
            using var __ = _logger.BeginScope(new Dictionary<string, object>
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
                    _logger.LogError("No teams found");
                    throw new ArgumentNullException(nameof(teamsResponse));
                }

                var result = GrpcMapper.MapToTeamDtoList(teamsResponse);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get teams");
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
            using var __ = _logger.BeginScope(new Dictionary<string, object>
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
                _logger.LogError(ex, "Failed to save activity");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
            {
                _logger.LogError(ex, "Failed to save activity");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save activity");
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
            using var __ = _logger.BeginScope(new Dictionary<string, object>
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
                _logger.LogError(ex, "Failed to get user daily summary");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status400BadRequest);
            }
            catch (RpcException ex) when (ex.StatusCode == global::Grpc.Core.StatusCode.NotFound)
            {
                _logger.LogError(ex, "Failed to get user daily summary");
                throw new BadHttpRequestException(ex.Message, StatusCodes.Status404NotFound);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user daily summary");
                throw new BadHttpRequestException("Failed to get user daily summary", StatusCodes.Status500InternalServerError, ex);
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