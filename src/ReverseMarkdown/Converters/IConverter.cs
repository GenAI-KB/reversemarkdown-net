using HtmlAgilityPack;
using System.Collections.Generic;

namespace ReverseMarkdown.Converters
{
    public interface IConverter
    {
        string Convert(HtmlNode node);
        string Convert(HtmlNode node, Dictionary<string, object> context);
    }
}
