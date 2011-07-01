using System;
using Samples.Infrastructure;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;
using Samples.Infrastructure.Extensions;
using Samples.Modules.Calculator.Views;

namespace Samples.Modules.Calculator
{
    public class CalculatorModule : ModuleBase
    {
        public CalculatorModule(IUnityContainer container, IRegionManager regionManager)
            : base(container, regionManager) { }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(NavigationView));
        }

        protected override void RegisterViewsAndTypes()
        {
            Container.RegisterNavigationType(typeof(HomeView));
            Container.RegisterNavigationType(typeof(CalculatorView));
            Container.RegisterNavigationType(typeof(CalculatorUpDownView));
        }
    }
}
