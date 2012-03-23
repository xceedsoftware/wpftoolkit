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

using System.Windows;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

namespace Samples
{
  public class Bootstrapper : UnityBootstrapper
  {
    protected override DependencyObject CreateShell()
    {
      Container.RegisterType<IShellViewModel, ShellViewModel>();
      return Container.Resolve<Shell>();
    }

    protected override void InitializeShell()
    {
      base.InitializeShell();

      App.Current.MainWindow = ( Shell )Shell;
      App.Current.MainWindow.Show();
    }

    protected override Microsoft.Practices.Prism.Modularity.IModuleCatalog CreateModuleCatalog()
    {
      return new ConfigurationModuleCatalog();
    }
  }
}
