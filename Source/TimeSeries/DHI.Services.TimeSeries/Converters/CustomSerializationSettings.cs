namespace DHI.Services.TimeSeries.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public static class CustomSerializationSettings
    {
        public static bool UseNullForNaN { get; set; } = false;
    }
}
