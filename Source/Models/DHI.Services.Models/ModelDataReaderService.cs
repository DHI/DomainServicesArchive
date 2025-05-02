namespace DHI.Services.Models
{
    using System;

    /// <summary>
    ///     Model Data Reader Service.
    /// </summary>
    public class ModelDataReaderService : BaseUpdatableDiscreteService<IModelDataReader, string>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelDataReaderService" /> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public ModelDataReaderService(IModelDataReaderRepository repository) : base(repository)
        {
        }

        /// <summary>
        ///     Gets the compatible repository types at the path of the executing assembly.
        /// </summary>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes()
        {
            return Service.GetProviderTypes<IModelDataReaderRepository>();
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">The path where to look for compatible providers.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string? path)
        {
            return Service.GetProviderTypes<IModelDataReaderRepository>(path);
        }

        /// <summary>
        ///     Gets the compatible repository types.
        /// </summary>
        /// <param name="path">
        ///     The path where to look for compatible providers. If path is null, the path of the executing assembly
        ///     is used.
        /// </param>
        /// <param name="searchPattern">
        ///     File name search pattern. Can contain a combination of valid literal path and wild card
        ///     (* and ?) characters.
        /// </param>
        /// <returns>Type[].</returns>
        public static Type[] GetRepositoryTypes(string path, string searchPattern)
        {
            return Service.GetProviderTypes<IModelDataReaderRepository>(path, searchPattern);
        }
    }
}