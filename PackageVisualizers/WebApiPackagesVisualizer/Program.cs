namespace WebApiPackagesVisualizer
{
    using System.Diagnostics;
    using System.IO;

    internal class Program
    {
        private static void Main()
        {
            var exePath = Path.Combine(Directory.GetCurrentDirectory(), "NuGetPackageVisualizer.exe");
            var startInfo = new ProcessStartInfo(exePath, "-file:\"..\\..\\packages.config\" -repositoryuri:\"http://dhi-nuget-server.azurewebsites.net/nuget\"");
            Process.Start(startInfo);
        }
    }
}