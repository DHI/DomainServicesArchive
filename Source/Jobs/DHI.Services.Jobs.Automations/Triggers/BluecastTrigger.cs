namespace DHI.Services.Jobs.Automations.Triggers
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Text.RegularExpressions;
    using System.Threading;
    using DHI.Services.Jobs.Automations.TriggerParametersExport;
    using Microsoft.Extensions.Logging;
    using MySqlConnector;

    [Serializable]
    public class BluecastTrigger : BaseTrigger, IBluecastTriggerParameters
    {
        private IDbConnection _connection;
        private ILogger Logger = null;

        public BluecastTrigger(string id, string description,
            string connectionString = "", string connectionStringFilename = "", DbmsType dbmsType = DbmsType.MySQL) : base(id, description)
        {
            ConnectionString = connectionString;
            ConnectionStringFilename = connectionStringFilename;
            DbmsType = dbmsType;
        }

        internal BluecastTrigger(IDbConnection connection, string id, string description) : base(id, description)
        {
            _connection = connection;
        }
        /// <summary>
        /// Type of database connection to use.
        /// </summary>
        public DbmsType DbmsType { get; set; } = DbmsType.MySQL;
        /// <summary>
        /// Connection string appropriate for the selected database.
        /// </summary>
        public string ConnectionString { get; set; } = "";
        /// <summary>
        /// As an alternative to ConnectionString, give a filename from which to read the ConnectionString.
        /// Note that this parameter is only used if ConnectionString is not set (is null or empty).
        /// </summary>
        public string ConnectionStringFilename { get; set; } = "";

        #region Class-scoped variable declarations

        // == Hardcodings block:

        // This is a list of the automation parameters (keys) that are absolutely necessary for the bluecast
        // trigger to "do it's thing". For these - by definition - we do not need default values, so a List is fine:
        private static readonly ImmutableList<string> _mandatoryParameters =
            ImmutableList.Create(
                "JobTable",
                "JobType",
                "BasetimeIntervalHours",
                "BlockOrder",
                "InitiateHours",
                "ExpiryHours"
                );
        // Optionals:
        //  For optional parameters, we need both a parameter name (key) and a value to use in case the
        //  parameter is not set - ie a dict.
        // For code simplicity, this is not declared Immutable, but it is meant to not be modified :)
        private static readonly Dictionary<string, string> _optionalParameters = new Dictionary<string, string>
    {
        { "MaxRunCount", "5" },          // Default max run each job 5 times
        { "RestartDelayMinutes", "5" },  // Delay 5 minutes after a failure before rerunning (the same job)
        { "HotWaitBlocks", "" },         // These blocks wait for same block previous basetime
        { "ChainWaitBlocks", "*" },      // These blocks wait for previous block same basetime. Default all, set to "" to disable
        { "PrerequisiteWaitJobs", "" },  // List of other jobs where same block same basetime must already be completed
        { "UseBulkCaching", "true" },    // Flag to change the way of caching: bulk: Single-call many rows, non-bulk: One call per block, less rows.
        { "TriggerNow", "false" }
    };

        // == It is convenient if we have direct access to some of the critical parameters and derived variables:

        // These two are going to be set once for the entire class: The name of the jobtype we are triggering and the table where it lives.
        private string _jobTable = string.Empty;
        private string _jobType = string.Empty;
        private int _basetimeIntervalHours = 6;
        private bool _triggerNow = false;

        // Also the block order is very often good to know. That also should be invariant.
        // This is just a parsed version of parameter "BlockOrder".
        private IEnumerable<string> _blocks = new List<string>();

        // This is a flag on whether to use legacy tables or not.
        // IFF JobTable == JobType, then we will assume that we use legacy tables.
        // This has an impact on the entire structure of DB queries.
        private bool _useLegacyTables = false;

        // If we want class-wide access to the connection, then we cannot use the readonly _connection defined above.
        //private IDbConnection Connection = null;
        private bool _usingLocalConnection = true; // Set to false if we use a alread-open passed-down connection

        // = More complicated derived variables:

        // Per-block minimum delay between a job failure and following rerun
        // ie parsed version of RestartDelayMinutes
        private Dictionary<string, int> _restartDelayMinutes = new Dictionary<string, int>();

        // Parsed version of MaxRunCount
        private Dictionary<string, int> _maxRunCount = new Dictionary<string, int>();

        // Parsed version of HotWaitBlocks:
        private IEnumerable<string> _hotWaitBlocks = new List<string>();

        // Parsed version of ChainWaitBlocks:
        private IEnumerable<string> _chainWaitBlocks = new List<string>();

        // Parsed version of PrerequisiteWaitJobs.
        // Each key pair (preqJobName, blockId) represents a prerequisite job for a particular
        // block of the present jobtype, and the dict value (double) represents a possible timeout
        // in hours relative to basetime. (Positive is *after* basetime, so sign is usually negative.)
        //private IEnumerable<string> _prerequisiteJobTypes = new List<string>();
        private Dictionary<(string preqJobName, string blockId), double?> _prerequisiteJobTypes = new Dictionary<(string preqJobName, string blockId), double?>();


        // A class-level dictionary to store job statuses, accessible to different methods in the class.
        // The key is a string generated by GetJobKey(JobType, BaseTime, BlockID).
        // The value is a JobStatus POCO object containing the current status of a job.
        // The idea here is to locally cache/echo present state of the fetched rows, so that
        // we can avoid querying for the same again - and can even query for multiple rows
        // in each call (and then store the results locally).
        private Dictionary<string, JobStatus> _jobsStatus = new Dictionary<string, JobStatus>();

        // A class-level dictionary with already-prepared sql query commands - ready for quick use.
        // New commands will be added only when needed.
        // TODO: Add a driver method, that will administrate this, preparing, storing and retrieving the commands.
        private Dictionary<string, IDbCommand> _iDbCommands = new Dictionary<string, IDbCommand>();
        // TODO: Maybe even create a method that can retrieve job info given (jobtype, basetime, blockid).
        // TODO: Consider if we want to make some pre-cache some basetimes/blocks "up front",
        //   for instance getting the latest 4 (or so)
        //   basetimes (all blocks) Maybe that is already too much info? Normally we need at leat the last two hot-stuff??
        //   For now no pre-caching.

        // This is for wallclock timing:
        private Stopwatch _stopwatchDatabase;
        private Stopwatch _stopwatchTotal;

        // This is just for logging (convenience variable)
        private string _executorName = "";
        #endregion

        /// <summary>
        /// Execute Bluecast trigger to check if a particular jobtype should run, and if so return the correct
        /// (highest prio) basetime and blockid that can be run.
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="parameters">Dictionary (string,string) with essential information needed to determine what to run (if anything):
        ///  DbTableName,
        ///  JobType,
        ///  BlockOrder,
        ///  BasetimeIntervalHours,
        /// Initially, JobTypeName will be the TABLE name, but we plan to move to a single table with jobtypename column later.
        /// The following really depend on the block to be executed, so we need to include some special thing for that.
        ///    We need a format, where we can have just one value - or separate a default and special for single or several blocks.
        ///         For ForecastBaseWorkflow we use a format like eg (this is for time slice, but it still work):
        ///          "H06=>0:6;B1=>6:48;B2=>48:96;B3=>96:144;B4=>144:192;B5=>192:240"
        ///         We can use this format, and have the no or empty key in "=>" mean "default", and then other blocks can be given explicitly.
        ///  InitiateHours
        ///  ExpiryHours
        ///  MaxRunCount
        ///  RestartDelayMinutes
        /// We need some way to relay job dependency - on:
        ///   1: Hotblock dependency (in principal, this can be a boolean - or more complicated/explicit)
        ///        Suggest: List of blocks, where this should be applied (or "*" for all blocks) [Should we accept eg "H*"?]
        ///        Example: "H06"
        ///        Empty should mean "no such requirement".
        ///   2: Same basetime - block-order chain (in principal, this can be a boolean - or more complicated/explicit)
        ///        Suggest: List of blocks, where this should be applied (or "*" for all blocks) [Should we accept eg "B*"?]
        ///        For the first block in BlockOrder it should be automatically skipped (as passed), as there is not prior.
        ///        Example: "*"
        ///        Empty should mean "no such requirement".
        ///   3: Other jobtype same basetime+block; Can be as simple as a list (semi-colon separated) of otherjobs.
        ///        TODO: What about when we move towards joining the table into one. Do we need both table and jobtypename at the same time?
        ///        Example: "Gefs504aget;Gefs504bget"
        ///        NOTE: At the moment, we should probably *not* consider jobs, where the blocks have different upstream prerequisites(?)
        ///        We could consider to separate the block-parts to have special requirements only for some blocks as eg
        ///         "H06:Gefs504aget;B*:Gefs504bget" ?? or "Gefs504aget[H06];Gefs504bget" ?? What is a good format?
        ///   4: Hardcoded alternatives with basetime-offset and explicit block??
        ///   ?
        /// 
        /// </param>
        /// <returns>AutomationResult</returns>
        public override AutomationResult Execute(ILogger logger, IDictionary<string, string> parameters = null)
        {

            // Start counting walltime
            _stopwatchTotal = Stopwatch.StartNew();
            _stopwatchDatabase = new Stopwatch();

            Logger = logger;

            // Shorthand for this automation - where we are - for logging:
            _executorName = $"{nameof(BluecastTrigger)}.{nameof(Execute)}";

            // Start making a sanity check on the parameters, as there may be some that we even want for the connection

            // Make sure that we have parameters dict at all.
            if (parameters == null)
            {
                Logger?.LogError($"{_executorName} called with parameters=null");
                return AutomationResult.NotMet();
            }

            // you can start using automation parameters passed into the code, by the key corresponding to the parameter name 
            // i.e. parameters["ParameterName"]
            Logger?.LogDebug("Executing BluecastTrigger with parameters=\"{Parameters}\"", JsonSerializer.Serialize(parameters));

            // Sanity checks on necessary parameters - and start conversion to stuff we can use in the trigger
            foreach (var parameterName in _mandatoryParameters)
            {
                if (!parameters.ContainsKey(parameterName))
                {
                    Logger?.LogError($"{_executorName}: Missing mandatory parameter '{parameterName}'. Cannot execute");
                    return AutomationResult.NotMet();
                }
            }
            // Get rid of all null values in the parameters - this is just to eliminate null-issues later on
            int nullParamCount = 0;
            foreach (var key in parameters.Keys.ToList()) // .ToList() to avoid modifying while iterating
            {
                if (parameters[key] == null)
                {
                    parameters[key] = string.Empty;
                    nullParamCount++;
                }
            }
            if (nullParamCount > 0)
            {
                Logger?.LogInformation($"Updated {nullParamCount} parameter values from null to empty string ('')");
            }

            // Set default values for optional parameters:
            foreach (var kvp in _optionalParameters)
            {
                if (!parameters.ContainsKey(kvp.Key))
                {
                    parameters[kvp.Key] = kvp.Value;
                }
            }

            try
            {
                // === Start by parsing the parameter info, which will be needed for querying the DB afterwards
                _jobTable = parameters["JobTable"];
                _jobType = parameters["JobType"];
                _basetimeIntervalHours = ParseBasetimeIntervalHours(parameters["BasetimeIntervalHours"]);
                _blocks = ParseDelimitedStringToList(parameters["BlockOrder"], failOnDuplicates: true, failOnEmpty: true);
                var initiateHours = BlockDictToFloatValues(ParseParameterToBlockDict(parameters["InitiateHours"], blocks: _blocks));
                var expiryHours = BlockDictToFloatValues(ParseParameterToBlockDict(parameters["ExpiryHours"], blocks: _blocks));
                _maxRunCount = BlockDictToIntValues(ParseParameterToBlockDict(parameters["MaxRunCount"], blocks: _blocks));
                _restartDelayMinutes = BlockDictToIntValues(ParseParameterToBlockDict(parameters["RestartDelayMinutes"], blocks: _blocks));
                _hotWaitBlocks = ParseParameterToBlockList(parameters["HotWaitBlocks"], blocks: _blocks);
                _chainWaitBlocks = ParseParameterToBlockList(parameters["ChainWaitBlocks"], blocks: _blocks);
                _prerequisiteJobTypes = ParsePrerequisiteWaitJobs(parameterValue: parameters["PrerequisiteWaitJobs"]);
                var useBulkCaching = ParseParameterBool(parameters["UseBulkCaching"]);
                _triggerNow = ParseParameterBool(parameters["TriggerNow"]);

                // === Now fledge out the information that we have from the various parameters:
                var blockBasetimes = GetBlockBasetimes(
                    blocks: _blocks, basetimeIntervalHours: _basetimeIntervalHours,
                    initiateHours: initiateHours, expiryHours: expiryHours
                    );
                // Sometimes narrow definitions just means that there presently are no valid basetimes (in range) to consider:
                if (blockBasetimes.Values.Sum(basetimes => basetimes.Count()) == 0)
                {
                    Logger?.LogInformation($"{_executorName}: No blocks have basetimes within [expiry:initiate] range. Nothing to execute at the moment.");
                    return AutomationResult.NotMet();
                }

                // Get range for basetime to be able to blanket-select from DB.
                var (minBasetime, maxBasetime) = GetBasetimeMinMax(blockBasetimes);
                if (minBasetime > maxBasetime)
                {
                    // This means that there are no basetimes set in any of the blocks, so the default/initialization
                    // values in GetBasetimeMinMax were not modified.
                    Logger?.LogInformation($"{_executorName}: No blocks have basetimes within [expiry:initiate] range. Nothing to execute at the moment.");
                    return AutomationResult.NotMet();
                }

                var jobs = PrioritizeJobs(blockBasetimes: blockBasetimes, blocks: _blocks, hotWaitBlocks: _hotWaitBlocks);

                // If the database table name *is* the jobtype name, then the format is by definition "legacy".
                if (_jobTable == _jobType)
                {
                    _useLegacyTables = true;
                }
                else
                {
                    _useLegacyTables = false;
                }

                // Ensure that we can get DB access:
                string dbConnectionString = GetConnectionString();
                // Establish (open) connection to DB.
                // Todo: maxRetries and delayBetweenRetries could be defaults to be adusted from parameters. If so they need better names.
                _connection = GetDbConnection(dbConnectionString: dbConnectionString, maxRetries: 1, delayBetweenRetries: 1000);


                PreCacheAllJobs(blockBasetimes, useBulkCaching: useBulkCaching);


                // Loop over each job in prioritized list. Grap the first that can run
                foreach (var (basetime, block) in jobs)
                {
                    if (JobCanRun(basetime, block))
                    {
                        //
                        // If condition met, return  AutomationResult.Met() or AutomationResult.Met(IDictionary<string><string> taskParameters_to_be_updated)
                        // where taskParameters_to_be_updated will be passed back to update any matching static task parameters defined,
                        // and they will be used to submit as the workflow parameters when submit a new job (workflow).

                        // set the taskParameters that you would want to update back before the submission of the new job for the workflow
                        var taskParameters = new Dictionary<string, string>();
                        // Need to convert basetime to basetime10!
                        // Format basetime as YYYYmmDDHH (BaseTime10)
                        taskParameters["BaseTime10"] = basetime.ToString("yyyyMMddHH");
                        taskParameters["BlockID"] = block;
                        return AutomationResult.Met(taskParameters);
                    }

                }

                // If we get here, then no job is eligible to run, so "condition not met".
                // return AutomationResult.NotMet();
                return AutomationResult.NotMet();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"{_executorName} got an exception: {ex.Message}");
                return AutomationResult.NotMet();
            }
            finally
            {
                // Veradej: Maybe we should keep an internal flag on whether we actually opened the connection
                //   in the first place(?) In principle, we *could* be passed an already-open connection, 
                //   and in that case we probably should not close it??
                if (_usingLocalConnection && _connection != null && _connection.State == System.Data.ConnectionState.Open)
                {
                    Logger?.LogInformation($"{_executorName}: Closing bluecast database connection");
                    _stopwatchDatabase.Start();
                    _connection.Close();
                    _stopwatchDatabase.Stop();
                }
                // Write wall clock timings for reference
                _stopwatchTotal.Stop();
                Logger.LogInformation($"{_executorName} ({_jobType}): Total wall time {_stopwatchTotal.ElapsedMilliseconds} ms; {_stopwatchDatabase.ElapsedMilliseconds} ms database interaction time");
            }
        }


        // SUBROUTINES

        public IEnumerable<string> ParseDelimitedStringToList(string parameterValue, string delimiter = ";", bool failOnDuplicates = false, bool failOnEmpty = false)
        {
            if (string.IsNullOrEmpty(delimiter))
            {
                throw new ArgumentException("Delimiter cannot be null or empty.");
            }

            var elements = parameterValue
                .Split(new[] { delimiter }, StringSplitOptions.None)
                .Where(element => !string.IsNullOrWhiteSpace(element))
                .ToList();

            if (failOnEmpty && elements.Count < 1)
            {
                throw new ArgumentException("No valid elements found in the input string");
            }

            if (failOnDuplicates && elements.Distinct().Count() != elements.Count)
            {
                throw new ArgumentException($"Duplicate elements found in the input string: '{parameterValue}'");
            }

            return elements;
        }


        public Dictionary<string, string> ParseParameterToBlockDict(string parameterValue, IEnumerable<string> blocks)
        {
            var explicitValues = new Dictionary<string, string>();
            string defaultValue = null;

            // Split by semicolon, trim whitespace, and process each element
            foreach (var listElement in parameterValue.Split(';').Select(e => e.Trim()).Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                if (listElement.Contains("=>"))
                {
                    var keyValue = listElement.Split(new[] { "=>" }, 2, StringSplitOptions.None)
                                              .Select(e => e.Trim()).ToArray(); // Trim around '=>'

                    if (string.IsNullOrWhiteSpace(keyValue[0]))
                    {
                        throw new ArgumentException($"{_executorName}: Missing blockname before '=>' in '{listElement}' (part of '{parameterValue}')");
                    }

                    explicitValues[keyValue[0]] = keyValue[1]; // Store key-value pair
                }
                else
                {
                    defaultValue = listElement; // Store as default value
                }
            }

            var blockParameters = new Dictionary<string, string>();

            // Loop over the Blocks class variable and populate BlockParameters
            foreach (var block in blocks)
            {
                if (explicitValues.ContainsKey(block))
                {
                    blockParameters[block] = explicitValues[block];
                }
                else
                {
                    blockParameters[block] = defaultValue; // Use default if key not found
                }
            }

            return blockParameters;
        }

        // Return an (ordered) list of existing blocks based on a semicolon-separated list in parameterValue.
        //  Each element can be a named block - or a glob matching zero or more.
        //  An entry of "*" means "all existing blocks".
        // NOTE: If an entry does not match an existing defined block (in blocks) we will still assume that it
        //     it is valid - just a block not administrated by the present trigger config.
        public IEnumerable<string> ParseParameterToBlockList(string parameterValue, IEnumerable<string> blocks)
        {
            // This will be the list we return for ordered matching blocks:
            var matchingBlockList = new List<string>();
            // This set is only for quick testing of duplicates, providing quicker lookup than from matchingBlockList
            var seenBlocks = new HashSet<string>();

            foreach (var listElement in parameterValue.Split(';').Select(e => e.Trim()).Where(e => !string.IsNullOrWhiteSpace(e)))
            {
                // Initialize matchingBlocks to an empty enumerable by default
                IEnumerable<string> matchingBlocks = Enumerable.Empty<string>();

                // = Handle the cases in quickest if-evaluations first.
                // We may often encounter "*" as it is default. It will evaluate to "all blocks", but we dont need a regex to tell us that:
                if (listElement == "*")
                {
                    matchingBlocks = blocks;
                }
                // Glob matching: if listElement contains "*" or "?", treat as glob pattern.
                //  This is the most expensive block, but the if-evaluation is still super-quick as listElement will be only a few chars.
                else if (listElement.Contains("*") || listElement.Contains("?"))
                {
                    var pattern = "^" + Regex.Escape(listElement).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                    var regex = new Regex(pattern);
                    matchingBlocks = blocks.Where(b => regex.IsMatch(b));
                }
                // Direct match: if listElement is found directly in blocks, then we need to add that.
                //  In this case we also dont need the regex.
                else
                {
                    if (!blocks.Contains(listElement))
                    {
                        Logger?.LogInformation($"Adding block '{listElement}' even though it is not on the global blocks list");
                    }
                    matchingBlocks = new List<string> { listElement };  // Direct match, no need for regex
                }

                // Add matched blocks to the list, being aware to skip duplicates already in the list
                foreach (var block in matchingBlocks)
                {
                    if (seenBlocks.Add(block))  // HashSet.Add() returns false if the block is already in the set
                    {
                        matchingBlockList.Add(block);  // Add block while preserving order
                    }
                }
            }

            return matchingBlockList;
        }


        // Take a convert dict <string,string> to <string,int>, complaining on errors.
        // This is convenient for block-dicts, where the values are really expected to be ints.
        public Dictionary<string, int> BlockDictToIntValues(Dictionary<string, string> stringDict)
        {
            return stringDict.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    if (!int.TryParse(kvp.Value, out int intValue))
                    {
                        throw new ArgumentException($"Value '{kvp.Value}' for key '{kvp.Key}' is not a valid integer.");
                    }
                    return intValue;
                }
            );
        }

        // Take a convert dict <string,string> to <string,float>, complaining on errors.
        // This is convenient for block-dicts, where the values are really expected to be floats.
        public Dictionary<string, float> BlockDictToFloatValues(Dictionary<string, string> stringDict)
        {
            return stringDict.ToDictionary(
                kvp => kvp.Key,
                kvp =>
                {
                    if (!float.TryParse(kvp.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatValue))
                    {
                        throw new ArgumentException($"Value '{kvp.Value}' for key '{kvp.Key}' is not a valid float.");
                    }
                    return floatValue;
                }
            );
        }

        // Convert parameter from TimeIntervalHours to int, and ensure that it
        // divides 24 (hours) nicely, so that "midnight is always a basetime".
        // Presently, basetimeIntervalHours should be in the set
        //  HashSet<int> allowedValues = new HashSet<int> { 1, 2, 3, 4, 6, 8, 12, 24 };
        public int ParseBasetimeIntervalHours(string baseTimeIntervalString)
        {
            if (!int.TryParse(baseTimeIntervalString, out int basetimeIntervalHours))
            {
                throw new ArgumentException($"Value '{baseTimeIntervalString}' is not a valid integer.");
            }
            // Check that intervalhours divides 24, so we have an integer intervals per day.
            // Negative values have to be excluded explicitly, but larger than 24 will be implicitly excluded in the %.
            if (basetimeIntervalHours <= 0 || 24 % basetimeIntervalHours != 0)
            {
                throw new ArgumentException("Invalid basetimeIntervalHours. Must be a positive divisor of 24.");
            }
            return basetimeIntervalHours;
        }

        public bool ParseParameterBool(string inputString)
        {
            if (!bool.TryParse(inputString, out bool parameter))
            {
                throw new ArgumentException($"Value '{inputString}' is not a valid boolean.");
            }
            return parameter;
        }

        /// <summary>
        /// Parses the PrerequisiteWaitJobs configuration string, returning a dictionary with job names and block IDs as keys,
        /// and optional timeout values (or null for no timeout) as values.
        /// </summary>
        /// <param name="parameterValue">
        /// A semicolon-separated string representing prerequisite job configurations. Each entry can include:
        /// <list type="bullet">
        /// <item><description>A job name for all blocks (e.g., "Gefs504aget")</description></item>
        /// <item><description>A job name with block-specific definitions inside brackets (e.g., "Gefs504aget[H06=>-6.0,B*=>-4.0]")</description></item>
        /// <item><description>Block specifications can use glob patterns like "*" or "B*".</description></item>
        /// <item><description>Timeouts are optional and can be specified as "blockspec=>timeout" (e.g., "H06=>-6.0").</description></item>
        /// </list>
        /// Examples:
        /// <example>
        /// "Gefs504aget[H06=>-6.0,B*=>-4.0];Gefs504bget"
        /// </example>
        /// </param>
        /// <returns>
        /// A dictionary where each key is a tuple of (prerequisiteJobName, blockId), and the value is a nullable double representing the timeout (null if no timeout is specified).
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if there is a malformed block specification, an invalid timeout format, or missing brackets.
        /// </exception>
        public Dictionary<(string preqJobName, string blockId), double?> ParsePrerequisiteWaitJobs(string parameterValue)
        {
            // Initialize the dictionary to hold prerequisite job names, block IDs, and optional timeouts
            var prerequisiteDict = new Dictionary<(string preqJobName, string blockId), double?>();

            // Split, trim, and filter valid elements
            var preqJobElements = parameterValue
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(element => element.Trim())  // Trim leading/trailing whitespace
                .Where(element => !string.IsNullOrWhiteSpace(element));

            // Loop through each element and handle parsing for the prerequisite job and block information (to be implemented)
            foreach (var preqJobElement in preqJobElements)
            {
                // Check if the element contains "[" - if not then we assume that it is just a plain name.
                var bracketIndex = preqJobElement.IndexOf('[');
                // If no "[", the entire element is the prerequisiteJobName
                string prerequisiteJobName = preqJobElement;

                if (bracketIndex == -1)
                {
                    // This prerequisite job is to be taken for: All blocks, no timeout, don't overwrite
                    foreach (var block in _blocks)
                    {
                        // TryAdd will only add if the (prerequisiteJobName, block) key doesn't already exist
                        if (!prerequisiteDict.ContainsKey((prerequisiteJobName, block)))
                        {
                            prerequisiteDict.Add((prerequisiteJobName, block), null);
                        }
                        // Newer versions of C# may use:
                        //prerequisiteDict.TryAdd((prerequisiteJobName, block), null);
                    }
                    // Skip to next definition element (presumably next prerequisite JobName
                    continue;
                }

                // If we get here, then we have an opening bracket.
                // Ensure that we have a closing bracket
                var closingBracketIndex = preqJobElement.IndexOf(']', bracketIndex);
                if (closingBracketIndex == -1)
                {
                    throw new ArgumentException($"Missing ']' after '[' in '{preqJobElement}'.");
                }
                // Ensure that closing bracket is at end-of-element.
                // This automatically ensures that it is also after the opening bracket.
                if (closingBracketIndex != preqJobElement.Length - 1)
                {
                    throw new ArgumentException($"Closing bracket is not at end of {prerequisiteJobName} element: '{preqJobElement}'.");
                }

                // If we split by the brackets we should get:
                //   0: The preqJobName
                //   1: block-modifier(s) from inside bracket
                //   2: empty string from after trailing bracket.
                var bracketSplit = preqJobElement.Split(new[] { '[', ']' }, StringSplitOptions.None);
                if (bracketSplit.Length != 3 || !string.IsNullOrEmpty(bracketSplit[2]))
                {
                    throw new ArgumentException($"Invalid bracket usage. Ensure only one pair of brackets in '{preqJobElement}'");
                }

                // Assign the prerequisiteJobName and blockInfo
                prerequisiteJobName = bracketSplit[0].Trim();
                var blockInfo = bracketSplit[1].Trim();  // This is the string inside the brackets

                // Validate blockInfo is not empty
                if (string.IsNullOrWhiteSpace(blockInfo))
                {
                    throw new ArgumentException($"No block information provided inside '[]' for '{preqJobElement}'. For all blocks use * or leave out bracket altogether.");
                }

                // Split blockInfo by comma to get each block-related definition
                var blockElements = blockInfo.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                // Loop through each blockElement (e.g., "H06=>-6.0", "B*=>-4.0", "-8.5", "H12" etc.)
                foreach (var blockElement in blockElements)
                {
                    // Initialize default values: blocks = _blocks (all blocks), timeout = null
                    var blocks = _blocks;  // Default to all blocks
                    double? timeout = null;  // Default to no timeout (null)

                    // Split blockElement by "=>"
                    var blockInfoParts = blockElement.Split(new[] { "=>" }, StringSplitOptions.None);

                    if (blockInfoParts.Length > 2)
                    {
                        throw new ArgumentException($"BlockElement specification splits into more than two parts: '{blockElement}' in '{preqJobElement}'.");
                    }

                    // Case when there's only one element in blockInfoParts
                    if (blockInfoParts.Length == 1)
                    {
                        // A: Try to cast the element to a float (interpreted as timeout)
                        if (double.TryParse(blockInfoParts[0], out var parsedTimeout))
                        {
                            timeout = parsedTimeout;  // Set the timeout if parsing is successful
                        }
                        else
                        {
                            // B: Otherwise, assume it is a block specification
                            blocks = ParseParameterToBlockList(blockInfoParts[0], _blocks);
                        }
                    }
                    else
                    {
                        // This is for Length==2. Zero is not possible, and we already threw out >2.
                        // Block part:
                        if (string.IsNullOrWhiteSpace(blockInfoParts[0]))
                        {
                            throw new ArgumentException($"Got empty/whitespace block-info (before =>): '{blockElement}' in '{preqJobElement}'.");
                        }
                        blocks = ParseParameterToBlockList(blockInfoParts[0], _blocks);
                        // Time part:
                        if (double.TryParse(blockInfoParts[1],
                            System.Globalization.NumberStyles.Float,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out var parsedTimeout))
                        {
                            timeout = parsedTimeout;  // Set the timeout if parsing is successful
                        }
                        else
                        {
                            throw new ArgumentException($"Illegal time-info (after =>). Expected float but got '{blockInfoParts[1]}' in '{blockElement}' in '{preqJobElement}'.");
                        }
                    }
                    // Deal with blocks and time here
                    if (timeout == null)
                    {
                        // Add each block with no timeout - but dont overwrite if already exists
                        foreach (var block in blocks)
                        {
                            // TryAdd will only add if the (prerequisiteJobName, block) key doesn't already exist
                            if (!prerequisiteDict.ContainsKey((prerequisiteJobName, block)))
                            {
                                prerequisiteDict.Add((prerequisiteJobName, block), null);
                            }
                            // Newer versions of C# may use:
                            //prerequisiteDict.TryAdd((prerequisiteJobName, block), null);
                        }
                    }
                    else
                    {
                        // Add each block with explicit timeout overwriting any possible (null) value:
                        foreach (var block in blocks)
                        {
                            prerequisiteDict[(prerequisiteJobName, block)] = timeout;
                        }

                    }
                }

            }

            // Return the populated dictionary
            return prerequisiteDict;
        }


        // For each block type/name, find the range of basetimes that we should consider for running.
        public Dictionary<string, HashSet<DateTime>> GetBlockBasetimes(
            IEnumerable<string> blocks,                     // IEnumerable of blocks
            int basetimeIntervalHours,                      // Basetime interval in hours
            Dictionary<string, float> initiateHours,        // Initiate hours (end interval) for each block
            Dictionary<string, float> expiryHours           // Expiry hours (start interval) for each block
        )
        {
            // 1. Declare a dictionary where block is the key and room for an IEnumerable<DateTime>
            var blockBasetimes = new Dictionary<string, HashSet<DateTime>>();

            // 2. Iterate over each block and call GetBasetimes, storing results in the dict
            foreach (var block in blocks)
            {
                if (initiateHours.ContainsKey(block) && expiryHours.ContainsKey(block))
                {
                    // Call GetBasetimes with values specific to this block
                    var basetimes = GetBasetimes(
                        basetimeIntervalHours,
                        initiateHours[block],
                        expiryHours[block]
                    );

                    // Store the IEnumerable<DateTime> in the dictionary with block as the key
                    blockBasetimes[block] = basetimes;
                }
                else
                {
                    throw new ArgumentException($"Missing initiate or expiry hours for block '{block}'");
                }
            }

            // Return the dictionary
            return blockBasetimes;
        }

        // Given interval between basetimes as well as initiate and expiry hours (relative to *now*)
        // conjure a set of all modulo-interval basetimes between now+expiryhours and now+initiatehours
        // Note that the present method has the order and complexity to return an ordered list, but
        // later in the process we will take advantage of the quick search capabilities of the hashset.
        public HashSet<DateTime> GetBasetimes(int basetimeIntervalHours, float initiateHours, float expiryHours)
        {
            // Ensure we are using UTC for the all wall-clock timings and possible basetimes
            DateTime now = DateTime.UtcNow;
            // We will return a decenting-sorted list of all bastimes between start and end (inclusive)
            DateTime start = now.AddHours(expiryHours);
            DateTime end = now.AddHours(initiateHours);

            // First code block is about finding the last possible basetime smaller than or equal to end.
            // As we know midnight is in general a valid basetime, we start with that, and then work
            // ahead as long as we stay <= end.
            DateTime firstBasetime = end.Date;

            // The following loop really should compare the *next* possible firstBasetime
            // (ie .AddHours(basetimeIntervalHours)) to end. But to avoid redundant additions, we
            // precompute an adjusted end:
            DateTime adjustedEnd = end.AddHours(-basetimeIntervalHours);

            // Now we cam find the largest basetime <= 'end'.
            //   We compare 'firstBasetime' with 'adjustedEnd' (which is end - basetimeIntervalHours)
            //   to avoid recalculating the interval during each iteration.
            while (firstBasetime <= adjustedEnd)
            {
                firstBasetime = firstBasetime.AddHours(basetimeIntervalHours); // Increment firstBasetime by the interval
            }

            // Create a list to hold the results
            var basetimes = new HashSet<DateTime>();

            // Iterate backwards, adding valid basetimes to the list
            while (firstBasetime >= start)
            {
                basetimes.Add(firstBasetime);
                firstBasetime = firstBasetime.AddHours(-basetimeIntervalHours); // Decrement by the interval
            }
            // Note that if there are no valid basetimes in range, then the return list (ienum)
            // may be empty, which is in general not an error.
            return basetimes; // Returning as IEnumerable<DateTime> for flexibility
        }


        // Find basetime range (min,max) across all block basetime lists.
        // If there are no valid basetimes, then we will return min>max.
        public (DateTime minBasetime, DateTime maxBasetime) GetBasetimeMinMax(Dictionary<string, HashSet<DateTime>> blockBasetimes)
        {
            // Initialize min and max with well-known values
            DateTime minBasetime = DateTime.MaxValue;
            DateTime maxBasetime = DateTime.MinValue;

            try
            {
                // Find the maximum from the first elements, skipping blocks with no valid basetimes
                maxBasetime = blockBasetimes
                    .Where(kvp => kvp.Value.Any()) // Skip blocks with no valid basetimes
                    .Select(kvp => kvp.Value.First())
                    .DefaultIfEmpty(DateTime.MinValue) // If no valid blocks, use default MinValue
                    .Max();

                // Find the minimum from the last elements, skipping blocks with no valid basetimes
                minBasetime = blockBasetimes
                    .Where(kvp => kvp.Value.Any()) // Skip blocks with no valid basetimes
                    .Select(kvp => kvp.Value.Last())
                    .DefaultIfEmpty(DateTime.MaxValue) // If no valid blocks, use default MaxValue
                    .Min();
            }
            catch (InvalidOperationException)
            {
                // Log that no valid basetimes were found
                Logger?.LogInformation($"{_executorName}: No valid basetimes found across all blocks");
                // Since no valid basetimes were found, min > max will be returned, indicating no basetimes
            }
            catch (Exception)
            {
                // Handle any unforeseen errors, if necessary
                Logger?.LogError($"{_executorName}: An unexpected error occurred while calculating basetime range");
                throw;
            }
            // Return both min and max as a tuple. The caller can check if min > max to detect no valid basetimes.
            return (minBasetime, maxBasetime);
        }

        // Prioritize "jobs" (basetime+block):
        //   hotWaitBlocks before any other blocks
        //   later basetimes (forecast) before any other basetimes (backfill)
        public IEnumerable<(DateTime basetime, string block)> PrioritizeJobs(
            Dictionary<string, HashSet<DateTime>> blockBasetimes,
            IEnumerable<string> blocks,
            IEnumerable<string> hotWaitBlocks)
        {
            // Sanity check: Validate that all blocks exist in blockBasetimes
            if (blocks.Any(block => !blockBasetimes.ContainsKey(block)))
            {
                throw new ArgumentException("Some blocks not found in blockBasetimes");
            }

            // Step 1: Use LINQ to create prioBlocks and nonprioBlocks as IEnumerables
            var prioBlocks = blocks.Where(block => hotWaitBlocks.Contains(block));
            var nonprioBlocks = blocks.Where(block => !hotWaitBlocks.Contains(block));

            // Step 2: Compile sorted descending list of all unique basetimes, so we can loop over them.
            //   Sort decendingly, so we can take latest basetimes first, and then add backfill later.
            var uniqueBasetimes = blockBasetimes.Values
                .SelectMany(basetimes => basetimes)
                .Distinct()
                .OrderByDescending(basetime => basetime);

            // Step 3a: Create the prioritized list of basetime-block pairs
            var prioritizedList = new List<(DateTime, string)>();

            // Step 3b: Add prioritized blocks to the list first.
            //  For hotstart-type blocks we want to do backfill first to move the hotstarts along.
            //  Normally, this reversing will not matter, as only a single job will likely be elegible to run, 
            //  but for forced running (TriggerNow) it can come in handy in deciding the "top priority non-completed job".
            foreach (var basetime in uniqueBasetimes.Reverse())
            {
                foreach (var block in prioBlocks)
                {
                    if (blockBasetimes[block].Contains(basetime))
                    {
                        prioritizedList.Add((basetime, block));
                    }
                }
            }

            // Step 3c: Add all remaining (non-prioritized, ie typically "plain" forecast) blocks.
            //  Here we will take "latest first" and postpone any backfill jobs.
            foreach (var basetime in uniqueBasetimes)
            {
                foreach (var block in nonprioBlocks)
                {
                    if (blockBasetimes[block].Contains(basetime))
                    {
                        prioritizedList.Add((basetime, block));
                    }
                }
            }

            return prioritizedList;
        }


        /// <summary>
        /// JobStatus POCO.
        /// Represents the status of a job, including essential fields like JobType, BaseTime, and BlockID.
        /// This class is designed to store/echo job status information from the database to decrease the
        /// number of request to the database.
        /// </summary>
        public class JobStatus
        {
            /// <summary>
            /// The type of the job. This field is required and helps distinguish between different job types.
            /// </summary>
            public string JobType { get; set; }  // Required, no default

            /// <summary>
            /// The base time associated with the job. This field is required as part of the unique job identification.
            /// </summary>
            public DateTime BaseTime { get; set; }  // Required, no default

            /// <summary>
            /// The block ID associated with the job. This field is required and is part of the unique job key.
            /// </summary>
            public string BlockID { get; set; }  // Required, no default

            /// <summary>
            /// The number of times the job has been run. Defaults to 0 for new jobs.
            /// </summary>
            public int RunCount { get; set; } = 0;  // Defaults to 0

            /// <summary>
            /// The timestamp when the job started running. This field is nullable and defaults to null.
            /// </summary>
            public DateTime? StartRun { get; set; } = null;  // Nullable, defaults to null

            /// <summary>
            /// The timestamp when the job finished running. This field is nullable and defaults to null.
            /// </summary>
            public DateTime? EndRun { get; set; } = null;  // Nullable, defaults to null

            /// <summary>
            /// The error code for the job's execution. Null means no error or the job hasn't finished.
            /// </summary>
            public int? ErrorCode { get; set; } = null;  // Nullable, defaults to null

            /// <summary>
            /// The timestamp of the last modification to the job's database entry. 
            /// Initialized to some date a long time ago to avoid confusion with actual job data.
            /// </summary>
            public DateTime LastModified { get; set; } = DateTime.MinValue;

            /// <summary>
            /// Constructor to initialize a job status object with the required fields.
            /// </summary>
            /// <param name="jobType">The type of the job.</param>
            /// <param name="baseTime">The base time associated with the job.</param>
            /// <param name="blockID">The block ID associated with the job.</param>
            public JobStatus(string jobType, DateTime baseTime, string blockID)
            {
                JobType = jobType;
                BaseTime = baseTime;
                BlockID = blockID;
            }
        }

        // Figure out if a job is elegible to run.
        // The jobtype is asumed here, as we will *only* deal with _jobType jobs.
        public bool JobCanRun(DateTime basetime, string block)
        {
            // Step 1: Check if the job is already completed - if so then just ignore without a comment/log.
            if (JobIsCompleted(_jobType, basetime, block))
            {
                return false;
            }

            // Forced run. This just says YES if the job is not already completed.
            if (_triggerNow)
            {
                Logger?.LogInformation($"Job {FormatJobInfo(_jobType, basetime, block)} is FORCED to run");
                return true;
            }

            // Prelude. Get me a local copy of the jobstatus poco for finegrained work.
            var jobStatus = GetJobStatus(_jobType, basetime, block);

            // Unified one-off loop. This is solely to have unified handling of errors etc at end.
            // Whenever we hit a "cannot run", we will set cantRunBecause and break out.
            string cantRunBecause = "";
            for (int i = 0; i < 1; i++)
            {
                // Step 2: Check if the job is currently running
                if (jobStatus.StartRun.HasValue && (!jobStatus.EndRun.HasValue || !jobStatus.ErrorCode.HasValue))
                {
                    cantRunBecause = $"Job started at {jobStatus.StartRun:HH:mm} UTC and is still running";
                    break;
                }

                // Step 3: Implement "cheap" eligibility checks
                if (jobStatus.RunCount >= _maxRunCount[block])
                {
                    cantRunBecause = $"MaxRunCount reached. Logged RunCount={jobStatus.RunCount}, MaxRunCount[{block}]={_maxRunCount[block]}";
                    break;
                }

                // Step 4: Check for job failure and cooldown period
                // Job is failed if: ErrorCode is set and non-zero, StartRun is set, EndRun is set
                if (jobStatus.ErrorCode.HasValue && jobStatus.ErrorCode != 0
                    && jobStatus.StartRun.HasValue && jobStatus.EndRun.HasValue)
                {
                    // Calculate the cooldown period: EndRun + _restartDelayMinutes[block]
                    var cooldownEnd = jobStatus.EndRun.Value.AddMinutes(_restartDelayMinutes[block]);

                    // If current time (UTC) is less than cooldownEnd, job is in cooldown
                    if (DateTime.UtcNow <= cooldownEnd)
                    {
                        cantRunBecause = $"Waiting for cooldown after failure until {cooldownEnd:HH:mm}";
                        break;
                    }
                }

                // Step 5: Check if there's a prerequisite job that needs to be completed first
                if (_chainWaitBlocks.Contains(block))
                {
                    // Find the index of the current block in _blocks
                    var blockIndex = _blocks.ToList().IndexOf(block);

                    // Check if this is not the first block in the chain
                    if (blockIndex > 0)
                    {
                        // Get the previous block (prerequisite block)
                        var prereqBlock = _blocks.ElementAt(blockIndex - 1);

                        // Check if the prerequisite job (same jobType, basetime, prereqBlock) is completed
                        if (!JobIsCompleted(_jobType, basetime, prereqBlock))
                        {
                            cantRunBecause = $"Waiting for {FormatJobInfo(_jobType, basetime, prereqBlock)}";
                            break;
                        }
                    }
                }

                // Step 6: Check for prerequisite job from previous basetime (applies to _hotWaitBlocks)
                if (_hotWaitBlocks.Contains(block))
                {
                    // Calculate the previous basetime
                    var hotBasetime = basetime.AddHours(-_basetimeIntervalHours);

                    // Check if the job for the previous basetime and same block is completed
                    if (!JobIsCompleted(_jobType, hotBasetime, block))
                    {
                        cantRunBecause = $"Waiting for {FormatJobInfo(_jobType, hotBasetime, block)}";
                        break;
                    }
                }

                // Step 7: Check prerequisite jobs of other job types
                //  7a: Get an iterable of (prereqJob,timeout) for this block:
                var matchingPrerequisites = _prerequisiteJobTypes
                    .Where(kv => kv.Key.blockId == block)
                    .Select(kv => (kv.Key.preqJobName, kv.Value));

                foreach (var (preqJobName, timeout) in matchingPrerequisites)
                {
                    string timeoutMsg = "";
                    if (timeout.HasValue)
                    {
                        var timeoutAt = basetime.AddHours(-timeout.Value);
                        string timeoutAtAsString = timeoutAt.ToString("yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture) + " UTC";
                        if (DateTime.UtcNow > timeoutAt)
                        {
                            // This prerequisite is no longer relevant as it timed out. We probably should write that
                            Logger?.LogInformation($"{preqJobName} requirement for {block} timed out at {timeoutAt}");
                            // Skip to next prerequisite matching block
                            continue;
                        }
                        // If we end up waiting for this job, then we will append a possible timeout
                        timeoutMsg = $" (will time out at {timeoutAtAsString})";
                    }
                    //
                    // If we get here, then we need to check if the prerequisite is completed OK,
                    // ie. it really is (still) a prerequisite.
                    // Here we will check if the actual prerequisiste job has finished. (next prompt)
                    if (!JobIsCompleted(preqJobName, basetime, block))
                    {
                        cantRunBecause = $"Waiting for {FormatJobInfo(preqJobName, basetime, block)}{timeoutMsg}";
                        // Note that this will only break us from matchingPrerequisites, so we need a secondary break after that loop.
                        break;
                    }
                }
                // We need a sec
                if (!string.IsNullOrEmpty(cantRunBecause))
                {
                    // Secondary break to get us out of one-off loop
                    break;
                }


            }
            if (!string.IsNullOrEmpty(cantRunBecause))
            {
                Logger?.LogInformation($"Cannot run job {FormatJobInfo(_jobType, basetime, block)}: {cantRunBecause}");
                return false;
            }

            // If no condition prevents it, return true (job is eligible to run)
            Logger?.LogInformation($"Job {FormatJobInfo(_jobType, basetime, block)} is eligible to run");
            return true;
        }

        // Just a helper to get a common format for writing a job instance info.
        private string FormatJobInfo(string jobType, DateTime basetime, string block)
        {
            // Format basetime as YYYYmmDDHH
            string formattedBasetime = basetime.ToString("yyyyMMddHH");

            // Return the formatted string with jobType, basetime, and block
            return $"[{jobType},{formattedBasetime},{block}]";
        }


        // Definition for if a job is already completed:
        public bool JobIsCompleted(string jobType, DateTime basetime, string block)
        {
            // Step 1: Retrieve the JobStatus for the specified job
            var jobStatus = GetJobStatus(jobType, basetime, block);

            // Step 2: Check if the job is completed
            // - The job is completed if:
            //    * ErrorCode is 0 (and not null)
            //    * StartRun is not null
            //    * EndRun is not null
            if (jobStatus.ErrorCode.HasValue && jobStatus.ErrorCode == 0
                && jobStatus.StartRun.HasValue
                && jobStatus.EndRun.HasValue)
            {
                return true;  // Job is completed
            }

            return false;  // Job is not completed
        }


        /// <summary>
        /// Generates a unique key for a job based on its JobType, BaseTime, and BlockID.
        /// The key is a string in the format "JobType_BaseTime_BlockID", where BaseTime is 
        /// formatted as "yyyyMMddHHmmss" to the second for consistency.
        /// Two calls with identical parameters will generate the same key.
        /// </summary>
        /// <param name="jobType">The type of the job, typically a string up to 24 characters.</param>
        /// <param name="baseTime">The base time associated with the job, formatted as "yyyyMMddHHmmss".</param>
        /// <param name="blockID">The block identifier, typically around 3 characters long.</param>
        /// <returns>A unique string key combining JobType, BaseTime, and BlockID.</returns>
        public static string GetJobKey(string jobType, DateTime baseTime, string blockID)
        {
            // Format: "JobType_BaseTime_BlockID"
            // BaseTime is formatted down to the second for consistency and uniqueness.
            return $"{jobType}_{baseTime:yyyyMMddHHmmss}_{blockID}";
        }



        public void PreCacheAllJobs(Dictionary<string, HashSet<DateTime>> blockBasetimes, bool useBulkCaching)
        {
            if (useBulkCaching)
            {
                // Use the bulk caching method (PreCacheBasetimeRangeAllBlocks)
                Logger.LogDebug("Using bulk caching.");

                // Call PreCacheBasetimeRangeAllBlocks for all blocks
                int nJobsCached = PreCacheBasetimeRangeAllBlocks(_jobType, blockBasetimes.Values.SelectMany(x => x).Min(), blockBasetimes.Values.SelectMany(x => x).Max());

                // Log the total number of jobs cached for bulk caching
                Logger.LogInformation($"Bulk caching for {_jobType} completed. {nJobsCached} jobs were cached.");
            }
            else
            {
                // Use individual block caching (PreCacheBasetimeRangeSingleBlock)
                Logger.LogDebug($"Applying per-block caching for {_jobType}.");

                // Loop through the blocks in blockBasetimes
                int nJobsCachedTotal = 0;
                foreach (var block in _blocks)
                {
                    if (blockBasetimes.ContainsKey(block))
                    {
                        var basetimeSet = blockBasetimes[block];
                        var minBaseTime = basetimeSet.Min();
                        var maxBaseTime = basetimeSet.Max();

                        // Call PreCacheBasetimeRangeSingleBlock for this block and log the number of jobs cached
                        int nJobsCached = PreCacheBasetimeRangeSingleBlock(_jobType, block, minBaseTime, maxBaseTime);
                        nJobsCachedTotal += nJobsCached;
                        // Log the number of jobs cached for the current block
                        Logger.LogInformation($"Cached {nJobsCached} jobs for block '{block}'.");
                    }
                }
                Logger.LogInformation($"Per-block caching completed. {nJobsCachedTotal} jobs were cached.");
            }

            // Final loop to ensure all jobs are represented, even if not in the database
            int nJobsDefaulted = 0;
            foreach (var block in _blocks)
            {
                if (blockBasetimes.ContainsKey(block))
                {
                    foreach (var basetime in blockBasetimes[block])
                    {
                        // Generate the job key for the current block and basetime
                        string jobKey = GetJobKey(_jobType, basetime, block);

                        // If the job is not already cached, insert a new/clean JobStatus
                        if (!_jobsStatus.ContainsKey(jobKey))
                        {
                            _jobsStatus[jobKey] = new JobStatus(_jobType, basetime, block);
                            nJobsDefaulted++;
                        }
                    }
                }
            }
            Logger.LogInformation($"Defaulting rows (nonexecuted jobs) for {nJobsDefaulted} jobs.");
        }


        #region == ACTUAL database interaction code ==




        public JobStatus GetJobStatus(string jobType, DateTime baseTime, string blockID)
        {
            // Generate the job key using the provided parameters
            string jobKey = GetJobKey(jobType, baseTime, blockID);

            // Check if the job status is already cached
            if (_jobsStatus.TryGetValue(jobKey, out var cachedJobStatus))
            {
                //Logger.LogDebug($"{nameof(GetJobStatus)}: Returning cached job status for {jobType} at {baseTime} and block {blockID}.");
                return cachedJobStatus;
            }

            // If we get here, then we did not cache this job so far, so let us do it now.
            // For efficiency, we will ask for all blocks (SingleBasetime) - knowing that some of the blocks may not have run yet.
            Logger.LogInformation($"Caching job status found for {jobType} at {baseTime} (all blocks).");

            // To leverage some caching, we will extract all rows for the candidate basetime at the same time.
            // This means using the SingleBasetime query type:
            // Generate the query key for SingleBasetime type
            var queryKey = GetQueryName(queryType: QueryType.SingleBasetime, jobType: jobType, legacy: _useLegacyTables);

            // Get the prepared command for SingleBasetime query
            var command = GetQuery(queryType: QueryType.SingleBasetime, jobType: jobType, legacy: _useLegacyTables);

            // Set parameters in command.
            //  If not legacy, set the JobType parameter first. If legacy, then the jobtype is hardbaked into the command (table name).
            if (!_useLegacyTables)
            {
                //var jobTypeParam = (IDbDataParameter)command.Parameters["@JobType"];
                //jobTypeParam.Value = jobType;
                ((IDbDataParameter)command.Parameters["@JobType"]).Value = jobType;
            }

            // Set the BaseTime parameter for the query
            //var baseTimeParam = (IDbDataParameter)command.Parameters["@BaseTime"];
            //baseTimeParam.Value = baseTime;
            ((IDbDataParameter)command.Parameters["@BaseTime"]).Value = baseTime;

            // Execute command and parse the results, storing in the JobsStatus dict:
            int nRows = ExecuteJobQuery(command: command, jobType: jobType);

            if (nRows == 0)
            {
                Logger.LogDebug($"No job status found for {jobType} at {baseTime} and block {blockID}.");
            }
            else
            {
                Logger.LogInformation($"Basetime-caching {FormatJobInfo(jobType, basetime: baseTime, block: "*")} completed. {nRows} jobs were cached.");
            }
            // We now cached all basetime-blocks (for this jobtype) in the db. If some blocks are missing,
            // then it means that the row is missing, ie that presumably the job has not run yet.
            // For those jobs we will add clean JobStatus objects to avoid further queries and streamline processing in this end
            // Thus, we will make sure that we have initialized all the possible blocks here, and then
            // we can overwrite with DB info later (if it is avaiable).
            // Note that one of these will be the original block from the argument list:
            int nJobsDefaulted = 0;
            foreach (var block in _blocks)
            {
                // Generate the job key for the current block
                string blockJobKey = GetJobKey(jobType, baseTime, block);
                if (!_jobsStatus.ContainsKey(blockJobKey))
                {
                    //Logger.LogDebug($"{nameof(GetJobStatus)}: Defaulting job status row for {jobType} at {baseTime} and block {block}.");
                    nJobsDefaulted++;
                    _jobsStatus[blockJobKey] = new JobStatus(jobType, baseTime, block);
                }
            }
            if (nJobsDefaulted > 0)
            {
                Logger.LogInformation($"Defaulting rows (nonexecuted jobs) for {nJobsDefaulted} jobs.");
            }

            // Now we really should have the job cached. If it is not in the DB, then we will have a default JobStatus object:
            // Check if the job status is already cached
            if (_jobsStatus.TryGetValue(jobKey, out var newJobStatus))
            {
                return newJobStatus;
            }
            // If we get here, then something really failed!
            throw new Exception($"{nameof(GetJobStatus)}: Somehow we failed to get a cached version of the data for jobtype='{jobType}' basetime='{baseTime}' and block='{blockID}'");
        }

        // This method is meant to read/retrieve all jobs for a single blockid and a basetime range.
        // When used, it must be called once per blockid - using the basetime range that goes with that block.
        // Thus, we will get exactly the right data (not too many), but we have to execute O(6) different calls
        // to get the data.
        public int PreCacheBasetimeRangeSingleBlock(string jobType, string blockID, DateTime basetimeMin, DateTime basetimeMax)
        {
            // Retrieve the prepared command for BasetimeRangeSingleBlock
            var command = GetQuery(queryType: QueryType.SingleBlockBasetimeRange, jobType: jobType, legacy: _useLegacyTables);

            // If non-legacy, then we need to set jobtype as a query parameter
            if (!_useLegacyTables)
            {
                ((IDbDataParameter)command.Parameters["@JobType"]).Value = jobType;
            }

            // Then set the remaing parameters for SingleBlockBasetimeRange query:
            ((IDbDataParameter)command.Parameters["@BlockID"]).Value = blockID;
            ((IDbDataParameter)command.Parameters["@BaseTimeMin"]).Value = basetimeMin;
            ((IDbDataParameter)command.Parameters["@BaseTimeMax"]).Value = basetimeMax;

            // Execute the query and store the number of cached jobs (rows)
            int nJobsCached = ExecuteJobQuery(command, jobType);

            // Log a debug message if no jobs were cached
            if (nJobsCached == 0)
            {
                Logger.LogDebug($"No job status found for jobType: {jobType}, block: {blockID}, basetimeMin: {basetimeMin}, basetimeMax: {basetimeMax}.");
            }

            return nJobsCached;
        }

        // This method is meant to read/retrieve all jobs for a basetime range (all block ids).
        // If we use it on the entire range of interesting basetimes (all bclocks), then we will retrieve
        // more data than necessary (estimated x3), but we can do it in a single call.
        // Simple estimations suggest that this may be about 2x faster than using PreCacheBasetimeRangeSingleBlock.
        public int PreCacheBasetimeRangeAllBlocks(string jobType, DateTime basetimeMin, DateTime basetimeMax)
        {
            // Retrieve the prepared command for BasetimeRange (for all blocks)
            var command = GetQuery(queryType: QueryType.BasetimeRange, jobType: jobType, legacy: _useLegacyTables);

            // If non-legacy, set the JobType parameter first
            if (!_useLegacyTables)
            {
                ((IDbDataParameter)command.Parameters["@JobType"]).Value = jobType;
            }

            // Set the parameters for the basetime range
            ((IDbDataParameter)command.Parameters["@BaseTimeMin"]).Value = basetimeMin;
            ((IDbDataParameter)command.Parameters["@BaseTimeMax"]).Value = basetimeMax;

            // Note: In this approach, we are fetching job statuses for all blocks in a single query.
            // The results should contain job statuses for all blocks (hotsim and others) in the given basetime range.
            // We rely on ExecuteJobQuery to process and cache the results.

            // Execute the query and return the number of cached jobs
            int nJobsCached = ExecuteJobQuery(command, jobType);

            // Log if no jobs were cached
            if (nJobsCached == 0)
            {
                Logger.LogDebug($"No job status found for jobType: {jobType}, basetimeMin: {basetimeMin}, basetimeMax: {basetimeMax} across all blocks.");
            }

            return nJobsCached;
        }


        // Helper method to actually execute query command and parse and store the results.
        public int ExecuteJobQuery(IDbCommand command, string jobType)
        {
            // Start counting time for DB interaction
            _stopwatchDatabase.Start();
            int nRows = 0;
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    // 0: Increment row counter
                    nRows++;

                    // A: Store read BaseTime and BlockID into temporary variables
                    var rowBaseTime = reader.GetDateTime(reader.GetOrdinal("BaseTime"));
                    var rowBlockID = reader.GetString(reader.GetOrdinal("BlockID"));

                    //Logger.LogDebug($"{nameof(ExecuteJobQuery)}: Retrieved job status for {jobType} at {rowBaseTime} and block {rowBlockID}.");

                    // B: Instantiate a clean JobStatus object for each row
                    var rowJobStatus = new JobStatus(jobType, rowBaseTime, rowBlockID);

                    // C: Update the remaining fields of jobStatus
                    rowJobStatus.RunCount = reader.GetInt32(reader.GetOrdinal("RunCount"));
                    rowJobStatus.StartRun = reader.IsDBNull(reader.GetOrdinal("StartRun")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("StartRun"));
                    rowJobStatus.EndRun = reader.IsDBNull(reader.GetOrdinal("EndRun")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("EndRun"));
                    rowJobStatus.ErrorCode = reader.IsDBNull(reader.GetOrdinal("ErrorCode")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("ErrorCode"));
                    rowJobStatus.LastModified = reader.GetDateTime(reader.GetOrdinal("LastModified"));

                    // D: Get a clean job status key using GetJobKey (note: block may vary)
                    var rowJobKey = GetJobKey(jobType, rowBaseTime, rowBlockID);

                    // E: Store in JobsStatus dictionary
                    _jobsStatus[rowJobKey] = rowJobStatus;
                }
            }
            _stopwatchDatabase.Stop();
            return nRows;  // Return the number of rows processed
        }


        public string GetConnectionString()
        {
            string dbConnectionString = ConnectionString;
            if (string.IsNullOrEmpty(dbConnectionString) && !String.IsNullOrEmpty(ConnectionStringFilename))
            {
                Logger?.LogDebug("Reading DB connection string from file: {FileName}", ConnectionStringFilename);
                if (!File.Exists(ConnectionStringFilename))
                {
                    throw new ArgumentException($"{_executorName}: No such file: {nameof(ConnectionStringFilename)}='{ConnectionStringFilename}'");
                }
                try
                {
                    // Assume that the first line contains any connection string. Chomp/trim newlines and carriage returns.
                    dbConnectionString = File.ReadLines(ConnectionStringFilename).First().TrimEnd('\r', '\n');
                    if (string.IsNullOrWhiteSpace(dbConnectionString))
                    {
                        throw new ArgumentException($"{_executorName}: First line of file='{ConnectionStringFilename}' contains only whitespace. No connectionstring");
                    }
                }
                catch (Exception ex)
                {
                    Logger?.LogError("Failed to read DB connection string file, {FileName}: {exception}", ConnectionStringFilename, ex.Message);
                    throw;
                }
            }
            if (string.IsNullOrEmpty(dbConnectionString))
            {
                throw new ArgumentException($"{_executorName}: Failed to get a connection string");
            }
            return dbConnectionString;
        }

        public IDbConnection GetDbConnection(string dbConnectionString = "", int maxRetries = 1, int delayBetweenRetries = 1000)
        {

            IDbConnection connection = _connection;
            // Note that if we are handed the connection from outside, then percievably it could already be opened.
            // Quick return if we have been handed an already-open connection:
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                // Connection is already open, so presumably we were handed _connection as already open.
                //logger.LogInformation("Database connection already open");
                _usingLocalConnection = false;
                return connection;
            }
            // Sanity check. dbConnectionString is not really needed if we somehow are passed an open connection.
            if (string.IsNullOrEmpty(dbConnectionString))
            {
                throw new ArgumentException("Cannot open connection with empty connection string");
            }
            if (maxRetries <= 0)
            {
                maxRetries = 1;
            }

            // If we have not been handed any connection, then we need to set it up
            if (connection == null)
            {
                connection = DbmsType switch
                {
                    DbmsType.MySQL => new MySqlConnection(dbConnectionString),
                    DbmsType.Undefined or _ => throw new ArgumentOutOfRangeException(nameof(DbmsType), DbmsType, $"DbmsType not supported, currently only {nameof(DbmsType.MySQL)} is supported")
                };
            }
            // If we get here, then we have a connection, but it cannot have been opened yet, so we need to do that.

            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    // try to open connection
                    _stopwatchDatabase.Start();
                    connection.Open();
                    _stopwatchDatabase.Stop();
                    Logger?.LogInformation("Connected to the database");

                    // if successful, break out of the loop and continue
                    break;
                }
                catch (MySqlException)
                {
                    //        logger.LogInformation($"Attempt {attempt} failed: {ex.Message}");

                    // If it's the last attempt fail to connection , exit with condition not met
                    if (attempt == maxRetries)
                    {
                        Logger?.LogError("Max retries reached. Unable to connect to the database.");
                        throw;
                        //return AutomationResult.NotMet();
                    }

                    // Wait for a specified amount of time before retrying.
                    if (delayBetweenRetries > 0)
                    {
                        Logger?.LogWarning($"Failed to connect to the database. Will retry after {delayBetweenRetries}ms sleep");
                        Thread.Sleep(delayBetweenRetries);
                    }
                    else
                    {
                        Logger?.LogWarning($"Failed to connect to the database. Will retry.");
                    }
                }
            }
            return connection;
        }

        // Prepare statements to query database to get faster query times.
        // NOTE: For legacy table layout, the jobtype *is* the table name, and that has to 
        //   be explicitly baked into the query - ie cannot be added as parameter to command later.
        //   Thus, we will (for now) just code the jobtype as table.
        // This means that we will prepare separate commands for all the jobtypes in play for the present job.
        // The jobtype itself - and all on dependency job list.

        // We will allow/support preparation of the following query types.
        // Each type will be supported for legacy (one table per jobtype) and new (non-legacy) single table, but with a JobType column.
        public enum QueryType
        {
            SingleJob,                 // Select a unique job based on the primary keys.
            SingleBasetime,            // Select all jobs for specific jobtype but a single basetime
            BasetimeRange,             // Select all jobs for specific jobtype within a basetime range (min to max)
            SingleBlockBasetimeRange   // As BasetimeRange, but select just a single block.
        }

        // This method will generate a unique name (key) for a query (command), so that it can be stored and later referenced.
        // If we store the prepared commands in a dict, then we can check the dict for a prepared command when we need one,
        // and just prepare a new command "just in time". We dont have to prepare all possible commands from the getgo.
        // For legacy mode, we will need to bake the jobtype (tablename) into the prepared query, thus the
        // name will be unique for each jobtype.
        public string GetQueryName(QueryType queryType, string jobType, bool legacy)
        {
            string queryName = queryType.ToString();

            if (legacy)
            {
                queryName = $"{jobType}_{queryName}";
            }

            return queryName;
        }

        // Driver method, which handles prepared queries. If a query is already prepared, then you get it right away,
        // otherwise we prepare one, store it and then return the newly prepared query.
        public IDbCommand GetQuery(QueryType queryType, string jobType, bool legacy)
        {
            // Generate a unique key for the query based on the query type, job type, and legacy flag
            string queryKey = GetQueryName(queryType, jobType, legacy);

            // Check if the command is already prepared and stored in the dictionary
            if (_iDbCommands.ContainsKey(queryKey))
            {
                // Return the cached command
                return _iDbCommands[queryKey];
            }

            // If the command doesn't exist, prepare a new one
            var command = PrepareQuery(queryType, jobType, legacy);

            // Store the prepared command in the dictionary for future use
            _iDbCommands[queryKey] = command;

            // Return the newly prepared command
            return command;
        }


        // This prepares a query selecting bluecast columns.
        // "legacy" makes the assumption that jobtype info is actually the table name.
        // This significantly changes the command, as we must bake in the jobtype in the table and not use it as a parameter to add.
        public IDbCommand PrepareQuery(QueryType queryType, string jobTable, bool legacy = false)
        {
            // This is what fields we select
            string select = "SELECT BaseTime, BlockID, RunCount, StartRun, EndRun, ErrorCode, LastModified";
            // and where we select from:
            string from = $"FROM {jobTable}";
            // For legacy, jobType *is* the table, and we do not query for JobType.
            string jobtype = "";
            // but for non-legacy it is a field. We will still always select by jobtype.
            // Note that the command will then need an extra parameter.
            if (!legacy)
            {
                jobtype = "JobType = @JobType AND";
            }

            // Select/switch how the query text should be for this type of query:
            string querytext;
            switch (queryType)
            {
                case QueryType.SingleJob:
                    querytext = $"{select} {from} WHERE {jobtype} BaseTime = @BaseTime AND BlockID = @BlockID";
                    break;

                case QueryType.SingleBasetime:
                    querytext = $"{select} {from} WHERE {jobtype} BaseTime = @BaseTime";
                    break;

                case QueryType.BasetimeRange:
                    querytext = $"{select} {from} WHERE {jobtype} BaseTime >= @BaseTimeMin AND BaseTime <= @BaseTimeMax";
                    break;

                case QueryType.SingleBlockBasetimeRange:
                    querytext = $"{select} {from} WHERE {jobtype} BaseTime >= @BaseTimeMin AND BaseTime <= @BaseTimeMax AND BlockID = @BlockID";
                    break;

                default:
                    throw new ArgumentException("Invalid QueryType");
            }

            Logger.LogDebug($"Preparing query '{querytext}'");
            var command = _connection.CreateCommand();
            command.CommandText = querytext;

            // Now add the correct parameters to the command.
            if (!legacy)
            {
                // For non-legacy, we will always select by jobtype. This defines the extra parameter:
                command.Parameters.Add(new MySqlParameter("@JobType", MySqlDbType.String));
            }
            switch (queryType)
            {
                case QueryType.SingleJob:
                    command.Parameters.Add(new MySqlParameter("@BaseTime", MySqlDbType.DateTime));
                    command.Parameters.Add(new MySqlParameter("@BlockID", MySqlDbType.String));
                    break;

                case QueryType.SingleBasetime:
                    command.Parameters.Add(new MySqlParameter("@BaseTime", MySqlDbType.DateTime));
                    break;

                case QueryType.BasetimeRange:
                    command.Parameters.Add(new MySqlParameter("@BaseTimeMin", MySqlDbType.DateTime));
                    command.Parameters.Add(new MySqlParameter("@BaseTimeMax", MySqlDbType.DateTime));
                    break;

                case QueryType.SingleBlockBasetimeRange:
                    command.Parameters.Add(new MySqlParameter("@BaseTimeMin", MySqlDbType.DateTime));
                    command.Parameters.Add(new MySqlParameter("@BaseTimeMax", MySqlDbType.DateTime));
                    command.Parameters.Add(new MySqlParameter("@BlockID", MySqlDbType.String));
                    break;

                default:
                    throw new ArgumentException("Invalid QueryType");
            }
            command.Prepare();
            return command;
        }



        #endregion
    }
}
