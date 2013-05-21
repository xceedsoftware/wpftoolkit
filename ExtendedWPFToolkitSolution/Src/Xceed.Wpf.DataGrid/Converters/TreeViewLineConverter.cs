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
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using System.Windows.Controls;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid.Converters
{
  [EditorBrowsable( EditorBrowsableState.Never )]
  public class TreeViewLineConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      TreeViewItem item = ( TreeViewItem )value;
      ItemsControl ic = ItemsControl.ItemsControlFromItemContainer( item );

      return ic.ItemContainerGenerator.IndexFromContainer( item ) == ic.Items.Count - 1;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      return Binding.DoNothing;
    }
  }


}
