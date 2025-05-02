namespace DHI.Services.Jobs.Automations.Triggers;

using System;
using System.Text;
using System.Text.RegularExpressions;

internal static class ExtensionMethods
{
    /// <summary>
    ///     Resolves a connection string according to the [AppData] and [env:{envVar}] conventions".
    /// </summary>
    /// <param name="connectionString">The connection string.</param>
    internal static string Resolve(this string connectionString)
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
            return connectionString.Replace("[AppData]", $"{dataDirectory}\\");
        }

        var message = new StringBuilder();
        message.AppendLine("Application Domain property 'DataDirectory' is not set.");
        message.AppendLine("This must be set at application startup using AppDomain.CurrentDomain.SetData()");
        message.AppendLine("Example: AppDomain.CurrentDomain.SetData(\"DataDirectory\", Path.Combine(contentRootPath, \"App_Data\"));");
        throw new ArgumentException(message.ToString(), nameof(connectionString));
    }
}