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
using System.Windows.Data;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class ItemContextVisitor : IDataGridContextVisitor
  {
    internal ItemContextVisitor( TableflowViewItemsHost itemsHost, double firstItemHiddenRatio )
    {
      m_itemsHost = itemsHost;
      m_firstItemHiddenRatio = firstItemHiddenRatio;
    }

    #region ParentDataGridContext

    public DataGridContext ParentDataGridContext
    {
      get
      {
        return m_parentDataGridContext;
      }
    }

    private DataGridContext m_parentDataGridContext;

    #endregion

    #region Item Property

    public object Item
    {
      get
      {
        return m_item;
      }
    }

    private object m_item;

    #endregion

    #region VisitSuccessful Property

    public bool VisitSuccessful
    {
      get
      {
        return m_success;
      }
    }

    private bool m_success; //false;

    #endregion

    #region IDataGridContextVisitor Members

    public void Visit( DataGridContext sourceContext, ref bool stopVisit )
    {
      throw new NotSupportedException( "The ItemAndDataGridContextVisitor is only capable of handling data items." );
    }

    public void Visit( DataGridContext sourceContext, int startSourceDataItemIndex, int endSourceDataItemIndex, ref bool stopVisit )
    {
      throw new NotSupportedException( "The ItemAndDataGridContextVisitor is only capable of handling data items." );
    }

    public void Visit( DataGridContext sourceContext, int sourceDataItemIndex, object item, ref bool stopVisit )
    {
      // In TableflowView, we must consider that sticky headers may hide the item.
      if( m_itemsHost != null )
      {
        if( m_isFirstItem )
        {
          m_isFirstItem = false;

          // At least half the item is hidden.
          if( m_firstItemHiddenRatio > 0.5d )
            return;
        }

        var areHeadersSticky = TableflowView.GetAreHeadersSticky( sourceContext );
        var areGroupHeadersSticky = TableflowView.GetAreGroupHeadersSticky( sourceContext );
        var areParentRowsSticky = TableflowView.GetAreParentRowsSticky( sourceContext );

        if( areHeadersSticky || areGroupHeadersSticky || areParentRowsSticky )
        {
          var stickyHeadersCount = sourceContext.CustomItemContainerGenerator.GetStickyHeaderCountForIndex( sourceDataItemIndex, areHeadersSticky, areGroupHeadersSticky, areParentRowsSticky );

          if( stickyHeadersCount > m_stickyHeadersSkipped )
          {
            m_stickyHeadersSkipped++;
            return;
          }
        }
      }

      m_item = item;
      m_parentDataGridContext = sourceContext;
      m_success = true;
      stopVisit = true;
    }

    public void Visit( DataGridContext sourceContext, CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded, ref bool stopVisit )
    {
      throw new NotSupportedException( "The ItemAndDataGridContextVisitor is only capable of handling data items." );
    }

    public void Visit( DataGridContext sourceContext, DataTemplate headerFooter, ref bool stopVisit )
    {
      throw new NotSupportedException( "The ItemAndDataGridContextVisitor is only capable of handling data items." );
    }

    public void Visit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter, ref bool stopVisit )
    {
      throw new NotSupportedException( "The ItemAndDataGridContextVisitor is only capable of handling data items." );
    }

    #endregion

    private readonly TableflowViewItemsHost m_itemsHost;
    private readonly double m_firstItemHiddenRatio;
    private bool m_isFirstItem = true;
    private int m_stickyHeadersSkipped; //0
  }
}
