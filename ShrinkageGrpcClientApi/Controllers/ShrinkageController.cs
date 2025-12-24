using DataAccess.CRUD.Mapper;
using DataAccess.CRUD.ModeleDto;
using DataAccess.CRUD.Modeles;
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

        // GET http://localhost:5000/api/shrinkage/emailAddress?email=sergesdirane@gmail.com
        // http://localhost:5000/api/shrinkage/emailAddress?emailAddress=sergesdirane@gmail.com&correlationId=8d3f2a61-3c9a-4b8e-b9e1-6c8c9cda1111

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
                         new GetUserByEmailRequest { 
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
        public async Task<IReadOnlyList<TeamDto>> GetTeams([FromQuery] Guid correlationId,CancellationToken cancellationToken)
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

    }
}

// POST /api/shrinkage/etudiant
// POST http://localhost:5000/api/shrinkage/etudiant

//{
//  "email": "diraneserges@gmail.com",
//  "nom": "Serges Kenfack",
//  "prenom": "Max",
//  "telephone": "+491638070334"
//}

