namespace DHI.Services.Tables.WebApi.Host.Test
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Win32;
    using Xunit;

    internal sealed class SkippableFactAttribute : FactAttribute
    {
        public SkippableFactAttribute()
        {
            if (!_GetOdbcDriverNames().Contains("Microsoft Access Driver (*.mdb, *.accdb)"))
            {
                Skip = "MS Access ODBC driver not installed. Can be installed from: https://www.microsoft.com/en-us/download/details.aspx?id=54920.";
            }
        }

        private static IEnumerable<string> _GetOdbcDriverNames()
        {
            var names = new List<string>();
            using (var localMachineHive = Registry.LocalMachine)
            using (var odbcDriversKey = localMachineHive.OpenSubKey(@"SOFTWARE\ODBC\ODBCINST.INI\ODBC Drivers"))
            {
                if (odbcDriversKey != null)
                {
                    names.AddRange(odbcDriversKey.GetValueNames());
                }
            }

            return names;
        }
    }
}
