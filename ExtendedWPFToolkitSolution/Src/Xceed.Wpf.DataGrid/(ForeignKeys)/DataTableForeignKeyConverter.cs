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
using System.Linq;
using System.Text;
using System.Data;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  public class DataTableForeignKeyConverter : ForeignKeyConverter
  {
    public DataTableForeignKeyConverter()
    {
    }

    public override object GetValueFromKey( object key, ForeignKeyConfiguration configuration )
    {
      return this.GetValueFromKeyCore( key, configuration.ItemsSource, configuration.ValuePath, configuration.DisplayMemberPath, true );
    }

    public override object GetKeyFromValue( object value, ForeignKeyConfiguration configuration )
    {
      var dataView = configuration.ItemsSource as DataView;
      if( dataView == null )
      {
        var dataTable = configuration.ItemsSource as DataTable;
        if( dataTable != null )
        {
          dataView = dataTable.DefaultView;
        }
      }

      if( dataView == null )
      {
        return value;
      }

      var dataRow = value as DataRowView;

      if( dataRow == null )
      {
        return null;
      }
      else
      {
        return dataRow[ dataView.Sort ];
      }
    }

    public override object GetValueFromKey( object key, DataGridForeignKeyDescription description )
    {
      return this.GetValueFromKeyCore( key, description.ItemsSource, description.ValuePath, description.DisplayMemberPath, false );
    }

    private object GetValueFromKeyCore( object key, IEnumerable itemsSource, string valuePath, string displayMemberPath, bool isConfiguration )
    {
      var dataView = itemsSource as DataView;
      if( dataView == null )
      {
        var dataTable = itemsSource as DataTable;
        if( dataTable != null )
        {
          dataView = dataTable.DefaultView;
        }
      }

      if( dataView == null )
      {
        return key;
      }

      if( string.IsNullOrEmpty( valuePath ) )
      {
        return key;
      }

      dataView.Sort = valuePath;

      DataRowView dataRow;

      int index = -1;

      if( ( key != null ) && ( !object.Equals( string.Empty, key ) ) )
      {
        try
        {
          index = dataView.Find( key );
        }
        catch( Exception )
        {
        }
      }

      if( index == -1 )
      {
        dataRow = null;
      }
      else
      {
        dataRow = dataView[ index ];
      }

      // Use the DisplayMemberPath if defined
      if( dataRow != null )
      {
        if( !string.IsNullOrEmpty( displayMemberPath ) )
        {
          return dataRow[ displayMemberPath ];
        }
      }

      if( isConfiguration )
        return dataRow;

      return key;
    }
  }
}
