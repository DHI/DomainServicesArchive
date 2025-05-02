namespace DHI.Services.TimeSeries.Test.Analysis
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using AutoFixture.Xunit2;
    using DHI.Services.TimeSeries;
    using Xunit;

    public class BasicAnalysisTest
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void TimeSpanOfEmptyIsDefaultTimeSpan()
        {
            Assert.Equal(default, new TimeSeriesData<float>().TimeSpan());
        }

        [Fact]
        public void TimeSpanIsOk()
        {
            Assert.Equal(new TimeSpan(10, 0, 0, 0), TestData.TimeSeriesData.TimeSpan());
        }

        [Fact]
        public void SumOfEmptyValuesIsZero()
        {
            Assert.Equal(0, new TimeSeriesData().Sum());
        }

        [Fact]
        public void SumDoubleIsOk()
        {
            var timeSeriesData = new TimeSeriesData();
            timeSeriesData.Values.Add(1.234);
            timeSeriesData.Values.Add(null);
            timeSeriesData.Values.Add(2.333);
            Assert.Equal(3.567, timeSeriesData.Sum());
        }

        [Theory, AutoData]
        public void SumDoubleAutoIsOk(TimeSeriesData timeSeriesData)
        {
            _fixture.AddManyTo(timeSeriesData.Values);
            Assert.Equal(timeSeriesData.Values.Sum(), timeSeriesData.Sum());
        }

        [Fact]
        public void SumFloatIsOk()
        {
            var timeSeriesData = new TimeSeriesData<float>();
            timeSeriesData.Values.Add(1.234f);
            timeSeriesData.Values.Add(2.333f);
            Assert.Equal(3.567f, timeSeriesData.Sum());
        }

        [Fact]
        public void SumIntIsOk()
        {
            var timeSeriesData = new TimeSeriesData<int>();
            timeSeriesData.Values.Add(11);
            timeSeriesData.Values.Add(22);
            Assert.Equal(33, timeSeriesData.Sum());
        }

        [Theory]
        [InlineData(2, 10, 9)]
        [InlineData(4, 8, 8)]
        public void MovingAverageDoubleIsOk(int period, int countExpected, double lastValueExpected)
        {
            var movingAverage = TestData.TimeSeriesData.MovingAverage(period);

            Assert.Equal(countExpected, movingAverage.Count);
            Assert.Equal(lastValueExpected, movingAverage.ToSortedSet().Single(dataPoint => dataPoint.DateTime == new DateTime(2000, 1, 11)).Value);
        }

        [Fact]
        public void MovingAverageWithTypeIsOk()
        {
            var movingAverageBackward = TestData.TimeSeriesDataDense.MovingAverage(TimeSpan.FromHours(3), MovingAggregationType.Backwards);
            Assert.Equal(7, movingAverageBackward.Values[3].Value);
            var movingAverageForward = TestData.TimeSeriesDataDense.MovingAverage(TimeSpan.FromHours(3), MovingAggregationType.Forward);
            Assert.Equal(8, movingAverageForward.Values[6].Value);
            var movingAverageMiddle = TestData.TimeSeriesDataDense.MovingAverage(TimeSpan.FromHours(3), MovingAggregationType.Middle);
            Assert.Equal(8, movingAverageMiddle.Values[9].Value);
        }

        [Fact]
        public void MovingMinimumWithTypeIsOk()
        {
            var movingMinimumBack = TestData.TimeSeriesDataDense.MovingMinimum(TimeSpan.FromHours(3), MovingAggregationType.Backwards);
            Assert.Equal(5, movingMinimumBack.Values[3].Value);
            var movingMinimumForward = TestData.TimeSeriesDataDense.MovingMinimum(TimeSpan.FromHours(3), MovingAggregationType.Forward);
            Assert.Equal(6, movingMinimumForward.Values[3].Value);
            var movingMinimumMiddle = TestData.TimeSeriesDataDense.MovingMinimum(TimeSpan.FromHours(3), MovingAggregationType.Middle);
            Assert.Equal(5, movingMinimumMiddle.Values[3].Value);
        }

        [Fact]
        public void MovingMaximumWithTypeIsOk()
        {
            var movingMaxBackward = TestData.TimeSeriesDataDense.MovingMaximum(TimeSpan.FromHours(3), MovingAggregationType.Backwards);
            Assert.Equal(10, movingMaxBackward.Values[3].Value);
            var movingMaxForward = TestData.TimeSeriesDataDense.MovingMaximum(TimeSpan.FromHours(3), MovingAggregationType.Forward);
            Assert.Equal(12, movingMaxForward.Values[3].Value);
            var movingMaxMiddle = TestData.TimeSeriesDataDense.MovingMaximum(TimeSpan.FromHours(3), MovingAggregationType.Middle);
            Assert.Equal(12, movingMaxMiddle.Values[3].Value);
        }

        [Fact]
        public void MinimumOfEmptyValuesReturnsNull()
        {
            Assert.Null(new TimeSeriesData<float>().Minimum());
        }

        [Fact]
        public void MinimumIsOk()
        {
            Assert.Equal(5, TestData.TimeSeriesData.Minimum());
        }

        [Fact]
        public void MinimumGenericIsOk()
        {
            var data = new TimeSeriesData<int>(new List<DateTime>
                {
                    new DateTime(2015, 1, 1),
                    new DateTime(2015, 1, 4),
                    new DateTime(2015, 1, 6),
                    new DateTime(2015, 1, 9)
                },
                new List<int?>
                {
                    10, 11, null, 9
                });

            Assert.Equal(9, data.Minimum());
        }

        [Fact]
        public void MaximumOfEmptyValuesReturnsNull()
        {
            Assert.Null(new TimeSeriesData<float>().Maximum());
        }

        [Fact]
        public void MaximumIsOk()
        {
            Assert.Equal(12, TestData.TimeSeriesData.Maximum());
        }

        [Fact]
        public void MaximumGenericIsOk()
        {
            var data = new TimeSeriesData<int>(new List<DateTime>
                {
                    new DateTime(2015, 1, 1),
                    new DateTime(2015, 1, 4),
                    new DateTime(2015, 1, 6),
                    new DateTime(2015, 1, 9)
                },
                new List<int?>
                {
                    10, 11, null, 9
                });

            Assert.Equal(11, data.Maximum());
        }

        [Fact]
        public void AverageOfEmptyValuesIsZero()
        {
            Assert.Equal(0, new TimeSeriesData<float>().Average());
        }

        [Fact]
        public void AverageNullValuesAreIgnored()
        {
            Assert.Equal(8.1, TestData.TimeSeriesData.Average());
        }

        [Fact]
        public void AverageGenericIsOk()
        {
            var data = new TimeSeriesData<int>(new List<DateTime>
                {
                    new DateTime(2015, 1, 1),
                    new DateTime(2015, 1, 4),
                    new DateTime(2015, 1, 6),
                    new DateTime(2015, 1, 9)
                },
                new List<int?>
                {
                    10, 11, null, 9
                });

            Assert.Equal(10, data.Average());
        }

        [Fact]
        public void EquidistantMiddleIsOk()
        {
            var interval = TimeSpan.FromDays(1);
            var equidistant = TestData.TimeSeriesDataWithGaps.ToEquidistant(interval);
            var expectedDayCount = TestData.TimeSeriesDataWithGaps.TimeSpan().Days;
            var actualDayCount = equidistant.TimeSpan().Days;
            Assert.Equal(expectedDayCount, actualDayCount);
        }

        [Fact]
        public void EquidistantWithStartIsOk()
        {
            var startTime = TestData.TimeSeriesDataWithGaps.DateTimes.First().AddDays(-4);
            var interval = TimeSpan.FromDays(1);
            var equidistant = TestData.TimeSeriesDataWithGaps.ToEquidistant(interval, null, startTime);
            var expectedDayCount = (TestData.TimeSeriesDataWithGaps.DateTimes.Max() - startTime).Days;
            var actualDayCount = equidistant.TimeSpan().Days;
            Assert.Equal(expectedDayCount, actualDayCount);
        }

        [Fact]
        public void EquidistantWithStartEndIsOk()
        {
            var startTime = TestData.TimeSeriesDataWithGaps.DateTimes.First().AddDays(-4);
            var endTime = TestData.TimeSeriesDataWithGaps.DateTimes.Last().AddDays(4);
            var interval = TimeSpan.FromDays(1);
            var equidistant = TestData.TimeSeriesDataWithGaps.ToEquidistant(interval, null, startTime, endTime);
            var expectedDayCount = (endTime - startTime).Days;
            var actualDayCount = equidistant.TimeSpan().Days;
            Assert.Equal(expectedDayCount, actualDayCount);
        }

        [Fact]
        public void SumPeriodicallyIsOk()
        {
            var yearly = TestData.TimeSeriesData.Sum(Period.Yearly);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(81, yearly.Values[0]);

            var monthly = TestData.TimeSeriesData.Sum(Period.Monthly);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(81, monthly.Values[0]);

            var daily = TestData.TimeSeriesData.Sum(Period.Daily);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Equal(0, daily.Values[6]);

            var hourly = TestData.TimeSeriesDataDense.Sum(Period.Hourly);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(11, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Theory]
        [InlineData(Period.Weekly)]
        [InlineData(Period.Quarterly)]
        public void SumPeriodicallyThrowsOnIllegalPeriod(Period period)
        {
            Assert.Throws<NotSupportedException>(() => TestData.TimeSeriesData.Sum(period));
        }

        [Fact]
        public void AveragePeriodicallyIsOk()
        {
            var yearly = TestData.TimeSeriesData.Average(Period.Yearly);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(8.1, yearly.Values[0]);

            var monthly = TestData.TimeSeriesData.Average(Period.Monthly);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(8.1, monthly.Values[0]);

            var daily = TestData.TimeSeriesData.Average(Period.Daily);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = TestData.TimeSeriesDataDense.Average(Period.Hourly);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(5.5, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Theory]
        [InlineData(Period.Weekly)]
        [InlineData(Period.Quarterly)]
        public void AveragePeriodicallyThrowsOnIllegalPeriod(Period period)
        {
            Assert.Throws<NotSupportedException>(() => TestData.TimeSeriesData.Average(period));
        }

        [Fact]
        public void MinimumPeriodicallyIsOk()
        {
            var yearly = TestData.TimeSeriesData.Minimum(Period.Yearly);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(5, yearly.Values[0]);

            var monthly = TestData.TimeSeriesData.Minimum(Period.Monthly);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(5, monthly.Values[0]);

            var daily = TestData.TimeSeriesData.Minimum(Period.Daily);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = TestData.TimeSeriesDataDense.Minimum(Period.Hourly);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(5, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Theory]
        [InlineData(Period.Weekly)]
        [InlineData(Period.Quarterly)]
        public void MinimumPeriodicallyThrowsOnIllegalPeriod(Period period)
        {
            Assert.Throws<NotSupportedException>(() => TestData.TimeSeriesData.Minimum(period));
        }

        [Fact]
        public void MaximumPeriodicallyIsOk()
        {
            var yearly = TestData.TimeSeriesData.Maximum(Period.Yearly);
            Assert.Single(yearly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), yearly.DateTimes[0]);
            Assert.Equal(12, yearly.Values[0]);

            var monthly = TestData.TimeSeriesData.Maximum(Period.Monthly);
            Assert.Single(monthly.DateTimes);
            Assert.Equal(new DateTime(2000, 1, 1), monthly.DateTimes[0]);
            Assert.Equal(12, monthly.Values[0]);

            var daily = TestData.TimeSeriesData.Maximum(Period.Daily);
            Assert.Equal(11, daily.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1), daily.DateTimes[0]);
            Assert.Equal(new DateTime(2000, 1, 11), daily.DateTimes[10]);
            Assert.Equal(12, daily.Values[5]);
            Assert.Null(daily.Values[6]);

            var hourly = TestData.TimeSeriesDataDense.Maximum(Period.Hourly);
            Assert.Equal(6, hourly.DateTimes.Count);
            Assert.Equal(new DateTime(2000, 1, 1, 0, 0, 0), hourly.DateTimes[0]);
            Assert.Equal(6, hourly.Values[0]);
            Assert.Equal(new DateTime(2000, 1, 1, 3, 0, 0), hourly.DateTimes[3]);
            Assert.Equal(8, hourly.Values[3]);
        }

        [Theory]
        [InlineData(Period.Weekly)]
        [InlineData(Period.Quarterly)]
        public void MaximumPeriodicallyThrowsOnIllegalPeriod(Period period)
        {
            Assert.Throws<NotSupportedException>(() => TestData.TimeSeriesData.Maximum(period));
        }

        [Fact]
        public void AverageListIsOk()
        {
            var timeSeriesDataList = new List<ITimeSeriesData<double>>
            {
                TestData.TimeSeriesData,
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes, TestData.TimeSeriesData.Values.Select(v => v * 2).ToList()),
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(5.2)).ToList(), TestData.TimeSeriesData.Values)
            };

            var averageData = timeSeriesDataList.Average();
            Assert.Equal(22, averageData.DateTimes.Count);
            Assert.Equal(7.5, averageData.Values[0]);
            Assert.Equal(10, averageData.Values[21]);
            Assert.Equal(double.NaN, averageData.Values[17]);
        }

        [Fact]
        public void SumListIsOk()
        {
            var timeSeriesDataList = new List<ITimeSeriesData<double>>
            {
                TestData.TimeSeriesData,
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes, TestData.TimeSeriesData.Values.Select(v => v * 2).ToList()),
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(5.2)).ToList(), TestData.TimeSeriesData.Values)
            };

            var sumData = timeSeriesDataList.Sum();
            Assert.Equal(22, sumData.DateTimes.Count);
            Assert.Equal(15, sumData.Values[0]);
            Assert.Equal(10, sumData.Values[21]);
            Assert.Equal(0, sumData.Values[17]);
        }

        [Fact]
        public void MinListIsOk()
        {
            var timeSeriesDataList = new List<ITimeSeriesData<double>>
            {
                TestData.TimeSeriesData,
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes, TestData.TimeSeriesData.Values.Select(v => v * 2).ToList()),
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(5.2)).ToList(), TestData.TimeSeriesData.Values)
            };

            var minData = timeSeriesDataList.Min();
            Assert.Equal(22, minData.data.DateTimes.Count);
            Assert.Equal(10, minData.data.Values[3]);
            Assert.Equal(8, minData.data.Values[20]);
        }

        [Fact]
        public void MinListFirstOnlyIsOk()
        {
            var timeSeriesDataList = new List<ITimeSeriesData<double>>
            {
                TestData.TimeSeriesData,
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes, TestData.TimeSeriesData.Values.Select(v => v * 2).ToList())
            };

            var data = timeSeriesDataList.Min(TimeStepsSelection.FirstOnly);
            Assert.Equal(TestData.TimeSeriesData.Count, data.data.DateTimes.Count);
            Assert.Equal(TestData.TimeSeriesData.Values[3], data.data.Values[3]);
        }

        [Fact]
        public void MinListCommonOnlyIsOk()
        {
            var timeSeriesDataList = new List<ITimeSeriesData<double>>
            {
                TestData.TimeSeriesData,
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values)
            };

            var data = timeSeriesDataList.Min(TimeStepsSelection.CommonOnly);
            Assert.Equal(7, data.data.Values[2]); // 10 (interpolated) > 7 = 7
            Assert.Equal(7, data.data.Values.Count);
        }

        [Fact]
        public void MaxListIsOk()
        {
            var timeSeriesDataList = new List<ITimeSeriesData<double>>
            {
                TestData.TimeSeriesData,
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes, TestData.TimeSeriesData.Values.Select(v => v * 2).ToList()),
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(5.2)).ToList(), TestData.TimeSeriesData.Values)
            };

            var data = timeSeriesDataList.Max();
            Assert.Equal(22, data.data.DateTimes.Count);
            Assert.Equal(20, data.data.Values[3]);
            Assert.Equal(23.2, data.data.Values[6]);
        }

        [Fact]
        public void MaxListFirstOnlyIsOk()
        {
            var timeSeriesDataList = new List<ITimeSeriesData<double>>
            {
                TestData.TimeSeriesData,
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes, TestData.TimeSeriesData.Values.Select(v => v * 2).ToList())
            };

            var data = timeSeriesDataList.Max(TimeStepsSelection.FirstOnly);
            Assert.Equal(TestData.TimeSeriesData.Count, data.data.DateTimes.Count);
            Assert.Equal(TestData.TimeSeriesData.Values[3] * 2, data.data.Values[3]);
        }

        [Fact]
        public void MaxListCommonOnlyIsOk()
        {
            var timeSeriesDataList = new List<ITimeSeriesData<double>>
            {
                TestData.TimeSeriesData,
                new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values)
            };

            var data = timeSeriesDataList.Max(TimeStepsSelection.CommonOnly);
            Assert.Equal(10, data.data.Values[2]); // Interpolates
            Assert.Equal(7, data.data.Values.Count);
            Assert.Equal(2, data.interpolatedCount);
        }

        [Fact]
        public void MultiplyWithIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            var (multipliedData, found, notFound) = TestData.TimeSeriesData.MultiplyWith(other);
            Assert.Equal(7, multipliedData.Values.Count);
            Assert.Equal(7, found);
            Assert.Equal(4, notFound);
            Assert.Equal(10, multipliedData.Values[6]);
        }

        [Fact]
        public void DivideWithIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            var (dividedData, found, notFound) = TestData.TimeSeriesData.DivideWith(other);
            Assert.Equal(7, dividedData.Values.Count);
            Assert.Equal(7, found);
            Assert.Equal(4, notFound);
            Assert.Equal(0.8, dividedData.Values[3]);
        }

        [Fact]
        public void AddWithIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            var (addedData, found, notFound) = TestData.TimeSeriesData.AddWith(other);
            Assert.Equal(7, addedData.Values.Count);
            Assert.Equal(7, found);
            Assert.Equal(4, notFound);
            Assert.Equal(18, addedData.Values[3]);
        }

        [Fact]
        public void SubtractWithIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            var (multipliedData, found, notFound) = TestData.TimeSeriesData.SubtractWith(other);
            Assert.Equal(7, multipliedData.Values.Count);
            Assert.Equal(7, found);
            Assert.Equal(4, notFound);
            Assert.Equal(-2, multipliedData.Values[3]);
        }

        [Fact]
        public void MultiplyWithCommonOnlyIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.MultiplyWith(other, TimeStepsSelection.CommonOnly);

            Assert.Equal(7, data.Values.Count);
            Assert.Equal(12.0 * 6.0, data.Values[1]);
            Assert.Equal(10.0 * 7.0, data.Values[2]);
            Assert.Equal(10.0 * 10.0, data.Values[6]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(1, interpolatedCountOther);
            Assert.Equal(0, data.Values.Count(v => v is null));
        }

        [Fact]
        public void MultiplyWithCommonOnlyAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.MultiplyWith(other, TimeStepsSelection.CommonOnly, TimeSpan.FromDays(1));

            Assert.Equal(7, data.Values.Count);
            Assert.Equal(12.0 * 6.0, data.Values[1]);
            Assert.Null(data.Values[2]);
            Assert.Null(data.Values[6]);
            Assert.Equal(0, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(2, data.Values.Count(v => v is null));
        }

        [Fact]
        public void MultiplyWithAllIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.MultiplyWith(other, TimeStepsSelection.All);

            Assert.Equal(16, data.Values.Count);
            Assert.Equal(12.0 * 6.0, data.Values[6]);
            Assert.Equal(10.0 * 7.0, data.Values[7]);
            Assert.Equal(10.0 * 10.0, data.Values[11]);
            Assert.Equal(2, interpolatedCount);
            Assert.Equal(2, interpolatedCountOther);
            Assert.Equal(7, data.Values.Count(v => v is null));
        }

        [Fact]
        public void MultiplyWithAllAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.MultiplyWith(other, TimeStepsSelection.All, TimeSpan.FromDays(1));

            Assert.Equal(16, data.Values.Count);
            Assert.Equal(12.0 * 6.0, data.Values[6]);
            Assert.Null(data.Values[7]);
            Assert.Null(data.Values[11]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(10, data.Values.Count(v => v is null));
        }

        [Fact]
        public void MultiplyWithFirstOnlyIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.MultiplyWith(other, TimeStepsSelection.FirstOnly);

            Assert.Equal(11, data.Values.Count);
            Assert.Equal(12.0 * 6.0, data.Values[5]);
            Assert.Equal(10.0 * 7.0, data.Values[6]);
            Assert.Equal(10.0 * 10.0, data.Values[10]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(2, interpolatedCountOther);
            Assert.Equal(3, data.Values.Count(v => v is null));
        }

        [Fact]
        public void MultiplyWithFirstOnlyAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.MultiplyWith(other, TimeStepsSelection.FirstOnly, TimeSpan.FromDays(1));

            Assert.Equal(11, data.Values.Count);
            Assert.Equal(12.0 * 6.0, data.Values[5]);
            Assert.Null(data.Values[6]);
            Assert.Null(data.Values[10]);
            Assert.Equal(0, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(6, data.Values.Count(v => v is null));
        }

        [Fact]
        public void DivideWithCommonOnlyIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.DivideWith(other, TimeStepsSelection.CommonOnly);

            Assert.Equal(7, data.Values.Count);
            Assert.Equal(12.0 / 6.0, data.Values[1]);
            Assert.Equal(10.0 / 7.0, data.Values[2]);
            Assert.Equal(10.0 / 10.0, data.Values[6]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(1, interpolatedCountOther);
            Assert.Equal(0, data.Values.Count(v => v is null));
        }

        [Fact]
        public void DivideWithCommonOnlyAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.DivideWith(other, TimeStepsSelection.CommonOnly, TimeSpan.FromDays(1));

            Assert.Equal(7, data.Values.Count);
            Assert.Equal(12.0 / 6.0, data.Values[1]);
            Assert.Null(data.Values[2]);
            Assert.Null(data.Values[6]);
            Assert.Equal(0, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(2, data.Values.Count(v => v is null));
        }

        [Fact]
        public void DivideWithAllIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.DivideWith(other, TimeStepsSelection.All);

            Assert.Equal(16, data.Values.Count);
            Assert.Equal(12.0 / 6.0, data.Values[6]);
            Assert.Equal(10.0 / 7.0, data.Values[7]);
            Assert.Equal(10.0 / 10.0, data.Values[11]);
            Assert.Equal(2, interpolatedCount);
            Assert.Equal(2, interpolatedCountOther);
            Assert.Equal(7, data.Values.Count(v => v is null));
        }

        [Fact]
        public void DivideWithAllAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.DivideWith(other, TimeStepsSelection.All, TimeSpan.FromDays(1));

            Assert.Equal(16, data.Values.Count);
            Assert.Equal(12.0 / 6.0, data.Values[6]);
            Assert.Null(data.Values[7]);
            Assert.Null(data.Values[11]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(10, data.Values.Count(v => v is null));
        }

        [Fact]
        public void DivideWithFirstOnlyIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.DivideWith(other, TimeStepsSelection.FirstOnly);

            Assert.Equal(11, data.Values.Count);
            Assert.Equal(12.0 / 6.0, data.Values[5]);
            Assert.Equal(10.0 / 7.0, data.Values[6]);
            Assert.Equal(10.0 / 10.0, data.Values[10]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(2, interpolatedCountOther);
            Assert.Equal(3, data.Values.Count(v => v is null));
        }

        [Fact]
        public void DivideWithFirstOnlyAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.DivideWith(other, TimeStepsSelection.FirstOnly, TimeSpan.FromDays(1));

            Assert.Equal(11, data.Values.Count);
            Assert.Equal(12.0 / 6.0, data.Values[5]);
            Assert.Null(data.Values[6]);
            Assert.Null(data.Values[10]);
            Assert.Equal(0, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(6, data.Values.Count(v => v is null));
        }

        [Fact]
        public void AddWithCommonOnlyIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.AddWith(other, TimeStepsSelection.CommonOnly);

            Assert.Equal(7, data.Values.Count);
            Assert.Equal(12.0 + 6.0, data.Values[1]);
            Assert.Equal(10.0 + 7.0, data.Values[2]);
            Assert.Equal(10.0 + 10.0, data.Values[6]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(1, interpolatedCountOther);
            Assert.Equal(0, data.Values.Count(v => v is null));
        }

        [Fact]
        public void AddWithCommonOnlyAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.AddWith(other, TimeStepsSelection.CommonOnly, TimeSpan.FromDays(1));

            Assert.Equal(7, data.Values.Count);
            Assert.Equal(12.0 + 6.0, data.Values[1]);
            Assert.Null(data.Values[2]);
            Assert.Null(data.Values[6]);
            Assert.Equal(0, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(2, data.Values.Count(v => v is null));
        }

        [Fact]
        public void AddWithAllIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.AddWith(other, TimeStepsSelection.All);

            Assert.Equal(16, data.Values.Count);
            Assert.Equal(12.0 + 6.0, data.Values[6]);
            Assert.Equal(10.0 + 7.0, data.Values[7]);
            Assert.Equal(10.0 + 10.0, data.Values[11]);
            Assert.Equal(2, interpolatedCount);
            Assert.Equal(2, interpolatedCountOther);
            Assert.Equal(7, data.Values.Count(v => v is null));
        }

        [Fact]
        public void AddWithAllAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.AddWith(other, TimeStepsSelection.All, TimeSpan.FromDays(1));

            Assert.Equal(16, data.Values.Count);
            Assert.Equal(12.0 + 6.0, data.Values[6]);
            Assert.Null(data.Values[7]);
            Assert.Null(data.Values[11]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(10, data.Values.Count(v => v is null));
        }

        [Fact]
        public void AddWithFirstOnlyIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.AddWith(other, TimeStepsSelection.FirstOnly);

            Assert.Equal(11, data.Values.Count);
            Assert.Equal(12.0 + 6.0, data.Values[5]);
            Assert.Equal(10.0 + 7.0, data.Values[6]);
            Assert.Equal(10.0 + 10.0, data.Values[10]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(2, interpolatedCountOther);
            Assert.Equal(3, data.Values.Count(v => v is null));
        }

        [Fact]
        public void AddWithFirstOnlyAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.AddWith(other, TimeStepsSelection.FirstOnly, TimeSpan.FromDays(1));

            Assert.Equal(11, data.Values.Count);
            Assert.Equal(12.0 + 6.0, data.Values[5]);
            Assert.Null(data.Values[6]);
            Assert.Null(data.Values[10]);
            Assert.Equal(0, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(6, data.Values.Count(v => v is null));
        }

        [Fact]
        public void SubtractWithCommonOnlyIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.SubtractWith(other, TimeStepsSelection.CommonOnly);

            Assert.Equal(7, data.Values.Count);
            Assert.Equal(12.0 - 6.0, data.Values[1]);
            Assert.Equal(10.0 - 7.0, data.Values[2]);
            Assert.Equal(10.0 - 10.0, data.Values[6]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(1, interpolatedCountOther);
            Assert.Equal(0, data.Values.Count(v => v is null));
        }

        [Fact]
        public void SubtractWithCommonOnlyAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.SubtractWith(other, TimeStepsSelection.CommonOnly, TimeSpan.FromDays(1));

            Assert.Equal(7, data.Values.Count);
            Assert.Equal(12.0 - 6.0, data.Values[1]);
            Assert.Null(data.Values[2]);
            Assert.Null(data.Values[6]);
            Assert.Equal(0, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(2, data.Values.Count(v => v is null));
        }

        [Fact]
        public void SubtractWithAllIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.SubtractWith(other, TimeStepsSelection.All);

            Assert.Equal(16, data.Values.Count);
            Assert.Equal(12.0 - 6.0, data.Values[6]);
            Assert.Equal(10.0 - 7.0, data.Values[7]);
            Assert.Equal(10.0 - 10.0, data.Values[11]);
            Assert.Equal(2, interpolatedCount);
            Assert.Equal(2, interpolatedCountOther);
            Assert.Equal(7, data.Values.Count(v => v is null));
        }

        [Fact]
        public void SubtractWithAllAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.SubtractWith(other, TimeStepsSelection.All, TimeSpan.FromDays(1));

            Assert.Equal(16, data.Values.Count);
            Assert.Equal(12.0 - 6.0, data.Values[6]);
            Assert.Null(data.Values[7]);
            Assert.Null(data.Values[11]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(10, data.Values.Count(v => v is null));
        }

        [Fact]
        public void SubtractWithFirstOnlyIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.SubtractWith(other, TimeStepsSelection.FirstOnly);

            Assert.Equal(11, data.Values.Count);
            Assert.Equal(12.0 - 6.0, data.Values[5]);
            Assert.Equal(10.0 - 7.0, data.Values[6]);
            Assert.Equal(10.0 - 10.0, data.Values[10]);
            Assert.Equal(1, interpolatedCount);
            Assert.Equal(2, interpolatedCountOther);
            Assert.Equal(3, data.Values.Count(v => v is null));
        }

        [Fact]
        public void SubtractWithFirstOnlyAndGapToleranceIsOk()
        {
            var other = new TimeSeriesData(TestData.TimeSeriesData.DateTimes.Select(d => d.AddDays(4)).ToList(), TestData.TimeSeriesData.Values);
            other.Insert(new DateTime(2000, 1, 3, 12, 0, 0), 6.0);
            var (data, interpolatedCount, interpolatedCountOther) = TestData.TimeSeriesData.SubtractWith(other, TimeStepsSelection.FirstOnly, TimeSpan.FromDays(1));

            Assert.Equal(11, data.Values.Count);
            Assert.Equal(12.0 - 6.0, data.Values[5]);
            Assert.Null(data.Values[6]);
            Assert.Null(data.Values[10]);
            Assert.Equal(0, interpolatedCount);
            Assert.Equal(0, interpolatedCountOther);
            Assert.Equal(6, data.Values.Count(v => v is null));
        }

        [Fact]
        public void ReplaceOutsideIntervalThrowsIfIllegalInterval()
        {
            Assert.Throws<ArgumentException>(() => TestData.TimeSeriesData.Replace(999, 10, 5));
        }

        [Fact]
        public void ReplaceOutsideIntervalThrowsIfNoInterval()
        {
            Assert.Throws<ArgumentException>(() => TestData.TimeSeriesData.Replace(999));
        }

        [Fact]
        public void ReplaceOutsideIntervalIsOk()
        {
            var (replacedSeries, replacedCount) = TestData.TimeSeriesData.Replace(null, 6, 9);
            Assert.Equal(4, replacedCount);
            Assert.True(replacedSeries.Values.Max() <= 9);
            Assert.True(replacedSeries.Values.Min() >= 6);
        }

        [Fact]
        public void ReplaceBelowIsOk()
        {
            var (replacedSeries, replacedCount) = TestData.TimeSeriesData.Replace(null, 6);
            Assert.Equal(1, replacedCount);
            Assert.Equal(12, replacedSeries.Values.Max());
            Assert.True(replacedSeries.Values.Min() >= 6);
        }

        [Fact]
        public void ReplaceAboveIsOk()
        {
            var (replacedSeries, replacedCount) = TestData.TimeSeriesData.Replace(null, above:9);
            Assert.Equal(3, replacedCount);
            Assert.True(replacedSeries.Values.Max() <= 9);
            Assert.Equal(5, replacedSeries.Values.Min());
        }

        [Fact]
        public void ReplaceIsOk()
        {
            var (replacedSeries, replacedCount) = TestData.TimeSeriesData.Replace(12, null);
            Assert.Equal(1, replacedCount);
            Assert.Null(replacedSeries.Get(new DateTime(2000, 1, 6)).Value.Value);
        }
    }
}