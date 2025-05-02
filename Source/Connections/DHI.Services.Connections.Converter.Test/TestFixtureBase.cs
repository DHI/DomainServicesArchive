namespace DHI.Services.Connections.Converter.Test
{
    using System;
    using System.IO;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Connections.Converter.Factory;

    public abstract class TestFixtureBase : IDisposable
    {
        private readonly string _appDataPath = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _tempContentRootPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public TestFixtureBase()
        {
            _tempContentRootPath = Path.Combine(_appDataPath, $"Tests-{DateTime.Now.Ticks}");
            TempAppDataPath = Path.Combine(_tempContentRootPath, "App_Data");
            Directory.CreateDirectory(TempAppDataPath);
            CopyFileToTempAppDataPath("connections.json");

            _jsonOptions = new JsonSerializerOptions
            {
                Converters =
                {
                    //new InterfaceConverter<IConnection>(),
                    //new InterfaceWriteConverter<IConnection>(),
                    new ConnectionDictionaryConverter<string, IConnection>()
                }
            };

            foreach (var converterFactory in ConverterFactory.ConnectionConverterScan())
            {
                _jsonOptions.Converters.Add(converterFactory);
            }

            _jsonOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        }

        public string TempAppDataPath { get; }

        public JsonSerializerOptions SerializerOptions => _jsonOptions;

        public void CopyFileToTempAppDataPath(string sourceFileName)
        {
            var destinationFilePath = Path.Combine(TempAppDataPath, sourceFileName);
            File.Copy(Path.Combine(_appDataPath, sourceFileName), destinationFilePath, true);
            new FileInfo(destinationFilePath).IsReadOnly = false;
        }

        private void _CopyFolderToTempAppDataPath(string sourceFolderName)
        {
            var destinationFolderPath = Path.Combine(TempAppDataPath, sourceFolderName);
            Directory.CreateDirectory(destinationFolderPath);
            var sourceFolder = new DirectoryInfo(Path.Combine(_appDataPath, sourceFolderName));
            foreach (var file in sourceFolder.GetFiles())
            {
                file.CopyTo(Path.Combine(destinationFolderPath, file.Name));
                file.IsReadOnly = false;
            }
        }

        private bool _disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (Directory.Exists(_tempContentRootPath))
                    {
                        Directory.Delete(_tempContentRootPath, true);
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                _disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~TestFixtureBase()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
