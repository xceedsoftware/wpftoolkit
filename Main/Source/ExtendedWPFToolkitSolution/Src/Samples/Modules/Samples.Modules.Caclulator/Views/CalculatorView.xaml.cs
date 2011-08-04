using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Modules.Calculator.Views
{
    /// <summary>
    /// Interaction logic for CalculatorView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
    public partial class CalculatorView : DemoView
    {
        public CalculatorView()
        {
            InitializeComponent();
        }
    }
}
