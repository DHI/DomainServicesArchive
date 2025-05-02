namespace DHI.Services.Rasters.Radar
{
    using System.IO;

    /// <summary>
    /// Stream extension methods.
    /// </summary>
    public static class StreamExtensionMethods
    {
        /// <summary>
        /// Converts a stream to a byte array.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <returns>A byte array.</returns>
        public static byte[] ToByteArray(this Stream stream)
        {
            using var streamReader = new MemoryStream();
            stream.CopyTo(streamReader);
            var result = streamReader.ToArray();

            return result;
        }
    }
}
