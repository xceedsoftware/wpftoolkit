using System;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Prism.Regions;

namespace Samples.Infrastructure
{
    public abstract class ModuleBase : IModule
    {
        protected IRegionManager RegionManager { get; private set; }
        protected IUnityContainer Container { get; private set; }

        protected ModuleBase(IUnityContainer container, IRegionManager regionManager)
        {
            Container = container;
            RegionManager = regionManager;
        }

        public void Initialize()
        {
            RegisterViewsAndTypes();
        }

        protected abstract void RegisterViewsAndTypes();
    }
}
