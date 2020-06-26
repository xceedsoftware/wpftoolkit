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

using System;
using System.Windows.Data;
using Xceed.Wpf.AvalonDock.Layout;
using Xceed.Wpf.AvalonDock.Controls;

namespace Xceed.Wpf.AvalonDock.Converters
{
  public class AutoHideCommandLayoutItemFromLayoutModelConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      //when this converter is called layout could be constructing so many properties here are potentially not valid
      var layoutModel = value as LayoutContent;
      if( layoutModel == null )
        return null;
      if( layoutModel.Root == null )
        return null;
      if( layoutModel.Root.Manager == null )
        return null;

      var layoutItemModel = layoutModel.Root.Manager.GetLayoutItemFromModel( layoutModel ) as LayoutAnchorableItem;
      if( layoutItemModel == null )
        return Binding.DoNothing;

      return layoutItemModel.AutoHideCommand;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
