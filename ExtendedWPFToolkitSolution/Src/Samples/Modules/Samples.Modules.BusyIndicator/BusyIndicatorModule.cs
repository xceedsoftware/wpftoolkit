using System;
using Samples.Infrastructure;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Extensions;

namespace Samples.Modules.BusyIndicator
{
    public class BusyIndicatorModule : ModuleBase
    {
        public BusyIndicatorModule(IUnityContainer container, IRegionManager regionManager)
            : base(container, regionManager)
        {
        }

        protected override void RegisterViewsAndTypes()
        {
            Container.RegisterNavigationType(typeof(HomeView));
        }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(NavigationView));
        }
    }
}
