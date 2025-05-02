namespace DHI.Services.Test.Converters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using DHI.Services.Converters;
    using Xunit;
    using static DHI.Services.Test.Converters.DictionaryConverterFixture;

    public class DictionaryConverterTest : IClassFixture<DictionaryConverterFixture>
    {
        private readonly JsonSerializerOptions _options;
        private readonly DictionaryConverterFixture _fixture;

        public DictionaryConverterTest(DictionaryConverterFixture fixture)
        {
            _fixture = fixture;
            _options = fixture.SerializerOptions;
        }

        [Fact]
        public void CanConvert()
        {
            Assert.True(new DictionaryTypeResolverConverter<string, SampleClass>(isNestedDictionary: true).CanConvert(typeof(IDictionary<string, IDictionary<string, SampleClass>>)));
            Assert.True(new DictionaryTypeResolverConverter<int, SampleClass>(isNestedDictionary: true).CanConvert(typeof(IDictionary<string, IDictionary<int, SampleClass>>)));
        }

        [Fact]
        public void CanSerializedNestedDictionaryWithTypeDiscriminator()
        {
            var actual = JsonSerializer.Serialize(_fixture.SampleData, _options);

            var expected = "{\"$type\":\"System.Collections.Generic.Dictionary\\u00602[[System.String, mscorlib],[System.Collections.Generic.IDictionary\\u00602[[System.String, mscorlib],[DHI.Services.Test.Converters.SampleClass, DHI.Services.Test.Converters.SampleClass]], mscorlib]], mscorlib\",\"Root-1\":{\"$type\":\"System.Collections.Generic.Dictionary\\u00602[[System.String, mscorlib],[DHI.Services.Test.Converters.SampleClass, DHI.Services.Test.Converters.SampleClass]], mscorlib\",\"Root1-Key-1\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key1-Name-1\",\"Value\":10},\"Root1-Key-2\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key2-Name-2\",\"Value\":20}},\"Root-2\":{\"$type\":\"System.Collections.Generic.Dictionary\\u00602[[System.String, mscorlib],[DHI.Services.Test.Converters.SampleClass, DHI.Services.Test.Converters.SampleClass]], mscorlib\",\"Root2-Key-1\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root2-Key1-Name-1\",\"Value\":100},\"Root2-Key-2\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root2-Key2-Name-2\",\"Value\":200}}}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanDeserializedNestedDictionaryWithTypeDiscriminator()
        {
            var json = "{\"$type\":\"System.Collections.Generic.Dictionary\\u00602[[System.String, mscorlib],[System.Collections.Generic.IDictionary\\u00602[[System.String, mscorlib],[DHI.Services.Test.Converters.SampleClass, DHI.Services.Test.Converters.SampleClass]], mscorlib]], mscorlib\",\"Root-1\":{\"$type\":\"System.Collections.Generic.Dictionary\\u00602[[System.String, mscorlib],[DHI.Services.Test.Converters.SampleClass, DHI.Services.Test.Converters.SampleClass]], mscorlib\",\"Root1-Key-1\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key1-Name-1\",\"Value\":10},\"Root1-Key-2\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key2-Name-2\",\"Value\":20}},\"Root-2\":{\"$type\":\"System.Collections.Generic.Dictionary\\u00602[[System.String, mscorlib],[DHI.Services.Test.Converters.SampleClass, DHI.Services.Test.Converters.SampleClass]], mscorlib\",\"Root2-Key-1\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root2-Key1-Name-1\",\"Value\":100},\"Root2-Key-2\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root2-Key2-Name-2\",\"Value\":200}}}";

            var actual = JsonSerializer.Deserialize<IDictionary<string, IDictionary<string, SampleClass>>>(json, _options);
            var expected = _fixture.SampleData;

            Assert.Equal(expected.Count, actual.Count);
            Assert.All(actual.Keys, actualKey => Assert.Contains(actualKey, expected.Keys));
            Assert.All(actual.Values.SelectMany(x => x.Values), x => Assert.Contains(x, expected.Values.SelectMany(expect => expect.Values)));
        }


        [Fact(Skip = "Only 1 level of nested dictionary")]
        public void CanSerializedDepthNestedDictionary()
        {
            var dictionary = new Dictionary<string, IDictionary<int, IDictionary<string, SampleClass>>>();
            dictionary.Add("Outer-1",
                new Dictionary<int, IDictionary<string, SampleClass>>
                {
                    {  10, new Dictionary<string, SampleClass>
                        {
                            { "Root1-Key-10",  new SampleClass{ Name = "Root1-Key10-Name-1", Value = 10 } }
                        }
                    },
                     {  20, new Dictionary<string, SampleClass>
                        {
                            { "Root1-Key-20",  new SampleClass{ Name = "Root1-Key20-Name-21", Value = 20 } }
                        }
                    },
                });
            dictionary.Add("Outer-2",
              new Dictionary<int, IDictionary<string, SampleClass>>
              {
                    {  100, new Dictionary<string, SampleClass>
                        {
                            { "Root2-Key-100",  new SampleClass{ Name = "Root1-Key100-Name-1", Value = 100 } },
                            { "Root2-Key-110",  new SampleClass{ Name = "Root1-Key100-Name-11", Value = 110 } }
                        }
                    },
                     {  200, new Dictionary<string, SampleClass>
                        {
                            { "Root2-Key-200",  new SampleClass{ Name = "Root1-Key200-Name-21", Value = 200 } }
                        }
                    },
              });

            var serializer = new JsonSerializerOptions
            {
                Converters =
                {
                    new DictionaryTypeResolverConverter<int, IDictionary<string, SampleClass>>(isNestedDictionary: true)
                }
            };
            var actual = JsonSerializer.Serialize(dictionary, serializer);

            var expected = "{\"Outer-1\":{\"10\":{\"Root1-Key-10\":{\"Name\":\"Root1-Key10-Name-1\",\"Value\":10}},\"20\":{\"Root1-Key-20\":{\"Name\":\"Root1-Key20-Name-21\",\"Value\":20}}},\"Outer-2\":{\"100\":{\"Root2-Key-100\":{\"Name\":\"Root1-Key100-Name-1\",\"Value\":100},\"Root2-Key-110\":{\"Name\":\"Root1-Key100-Name-11\",\"Value\":110}},\"200\":{\"Root2-Key-200\":{\"Name\":\"Root1-Key200-Name-21\",\"Value\":200}}}}";
            Assert.Equal(expected, actual);
        }

        [Fact(Skip = "Only 1 level of nested dictionary")]
        public void CanDeserializedDepthNestedDictionary()
        {
            var expected = new Dictionary<string, IDictionary<int, IDictionary<string, SampleClass>>>();
            expected.Add("Outer-1",
                new Dictionary<int, IDictionary<string, SampleClass>>
                {
                    {  10, new Dictionary<string, SampleClass>
                        {
                            { "Root1-Key-10",  new SampleClass{ Name = "Root1-Key10-Name-1", Value = 10 } }
                        }
                    },
                     {  20, new Dictionary<string, SampleClass>
                        {
                            { "Root1-Key-20",  new SampleClass{ Name = "Root1-Key20-Name-21", Value = 20 } }
                        }
                    },
                });
            expected.Add("Outer-2",
              new Dictionary<int, IDictionary<string, SampleClass>>
              {
                    {  100, new Dictionary<string, SampleClass>
                        {
                            { "Root2-Key-100",  new SampleClass{ Name = "Root1-Key100-Name-1", Value = 100 } },
                            { "Root2-Key-110",  new SampleClass{ Name = "Root1-Key100-Name-11", Value = 110 } }
                        }
                    },
                     {  200, new Dictionary<string, SampleClass>
                        {
                            { "Root2-Key-200",  new SampleClass{ Name = "Root1-Key200-Name-21", Value = 200 } }
                        }
                    },
              });

            var json = "{\"Outer-1\":{\"10\":{\"Root1-Key-10\":{\"Name\":\"Root1-Key10-Name-1\",\"Value\":10}},\"20\":{\"Root1-Key-20\":{\"Name\":\"Root1-Key20-Name-21\",\"Value\":20}}},\"Outer-2\":{\"100\":{\"Root2-Key-100\":{\"Name\":\"Root1-Key100-Name-1\",\"Value\":100},\"Root2-Key-110\":{\"Name\":\"Root1-Key100-Name-11\",\"Value\":110}},\"200\":{\"Root2-Key-200\":{\"Name\":\"Root1-Key200-Name-21\",\"Value\":200}}}}";
            var actual = JsonSerializer.Deserialize<Dictionary<string, IDictionary<int, IDictionary<string, SampleClass>>>>(json, _options);

            Assert.Equal(expected, actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.All(actual.Keys, actualKey => Assert.Contains(actualKey, expected.Keys));
            Assert.All(actual.Values.SelectMany(x => x.Keys), actualKey => Assert.Contains(actualKey, expected.Values.SelectMany(x => x.Keys)));
            Assert.All(actual.Values.SelectMany(x => x.Values), x => Assert.Contains(x, expected.Values.SelectMany(expect => expect.Values)));
        }


        [Fact]
        public void CanSerializedDictionaryWithTypeDiscriminator()
        {
            var dictionary = new Dictionary<int, SampleClass>()
            {
                { 10,  new SampleClass{ Name = "Root1-Key10-Name-1", Value = 10 } },
                { 20,  new SampleClass{ Name = "Root1-Key20-Name-21", Value = 20 } },
                { 100,  new SampleClass{ Name = "Root1-Key100-Name-1", Value = 100 } },
                { 110,  new SampleClass{ Name = "Root1-Key100-Name-11", Value = 110 } }
            };

            var options = new JsonSerializerOptions
            {
                Converters = { new DictionaryTypeResolverConverter<int, SampleClass>() }
            };

            var actual = JsonSerializer.Serialize(dictionary, _options);

            var expected = "{\"$type\":\"System.Collections.Generic.Dictionary\\u00602[[System.Int32, mscorlib],[DHI.Services.Test.Converters.SampleClass, DHI.Services.Test.Converters.SampleClass]], mscorlib\",\"10\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key10-Name-1\",\"Value\":10},\"20\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key20-Name-21\",\"Value\":20},\"100\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key100-Name-1\",\"Value\":100},\"110\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key100-Name-11\",\"Value\":110}}";
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CanDeserializedDictionaryWithTypeDiscriminator()
        {
            var json = "{\"$type\":\"System.Collections.Generic.Dictionary\\u00602[[System.Int32, mscorlib],[DHI.Services.Test.Converters.SampleClass, DHI.Services.Test.Converters.SampleClass]], mscorlib\",\"10\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key10-Name-1\",\"Value\":10},\"20\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key20-Name-21\",\"Value\":20},\"100\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key100-Name-1\",\"Value\":100},\"110\":{\"$type\":\"DHI.Services.Test.Converters.SampleClass, DHI.Services.Test\",\"Name\":\"Root1-Key100-Name-11\",\"Value\":110}}";

            var actual = JsonSerializer.Deserialize<IDictionary<int, SampleClass>>(json, _options);
            var expected = new Dictionary<int, SampleClass>()
            {
                { 10,  new SampleClass{ Name = "Root1-Key10-Name-1", Value = 10 } },
                { 20,  new SampleClass{ Name = "Root1-Key20-Name-21", Value = 20 } },
                { 100, new SampleClass{ Name = "Root1-Key100-Name-1", Value = 100 } },
                { 110, new SampleClass{ Name = "Root1-Key100-Name-11", Value = 110 } }
            };
            Assert.Equal(expected, actual);
        }
    }
}
