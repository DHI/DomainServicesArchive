namespace DHI.Services.Security.WebApi
{
    using System;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    ///     JSON file-based RefreshTokenRepository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="Authentication.RefreshTokenRepository" />
    public class RefreshTokenRepository : Authentication.RefreshTokenRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RefreshTokenRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public RefreshTokenRepository(string fileName)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName))
        {
        }

        public RefreshTokenRepository(string fileName, JsonSerializerOptions serializerOptions, JsonSerializerOptions deserializerOptions = null)
            : base(fileName, serializerOptions, deserializerOptions)
        {
        }
    }
}