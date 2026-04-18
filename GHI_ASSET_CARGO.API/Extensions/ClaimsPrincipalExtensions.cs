using System.Linq;
using System.Security.Claims;

namespace GHI_ASSET_CARGO.API.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static IEnumerable<string> GetAirlineIds(this ClaimsPrincipal user)
        {
            if (user == null)
                return Enumerable.Empty<string>();

            return user.FindAll("AirlineId")
                .SelectMany(c => c.Value.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(value => value.Trim())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}
