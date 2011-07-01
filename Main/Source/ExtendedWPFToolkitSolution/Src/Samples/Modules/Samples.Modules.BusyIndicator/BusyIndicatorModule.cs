using System;
using Samples.Infrastructure;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;

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
            Container.RegisterType(typeof(object), typeof(HomeView), typeof(HomeView).FullName);
        }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(NavigationView));
        }
    }
}
