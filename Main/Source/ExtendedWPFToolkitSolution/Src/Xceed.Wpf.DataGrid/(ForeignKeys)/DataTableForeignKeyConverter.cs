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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Xceed.Wpf.DataGrid
{
  public class DataTableForeignKeyConverter : ForeignKeyConverter
  {
    public DataTableForeignKeyConverter()
    {
    }

    public override object GetKeyFromValue( object value, ForeignKeyConfiguration configuration )
    {
      // When the Key changes, try to find the data row that has the new key.
      // If it is not found, return null.
      DataView dataView = configuration.ItemsSource as DataView;
      if( dataView == null )
      {
        DataTable dataTable = configuration.ItemsSource as DataTable;
        if( dataTable != null )
        {
          dataView = dataTable.DefaultView;
        }
      }

      if( dataView == null )
      {
        return value;
      }

      DataRowView dataRow = value as DataRowView;

      if( dataRow == null )
      {
        return null;
      }
      else
      {
        return dataRow[ dataView.Sort ];
      }
    }

    public override object GetValueFromKey( object key, ForeignKeyConfiguration configuration )
    {
      // When the Key changes, try to find the data row that has the new key.
      // If it is not found, return null.
      DataView dataView = configuration.ItemsSource as DataView;
      if( dataView == null )
      {
        DataTable dataTable = configuration.ItemsSource as DataTable;
        if( dataTable != null )
        {
          dataView = dataTable.DefaultView;
        }
      }

      if( dataView == null )
      {
        return key;
      }

      string valuePath = configuration.ValuePath;

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

      object value = dataRow;

      // Use the DisplayMemberPath if defined
      if( dataRow != null )
      {
        if( !string.IsNullOrEmpty( configuration.DisplayMemberPath ) )
        {
          value = dataRow[ configuration.DisplayMemberPath ];
        }
      }

      return value;
    }
  }
}
