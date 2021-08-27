using System;
using System.Text;

namespace Swagger2Pdf.HtmlDocumentBuilder
{
    public sealed class Link : HtmlElement
    {
        string text = "";

        public Link() : base("a")
        {
        }

        protected override void WriteStartTag(StringBuilder htmlStringBuilder)
        {
            htmlStringBuilder.Append("<");
            htmlStringBuilder.Append(ElementName);
            WriteAttributes(htmlStringBuilder);
            WriteDictionaryAttributes(htmlStringBuilder);
        }

        protected override void WriteEndTag(StringBuilder htmlStringBuilder)
        {
            if (string.IsNullOrEmpty(text))
            {
                htmlStringBuilder.Append("/>");
            }
            else
            {
                htmlStringBuilder.Append(">");
                htmlStringBuilder.Append(text);
                htmlStringBuilder.Append("</a>");
            }
        }

        public Link Href(string source)
        {
            SetAttribute("href", source);
            return this;
        }

        public Link SetText(string value)
        {
            text = value;
            return this;
        }
    }
}