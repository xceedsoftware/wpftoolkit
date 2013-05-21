/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.ObjectModel;
using System.Windows;
using Xceed.Wpf.DataGrid.Markup;

namespace Xceed.Wpf.DataGrid
{
  internal class GroupHeaderFooterCollection : ObservableCollection<object>
  {
    protected override void InsertItem( int index, object item )
    {
      if( item is DataTemplate ) 
      {
        base.InsertItem( index, item );
      }
      else
      {
        GroupHeaderFooterItemTemplate vwc = item as GroupHeaderFooterItemTemplate;
        if( vwc != null )
        {
          if(vwc.Template == null)
          {
            throw new ArgumentException( "A VisibleWhenCollapsed object must have its Template property set to an instance of a DataTemplate.");
          }
          else
          {
            base.InsertItem( index, item );
          }
        }
        else
        {
          throw new ArgumentException( "Group headers and footers can only receive ClearHeadersFooters, DataTemplate, or VisibleWhenCollapsed objects." );
        }
      }
    }
  }
}
