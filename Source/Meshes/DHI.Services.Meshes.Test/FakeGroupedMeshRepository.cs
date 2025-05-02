namespace DHI.Services.Meshes.Test
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Spatial;
    using TimeSeries;

    public class FakeGroupedMeshRepository : FakeGroupedRepository<MeshInfo<Guid>, Guid>, IGroupedMeshRepository<Guid>
    {
        public FakeGroupedMeshRepository(IEnumerable<MeshInfo<Guid>> meshInfoList)
            : base(meshInfoList)
        {
        }

        public IEnumerable<DateTime> GetDateTimes(Guid id, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public ITimeSeriesData<double> GetValues(Guid id, string item, Point point, DateRange dataRange, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, ITimeSeriesData<double>> GetValues(Guid id, Point point, DateRange dataRange, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public ITimeSeriesData<double> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, DateRange dataRange, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public ITimeSeriesData<double> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, Polygon polygon, DateRange dataRange, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public ITimeSeriesData<double> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, Period period, DateRange dataRange, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public ITimeSeriesData<double> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, Polygon polygon, Period period, DateRange dataRange, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Maybe<double> GetAggregatedValue(Guid id, AggregationType aggregationType, string item, DateTime dateTime, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Maybe<double> GetAggregatedValue(Guid id, AggregationType aggregationType, string item, Polygon polygon, DateTime dateTime, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ITimeSeriesData<double>> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateRange dateRange, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Maybe<double>> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateTime dateTime, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public (Mesh mesh, float[] elementData) GetMeshData(Guid id, string item, DateTime? dateTime = null, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }
    }
}