namespace DHI.Services.Filters
{
    public class TransportConnectionCancelEventArgs : CancelEventArgs<(string transportConnectioId, string filterId)>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="TransportConnectionCancelEventArgs"/> class.
        /// </summary>
        public TransportConnectionCancelEventArgs((string transportConnectioId, string filterId) info) : base(info)
        {
        }
    }
}