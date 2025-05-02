namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.IO;

    /// <summary>
    ///     JSON file-based JobRepository that is located in the App_Data folder.
    /// </summary>
    public class JobRepository : JobRepository<Guid, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JobRepository"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public JobRepository(string fileName)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName))
        {
        }
    }
}