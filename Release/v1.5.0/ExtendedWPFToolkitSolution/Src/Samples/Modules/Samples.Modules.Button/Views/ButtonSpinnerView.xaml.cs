using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Controls;
using System.Windows.Controls;

namespace Samples.Modules.Button.Views
{
    /// <summary>
    /// Interaction logic for ButtonSpinnerView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
    public partial class ButtonSpinnerView : DemoView
    {
        public ButtonSpinnerView()
        {
            InitializeComponent();
        }

        private void ButtonSpinner_Spin(object sender, Microsoft.Windows.Controls.SpinEventArgs e)
        {
            ButtonSpinner spinner = (ButtonSpinner)sender;
            TextBox txtBox = (TextBox)spinner.Content;

            try
            {
                int value = String.IsNullOrEmpty(txtBox.Text) ? 0 : Convert.ToInt32(txtBox.Text);
                if (e.Direction == Microsoft.Windows.Controls.SpinDirection.Increase)
                    value++;
                else
                    value--;
                txtBox.Text = value.ToString();
            }
            catch
            {
                txtBox.Text = "0";
            }
        }
    }
}
