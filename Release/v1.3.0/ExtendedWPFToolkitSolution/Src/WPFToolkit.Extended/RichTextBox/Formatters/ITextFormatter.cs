using System;
using System.Windows.Documents;

namespace Microsoft.Windows.Controls
{
    public interface ITextFormatter
    {
        string GetText(FlowDocument document);
        void SetText(FlowDocument document, string text);
    }
}
