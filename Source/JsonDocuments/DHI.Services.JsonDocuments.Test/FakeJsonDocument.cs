namespace DHI.Services.JsonDocuments.Test
{
    using System;

    [Serializable]
    public class FakeJsonDocument : JsonDocument
    {
        public FakeJsonDocument(string id, string name, string group)
            : base(id, name, group, "{ \"string\": \"Hello World\" }")
        {
        }
    }
}