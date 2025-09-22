namespace DHI.Services.Security.WebApi
{
    using System;
    using System.Linq;
    using System.Net;
    using Microsoft.AspNetCore.HttpOverrides;

    /// <summary>
    ///     Static class with IP address whitelisting functions.
    /// </summary>
    public static class IpWhitelist
    {
        /// <summary>
        ///     Tests an IP address against a set of whitelisted CIDR blocks.
        /// </summary>
        /// <param name="whitelistCidrBlocks">Array of CIDR blocks.</param>
        /// <param name="clientIp">IP Address to test.</param>
        /// <returns></returns>
        public static bool Validate(string[] whitelistCidrBlocks, string clientIp)
        {
            if (whitelistCidrBlocks == null || !whitelistCidrBlocks.Any())
            {
                return true;
            }

            var cleanWhitelistCidrBlocks = whitelistCidrBlocks.Select(s => s.Split(new[] { "&Comment:", "&comment:" }, StringSplitOptions.None).First().Trim());

            var ip = IPAddress.Parse(clientIp);

            var networks = cleanWhitelistCidrBlocks.Select(b => b.Split(new [] { '/' }, 2)).Select(ParseInputToIpNetwork);

            return networks.Any(n => n.Contains(ip));

            static Microsoft.AspNetCore.HttpOverrides.IPNetwork ParseInputToIpNetwork(string[] cidrBlocks)
            {
                return new Microsoft.AspNetCore.HttpOverrides.IPNetwork(
                    IPAddress.Parse(cidrBlocks[0]),
                    cidrBlocks.Length == 1 ?
                        32 :
                        int.TryParse(cidrBlocks[1], out var prefix) ?
                            prefix :
                            throw new FormatException($"{cidrBlocks[1]} is not a valid CIDR prefix"));
            }
        }
    }
}
