namespace DHI.Services.Jobs.Automations.Expressions
{
    using System.Collections.Generic;

    public class Or : List<ILogical>, ILogical
    {
        public bool Evaluate()
        {
            foreach (var item in this)
            {
                var thisResult = item.Evaluate();

                if (thisResult)
                {
                    return true;
                }
            }

            return false;
        }
    }
}