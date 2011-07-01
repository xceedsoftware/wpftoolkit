using System;
using Samples.Infrastructure;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Modules.BusyIndicator
{
    public class BusyIndicatorModule : ModuleBase
    {
        protected BusyIndicatorModule(IUnityContainer container, IRegionManager regionManager)
            : base(container, regionManager)
        {
        }

        protected override void RegisterViewsAndTypes()
        {

        }
    }
}
