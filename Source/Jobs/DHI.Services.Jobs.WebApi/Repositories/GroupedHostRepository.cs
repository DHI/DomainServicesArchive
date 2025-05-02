namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.IO;

    /// <summary>
    ///     JSON file-based grouped host repository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="Jobs.GroupedHostRepository" />
    public class GroupedHostRepository : Jobs.GroupedHostRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="GroupedHostRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public GroupedHostRepository(string fileName)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName)) 
        {
        }
    }
}