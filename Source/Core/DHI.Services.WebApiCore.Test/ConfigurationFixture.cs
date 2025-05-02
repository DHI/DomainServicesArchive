namespace DHI.Services.WebApiCore.Test
{
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;

    public class ConfigurationFixture
    {
        public ConfigurationFixture()
        {
            var dictionary = new Dictionary<string, string>
            {
                {"Key1", "Value1"},
                {"Nested:Key1", "NestedValue1"},
                {"Nested:Key2", "NestedValue2"}
            };

            Configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(dictionary)
                .Build();
        }

        public IConfiguration Configuration { get; }
    }
}
