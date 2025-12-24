using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace BlazorLayout.Authentification
{
    public sealed class FakeAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static readonly ClaimsPrincipal FakeUser =
            new(new ClaimsIdentity(
                new[]
                {
                new Claim(ClaimTypes.NameIdentifier, "diraneserges@gmail.com"),
                new Claim(ClaimTypes.Email, "diraneserges@gmail.com"),
                new Claim(ClaimTypes.Name, "Dirane Serges"),
                },
                authenticationType: "FakeAuth"));

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(FakeUser));
        }

      

    }
}
