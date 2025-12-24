using System.Security.Claims;

namespace BlazorLayout
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetId(this ClaimsPrincipal user)
        {
            return user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("User has no id");
        }
    }

}
