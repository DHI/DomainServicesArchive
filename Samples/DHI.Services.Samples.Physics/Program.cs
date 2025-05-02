namespace DHI.Services.Samples.Physics
{
    using System;
    using System.IO;
    using DHI.Physics;
    using DHI.Services.Physics;
    using Unit = DHI.Services.Physics.Unit;

    internal class Program
    {
        private static void Main()
        {
            // Wire up the unit service
            var filePath = Path.Combine(Path.GetTempPath(), "__units.json");
            var unitRepository = new UnitRepository(filePath);
            var unitService = new UnitService(unitRepository);

            // Create a few units
            var meter = new Unit("meter", "meter", "m", Dimension.Length);
            var kilometer = new Unit("kilometer", "kilometer", "km", 1000 * meter);
            var second = new Unit("second", "second", "s", Dimension.Time);
            var hour = new Unit("hour", "hour", "h", 60 * 60 * second);

            // Convert some values
            const double speedInMeterPrSecond = 10.0;
            var speedInKilometerPerHour = DHI.Physics.Unit.Convert(speedInMeterPrSecond, meter / second, kilometer / hour);
            Console.WriteLine("{0} m/s = {1} km/h", speedInMeterPrSecond, speedInKilometerPerHour);

            // Persist some units in a unit repository
            unitService.AddOrUpdate(meter);
            unitService.AddOrUpdate(kilometer);
            unitService.AddOrUpdate(second);
            unitService.AddOrUpdate(hour);
            var meterPerSecond = new Unit("meter/second", "meter/second", "m/s", meter /second);
            var kilometerPerHour = new Unit("kilometer/hour", "kilometer/hour", "km/h", kilometer /hour);
            unitService.AddOrUpdate(meterPerSecond);
            unitService.AddOrUpdate(kilometerPerHour);

            // Retrieve units from unit service and convert some values
            speedInKilometerPerHour = unitService.Convert(speedInMeterPrSecond, "meter/second", "kilometer/hour");
            Console.WriteLine("{0} m/s = {1} km/h", speedInMeterPrSecond, speedInKilometerPerHour);

            // Create and add some temperature units
            var kelvin = new Unit("kelvin", "kelvin", "K", Dimension.Temperature);
            var celcius = new Unit("celcius", "celcius", "C", Dimension.Temperature);
            var fahrenheit = new Unit("fahrenheit", "fahrenheit", "F", Dimension.Temperature);

            // Register some custom conversion functions for temperature units
            unitService.RegisterConversion("fahrenheit", "celcius", d => (d - 32.0) * 5.0 / 9.0);
            unitService.RegisterConversion("celcius", "fahrenheit", d => d * 9.0 / 5.0 + 32.0);
            unitService.RegisterConversion("celcius", "kelvin", d => d + 273.15);
            unitService.RegisterConversion("kelvin", "celcius", d => d - 273.15);
            unitService.RegisterConversion("fahrenheit", "kelvin", d => (d + 459.67) * 5.0 / 9.0);
            unitService.RegisterConversion("kelvin", "fahrenheit", d => d * 9.0 / 5.0 - 459.67);

            // Convert some temperatures
            const double temperatureInFahrenheit = 68;
            var temperatureInCelcius = unitService.Convert(temperatureInFahrenheit, "fahrenheit", "celcius");
            Console.WriteLine("{0} fahrenheit = {1} celcius", temperatureInFahrenheit, temperatureInCelcius);

            Console.ReadLine();
        }
    }
}