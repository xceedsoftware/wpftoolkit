using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;

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
    }
}
