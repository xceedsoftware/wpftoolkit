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

namespace Xceed.Wpf.DataGrid
{
  internal class DeferredOperation
  {
    internal static DeferredOperation RefreshDistinctValuesOperation = new DeferredOperation( DeferredOperationAction.RefreshDistincValues, false );

    internal static DeferredOperation RefreshDistinctValuesOperationWithFilteredItemsChanged = new DeferredOperation( DeferredOperationAction.RefreshDistincValues, true );

    public DeferredOperation( DeferredOperationAction action, bool filteredItemsChanged )
      : this( action, -1, -1, null )
    {
      m_filteredItemsChanged = filteredItemsChanged;
    }

    public DeferredOperation( DeferredOperationAction action, object dataItem )
    {
      m_action = action;
      m_dataItem = dataItem;
    }

    public DeferredOperation( DeferredOperationAction action, IList items )
    {
      m_action = action;
      m_newItems = items;
    }

    public DeferredOperation( DeferredOperationAction action, int startingIndex, IList items )
      : this( action, -1, startingIndex, items )
    {
    }

    public DeferredOperation( DeferredOperationAction action, int newSourceItemCount, int startingIndex, IList items )
    {
      // newSourceItemCount is for when we are adding item in a IBindingList
      // It is use to detect we 2 add are in fact only one, bcause the second one is just for confirming the first one.
      m_action = action;
      m_newSourceItemCount = newSourceItemCount;

      switch( action )
      {
        case DeferredOperationAction.Add:
        case DeferredOperationAction.Replace:
          {
            m_newItems = items;
            m_newStartingIndex = startingIndex;
            break;
          }

        case DeferredOperationAction.Remove:
          {
            m_oldItems = items;
            m_oldStartingIndex = startingIndex;
            break;
          }

        case DeferredOperationAction.Refresh:
        case DeferredOperationAction.Resort:
        case DeferredOperationAction.Regroup:
        case DeferredOperationAction.RefreshDistincValues:
          break;

        default:
          {
            throw new ArgumentException( "An attempt was made to use the " + action.ToString() + " action, which is not supported by this constructor." );
          }
      }
    }

    public DeferredOperation( DeferredOperationAction action, int newSourceItemCount, int newStartingIndex, IList newItems, int oldStartingIndex, IList oldItems )
    {
      // newSourceItemCount is for when we are adding item in a IBindingList
      // It is use to detect that 2 add are in fact only one, because the second one is just for confirming the first one.
      m_action = action;
      m_newSourceItemCount = newSourceItemCount;
      m_newItems = newItems;
      m_newStartingIndex = newStartingIndex;
      m_oldItems = oldItems;
      m_oldStartingIndex = oldStartingIndex;
    }

    #region Action Property

    public DeferredOperationAction Action
    {
      get
      {
        return m_action;
      }
    }

    private DeferredOperationAction m_action;

    #endregion Action Property

    #region NewSourceItemCount Property

    public int NewSourceItemCount
    {
      get
      {
        return m_newSourceItemCount;
      }
    }

    private int m_newSourceItemCount;

    #endregion NewSourceItemCount Property

    #region NewStartingIndex Property

    public int NewStartingIndex
    {
      get
      {
        return m_newStartingIndex;
      }
    }

    private int m_newStartingIndex;

    #endregion NewStartingIndex Property

    #region NewItems Property

    public IList NewItems
    {
      get
      {
        return m_newItems;
      }
    }

    private IList m_newItems;

    #endregion NewItems Property

    #region OldStartingIndex Property

    public int OldStartingIndex
    {
      get
      {
        return m_oldStartingIndex;
      }
    }

    private int m_oldStartingIndex;

    #endregion OldStartingIndex Property

    #region OldItems Property

    public IList OldItems
    {
      get
      {
        return m_oldItems;
      }
    }

    private IList m_oldItems;

    #endregion OldItems Property

    #region DataItem Property

    public object DataItem
    {
      get
      {
        return m_dataItem;
      }
    }

    private object m_dataItem;

    #endregion

    #region FilteredItemsChanged Property

    public bool FilteredItemsChanged
    {
      get
      {
        return m_filteredItemsChanged;
      }
    }

    private bool m_filteredItemsChanged;

    #endregion FilteredItemsChanged Property

    internal enum DeferredOperationAction
    {
      Unknow = 0,
      Add = 1,
      Move = 2,
      Remove = 3,
      Replace = 4,
      Refresh = 5,
      Resort = 6,
      Regroup = 7,
      RefreshDistincValues = 8,
      ResetItem = 9,
      RefreshUnboundItemProperties = 10
    }
  }
}
