namespace DHI.Services.Test.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text.Json;
    using DHI.Services.Converters;

    public class DictionaryConverterFixture : IDisposable
    {
        private readonly JsonSerializerOptions _options;
        private IDictionary<string, IDictionary<string, SampleClass>> _nestedDictionary;

        public DictionaryConverterFixture()
        {
            _options = new JsonSerializerOptions
            {
                Converters = {
                    new DictionaryTypeResolverConverter<int, SampleClass>(),
                    new DictionaryTypeResolverConverter<string, SampleClass>(isNestedDictionary: true)}
            };

            _nestedDictionary = new Dictionary<string, IDictionary<string, SampleClass>>();
            _nestedDictionary.Add("Root-1", new Dictionary<string, SampleClass>
            {
                { "Root1-Key-1",  new SampleClass{ Name = "Root1-Key1-Name-1", Value = 10 } },
                { "Root1-Key-2",  new SampleClass{ Name = "Root1-Key2-Name-2", Value = 20 } },
            });
            _nestedDictionary.Add("Root-2", new Dictionary<string, SampleClass>
            {
                { "Root2-Key-1",  new SampleClass{ Name = "Root2-Key1-Name-1", Value = 100 } },
                { "Root2-Key-2",  new SampleClass{ Name = "Root2-Key2-Name-2", Value = 200 } },
            });
        }

        public JsonSerializerOptions SerializerOptions => _options;

        public IDictionary<string, IDictionary<string, SampleClass>> SampleData => _nestedDictionary;

        public class SampleClass : IEqualityComparer<SampleClass>
        {
            public string Name { get; set; }

            public int Value { get; set; }

            public override bool Equals(object obj)
            {
                return Equals(obj as SampleClass, this);
            }

            public bool Equals(SampleClass x, SampleClass y)
            {
                if (x.Name != y.Name) return false;
                if (x.Value != y.Value) return false;

                return true;
            }

            public int GetHashCode([DisallowNull] SampleClass obj)
            {
                return obj.Name.GetHashCode() + obj.Value.GetHashCode();
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        public void Dispose()
        {
            _nestedDictionary.Clear();
            _nestedDictionary = null;
        }
    }
}
