/**************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System.Windows;
using System;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Wizard.Views
{
  /// <summary>
  /// Interaction logic for WizardView.xaml
  /// </summary>
  public partial class WizardView : DemoView
  {
    private System.Windows.Window _window;

    public WizardView()
    {
      InitializeComponent();
    }

    private void OnButtonClick( object sender, EventArgs e )
    {
      Xceed.Wpf.Toolkit.Wizard wizard = this.Resources[ "_wizard" ] as Xceed.Wpf.Toolkit.Wizard;
      if( wizard != null )
      {
        wizard.CurrentPage = wizard.Items[ 0 ] as Xceed.Wpf.Toolkit.WizardPage;

        if( _window != null )
        {
          _window.Content = null;
          _window = null;
        }
        _window = new System.Windows.Window();
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
