namespace DHI.Services.WebApiCore
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public static class ExtensionMethods
    {
        /// <summary>
        ///     Resolves a connection string according to the [AppData] and [env:{envVar}] conventions".
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public static string Resolve(this string connectionString)
        {
            var pattern = new Regex(@"\[env:(?<variable>\w+)\]");
            var match = pattern.Match(connectionString);
            if (match.Success)
            {
                var variable = match.Groups["variable"].Value;
                var value = Environment.GetEnvironmentVariable(variable);
                if (value is null)
                {
                    throw new ArgumentException($"Environment variable '{variable}' was not found.", nameof(connectionString));
                }

                connectionString = connectionString.Replace(match.Value, value);
            }

            if (!connectionString.Contains(@"[AppData]"))
            {
                return connectionString;
            }

            var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory");
            if (!(dataDirectory is null))
            {
                // https://github.com/DHI/DomainServices/issues/472
                var dataDirectoryPath = Path.Join($"{dataDirectory}", Path.DirectorySeparatorChar.ToString());

                return connectionString.Replace("[AppData]", $"{dataDirectoryPath}");
            }

            var message = new StringBuilder();
            message.AppendLine("Application Domain property 'DataDirectory' is not set.");
            message.AppendLine("This must be set at application startup using AppDomain.CurrentDomain.SetData()");
            message.AppendLine("Example: AppDomain.CurrentDomain.SetData(\"DataDirectory\", Path.Combine(contentRootPath, \"App_Data\"));");
            throw new ArgumentException(message.ToString(), nameof(connectionString));
        }

        /// <summary>
        ///     Creates a response where an exception is mapped to an HTTP status code.
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <param name="context">The HTTP context.</param>
        public static Task ToHttpResponse(this Exception ex, HttpContext context)
        {
            HttpStatusCode code;
            if (ex is KeyNotFoundException || ex is ArgumentOutOfRangeException)
            {
                code = HttpStatusCode.NotFound;
            }
            else if (ex is ArgumentException)
            {
                code = HttpStatusCode.BadRequest;
            }
            else if (ex is NotImplementedException || ex is NotSupportedException)
            {
                code = HttpStatusCode.NotImplemented;
            }
            else
            {
                code = HttpStatusCode.InternalServerError;
            }

            var result = JsonSerializer.Serialize(new { error = ex.ToString() });
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(result);
        }

        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }

        public static IApplicationBuilder UseExceptionHandlingWithLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingWithLoggingMiddleware>();
        }

        /// <summary>
        ///     Converts a query collection (the query string parameters) to a Query object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queryCollection">The query collection.</param>
        public static Query<T> ToQuery<T>(this IQueryCollection queryCollection)
        {
            var query = new Query<T>();
            foreach (var condition in queryCollection)
            {
                var queryCondition = new QueryCondition(condition.Key, condition.Value.ToString().ToObject());
                query.Add(queryCondition);
            }

            return query;
        }

        /// <summary>
        ///     Converts a string representation of a value object to an object.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        public static object ToObject(this string stringValue)
        {
            if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
            {
                return intValue;
            }

            if (double.TryParse(stringValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var doubleValue))
            {
                return doubleValue;
            }

            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeValue))
            {
                return dateTimeValue;
            }

            if (bool.TryParse(stringValue, out var boolValue))
            {
                return boolValue;
            }

            if (stringValue.StartsWith("LogLevel."))
            {
                if (Enum.TryParse(stringValue.Remove(0, "LogLevel.".Length), out LogLevel logLevel))
                {
                    return logLevel;
                }
            }

            return stringValue;
        }

        /// <summary>
        ///     Return all additional parameters (excluding the given reserved parameters) from a query collection (the query string parameters)
        /// </summary>
        /// <param name="queryCollection">The query collection.</param>
        /// <param name="reservedParameters">The reserved parameters.</param>
        public static Parameters GetAdditionalParameters(this IQueryCollection queryCollection, string[] reservedParameters)
        {
            var queryParameters = queryCollection.ToDictionary(pair => pair.Key.ToLowerInvariant(), pair => pair.Value.ToString());
            var parameters = new Parameters();
            foreach (var pair in queryParameters)
            {
                if (!reservedParameters.Contains(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
                {
                    parameters.Add(pair.Key, pair.Value);
                }
            }

            return parameters;
        }

        public static T GetMandatoryValue<T>(this IConfiguration configuration, string key)
        {
            var value = configuration.GetValue<T>(key);
            if (value is not null)
            {
                return value;
            }

            throw new ArgumentException($"The configuration does not contain the mandatory key '{key}'. You probably need to add this key and a corresponding value to the appsettings.json file.", nameof(key));
        }
    }
}