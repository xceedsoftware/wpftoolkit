/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/


namespace Xceed.Wpf.DataGrid.Views
{
  internal interface IColumnInfoCollection<T>
  {
    void Clear();

    T this[ string fieldName ]
    {
      get;
      set;
    }

    T this[ ColumnBase column ]
    {
      get;
      set;
    }

    void Reset( string fieldName );
    void Reset( ColumnBase column );

    bool Contains( string fieldName );
    bool Contains( ColumnBase column );

    bool TryGetValue( string fieldName, out T value );
    bool TryGetValue( ColumnBase column, out T value );
  }
}
