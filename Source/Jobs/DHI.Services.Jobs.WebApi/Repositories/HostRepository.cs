namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.IO;

    /// <summary>
    ///     JSON file-based HostRepository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="Jobs.HostRepository" />
    public class HostRepository : Jobs.HostRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="HostRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public HostRepository(string fileName)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName)) 
        {
        }
    }
}