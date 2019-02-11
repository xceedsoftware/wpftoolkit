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
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class DataGridItemPropertyRoute
  {
    private DataGridItemPropertyRoute( DataGridItemPropertyBase itemProperty )
      : this( itemProperty, null )
    {
    }

    private DataGridItemPropertyRoute( DataGridItemPropertyBase itemProperty, DataGridItemPropertyRoute parent )
    {
      Debug.Assert( itemProperty != null );

      m_itemProperty = itemProperty;
      m_parent = parent;
    }

    #region Current Property

    internal DataGridItemPropertyBase Current
    {
      get
      {
        return m_itemProperty;
      }
    }

    private readonly DataGridItemPropertyBase m_itemProperty;

    #endregion

    #region Parent Property

    internal DataGridItemPropertyRoute Parent
    {
      get
      {
        return m_parent;
      }
    }

    private readonly DataGridItemPropertyRoute m_parent;

    #endregion

    internal static DataGridItemPropertyRoute Create( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return null;

      var collection = itemProperty.ContainingCollection;
      if( collection == null )
        return new DataGridItemPropertyRoute( itemProperty );

      return new DataGridItemPropertyRoute(
                   itemProperty,
                   DataGridItemPropertyRoute.Create( collection.Owner ) );
    }

    internal static DataGridItemPropertyRoute Combine( DataGridItemPropertyBase itemProperty, DataGridItemPropertyRoute ancestors )
    {
      if( itemProperty == null )
        return ancestors;

      if( ancestors == null )
        return DataGridItemPropertyRoute.Create( itemProperty );

      var collection = itemProperty.ContainingCollection;
      if( collection == null )
        throw new InvalidOperationException();

      if( collection.Owner != ancestors.Current )
        throw new InvalidOperationException();

      return new DataGridItemPropertyRoute( itemProperty, ancestors );
    }
  }
}
