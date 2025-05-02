namespace DHI.Services.Jobs.Automations.Expressions
{
    using System.Collections.Generic;

    public class And : List<ILogical>, ILogical
    {
        public bool Evaluate()
        {
            foreach (var item in this)
            {
                var thisResult = item.Evaluate();

                if (!thisResult)
                {
                    return false;
                }
            }

            return true;
        }
    }
}