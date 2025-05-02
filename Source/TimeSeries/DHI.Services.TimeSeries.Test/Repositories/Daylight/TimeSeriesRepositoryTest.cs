namespace DHI.Services.TimeSeries.Test.Repositories.Daylight
{
    using System;
    using DHI.Services.TimeSeries.Daylight;
    using Xunit;

    public class TimeSeriesRepositoryTest
    {
        [Fact]
        public void ImplementsInterface()
        {
            Assert.IsAssignableFrom<ICoreTimeSeriesRepository<string, double>>(new TimeSeriesRepository());
        }

        [Fact]
        public void GetThrows()
        {
            const string id = "Latitude=-28.016667;Longitude=153.4;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<NotSupportedException>(() => repository.Get(id));
        }

        [Fact]
        public void GetValuesWithEmptyIdThrows()
        {
            const string id = "";
            var repository = new TimeSeriesRepository();
            Assert.Throws<ArgumentException>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void GetValuesWithNullIdThrows()
        {
            const string id = null;
            var repository = new TimeSeriesRepository();
            Assert.Throws<ArgumentNullException>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void GetDayValueIsOk()
        {
            const string id = "Latitude=-28.016667;Longitude=153.4;TimeZoneTo=E. Australia Standard Time;TimeZoneFrom=UTC";
            var repository = new TimeSeriesRepository();
            var dataPoint = repository.GetValue(id, new DateTime(2020, 6, 23, 13, 00, 00)).Value;

            Assert.Equal(1, dataPoint.Value);
        }

        [Fact]
        public void GetNightValueIsOk()
        {
            const string id = "Latitude=-28.016667;Longitude=153.4;TimeZoneTo=E. Australia Standard Time;TimeZoneFrom=UTC";
            var repository = new TimeSeriesRepository();
            var dataPoint = repository.GetValue(id, new DateTime(2020, 6, 23, 01, 00, 00)).Value;

            Assert.Equal(0, dataPoint.Value);
        }

        [Fact]
        public void GetValuesIsOk()
        {
            const string id = "Latitude=-28.016667;Longitude=153.4;TimeZoneTo=E. Australia Standard Time;TimeZoneFrom=UTC";
            var repository = new TimeSeriesRepository();
            var data = repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)).Value;

            Assert.Equal(new DateTime(2020, 6, 23, 6, 37, 43, 140), data.DateTimes[0]);
            Assert.Equal(new DateTime(2020, 6, 23, 16, 59, 32, 835), data.DateTimes[1]);
        }

        [Fact]
        public void GetDayValuesDifferentZenithIsOk()
        {
            const string id = "Latitude=-28.016667;Longitude=153.4;TimeZoneTo=E. Australia Standard Time;TimeZoneFrom=UTC;SunZenith=106";
            var repository = new TimeSeriesRepository();
            var data = repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)).Value;

            Assert.Equal(new DateTime(2020, 6, 23, 5, 23, 25, 461), data.DateTimes[0]);
            Assert.Equal(new DateTime(2020, 6, 23, 18, 13, 50, 514), data.DateTimes[1]);
        }

        [Fact]
        public void GetValuesWithCustomNightAndDayValuesIsOk()
        {
            const string id = "Latitude=-28.016667;NightValue=10.2;DayValue=-99;Longitude=153.4;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            var data = repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)).Value;

            Assert.Equal(-99, data.Values[0]);
            Assert.Equal(10.2, data.Values[1]);
        }

        [Fact]
        public void BadLatitudeThrows()
        {
            const string id = "Latitude=hello;Longitude=153.4;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();

            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void MissingLatitudeThrows()
        {
            const string id = "Longitude=153.4;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();

            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void BadLongitudeThrows()
        {
            const string id = "Latitude=-28.016667;Longitude=vCool;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void MissingLongitudeThrows()
        {
            const string id = "Latitude=-28.016667;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void BadNightValuesThrows()
        {
            const string id = "Latitude=-28.016667;NightValue=wrong;DayValue=-99;Longitude=153.4;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void BadDayValuesThrows()
        {
            const string id = "Latitude=-28.016667;DayValue=BadDay;Longitude=153.4;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void TimeZoneFromThrows()
        {
            const string id = "Latitude=-28.016667;NightValue=10.2;DayValue=-99;Longitude=153.4;TimeZoneFrom=East. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void TimeZoneToThrows()
        {
            const string id = "Latitude=-28.016667;Longitude=153.4;TimeZoneTo=East. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void DefaultsIsOk()
        {
            const string id = "NotARealArg=-28.016667;Longitude=153.4;TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void MissingLatLngThrows()
        {
            const string id = "TimeZoneTo=E. Australia Standard Time";
            var repository = new TimeSeriesRepository();
            Assert.Throws<Exception>(() => repository.GetValues(id, new DateTime(2020, 6, 23), new DateTime(2020, 6, 23)));
        }

        [Fact]
        public void CalculateJulianTimesIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            Assert.Equal(2458849.5, jDay);

            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);
            Assert.InRange(jCent, 0.1999, 0.2);
        }

        [Fact]
        public static void CalcGeometricMeanLongitudeIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.Equal(280, (int)TimeSeriesRepository.CalculateGeometricMeanLongitude(jCent));
            Assert.Equal(182, (int)TimeSeriesRepository.CalculateGeometricMeanLongitude(-950));
        }

        [Fact]
        public static void CalcGeometricMeanAnomalyIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.Equal(7556, (int)TimeSeriesRepository.CalculateGeometricMeanAnomaly(jCent));
        }

        [Fact]
        public static void CalcEccentricityEarthOrbitIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.InRange(TimeSeriesRepository.CalculateEccentricityEarthOrbit(jCent), 0.01, 0.02);
        }

        [Fact]
        public static void SolveCenterOfSunEquationIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.InRange(TimeSeriesRepository.SolveCenterOfSunEquation(jCent), -0.11, -0.09);
        }

        [Fact]
        public static void CalcTrueLongitudeIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.Equal(280, (int)TimeSeriesRepository.CalculateTrueLongitude(jCent));
        }

        [Fact]
        public static void CalcApparentLongitudeIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.Equal(280, (int)TimeSeriesRepository.CalculateApparentLongitude(jCent));
        }

        [Fact]
        public static void CalcObliquityCorrectionIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.Equal(23, (int)TimeSeriesRepository.CalculateObliquityCorrection(jCent));
        }

        [Fact]
        public static void CalcDeclinationIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.Equal(-23, (int)TimeSeriesRepository.CalculateDeclination(jCent));
        }

        [Fact]
        public static void CalcEquationOfTimeIsOk()
        {
            var jDay = TimeSeriesRepository.CalculateJulianDay(2020, 01, 01);
            var jCent = TimeSeriesRepository.CalculateJulianCentury(jDay);

            Assert.Equal(-3, (int)TimeSeriesRepository.CalculateEquationOfTime(jCent));
        }
    
        [Fact]
        public static void GetDateTimeIsOk()
        {
            var dt = new DateTime(2020, 01, 01, 00, 00,00);
            Assert.Equal(dt.AddMinutes(60), TimeSeriesRepository.GetDateTimeObject(60, dt));
        }
    }
}