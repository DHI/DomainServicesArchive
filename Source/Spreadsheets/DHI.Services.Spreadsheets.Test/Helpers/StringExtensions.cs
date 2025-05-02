namespace DHI.Services.Spreadsheets.Test.Helpers
{
    using System;
    using System.Linq;

    internal static class StringExtensions
    {
        /// <summary>
        /// Trims This is used to make the strings in the unit tests a bit easier to read.
        /// </summary>
        public static string TrimNewLineAtStart(this string str)
        {
            if (str.StartsWith(Environment.NewLine))
            {
                return str[Environment.NewLine.Length..];
            }
            else
            {
                var eol = new string[] {
                    "\r\n",
                    "\r",
                    "\n",
                };
                var newLine = eol.Select(x =>
                {
                    if (str.StartsWith(x))
                    {
                        return x;
                    }
                    return string.Empty;

                }).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(newLine) == false)
                {
                    return str[newLine.Length..];
                }
            }
            return str;
        }
    }
}
