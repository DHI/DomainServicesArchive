namespace DHI.Services.Jobs
{
    using Argon;

    internal static class ExtensionMethods
    {
        public static JObject Filter(this JObject obj, params string[] selects)
        {
            var result = (JContainer)new JObject();

            foreach (var select in selects)
            {
                var token = obj.SelectToken(select);
                if (token == null)
                    continue;

                result.Merge(GetNewParent(token.Parent));
            }

            return (JObject)result;
        }

        private static JToken GetNewParent(JToken token)
        {
            var result = new JObject(token);

            var parent = token;
            while ((parent = parent.Parent) != null)
            {
                if (parent is JProperty property)
                    result = new JObject(new JProperty(property.Name, result));
            }

            return result;
        }
    }
}