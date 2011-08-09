using System;
using Samples.Infrastructure;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Samples.Infrastructure.Extensions;
using Samples.Modules.Calculator.Views;
using Samples.Modules.Calculator.NavigationItems;

namespace Samples.Modules.Calculator
{
    public class CalculatorModule : ModuleBase
    {
        public CalculatorModule(IUnityContainer container, IRegionManager regionManager)
            : base(container, regionManager) { }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(CalculatorNavItem));
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(CalculatorUpDownNavItem));
        }

        protected override void RegisterViewsAndTypes()
        {
            Container.RegisterNavigationType(typeof(CalculatorView));
            Container.RegisterNavigationType(typeof(CalculatorUpDownView));
        }
    }
}
