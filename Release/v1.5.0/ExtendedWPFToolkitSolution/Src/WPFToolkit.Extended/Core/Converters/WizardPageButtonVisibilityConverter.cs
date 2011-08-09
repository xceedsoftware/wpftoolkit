using System;
using System.Windows.Data;
using System.Windows;

namespace Microsoft.Windows.Controls.Core.Converters
{
    public class WizardPageButtonVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility wizardVisibility = (Visibility)values[0];
            WizardPageButtonVisibility wizardPageVisibility = (WizardPageButtonVisibility)values[1];

            Visibility visibility = Visibility.Visible;

            switch (wizardPageVisibility)
            {
                case WizardPageButtonVisibility.Inherit:
                    visibility = wizardVisibility;
                    break;
                case WizardPageButtonVisibility.Collapsed:
                    visibility = Visibility.Collapsed;
                    break;
                case WizardPageButtonVisibility.Hidden:
                    visibility = Visibility.Hidden;
                    break;
                case WizardPageButtonVisibility.Visible:
                    visibility = Visibility.Visible;
                    break;
            }

            return visibility;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
