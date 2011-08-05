using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Modules.Text.Views
{
    /// <summary>
    /// Interaction logic for WatermarkTextBoxView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive=false)]
    public partial class WatermarkTextBoxView : DemoView
    {
        public WatermarkTextBoxView()
        {
            InitializeComponent();
        }
    }
}
