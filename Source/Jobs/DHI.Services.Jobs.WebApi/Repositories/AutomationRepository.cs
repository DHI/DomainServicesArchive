namespace DHI.Services.Jobs.WebApi
{
    using System;
    using System.IO;

    /// <summary>
    ///     JSON file-based automation repository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="AutomationRepository" />
    public class AutomationRepository : Automations.AutomationRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AutomationRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public AutomationRepository(string fileName)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName)) 
        {
        }
    }
}