using System;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Microsoft.Windows.Controls.PropertyGrid.Converters
{
    public class ValueSourceToImagePathConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BaseValueSource bvs = (BaseValueSource)value;

            string uriPrefix = "/WPFToolkit.Extended;component/PropertyGrid/Images/";
            string imageName = "AdvancedProperties11";

            switch (bvs)
            {
                case BaseValueSource.Inherited:
                case BaseValueSource.DefaultStyle:
                case BaseValueSource.ImplicitStyleReference:
                    imageName = "Inheritance11";
                    break;
                case BaseValueSource.DefaultStyleTrigger:                    
                    break;
                case BaseValueSource.Style:
                    imageName = "Style11";
                    break;

                case BaseValueSource.Local:
                    imageName = "Local11";
                    break;
            }


            return new BitmapImage(new Uri(String.Format("{0}{1}.png", uriPrefix, imageName), UriKind.RelativeOrAbsolute));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
