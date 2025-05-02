namespace DHI.Services.Security.WebApi
{
    using System;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    ///     JSON file-based UserGroupRepository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="Authorization.UserGroupRepository" />
    public class UserGroupRepository : Authorization.UserGroupRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UserGroupRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public UserGroupRepository(string fileName)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName))
        {
        }

        public UserGroupRepository(string fileName, JsonSerializerOptions serializerOptions, JsonSerializerOptions deserializerOptions = null)
            : base(fileName, serializerOptions, deserializerOptions)
        {
        }
    }
}