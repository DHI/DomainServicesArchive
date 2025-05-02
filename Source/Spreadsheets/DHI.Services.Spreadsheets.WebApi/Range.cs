using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.Spreadsheets.WebApi.Host.Test")]
namespace DHI.Services.Spreadsheets.WebApi
{
    using System;

    internal class Range
    {
        internal static Spreadsheets.Range Parse(string s)
        {
            var cells = s.Split(',');

            if (cells.Length != 2)
            {
                throw new ArgumentException($"Cannot parse string '{s}'. A range must be given as two corner cells separated by a comma (<upper-left cell>,<lower-right cell>). A cell must be given in the R1C1 style.\nExample: R0C0,R8C24", nameof(s));
            }

            return new Spreadsheets.Range(Cell.Parse(cells[0]), Cell.Parse(cells[1]));
        }
    }
}