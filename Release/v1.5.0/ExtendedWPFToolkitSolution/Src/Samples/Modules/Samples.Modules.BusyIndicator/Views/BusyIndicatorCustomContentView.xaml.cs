using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Modules.BusyIndicator.Views
{
    /// <summary>
    /// Interaction logic for BusyIndicatorCustomContentView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive=false)]
    public partial class BusyIndicatorCustomContentView : DemoView
    {
        public BusyIndicatorCustomContentView()
        {
            InitializeComponent();
        }
    }
}
