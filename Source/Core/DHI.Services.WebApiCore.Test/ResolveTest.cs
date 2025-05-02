namespace DHI.Services.WebApiCore.Test
{
    using System;
    using System.IO;
    using FluentAssertions;
    using WebApiCore;
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
        public void AppDomainDataDirectoryNotFoundThrows()
        {
            const string connectionString = "[AppData]MyFile.dat";
            Action action = () => connectionString.Resolve();

            action.Should().Throw<ArgumentException>().And.ParamName.Should().Be("connectionString");
        }

        [Fact]
        public void ResolveAppDataFolderIsOk()
        {
            const string connectionString = "[AppData]MyFile.dat";
            var appDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            AppDomain.CurrentDomain.SetData("DataDirectory", appDataFolder);

            var resolvedConnectionString = connectionString.Resolve();
            resolvedConnectionString.Should().Be(Path.Combine(appDataFolder, "MyFile.dat"));

            AppDomain.CurrentDomain.SetData("DataDirectory", null);
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
            const string connectionString = "Server=localhost;Port=5432;Database=[AppData]MCSQLiteTest.sqlite;dbflavour=SQLite;User Id=postgres;Password=[env:SQLitePassword];";
            var appDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data");
            AppDomain.CurrentDomain.SetData("DataDirectory", appDataFolder);
            const string password = "myPassword";
            Environment.SetEnvironmentVariable("SQLitePassword", password);
            var result = $"Server=localhost;Port=5432;Database={appDataFolder}\\MCSQLiteTest.sqlite;dbflavour=SQLite;User Id=postgres;Password={password};";

            connectionString.Resolve().Should().Be(result);

            AppDomain.CurrentDomain.SetData("DataDirectory", null);
            Environment.SetEnvironmentVariable("SQLitePassword", null);
        }
    }
}