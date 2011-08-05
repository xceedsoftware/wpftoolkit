using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;
using System.Windows.Data;

namespace Samples.Modules.BusyIndicator.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive=false)]
    public partial class BusyIndicatorView : DemoView
    {
        public BusyIndicatorView()
        {
            InitializeComponent();
        }
    }

    public class IntegerToTimespanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return TimeSpan.FromMilliseconds((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
