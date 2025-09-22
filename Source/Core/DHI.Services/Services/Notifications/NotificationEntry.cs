namespace DHI.Services.Notifications
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     An immutable structure representing a notification entry
    /// </summary>
    public readonly struct NotificationEntry : IComparable<NotificationEntry>, IEntity<Guid>
    {
        private readonly IDictionary<string, object> _metadata;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationEntry" /> struct.
        /// </summary>
        /// <param name="id">The unique identifier</param>
        /// <param name="notificationLevel">The notification level.</param>
        /// <param name="text">The text message.</param>
        /// <param name="source">The source.</param>
        /// <param name="tag">A tag.</param>
        /// <param name="machineName">The machine name.</param>
        /// <param name="dateTime">The date time.</param>
        /// <param name="metadata">Metadata.</param>
        public NotificationEntry(Guid id, NotificationLevel notificationLevel, string text, string source, string tag = null, string machineName = null, DateTime dateTime = default, IDictionary<string, object> metadata = null)
            : this()
        {
            Guard.Against.NullOrEmpty(text, nameof(text));
            Guard.Against.NullOrEmpty(source, nameof(source));
            Id = id;
            NotificationLevel = notificationLevel;
            Text = text;
            Source = source;
            Tag = tag;
            DateTime = dateTime == default ? DateTime.Now : dateTime;
            MachineName = machineName ?? Environment.GetEnvironmentVariable("COMPUTERNAME");
            _metadata = metadata ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// NEW ctor – different **parameter order**, no defaults, has the attribute
        /// </summary>
        /// <param name="id"></param>
        /// <param name="notificationLevel"></param>
        /// <param name="text"></param>
        /// <param name="source"></param>
        /// <param name="dateTime"></param>
        /// <param name="tag"></param>
        /// <param name="machineName"></param>
        /// <param name="metadata"></param>
        [JsonConstructor]
        private NotificationEntry(Guid id, NotificationLevel notificationLevel, string text, string source, DateTime dateTime, string? tag, string? machineName, IDictionary<string, object>? metadata)
            : this()
        {
            Guard.Against.NullOrEmpty(text, nameof(text));
            Guard.Against.NullOrEmpty(source, nameof(source));
            
            Id = id;
            NotificationLevel = notificationLevel;
            Text = text;
            Source = source;
            Tag = tag;
            DateTime = dateTime == default ? DateTime.Now : dateTime;
            MachineName = string.IsNullOrWhiteSpace(machineName) ? Environment.GetEnvironmentVariable("COMPUTERNAME") : machineName;
            _metadata = metadata ?? new Dictionary<string, object>();
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotificationEntry" /> struct.
        ///     Just forwards to the new private one as before
        /// </summary>
        /// <param name="notificationLevel">The notification level.</param>
        /// <param name="text">The text message.</param>
        /// <param name="source">The source.</param>
        /// <param name="tag">A tag.</param>
        /// <param name="machineName">The machine name.</param>
        /// <param name="dateTime">The date time.</param>
        public NotificationEntry(NotificationLevel notificationLevel, string text, string source, string? tag = null, string? machineName = null, DateTime dateTime = default, IDictionary<string, object>? metadata = null)
        : this(Guid.NewGuid(), notificationLevel, text, source, dateTime, tag, machineName, metadata)
        { }

        /// <summary>
        ///     Gets the unique identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id { get; }

        /// <summary>
        ///     Gets the date time.
        /// </summary>
        /// <value>The date time.</value>
        public DateTime DateTime { get; }

        /// <summary>
        ///     Gets the notification level.
        /// </summary>
        /// <value>The notification level.</value>
        public NotificationLevel NotificationLevel { get; }

        /// <summary>
        ///     Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public string Source { get; }

        /// <summary>
        ///     Gets the tag.
        /// </summary>
        /// <value>The tag.</value>
        public string Tag { get; }

        /// <summary>
        ///     Gets the machine name.
        /// </summary>
        /// <value>The machine name.</value>
        public string MachineName { get; }

        /// <summary>
        ///     Gets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text { get; }

        /// <summary>
        ///     Gets the metadata.
        /// </summary>
        /// <value>The metadata.</value>
        public IDictionary<string, object> Metadata => new ReadOnlyDictionary<string, object>(_metadata);

        /// <summary>
        ///     Compares the current object with another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared.
        /// </returns>
        public int CompareTo(NotificationEntry other)
        {
            return DateTime.CompareTo(other.DateTime);
        }

        /// <summary>
        ///     Determines whether the Metadata property should be serialized
        /// </summary>
        public bool ShouldSerializeMetadata()
        {
            return Metadata.Count > 0;
        }
    }
}