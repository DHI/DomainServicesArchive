namespace DHI.Services
{

    /// <summary>
    /// Interface IFactory 
    /// </summary>
    /// <typeparam name="T">Return of instances</typeparam>
    public interface IFactory<T>
    {
        /// <summary>
        /// Create new instance of <typeparamref name="T"/>
        /// </summary>
        /// <returns></returns>
        T Create();
    }

    /// <summary>
    /// Interface IFactory
    /// </summary>
    public interface IFactory : IFactory<object>
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <returns>System.Object.</returns>
        //object Create();
    }

}