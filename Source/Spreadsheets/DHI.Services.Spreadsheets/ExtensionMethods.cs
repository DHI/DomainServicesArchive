namespace DHI.Services.Spreadsheets
{
    /// <summary>
    ///     Extension Methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        ///     Create a sheet from an object array
        /// </summary>
        /// <param name="data">The object array data.</param>
        public static Sheet ToSheet(this object[,] data)
        {
            return new Sheet(data);
        }
    }
}