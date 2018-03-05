/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections;
using System.Collections.Generic;
using Xceed.Utils.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class UnboundDataItem
  {
    #region Static Fields

    private static readonly WeakDictionary<object, UnboundDataItem> s_collection = new WeakDictionary<object, UnboundDataItem>( 0, new Comparer() );
    private static readonly UnboundDataItem s_empty = new UnboundDataItem( null );

    #endregion

    private UnboundDataItem( object dataItem )
    {
      m_dataItem = dataItem;
    }

    #region DataItem Property

    internal object DataItem
    {
      get
      {
        return m_dataItem;
      }
    }

    private readonly object m_dataItem;

    #endregion

    internal static UnboundDataItem GetUnboundDataItem( object dataItem )
    {
      if( dataItem == null )
        return s_empty;

      var value = dataItem as UnboundDataItem;
      if( value != null )
        return value;

      lock( ( ( ICollection )s_collection ).SyncRoot )
      {
        if( !s_collection.TryGetValue( dataItem, out value ) )
        {
          value = new UnboundDataItem( dataItem );
          s_collection.Add( dataItem, value );
        }

        return value;
      }
    }

    #region Comparer Private Class

    private sealed class Comparer : IEqualityComparer<object>
    {
      int IEqualityComparer<object>.GetHashCode( object obj )
      {
        if( obj == null )
          return 0;

        return obj.GetHashCode();
      }

      bool IEqualityComparer<object>.Equals( object x, object y )
      {
        return object.Equals( x, y );
      }
    }

    #endregion
  }
}
