using BlazorLayout.Exceptions;
using BlazorLayout.Extensions;
using BlazorLayout.Modeles;
using BlazorLayout.StateManagement;
using BlazorLayout.Stores;
using System.Net;
using System.Net.Http;

namespace BlazorLayout.Gateways
{
    public class ShrinkageApi (IHttpClientFactory httpClientFactory, TeamsStore teamsStore,
                               UserByEmailStore userByEmailStore,
                               ILogger<ShrinkageApi> logger)
    {
        private HttpClient HttpClient => httpClientFactory.CreateClient(HttpClients.ApiGateway);
        private readonly Dictionary<string, IdempotentApiRequest> ensureUserByEmail = new();

        // Teams Dictionary
        private readonly Dictionary<string, IdempotentApiRequest> ensureGetTeams = new();
        public ValueTask EnsureGetUserByEmail(string userMail, bool forceRefresh, CancellationToken cancellationToken)
        {
            var request = ensureUserByEmail.GetOrAdd(userMail, () => new IdempotentApiRequest(async token =>
            {
                var correlationId = Guid.NewGuid();
                using var __ = logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["Email"] = userMail,
                });
                try
                {
                    var parameters = new Dictionary<string, string?>
                    {
                        ["correlationId"] = correlationId.ToString(),
                        ["emailAddress"] = userMail,
                    };
                     var url = QueryHelpers.AddQueryString("api/shrinkage/emailAddress", parameters);
                   // var url = $"api/shrinkage/emailAddress" + $"?emailAddress={Uri.EscapeDataString(userMail)}" + $"&correlationId={correlationId}";
                    var user = await HttpClient.GetFromJsonAsyncNotNull<UserDto>(url, token);

                   userByEmailStore.InitializeUser(user);

                    


                }
                catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.BadRequest })
                {
                    logger.LogError(ex, "Failed to get user by email.");
                    throw new BadRequestException(ex, correlationId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to get user by email.");

                    throw new GetUserByEmailException(ex, correlationId);
                }
            }));

            if (forceRefresh)
            {
                request.Reset();
                userByEmailStore.Reset();
            }

            return request.Run(cancellationToken);
        }

        // Ensure Teams
        public ValueTask EnsureGetTeams(string userMail, bool forceRefresh, CancellationToken cancellationToken)
        {
            var request = ensureGetTeams.GetOrAdd(userMail, () => new IdempotentApiRequest(async token =>
            {
                var correlationId = Guid.NewGuid();
                using var __ = logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["UserMail"] = userMail,
                });
                try
                {
                    var parameters = new Dictionary<string, string?>
                    {
                        ["correlationId"] = correlationId.ToString(),
                    };
                   var url = QueryHelpers.AddQueryString("api/shrinkage/teams", parameters);

                    //var url = $"api/shrinkage/teams?correlationId={correlationId}";

                    var result = await HttpClient.GetFromJsonAsyncNotNull<IReadOnlyList<TeamDto>>(url, token);

                    teamsStore.InitializeTeams(result);
                }
                catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.NotFound })
                {
                    logger.LogError(ex, "Failed to get teams.");
                    throw new NotFoundException(ex, correlationId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to get teams.");
                    throw new GetTeamsException(ex, correlationId);
                }
            }));
            if (forceRefresh)
            {
                request.Reset();
                teamsStore.Reset();
            }

            return request.Run(cancellationToken);
        }

    }
}
