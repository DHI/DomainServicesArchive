namespace DHI.Services.JsonDocuments.Test
{
    using Argon;
    using System;
    using System.Text.Json;
    using Xunit;

    public class JsonDocumentTest
    {
        [Theory]
        [InlineData("InvalidJson")]
        [InlineData("{ \"string\"= \"Hello World\" }")]
        [InlineData("{ \"string\": \"Hello World\", \"}")]
        public void CreateWithInvalidJsonThrows(string json)
        {
            Assert.Throws<ArgumentException>(() => new JsonDocuments.JsonDocument("myDocument", "My Document", json));
        }

        [Fact]
        public void CreateIsOk()
        {
            var doc = new JsonDocuments.JsonDocument("myDocument", "My Document", "{ \"string\": \"Hello World\" }");
            var o = (JObject)JsonConvert.DeserializeObject<object>(doc.Data);
            Assert.Equal("Hello World", (string)o.SelectToken("string"));
        }

        [Fact]
        public void CloneIsOk()
        {
            var doc = new JsonDocuments.JsonDocument(
                Guid.NewGuid().ToString(), 
                "MyDocument", "MyGroup", 
                JsonConvert.SerializeObject(new { foo = 1, forecastTime = DateTime.UtcNow, shared = true }))
            {
                Added = DateTime.Now
            };
            doc.Metadata.Add("foo", "bar");
            doc.AddPermissions(new[] { "Administrators" }, new[] { "read", "update", "delete" });
            doc.AddPermissions(new[] { "Editors" }, new[] { "read", "update" });
            doc.AddPermissions(new[] { "Users" }, new[] { "read" });
            var docClone = doc.Clone<JsonDocuments.JsonDocument>();
            Assert.Equal(doc.Data, docClone.Data);
            Assert.Equivalent(doc.Metadata, docClone.Metadata, strict: true);
            Assert.Equivalent(docClone.Permissions, docClone.Permissions, strict: true);
        }
    }
}