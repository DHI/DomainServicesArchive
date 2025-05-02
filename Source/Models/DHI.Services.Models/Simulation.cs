namespace DHI.Services.Models
{
    using System;
    using System.Text.Json.Serialization;

    /// <summary>
    ///     Class Simulation.
    /// </summary>
    [Serializable]
    public class Simulation : BaseEntity<Guid>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Simulation" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="scenarioId">The scenario identifier.</param>
        [JsonConstructor]
        public Simulation(Guid id, string scenarioId) : base(id)
        {
            ScenarioId = scenarioId;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Simulation" /> class.
        /// </summary>
        /// <param name="scenarioId">The scenario identifier.</param>
        public Simulation(string scenarioId) : this(Guid.NewGuid(), scenarioId)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Simulation" /> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="name">The simulation name.</param>
        /// <param name="scenarioId">The scenario identifier.</param>
        public Simulation(Guid id, string name, string scenarioId) : base(id)
        {
            ScenarioId = scenarioId;
            Name = name;
        }

        /// <summary>
        ///     The simulation name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     The simulation short name.
        /// </summary>
        public string ShortName { get; set; }

        /// <summary>
        ///     The model setup name.
        /// </summary>
        public string ModelSetupName { get; set; }

        /// <summary>
        ///     Gets the scenario identifier.
        /// </summary>
        public string ScenarioId { get; }

        /// <summary>
        ///     Gets or sets the status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        ///     The simulation group.
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        ///     Gets or sets the requested time.
        /// </summary>
        public DateTime Requested { get; set; }

        /// <summary>
        ///     Gets or sets the started time.
        /// </summary>
        public DateTime? Start { get; set; }

        /// <summary>
        ///     Gets or sets the finished time.
        /// </summary>
        public DateTime? End { get; set; }

        /// <summary>
        ///     Execution time of the simulaiton.
        /// </summary>
        public DateTime? ExecutionTime { get; set; }

        /// <summary>
        ///     Gets or sets the progress.
        /// </summary>
        public int? Progress { get; set; }

        /// <summary>
        ///     Gets or sets the time interval for the simulation .
        /// </summary>
        /// <value>The simulation range.</value>
        public DateRange? SimulationRange { get; set; }

        /// <summary>
        ///     Gets or sets the time of forecast.
        /// </summary>
        public DateTime? TimeOfForecast { get; set; }

        public string User { get; set; }
    }
}