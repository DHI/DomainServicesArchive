using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.Spreadsheets.WebApi.Host.Test")]
namespace DHI.Services.Spreadsheets.WebApi
{
    using System;

    internal class Cell
    {
        internal static Spreadsheets.Cell Parse(string s)
        {
            var rpos = s.IndexOf("R", StringComparison.InvariantCultureIgnoreCase);
            var cpos = s.IndexOf("C", StringComparison.InvariantCultureIgnoreCase);
            try
            {
                var row = int.Parse(s.Substring(rpos + 1, cpos - (rpos + 1)));
                var col = int.Parse(s.Substring(cpos + 1, s.Length - (cpos + 1)));
                return new Spreadsheets.Cell(row, col);
            }
            catch (Exception)
            {
                throw new ArgumentException($"Cannot parse string '{s}'. A cell must be given in the R1C1 style - e.g. R8C24.", nameof(s));
            }
        }
    }
}