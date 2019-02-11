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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridForeignKeyConverter : ForeignKeyConverter
  {
    public DataGridForeignKeyConverter()
    {
    }

    public override object GetValueFromKey( object key, ForeignKeyConfiguration configuration )
    {
      var itemsSource = configuration.ItemsSource;
      if( itemsSource == null )
        return key;

      if( ( itemsSource is DataView ) || ( itemsSource is DataTable ) )
        return new DataTableForeignKeyConverter().GetValueFromKey( key, configuration );

      return this.GetValueFromKeyCore( key, itemsSource, configuration.ValuePath, configuration.DisplayMemberPath );
    }

    public override object GetKeyFromValue( object value, ForeignKeyConfiguration configuration )
    {
      if( value == null )
        return null;

      var displayMemberPath = configuration.DisplayMemberPath;
      if( string.IsNullOrWhiteSpace( displayMemberPath ) )
        return value;

      var valuePath = configuration.ValuePath;
      if( string.IsNullOrWhiteSpace( valuePath ) )
        return value;

      var itemsSource = configuration.ItemsSource;
      if( itemsSource == null )
        return value;

      if( ( itemsSource is DataView ) || ( itemsSource is DataTable ) )
        return new DataTableForeignKeyConverter().GetKeyFromValue( value, configuration );

      try
      {
        var enumerator = itemsSource.GetEnumerator();
        enumerator.MoveNext();
        var firstItem = enumerator.Current;
        var itemType = firstItem.GetType();
        var propertyInfo = itemType.GetProperty( displayMemberPath );

        foreach( object item in itemsSource )
        {
          if( item == null )
            continue;

          var displayValue = propertyInfo.GetValue( item, null );

          if( value.Equals( displayValue ) )
            return itemType.GetProperty( valuePath ).GetValue( item, null );
        }
      }
      catch
      {
        //Swallow the exception, no need to terminate the application, just return the original value.
      }

      return value;
    }

    public override object GetValueFromKey( object key, DataGridForeignKeyDescription description )
    {
      var itemsSource = description.ItemsSource;
      if( itemsSource == null )
        return key;

      if( ( itemsSource is DataView ) || ( itemsSource is DataTable ) )
        return new DataTableForeignKeyConverter().GetValueFromKey( key, description );

      return this.GetValueFromKeyCore( key, itemsSource, description.ValuePath, description.DisplayMemberPath );
    }

    private object GetValueFromKeyCore( object key, IEnumerable itemsSource, string valuePath, string displayMemberPath )
    {
      if( key == null )
        return null;

      if( string.IsNullOrWhiteSpace( displayMemberPath ) )
        return key;

      if( string.IsNullOrWhiteSpace( valuePath ) )
        return key;

      try
      {
        var enumerator = itemsSource.GetEnumerator();
        enumerator.MoveNext();
        var firstItem = enumerator.Current;
        var itemType = firstItem.GetType();
        var propertyInfo = itemType.GetProperty( valuePath );

        foreach( object item in itemsSource )
        {
          if( item == null )
            continue;

          var value = propertyInfo.GetValue( item, null );

          if( key.Equals( value ) )
            return itemType.GetProperty( displayMemberPath ).GetValue( item, null );
        }
      }
      catch
      {
        //Swallow the exception, no need to terminate the application, just return the original value.
      }

      return key;
    }
  }
}
