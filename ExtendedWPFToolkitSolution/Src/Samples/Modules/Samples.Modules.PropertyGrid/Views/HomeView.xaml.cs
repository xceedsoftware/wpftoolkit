using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;

namespace Samples.Modules.PropertyGrid.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    [RegionMemberLifetime(KeepAlive = false)]
    public partial class HomeView : DemoView
    {
        public HomeView()
        {
            InitializeComponent();
        }
    }
}
