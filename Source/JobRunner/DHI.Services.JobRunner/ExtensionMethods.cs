namespace DHI.Services.JobRunner
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text.RegularExpressions;

    public static class ExtensionMethods
    {
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

            if (connectionString.Contains(@"[Path]"))
            {
                return connectionString.Replace("[Path]", Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath)) + @"\");
            }

            return connectionString;
        }
    }
}