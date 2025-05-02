namespace DHI.Services.Logging.WebApi.Host.Test
{
	using System.Collections.Generic;
	using System.IO;
	using Xunit;

	public class LogServiceTest
    {
        [Fact]
        public void LogsAreReturnedInTheOrderDefinedByTheLogTimeProperty()
		{
			using var memoryStream = new MemoryStream();
			using var streamWriter = new StreamWriter(memoryStream);
			streamWriter.WriteLine("{\"@t\":\"2025-02-12T06:50:00.0000000Z\",\"@mt\":\"Log 4\",\"@tr\":\"f38c142c0e8037af78f81eb4c9725511\",\"@sp\":\"d57dd80ee2a203e1\",\"LogTime\":\"2025-02-12T06:50:40.0000000Z\",\"Source\":\"DHI.Workflow.CodeWorkflowEngine.CodeWorkflowEngine\"}");
			streamWriter.WriteLine("{\"@t\":\"2025-02-12T06:50:10.0000000Z\",\"@mt\":\"Log 3\",\"@tr\":\"f38c142c0e8037af78f81eb4c9725511\",\"@sp\":\"d57dd80ee2a203e1\",\"LogTime\":\"2025-02-12T06:50:30.0000000Z\",\"Source\":\"DHI.Workflow.CodeWorkflowEngine.CodeWorkflowEngine\"}");
			streamWriter.WriteLine("{\"@t\":\"2025-02-12T06:50:20.0000000Z\",\"@mt\":\"Log 2\",\"@tr\":\"f38c142c0e8037af78f81eb4c9725511\",\"@sp\":\"d57dd80ee2a203e1\",\"LogTime\":\"2025-02-12T06:50:10.0000000Z\",\"Source\":\"DHI.Workflow.CodeWorkflowEngine.CodeWorkflowEngine\"}");
			streamWriter.WriteLine("{\"@t\":\"2025-02-12T06:50:30.0000000Z\",\"@mt\":\"Log 1\",\"@tr\":\"f38c142c0e8037af78f81eb4c9725511\",\"@sp\":\"d57dd80ee2a203e1\",\"LogTime\":\"2025-02-12T06:50:00.0000000Z\",\"Source\":\"DHI.Workflow.CodeWorkflowEngine.CodeWorkflowEngine\"}");
			streamWriter.Flush();
			memoryStream.Position = 0;
			var logRepository = new ClefLogRepository(Path.GetTempPath(), (_) => memoryStream);
			
			var logService = new LogService(logRepository);
			var query = new List<QueryCondition>
			{
				new QueryCondition("Tag", QueryOperator.Equal, "log")
			};
			var entries = logService.Get(query);

			//Assert that the logs are returned in the order defined by the LogTime property
			Assert.Collection(entries,
				item => Assert.Equal("Log 1", item.Text),
				item => Assert.Equal("Log 2", item.Text),
				item => Assert.Equal("Log 3", item.Text),
				item => Assert.Equal("Log 4", item.Text));
		}
	}
}