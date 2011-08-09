using System;
using Samples.Infrastructure;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Extensions;
using Samples.Modules.BusyIndicator.NavigationItems;
using Samples.Modules.BusyIndicator.Views;

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
            Container.RegisterNavigationType(typeof(BusyIndicatorView));
            Container.RegisterNavigationType(typeof(BusyIndicatorCustomContentView));
            Container.RegisterNavigationType(typeof(BusyIndicatorCustomContentTemplateView));
        }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(BusyIndicatorNavItem));
        }
    }
}
