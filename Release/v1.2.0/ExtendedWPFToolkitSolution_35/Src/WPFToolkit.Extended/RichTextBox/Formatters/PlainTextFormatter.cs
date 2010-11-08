using System;
using System.Windows.Documents;

namespace Microsoft.Windows.Controls
{
    /// <summary>
    /// Formats the RichTextBox text as plain text
    /// </summary>
    public class PlainTextFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {
            return new TextRange(document.ContentStart, document.ContentEnd).Text;
        }

        public void SetText(FlowDocument document, string text)
        {
            new TextRange(document.ContentStart, document.ContentEnd).Text = text;
        }
    }
}
