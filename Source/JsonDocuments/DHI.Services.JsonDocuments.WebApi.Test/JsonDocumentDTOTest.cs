namespace DHI.Services.JsonDocuments.WebApi.Test
{
    using System;
    using System.Collections.Generic;
    using Xunit;

    public class JsonDocumentDTOTest
    {
        private const string Json = "{ \"string\": \"Hello World\" }";

        [Fact]
        public void JsonDocumentToDtoIsOk()
        {
            var fullName = new FullName("MyGroup", "MyDocument");
            var jsonDocument = new JsonDocument<string>(fullName.ToString(), fullName.Name, fullName.Group, Json)
            {
                Added = new DateTime(2021, 3, 10, 10, 11, 0),
                Updated = new DateTime(2021, 3, 10, 10, 12, 0),
            };

            jsonDocument.Metadata.Add("foo", "bar");

            var jsonDocumentDTO = jsonDocument.ToDTO();

            Assert.Equal(jsonDocument.FullName, jsonDocumentDTO.FullName);
            Assert.Equal(jsonDocument.Data, jsonDocumentDTO.Data);
            Assert.NotNull(jsonDocumentDTO.Added);
            Assert.Equal(jsonDocument.Added, jsonDocumentDTO.Added);
            Assert.NotNull(jsonDocumentDTO.Updated);
            Assert.Equal(jsonDocument.Updated, jsonDocumentDTO.Updated);
            Assert.True(jsonDocumentDTO.Metadata.ContainsKey("foo"));
            Assert.Equal("bar", jsonDocumentDTO.Metadata["foo"]);
        }

        [Fact]
        public void DtoToJsonDocumentIsOk()
        {
            var fullName = new FullName("MyGroup", "MyDocument");
            var jsonDocumentDTO = new JsonDocumentDTO
            {
                FullName = fullName.ToString(),
                Data = Json,
                Updated = new DateTime(2021, 3, 10, 10, 11, 0),
                Metadata = new Dictionary<object, object> { { "foo", "bar"} }
            };

            var jsonDocument = jsonDocumentDTO.ToJsonDocument();

            Assert.Equal(fullName.ToString(), jsonDocument.Id);
            Assert.Equal(fullName.Group, jsonDocument.Group);
            Assert.Equal(fullName.Name, jsonDocument.Name);
            Assert.Equal(jsonDocumentDTO.Data, jsonDocument.Data);
            Assert.NotNull(jsonDocument.Updated);
            Assert.Equal(jsonDocumentDTO.Updated, jsonDocument.Updated);
            Assert.Null(jsonDocument.Added);
            Assert.True(jsonDocument.Metadata.ContainsKey("foo"));
            Assert.Equal("bar", jsonDocument.Metadata["foo"]);
        }
    }
}