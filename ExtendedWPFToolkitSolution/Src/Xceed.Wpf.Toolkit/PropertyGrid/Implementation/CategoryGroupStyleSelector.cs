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

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class CategoryGroupStyleSelector : StyleSelector
  {
    public Style SingleDefaultCategoryItemGroupStyle
    {
      get;
      set;
    }
    public Style ItemGroupStyle
    {
      get;
      set;
    }

    public override Style SelectStyle( object item, DependencyObject container )
    {
      var group = item as CollectionViewGroup;
      // Category is not "Misc" => use regular ItemGroupStyle
      if( (group.Name != null) && !group.Name.Equals( CategoryAttribute.Default.Category ) )
        return this.ItemGroupStyle;

      // Category is "Misc"
      while( container != null )
      {
        container = VisualTreeHelper.GetParent( container );
        if( container is ItemsControl )
          break;
      }

      var itemsControl = container as ItemsControl;
      if( itemsControl != null )
      {
        // Category is "Misc" and this is the only category => use SingleDefaultCategoryItemGroupContainerStyle
        if( (itemsControl.Items.Count > 0) && (itemsControl.Items.Groups.Count == 1) )
          return this.SingleDefaultCategoryItemGroupStyle;
      }

      // Category is "Misc" and this is NOT the only category => use regular ItemGroupStyle
      return this.ItemGroupStyle;
    }
  }
}
