using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Modules.Button.Views
{
    /// <summary>
    /// Interaction logic for SplitButtonView.xaml
    /// </summary>
     [RegionMemberLifetime(KeepAlive = false)]
    public partial class SplitButtonView : DemoView
    {
        public SplitButtonView()
        {
            InitializeComponent();
        }

        private void SplitButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Microsoft.Windows.Controls.MessageBox.Show("Thanks for clicking me!", "SplitButton Click");
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            _splitButton.IsOpen = false;
        }
    }
}
