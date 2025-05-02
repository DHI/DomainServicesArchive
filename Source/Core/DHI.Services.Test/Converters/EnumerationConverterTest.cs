namespace DHI.Services.Test.Converters
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DHI.Services.Converters;
    using Xunit;

    public class EnumerationConverterTest
    {
        private readonly JsonSerializerOptions _options;

        public EnumerationConverterTest()
        {
            _options = new JsonSerializerOptions
            {
                Converters = { new EnumerationConverter() }
            };
        }

        [Fact]
        public void CanConvert()
        {
            Assert.True(new EnumerationConverter().CanConvert(typeof(Model.EnumerationSample)));
        }

        [Fact]
        public void EnumerationCanDeserialize()
        {
            var json = "{\"Type0\":{\"DisplayName\":\"Type 1\",\"Value\":1},\"Type1\":{\"DisplayName\":\"Type 2\",\"Value\":2},\"Type2\":{\"DisplayName\":\"Type 0\",\"Value\":0}}";

            var actual = JsonSerializer.Deserialize<Model>(json, _options);

            var expected = new Model
            {
                Type0 = Model.EnumerationSample.Type1,
                Type1 = Model.EnumerationSample.Type2,
                Type2 = Model.EnumerationSample.Type0
            };

            Assert.Equal(expected.Type0, actual.Type0);
            Assert.Equal(expected.Type1, actual.Type1);
            Assert.Equal(expected.Type2, actual.Type2);
        }

        [Fact]
        public void EnumerationCanDeserializeFromName()
        {
            var json = "{\"Type0\":\"Type 1\",\"Type1\":\"Type 2\",\"Type2\":\"Type 0\"}";

            var actual = JsonSerializer.Deserialize<Model>(json, _options);

            var expected = new Model
            {
                Type0 = Model.EnumerationSample.Type1,
                Type1 = Model.EnumerationSample.Type2,
                Type2 = Model.EnumerationSample.Type0
            };

            Assert.Equal(expected.Type0, actual.Type0);
            Assert.Equal(expected.Type1, actual.Type1);
            Assert.Equal(expected.Type2, actual.Type2);
        }

        [Fact]
        public void EnumerationCanSerializeAndDeserializedBack()
        {
            var model = new Model
            {
                Type0 = Model.EnumerationSample.Type1,
                Type1 = Model.EnumerationSample.Type2,
                Type2 = Model.EnumerationSample.Type0
            };

            var actual = JsonSerializer.Serialize(model, _options);

            var expected = "{\"Type0\":\"Type 1\",\"Type1\":\"Type 2\",\"Type2\":\"Type 0\"}";

            var deserializedBack = JsonSerializer.Deserialize<Model>(expected, _options);

            Assert.Equal(expected, actual);
            Assert.Equal(model.Type0, deserializedBack.Type0);
            Assert.Equal(model.Type1, deserializedBack.Type1);
            Assert.Equal(model.Type2, deserializedBack.Type2);
        }

        [Fact]
        public void EnumerationCanDeserializedFromValue()
        {
            var json = "{\"Type0\":1,\"Type1\":2,\"Type2\":0}";

            var actual = JsonSerializer.Deserialize<Model>(json, _options);

            var model = new Model
            {
                Type0 = Model.EnumerationSample.Type1,
                Type1 = Model.EnumerationSample.Type2,
                Type2 = Model.EnumerationSample.Type0
            };
        }

        [Serializable]
        private class Model
        {
            [JsonConstructor]
            public Model()
            {
                Type0 = EnumerationSample.Type0;
                Type1 = EnumerationSample.Type1;
                Type2 = EnumerationSample.Type2;
            }

            public EnumerationSample Type0 { get; set; }
            public EnumerationSample Type1 { get; set; }
            public EnumerationSample Type2 { get; set; }

            public class EnumerationSample : Enumeration
            {
                public static readonly EnumerationSample Type0 = new EnumerationSample(0, "Type 0");

                public static readonly EnumerationSample Type1 = new EnumerationSample(1, "Type 1");

                public static readonly EnumerationSample Type2 = new EnumerationSample(2, "Type 2");

                private EnumerationSample(int value, string displayName)
                    : base(value, displayName)
                {
                }
            }
        }
    }
}
