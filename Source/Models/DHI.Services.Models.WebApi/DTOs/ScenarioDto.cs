namespace DHI.Services.Models.WebApi
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using TimeSeries;

    public class ScenarioDto
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ModelDataReaderId { get; set; }

        //[Required]
        public IDictionary<string, object> ParameterValues { get; set; }

        //[Required]
        public IDictionary<string, string> InputTimeSeriesValues { get; set; }

        public Scenario ToScenario()
        {
            return new Scenario(Id, Name, ModelDataReaderId, ParameterValues, InputTimeSeriesValues);
        }
    }
}