using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace ReverseMarkdown.Converters
{
    public class Tr : ConverterBase
    {
        public Tr(Converter converter) : base(converter)
        {
            Converter.Register("tr", this);
        }
        public override string Convert(HtmlNode node)
        {
            return string.Empty;
        }
        public override string Convert(HtmlNode node, Dictionary<string, object> context)
        {
            TableInfo ti = context["table"] as TableInfo;
            int rowIndex = (int)context["rowIndex"];
            RowInfo ri = new RowInfo();            
            string content = string.Empty;
            int colIndex = 0;
            foreach (var n in node.ChildNodes)
            {  
                var result = Treat(n);
                if (n.Name == "th" || n.Name == "td")
                {
                    var tdInfo = ColInfo.Parse(n);
                    foreach (var rIndex in ti.Rows.Keys)
                    {
                        var r = ti.Rows[rIndex];
                        if (r.Cols.TryGetValue(colIndex,out var c))
                        {
                            colIndex += c.ColSpan-1;
                            content += c.Content;
                        }
                    } 
                    if (tdInfo.RowSpan > 1)
                    {
                        tdInfo.Content = result;
                        ri.Cols.Add(colIndex, tdInfo);
                    }
                    colIndex += tdInfo.ColSpan;
                }
                content += result;
            }
            if(ri.Cols.Count>0)
                ti.Rows.Add(rowIndex, ri);
            content = content.TrimEnd();
            var underline = "";

            if (string.IsNullOrWhiteSpace(content))
            {
                return "";
            }

            // if parent is an ordered or unordered list
            // then table need to be indented as well
            var indent = IndentationFor(node);

            if (IsTableHeaderRow(node) || UseFirstRowAsHeaderRow(node))
            {
                underline = UnderlineFor(node, indent);
            }

            return $"{indent}|{content}{Environment.NewLine}{underline}";
        }

        private bool UseFirstRowAsHeaderRow(HtmlNode node)
        {
            var tableNode = node.Ancestors("table").FirstOrDefault();
            var firstRow = tableNode?.SelectSingleNode(".//tr");

            if (firstRow == null)
            {
                return false;
            }

            var isFirstRow = firstRow == node;
            var hasNoHeaderRow = tableNode.SelectNodes(".//th")?.FirstOrDefault() == null;

            return isFirstRow
                   && hasNoHeaderRow
                   && Converter.Config.TableWithoutHeaderRowHandling ==
                   Config.TableWithoutHeaderRowHandlingOption.Default;
        }

        private static bool IsTableHeaderRow(HtmlNode node)
        {
            return node.ChildNodes.FindFirst("th") != null;
        }

        private static string UnderlineFor(HtmlNode node, string indent)
        {
            var nodes = node.ChildNodes.Where(x => x.Name == "th" || x.Name == "td").ToList();

            var cols = new List<string>();
            foreach (var nd in nodes)
            {
                var tdInfo = ColInfo.Parse(nd);
                var styles = StringUtils.ParseStyle(nd.GetAttributeValue("style", ""));
                styles.TryGetValue("text-align", out var align);
                string line = "---";
                switch (align?.Trim())
                {
                    case "left":
                        line = ":---";
                        break;
                    case "right":
                        line = "---:";
                        break;
                    case "center":
                        line = ":---:";
                        break;
                    default:
                        line = "---";
                        break;
                }
                for (int i = 0; i < tdInfo.ColSpan; i++)
                {
                    cols.Add(line);
                }
            }

            var colsAggregated = string.Join(" | ", cols);

            return $"{indent}| {colsAggregated} |{Environment.NewLine}";
        }


    }
}
