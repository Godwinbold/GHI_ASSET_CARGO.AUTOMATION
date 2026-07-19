using System.Security.Claims;

namespace GHI_ASSET_CARGO.API.Extensions
{
    public static class UserClaimsExtension
    {
        /// <summary>
        /// Extracts user audit information from the current principal
        /// </summary>
        public static (string UserId, string Email, string FullName) GetAuditUserInfo(this ClaimsPrincipal user)
        {
            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
            var email = user.FindFirstValue(ClaimTypes.Email) ?? "unknown@email.com";
            var firstName = user.FindFirstValue(ClaimTypes.GivenName) ?? "";
            var lastName = user.FindFirstValue(ClaimTypes.Surname) ?? "";
            var fullName = $"{firstName} {lastName}".Trim();
            
            return (userId, email, fullName);
        }

        /// <summary>
        /// Gets the user's IP address
        /// </summary>
        public static string GetClientIpAddress(this HttpContext context)
        {
            if (context?.Request?.Headers?.ContainsKey("X-Forwarded-For") == true)
            {
                return context.Request.Headers["X-Forwarded-For"].ToString().Split(',')[0].Trim();
            }
            return context?.Connection?.RemoteIpAddress?.ToString() ?? "";
        }
    }
}
