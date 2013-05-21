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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;

namespace Xceed.Wpf.DataGrid
{
  internal class SourceItemCollection : IList, IAddChild
  {
    internal SourceItemCollection( DataGridCollectionView parentCollectionView )
    {
      if( parentCollectionView == null )
        throw new ArgumentNullException( "parentCollectionView" );

      m_parentCollectionView = parentCollectionView;
    }

    #region IList Members

    public int Add( object value )
    {
      int index = this.Count;
      this.Insert( index, value );
      return index;
    }

    public void Clear()
    {
      this.CheckParentCollectionViewSourceNotUsed();
      m_parentCollectionView.RemoveSourceItem( 0, this.Count );
    }

    public bool Contains( object value )
    {
      return m_parentCollectionView.IndexOfSourceItem( value ) != -1;
    }

    public int IndexOf( object value )
    {
      return m_parentCollectionView.IndexOfSourceItem( value );
    }

    public void Insert( int index, object value )
    {
      this.CheckParentCollectionViewSourceNotUsed();

      if( ( index < 0 ) || ( index > this.Count ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than or equal to Count." );

      m_parentCollectionView.AddSourceItem( index, new object[] { value }, this.Count + 1 );
    }

    public bool IsFixedSize
    {
      get
      {
        return !this.ParentCollectionViewSourceNotUsed();
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return !this.ParentCollectionViewSourceNotUsed();
      }
    }

    public void Remove( object value )
    {
      this.CheckParentCollectionViewSourceNotUsed();
      int index = this.IndexOf( value );

      if( index != -1 )
        m_parentCollectionView.RemoveSourceItem( index, 1 );
    }

    public void RemoveAt( int index )
    {
      this.CheckParentCollectionViewSourceNotUsed();

      if( ( index < 0 ) || ( index >= this.Count ) )
        throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than Count." );

      m_parentCollectionView.RemoveSourceItem( index, 1 );
    }

    public object this[ int index ]
    {
      get
      {
        if( ( index < 0 ) || ( index >= this.Count ) )
          throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than Count." );

        return m_parentCollectionView.GetSourceItemAt( index );
      }
      set
      {
        if( ( index < 0 ) || ( index >= this.Count ) )
          throw new ArgumentOutOfRangeException( "index", index, "index must be greater than or equal to zero and less than Count." );

        m_parentCollectionView.ReplaceSourceItem(
          index, new object[] { this[ index ] },
          index, new object[] { value } );
      }
    }

    #endregion

    #region ICollection Members

    public void CopyTo( Array array, int index )
    {
      int count = this.Count;

      for( int i = 0; i < count; i++ )
      {
        array.SetValue( this[ i ], index );
        index++;
      }
    }

    public int Count
    {
      get
      {
        return m_parentCollectionView.SourceItemCount;
      }
    }

    public bool IsSynchronized
    {
      get
      {
        return false;
      }
    }

    public object SyncRoot
    {
      get
      {
        return this;
      }
    }

    #endregion

    #region IEnumerable Members

    public IEnumerator GetEnumerator()
    {
      return new SourceItemCollectionEnumerator( m_parentCollectionView.GetSourceListEnumerator() );
    }

    #endregion

    #region IAddChild Members

    void IAddChild.AddChild( object value )
    {
      this.Add( value );
    }

    void IAddChild.AddText( string text )
    {
      this.Add( text );
    }

    #endregion

    private void CheckParentCollectionViewSourceNotUsed()
    {
      if( this.ParentCollectionViewSourceNotUsed() )
        throw new InvalidOperationException( "An attempt was made to modify the list of items while a source is being used." );
    }

    private bool ParentCollectionViewSourceNotUsed()
    {
      return ( m_parentCollectionView.SourceCollection != null );
    }

    private DataGridCollectionView m_parentCollectionView;

    private class SourceItemCollectionEnumerator : IEnumerator
    {
      public SourceItemCollectionEnumerator( IEnumerator<RawItem> rawItemEnumerator )
      {
        m_rawItemEnumerator = rawItemEnumerator;
      }

      #region IEnumerator Members

      public object Current
      {
        get
        {
          RawItem currentItem = m_rawItemEnumerator.Current;

          if( currentItem == null )
            return null;

          return currentItem.DataItem;
        }
      }

      public bool MoveNext()
      {
        return m_rawItemEnumerator.MoveNext();
      }

      public void Reset()
      {
        m_rawItemEnumerator.Reset();
      }

      #endregion

      private IEnumerator<RawItem> m_rawItemEnumerator;
    }
  }
}
