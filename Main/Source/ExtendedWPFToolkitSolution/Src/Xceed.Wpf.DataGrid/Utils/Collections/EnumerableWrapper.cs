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
using System.Text;
using System.Collections;

namespace Xceed.Utils.Collections
{
  internal class EnumerableWrapper<T> : IEnumerable<T> where T : class
  {
    public EnumerableWrapper( IEnumerable collection )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );

      m_collection = collection;
    }

    #region IEnumerable<T> Members

    public IEnumerator<T> GetEnumerator()
    {
      return new EnumeratorWrapper<T>( m_collection.GetEnumerator() );
    }

    #endregion

    #region IEnumerable Members

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return m_collection.GetEnumerator();
    }

    #endregion

    private IEnumerable m_collection;

    private class EnumeratorWrapper<U> : IEnumerator<U> where U : class
    {
      public EnumeratorWrapper( IEnumerator enumerator )
      {
        m_wrappedEnumerator = enumerator;
      }

      #region IEnumerator<U> Members

      public U Current
      {
        get
        {
          return m_wrappedEnumerator.Current as U;
        }
      }

      #endregion

      #region IDisposable Members

      public void Dispose()
      {
        IDisposable disposable = m_wrappedEnumerator as IDisposable;
        if(disposable != null)
        {
          disposable.Dispose();
        }
      }

      #endregion

      #region IEnumerator Members

      object System.Collections.IEnumerator.Current
      {
        get
        {
          return m_wrappedEnumerator.Current;
        }
      }

      public bool MoveNext()
      {
        return m_wrappedEnumerator.MoveNext();
      }

      public void Reset()
      {
        m_wrappedEnumerator.Reset();
      }

      #endregion

      IEnumerator m_wrappedEnumerator;
    }
  }
}
