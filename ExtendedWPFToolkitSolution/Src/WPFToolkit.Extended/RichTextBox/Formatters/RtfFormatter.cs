using System;
using System.Text;
using System.Windows.Documents;
using System.IO;
using System.Windows;

namespace Microsoft.Windows.Controls
{
    /// <summary>
    /// Formats the RichTextBox text as RTF
    /// </summary>
    public class RtfFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {
            TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
            using (MemoryStream ms = new MemoryStream())
            {
                tr.Save(ms, DataFormats.Rtf);
                return ASCIIEncoding.Default.GetString(ms.ToArray());
            }
        }

        public void SetText(FlowDocument document, string text)
        {
            try
            {
                //if the text is null/empty clear the contents of the RTB. If you were to pass a null/empty string
                //to the TextRange.Load method an exception would occur.
                if (String.IsNullOrEmpty(text))
                {
                    document.Blocks.Clear();
                }
                else
                {
                    TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                    using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(text)))
                    {
                        tr.Load(ms, DataFormats.Rtf);
                    }
                }
            }
            catch
            {
                throw new InvalidDataException("Data provided is not in the correct RTF format.");
            }
        }
    }
}
