namespace DHI.Services.Test
{
    using System;

    [Serializable]
    public class FakeConnection : BaseConnection
    {
        public FakeConnection(string id, string name) : base(id, name)
        {
        }

        public override object Create()
        {
            return new FakeService();
        }
    }
}