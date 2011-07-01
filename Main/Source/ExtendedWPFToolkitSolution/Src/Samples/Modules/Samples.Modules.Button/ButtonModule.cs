using System;
using Samples.Infrastructure;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Samples.Infrastructure.Extensions;
using Samples.Modules.Button.Views;

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
            Container.RegisterNavigationType(typeof(HomeView));
            Container.RegisterNavigationType(typeof(ButtonSpinnerView));
            Container.RegisterNavigationType(typeof(SplitButtonView));
        }
    }
}
