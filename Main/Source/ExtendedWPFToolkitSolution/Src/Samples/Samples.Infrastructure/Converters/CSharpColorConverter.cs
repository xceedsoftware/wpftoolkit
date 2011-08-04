using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Samples.Infrastructure.Core.CodeFormatting;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Samples.Infrastructure.Converters
{
    public class CSharpColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return value;

            String val = (String)value;

            CSharpFormat cSharpFormat = new CSharpFormat();
            FlowDocument doc = new FlowDocument();
            Paragraph p = new Paragraph();
            p = cSharpFormat.FormatCode(val);
            doc.Blocks.Add(p);

            RichTextBox rtb = new RichTextBox();
            rtb.IsReadOnly = true;
            rtb.Document = doc;
            rtb.Document.PageWidth = 2500.0;
            rtb.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            rtb.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            rtb.FontFamily = new FontFamily("Courier New");
            return rtb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
