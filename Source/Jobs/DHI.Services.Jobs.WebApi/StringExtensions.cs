namespace DHI.Services.Jobs.WebApi
{
    using System;

    internal static class StringExtensions
    {
        /// <summary>
        /// Returns the input string with the first character converted to uppercase
        /// </summary>
        public static string CapitalizeFirstLetter(this string source)
        {
            if (string.IsNullOrEmpty(source)) throw new ArgumentException("There is no first letter");

            var charArray = source.ToCharArray();
            charArray[0] = char.ToUpper(charArray[0]);
            return new string(charArray);
        }
    }
}
