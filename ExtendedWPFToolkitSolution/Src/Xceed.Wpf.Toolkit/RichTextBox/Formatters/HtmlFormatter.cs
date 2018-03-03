using System.IO;
using System.Text;
using System.Windows.Documents;
using System.Xml;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit.Core.Html;

namespace Xceed.Wpf.Toolkit
{
    /// <summary>
    /// Formats the RichTextBox text as html
    /// </summary>
    public class HtmlFormatter : ITextFormatter
    {
        private readonly XamlFormatter _formatter = new XamlFormatter();
        public string GetText( FlowDocument document )
        {
            string xaml = _formatter.GetText(document);
            xaml = AddFlowDocumentParentNode(xaml);
            string html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml);

            return html;
        }

        private string AddFlowDocumentParentNode(string xaml)
        {
            XElement innerXaml = XElement.Parse(xaml);
            XElement root = new XElement("FlowDocument", innerXaml);

            using (var reader = root.CreateReader())
            {
                reader.MoveToContent();
                xaml = reader.ReadOuterXml();
            }

            return xaml;
        }

        public void SetText( FlowDocument document, string text )
        {
            string xaml = HtmlToXamlConverter.ConvertHtmlToXaml(text, false);
            _formatter.SetText(document, xaml);
        }
    }
}
