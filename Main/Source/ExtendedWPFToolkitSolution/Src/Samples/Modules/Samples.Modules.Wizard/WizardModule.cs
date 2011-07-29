using System;
using Samples.Infrastructure;
using Samples.Infrastructure.Extensions;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Samples.Modules.Wizard.Views;

namespace Samples.Modules.Wizard
{
    public class WizardModule : ModuleBase
    {
        public WizardModule(IUnityContainer container, IRegionManager regionManager)
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
