namespace DHI.Services.Meshes.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using AutoFixture;
    using AutoFixture.Kernel;
    using Spatial;
    using TimeSeries;
    using Xunit;

    public class BaseGroupedMeshRepositoryTest
    {
        public BaseGroupedMeshRepositoryTest()
        {
            var fixture = new Fixture();
            fixture.Customize<MeshInfo<Guid>>(c => c.FromFactory(new MethodInvoker(new GreedyConstructorQuery())));
            fixture.Inject(new DateRange(fixture.Create<DateTime>(), fixture.Create<TimeSpan>()));
            _repository = new TestMeshRepository(fixture.CreateMany<MeshInfo<Guid>>().ToList());
            _repeatCount = fixture.RepeatCount;
        }

        private readonly BaseGroupedMeshRepository<Guid> _repository;
        private readonly int _repeatCount;

        private static readonly TimeSeriesData _timeSeriesDataDense = new(
            new List<DateTime>
            {
                new(2000, 1, 1, 0, 0, 0),
                new(2000, 1, 1, 0, 30, 0),
                new(2000, 1, 1, 1, 0, 0),
                new(2000, 1, 1, 1, 30, 0),
                new(2000, 1, 1, 2, 0, 0),
                new(2000, 1, 1, 2, 30, 0),
                new(2000, 1, 1, 3, 0, 0),
                new(2000, 1, 1, 3, 30, 0),
                new(2000, 1, 1, 4, 0, 0),
                new(2000, 1, 1, 4, 30, 0),
                new(2000, 1, 1, 5, 0, 0)
            },
            new List<double?>
            {
                5, 6, 7, 10, 9, 12, null, 8, 6, 8, 10
            });

        private static TimeSeriesDataWFlag<int?> TimeSeriesData => new(
            new List<DateTime>
            {
                new(2000, 1, 1),
                new(2000, 1, 2),
                new(2000, 1, 3),
                new(2000, 1, 4),
                new(2000, 1, 5),
                new(2000, 1, 6),
                new(2000, 1, 7),
                new(2000, 1, 8),
                new(2000, 1, 9),
                new(2000, 1, 10),
                new(2000, 1, 11)
            },
            new List<double?>
            {
                5, 6, 7, 10, 9, 12, null, 8, 6, 8, 10
            },
            new List<int?>
            {
                1, 1, 1, 0, 1, 1, null, 0, null, 1, 0
            });

        

        private class TestMeshRepository : BaseGroupedMeshRepository<Guid>
        {
            private readonly Dictionary<Guid, MeshInfo<Guid>> _meshDictionary = new();

            public TestMeshRepository(IEnumerable<MeshInfo<Guid>> meshes)
            {
                foreach (var mesh in meshes)
                {
                    _meshDictionary.Add(mesh.Id, mesh);
                }
            }

            public override IEnumerable<MeshInfo<Guid>> GetAll(ClaimsPrincipal? user = null)
            {
                return _meshDictionary.Values.ToList();
            }

            public override bool ContainsGroup(string group, ClaimsPrincipal? user = null)
            {
                return _meshDictionary.Any(kvp => kvp.Value.Group == group);
            }

            public override IEnumerable<MeshInfo<Guid>> GetByGroup(string group, ClaimsPrincipal? user = null)
            {
                return _meshDictionary.Where(kvp => kvp.Value.Group == group).Select(kvp => kvp.Value).ToList();
            }

            public override IEnumerable<DateTime> GetDateTimes(Guid id, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override ITimeSeriesData<double> GetValues(Guid id, string item, Point point, DateRange dateRange, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override Dictionary<string, ITimeSeriesData<double>> GetValues(Guid id, Point point, DateRange dateRange, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override ITimeSeriesData<double> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, DateRange dateRange, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override ITimeSeriesData<double> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, Polygon polygon, DateRange dateRange, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override Maybe<double> GetAggregatedValue(Guid id, AggregationType aggregationType, string item, DateTime dateTime, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override Maybe<double> GetAggregatedValue(Guid id, AggregationType aggregationType, string item, Polygon polygon, DateTime dateTime, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<ITimeSeriesData<double>> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateRange dateRange, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<Maybe<double>> GetAggregatedValues(Guid id, AggregationType aggregationType, string item, IEnumerable<Polygon> polygons, DateTime dateTime, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }

            public override (Mesh mesh, float[] elementData) GetMeshData(Guid id, string item, DateTime? dateTime = null, ClaimsPrincipal? user = null)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void GetFullNamesByGroupIsOk()
        {
            var group = FullName.Parse(_repository.GetFullNames().First()).Group;
            var fullNames = _repository.GetFullNames(group).ToArray();
            Assert.Single(fullNames);
            var fullName = FullName.Parse(fullNames.First());
            Assert.NotNull(fullName.Group);
            Assert.NotNull(fullName.Name);
        }

        [Fact]
        public void GetFullNamesIsOk()
        {
            var fullNames = _repository.GetFullNames().ToArray();
            Assert.Equal(_repeatCount, fullNames.Length);
            var fullName = FullName.Parse(fullNames.First());
            Assert.NotNull(fullName.Group);
            Assert.NotNull(fullName.Name);
        }

        [Theory]
        [InlineData(Period.Weekly)]
        [InlineData(Period.Quarterly)]
        public void GetGroupedValuesThrowsOnIllegalPeriod(Period period)
        {
            Assert.Throws<NotSupportedException>(() => BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, period, TimeSeriesData));
            Assert.Throws<NotSupportedException>(() => BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, period, TimeSeriesData));
            Assert.Throws<NotSupportedException>(() => BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, period, TimeSeriesData));
            Assert.Throws<NotSupportedException>(() => BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, period, TimeSeriesData));
        }

        [Fact]
        public void GetGroupedValuesAverageIsOk()
        {
            var yearly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, Period.Yearly, TimeSeriesData);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(8.1, yearly.Values[0]);

            var monthly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, Period.Monthly, TimeSeriesData);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(8.1, monthly.Values[0]);

            var daily = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, Period.Daily, TimeSeriesData);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Average, Period.Hourly, _timeSeriesDataDense);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(5.5, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Fact]
        public void GetGroupedValuesMaximumIsOk()
        {
            var yearly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, Period.Yearly, TimeSeriesData);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(12, yearly.Values[0]);

            var monthly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, Period.Monthly, TimeSeriesData);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(12, monthly.Values[0]);

            var daily = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, Period.Daily, TimeSeriesData);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Maximum, Period.Hourly, _timeSeriesDataDense);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(6, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Fact]
        public void GetGroupedValuesMinimumIsOk()
        {
            var yearly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, Period.Yearly, TimeSeriesData);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(5, yearly.Values[0]);

            var monthly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, Period.Monthly, TimeSeriesData);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(5, monthly.Values[0]);

            var daily = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, Period.Daily, TimeSeriesData);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Minimum, Period.Hourly, _timeSeriesDataDense);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(5, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Fact]
        public void GetGroupedValuesSumIsOk()
        {
            var yearly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, Period.Yearly, TimeSeriesData);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(81, yearly.Values[0]);

            var monthly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, Period.Monthly, TimeSeriesData);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(81, monthly.Values[0]);

            var daily = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, Period.Daily, TimeSeriesData);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Equal(0, daily.Values[6]);

            var hourly = BaseGroupedMeshRepository<Guid>.GetGroupedValues(AggregationType.Sum, Period.Hourly, _timeSeriesDataDense);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(11, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }
    }
}