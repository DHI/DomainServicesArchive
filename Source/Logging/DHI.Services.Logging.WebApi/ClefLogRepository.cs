namespace DHI.Services.Logging.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
	using System.Text.Json;
	using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Core;
    using Serilog.Formatting.Compact;
    using Serilog.Formatting.Compact.Reader;

    public class ClefLogRepository : IDisposable
    {
        private readonly string _logDirectory;
        private readonly MemoryCache _loggerCache = new MemoryCache(new MemoryCacheOptions());
        private readonly TimeSpan _loggerExpiration = TimeSpan.FromMinutes(5);
        private readonly HashSet<string> _tags = new HashSet<string>();
        private readonly Func<string, Stream> _streamReader;
        private readonly bool _isFileBased;

        public ClefLogRepository(string logDirectory)
        {
            _logDirectory = logDirectory;
            _streamReader = (file) => new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			_isFileBased = true;
		}

        public ClefLogRepository(string logDirectory, Func<string, Stream> streamReader)
		{
			_logDirectory = logDirectory;
			_streamReader = streamReader;
            _isFileBased = false;
		}

		public void Add(LogEntryDTO logEntry)
        {
            var logger = _loggerCache.GetOrCreate(logEntry.Tag, entry =>
            {
                entry.SlidingExpiration = _loggerExpiration;
                entry.RegisterPostEvictionCallback((key, value, reason, state) =>
                {
                    (value as Logger)?.Dispose();
                    _tags.Remove(key.ToString());
                });

                _tags.Add(logEntry.Tag);
                return CreateLoggerForTag(logEntry.Tag);
            });

            var l = logger
                .ForContext("Source", logEntry.Source)
                .ForContext("LogTime", logEntry.DateTime);

            l.Write(LogLevelHelper.Parse(logEntry.LogLevel), logEntry.Text);
        }

        public IEnumerable<LogEntryDTO> Get(IEnumerable<QueryCondition> query)
        {
            var queryCondition = query.FirstOrDefault(q => q.Item == "Tag");
            if (queryCondition == null)
            {
                yield break;
            }

            // We only support Tag and Equal
            if (queryCondition.QueryOperator != QueryOperator.Equal)
            {
                throw new Exception("Only Tag and Equal are allowed");
            }

            var file = Path.Combine(_logDirectory, $"{queryCondition.Value}.log");

            if (!File.Exists(file) && _isFileBased)
            {
                yield break;
            }

            using var fs = _streamReader(file);
            using var sr = new StreamReader(fs);
            using var reader = new LogEventReader(sr);
            while (reader.TryRead(out var evt))
            {
                if (!evt.Properties.TryGetValue("LogTime", out var evnt) ||
					(!DateTime.TryParse(evnt.ToString(), out var dateTime) && !DateTime.TryParse(JsonSerializer.Deserialize<string>(evnt.ToString()), out dateTime)))
                {
                    dateTime = evt.Timestamp.DateTime;
                }

                var source = evt.Properties.TryGetValue("Source", out var s) ? s.ToString() : string.Empty;
                var logLevel = (LogLevel)evt.Level;
                var text = evt.RenderMessage();

                yield return new LogEntryDTO(Guid.NewGuid(), logLevel, dateTime, text, source, queryCondition.Value.ToString(), null, null);
            }
        }

        public Maybe<LogEntryDTO> Last(IEnumerable<QueryCondition> query)
        {
            var last = Get(query).LastOrDefault();
            if (last == default)
            {
                return Maybe.Empty<LogEntryDTO>();
            }

            return last.ToMaybe();
        }

        private Logger CreateLoggerForTag(string tag)
        {
            return new LoggerConfiguration()
                .WriteTo.File(new CompactJsonFormatter(), Path.Combine(_logDirectory, $"{tag}.log"), shared: true, flushToDiskInterval: TimeSpan.FromSeconds(3))
                .CreateLogger();
        }

        public void Dispose()
        {
            try
            {
                foreach (var tag in _tags)
                {
                    if (_loggerCache.TryGetValue(tag, out var logger))
                    {
                        (logger as Logger)?.Dispose();
                    }
                }

                _loggerCache.Clear();
                _loggerCache.Dispose();
            }
            catch
            {
                //do nothing
            }
        }
    }
}