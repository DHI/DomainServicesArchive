namespace DHI.Services.Samples.Spreadsheet.ViewModels.Types
{
    using System;

    public sealed class RepositoryTypeItem
    {
        public RepositoryTypeItem(Type type) { Type = type; }
        public Type Type { get; }
        public string DisplayName => Type.FullName ?? Type.Name;
        public override string ToString() => DisplayName;
    }

    public sealed class SpreadsheetItem
    {
        public SpreadsheetItem(DHI.Services.Spreadsheets.Spreadsheet<string> s) { Spreadsheet = s; }
        public DHI.Services.Spreadsheets.Spreadsheet<string> Spreadsheet { get; }
        public string Id => Spreadsheet.Id;
        public string Name => Spreadsheet.Name;
        public string Group => Spreadsheet.Group ?? "";
        public string DisplayName => string.IsNullOrEmpty(Group) ? Name : $"{Group}/{Name}";
        public override string ToString() => DisplayName;
    }
}
