namespace DHI.Services.Filters
{
    public class TransportConnectionEventArgs : EventArgs<(string transportConnectioId, string filterId, int count)>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportConnectionEventArgs"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        public TransportConnectionEventArgs((string transportConnectioId, string filterId, int count) info) : base(info)
        {
        }
    }
}