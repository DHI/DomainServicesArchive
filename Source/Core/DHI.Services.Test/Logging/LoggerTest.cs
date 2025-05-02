namespace DHI.Services.Test.Logging
{
    using DHI.Services.Logging;
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