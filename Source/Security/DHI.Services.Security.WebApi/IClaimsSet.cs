namespace DHI.Services.Security.WebApi
{
	using DHI.Services.Accounts;
	using System.Collections.Generic;
	using System.Security.Claims;

	public interface IClaimsSet
	{
		IEnumerable<Claim> GetClaims(Account account);
	}
}