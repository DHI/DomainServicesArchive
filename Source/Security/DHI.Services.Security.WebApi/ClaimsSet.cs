namespace DHI.Services.Security.WebApi
{
	using DHI.Services.Accounts;
	using DHI.Services.Authorization;
	using System.Collections.Generic;
	using System.IdentityModel.Tokens.Jwt;
	using System.Linq;
	using System.Security.Claims;

	public class ClaimsSet : IClaimsSet
	{
		private readonly UserGroupService _userGroupService;

		public ClaimsSet(UserGroupService userGroupService)
		{
			_userGroupService = userGroupService;
		}

		public IEnumerable<Claim> GetClaims(Account account)
		{
			var claims = new List<Claim>
			{
				new(JwtRegisteredClaimNames.Sub, account.Id),
				new(ClaimTypes.Name, account.Name)
			};

			if (!string.IsNullOrEmpty(account.Email))
			{
				claims.Add(new Claim(JwtRegisteredClaimNames.Email, account.Email));
			}

			if (!string.IsNullOrEmpty(account.Company))
			{
				claims.Add(new Claim("company", account.Company));
			}

			claims.AddRange(account.Metadata.Select(kvp => new Claim(kvp.Key.ToString().ToLower(), kvp.Value.ToString())));
			var roles = account.GetRoles();
			claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
			var userGroups = _userGroupService.GetIds(account.Id);
			claims.AddRange(userGroups.Select(userGroup => new Claim(ClaimTypes.GroupSid, userGroup)));
			return claims;
		}
	}
}
