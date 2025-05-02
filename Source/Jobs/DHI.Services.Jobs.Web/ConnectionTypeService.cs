namespace DHI.Services.Jobs.Web
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class ConnectionTypeService. This class cannot be inherited.
    /// </summary>
    public sealed class ConnectionTypeService : BaseUpdatableDiscreteService<ConnectionType, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTypeService"/> class.
        /// </summary>
        /// <param name="path">A path to look for connection types.</param>
        /// <exception cref="Exception"></exception>
        public ConnectionTypeService(string path = null)
            : base(new ConnectionTypeRepository())
        {
            try
            {
                if (path == null)
                {
                    path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                }

                var types = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(BaseConnection))).ToArray();
                foreach (var type in types)
                {
                    var method = type.GetMethod("CreateConnectionType", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (method.IsGenericMethod)
                    {
                        method = method.MakeGenericMethod(type);
                    }

                    var connectionType = method.Invoke(null, new object[] { path });
                    Add((ConnectionType)connectionType);
                }
            }
            catch (Exception ex)
            {
                if (ex is ReflectionTypeLoadException typeLoadException)
                {
                    throw new Exception(string.Join(Environment.NewLine, typeLoadException.LoaderExceptions.Select(r => r.Message)));
                }

                throw;
            }
        }
    }
}