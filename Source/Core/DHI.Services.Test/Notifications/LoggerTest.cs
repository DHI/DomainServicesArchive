namespace DHI.Services.Test.Notifications
{
    using DHI.Services.Notifications;
    using Xunit;

    public class LoggerTest
    {
        [Fact]
        public void GetLoggerTypesIsOk()
        {
            var loggerTypes = Logger.GetLoggerTypes();
            Assert.Contains(typeof(SimpleLogger), loggerTypes);
        }
    }
}