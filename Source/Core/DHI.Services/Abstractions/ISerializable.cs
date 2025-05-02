namespace DHI.Services
{
    /// <summary>
    ///     Interface ISerializable
    /// </summary>
    public interface ISerializable
    {
        /// <summary>
        ///     Serializes this instance.
        /// </summary>
        /// <returns>System.String.</returns>
        string Serialize();
    }
}