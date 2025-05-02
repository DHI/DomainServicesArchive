namespace DHI.Services.Jobs.Workflows
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json.Serialization;

    [Serializable]
    [Obsolete("Use CodeWorkflow instead. This type will eventually be removed.")]
    public class Workflow : BaseNamedEntity<string>, ITask<string>
    {
        private readonly Dictionary<string, object> _parameters;

        [JsonConstructor]
        public Workflow(string id, string name, string definition)
            : base(id, name)
        {
            Definition = definition;
            _parameters = new Dictionary<string, object>();
        }

        public string Definition { get; set; }

        private bool SerializeDefinition { get; set; } = true;

        public TimeSpan? Timeout { get; set; }

        [JsonIgnore]
        public virtual IDictionary<string, object> Parameters => _parameters;

        public string HostGroup { get; set; }

        public void SetSerializeDefinition(bool value)
        {
            SerializeDefinition = value;
        }

        public void SaveAsXaml(string filePath)
        {
            if (Path.GetExtension(filePath) != ".xaml")
            {
                throw new ArgumentException(
                    $"Wrong extension for file '{Path.GetFileName(filePath)}'. Extension must be '.xaml'");
            }

            File.WriteAllText(filePath, Definition);
        }

        public bool ShouldSerializeParameters()
        {
            return _parameters.Count > 0;
        }

        public bool ShouldSerializeDefinition()
        {
            return SerializeDefinition;
        }
    }
}