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
using System.Diagnostics;
using System.Linq;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class RawItemMap
  {
    #region [] Property

    internal RawItem this[ object dataItem ]
    {
      get
      {
        Debug.Assert( dataItem != null );

        RawItem value;
        if( m_singleMap.TryGetValue( dataItem, out value ) )
          return value;

        RawItem[] values;
        if( m_multiMap.TryGetValue( dataItem, out values ) )
          return values[ 0 ];

        return null;
      }
    }

    #endregion

    internal void Add( object dataItem, RawItem rawItem )
    {
      Debug.Assert( dataItem != null );
      Debug.Assert( rawItem != null );
      Debug.Assert( dataItem == rawItem.DataItem );

      RawItem single;
      RawItem[] multiple;

      if( m_singleMap.TryGetValue( dataItem, out single ) )
      {
        Debug.Assert( rawItem != single, "It's not normal to be called twice for the same RawItem." );

        m_multiMap.Add( dataItem, new RawItem[] { single, rawItem } );
        m_singleMap.Remove( dataItem );
      }
      else if( m_multiMap.TryGetValue( dataItem, out multiple ) )
      {
        Debug.Assert( !multiple.Contains( rawItem ), "It's not normal to be called twice for the same RawItem." );

        var length = multiple.Length;

        Array.Resize<RawItem>( ref multiple, length + 1 );
        multiple[ length ] = rawItem;

        m_multiMap[ dataItem ] = multiple;
      }
      else
      {
        m_singleMap.Add( dataItem, rawItem );
      }
    }

    internal void Remove( object dataItem, RawItem rawItem )
    {
      Debug.Assert( dataItem != null );
      Debug.Assert( rawItem != null );
      Debug.Assert( dataItem == rawItem.DataItem );

      if( m_singleMap.Remove( dataItem ) )
        return;

      RawItem[] multiple;
      if( !m_multiMap.TryGetValue( dataItem, out multiple ) )
        return;

      var length = multiple.Length;
      if( length == 2 )
      {
        if( multiple[ 0 ] == rawItem )
        {
          m_singleMap.Add( dataItem, multiple[ 1 ] );
          m_multiMap.Remove( dataItem );
        }
        else if( multiple[ 1 ] == rawItem )
        {
          m_singleMap.Add( dataItem, multiple[ 0 ] );
          m_multiMap.Remove( dataItem );
        }
      }
      else
      {
        Debug.Assert( length > 2 );

        var index = Array.IndexOf( multiple, rawItem );
        if( index < 0 )
          return;

        RawItem[] copy = new RawItem[ length - 1 ];

        if( index > 0 )
        {
          Array.Copy( multiple, 0, copy, 0, index );
        }

        if( index < length - 1 )
        {
          Array.Copy( multiple, index + 1, copy, index, length - index - 1 );
        }

        m_multiMap[ dataItem ] = copy;
      }
    }

    internal void Clear()
    {
      m_singleMap.Clear();
      m_multiMap.Clear();
    }

    #region Private Fields

    private readonly Dictionary<object, RawItem> m_singleMap = new Dictionary<object, RawItem>( 128 );
    private readonly Dictionary<object, RawItem[]> m_multiMap = new Dictionary<object, RawItem[]>( 0 );

    #endregion
  }
}
