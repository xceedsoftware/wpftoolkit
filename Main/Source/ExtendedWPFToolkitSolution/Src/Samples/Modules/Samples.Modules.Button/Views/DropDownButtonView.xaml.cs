using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Modules.Button.Views
{
    /// <summary>
    /// Interaction logic for DropDownButtonView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
    public partial class DropDownButtonView : DemoView
    {
        public DropDownButtonView()
        {
            InitializeComponent();
        }

        private void DropDownButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _dropDownButton.IsOpen = false;
        }
    }
}
