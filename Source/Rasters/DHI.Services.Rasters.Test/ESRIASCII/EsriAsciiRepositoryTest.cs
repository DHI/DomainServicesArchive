namespace DHI.Services.Rasters.Test.ESRIASCII
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Radar.ESRIASCII;
    using Xunit;

    public class EsriAsciiRepositoryTest : IClassFixture<EsriAsciiRepositoryFixture>
    {
        private readonly EsriAsciiRepository _repository;

        public EsriAsciiRepositoryTest(EsriAsciiRepositoryFixture fixture)
        {
            _repository = fixture.Repository;
        }

        [Fact]
        public void CreateWithIllegalConnectionStringThrows()
        {
            Assert.Throws<ArgumentException>(() => new EsriAsciiRepository("test;test"));
        }

        [Fact]
        public void GetDateTimesOk()
        {
            Assert.Equal(3, _repository.GetDateTimes(DateTime.MinValue, DateTime.MaxValue).Count());
        }

        [Fact]
        public void ContainsOk()
        {
            Assert.True(_repository.Contains(new DateTime(2018, 3, 15, 15, 45, 0)));
            Assert.False(_repository.Contains(new DateTime(2017, 3, 15, 15, 45, 0)));
        }

        [Fact]
        public void FirstDateTimeOk()
        {
            Assert.Equal(new DateTime(2018, 3, 15, 12, 45, 0), _repository.FirstDateTime());
        }

        [Fact]
        public void GetFromToOk()
        {
            Assert.Equal(3, _repository.Get(new DateTime(2018, 3, 15, 12, 12, 0), new DateTime(2018, 3, 16, 15, 50, 0)).Count);
        }

        [Fact]
        public void GetDateTimesFirstAfterOk()
        {
            var datetimes = new List<DateTime>
            {
                new DateTime(2018, 3, 15, 15, 40, 0),
                new DateTime(2018, 3, 16, 12, 40, 0)
            };

            var query = _repository.GetDateTimesFirstAfter(datetimes);
            Assert.Equal(2, query.Count());
        }

        [Fact]
        public void GetDateTimesLastBeforeOk()
        {
            var datetimes = new List<DateTime>
            {
                new DateTime(2018, 3, 15, 12, 40, 0),
                new DateTime(2018, 3, 16, 12, 40, 0)
            };

            var query = _repository.GetDateTimesLastBefore(datetimes);
            Assert.Single(query);
        }

        [Fact]
        public void GetOk()
        {
            var query = _repository.Get(new DateTime(2018, 3, 16, 12, 45, 0));
            Assert.True(query.Value.MaxValue == 173.313f);
        }

        [Fact]
        public void GetFirstAfterOk()
        {
            var query = _repository.GetFirstAfter(new DateTime(2018, 3, 15, 12, 45, 0));
            Assert.True(query.MaxValue == 173.313f);
        }

        [Fact]
        public void GetFirstAfterListOk()
        {
            var datetimes = new List<DateTime>
            {
                new DateTime(2018, 3, 15, 12, 40, 0),
                new DateTime(2018, 3, 16, 12, 40, 0)
            };
            
            var query = _repository.GetFirstAfter(datetimes);
            Assert.True(query.Count() == 2);
        }


        [Fact]
        public void GetLastBeforeOk()
        {
            var query = _repository.GetLastBefore(new DateTime(2018, 3, 16, 12, 45, 0));
            Assert.NotNull(query.MaxValue);
        }

        [Fact]
        public void GetLastBeforeListOk()
        {
            var datetimes = new List<DateTime>
            {
                new DateTime(2018, 3, 15, 12, 50, 0),
                new DateTime(2018, 3, 16, 12, 50, 0)
            };

            var query = _repository.GetLastBefore(datetimes);
            Assert.Equal(2, query.Count());
        }

        [Fact]
        public void LastOk()
        {
            var query = _repository.Last();
            Assert.NotNull(query.MaxValue);
        }

        [Fact]
        public void LastDateTimeOk()
        {
            var query = _repository.LastDateTime();
            Assert.Equal(query, new DateTime(2018, 3, 16, 12, 45, 0));
        }
    }
}