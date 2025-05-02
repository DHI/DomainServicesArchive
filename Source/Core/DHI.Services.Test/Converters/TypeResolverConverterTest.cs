namespace DHI.Services.Test.Converters
{
    using System.Text.Json;
    using DHI.Services.Converters;
    using Xunit;

    public class TypeResolverConverterTest
    {
        private readonly JsonSerializerOptions _options;

        public TypeResolverConverterTest()
        {
            _options = new JsonSerializerOptions
            {
                Converters =
                {
                    new TypeResolverConverter<ISample>(),
                }
            };
        }

        [Fact]
        public void CanSerializeToInterface()
        {
            var test1 = new SampleTest_1
            {
                Name = "Foo",
                Value = 0.5
            };

            var actual = JsonSerializer.Serialize(test1, _options);

            Assert.Contains("$type", actual);
            Assert.Contains($"{typeof(SampleTest_1).Namespace}.{typeof(SampleTest_1).Name}", actual);
        }

        [Fact]
        public void DeserializeMatchWithTypeDiscriminator()
        {
            var json = "{\"$type\":\"DHI.Services.Test.Converters.SampleTest_1, DHI.Services.Test.Converters\",\"Name\":\"Bar\",\"Value\":0.25}";

            var actual = JsonSerializer.Deserialize<SampleTest_1>(json, _options);

            var expected = new SampleTest_1
            {
                Name = "Bar",
                Value = 0.25
            };

            Assert.IsType<SampleTest_1>(expected);
            Assert.Equal(actual.Name, expected.Name);
            Assert.Equal(actual.Value, expected.Value);
        }

        [Fact]
        public void ThrowWhenNotMatchWithTypeDiscriminator()
        {
            var json = "{\"$type\":\"DHI.Services.Test.Converters.SampleTest_1, DHI.Services.Test.Converters\",\"Name\":\"Bar\",\"Value\":0.25}";

            var serializer = new JsonSerializerOptions
            {
                Converters = { new TypeResolverConverter<ISample>() }
            };

            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<SampleTest_2>(json, serializer));
            Assert.Contains("This type cannot be found or not matched with type", ex.Message);
        }


        private interface ISample
        {
            public string Name { get; set; }
            public double Value { get; set; }
        }

        private class SampleTest_1 : ISample
        {
            public string Name { get; set; }
            public double Value { get; set; }
        }
        private class SampleTest_2 : ISample
        {
            public string Name { get; set; }
            public double Value { get; set; }
        }
    }
}
