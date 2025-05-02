namespace DHI.Services.Spreadsheets.Markdown
{
    using System;
    using System.Text;

    /// <summary>
    /// Converts a MarkdownDocument into a string that matches a markdown.md file.
    /// </summary>
    public static class MarkdownBuilder
    {
        /// <summary>
        /// Converts a markdown document to the string contents of a markdown md file.
        /// </summary>
        public static string Build(MarkdownDocument document)
        {
            var stringBuilder = new StringBuilder();

            foreach (var element in document.Elements)
            {
                switch (element.Type)
                {
                    case MarkdownType.Text: stringBuilder.AppendLine(BuildText(element)); break;
                    case MarkdownType.Heading1: stringBuilder.AppendLine(BuildHeading(element)); break;
                    case MarkdownType.Heading2: stringBuilder.AppendLine(BuildHeading(element)); break;
                    case MarkdownType.Heading3: stringBuilder.AppendLine(BuildHeading(element)); break;
                    case MarkdownType.Heading4: stringBuilder.AppendLine(BuildHeading(element)); break;
                    case MarkdownType.Heading5: stringBuilder.AppendLine(BuildHeading(element)); break;
                    case MarkdownType.Heading6: stringBuilder.AppendLine(BuildHeading(element)); break;
                    case MarkdownType.Table: stringBuilder.AppendLine(BuildTable(element)); break;
                }
            }

            // Remove the last line feed from the string as the AppendLine will create one extra new line.
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            }

            return stringBuilder.ToString();
        }

        internal static string BuildText(Element element)
        {
            var textElement = element as TextElement;
            return $"{textElement.Text}";
        }

        internal static string BuildHeading(Element element)
        {
            var headingElement = element as HeadingElement;

            switch (headingElement.Type)
            {
                case MarkdownType.Heading1: return $"# {headingElement.Title}";
                case MarkdownType.Heading2: return $"## {headingElement.Title}";
                case MarkdownType.Heading3: return $"### {headingElement.Title}";
                case MarkdownType.Heading4: return $"#### {headingElement.Title}";
                case MarkdownType.Heading5: return $"##### {headingElement.Title}";
                case MarkdownType.Heading6: return $"###### {headingElement.Title}";
                default: throw new Exception("Default case reached in HeadingElement.ToMarkdown()");
            }
        }

        internal static string BuildTable(Element element)
        {
            var tableElement = element as TableElement;

            var stringBuilder = new StringBuilder();

            // Build the Header
            var headerLine = "|";
            foreach (var header in tableElement.Headers)
            {
                headerLine += $"{header.Name}|";
            }
            stringBuilder.AppendLine(headerLine);

            // Build the Table Spec
            var specLine = "|";
            foreach (var header in tableElement.Headers)
            {
                switch (header.Alignment)
                {
                    case TableHeaderAlignment.Center: specLine += $":-:|"; break;
                    case TableHeaderAlignment.Right: specLine += $"-:|"; break;
                    default: specLine += "-|"; break;
                }
            }
            stringBuilder.AppendLine(specLine);

            foreach (var row in tableElement.Rows)
            {
                var rowLine = "|";
                foreach (var column in row.Columns)
                {
                    if (column == null)
                    {
                        rowLine += "|";
                    }
                    else
                    {   // Accounts for a pipe in a cell...
                        rowLine += $"{column.Replace("|", @"\|")}|";
                    }
                }
                stringBuilder.AppendLine(rowLine);
            }

            // Remove the last line feed from the string.
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - Environment.NewLine.Length, Environment.NewLine.Length);
            }

            return stringBuilder.ToString();
        }
    }
}
