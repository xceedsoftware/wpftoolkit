/***************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  *************************************************************************************/

using System;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.Button.Views
{
  /// <summary>
  /// Interaction logic for ButtonSpinnerView.xaml
  /// </summary>
  public partial class ButtonSpinnerView : DemoView
  {
    public ButtonSpinnerView()
    {
      InitializeComponent();
    }

    private void ButtonSpinner_Spin( object sender, SpinEventArgs e )
    {
      String[] names = (String[])this.Resources[ "names" ];

      ButtonSpinner spinner = ( ButtonSpinner )sender;
      TextBox txtBox = ( TextBox )spinner.Content;

      int value = Array.IndexOf( names, txtBox.Text );
      if( e.Direction == SpinDirection.Increase )
        value++;
      else
        value--;

      if( value < 0 )
        value = names.Length - 1;
      else if( value >= names.Length )
        value = 0;

      txtBox.Text = names[ value ];
    }
  }
}
