namespace DHI.Services.WebApiCore
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;

    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await ex.ToHttpResponse(context);
            }
        }
    }
}