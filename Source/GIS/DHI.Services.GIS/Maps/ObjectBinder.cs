namespace DHI.Services.GIS.Maps
{
    using System;
    using System.Reflection;
    using System.Runtime.Serialization;

    public sealed class ObjectBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            var currentAssembly = Assembly.GetExecutingAssembly().FullName;
            assemblyName = currentAssembly;
            var typeToDeserialize = Type.GetType($"{typeName}, {assemblyName}");
            return typeToDeserialize;
        }
    }
}