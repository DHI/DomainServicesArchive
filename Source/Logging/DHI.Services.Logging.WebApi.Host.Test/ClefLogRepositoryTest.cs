namespace DHI.Services.Logging.WebApi.Host.Test
{
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using Xunit;

	public class ClefLogRepositoryTest
    {
        [Fact]
        public void GetByQueryUsesCorrectDateProperty()
        {
            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream);
            streamWriter.WriteLine("{\"@t\":\"2025-02-12T06:50:10.4437481Z\",\"@mt\":\"Processing Variables. Name: TerminationGracePeriod. Value: 0.00:00:5\",\"@tr\":\"f38c142c0e8037af78f81eb4c9725511\",\"@sp\":\"d57dd80ee2a203e1\",\"LogTime\":\"2025-02-12T06:50:10.4149158Z\",\"Source\":\"DHI.Workflow.CodeWorkflowEngine.CodeWorkflowEngine\"}");
            streamWriter.Flush();
            memoryStream.Position = 0;
            var logRepository = new ClefLogRepository(Path.GetTempPath(), (_) => memoryStream);
            var query = new List<QueryCondition>
            {
                new QueryCondition("Tag", QueryOperator.Equal, "log")
            };
            var entries = logRepository.Get(query);
            var entry = entries.First();

			// Assert that the time used for the log entry is the one specified in the LogTime property, not the one in the @t property
			Assert.Equal("06:50:10.4149158", entry.DateTime.TimeOfDay.ToString());
        }
    }
}