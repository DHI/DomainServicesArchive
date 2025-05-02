namespace DHI.Services.Spreadsheets.Markdown
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;


    /// <summary>
    /// Converts a Markdown string from a markdown.md file into a MarkdownDocument, a structured representation of markdown.
    /// </summary>
    public static class MarkdownParser
    {
        /// <summary>
        /// Parses the string of a markdown md file and converts it to a MarkdownDocument.
        /// </summary>
        public static MarkdownDocument Parse(string str)
        {
            var document = new MarkdownDocument();
            TableElement table = null;

            var lines = str.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries); // I wish we had .ReplaceLineEndings() in C# 7.3...

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();

                // Table
                if (line.StartsWith("|"))
                {
                    // Building a table...
                    // Trim the first and last pipe...
                    var content = line.SplitOnPipe();

                    if (table != null)
                    {
                        var trimmedContent = new List<string>();
                        foreach (var val in content)
                        {
                            // Note: As of mid - 2017, the pipe may simply be escaped with a backslash, like so: \|
                            if (val.StartsWith(" ") && val.EndsWith(" ") && val != " ")
                            {
                                // Remove start and end and unescape a pipe                                
                                trimmedContent.Add(val.Substring(1, val.Length - 2).Replace(@"\|", "|").Trim());
                            }
                            else
                            {
                                // Unescape a pipe
                                trimmedContent.Add(val.Replace(@"\|", "|").Trim());
                            }
                        }

                        table.Rows.Add(new TableRow(trimmedContent));
                    }
                    else
                    {
                        // New Table
                        // Assure the next line exists or its going to be a sad table.
                        if (i + 1 >= lines.Length)
                        {   // Last line in the file.
                            document.Elements.Add(new TextElement(line));
                            continue;
                        }

                        // Assure the next line is a table definition and only contains a ' ', '|', ':', '\t'
                        var nextLine = lines[i + 1];
                        if (nextLine.Any(x => !" |:-\t".Contains(x)))
                        {
                            document.Elements.Add(new TextElement(line));
                            continue;
                        }

                        i++; // Skip the table definition as we will process it here.

                        table = new TableElement();


                        // Trim the first and last pipe...
                        var dividerContentString = nextLine.Trim().TrimStartAndEndingPipes();
                        var dividerContent = dividerContentString.Split('|').ToList();

                        var count = content.Count > dividerContent.Count ? content.Count : dividerContent.Count;
                        for (var x = 0; x < count; x++)
                        {
                            string header = null; // If a header isnt provided default to null
                            var divider = "";

                            if (content.Count > x)
                            {
                                header = content[x].Trim();
                            }
                            if (dividerContent.Count > x)
                            {
                                divider = dividerContent[x].Trim();
                            }

                            var alignment = TableHeaderAlignment.Left;
                            if (divider.StartsWith(":") && divider.EndsWith(":"))
                            {
                                alignment = TableHeaderAlignment.Center;
                            }
                            else if (divider.StartsWith(":"))
                            {
                                alignment = TableHeaderAlignment.Left;
                            }
                            if (divider.EndsWith(":"))
                            {
                                alignment = TableHeaderAlignment.Right;
                            }

                            table.Headers.Add(new TableHeader(header, alignment));
                        }

                        document.Elements.Add(table);
                    }
                }
                else
                {
                    table = null;

                    // Headings...
                    if (line.StartsWith("# "))
                    {
                        var title = line.Substring("# ".Length);
                        document.Elements.Add(new HeadingElement(MarkdownType.Heading1, title));
                    }
                    else if (line.StartsWith("## "))
                    {
                        var title = line.Substring("## ".Length);
                        document.Elements.Add(new HeadingElement(MarkdownType.Heading2, title));
                    }
                    else if (line.StartsWith("### "))
                    {
                        var title = line.Substring("### ".Length);
                        document.Elements.Add(new HeadingElement(MarkdownType.Heading3, title));
                    }
                    else if (line.StartsWith("#### "))
                    {
                        var title = line.Substring("#### ".Length);
                        document.Elements.Add(new HeadingElement(MarkdownType.Heading4, title));
                    }
                    else if (line.StartsWith("##### "))
                    {
                        var title = line.Substring("##### ".Length);
                        document.Elements.Add(new HeadingElement(MarkdownType.Heading5, title));
                    }
                    else if (line.StartsWith("###### "))
                    {
                        var title = line.Substring("###### ".Length);
                        document.Elements.Add(new HeadingElement(MarkdownType.Heading6, title));
                    }
                    else
                    {
                        // Text Element (or we don't know what it is so just store it so we can reconstitute it when turning the object back into markup.
                        document.Elements.Add(new TextElement(line));
                    }
                }
            }

            return document;
        }
    }
}
