namespace DHI.Services.Logging.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Extensions.Logging;
    using Serilog.Events;

    /// <summary>
    ///     Data transfer object for a LogEntry representation.
    /// </summary>
    public class LogEntryDTO
    {
        public LogEntryDTO(Guid id, LogLevel logLevel, DateTime dateTime, string text, string source, string tag, string machineName, Dictionary<string, object> metadata)
        {
            Id = id.ToString();
            LogLevel = logLevel.ToString();
            DateTime = dateTime;
            Text = text;
            Source = source;
            Tag = tag;
            Metadata = metadata;
            MachineName = machineName ?? Environment.GetEnvironmentVariable("COMPUTERNAME");
        }
        
        public LogEntryDTO(Guid id, LogLevel logLevel, string text, string source, string tag, string machineName, Dictionary<string, object> metadata)
        {
            Id = id.ToString();
            LogLevel = logLevel.ToString();
            DateTime = DateTime.UtcNow;
            Text = text;
            Source = source;
            Tag = tag;
            Metadata = metadata;
            MachineName = machineName ?? Environment.GetEnvironmentVariable("COMPUTERNAME");
        }


        public LogEntryDTO()
        {
        }

        public string Id { get; set; }

        public DateTime DateTime { get; set; }
        /// <summary>
        ///     Gets or sets the LogLevel.
        /// </summary>
        [Required]
        public string LogLevel { get; set; }

        /// <summary>
        ///     Gets or sets the text.
        /// </summary>
        [Required]
        public string Text { get; set; }

        /// <summary>
        ///     Gets or sets the source.
        /// </summary>
        [Required]
        public string Source { get; set; }

        /// <summary>
        ///     Gets or sets the tag.
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        ///     Gets or sets the machine name.
        /// </summary>
        public string MachineName { get; set; }

        /// <summary>
        ///     Gets or sets the metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; }
    }

    public static class LogLevelHelper
    {
        public static LogEventLevel Parse(string logLevel)
        {
            var msLogLevel = (LogLevel)Enum.Parse(typeof(LogLevel), logLevel);
            switch (msLogLevel)
            {
                case LogLevel.Trace:
                    return LogEventLevel.Verbose;
                case LogLevel.Debug:
                    return LogEventLevel.Debug;
                case LogLevel.Information:
                    return LogEventLevel.Information;
                case LogLevel.Warning:
                    return LogEventLevel.Warning;
                case LogLevel.Error:
                    return LogEventLevel.Error;
                case LogLevel.Critical:
                    return LogEventLevel.Fatal;
                case LogLevel.None:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}