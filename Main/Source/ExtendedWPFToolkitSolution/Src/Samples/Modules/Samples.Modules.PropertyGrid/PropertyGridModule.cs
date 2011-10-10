using System;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure;
using Samples.Infrastructure.Extensions;
using Samples.Modules.PropertyGrid.Views;

namespace Samples.Modules.PropertyGrid
{
    public class PropertyGridModule: ModuleBase
    {
        public PropertyGridModule(IUnityContainer container, IRegionManager regionManager)
            : base(container, regionManager) { }

        protected override void InitializeModule()
        {
            RegionManager.RegisterViewWithRegion(RegionNames.NavigationRegion, typeof(NavigationView));
        }

        protected override void RegisterViewsAndTypes()
        {
            Container.RegisterNavigationType(typeof(HomeView));
            Container.RegisterNavigationType(typeof(CustomEditors));
            Container.RegisterNavigationType(typeof(CustomItemsSource));
            Container.RegisterNavigationType(typeof(DefaultEditors));
            Container.RegisterNavigationType(typeof(ExpandableProperties));
            Container.RegisterNavigationType(typeof(SpecifyingProperties));
            Container.RegisterNavigationType(typeof(BindingToStructs));
        }
    }
}
