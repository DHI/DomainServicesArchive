namespace DHI.Services.Spreadsheets
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class Spreadsheet<TId> : BaseGroupedEntity<TId>
    {
        public Spreadsheet(TId id, string name, string group)
            : base(id, name, group)
        {
        }

        public List<object[,]> Data { get; } = new List<object[,]>();

        public bool ShouldSerializeData()
        {
            return Data.Count > 0;
        }

        //public override T Clone<T>()
        //{
        //    var writeOptions = new JsonSerializerOptions();
        //    writeOptions.Converters.Add(new JsonStringEnumConverter());
        //    writeOptions.Converters.Add(new SpreadsheetConverter<TId>());
        //    var json = JsonSerializer.Serialize(this, typeof(T), writeOptions);
        //    var readOptions = new JsonSerializerOptions();
        //    readOptions.Converters.Add(new JsonStringEnumConverter());
        //    readOptions.Converters.Add(new ObjectToInferredTypeConverter());
        //    return JsonSerializer.Deserialize<T>(json, readOptions)!; 
        //}
    }

    [Serializable]
    public class Spreadsheet : Spreadsheet<string>
    {
        public Spreadsheet(string id, string name, string group)
            : base(id, name, group)
        {
        }
    }
}