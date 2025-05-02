namespace DHI.Services.Rasters.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public abstract class BaseFileRepositoryFixture : IDisposable
    {
        protected BaseFileRepositoryFixture(IEnumerable<string> fileNames)
        {
            var tempPath = Path.GetTempPath();
            FolderPath = Path.Combine(tempPath, "ds-test-repository" + Guid.NewGuid());
            Directory.CreateDirectory(FolderPath);

            foreach (var fileName in fileNames)
            {
                var filePath = Path.Combine(FolderPath, fileName);
                File.Copy(@"..\..\..\Data\" + fileName, filePath, true);
                new FileInfo(filePath).IsReadOnly = false;
            }
        }

        protected string FolderPath { get; }

        public virtual void Dispose()
        {
            if (Directory.Exists(FolderPath))
            {
                Directory.Delete(FolderPath, true);
            }
        }
    }
}