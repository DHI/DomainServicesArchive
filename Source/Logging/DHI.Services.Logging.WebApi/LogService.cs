namespace DHI.Services.Logging.WebApi
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class LogService : IDisposable
    {
        private readonly ClefLogRepository _logRepository;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LogService" /> class.
        /// </summary>
        /// <param name="logRepository">The log repository.</param>
        /// <exception cref="ArgumentNullException">logRepository</exception>
        public LogService(ClefLogRepository logRepository)
        {
            _logRepository = logRepository ?? throw new ArgumentNullException(nameof(logRepository));
        }

        /// <summary>
        ///     Gets an array of log entries fulfilling the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>LogEntry[].</returns>
        public LogEntryDTO[] Get(IEnumerable<QueryCondition> query)
        {
			return _logRepository.Get(query).OrderBy(l => l.DateTime).ToArray();
		}

        /// <summary>
        ///     Gets the last log entry that fulfills the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>LogEntry.</returns>
        public LogEntryDTO Last(IEnumerable<QueryCondition> query)
        {
            return _logRepository.Last(query) | default(LogEntryDTO);
        }

        public void Add(LogEntryDTO logEntryDto)
        {
            _logRepository.Add(logEntryDto);
        }

        public void Dispose()
        {
            _logRepository.Dispose();
        }
    }
}