namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Class ConnectionTypeService
    /// </summary>
    public class ConnectionTypeService : BaseUpdatableDiscreteService<ConnectionType, string>
    {
        private readonly string _directoryPath;
        private readonly string _assemblyFilter;

        /// <summary>
        /// Initiates a new instances of the <see cref="ConnectionTypeService"/>
        /// </summary> 
        /// <param name="directoryPath">Directory for target assembly</param>
        /// <param name="assemblyFilter">Assembly name for filtering</param>
        public ConnectionTypeService(string directoryPath, string assemblyFilter = "DHI.Services")
            : base(new ConnectionTypeRepository())
        {
            if (string.IsNullOrWhiteSpace(directoryPath) == false
                && Directory.Exists(directoryPath) == false)
                throw new DirectoryNotFoundException(directoryPath);

            _directoryPath = directoryPath ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _assemblyFilter = assemblyFilter ?? "DHI.Services";

            var types = ScanAssemblyForConnectionTypes(directoryPath);
            Initialized(types);
        }

        /// <summary>
        /// Initiates a new instances of the <see cref="ConnectionTypeService"/>
        /// </summary>
        /// <param name="connectionTypes">The connection type collections</param>
        public ConnectionTypeService(IEnumerable<ConnectionType> connectionTypes) : base(new ConnectionTypeRepository())
        {
            Initialized(connectionTypes);
        }

        /// <summary>
        /// Scanning for <see cref="ConnectionType"/> from assemblies
        /// </summary>
        /// <param name="directory">Directory for target assembly</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        protected virtual IEnumerable<ConnectionType> ScanAssemblyForConnectionTypes(string directory)
        {
            try
            {
                Assembly
                    .GetExecutingAssembly()
                    .GetReferencedAssemblies()
                    .Where(assembly => assembly.Name.StartsWith(_assemblyFilter))
                    .Select(Assembly.Load);

                var connectionTypesFound = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(assembly => assembly.GetName().Name.StartsWith(_assemblyFilter))
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => !type.IsAbstract || !type.IsInterface)
                    .Where(type => typeof(IConnection).IsAssignableFrom(type) && type.IsSubclassOf(typeof(BaseConnection)))
                    .Where(type => type.ContainsGenericParameters == false)
                    .Where(type => type.GetMethod("CreateConnectionType", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy) != null);

                var connectionTypes = new List<ConnectionType>();
                foreach (var type in connectionTypesFound)
                {
                    var method = type.GetMethod("CreateConnectionType", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (method.IsGenericMethod)
                    {
                        method = method.MakeGenericMethod(type);
                    }

                    var connectionType = method.Invoke(null, new[] { directory });
                    if (connectionType as ConnectionType != null)
                        connectionTypes.Add((ConnectionType)connectionType);
                }

                return connectionTypes;
            }
            catch (Exception ex)
            {
                if (ex is ReflectionTypeLoadException)
                {
                    var typeLoadException = (ReflectionTypeLoadException)ex;
                    throw new Exception(string.Join(Environment.NewLine, typeLoadException.LoaderExceptions.Select(r => r.Message)));
                }

                throw;
            }
        }


        /// <summary>
        /// Initialized <seealso cref="ConnectionTypeRepository"/> values by given <paramref name="connectionTypes"/>
        /// </summary>
        /// <param name="connectionTypes">The connection type collections for initial value</param> 
        protected virtual void Initialized(IEnumerable<ConnectionType> connectionTypes)
        {
            foreach (var connectionType in connectionTypes)
            {
                if (!Exists(connectionType.Id))
                    Add(connectionType);
            }
        }
    }
}