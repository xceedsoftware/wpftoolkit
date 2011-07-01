using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samples.Infrastructure;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace Samples.Modules.Button
{
    public class ButtonModule : ModuleBase
    {
        public ButtonModule(IUnityContainer container, IRegionManager regionManager)
            : base(container, regionManager) { }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(NavigationView));
        }

        protected override void RegisterViewsAndTypes()
        {
            Container.RegisterType(typeof(object), typeof(HomeView), typeof(HomeView).FullName);
        }
    }
}
