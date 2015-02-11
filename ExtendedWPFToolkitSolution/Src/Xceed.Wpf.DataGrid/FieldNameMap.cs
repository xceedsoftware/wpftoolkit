/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  internal class FieldNameMap
  {
    public bool TryGetColumnFieldName( string name, out string columnName )
    {
      return this.TryGetTarget( name, out columnName );
    }

    public bool TryGetColumnFieldName( ColumnBase column, out string columnName )
    {
      if( column != null )
        return this.TryGetTarget( column.FieldName, out columnName );

      columnName = null;

      return false;
    }

    public bool TryGetColumnFieldName( SortDescription sortDescription, out string columnName )
    {
      return this.TryGetTarget( sortDescription.PropertyName, out columnName );
    }

    public bool TryGetItemPropertyName( string name, out string itemPropertyName )
    {
      return this.TryGetSource( name, out itemPropertyName );
    }

    public bool TryGetItemPropertyName( ColumnBase column, out string itemPropertyName )
    {
      if( column != null )
        return this.TryGetSource( column.FieldName, out itemPropertyName );

      itemPropertyName = null;

      return false;
    }

    public bool TryGetItemPropertyName( SortDescription sortDescription, out string itemPropertyName )
    {
      return this.TryGetSource( sortDescription.PropertyName, out itemPropertyName );
    }

    protected virtual bool TryGetSource( string targetName, out string sourceName )
    {
      sourceName = targetName;

      return !string.IsNullOrEmpty( targetName );
    }

    protected virtual bool TryGetTarget( string sourceName, out string targetName )
    {
      targetName = sourceName;

      return !string.IsNullOrEmpty( sourceName );
    }
  }
}
