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

using Microsoft.Practices.Prism.Regions;
using Samples.Infrastructure.Controls;
using System.Windows;
using System;

namespace Samples.Modules.Wizard.Views
{
  /// <summary>
  /// Interaction logic for HomeView.xaml
  /// </summary>
  [RegionMemberLifetime( KeepAlive = false )]
  public partial class HomeView : DemoView
  {
    private Window _window;

    public HomeView()
    {
      InitializeComponent();
    }

    private void OnButtonClick( object sender, EventArgs e )
    {
      Xceed.Wpf.Toolkit.Wizard wizard = Resources[ "_wizard" ] as Xceed.Wpf.Toolkit.Wizard;
      if( wizard != null )
      {
        wizard.CurrentPage = wizard.Items[ 0 ] as Xceed.Wpf.Toolkit.WizardPage;

        if( _window != null )
        {
          _window.Content = null;
          _window = null;
        }
        _window = new Window();
        _window.Title = "Wizard demonstration";
        _window.Content = wizard;
        _window.Width = 600;
        _window.Height = 400;
        _window.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
        // Window will be closed by Wizard because FinishButtonClosesWindow = true and CancelButtonClosesWindow = true
        _window.ShowDialog();
      }
    }

    private void OnWizardHelp( object sender, EventArgs e )
    {
      System.Windows.MessageBox.Show( "This is the Help for the Wizard\n\n\n\n\n", "Wizard Help" );
    }
  }
}
