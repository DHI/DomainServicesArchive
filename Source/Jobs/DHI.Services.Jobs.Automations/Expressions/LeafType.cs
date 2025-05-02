namespace DHI.Services.Jobs.Automations.Expressions
{
    public class LeafType : ILogical
    {
        public string ItemKey { get; set; }
        public bool EvaluatedValue { get; set; }
        public bool Evaluate()
        {
            return EvaluatedValue;
        }
    }
}
