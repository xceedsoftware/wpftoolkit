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

namespace Xceed.Wpf.DataGrid
{
  internal class GroupNamesTreeKey
  {
    public GroupNamesTreeKey( object[] namesTree )
    {
      m_namesTree = namesTree;

      this.Initialize();
    }

    private void Initialize()
    {
      m_cachedHash = 0;

      // We use this hashing algorithm in order to get a different hashCode
      // when the same values are in a different order in the object array.
      int namesTreeLength = m_namesTree.Length;
      for( int i = 0; i < namesTreeLength; i++ )
      {
        object groupName = m_namesTree[ i ];

        if( groupName == null )
        {
          m_cachedHash ^= DBNull.Value.GetHashCode();
        }
        else
        {
          m_cachedHash ^= groupName.GetHashCode();
        }

        m_cachedHash += ( m_cachedHash << 10 );
        m_cachedHash ^= ( m_cachedHash >> 6 );
      }
    }

    public override int GetHashCode()
    {
      return m_cachedHash;
    }

    public override bool Equals( object obj )
    {
      GroupNamesTreeKey groupStatusKey = obj as GroupNamesTreeKey;

      if( groupStatusKey == null )
        return false;

      if( m_cachedHash != groupStatusKey.m_cachedHash )
        return false;

      int groupStatusKeyNamesTreeLength = groupStatusKey.m_namesTree.Length;

      if( m_namesTree.Length != groupStatusKeyNamesTreeLength )
        return false;

      for( int i = 0; i < groupStatusKeyNamesTreeLength; i++ )
      {
        if( !Object.Equals( m_namesTree[ i ], groupStatusKey.m_namesTree[ i ] ) )
          return false;
      }

      return true;
    }

    #region PRIVATE FIELDS

    private object[] m_namesTree;
    private int m_cachedHash;

    #endregion PRIVATE FIELDS
  }
}
