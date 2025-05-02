namespace DHI.Services.JobRunner.Test
{
    using System;
    using System.IO;
    using System.Reflection;
    using FluentAssertions;
    using Xunit;

    public class ResolveTest
    {
        [Fact]
        public void EnvironmentVariableNotFoundThrows()
        {
            const string connectionString = "[env:NonExistingConnectionString]";
            Action resolve = () => connectionString.Resolve();

            resolve.Should().Throw<ArgumentException>().And.ParamName.Should().Be("connectionString");
        }

        [Fact]
        public void ResolveAppFolderIsOk()
        {
            const string connectionString = "[Path]MyFile.dat";
            var appFolder = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath));

            connectionString.Resolve().Should().Be(Path.Combine(appFolder, "MyFile.dat"));
        }

        [Fact]
        public void ResolveEnvironmentVariableIsOk()
        {
            const string connectionString = "[env:LogServiceConnectionString]";
            const string variable = "Server=localhost;Port=5432;Database=LoggerTest;User Id=postgres;Password=myPassword;";
            Environment.SetEnvironmentVariable("LogServiceConnectionString", variable);

            connectionString.Resolve().Should().Be(variable);

            Environment.SetEnvironmentVariable("LogServiceConnectionString", null);
        }

        [Fact]
        public void ResolveIsOk()
        {
            const string connectionString = "Server=localhost;Port=5432;Database=[Path]MCSQLiteTest.sqlite;dbflavour=SQLite;User Id=postgres;Password=[env:SQLitePassword];";
            var appFolder = Path.GetDirectoryName(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath));
            const string password = "myPassword";
            Environment.SetEnvironmentVariable("SQLitePassword", password);
            var result = $"Server=localhost;Port=5432;Database={appFolder}\\MCSQLiteTest.sqlite;dbflavour=SQLite;User Id=postgres;Password={password};";

            connectionString.Resolve().Should().Be(result);

            Environment.SetEnvironmentVariable("SQLitePassword", null);
        }
    }
}