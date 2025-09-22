namespace AuthorizationServer.Test
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;

    public static class ContentHelper
    {
        public static StringContent GetStringContent(object obj)
        => new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
    }
}
