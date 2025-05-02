namespace DHI.Services.Security.WebApi.Host.Test
{
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    internal class FakeRemoteIpAddressMiddleware
    {
        private readonly RequestDelegate _next;

        public FakeRemoteIpAddressMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.168.1.32");
            await _next(httpContext);
        }
    }
}