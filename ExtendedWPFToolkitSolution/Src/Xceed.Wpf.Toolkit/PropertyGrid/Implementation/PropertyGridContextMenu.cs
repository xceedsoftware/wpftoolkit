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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyGridContextMenu : ContextMenu
  {
    protected override void OnOpened( RoutedEventArgs e )
    {
      base.OnOpened( e );

      var contextMenu = e.OriginalSource as ContextMenu;
      if( ( contextMenu != null ) && ( contextMenu.PlacementTarget != null ) )
      {
        var control = contextMenu.PlacementTarget;

        // Get PropertyItemBase parent
        var parent = VisualTreeHelper.GetParent( control );
        while( parent != null )
        {
          var propertyItemBase = parent as PropertyItemBase;
          if( propertyItemBase != null )
          {
            contextMenu.DataContext = propertyItemBase;
            break;
          }
          parent = VisualTreeHelper.GetParent( parent );
        }
      }
    }
  }

}
