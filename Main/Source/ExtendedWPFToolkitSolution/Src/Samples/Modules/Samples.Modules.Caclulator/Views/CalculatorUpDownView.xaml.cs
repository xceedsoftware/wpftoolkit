using System;
using Samples.Infrastructure.Controls;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Modules.Calculator.Views
{
    /// <summary>
    /// Interaction logic for CalculatorUpDownView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
    public partial class CalculatorUpDownView : DemoView
    {
        public CalculatorUpDownView()
        {
            InitializeComponent();
        }
    }
}
