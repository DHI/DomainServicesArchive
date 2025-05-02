namespace DHI.Services.Spreadsheets.Markdown
{
    using System.Collections.Generic;
    using System.Linq;

    public enum MarkdownType
    {
        Text = 0,
        Heading1,
        Heading2,
        Heading3,
        Heading4,
        Heading5,
        Heading6,
        Table
    }

    // Document Structure:
    //
    // Markdown Document
    //   |
    //   +-> Heading Element
    //   |
    //   +-> Text Element
    //   |
    //   +-> Table Element
    //         +- TableHeader[]
    //         +- TableRow[]
    //              +- Columns[]
    //

    /// <summary>
    /// Represents a markdown md file as an object model.
    /// </summary>
    public class MarkdownDocument
    {
        public List<Element> Elements { get; }


        public MarkdownDocument()
        {
            Elements = new List<Element>();
        }

        /// <summary>
        /// Returns true if the MarkdownDocument contains the specified heading.
        /// </summary>
        public bool ContainsHeading(string headingName)
        {
            return Elements.Any(x => x is HeadingElement heading && string.Compare(heading.Title, headingName, true) == 0);
        }

        /// <summary>
        /// Finds the first table after the specified heading.
        /// </summary>
        public TableElement FindTable(string headingTitle)
        {
            var index = Elements.FindIndex(x => x is HeadingElement heading && string.Compare(heading.Title, headingTitle, true) == 0);

            if (index == -1)
            {
                return null;
            }

            var element = Elements.Skip(index + 1).FirstOrDefault(x => x is TableElement || x is HeadingElement);

            if (element is TableElement tableElement)
            {
                return tableElement;
            }

            return null; // There wasn't a table after the last heading...
        }

        /// <summary>
        /// Returns a list of all the headings in the file.
        /// </summary>
        public List<HeadingElement> GetHeadings()
        {
            var headings = Elements.Where(x => x is HeadingElement).Cast<HeadingElement>().ToList();

            return headings;
        }

        /// <summary>
        /// Returns a list of all the tables in the file.
        /// </summary>
        public List<TableElement> GetTables()
        {
            var headings = Elements.Where(x => x is TableElement).Cast<TableElement>().ToList();

            return headings;
        }
    }
}
