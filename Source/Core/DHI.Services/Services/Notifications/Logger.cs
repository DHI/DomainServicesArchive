namespace DHI.Services.Notifications
{
    using System;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     Class Logger.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        ///     Gets an array of available logger types.
        /// </summary>
        /// <param name="path">The path where to look for logger types.</param>
        /// <returns>Type[].</returns>
        public static Type[] GetLoggerTypes(string path = null)
        {
            return Service.GetProviderTypes<ILogger>(path);
        }
    }
}