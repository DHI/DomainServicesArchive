namespace DHI.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    ///     Class Service.
    /// </summary>
    public static class Service
    {
        /// <summary>
        ///     Gets all the provider types at the path of the executing assembly that are compatible with the given abstraction type.
        /// </summary>
        /// <typeparam name="TAbstraction">The interface type of the abstraction.</typeparam>
        /// <returns>Type[].</returns>
        public static Type[] GetProviderTypes<TAbstraction>()
        {
            return _GetProviderTypes<TAbstraction>();
        }

        /// <summary>
        ///     Gets all the provider types in the given path location that are compatible with the given abstraction type.
        /// </summary>
        /// <typeparam name="TAbstraction">The interface type of the abstraction.</typeparam>
        /// <param name="path">The path.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetProviderTypes<TAbstraction>(string path)
        {
            return _GetProviderTypes<TAbstraction>(path);
        }

        /// <summary>
        ///     Gets all the provider types in the given path location that are compatible with the given abstraction type.
        /// </summary>
        /// <typeparam name="TAbstraction">The interface type of the abstraction.</typeparam>
        /// <param name="path">The path.  If path is null, the path of the executing assembly is used.</param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wildcard (*and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetProviderTypes<TAbstraction>(string path, string searchPattern)
        {
            return _GetProviderTypes<TAbstraction>(path, searchPattern);
        }

        private static Type[] _GetProviderTypes<TAbstraction>(string path = null, string searchPattern = "dhi*.dll")
        {
            var types = new List<Type>();

            try
            {
                path ??= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                var assemblies = new List<Assembly>();
                foreach (var dll in Directory.EnumerateFiles(path, searchPattern))
                {
                    try
                    {
                        assemblies.Add(Assembly.UnsafeLoadFrom(dll));
                    }
                    catch (Exception)
                    {
                        // Probably an unmanaged dll or a 32/64-bit conflict
                    }
                }

                var abstractionType = typeof(TAbstraction);
                foreach (var assembly in assemblies)
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.GetInterfaces().Any(i => i.FullName == abstractionType.FullName))
                        {
                            types.Add(type);
                        }
                    }
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

            return types.ToArray();
        }
    }
}