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
    }
}
