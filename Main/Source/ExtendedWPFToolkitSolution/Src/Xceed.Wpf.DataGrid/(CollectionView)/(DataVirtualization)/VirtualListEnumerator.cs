﻿/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class VirtualListEnumerator : IEnumerator<object>, IEnumerator
  {
    public VirtualListEnumerator( VirtualList virtualList )
    {
      if( virtualList == null )
        throw new ArgumentNullException( "virtualList" );

      m_virtualList = virtualList;

      m_version = m_virtualList.PagingManager.Version;

      m_orderedFilledVirtualPages =
        m_virtualList.TableOfContent.VirtualPages.OrderBy(
        virtualPage => virtualPage.StartDataIndex ).Where(
          virtualPage => virtualPage.IsFilled ).ToArray();
    }

    #region INTERNAL PROPERTIES

    internal bool BeforeStart
    {
      get
      {
        return m_beforeStart;
      }
    }

    internal bool AfterEnd
    {
      get
      {
        return m_afterEnd;
      }
    }

    #endregion INTERNAL PROPERTIES

    #region IDisposable Members

    public void Dispose()
    {
      m_currentItemInfo = null;
      m_orderedFilledVirtualPages = null;
      m_virtualList = null;
    }

    #endregion IDisposable Members

    #region IEnumerator<object> Members

    public object Current
    {
      get
      {
        return ( m_currentItemInfo == null ) ? null : m_currentItemInfo.DataItem;
      }
    }

    #endregion IEnumerator<object> Members

    #region IEnumerator Members

    object IEnumerator.Current
    {
      get
      {
        return ( m_currentItemInfo == null ) ? null : m_currentItemInfo.DataItem;
      }
    }

    public bool MoveNext()
    {
      if( m_version != m_virtualList.PagingManager.Version )
        throw new InvalidOperationException( "TODO: Collection was modified." );

      if( m_beforeStart )
        m_beforeStart = false;

      if( m_currentPageIndex < m_orderedFilledVirtualPages.Length )
      {
        VirtualPage virtualPage = m_orderedFilledVirtualPages[ m_currentPageIndex ];

        m_currentItemInfo = virtualPage[ m_currentItemIndex ];

        m_currentItemIndex++;

        if( m_currentItemIndex >= virtualPage.Count )
        {
          m_currentItemIndex = 0;
          m_currentPageIndex++;
        }

        return true;
      }
      else
      {
        m_currentItemInfo = null;
        m_afterEnd = true;

        return false;
      }
    }

    public void Reset()
    {
      m_currentItemInfo = null;
      m_currentItemIndex = 0;
      m_currentPageIndex = 0;
      m_afterEnd = false;
      m_beforeStart = true;
    }

    #endregion IEnumerator Members

    #region PRIVATE FIELDS

    private VirtualList m_virtualList;
    private VirtualPage[] m_orderedFilledVirtualPages;

    private int m_version;
    private VirtualizedItemInfo m_currentItemInfo;
    private int m_currentPageIndex;
    private int m_currentItemIndex;

    private bool m_beforeStart;
    private bool m_afterEnd;

    #endregion PRIVATE FIELDS
  }
}
