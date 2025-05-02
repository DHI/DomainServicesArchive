namespace DHI.Services.Physics
{
    using System;
    using System.Collections.Generic;

    public class UnitService : BaseUpdatableDiscreteService<Unit, string>
    {
        private readonly Dictionary<Tuple<string, string>, Func<double, double>> _conversions;

        public UnitService(IUnitRepository repository)
            : base(repository)
        {
            _conversions = new Dictionary<Tuple<string, string>, Func<double, double>>();
        }

        public double Convert(double value, string from, string to)
        {
            var key = new Tuple<string, string>(from, to);
            if (_conversions.TryGetValue(key, out var conversion))
            {
                return conversion.Invoke(value);
            }

            if (TryGet(from, out var fromUnit) && TryGet(to, out var toUnit))
            {
                return DHI.Physics.Unit.Convert(value, fromUnit, toUnit);
            }

            throw new ArgumentException($"Could not convert from \"{from}\" to \"{to}\"");
        }

        public void RegisterConversion(string from, string to, Func<double, double> conversion)
        {
            _conversions.Add(new Tuple<string, string>(from, to), conversion);
        }
    }
}