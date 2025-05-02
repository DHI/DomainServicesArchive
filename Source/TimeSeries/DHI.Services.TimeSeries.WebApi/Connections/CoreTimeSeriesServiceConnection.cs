namespace DHI.Services.TimeSeries.WebApi
{
    public class CoreTimeSeriesServiceConnection : CoreTimeSeriesServiceConnection<string, double>
    {
        public CoreTimeSeriesServiceConnection(string id, string name)
            : base(id, name)
        {
        }
    }
}
