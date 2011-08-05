using System;
using Samples.Infrastructure;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Samples.Infrastructure.Extensions;
using Samples.Modules.Button.Views;
using Samples.Modules.Button.NavigationItems;

namespace Samples.Modules.Button
{
    public class ButtonModule : ModuleBase
    {
        public ButtonModule(IUnityContainer container, IRegionManager regionManager)
            : base(container, regionManager) { }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(ButtonSpinnerNavItem));
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(DropDownButtonNavItem));
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(SplitButtonNavItem));
        }

        protected override void RegisterViewsAndTypes()
        {
            Container.RegisterNavigationType(typeof(ButtonSpinnerView));
            Container.RegisterNavigationType(typeof(DropDownButtonView));
            Container.RegisterNavigationType(typeof(SplitButtonView));
        }
    }
}
