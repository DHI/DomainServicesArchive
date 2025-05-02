using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DHI.Services.TimeSeries.Test")]
namespace DHI.Services.TimeSeries.Daylight
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;

    /// <summary>
    ///     This repository calculates the sunrise and sunset times given a location longitude and latitude, provided a timespan.
    ///     The output times can either be in UTC time or in a specified timezone.
    ///     The timeseries values includes 1s for daytime (sunrise to sunset) and 0s for nighttime (sunset to sunrise).
    ///     Calculations and explanation of the code/math involved can be found here:
    ///     https://squarewidget.com/solar-coordinates/
    ///     https://en.wikipedia.org/wiki/Position_of_the_Sun
    ///     https://en.wikipedia.org/wiki/Sunrise_equation
    /// </summary>
    public class TimeSeriesRepository : BaseTimeSeriesRepository<string, double>
    {
        public override Maybe<TimeSeries<string, double>> Get(string id, ClaimsPrincipal user = null)
        {
            throw new NotSupportedException("This timeseries is created on the fly, all the meta data is contained within the id.");
        }

        public override Maybe<DataPoint<double>> GetValue(string id, DateTime dateTime, ClaimsPrincipal user = null)
        {
            Guard.Against.NullOrWhiteSpace(id, nameof(id));

            var times = GetValues(id, dateTime.AddDays(-1), dateTime.AddDays(1)).Value.ToSortedSet();
            return times.Last(t => t.DateTime < dateTime).ToMaybe();
        }

        public override Maybe<ITimeSeriesData<double>> GetValues(string id, ClaimsPrincipal user = null)
        {
            throw new NotSupportedException("You cannot ask for all values in this type of time series. Please specify start and end date");
        }

        /// <inheritdoc />
        public override Maybe<ITimeSeriesData<double>> GetValues(string id, DateTime from, DateTime to, ClaimsPrincipal user = null)
        {
            Guard.Against.NullOrWhiteSpace(id, nameof(id));
            var timeSeriesId = new TimeSeriesId(id);
            return ReadTimeSeries(timeSeriesId, from, to).ToMaybe();
        }

        internal ITimeSeriesData<double> ReadTimeSeries(TimeSeriesId timeSeriesId, DateTime from, DateTime to)
        {
            var startTime = TimeZoneInfo.ConvertTimeToUtc(from, timeSeriesId.TimeZoneFrom);
            var totalDays = (int)(to - from).TotalDays + 1;
            var times = new List<DateTime>(totalDays);
            var values = new List<double>(totalDays);
            for (var day = 0; day < totalDays; day++)
            {
                var julianDay = CalculateJulianDay(startTime.AddDays(day));
                var sunriseMinutes = CalculateSunriseUTC(julianDay, timeSeriesId.SunZenith, timeSeriesId.Latitude, timeSeriesId.Longitude);
                var sunrise = GetDateTimeObject(sunriseMinutes, startTime.Date.AddDays(day));
                times.Add(sunrise);
                values.Add(timeSeriesId.DayValue);
                var sunsetMinutes = CalculateSunsetUTC(julianDay, timeSeriesId.SunZenith, timeSeriesId.Latitude, timeSeriesId.Longitude);
                var sunset = GetDateTimeObject(sunsetMinutes, startTime.Date.AddDays(day));
                times.Add(sunset);
                values.Add(timeSeriesId.NightValue);
            }

            times = times.Select(t => TimeZoneInfo.ConvertTimeFromUtc(t, timeSeriesId.TimeZoneTo)).ToList();
            return new TimeSeriesData<double>(times, values);
        }

        /// <summary>
        ///     Calculates Julian day from calendar day
        /// </summary>
        /// <param name="year"> 4 digit year</param>
        /// <param name="month">January = 1	</param>
        /// <param name="day">1 - 31</param>
        /// <returns>The Julian day corresponding to the date</returns>
        /// <remarks>Number is returned for start of day. Fractional days should be	added later.</remarks>
        internal static double CalculateJulianDay(int year, int month, int day)
        {
            if (month <= 2)
            {
                year -= 1;
                month += 12;
            }

            var a = Math.Floor(year / 100.0); // used to make return statement shorter
            var b = 2 - a + Math.Floor(a / 4);

            return Math.Floor(365.25 * (year + 4716)) + Math.Floor(30.6001 * (month + 1)) + day + b - 1524.5;
        }

        internal static double CalculateJulianDay(DateTime date)
        {
            return CalculateJulianDay(date.Year, date.Month, date.Day);
        }

        /// <summary>
        ///     convert Julian Day to centuries since J2000.0.
        /// </summary>
        /// <param name="julianDay">the Julian Day to convert</param>
        /// <returns>the T value corresponding to the Julian Day	</returns>
        internal static double CalculateJulianCentury(double julianDay)
        {
            return (julianDay - 2451545.0) / 36525.0;
        }

        /// <summary>
        ///     calculate the Geometric Mean Longitude of the Sun
        /// </summary>
        /// <param name="julianCentury">number of Julian centuries since J2000.0	</param>
        /// <returns>the Geometric Mean Longitude of the Sun in degrees	</returns>
        internal static double CalculateGeometricMeanLongitude(double julianCentury)
        {
            var l0 = 280.46646 + julianCentury * (36000.76983 + 0.0003032 * julianCentury);
            while (l0 < 0.0)
            {
                l0 += 360.0;
            }

            return l0 % 360; // in degrees
        }

        /// <summary>
        ///     calculate the Geometric Mean Anomaly of the Sun
        /// </summary>
        /// <param name="julianCentury">number of Julian centuries since J2000.0	</param>
        /// <returns>the Geometric Mean Anomaly of the Sun in degrees	</returns>
        internal static double CalculateGeometricMeanAnomaly(double julianCentury)
        {
            return 357.52911 + julianCentury * (35999.05029 - 0.0001537 * julianCentury); // m in deg
        }

        /// <summary>
        ///     calculate the eccentricity of earth's orbit
        /// </summary>
        /// <param name="julianCentury">number of Julian centuries since J2000.0	</param>
        /// <returns>the unitless eccentricity	</returns>
        internal static double CalculateEccentricityEarthOrbit(double julianCentury)
        {
            return 0.016708634 - julianCentury * (0.000042037 + 0.0000001267 * julianCentury); // e unitless
        }

        /// <summary>
        ///     calculate the equation of center for the sun
        /// </summary>
        /// <param name="julianCentury"> number of Julian centuries since J2000.0	</param>
        /// <returns></returns>
        internal static double SolveCenterOfSunEquation(double julianCentury)
        {
            var m = CalculateGeometricMeanAnomaly(julianCentury);

            var mRadians = DegToRad(m);
            var sinM = Math.Sin(mRadians);
            var sin2M = Math.Sin(2 * mRadians);
            var sin3M = Math.Sin(3 * mRadians);

            return sinM * (1.914602 - julianCentury * (0.004817 + 0.000014 * julianCentury)) + sin2M * (0.019993 - 0.000101 * julianCentury) + sin3M * 0.000289; //in deg
        }

        /// <summary>
        ///     calculate the true longitude of the sun
        /// </summary>
        /// <param name="julianCentury">number of Julian centuries since J2000.0	</param>
        /// <returns>sun's true longitude in degrees	</returns>
        internal static double CalculateTrueLongitude(double julianCentury)
        {
            return CalculateGeometricMeanLongitude(julianCentury) + SolveCenterOfSunEquation(julianCentury);
        }

        /// <summary>
        ///     calculate the apparent longitude of the sun
        /// </summary>
        /// <param name="julianCentury">number of Julian centuries since J2000.0	</param>
        /// <returns>sun's apparent longitude in degrees	</returns>
        internal static double CalculateApparentLongitude(double julianCentury)
        {
            var omega = 125.04 - 1934.136 * julianCentury;
            return CalculateTrueLongitude(julianCentury) - 0.00569 - 0.00478 * Math.Sin(DegToRad(omega)); // in degrees
        }

        /// <summary>
        ///     calculate the mean obliquity of the ecliptic
        /// </summary>
        /// <param name="julianCentury">number of Julian centuries since J2000.0	</param>
        /// <returns>mean obliquity in degrees	</returns>
        internal static double CalculateMeanObliquityOfEcliptic(double julianCentury)
        {
            var seconds = 21.448 - julianCentury * (46.8150 + julianCentury * (0.00059 - julianCentury * 0.001813));
            return 23.0 + (26.0 + seconds / 60.0) / 60.0; // e0 in degrees
        }

        /// <summary>
        ///     calculate the corrected obliquity of the ecliptic
        /// </summary>
        /// <param name="julianCentury">number of Julian centuries since J2000.0	</param>
        /// <returns>corrected obliquity in degrees</returns>
        internal static double CalculateObliquityCorrection(double julianCentury)
        {
            var e0 = CalculateMeanObliquityOfEcliptic(julianCentury);

            var omega = 125.04 - 1934.136 * julianCentury;
            return e0 + 0.00256 * Math.Cos(DegToRad(omega)); // in degrees
        }

        /// <summary>
        ///     calculate the declination of the sun
        /// </summary>
        /// <param name="julianCentury">number of Julian centuries since J2000.0</param>
        /// <returns>sun's declination in degrees</returns>
        internal static double CalculateDeclination(double julianCentury)
        {
            var e = CalculateObliquityCorrection(julianCentury);
            var lambda = CalculateApparentLongitude(julianCentury);

            return RadToDeg(Math.Asin(Math.Sin(DegToRad(e)) * Math.Sin(DegToRad(lambda)))); // in degrees
        }

        /// <summary>
        ///     calculate the difference between true solar time and mean
        /// </summary>
        /// <param name="julianCentury"> number of Julian centuries since J2000.0	</param>
        /// <returns>equation of time in minutes of time	</returns>
        internal static double CalculateEquationOfTime(double julianCentury)
        {
            var epsilon = CalculateObliquityCorrection(julianCentury);
            var l0 = CalculateGeometricMeanLongitude(julianCentury);
            var e = CalculateEccentricityEarthOrbit(julianCentury);
            var m = CalculateGeometricMeanAnomaly(julianCentury);

            var y = Math.Pow(Math.Tan(DegToRad(epsilon) / 2.0), 2); // used to make the return statement more readable

            var sin2L0 = Math.Sin(2.0 * DegToRad(l0));
            var sinM = Math.Sin(DegToRad(m));
            var cos2L0 = Math.Cos(2.0 * DegToRad(l0));
            var sin4L0 = Math.Sin(4.0 * DegToRad(l0));
            var sin2M = Math.Sin(2.0 * DegToRad(m));

            var equationOfTime = y * sin2L0 - 2.0 * e * sinM + 4.0 * e * y * sinM * cos2L0 - 0.5 * y * y * sin4L0 - 1.25 * e * e * sin2M;

            return RadToDeg(equationOfTime) * 4.0; // in minutes of time
        }

        /// <summary>
        ///     calculate the hour angle of the sun at sunrise for the
        /// </summary>
        /// <param name="sunZenith">Sun's zenith in degrees. Default 90.833 degrees</param>
        /// <param name="latitude">latitude of observer in degrees	</param>
        /// <param name="solarDeclination">declination angle of sun in degrees	</param>
        /// <returns>hour angle of sunrise in radians	</returns>
        internal static double CalculateHourAngleSunrise(double sunZenith, double latitude, double solarDeclination)
        {
            return Math.Acos(Math.Cos(DegToRad(sunZenith)) / (Math.Cos(DegToRad(latitude)) * Math.Cos(DegToRad(solarDeclination))) - Math.Tan(DegToRad(latitude)) * Math.Tan(DegToRad(solarDeclination))); // in radians
        }

        /// <summary>
        ///     calculate the Universal Coordinated Time (UTC) of sunset for the given day at the given location on earth
        /// </summary>
        /// <param name="julianDay">Julian day</param>
        /// <param name="sunZenith">Sun's zenith in degrees. Default 90.833 degrees</param>
        /// <param name="latitude">Latitude of observer in degrees</param>
        /// <param name="longitude">Longitude of observer in degrees</param>
        /// <returns>time in minutes from zero Z	</returns>
        internal static double CalculateSunsetUTC(double julianDay, double sunZenith, double latitude, double longitude)
        {
            var julianCentury = CalculateJulianCentury(julianDay);
            var equationOfTime = CalculateEquationOfTime(julianCentury);
            var solarDeclination = CalculateDeclination(julianCentury);
            var hourAngle = CalculateHourAngleSunrise(sunZenith, latitude, solarDeclination);
            hourAngle = -hourAngle;
            var delta = longitude + RadToDeg(hourAngle);
            return 720 - 4.0 * delta - equationOfTime; // in minutes
        }

        internal static double CalculateSunriseUTC(double julianDay, double sunZenith, double latitude, double longitude)
        {
            var julianCentury = CalculateJulianCentury(julianDay);
            var equationOfTime = CalculateEquationOfTime(julianCentury);
            var solarDeclination = CalculateDeclination(julianCentury);
            var hourAngle = CalculateHourAngleSunrise(sunZenith, latitude, solarDeclination);
            var delta = longitude + RadToDeg(hourAngle);
            return 720 - 4.0 * delta - equationOfTime; // in minutes
        }

        internal static DateTime GetDateTimeObject(double time, DateTime date)
        {
            return date.AddMinutes(time);
        }

        private static double RadToDeg(double angleRad)
        {
            return 180.0 * angleRad / Math.PI;
        }

        private static double DegToRad(double angleDeg)
        {
            return Math.PI * angleDeg / 180.0;
        }
    }
}