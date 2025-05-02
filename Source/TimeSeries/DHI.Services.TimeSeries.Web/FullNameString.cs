namespace DHI.Services.TimeSeries.Web
{
    using System.Linq;

    internal static class FullNameString
    {
        internal static string FromUrl(string s)
        {
            const string pipe = "<<pipecharcter>>";
            s = s.Replace("||", pipe);
            s = s.Replace("|", "/");
            s = s.Replace(pipe, "|");
            return s;
        }

        internal static string ToUrl(string s)
        {
            s = s.Replace("|", "||");
            s = s.Replace("/", "|");
            return s;
        }

        internal static string[] FromUrl(string[] a)
        {
            return a.Select(FromUrl).ToArray();
        }
    }
}