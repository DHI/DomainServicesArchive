namespace DHI.Services.Test
{
    using System;
    using System.IO;

    public class ServicesFixture
    {
        public ServicesFixture()
        {
            var _filePath = Path.Combine(Path.GetTempPath(), $"__connections_{DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss_sss")}.json");
            var connectionRepository = new ConnectionRepository(_filePath);
            var connection = new FakeConnection("MyConnection", "My Service Connection");
            connectionRepository.Add(connection);
            Services.Configure(connectionRepository);
        }
    }
}