/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Unity;

namespace Samples.Infrastructure
{
  public abstract class ModuleBase : IModule
  {
    protected IRegionManager RegionManager
    {
      get;
      private set;
    }
    protected IUnityContainer Container
    {
      get;
      private set;
    }

    protected ModuleBase( IUnityContainer container, IRegionManager regionManager )
    {
      Container = container;
      RegionManager = regionManager;
    }

    public void Initialize()
    {
      //types must be registered first
      RegisterViewsAndTypes();
      //now initialize the module
      InitializeModule();
    }

    protected abstract void InitializeModule();
    protected abstract void RegisterViewsAndTypes();
  }
}
