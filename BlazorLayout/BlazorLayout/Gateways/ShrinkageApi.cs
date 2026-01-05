using BlazorLayout.Exceptions;
using BlazorLayout.Extensions;
using BlazorLayout.Modeles;
using BlazorLayout.ModelRequest;
using BlazorLayout.StateManagement;
using BlazorLayout.Stores;
using Google.Protobuf.WellKnownTypes;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;




namespace BlazorLayout.Gateways
{
    public class ShrinkageApi (IHttpClientFactory httpClientFactory, 
                               TeamsStore teamsStore, 
                               UserShrinkageStore userShrinkageStore,
                               UserByEmailStore userByEmailStore,
                                UserDailySummaryStore userDailySummaryStore,
                                UserAbsencesStore userAbsencesStore,
                               ILogger<ShrinkageApi> logger)
    {
        private HttpClient HttpClient => httpClientFactory.CreateClient(HttpClients.ApiGateway);
        private readonly Dictionary<string, IdempotentApiRequest> ensureUserByEmail = new();

        // Teams Dictionary
        private readonly Dictionary<string, IdempotentApiRequest> ensureGetTeams = new();

        // User Daily Summary Dictionary
        private readonly Dictionary<Guid, IdempotentApiRequest> ensureGetUserDailySummary = new();

        // User Shrinkage Dictionary
        private readonly Dictionary<Guid, Dictionary<DateOnly, IdempotentApiRequest>> ensureGetUserShrinkage = new();

        // User Absences Dictionary
        private readonly Dictionary<IReadOnlyList<Guid>, IdempotentApiRequest> ensureGetUserAbsences = new();
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

        // Ensure Get User Daily Summary
        public ValueTask EnsureGetUserDailySummary(Guid userId, bool forceRefresh, CancellationToken cancellationToken)
        {
            var request = ensureGetUserDailySummary.GetOrAdd(userId, () => new IdempotentApiRequest(async token =>
            {
                var correlationId = Guid.NewGuid();
                using var __ = logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["UserId"] = userId,
                });
                try
                {
                    var response = await HttpClient.PostAsJsonAsync("api/shrinkage/get-user-daily-summary",
                        new GetUserDailySummaryRequest_M
                        {
                            CorrelationId = correlationId,
                            UserId = userId,
                        }, token);
                    response.EnsureSuccessStatusCode();

                    var dto = await response.Content.ReadFromJsonAsyncNotNull<IReadOnlyList<UserDailySummaryDto>>(token);

                    // Initialise the get-user-daily-summary dans un Store
                    userDailySummaryStore.InitializeSummary(dto);
                }
                catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.BadRequest })
                {
                    logger.LogError(ex, "Failed to get user daily summary.");
                    throw new BadRequestException(ex, correlationId);
                }
                catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.NotFound })
                {
                    logger.LogError(ex, "Failed to get user daily summary.");
                    throw new NotFoundException(ex, correlationId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to get user daily summary.");
                    throw new GetUserDailySummaryException(ex, correlationId);
                }
            }));

            if (forceRefresh)
            {
                request.Reset();
                userDailySummaryStore.Reset();
            }

            return request.Run(cancellationToken);
        }


        // Ensure get User Shrinkage
        public ValueTask EnsureGetUserShrinkage(DateOnly shrinkageDate, Guid userId, bool forceRefresh, CancellationToken cancellationToken)
        {
            IdempotentApiRequest CreateRequest(DateOnly date, Guid uid) => new(async token =>
            {
                var correlationId = Guid.NewGuid();
                using var __ = logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["ShrinkageDate"] = date,
                    ["UserId"] = uid,
                });

                try
                {
                    var response = await HttpClient.PostAsJsonAsync("api/shrinkage/get-user-shrinkage",
                        new GetUserShrinkageRequest_M
                        {
                            CorrelationId = correlationId,
                            UserId = uid,
                            ShrinkageDate = date
                            ,
                        }, token);

                    response.EnsureSuccessStatusCode();
                    var shrinkage = await response.Content.ReadFromJsonAsyncNotNull<UserShrinkageDto>(token);
                    userShrinkageStore.InitializeShrinkage(uid, date, shrinkage);

                    if (shrinkage.UserDailyValues != null)
                    {
                        userDailySummaryStore.UpdateIdBasedOnDate(shrinkage.UserDailyValues.Id, date);
                    }
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.BadRequest)
                {
                    logger.LogError(ex, "Failed to get user shrinkage.");
                    throw new BadRequestException(ex, correlationId);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    logger.LogError(ex, "Failed to get user shrinkage.");
                    throw new NotFoundException(ex, correlationId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to get user shrinkage.");
                    throw new GetUsersShrinkageException(ex, correlationId);
                }
            });

            var perUser = ensureGetUserShrinkage.GetOrAdd(userId, () => new Dictionary<DateOnly, IdempotentApiRequest>());
            IdempotentApiRequest request;

            if (forceRefresh)
            {
                userShrinkageStore.Reset();
                ensureGetUserShrinkage.Clear();

                perUser = ensureGetUserShrinkage.GetOrAdd(userId, () => new Dictionary<DateOnly, IdempotentApiRequest>());
                request = CreateRequest(shrinkageDate, userId);
                perUser[shrinkageDate] = request;
            }
            else
            {
                request = perUser.GetOrAdd(shrinkageDate, () => CreateRequest(shrinkageDate, userId));
            }

            return request.Run(cancellationToken);
        }

















        // Save Activity By User
        public async Task SaveActivityByUserAsync(ActivityDto activity, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["@Activity"] = activity,
                ["CorrelationId"] = correlationId,
            });
            try
            {
                await HttpClient.PostAsJsonAsync("api/shrinkage/save-user-activity",
                    new SaveActivityDto
                    {
                        CorrelationId = correlationId,
                        Activity = activity,
                    }, cancellationToken);
                userShrinkageStore.UpdateUserShrinkage(activity);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.BadRequest })
            {
                logger.LogError(ex, "Failed to save activity.");
                throw new BadRequestException(ex, correlationId);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.NotFound })
            {
                logger.LogError(ex, "Failed to save activity.");
                throw new NotFoundException(ex, correlationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save activity.");
                throw new SaveActivityException(ex, correlationId);
            }
        }

        // Delete Activity By Id
        public async Task DeleteActivityForUserAsync(Guid id, Guid deletedBy, DateOnly activityDate, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();

            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["ActivityId"] = id,
                ["DeletedById"] = deletedBy,
            });
            try
            {
                var request = new DeleteActivityRequest_M
                {
                    CorrelationId = correlationId,
                    ActivityId = id,
                    DeletedBy = deletedBy,
                };
                var response = await HttpClient.DeleteJsonAsync("api/shrinkage/activities", request, cancellationToken);
                response.EnsureSuccessStatusCode();
                userShrinkageStore.DeleteActivityFromUserShrinkage(deletedBy, id, activityDate);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.BadRequest })
            {
                logger.LogError(ex, "Failed to delete activity by id.");
                throw new BadRequestException(ex, correlationId);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.NotFound })
            {
                logger.LogError(ex, "Failed to delete activity by id.");
                throw new NotFoundException(ex, correlationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete activity by id.");
                throw new DeleteActivityException(ex, correlationId);
            }
        }
        public async Task SaveAbsenceAsync(AbsenceDto absence, CancellationToken token)
        {
            var correlationId = Guid.NewGuid();
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["@Request"] = absence,
                ["CorrelationId"] = correlationId,
            });
            try
            {
                await HttpClient.PostAsJsonAsync($"api/shrinkage/absence", new SaveUserAbsenceRequest_M
                {
                    CorrelationId = correlationId,
                    Absence = absence,
                }, token);
                userAbsencesStore.Update(absence);
                userShrinkageStore.RemoveUserShrinkage(absence.UserId, absence.StartDate, absence.EndDate);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.BadRequest })
            {
                logger.LogError(ex, "Failed to save absence.");
                throw new BadRequestException(ex, correlationId);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.NotFound })
            {
                logger.LogError(ex, "Failed to save absence.");
                throw new NotFoundException(ex, correlationId);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.Conflict })
            {
                logger.LogError(ex, "Failed to save absence.");
                throw new ConflictException(ex, correlationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to save absence.");
                throw new SaveAbsenceException(ex, correlationId);
            }
        }
        public ValueTask EnsureGetAbsencesByUser(IReadOnlyList<Guid> userIds, bool forceRefresh, CancellationToken cancellationToken)
        {
            var request = ensureGetUserAbsences.GetOrAdd(userIds, () => new IdempotentApiRequest(async token =>
            {
                var correlationId = Guid.NewGuid();
                using var __ = logger.BeginScope(new Dictionary<string, object>
                {
                    ["CorrelationId"] = correlationId,
                    ["UserIds"] = string.Join(", ", userIds),
                });

                try
                {
                    var absences = await HttpClient.PostAsJsonAsync("api/shrinkage/absences/by-user-ids",
                        new GetAbsencesByUserIdsRequest_M
                        {
                            CorrelationId = correlationId,
                            UserIds = userIds,
                        }, token);
                    absences.EnsureSuccessStatusCode();
                    var response = await absences.Content.ReadFromJsonAsyncNotNull<IReadOnlyList<AbsenceDto>>(token);

                    foreach (var userId in userIds)
                    {
                        userAbsencesStore.InitializeUserAbsences(userId, response.Where(x => x.UserId == userId).OrderByDescending(a => a.StartDate).ToList());
                    }
                }
                catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.BadRequest })
                {
                    logger.LogError(ex, "Failed to get absences by userId.");
                    throw new BadRequestException(ex, correlationId);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to get absences by userId.");
                    throw new GetAbsencesByUserIdsException(ex, correlationId);
                }
            }));

            if (forceRefresh)
            {
                request.Reset();
                userAbsencesStore.Reset();
            }

            return request.Run(cancellationToken);
        }


        public async Task DeleteAbsenceByUserAsync(Guid id, Guid userId, Guid deletedBy, Guid teamId, DateOnly startDate, DateOnly endDate, CancellationToken cancellationToken)
        {
            var correlationId = Guid.NewGuid();
            using var __ = logger.BeginScope(new Dictionary<string, object>
            {
                ["CorrelationId"] = correlationId,
                ["AbsenceId"] = id,
                ["DeletedBy"] = deletedBy,
            });
            try
            {
                var request = new DeleteUserAbsenceRequest_M
                {
                    CorrelationId = correlationId,
                    Id = id,
                    DeletedBy = deletedBy,
                };
                var response = await HttpClient.DeleteJsonAsync("api/shrinkage/absences", request, cancellationToken);
                response.EnsureSuccessStatusCode();
                userAbsencesStore.Remove(userId, id);
                userShrinkageStore.RemoveUserShrinkage(userId, startDate, endDate);
                //var publicHolidays = publicHolidaysStore.GetPublicHolidaysForTeamId(teamId);
                //userDailySummaryStore.RemoveAbsence(startDate, endDate, publicHolidays);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.BadRequest })
            {
                logger.LogError(ex, "Failed to delete absence by id.");
                throw new BadRequestException(ex, correlationId);
            }
            catch (HttpRequestException ex) when (ex is { StatusCode: HttpStatusCode.NotFound })
            {
                logger.LogError(ex, "Failed to delete absence by id.");
                throw new NotFoundException(ex, correlationId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to delete absence by id.");
                throw new DeleteAbsenceException(ex, correlationId);
            }
        }

        public void RemoveIdempotencyRequest(Guid userId, DateOnly date)
        {
            ensureGetUserShrinkage[userId].Remove(date);
        }
    }
}
