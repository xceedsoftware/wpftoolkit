/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class WeakDataGridContextKey
  {
    public WeakDataGridContextKey( DataGridContext dataGridContext )
    {
      DataGridContext parentDataGridContext = dataGridContext.ParentDataGridContext;
      int level = 0;
      while( parentDataGridContext != null )
      {
        level++;
        parentDataGridContext = parentDataGridContext.ParentDataGridContext;
      }

      System.Diagnostics.Debug.Assert( ( ( level == 0 ) || ( dataGridContext.SourceDetailConfiguration != null ) ),
        "A child dataGridContext must have a SourceDetailConfiguration." );

      m_sourceDetailConfigurationRelationName = ( dataGridContext.SourceDetailConfiguration != null ) ?
        dataGridContext.SourceDetailConfiguration.RelationName : string.Empty;

      if( level > 0 )
      {
        // We are NOT dealing with the root DataGridContext.

        // Build the tree of master items.
        m_weakItemsTree = new WeakReference[ level ];

        DataGridContext tempDataGridContext = dataGridContext;
        for( int i = level - 1; i >= 0; i-- )
        {
          // Ensure to get a reference to the System.Data.DataRow when doing a 
          // save/restore of a System.Data.DataRowView since the view is recreated
          // for every detail views
          object parentItem = ItemsSourceHelper.TryGetDataRowFromDataItem( tempDataGridContext.ParentItem );

          m_weakItemsTree[ i ] = new WeakReference( parentItem );

          tempDataGridContext = tempDataGridContext.ParentDataGridContext;
        }
      }

      this.Initialize();
    }

    private void Initialize()
    {
      m_cachedHash = 0;

      if( !string.IsNullOrEmpty( m_sourceDetailConfigurationRelationName ) )
        m_cachedHash = m_sourceDetailConfigurationRelationName.GetHashCode();

      if( m_weakItemsTree != null )
      {
        // We use this hashing algorithm in order to get a different hashCode
        // when the same values are in a different order in the object array.
        int weakItemsTreeLength = m_weakItemsTree.Length;
        for( int i = 0; i < weakItemsTreeLength; i++ )
        {
          object item = m_weakItemsTree[ i ].Target;

          System.Diagnostics.Debug.Assert( item != null, "Item should still be referenced by the Child DataGridContext at this point!." );

          m_cachedHash ^= item.GetHashCode();
          m_cachedHash += ( m_cachedHash << 10 );
          m_cachedHash ^= ( m_cachedHash >> 6 );
        }
      }
    }

    private int m_cachedHash;

    public override int GetHashCode()
    {
      return m_cachedHash;
    }

    private WeakReference[] m_weakItemsTree;
    private string m_sourceDetailConfigurationRelationName;

    public override bool Equals( object obj )
    {
      WeakDataGridContextKey weakDataGridContextKey = obj as WeakDataGridContextKey;

      if( weakDataGridContextKey == null )
        return false;

      if( m_cachedHash != weakDataGridContextKey.m_cachedHash )
        return false;

      if( m_sourceDetailConfigurationRelationName != weakDataGridContextKey.m_sourceDetailConfigurationRelationName )
        return false;

      int dataGridContextStatusKeyLength = ( weakDataGridContextKey.m_weakItemsTree == null ) ? 0 : weakDataGridContextKey.m_weakItemsTree.Length;
      int myLength = ( m_weakItemsTree == null ) ? 0 : m_weakItemsTree.Length;

      if( myLength != dataGridContextStatusKeyLength )
        return false;

      for( int i = 0; i < dataGridContextStatusKeyLength; i++ )
      {
        object sourceItem = m_weakItemsTree[ i ].Target;
        object targetItem = weakDataGridContextKey.m_weakItemsTree[ i ].Target;

        if( !object.Equals( sourceItem, targetItem ) )
          return false;
      }

      return true;
    }
  }

}
