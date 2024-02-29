using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ReverseMarkdown.Converters
{
    public class Table : ConverterBase
    {
        public Table(Converter converter) : base(converter)
        {
            Converter.Register("table", this);
        }

        public override string Convert(HtmlNode node)
        {
            // if table does not have a header row , add empty header row if set in config
            var useEmptyRowForHeader = this.Converter.Config.TableWithoutHeaderRowHandling ==
                                       Config.TableWithoutHeaderRowHandlingOption.EmptyRow;

            var emptyHeaderRow = HasNoTableHeaderRow(node) && useEmptyRowForHeader
                ? EmptyHeader(node)
                : string.Empty;
            string content = string.Empty;
            TableInfo ti = new TableInfo();
            Dictionary<string, object> context = new Dictionary<string, object> {
                { "table",ti}
            };
            int rowIndex = 0;
            foreach (var n in node.ChildNodes)
            {
                context["rowIndex"]= rowIndex;
                var result = Treat(n, context);
                content += result;
                rowIndex++;
            }

            return $"{Environment.NewLine}{Environment.NewLine}{emptyHeaderRow}{content}{Environment.NewLine}";
        }

        private static bool HasNoTableHeaderRow(HtmlNode node)
        {
            var thNode = node.SelectNodes("//th")?.FirstOrDefault();
            return thNode == null;
        }

        private static string EmptyHeader(HtmlNode node)
        {
            var firstRow = node.SelectNodes("//tr")?.FirstOrDefault();

            if (firstRow == null)
            {
                return string.Empty;
            }
            int colCount = 0;
            foreach (var childNode in firstRow.ChildNodes)
            {
                if (childNode.Name.Contains("td"))
                {
                    var colspan = childNode.GetAttributeValue("colspan", "1");
                    if (!int.TryParse(colspan, out var span))
                        span = 1;
                    colCount += span;
                }
            }

            var headerRowItems = new List<string>();
            var underlineRowItems = new List<string>();

            for (var i = 0; i < colCount; i++)
            {
                headerRowItems.Add("<!---->");
                underlineRowItems.Add("---");
            }

            var headerRow = $"| {string.Join(" | ", headerRowItems)} |{Environment.NewLine}";
            var underlineRow = $"| {string.Join(" | ", underlineRowItems)} |{Environment.NewLine}";

            return headerRow + underlineRow;
        }
    }
}
