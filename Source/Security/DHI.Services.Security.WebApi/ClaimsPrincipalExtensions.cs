namespace DHI.Services.Security.WebApi
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    /// Helpers for working with JWT / WS-Fed claim sets.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns the user-id for the principal.
        /// • prefers standard JWT <c>sub</c>  
        /// • falls back to WS-Fed <c>NameIdentifier</c>  
        /// • finally tries the short <c>nameid</c> form
        /// Throws if any claim type has more than one value.
        /// </summary>
        public static string? GetUserIdFromAnyClaim(this ClaimsPrincipal user)
        {
            return GetSingleClaimValue(user, JwtRegisteredClaimNames.Sub)
                ?? GetSingleClaimValue(user, ClaimTypes.NameIdentifier)
                ?? GetSingleClaimValue(user, "nameid");
        }

        private static string? GetSingleClaimValue(ClaimsPrincipal user, string claimType)
        {
            var matches = user.Claims.Where(c => c.Type == claimType).ToList();
            return matches.Count switch
            {
                0 => null,
                1 => matches[0].Value,
                _ => throw new InvalidOperationException($"Multiple claims of type '{claimType}' found when only one was expected.")
            };
        }
    }
}
