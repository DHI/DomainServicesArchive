namespace DHI.Services.WebApiCore
{
    using System.Linq;

    public static class FullNameString
    {
        public static string FromUrl(string s)
        {
            const string pipe = "<<pipecharacter>>";
            s = s.Replace("||", pipe);
            s = s.Replace("|", "/");
            s = s.Replace(pipe, "|");
            return s;
        }

        public static string ToUrl(string s)
        {
            s = s.Replace("|", "||");
            s = s.Replace("/", "|");
            return s;
        }

        public static string[] FromUrl(string[] a)
        {
            return a.Select(FromUrl).ToArray();
        }
    }
}