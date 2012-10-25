/************************************************************************

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
using System.Text;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal class ItemContextVisitor : IDataGridContextVisitor
  {
    public ItemContextVisitor( bool isTableFlowView )
    {
      m_isTableFlowView = isTableFlowView;
    }

    #region ParentDataGridContext

    private DataGridContext m_parentDataGridContext;

    public DataGridContext ParentDataGridContext
    {
      get
      {
        return m_parentDataGridContext;
      }
    }

    #endregion ParentDataGridContext

    #region Item Property

    private object m_item;

    public object Item
    {
      get
      {
        return m_item;
      }
    }

    #endregion Item Property

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
      if( !m_isTableFlowView )
      {
        m_item = item;
        m_parentDataGridContext = sourceContext;
        m_success = true;
        stopVisit = true;
        return;
      }

      UIElement dataRow = sourceContext.CustomItemContainerGenerator.ContainerFromItem( item ) as UIElement;

      if( dataRow != null )
      {
        if( !ScrollTip.IsItemInView( dataRow, sourceContext.DataGridControl.ItemsHost ) )
        {
          stopVisit = true;
        }
        else if( !ScrollTip.IsDataItemHiddenBySticky( sourceContext, item ) )
        {
          m_item = item;
          m_parentDataGridContext = sourceContext;
          m_success = true;
          stopVisit = true;
        }
      }
    }

    public void Visit( DataGridContext sourceContext, System.Windows.Data.CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded, ref bool stopVisit )
    {
      throw new NotSupportedException( "The ItemAndDataGridContextVisitor is only capable of handling data items." );
    }

    public void Visit( DataGridContext sourceContext, System.Windows.DataTemplate headerFooter, ref bool stopVisit )
    {
      throw new NotSupportedException( "The ItemAndDataGridContextVisitor is only capable of handling data items." );
    }

    public void Visit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter, ref bool stopVisit )
    {
      throw new NotSupportedException( "The ItemAndDataGridContextVisitor is only capable of handling data items." );
    }

    #endregion

    private bool m_isTableFlowView;
  }
}
