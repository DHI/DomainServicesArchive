namespace DHI.Services.Documents
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    /// <summary>
    ///     Class BaseValidator.
    /// </summary>
    public abstract class BaseValidator : IValidator
    {
        private readonly string _pattern;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BaseValidator" /> class.
        /// </summary>
        /// <param name="pattern">The regular expression pattern to match to activate validator.</param>
        protected BaseValidator(string pattern)
        {
            _pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
        }

        /// <summary>
        ///     Determines whether this instance can validate the document with specified file name.
        /// </summary>
        /// <param name="fileName">The document file name.</param>
        /// <returns><c>true</c> if this instance can validate the document with the specified file name; otherwise, <c>false</c>.</returns>
        public bool CanValidate(string fileName)
        {
            return Regex.IsMatch(fileName, _pattern);
        }

        /// <summary>
        ///     Validates the given document stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>System.ValueTuple&lt;System.Boolean, System.String&gt;.</returns>
        public abstract (bool validated, string message) Validate(Stream stream);
    }
}