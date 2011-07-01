using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure;
using Samples.Infrastructure.Extensions;
using Samples.Modules.DateTime.Views;

namespace Samples.Modules.DateTime
{
    public class DateTimeModule: ModuleBase
    {
        public DateTimeModule(IUnityContainer container, IRegionManager regionManager)
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
