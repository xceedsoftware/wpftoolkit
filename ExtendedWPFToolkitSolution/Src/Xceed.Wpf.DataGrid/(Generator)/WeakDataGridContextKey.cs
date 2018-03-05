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
  internal sealed class WeakDataGridContextKey
  {
    internal WeakDataGridContextKey( DataGridContext dataGridContext )
    {
      var parentDataGridContext = dataGridContext.ParentDataGridContext;
      var level = 0;

      while( parentDataGridContext != null )
      {
        level++;
        parentDataGridContext = parentDataGridContext.ParentDataGridContext;
      }

      Debug.Assert( ( ( level == 0 ) || ( dataGridContext.SourceDetailConfiguration != null ) ), "A child dataGridContext must have a SourceDetailConfiguration." );

      m_sourceDetailConfigurationRelationName = ( dataGridContext.SourceDetailConfiguration != null ) ? dataGridContext.SourceDetailConfiguration.RelationName : string.Empty;

      if( level > 0 )
      {
        // We are NOT dealing with the root DataGridContext.

        // Build the tree of master items.
        m_weakItemsTree = new WeakReference[ level ];

        var tempDataGridContext = dataGridContext;
        for( int i = level - 1; i >= 0; i-- )
        {
          // Ensure to get a reference to the System.Data.DataRow when doing a 
          // save/restore of a System.Data.DataRowView since the view is recreated
          // for every detail views
          var parentItem = ItemsSourceHelper.TryGetDataRowFromDataItem( tempDataGridContext.ParentItem );

          m_weakItemsTree[ i ] = new WeakReference( parentItem );

          tempDataGridContext = tempDataGridContext.ParentDataGridContext;
        }
      }

      m_hashCode = WeakDataGridContextKey.CalculateHashCode( m_sourceDetailConfigurationRelationName, m_weakItemsTree );
    }

    public override int GetHashCode()
    {
      return m_hashCode;
    }

    public override bool Equals( object obj )
    {
      var target = obj as WeakDataGridContextKey;
      if( ( target == null ) || ( m_hashCode != target.m_hashCode ) )
        return false;

      if( m_sourceDetailConfigurationRelationName != target.m_sourceDetailConfigurationRelationName )
        return false;

      var dataGridContextStatusKeyLength = ( target.m_weakItemsTree == null ) ? 0 : target.m_weakItemsTree.Length;
      var myLength = ( m_weakItemsTree == null ) ? 0 : m_weakItemsTree.Length;

      if( myLength != dataGridContextStatusKeyLength )
        return false;

      for( var i = 0; i < dataGridContextStatusKeyLength; i++ )
      {
        var sourceItem = m_weakItemsTree[ i ].Target;
        var targetItem = target.m_weakItemsTree[ i ].Target;

        if( !object.Equals( sourceItem, targetItem ) )
          return false;
      }

      return true;
    }

    private static int CalculateHashCode( string relationName, WeakReference[] items )
    {
      var hashCode = ( !string.IsNullOrEmpty( relationName ) ) ? relationName.GetHashCode() : 0;

      if( items != null )
      {
        // We use this hashing algorithm in order to get a different hashCode
        // when the same values are in a different order in the object array.
        for( var i = 0; i < items.Length; i++ )
        {
          var item = items[ i ].Target;
          Debug.Assert( item != null, "Item should still be referenced by the Child DataGridContext at this point!." );

          if( item == null )
            continue;

          hashCode ^= item.GetHashCode();
          hashCode += ( hashCode << 10 );
          hashCode ^= ( hashCode >> 6 );
        }
      }

      return hashCode;
    }

    private readonly WeakReference[] m_weakItemsTree;
    private readonly string m_sourceDetailConfigurationRelationName;
    private readonly int m_hashCode;
  }
}
