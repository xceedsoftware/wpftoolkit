using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Modules.BusyIndicator.Views
{
    /// <summary>
    /// Interaction logic for BusyIndicatorCustomContentTemplateView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive=false)]
    public partial class BusyIndicatorCustomContentTemplateView : DemoView
    {
        public BusyIndicatorCustomContentTemplateView()
        {
            InitializeComponent();
        }
    }
}
