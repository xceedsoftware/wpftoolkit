/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/
using System;
using System.Windows.Input;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure;

namespace Samples
{
  public class ShellViewModel : ViewModelBase, IShellViewModel
  {
    private readonly IRegionManager _regionManager;

    public ICommand NavigateCommand
    {
      get;
      set;
    }

    public ShellViewModel( IRegionManager regionManager )
    {
      _regionManager = regionManager;
      NavigateCommand = new DelegateCommand<string>( Navigate );
    }

    private void Navigate( string navigatePath )
    {
      if( !String.IsNullOrWhiteSpace( navigatePath ) )
        _regionManager.RequestNavigate( RegionNames.ContentRegion, navigatePath );
    }
  }
}
