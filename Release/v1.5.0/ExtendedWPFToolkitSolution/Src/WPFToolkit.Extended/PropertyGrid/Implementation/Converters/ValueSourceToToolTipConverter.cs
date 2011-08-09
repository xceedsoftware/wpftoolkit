using System;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Windows.Controls.PropertyGrid.Converters
{
    class ValueSourceToToolTipConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            BaseValueSource bvs = (BaseValueSource)value;
            string toolTip = "Advanced Properties";

            switch (bvs)
            {
                case BaseValueSource.Inherited:
                case BaseValueSource.DefaultStyle:
                case BaseValueSource.ImplicitStyleReference:
                    toolTip = "Inheritance";
                    break;
                case BaseValueSource.Style:
                    toolTip = "Style Setter";
                    break;

                case BaseValueSource.Local:
                    toolTip = "Local";
                    break;
            }

            return toolTip;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
