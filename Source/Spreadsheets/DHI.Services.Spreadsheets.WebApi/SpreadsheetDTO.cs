namespace DHI.Services.Spreadsheets.WebApi
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    ///     Data transfer object for spreadsheet resource representation
    /// </summary>
    public class SpreadsheetDTO
    {
        /// <summary>
        ///     Gets or sets the fullname.
        /// </summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>
        ///     Gets or sets the data. Each object array in the list represents an individual sheet.
        /// </summary>
        [Required]
        public IList<object[,]> Data { get; set; }

        /// <summary>
        ///     Gets or sets the individual sheet names.
        /// </summary>
        public IList<string> SheetNames { get; set; }

        /// <summary>
        ///     Converts the DTO to a Spreadsheet object.
        /// </summary>
        public Spreadsheet<string> ToSpreadsheet()
        {
            var fullName = DHI.Services.FullName.Parse(FullName);
            var spreadsheet = new Spreadsheet<string>(fullName.ToString(), fullName.Name, fullName.Group);

            foreach (var sheet in Data)
            {
                spreadsheet.Data.Add(sheet);
            }

            if (SheetNames != null)
            {
                spreadsheet.Metadata.Add("SheetNames", SheetNames);
            }

            return spreadsheet;
        }
    }
}