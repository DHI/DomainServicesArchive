namespace DHI.Services.Mails
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     Class MailTemplate.
    /// </summary>
    [Serializable]
    public class MailTemplate : BaseNamedEntity<string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MailTemplate" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The name.</param>
        public MailTemplate(string id, string name)
            : base(id, name)
        {
        }

        /// <summary>
        ///     Gets or sets the subject.
        /// </summary>
        /// <value>The subject.</value>
        public string Subject { get; set; }

        /// <summary>
        ///     Gets or sets the body.
        /// </summary>
        /// <value>The body.</value>
        public string Body { get; set; }

        /// <summary>
        ///     Optional bodies to serve different templates
        /// </summary>
        public Dictionary<string, string> Bodies { get; set; }

        /// <summary>
        ///     Gets or sets from.
        /// </summary>
        /// <value>From.</value>
        public string From { get; set; }

        /// <summary>
        ///     Gets or sets from display name.
        /// </summary>
        /// <value>From display name.</value>
        public string FromDisplayName { get; set; }
    }
}