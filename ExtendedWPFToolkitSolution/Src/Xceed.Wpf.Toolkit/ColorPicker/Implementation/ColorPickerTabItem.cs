/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Windows.Controls;
using System.Windows.Input;

namespace Xceed.Wpf.Toolkit
{
  public class ColorPickerTabItem : TabItem
  {
    protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      if( e.Source == this || !this.IsSelected )
      {
        e.Handled = true;
        return;
      }

      base.OnMouseLeftButtonDown( e );
    }

    protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      //Selection on Mouse Up
      if( e.Source == this || !this.IsSelected )
      {
        base.OnMouseLeftButtonDown( e );
      }

      base.OnMouseLeftButtonUp( e );
    }
  }
}
