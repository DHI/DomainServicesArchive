namespace DHI.Services.TimeSeries.Web
{
    using System.Web;

    internal static class ExtensionMethods
    {
        public static string Resolve(this string connectionString)
        {
            return HttpContext.Current != null ? connectionString.Replace("[AppData]", HttpContext.Current.Server.MapPath(@"~\App_Data") + @"\") : connectionString;
        }
    }
}