/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid.Views
{
  internal interface IColumnNameCollection : IEnumerable<string>
  {
    int Count
    {
      get;
    }

    string this[ int index ]
    {
      get;
    }

    void Clear();

    void Add( string fieldName );
    void Add( ColumnBase column );

    void Remove( string fieldName );
    void Remove( ColumnBase column );

    bool Contains( string fieldName );
    bool Contains( ColumnBase column );
  }
}
