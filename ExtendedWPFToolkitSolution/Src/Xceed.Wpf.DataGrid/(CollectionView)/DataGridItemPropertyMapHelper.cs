/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/


namespace Xceed.Wpf.DataGrid
{
  internal static class DataGridItemPropertyMapHelper
  {
    internal static bool TryGetMasterColumnName( DataGridItemPropertyMap itemPropertyMap, string detailColumnName, out string masterColumnName )
    {
      masterColumnName = default( string );

      if( itemPropertyMap == null )
        return false;

      var detailItemProperty = ItemsSourceHelper.GetItemPropertyFromProperty( itemPropertyMap.DetailItemProperties, detailColumnName );
      if( detailItemProperty == null )
        return false;

      DataGridItemPropertyBase masterItemProperty;
      if( !itemPropertyMap.TryGetMasterItemProperty( detailItemProperty, out masterItemProperty ) )
        return false;

      masterColumnName = PropertyRouteParser.Parse( masterItemProperty );

      return !string.IsNullOrEmpty( masterColumnName );
    }

    internal static bool TryGetDetailColumnName( DataGridItemPropertyMap itemPropertyMap, string masterColumnName, out string detailColumnName )
    {
      detailColumnName = default( string );

      if( itemPropertyMap == null )
        return false;

      var masterItemProperty = ItemsSourceHelper.GetItemPropertyFromProperty( itemPropertyMap.MasterItemProperties, masterColumnName );
      if( masterItemProperty == null )
        return false;

      DataGridItemPropertyBase detailItemProperty;
      if( !itemPropertyMap.TryGetDetailItemProperty( masterItemProperty, out detailItemProperty ) )
        return false;

      detailColumnName = PropertyRouteParser.Parse( detailItemProperty );

      return !string.IsNullOrEmpty( detailColumnName );
    }

    internal static bool TryGetMasterColumn( DataGridContext detailContext, ColumnBase detailColumn, out ColumnBase masterColumn )
    {
      if( detailContext != null )
      {
        var masterContext = detailContext.RootDataGridContext;
        if( masterContext != null )
          return DataGridItemPropertyMapHelper.TryGetMasterColumn( detailContext.ItemPropertyMap, masterContext.Columns, detailColumn, out masterColumn );
      }

      masterColumn = default( ColumnBase );

      return false;
    }

    internal static bool TryGetDetailColumn( DataGridContext detailContext, ColumnBase masterColumn, out ColumnBase detailColumn )
    {
      if( detailContext != null )
        return DataGridItemPropertyMapHelper.TryGetDetailColumn( detailContext.ItemPropertyMap, detailContext.Columns, masterColumn, out detailColumn );

      detailColumn = default( ColumnBase );

      return false;
    }

    internal static bool TryGetMasterColumn( DataGridItemPropertyMap itemPropertyMap, ColumnCollection masterColumns, ColumnBase detailColumn, out ColumnBase masterColumn )
    {
      masterColumn = default( ColumnBase );

      if( ( masterColumns == null ) || ( detailColumn == null ) )
        return false;

      string masterColumnName;
      if( !DataGridItemPropertyMapHelper.TryGetMasterColumnName( itemPropertyMap, detailColumn.FieldName, out masterColumnName ) )
        return false;

      masterColumn = masterColumns[ masterColumnName ];

      return ( masterColumn != null );
    }

    internal static bool TryGetDetailColumn( DataGridItemPropertyMap itemPropertyMap, ColumnCollection detailColumns, ColumnBase masterColumn, out ColumnBase detailColumn )
    {
      detailColumn = default( ColumnBase );

      if( ( detailColumns == null ) || ( masterColumn == null ) )
        return false;

      string detailColumnName;
      if( !DataGridItemPropertyMapHelper.TryGetDetailColumnName( itemPropertyMap, masterColumn.FieldName, out detailColumnName ) )
        return false;

      detailColumn = detailColumns[ detailColumnName ];

      return ( detailColumn != null );
    }
  }
}
