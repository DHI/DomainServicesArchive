namespace DHI.Services
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Various generic extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Overriding ToString() method of object to return as string determine by type value
        /// </summary>
        /// <param name="object">The object</param>
        /// <param name="culture"><see cref="System.Globalization.CultureInfo"/></param>
        /// <param name="unsupportedReturnString">Defined user specific string return if not found any type match</param>
        /// <returns></returns>
        public static string AsString(this object @object, System.Globalization.CultureInfo culture = null, string unsupportedReturnString = null)
        {
            if (@object == null) return string.Empty;

            culture ??= System.Globalization.CultureInfo.CurrentCulture;

            Type valueType = @object.GetType();

            string returnString = string.Empty;

            if (valueType == typeof(string))
                returnString = @object as string;
            else if (valueType == typeof(int) || valueType == typeof(decimal) ||
                valueType == typeof(double) || valueType == typeof(float) || valueType == typeof(Single))
                returnString = string.Format(culture.NumberFormat, "{0}", @object);
            else if (valueType == typeof(DateTime))
                returnString = string.Format(culture.DateTimeFormat, "{0}", @object);
            else if (valueType == typeof(bool) || valueType == typeof(Byte) || valueType.IsEnum)
                returnString = @object.ToString();
            else if (valueType == typeof(byte[]))
                returnString = Convert.ToBase64String(@object as byte[]);
            else if (valueType == typeof(Guid?))
            {
                if (@object == null)
                    returnString = string.Empty;
                else
                    return @object.ToString();
            }
            else
            {
                // Any type that supports a type converter
                TypeConverter converter = TypeDescriptor.GetConverter(valueType);
                if (converter != null && converter.CanConvertTo(typeof(string)) && converter.CanConvertFrom(typeof(string)))
                    returnString = converter.ConvertToString(null, culture, @object);
                else
                {
                    // Last resort - just call ToString() on unknown type
                    if (!string.IsNullOrEmpty(unsupportedReturnString))
                        returnString = unsupportedReturnString;
                    else
                        returnString = @object.ToString();
                }
            }

            return returnString;
        }

        /// <summary>
        ///     Check whether the given <paramref name="filePath"/> is full path.
        /// </summary>
        /// <param name="filePath">The file path. </param>
        /// <returns>If not full path, it will combine with base directory. eg. AppDomain.CurrentDomain.GetData("DataDirectory") or AppDomain.CurrentDomain.BaseDirectory</returns>
        public static string TryResolveFullPath(this string filePath)
        {
            if (isFullPath(filePath) == false)
            {
                var dataDirectory = AppDomain.CurrentDomain.GetData("DataDirectory") as string ?? AppDomain.CurrentDomain.BaseDirectory;

                if (filePath.StartsWith(@"[AppData]"))
                {
                    return filePath.Replace("[AppData]", $"{dataDirectory}\\");
                }
                return Path.Combine(dataDirectory, filePath);
            }

            return filePath;

            bool isFullPath(string path)
            {
                return !String.IsNullOrWhiteSpace(path)
                    && path.IndexOfAny(Path.GetInvalidPathChars().ToArray()) == -1
                    && Path.IsPathRooted(path)
                    && !Path.GetPathRoot(path).Equals(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal);
            }
        }

        /// <summary>
        ///     Resolve assembly name from <paramref name="type"/>
        /// </summary>
        /// <param name="type">The <seealso cref="System.Type"/></param>
        /// <param name="fqdn">Specify to get assembly fully qualified name or not</param> 
        /// <returns>Friendly name with format {<paramref name="type"/>.NameSpace}.{<paramref name="type"/>.Name}, {<paramref name="type"/>.NameSpace}</returns>
        public static string ResolveAssemblyName(this Type type, bool fqdn = false)
        {
            //return fqdn ? type.Assembly.FullName : type.Assembly.GetName().Name; 
            return type.Namespace.ToLowerInvariant().StartsWith("system") ? "mscorlib" : (fqdn ? type.Assembly.FullName : type.Assembly.GetName().Name);
        }

        /// <summary>
        ///     Resolve type friendly name from <paramref name="type"/>
        /// </summary>
        /// <param name="type">The <seealso cref="System.Type"/></param> 
        /// <returns>Friendly name with format {<paramref name="type"/>.NameSpace}.{<paramref name="type"/>.Name}, {<paramref name="type"/>.NameSpace}</returns>
        public static string ResolveTypeFriendlyName(this Type type)
        {
            return ResolveTypeFriendlyName(type, ("system", "mscorlib"));
        }

        /// <summary>
        ///     Resolve type friendly name from <paramref name="type"/>
        /// </summary>
        /// <param name="type">The <seealso cref="System.Type"/></param>
        /// <param name="namespaceReplace"></param>  
        /// <returns>Friendly name with format {<paramref name="type"/>.NameSpace}.{<paramref name="type"/>.Name}, {<paramref name="type"/>.NameSpace}</returns>
        public static string ResolveTypeFriendlyName(this Type type,
            (string replace, string replaceWith) namespaceReplace)
        {
            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (type.IsArray)
                return GetFriendlyNameOfArrayType(type);
            if (type.IsGenericType)
                return GetFriendlyNameOfGenericType(type);
            if (type.IsPointer)
                return GetFriendlyNameOfPointerType(type);
            return $"{type.Namespace}.{type.Name}";

            string GetFriendlyNameOfArrayType(Type type)
            {
                var arrayMarker = string.Empty;
                while (type.IsArray)
                {
                    var commas = new string(Enumerable.Repeat(',', type.GetArrayRank() - 1).ToArray());
                    arrayMarker += $"[{commas}]";
                    type = type.GetElementType();
                }
                return type.ResolveTypeFriendlyName(namespaceReplace) + arrayMarker;
            }

            string GetFriendlyNameOfGenericType(Type type)
            {
                if (type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    return type.GetGenericArguments().First().ResolveTypeFriendlyName(namespaceReplace) + "?";

                var friendlyName = $"{type.Namespace}.{type.Name}";

                var typeParameterNames = type
                    .GetGenericArguments()
                    .Select(typeParameter => $"[{typeParameter.ResolveTypeFriendlyName(namespaceReplace)}, {ReplaceNameSpace(typeParameter)}]");
                var joinedTypeParameters = string.Join(",", typeParameterNames);
                return string.Format("{0}[{1}]", friendlyName, joinedTypeParameters);
            }

            string GetFriendlyNameOfPointerType(Type type) =>
                type.GetElementType().ResolveTypeFriendlyName(namespaceReplace) + "*";

            string ReplaceNameSpace(Type type)
            {
                if (string.IsNullOrEmpty(namespaceReplace.replace) || string.IsNullOrEmpty(namespaceReplace.replaceWith))
                    return type.Assembly.FullName;

                return type.Namespace.ToLowerInvariant().StartsWith(namespaceReplace.replace) ? namespaceReplace.replaceWith : $"{type.Namespace}.{type.Name}";
            }
        }


        /// <summary>
        ///     Gets the description (symbology) of the query operator.
        /// </summary>
        /// <param name="queryOperator">The query operator.</param>
        /// <returns>System.String.</returns>
        public static string GetDescription(this QueryOperator queryOperator)
        {
            var attributes = (DescriptionAttribute[])queryOperator.GetType().GetField(queryOperator.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : string.Empty;
        }

        /// <summary>
        ///     Converts the query to a command string.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>System.String.</returns>
        public static string ToCommandString(this IEnumerable<QueryCondition> query)
        {
            var sb = new StringBuilder();
            foreach (var condition in query)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" AND ");
                }

                sb.Append($"({condition.Item} {condition.QueryOperator.GetDescription()} ?)");
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Converts the object array to a DataTable object.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>DataTable.</returns>
        public static DataTable ToDataTable(this object[,] data)
        {
            var rowCount = data.GetLength(0);
            var columnCount = data.GetLength(1);
            var dataTable = new DataTable();
            for (var i = 0; i < columnCount; i++)
            {
                dataTable.Columns.Add();
            }

            foreach (var row in Enumerable.Range(1, rowCount))
            {
                dataTable.Rows.Add(Enumerable.Range(1, columnCount).Select(col => data[row - 1, col - 1]).ToArray());
            }

            return dataTable;
        }

        /// <summary>
        ///     Converts an object to a dynamic object.
        /// </summary>
        /// <param name="value">The object to convert.</param>
        /// <returns>dynamic.</returns>
        public static dynamic ToDynamic(this object value)
        {
            IDictionary<string, object> expando = new ExpandoObject();
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(value.GetType()))
            {
                expando.Add(property.Name, property.GetValue(value));
            }

            return (ExpandoObject)expando;
        }

        public static bool IsCollection(this Type type)
        {
            return typeof(ICollection).IsAssignableFrom(type);
        }

        //public static JObject Filter(this JObject obj, params string[] selects)
        //{
        //    var result = (JContainer)new JObject();

        //    foreach (var select in selects)
        //    {
        //        var token = obj.SelectToken(select);
        //        if (token == null)
        //            continue;

        //        result.Merge(GetNewParent(token.Parent));
        //    }

        //    return (JObject)result;
        //}

        /// <summary>
        ///     Retrieves a set of <see cref="TreeOptions" /> from a string.
        ///     The string must end with a ";" followed by a comma-separated list of tree options.
        ///     The return value indicates whether the options were successfully retrieved.
        /// </summary>
        /// <remarks>
        ///     Example string: "myGroup;nonrecursive,groupsonly"
        /// </remarks>
        /// <param name="s">The string.</param>
        /// <param name="treeOptions">The tree options.</param>
        /// <param name="trimmedString">The string trimmed for the tree options.</param>
        /// <returns><c>true</c> if the tree options were successfully retrieved, <c>false</c> otherwise.</returns>
        public static bool TryGetTreeOptions(this string s, out TreeOptions treeOptions, out string trimmedString)
        {
            treeOptions = default;
            trimmedString = s;
            var index = s.LastIndexOf(";", StringComparison.Ordinal);
            if (index == -1)
            {
                return false;
            }

            var treeOptionString = s.Substring(index + 1);
            if (!Enum.TryParse<TreeOptions>(treeOptionString, true, out var options))
            {
                return false;
            }

            trimmedString = s.Remove(index);
            treeOptions = options;
            return true;
        }

        //private static JToken GetNewParent(JToken token)
        //{
        //    var result = new JObject(token);

        //    var parent = token;
        //    while ((parent = parent.Parent) != null)
        //    {
        //        if (parent is JProperty property)
        //            result = new JObject(new JProperty(property.Name, result));
        //    }

        //    return result;
        //}

    }
}