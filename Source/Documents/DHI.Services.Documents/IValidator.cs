namespace DHI.Services.Documents
{
    using System.IO;

    /// <summary>
    ///     Interface IValidator
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        ///     Determines whether this instance can validate the document with specified file name.
        /// </summary>
        /// <param name="fileName">The document file name.</param>
        /// <returns><c>true</c> if this instance can validate the document with the specified file name; otherwise, <c>false</c>.</returns>
        bool CanValidate(string fileName);

        /// <summary>
        ///     Validates the given document stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.ValueTuple&lt;System.Boolean, System.String&gt;.</returns>
        (bool validated, string message) Validate(Stream stream);
    }
}