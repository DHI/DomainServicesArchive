namespace DHI.Services.TimeSteps
{
    public class Item<TItemId> : BaseNamedEntity<TItemId>
    {
        public Item(TItemId id, string name) : base(id, name)
        {
        }
    }
}