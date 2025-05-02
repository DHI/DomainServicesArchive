namespace DHI.Services.Security.WebApi
{
    using System;
    using System.IO;
    using System.Text.Json;

    /// <summary>
    ///     JSON file-based MailTemplateRepository that is located in the App_Data folder.
    /// </summary>
    /// <seealso cref="Mails.MailTemplateRepository" />
    public class MailTemplateRepository : Mails.MailTemplateRepository
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MailTemplateRepository" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public MailTemplateRepository(string fileName)
            : base(Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), fileName))
        {
        }

        public MailTemplateRepository(string fileName, JsonSerializerOptions serializerOptions, JsonSerializerOptions deserializerOptions = null)
            : base(fileName, serializerOptions, deserializerOptions)
        {
        }
    }
}
