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
using System.Windows;
using System.Windows.Media;
using System.Collections;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class TableflowViewUIElementCollection : IList<UIElement>
  {
    #region CONSTRUCTORS

    public TableflowViewUIElementCollection( UIElement visualParent )
    {
      if( visualParent == null )
        throw new ArgumentNullException( "visualParent" );

      m_visualCollection = new VisualCollection( visualParent );
    }

    #endregion CONSTRUCTORS

    #region IList<UIElement> Members

    public int IndexOf( UIElement item )
    {
      return m_visualCollection.IndexOf( item );
    }

    public void Insert( int index, UIElement item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      m_visualCollection.Insert( index, item );
    }

    public void RemoveAt( int index )
    {
      m_visualCollection.RemoveAt( index );
    }

    public UIElement this[ int index ]
    {
      get
      {
        return m_visualCollection[ index ] as UIElement;
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "value" );

        if( m_visualCollection[ index ] == value )
          return;

        m_visualCollection[ index ] = value;
      }
    }

    #endregion

    #region ICollection<UIElement> Members

    public void Add( UIElement item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      m_visualCollection.Add( item );
    }

    public void Clear()
    {
      m_visualCollection.Clear();
    }

    public bool Contains( UIElement item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      return m_visualCollection.Contains( item );
    }

    public void CopyTo( UIElement[] array, int arrayIndex )
    {
      m_visualCollection.CopyTo( array, arrayIndex );
    }

    public int Count
    {
      get
      {
        return m_visualCollection.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        return m_visualCollection.IsReadOnly;
      }
    }

    public bool Remove( UIElement item )
    {
      if( item == null )
        throw new ArgumentNullException( "item" );

      if( m_visualCollection.Contains( item ) )
      {
        m_visualCollection.Remove( item );
        return true;
      }

      return false;
    }

    #endregion

    #region IEnumerable<UIElement> Members

    public IEnumerator<UIElement> GetEnumerator()
    {
      return new Enumerator( m_visualCollection.GetEnumerator() );
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return m_visualCollection.GetEnumerator();
    }

    #endregion

    #region PRIVATE FIELDS

    private VisualCollection m_visualCollection;

    #endregion PRIVATE FIELDS

    #region Enumerator Private Class

    private class Enumerator : IEnumerator<UIElement>
    {
      public Enumerator( IEnumerator nonGenericEnumerator )
      {
        if( nonGenericEnumerator == null )
          throw new ArgumentNullException( "nonGenericEnumerator" );

        m_enumerator = nonGenericEnumerator;
      }

      #region IEnumerator<UIElement> Members

      UIElement IEnumerator<UIElement>.Current
      {
        get
        {
          return m_enumerator.Current as UIElement;
        }
      }

      #endregion

      #region IDisposable Members

      void IDisposable.Dispose()
      {
        // Nothing to do
      }

      #endregion

      #region IEnumerator Members

      object IEnumerator.Current
      {
        get
        {
          return m_enumerator.Current;
        }
      }

      bool IEnumerator.MoveNext()
      {
        return m_enumerator.MoveNext();
      }

      void IEnumerator.Reset()
      {
        m_enumerator.Reset();
      }

      #endregion

      IEnumerator m_enumerator;
    }

    #endregion Enumerator Private Class
  }
}
