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
using System.Windows;
using System.Windows.Media;
using System.Collections;
using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ItemsHostUIElementCollection : IList<UIElement>
  {
    #region Constructors

    public ItemsHostUIElementCollection( UIElement visualParent )
    {
      if( visualParent == null )
        throw new ArgumentNullException( "visualParent" );

      m_visualCollection = new VisualCollection( visualParent );
      m_visualCollectionLookupDictionary = new Dictionary<UIElement, object>();
      m_visualParent = visualParent;
    }

    #endregion

    #region IList<UIElement> Members

    public int IndexOf( UIElement element )
    {
      if( !m_visualCollectionLookupDictionary.ContainsKey( element ) )
        return -1;

      return m_visualCollection.IndexOf( element );
    }

    public void Insert( int index, UIElement element )
    {
      if( element == null )
        throw new ArgumentNullException( "element" );

      m_visualCollection.Insert( index, element );
      m_visualCollectionLookupDictionary.Add( element, null );
      m_visualParent.InvalidateMeasure();
    }

    public void RemoveAt( int index )
    {
      UIElement element = m_visualCollection[ index ] as UIElement;

      if( element != null )
      {
        m_visualCollectionLookupDictionary.Remove( element );
      }

      m_visualCollection.RemoveAt( index );
      m_visualParent.InvalidateMeasure();
    }

    public UIElement this[ int index ]
    {
      get
      {
        return ( m_visualCollection[ index ] as UIElement );
      }
      set
      {
        if( value == null )
          throw new ArgumentNullException( "value" );

        // Ensure to contain the value in the lookup dictionary
        if( !m_visualCollectionLookupDictionary.ContainsKey( value ) )
        {
          m_visualCollectionLookupDictionary.Add( value, null );
        }

        if( m_visualCollection[ index ] == value )
          return;

        m_visualCollection[ index ] = value;

        m_visualParent.InvalidateMeasure();
      }
    }

    #endregion

    #region ICollection<UIElement> Members

    public void Add( UIElement element )
    {
      if( element == null )
        throw new ArgumentNullException( "element" );

      m_visualCollection.Add( element );
      m_visualCollectionLookupDictionary.Add( element, null );
      m_visualParent.InvalidateMeasure();
    }

    public void Clear()
    {
      if( m_visualCollection.Count == 0 )
        return;

      m_visualCollection.Clear();
      m_visualCollectionLookupDictionary.Clear();
      m_visualParent.InvalidateMeasure();
    }

    public bool Contains( UIElement element )
    {
      return m_visualCollectionLookupDictionary.ContainsKey( element );
    }

    public void CopyTo( UIElement[] array, int index )
    {
      m_visualCollection.CopyTo( array, index );
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
        return false;
      }
    }

    public bool Remove( UIElement element )
    {
      m_visualCollection.Remove( element );
      m_visualCollectionLookupDictionary.Remove( element );
      m_visualParent.InvalidateMeasure();

      return true;
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

    #region Enumerator Private Class

    private class Enumerator : IEnumerator<UIElement>
    {
      public Enumerator( IEnumerator nonGenericEnumerator )
      {
        if( nonGenericEnumerator == null )
          throw new ArgumentNullException( "nonGenericEnumerator" );

        m_enumerator = nonGenericEnumerator;
      }

      UIElement IEnumerator<UIElement>.Current
      {
        get
        {
          return m_enumerator.Current as UIElement;
        }
      }

      void IDisposable.Dispose()
      {
        // Nothing to do
      }

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

      IEnumerator m_enumerator;
    }

    #endregion

    #region Private Fields

    private readonly Dictionary<UIElement, object> m_visualCollectionLookupDictionary;
    private readonly VisualCollection m_visualCollection;
    private readonly UIElement m_visualParent;

    #endregion
  }
}
