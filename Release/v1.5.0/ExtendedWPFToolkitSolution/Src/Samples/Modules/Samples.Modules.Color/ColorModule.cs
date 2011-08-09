using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure;
using Samples.Infrastructure.Extensions;
using Samples.Modules.Color.Views;


namespace Samples.Modules.Color
{
    public class ColorModule: ModuleBase
    {
        public ColorModule(IUnityContainer container, IRegionManager regionManager)
            : base(container, regionManager) { }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(NavigationView));
        }

        protected override void RegisterViewsAndTypes()
        {
            Container.RegisterNavigationType(typeof(HomeView));
        }
    }
}
