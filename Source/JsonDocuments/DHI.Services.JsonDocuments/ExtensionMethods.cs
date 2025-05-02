namespace DHI.Services.JsonDocuments
{

    /// <summary>
    ///     Various generic extension methods.
    /// </summary>
    public static class ExtensionMethods
    {


        /// <summary>
        ///     Converts the object array to a DataTable object.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>DataTable.</r
        public static Argon.JObject Filter(this Argon.JObject obj, params string[] selects)
        {
            var result = (Argon.JContainer)new Argon.JObject();

            foreach (var select in selects)
            {
                var token = obj.SelectToken(select);
                if (token == null)
                    continue;

                result.Merge(GetNewParent(token.Parent));
            }

            return (Argon.JObject)result;
        }
        private static Argon.JToken GetNewParent(Argon.JToken token)
        {
            var result = new Argon.JObject(token);

            var parent = token;
            while ((parent = parent.Parent) != null)
            {
                if (parent is Argon.JProperty property)
                    result = new Argon.JObject(new Argon.JProperty(property.Name, result));
            }

            return result;
        }
        
    }
}