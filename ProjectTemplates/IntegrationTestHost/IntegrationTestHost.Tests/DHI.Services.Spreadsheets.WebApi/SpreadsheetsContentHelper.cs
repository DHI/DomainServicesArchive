namespace IntegrationTestHost.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public static class SpreadsheetsContentHelper
    {
        public static StringContent GetStringContent<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj, DHI.Services.Spreadsheets.WebApi.SerializerOptionsDefault.Options);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }
    }
}
