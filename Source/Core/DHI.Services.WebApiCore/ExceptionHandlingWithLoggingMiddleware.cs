namespace DHI.Services.WebApiCore
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Notifications;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public class ExceptionHandlingWithLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingWithLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IConfiguration configuration, ILogger logger)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                LogLevel logLevel;
                var verbose = configuration.GetValue("Logger:Verbose", false);
                var logLevelString = configuration.GetValue<string>("Logger:LogLevel");
                if (logLevelString is null)
                {
                    logLevel = LogLevel.Warning;
                }
                else
                {
                    logLevel = Enum.TryParse(logLevelString, out LogLevel level) ? level : LogLevel.Warning;
                }

                var request = context.Request;
                var metadata = new Dictionary<string, object> { { "Method", request.Method }, { "Path", request.Path } };
                if (request.QueryString.HasValue)
                {
                    metadata.Add("QueryString", request.QueryString);
                }

                logger.Log(logLevel, ex, "Caught Exception in {Source} from HTTP Method = {HttpMethod} with path = {HttpPath} and (optional) query string = {QueryString} ", "Web API exception", request.Method, request.Path, request.QueryString.HasValue ? request.QueryString : "None");
                await ex.ToHttpResponse(context);
            }
        }
    }
}