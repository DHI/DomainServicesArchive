namespace DHI.Services.Models.Converters
{
    using System;
    using System.Collections.Generic;

    public static class ModelDataReaderTypeRegistry
    {
        private static readonly Dictionary<string, Type> _registeredTypes = new();

        public static void Register<TModelReader>(string typeName)
            where TModelReader : IModelInputReader, IModelOutputReader, new()
        {
            var wrapperType = typeof(ModelDataReader<>).MakeGenericType(typeof(TModelReader));
            _registeredTypes[typeName] = wrapperType;
        }

        public static void Register(string typeName, Type wrapperType)
        {
            _registeredTypes[typeName] = wrapperType;
        }

        public static Type? GetTypeFor(string typeName)
        {
            _registeredTypes.TryGetValue(typeName, out var type);
            return type;
        }

        public static IReadOnlyDictionary<string, Type> GetAll() => _registeredTypes;
    }
}
