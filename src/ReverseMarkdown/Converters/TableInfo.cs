using HtmlAgilityPack;
using System.Collections.Generic;

namespace ReverseMarkdown.Converters
{
    public class TableInfo
    {
        public Dictionary<int, RowInfo> Rows { get; } = new Dictionary<int, RowInfo>();
    }
    public class RowInfo
    {
        public Dictionary<int, ColInfo> Cols { get; } = new Dictionary<int, ColInfo>();
    }
    public class ColInfo
    {
        public int RowSpan { get; set; } = 1;
        public int ColSpan { get; set; } = 1;
        public string Content { get; set; }
        public static ColInfo Parse(HtmlNode node)
        {
            var colspan = node.GetAttributeValue("colspan", "1");
            if (!int.TryParse(colspan, out var colSpan))
                colSpan = 1;
            var rowspan = node.GetAttributeValue("rowspan", "1");
            if (!int.TryParse(rowspan, out var rowSpan))
                rowSpan = 1;
            return new ColInfo
            {
                RowSpan = rowSpan,
                ColSpan = colSpan,
            };
        }
    }
}
