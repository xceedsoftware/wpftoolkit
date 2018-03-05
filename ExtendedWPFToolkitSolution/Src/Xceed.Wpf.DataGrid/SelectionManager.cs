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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid
{
  internal class SelectionManager
  {
    public enum UpdateSelectionSource
    {
      Navigation = 0,
      MouseDown = 1,
      MouseUp = 2,
      SpaceDown = 3,
      RowSelector = 4,
      MouseMove = 5,
      None = 99
    }

    public SelectionManager( DataGridControl owner )
    {
      if( owner == null )
        throw new ArgumentNullException( "owner" );

      m_owner = owner;
    }

    #region IsActive Property

    public bool IsActive
    {
      get
      {
        return m_isActive;
      }
    }

    #endregion

    public IDisposable PushUpdateSelectionSource( UpdateSelectionSource updateSelectionSource )
    {
      // We raise the fromRowSelector flag here since it's possible
      // that the focus is already on the correct Cell for the container
      // mapped to the RowSelector, preventing the SelectionManager to
      // do any processing.
      if( updateSelectionSource == UpdateSelectionSource.RowSelector )
        m_fromRowSelector = true;

      return new UpdateSelectionSourceHelper( this, updateSelectionSource );
    }

    public void EnsureRootSelectedItemAndSelectedIndex()
    {
      // This is done to force the DataGridControl's Root DataGridContext to be in the activated selection changer so that even if no changes were made
      // the CommitSelectionChanger method will be called and the DataGridControl's SelectedItem/SelectedIndex will be rectified (ie: When sorting or grouping).
      this.GetSelectionChanger( m_owner.DataGridContext );
    }

    public void Begin()
    {
      if( m_isActive )
        throw new InvalidOperationException( "An attempt was made to change the selection while a selection-change process is already in progress." );

      m_isActive = true;
    }

    public void Cancel()
    {
      Debug.Assert( m_isActive );
      m_isActive = false;

      foreach( SelectionChanger selectionChanger in m_activatedSelectionChanger.Values )
      {
        selectionChanger.Cleanup();
      }
    }

    public void End( bool allowCancelingOfSelection, bool allowSynchronizeSelectionWithCurrent )
    {
      Debug.Assert( m_isActive );

      var activatedSelectionDeltas = new List<SelectionInfo>();
      var selectionInfo = default( SelectionInfo );

      try
      {
        foreach( var selectionChanger in m_activatedSelectionChanger.Values )
        {
          selectionInfo = selectionChanger.GetSelectionInfo();

          if( !selectionInfo.IsEmpty )
          {
            activatedSelectionDeltas.Add( selectionInfo );
          }
        }

        if( activatedSelectionDeltas.Count > 0 )
        {
          var eventArgs = new DataGridSelectionChangingEventArgs( new ReadOnlyCollection<SelectionInfo>( activatedSelectionDeltas ), allowCancelingOfSelection );

          m_owner.RaiseSelectionChanging( eventArgs );

          if( allowCancelingOfSelection && eventArgs.Cancel )
          {
            this.Cancel();
            return;
          }
        }

        foreach( var selectionChanger in m_activatedSelectionChanger.Values )
        {
          this.CommitSelectionChanger( selectionChanger );
        }

        if( ( allowSynchronizeSelectionWithCurrent ) && ( m_owner.SynchronizeSelectionWithCurrent ) )
        {
          this.UpdateCurrentToSelection();
        }
      }
      finally
      {
        m_isActive = false;
        m_activatedSelectionChanger.Clear();
      }

      // Notify that GlobalSelectedItems has changed if at least one selection has changed.
      if( activatedSelectionDeltas.Count > 0 )
      {
        for( int i = activatedSelectionDeltas.Count - 1; i >= 0; i-- )
        {
          selectionInfo = activatedSelectionDeltas[ i ];

          // Invoke the selection changed on the DataGridContext.
          selectionInfo.DataGridContext.InvokeSelectionChanged( selectionInfo );
        }

        m_owner.RaiseSelectionChanged( new DataGridSelectionChangedEventArgs( new ReadOnlyCollection<SelectionInfo>( activatedSelectionDeltas ) ) );
        m_owner.NotifyGlobalSelectedItemsChanged();
      }
    }

    public bool ToggleItemSelection( SelectionRangePoint rangePosition )
    {
      var group = rangePosition.Group;
      if( group != null )
      {
        bool unselect = false;
        var range = rangePosition.ToSelectionRangeWithItems();

        // When [un]selecting a group, select all children, and recursively
        // the expanded details nodes.
        return this.AffectSelectionRecursivelyOnDetails(
          rangePosition.DataGridContext, range.Range.StartIndex, range.Range.EndIndex,
          unselect );
      }
      else
      {
        return this.ToggleItemSelection(
          rangePosition.DataGridContext,
          rangePosition.ItemIndex, rangePosition.Item );
      }
    }

    public bool ToggleItemSelection( DataGridContext context, int itemIndex, object item )
    {
      if( this.IsItemSelected( context, itemIndex ) )
      {
        return this.UnselectItems( context, new SelectionRangeWithItems( itemIndex, item ) );
      }
      else
      {
        return this.SelectItems( context, new SelectionRangeWithItems( itemIndex, item ) );
      }
    }

    public bool ToggleCellSelection( DataGridContext context, int itemIndex, object item, int columnIndex )
    {
      if( this.IsCellSelected( context, itemIndex, columnIndex ) )
      {
        return this.UnselectCells( context, new SelectionCellRangeWithItems( itemIndex, item, columnIndex ) );
      }
      else
      {
        return this.SelectCells( context, new SelectionCellRangeWithItems( itemIndex, item, columnIndex ) );
      }
    }

    public bool ToggleItemCellsSelection( DataGridContext dataGridContext, int itemIndex, object item )
    {
      SelectedCellsStorage tempStorage = this.GetItemCellStorage( dataGridContext, itemIndex, item );

      bool selectionDone = true;

      if( this.IsItemCellsSelected( dataGridContext, tempStorage ) )
      {
        foreach( SelectionCellRangeWithItems allCellSelection in tempStorage )
        {
          selectionDone &= this.UnselectCells( dataGridContext, allCellSelection );
        }
      }
      else
      {
        foreach( SelectionCellRangeWithItems allCellSelection in tempStorage )
        {
          selectionDone &= this.SelectCells( dataGridContext, allCellSelection );
        }
      }

      return selectionDone;
    }

    public bool SelectCells( DataGridContext dataGridContext, SelectionCellRangeWithItems cellRangeWithItems )
    {
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
      return selectionChanger.SelectCells( cellRangeWithItems );
    }

    public bool SelectItems( SelectionRangePoint position )
    {
      if( position.Group != null )
      {
        // When [un]selecting a group, select all children, and recursively
        // the expanded details nodes.
        var range = position.ToSelectionRangeWithItems();
        return this.AffectSelectionRecursivelyOnDetails(
          position.DataGridContext, range.Range.StartIndex, range.Range.EndIndex,
          false );
      }
      else
      {
        return this.SelectItems( position.DataGridContext, position.ToSelectionRangeWithItems() );
      }
    }

    public bool SelectItems( DataGridContext dataGridContext, SelectionRangeWithItems rangeWithItems )
    {
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
      return selectionChanger.SelectItems( rangeWithItems );
    }

    public bool SelectJustThisCell( DataGridContext dataGridContext, int itemIndex, object item, int columnIndex )
    {
      bool selected = false;
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );

      // Add all the selected context to the activatedSelectionChanger since
      // we will unselect all the selected item from it.
      foreach( DataGridContext selectedDataGridContext in m_owner.SelectedContexts )
      {
        this.GetSelectionChanger( selectedDataGridContext );
      }

      // select only the wanted item.
      foreach( SelectionChanger activatedSelectionChanger in m_activatedSelectionChanger.Values )
      {
        if( activatedSelectionChanger == selectionChanger )
        {
          selected = activatedSelectionChanger.SelectJustThisCell( itemIndex, item, columnIndex );
        }
        else
        {
          activatedSelectionChanger.UnselectAllItems();
          activatedSelectionChanger.UnselectAllCells();
        }
      }

      return selected;
    }

    public bool SelectItemCells( DataGridContext dataGridContext, int itemIndex, object item, bool preserveSelection )
    {
      bool selected = false;
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );

      // Add all the selected context to the activatedSelectionChanger since
      // we will unselect all the selected item from it.
      foreach( DataGridContext selectedDataGridContext in m_owner.SelectedContexts )
      {
        this.GetSelectionChanger( selectedDataGridContext );
      }

      // select only the wanted item.
      foreach( SelectionChanger activatedSelectionChanger in m_activatedSelectionChanger.Values )
      {
        if( activatedSelectionChanger == selectionChanger )
        {
          selected = activatedSelectionChanger.SelectItemCells( itemIndex,
            item,
            dataGridContext.ColumnsByVisiblePosition,
            preserveSelection );
        }
        else
        {
          if( !preserveSelection )
          {
            activatedSelectionChanger.UnselectAllItems();
            activatedSelectionChanger.UnselectAllCells();
          }
        }
      }

      return selected;
    }

    public bool SelectJustThisItem( SelectionRangePoint point )
    {
      bool selected = false;
      var group = point.Group;

      if( group != null )
      {
        this.UnselectAll();
        // This will also recursively select all expanded detail childrens.
        selected = this.SelectItems( point );
      }
      else
      {
        var item = point.Item;
        var itemIndex = point.ItemIndex;
        if( itemIndex != -1 )
        {
          selected = this.SelectJustThisItem( point.DataGridContext, itemIndex, item );
        }
      }

      return selected;
    }

    public bool SelectJustThisItem( DataGridContext dataGridContext, int itemIndex, object item )
    {
      bool selected = false;
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );

      // Add all the selected context to the activatedSelectionChanger since
      // we will unselect all the selected item from it.
      foreach( DataGridContext selectedDataGridContext in m_owner.SelectedContexts )
      {
        this.GetSelectionChanger( selectedDataGridContext );
      }

      // select only the wanted item.
      foreach( SelectionChanger activatedSelectionChanger in m_activatedSelectionChanger.Values )
      {
        if( activatedSelectionChanger == selectionChanger )
        {
          selected = activatedSelectionChanger.SelectJustThisItem( itemIndex, item );
        }
        else
        {
          activatedSelectionChanger.UnselectAllItems();
          activatedSelectionChanger.UnselectAllCells();
        }
      }

      return selected;
    }

    public bool UnselectCells( DataGridContext dataGridContext, SelectionCellRangeWithItems cellRangeWithItems )
    {
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
      return selectionChanger.UnselectCells( cellRangeWithItems );
    }

    public bool UnselectItems( DataGridContext dataGridContext, SelectionRangeWithItems rangeWithItems )
    {
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
      return selectionChanger.UnselectItems( rangeWithItems );
    }

    public void CleanupDeferSelection( DataGridContext dataGridContext )
    {
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
      selectionChanger.CleanupDeferSelection();
    }

    public void UpdateSelectionAfterSourceCollectionChanged( DataGridContext dataGridContext, NotifyCollectionChangedEventArgs e )
    {
      switch( e.Action )
      {
        case NotifyCollectionChangedAction.Replace:
          {
            // When we get a replace with the same instance, we just have nothing to do.
            if( ( e.NewItems.Count == e.OldItems.Count ) && e.NewItems.Cast<object>().SequenceEqual( e.OldItems.Cast<object>() ) )
              break;

            this.Begin();

            try
            {
              // This is done to force the SelectionChangerManager to call the DataGridControl's
              // DataGridContext UpdatePublicSelectionProperties method wheter or not
              // there is any changes to the selection.  This is usefull when resetting due to a Sort operation for instance.
              this.EnsureRootSelectedItemAndSelectedIndex();

              SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
              selectionChanger.UpdateSelectionAfterSourceDataItemReplaced( e );
            }
            finally
            {
              this.End( false, true );
            }

            break;
          }

        case NotifyCollectionChangedAction.Add:
          {
            this.Begin();

            try
            {
              // This is done to force the SelectionChangerManager to call the DataGridControl's
              // DataGridContext UpdatePublicSelectionProperties method wheter or not
              // there is any changes to the selection.  This is usefull when resetting due to a Sort operation for instance.
              this.EnsureRootSelectedItemAndSelectedIndex();

              m_selectionRangeStartPoint = null;
              SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
              selectionChanger.UpdateSelectionAfterSourceDataItemAdded( e );
            }
            finally
            {
              this.End( false, true );
            }

            break;
          }

        case NotifyCollectionChangedAction.Remove:
          {
            this.Begin();

            try
            {
              // This is done to force the SelectionChangerManager to call the DataGridControl's
              // DataGridContext UpdatePublicSelectionProperties method wheter or not
              // there is any changes to the selection.  This is usefull when resetting due to a Sort operation for instance.
              this.EnsureRootSelectedItemAndSelectedIndex();

              m_selectionRangeStartPoint = null;
              SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
              selectionChanger.UpdateSelectionAfterSourceDataItemRemoved( e );
            }
            finally
            {
              this.End( false, true );
            }

            break;
          }

        case NotifyCollectionChangedAction.Move:
        case NotifyCollectionChangedAction.Reset:
          {
            this.Begin();

            try
            {
              // This is done to force the SelectionChangerManager to call the DataGridControl's
              // DataGridContext UpdatePublicSelectionProperties method wheter or not
              // there is any changes to the selection.  This is usefull when resetting due to a Sort operation for instance.
              this.EnsureRootSelectedItemAndSelectedIndex();

              m_selectionRangeStartPoint = null;

              SelectedItemsStorage selectedItemsStorage = dataGridContext.SelectedItemsStore;
              int selectedItemsCount = selectedItemsStorage.Count;
              SelectedCellsStorage selectedCellsStorage = dataGridContext.SelectedCellsStore;
              int selectedCellsCount = selectedCellsStorage.Count;
              SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );

              int newItemCount = dataGridContext.Items.Count;
              SelectionRange maxItemRange;

              if( newItemCount == 0 )
              {
                maxItemRange = SelectionRange.Empty;
              }
              else
              {
                maxItemRange = new SelectionRange( 0, newItemCount - 1 );
              }

              if( selectedItemsCount > 0 )
              {
                selectionChanger.UnselectAllItems();

                for( int i = 0; i < selectedItemsCount; i++ )
                {
                  SelectionRangeWithItems rangeWithItems = selectedItemsStorage[ i ];
                  object[] items = rangeWithItems.Items;

                  if( items == null )
                  {
                    SelectionRange itemRange = rangeWithItems.Range;
                    SelectionRange rangeIntersection = itemRange.Intersect( maxItemRange );

                    if( rangeIntersection != itemRange )
                    {
                      if( rangeIntersection.IsEmpty )
                        continue;

                      rangeWithItems = new SelectionRangeWithItems( rangeIntersection, null );
                    }

                    selectionChanger.SelectItems( rangeWithItems );
                  }
                  else
                  {
                    CollectionView sourceItems = dataGridContext.Items;
                    int itemsCount = items.Length;
                    SelectionRange range = rangeWithItems.Range;

                    for( int j = 0; j < itemsCount; j++ )
                    {
                      object item = items[ j ];
                      object replacement = e.GetReplacement( item ) ?? item;

                      int index = sourceItems.IndexOf( replacement );
                      if( index >= 0 )
                      {
                        selectionChanger.SelectItems( new SelectionRangeWithItems( index, replacement ) );
                      }
                    }
                  }
                }
              }

              if( selectedCellsCount > 0 )
              {
                selectionChanger.UnselectAllCells();

                for( int i = 0; i < selectedCellsCount; i++ )
                {
                  SelectionCellRangeWithItems cellRangeWithItems = selectedCellsStorage[ i ];
                  SelectionRangeWithItems itemRangeWithItems = cellRangeWithItems.ItemRangeWithItems;
                  object[] items = itemRangeWithItems.Items;

                  if( items == null )
                  {
                    SelectionRange itemRange = itemRangeWithItems.Range;
                    SelectionRange itemRangeIntersection = itemRange.Intersect( maxItemRange );

                    if( itemRangeIntersection != itemRange )
                    {
                      if( itemRangeIntersection.IsEmpty )
                        continue;

                      cellRangeWithItems = new SelectionCellRangeWithItems( itemRangeIntersection, null, cellRangeWithItems.ColumnRange );
                    }

                    selectionChanger.SelectCells( cellRangeWithItems );
                  }
                  else
                  {
                    CollectionView sourceItems = dataGridContext.Items;
                    int itemsCount = items.Length;
                    SelectionRange itemRange = cellRangeWithItems.ItemRange;

                    for( int j = 0; j < itemsCount; j++ )
                    {
                      object item = items[ j ];
                      int index = sourceItems.IndexOf( item );

                      if( index != -1 )
                      {
                        selectionChanger.SelectCells( new SelectionCellRangeWithItems(
                          new SelectionRange( index ), new object[] { item }, cellRangeWithItems.ColumnRange ) );
                      }
                    }
                  }
                }
              }
            }
            finally
            {
              this.End( false, true );
            }

            break;
          }
      }
    }

    public void UpdateSelection(
      SelectionRangePoint oldPosition,
      SelectionRangePoint newPosition,
      bool oldFocusWasInTheGrid,
      bool rowIsBeingEditedAndCurrentRowNotChanged,
      Nullable<UpdateSelectionSource> updateSelectionSourceParam )
    {
      if( oldPosition != null && oldPosition.Group != null )
      {
        oldPosition = null;
      }

      if( newPosition != null && newPosition.Group != null )
      {
        newPosition = null;
      }

      if( newPosition == null )
        return;

      UpdateSelectionSource updateSelectionSource = ( updateSelectionSourceParam.HasValue )
        ? updateSelectionSourceParam.Value : m_currentUpdateSelectionSource;

      // Force the node tree to be created since detail can be deleted during that node creation and selection uptade will be call
      // That way we prevent selection code reentrance.
      this.EnsureNodeTreeCreatedOnAllSubContext( m_owner.DataGridContext );

      if( ( updateSelectionSource != UpdateSelectionSource.RowSelector )
          && ( updateSelectionSource != UpdateSelectionSource.MouseUp )
          && ( updateSelectionSource != UpdateSelectionSource.MouseMove ) )
      {
        // Reset the fromRowSelector to ensure nothing special is done
        // on next MouseUp
        m_fromRowSelector = false;
      }

      if( updateSelectionSource == UpdateSelectionSource.None )
        return;

      this.Begin();

      try
      {
        switch( m_owner.SelectionModeToUse )
        {
          case SelectionMode.Single:
            {
              this.DoSingleSelection(
                newPosition,
                oldFocusWasInTheGrid, rowIsBeingEditedAndCurrentRowNotChanged, updateSelectionSource );

              break;
            }

          case SelectionMode.Multiple:
            {
              this.DoMultipleSelection(
                newPosition, rowIsBeingEditedAndCurrentRowNotChanged, updateSelectionSource );

              break;
            }

          case SelectionMode.Extended:
            {
              this.DoExtendedSelection(
                oldPosition, newPosition,
                oldFocusWasInTheGrid, rowIsBeingEditedAndCurrentRowNotChanged, updateSelectionSource );

              break;
            }
        }
      }
      finally
      {
        this.End( true, false );
      }
    }

    internal void UnselectAll()
    {
      // Add all the selected context to the activatedSelectionChanger since
      // we will unselect all the selected item from it.
      foreach( DataGridContext selectedDataGridContext in m_owner.SelectedContexts )
      {
        this.GetSelectionChanger( selectedDataGridContext );
      }

      // Unselect all items and cells.
      foreach( SelectionChanger activatedSelectionChanger in m_activatedSelectionChanger.Values )
      {
        activatedSelectionChanger.UnselectAllItems();
        activatedSelectionChanger.UnselectAllCells();
      }
    }

    internal void UnselectAllItems()
    {
      // Add all the selected context to the activatedSelectionChanger since
      // we will unselect all the selected item from it.
      foreach( DataGridContext selectedDataGridContext in m_owner.SelectedContexts )
      {
        this.GetSelectionChanger( selectedDataGridContext );
      }

      // Unselect all item.
      foreach( SelectionChanger activatedSelectionChanger in m_activatedSelectionChanger.Values )
      {
        activatedSelectionChanger.UnselectAllItems();
      }
    }

    internal void UnselectAllItems( DataGridContext dataGridContext )
    {
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
      selectionChanger.UnselectAllItems();
    }

    internal void UnselectAllCells()
    {
      // Add all the selected context to the activatedSelectionChanger since
      // we will unselect all the selected item from it.
      foreach( DataGridContext selectedDataGridContext in m_owner.SelectedContexts )
      {
        this.GetSelectionChanger( selectedDataGridContext );
      }

      // Unselect all item.
      foreach( SelectionChanger activatedSelectionChanger in m_activatedSelectionChanger.Values )
      {
        activatedSelectionChanger.UnselectAllCells();
      }
    }

    internal void UnselectAllCells( DataGridContext dataGridContext )
    {
      SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
      selectionChanger.UnselectAllCells();
    }

    private void EnsureNodeTreeCreatedOnAllSubContext( DataGridContext context )
    {
      // context.GetChildContexts() do a EnsureNodeTreeCreated(), no need to do one before calling it

      foreach( var subContext in context.GetChildContexts() )
      {
        this.EnsureNodeTreeCreatedOnAllSubContext( subContext );
      }
    }

    private void DoSingleSelection(
      SelectionRangePoint newPosition,
      bool oldFocusWasInTheGrid,
      bool rowIsBeingEditedAndCurrentRowNotChanged,
      UpdateSelectionSource updateSelectionSource )
    {

      //Group selection is not valid in single selection mode.
      if( newPosition == null || newPosition.Group != null )
        return;

      bool applySelection = true;

      if( updateSelectionSource == UpdateSelectionSource.Navigation )
      {
        bool isPageKeys = Keyboard.IsKeyDown( Key.PageUp )
              || Keyboard.IsKeyDown( Key.PageDown )
              || Keyboard.IsKeyDown( Key.End )
              || Keyboard.IsKeyDown( Key.Home );

        bool isCtrlPressed = ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control;

        //Special cases where selection should not be applied:
        //1. Focus was not in the datagrid
        //2. Ctrl is pressed but not the "page keys".
        if( !oldFocusWasInTheGrid || ( isCtrlPressed && !isPageKeys ) )
        {
          applySelection = false;
        }
      }

      // Special case for RowSelector : We do not want the
      // MouseUp to clear the Selection when it was just set
      // by the RowSelector
      m_fromRowSelector |= ( updateSelectionSource == UpdateSelectionSource.RowSelector );

      if( applySelection )
      {
        //Handle the case where the selected item is not a datarow/cell.
        bool isValid = ( m_owner.SelectionUnit == SelectionUnit.Row )
          ? ( newPosition.ItemIndex != -1 )
          : ( newPosition.ItemIndex != -1 ) && ( newPosition.ColumnIndex != -1 );

        // In cell selection, Do not remove selection when it comes from the RowSelector
        bool cellSelection = ( m_owner.SelectionUnit == SelectionUnit.Cell );

        this.ApplySelection( null, newPosition, m_fromRowSelector, isValid,
          doRangeSelection: false,
          allowRangeUnselection: false,
          toggleSelection: false,
          keepSelection: m_fromRowSelector && cellSelection );
      }
    }

    private void DoMultipleSelection( SelectionRangePoint newPosition, bool rowIsBeingEditedAndCurrentRowNotChanged, UpdateSelectionSource updateSelectionSource )
    {
      if( newPosition == null )
        return;

      bool applySelection = true;
      bool fromRowSelector = ( updateSelectionSource == UpdateSelectionSource.RowSelector ) || m_fromRowSelector;
      bool toggleSelection = true;

      //Handle the case where the selected item is not a datarow/cell.
      bool isValid = ( m_owner.SelectionUnit == SelectionUnit.Row ) || fromRowSelector
        ? ( newPosition.Group != null ) || ( newPosition.ItemIndex != -1 )
        : ( newPosition.ItemIndex != -1 ) && ( newPosition.ColumnIndex != -1 );

      switch( updateSelectionSource )
      {
        case UpdateSelectionSource.RowSelector:
          {
            m_fromRowSelector = true;
            goto case UpdateSelectionSource.MouseDown;
          }
        case UpdateSelectionSource.MouseDown:
          {
            m_updateSelectionOnNextMouseUp =
              newPosition.GetIsSelected( m_owner.SelectionUnit )
              && !m_owner.UseDragToSelectBehavior;

            if( m_updateSelectionOnNextMouseUp )
            {
              applySelection = false;
            }
            break;
          }

        case UpdateSelectionSource.MouseUp:
          {
            if( !isValid || !m_updateSelectionOnNextMouseUp || rowIsBeingEditedAndCurrentRowNotChanged )
            {
              applySelection = false;
            }

            // Reset the fromRowSelector to ensure nothing special is done
            // on next MouseUp (failsafe, should be reset by UpdateSelection)
            m_fromRowSelector = false;

            // We reset the m_updateSelectionOnNextMouseUp to be sure when no focus is moved
            // around that mouse up will toggle selection
            m_updateSelectionOnNextMouseUp = true;
            break;
          }

        case UpdateSelectionSource.SpaceDown:
          {
            break;
          }
        case UpdateSelectionSource.MouseMove:
          {
            //This is no "unselection" or "toggle" when doing drag selection.
            toggleSelection = false;
            break;
          }

        default:
          {
            //Fallback for "None", "Navigation" or other futur cases:
            applySelection = false;
            break;
          }
      }

      if( applySelection )
      {
        //Never do range selections, Always keep selection, always toggle.
        this.ApplySelection( null, newPosition,
          fromRowSelector, isValid,
          doRangeSelection: false,
          allowRangeUnselection: false,
          toggleSelection: toggleSelection,
          keepSelection: true );
      }
    }

    private void DoExtendedSelection(
      SelectionRangePoint oldPosition,
      SelectionRangePoint newPosition,
      bool oldFocusWasInTheGrid,
      bool rowIsBeingEditedAndCurrentRowNotChanged,
      UpdateSelectionSource updateSelectionSource )
    {
      if( newPosition == null )
        return;

      Group group = newPosition.Group;
      DataGridContext dataGridContext = newPosition.DataGridContext;
      object item = newPosition.Item;
      int sourceDataItemIndex = newPosition.ItemIndex;
      int columnIndex = newPosition.ColumnIndex;

      bool shift = ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift;
      bool ctrl = ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control;

      //Default behavior
      //Ctrl : Keep the current selection
      //Shift : Do a range selection and Do not update the selection anchor.
      //Ctrl+!Shift: Toggle current.

      bool keepSelection = ctrl;
      bool toggleSelection = ctrl && !shift;
      bool doRangeSelection = shift;
      bool allowRangeUnselection = ctrl;
      bool updateRangeStartPosition = !shift;
      bool applySelection = true;
      bool fromRowSelector = ( updateSelectionSource == UpdateSelectionSource.RowSelector ) || m_fromRowSelector;
      bool cellSelection = ( m_owner.SelectionUnit == SelectionUnit.Cell );

      //Handle the case where the selected item is not a datarow/cell.
      bool isValid = ( m_owner.SelectionUnit == SelectionUnit.Row || fromRowSelector )
        ? ( newPosition.Group != null ) || ( newPosition.ItemIndex != -1 )
        : ( newPosition.ItemIndex != -1 ) && ( newPosition.ColumnIndex != -1 );

      //Special case management in relation to update selection source
      switch( updateSelectionSource )
      {
        case UpdateSelectionSource.Navigation:
          {
            // Focus was not in the grid, do nothing.
            if( !oldFocusWasInTheGrid )
            {
              updateRangeStartPosition = false;
              applySelection = false;
              break;
            }

            // About every behavior of the selection is different in 
            // keyboard navigation...

            // Navigation never toggle. "Ctrl+Space" is the way
            // to select with the keyboard
            toggleSelection = false;

            // Do not make range selection for the "Shift-Tab" case...
            // Ignore the "Shift", just as it was not pressed.
            if( Keyboard.IsKeyDown( Key.Tab ) )
            {
              shift = false;
              doRangeSelection = false;
            }

            // Anchor also is keeped when Ctrl is pressed
            updateRangeStartPosition = !ctrl && !shift;

            // Ctrl + Page[Up/Down] and Ctrl+ [Home/End] are special ways
            // to scroll using navigation, and does reset the selection.
            if( !shift && ctrl )
            {
              if( Keyboard.IsKeyDown( Key.PageUp )
                || Keyboard.IsKeyDown( Key.PageDown )
                || Keyboard.IsKeyDown( Key.End )
                || Keyboard.IsKeyDown( Key.Home ) )
              {
                keepSelection = false;
              }
              else
              {
                //Do not apply selection when moving with the Ctrl pressed.
                applySelection = false;
              }
            }

            if( !keepSelection && !doRangeSelection )
            {
              bool isSelected = ( cellSelection )
                ? dataGridContext.SelectedCellsStore.Contains( sourceDataItemIndex, columnIndex )
                : dataGridContext.SelectedItemsStore.Contains( sourceDataItemIndex );
              if( group == null
                && isSelected
                && rowIsBeingEditedAndCurrentRowNotChanged )
              {
                applySelection = false;
              }
            }
            break;
          }

        case UpdateSelectionSource.MouseDown:
          {
            if( doRangeSelection )
            {
              m_updateSelectionOnNextMouseUp = false;
            }
            else
            {
              m_updateSelectionOnNextMouseUp =
                newPosition.GetIsSelected( m_owner.SelectionUnit )
                && !m_owner.UseDragToSelectBehavior;
              // If the target item is already selected, do not update
              // the selection on mouse down, differ this to mouse up.
              if( m_updateSelectionOnNextMouseUp )
              {
                applySelection = false;

                if( isValid )
                {
                  if( !( ctrl && !shift ) )
                  {
                    m_updateSelectionOnNextMouseUp = !rowIsBeingEditedAndCurrentRowNotChanged;
                  }
                }
              }
            }
            break;
          }
        case UpdateSelectionSource.RowSelector:
          {
            if( doRangeSelection )
            {
              m_updateSelectionOnNextMouseUp = false;
            }
            else
            {
              newPosition.ColumnIndex = 0;
              isValid = ( newPosition.ItemIndex != -1 );

              if( isValid )
              {
                // If the target item is already selected, do not update
                // the selection on mouse down, differ this to mouse up.
                m_updateSelectionOnNextMouseUp =
                  newPosition.GetIsSelected( SelectionUnit.Row )
                  && !m_owner.UseDragToSelectBehavior;

                if( m_updateSelectionOnNextMouseUp )
                {
                  applySelection = false;
                }
              }
            }
            break;
          }

        case UpdateSelectionSource.MouseUp:
          {
            if( !isValid || !m_updateSelectionOnNextMouseUp )
            {
              applySelection = false;
              updateRangeStartPosition = false;
            }


            // Reset the fromRowSelector to ensure nothing special is done
            // on next MouseUp (failsafe, should be reset by UpdateSelection)
            m_fromRowSelector = false;

            // We reset the m_updateSelectionOnNextMouseUp to be sure when no focus is moved
            // around that mouse up will toggle selection
            m_updateSelectionOnNextMouseUp = true;

            break;
          }

        case UpdateSelectionSource.SpaceDown:
          {
            //Space Will add to selection without removing the existing
            //selection. (ie. Like Windows Explorer)
            if( !ctrl && !shift )
            {
              keepSelection = true;
            }
            break;
          }
        case UpdateSelectionSource.MouseMove:
          {
            toggleSelection = false;
            doRangeSelection = true;
            updateRangeStartPosition = false;
            break;
          }
        default:
          {
            //Fallback for "None" or other futur cases:
            applySelection = false;
            updateRangeStartPosition = false;
            break;
          }
      }

      if( updateRangeStartPosition )
      {
        m_selectionRangeStartPoint = newPosition;
      }

      if( applySelection )
      {
        this.ApplySelection(
          oldPosition, newPosition,
          fromRowSelector,
          isValid,
          doRangeSelection,
          allowRangeUnselection,
          toggleSelection, keepSelection );
      }
    }

    private void ApplySelection(
      SelectionRangePoint oldPosition,
      SelectionRangePoint newPosition,
      bool fromRowSelector,
      bool isValid,
      bool doRangeSelection,
      bool allowRangeUnselection,
      bool toggleSelection,
      bool keepSelection )
    {
      if( !isValid )
      {
        if( !keepSelection )
        {
          this.UnselectAll();
        }
        return;
      }

      if( doRangeSelection )
      {
        this.DoRangeSelection( oldPosition, newPosition, keepSelection, fromRowSelector, allowRangeUnselection );
        return;
      }

      DataGridContext dataGridContext = newPosition.DataGridContext;
      object item = newPosition.Item;
      int sourceDataItemIndex = newPosition.ItemIndex;
      int columnIndex = newPosition.ColumnIndex;

      Action toggleAction;
      Action selectAndClearAction;
      Action addToSelectionAction;

      if( m_owner.SelectionUnit == SelectionUnit.Cell )
      {
        if( fromRowSelector )
        {
          toggleAction = () => this.ToggleItemCellsSelection( dataGridContext, sourceDataItemIndex, item );
          selectAndClearAction = () => this.SelectItemCells( dataGridContext, sourceDataItemIndex, item, false );
          addToSelectionAction = () => this.SelectItemCells( dataGridContext, sourceDataItemIndex, item, true );
        }
        else
        {
          toggleAction = () => this.ToggleCellSelection( dataGridContext, sourceDataItemIndex, item, columnIndex );
          selectAndClearAction = () => this.SelectJustThisCell( dataGridContext, sourceDataItemIndex, item, columnIndex );
          addToSelectionAction = () => this.SelectCells( dataGridContext, new SelectionCellRangeWithItems( sourceDataItemIndex, item, columnIndex ) );
        }
      }
      else
      {
        toggleAction = () => this.ToggleItemSelection( newPosition );
        selectAndClearAction = () => this.SelectJustThisItem( newPosition );
        addToSelectionAction = () => this.SelectItems( newPosition );
      }

      if( toggleSelection )
      {
        toggleAction();
      }
      else if( !keepSelection )
      {
        selectAndClearAction();
      }
      else
      {
        addToSelectionAction();
      }
    }

    private void DoRangeSelection(
      SelectionRangePoint oldPosition,
      SelectionRangePoint newPosition,
      bool keepPreviousSelection,
      bool fromRowSelector,
      bool allowRangeUnselection )
    {
      // Un-select a range if we keep previous selection, and
      // the selection anchor is a unselected item.

      //Group selection not supported. Be sure to have no ranges
      //that will be relative to a group position.
      if( newPosition != null && newPosition.Group != null )
        return;

      if( oldPosition != null && oldPosition.Group != null )
      {
        oldPosition = null;
      }

      if( m_selectionRangeStartPoint != null && m_selectionRangeStartPoint.Group != null )
      {
        m_selectionRangeStartPoint = null;
      }

      if( m_selectionRangeStartPoint == null )
      {
        m_selectionRangeStartPoint = oldPosition ?? newPosition;
      }

      bool unselect = false;
      if( allowRangeUnselection && keepPreviousSelection )
      {
        if( m_owner.SelectionUnit == SelectionUnit.Cell )
        {
          unselect = ( fromRowSelector )
            ? !this.IsItemCellsSelected( m_selectionRangeStartPoint )
            : !this.IsCellSelected( m_selectionRangeStartPoint );
        }
        else
        {
          unselect = !this.IsItemSelected( m_selectionRangeStartPoint );
        }
      }

      this.DoRangeSelectionNoGroupSupport( newPosition, keepPreviousSelection, fromRowSelector, unselect );
    }

    private void DoRangeSelectionNoGroupSupport( SelectionRangePoint newPosition, bool keepPreviousSelection, bool fromRowSelector, bool unselect )
    {
      int starting_index = m_selectionRangeStartPoint.ItemGlobalIndex;
      int ending_index = newPosition.ItemGlobalIndex;

      if( !keepPreviousSelection )
      {
        this.UnselectAll();
      }

      // here I need to normalize the values to ensure that I'm not catching the Fixed Headers or Fixed Footers
      if( starting_index != -1 && ending_index != -1 )
      {

        int min = Math.Min( starting_index, ending_index );
        int max = Math.Max( starting_index, ending_index );

        SelectionRange[] selectedColumns;

        if( m_owner.SelectionUnit == SelectionUnit.Row )
        {
          selectedColumns = null;
        }
        else
        {
          SelectionRange fullColumnRange;

          // If called from a RowSelector, we do a range
          // selection using all the columns no matter
          // what was the previous anchor
          if( fromRowSelector )
          {
            m_selectionRangeStartPoint.ColumnIndex = 0;
            fullColumnRange = new SelectionRange( 0, Math.Max( 0, newPosition.DataGridContext.ColumnsByVisiblePosition.Count - 1 ) );
          }
          else
          {
            if( m_selectionRangeStartPoint.ColumnIndex == -1 )
              m_selectionRangeStartPoint.ColumnIndex = 0;

            if( newPosition.ColumnIndex == -1 )
              return;

            fullColumnRange = new SelectionRange( m_selectionRangeStartPoint.ColumnIndex, newPosition.ColumnIndex );
          }

          SelectedItemsStorage selectedColumnStore = new SelectedItemsStorage( null );
          selectedColumnStore.Add( new SelectionRangeWithItems( fullColumnRange, null ) );
          int index = 0;

          foreach( ColumnBase column in newPosition.DataGridContext.ColumnsByVisiblePosition )
          {
            if( !column.Visible )
            {
              selectedColumnStore.Remove( new SelectionRangeWithItems( new SelectionRange( index ), null ) );
            }

            index++;
          }

          selectedColumns = selectedColumnStore.ToSelectionRangeArray();
        }

        IDataGridContextVisitable visitable = ( IDataGridContextVisitable )m_owner.DataGridContext;
        bool visitWasStopped;

        visitable.AcceptVisitor(
          min, max,
          new RangeSelectionVisitor( selectedColumns, unselect ),
          DataGridContextVisitorType.ItemsBlock, out visitWasStopped );
      }
    }

    private void AffectSelectionRange( List<SelectionRangePoint> startPath, List<SelectionRangePoint> endPath, bool unselect )
    {
      // For each DataContext depth of each path.
      for( int i = Math.Max( startPath.Count, endPath.Count ) - 1; i >= 0; i-- )
      {
        DataGridContext startContext = null;
        DataGridContext endContext = null;
        int startIndex = -1;
        int endIndex = -1;
        int notUsed;

        if( i < startPath.Count )
        {
          startContext = startPath[ i ].DataGridContext;
          this.ExtractRangePositionIndexes( startPath[ i ], out startIndex, out notUsed );
          if( i != startPath.Count - 1 )
          {
            // Except for the deepest level of the start path,
            // We should not include the start index in the selection
            // This is because the parent item placeholder (ie. the master row) is
            // over/before the displayed children, and is not visually within
            // the range of the selection.
            startIndex++;
          }
        }

        if( i < endPath.Count )
        {
          endContext = endPath[ i ].DataGridContext;
          this.ExtractRangePositionIndexes( endPath[ i ], out notUsed, out endIndex );
        }

        if( startContext != endContext )
        {
          if( startContext != null )
          {
            // This is a sub context unique to the start selection path.
            // Since the selection go beyond this data context, select all items starting
            // at the startIndex to the last item of this context.
            int lastItemIndex = startContext.Items.Count - 1;
            this.AffectSelectionRecursivelyForSelectRange( startContext, startIndex, lastItemIndex, unselect );
          }

          if( endContext != null )
          {
            // This is a sub context unique to the end selection path.
            // Since the selection is starting before this data context, select all items from
            // the first of this data context down to the endIndex.
            this.AffectSelectionRecursivelyForSelectRange( endContext, 0, endIndex, unselect );
          }
        }
        else
        {
          // The context is the same for start and end path.
          // Select only items within the range of the startIndex down to the
          // endIndex.
          Debug.Assert( startContext != null );
          this.AffectSelectionRecursivelyForSelectRange( startContext, startIndex, endIndex, unselect );
        }
      }
    }

    private void AffectSelectionRecursivelyForSelectRange( DataGridContext dataGridContext, int startIndex, int endIndex, bool unselect )
    {
      if( startIndex > endIndex )
        return;

      // We do not Recursively select the last item since the children
      // of theses will go beyond this last item.
      if( startIndex < endIndex )
      {
        this.AffectSelectionRecursivelyOnDetails( dataGridContext, startIndex, endIndex - 1, unselect );
      }

      //Select, not recursively the last item of the range.
      var selectionRange = new SelectionRangeWithItems( new SelectionRange( endIndex, endIndex ), null );
      this.AffectSelection( dataGridContext, selectionRange, unselect );
    }

    private bool AffectSelectionRecursivelyOnDetails( DataGridContext dataGridContext, int startIndex, int endIndex, bool unselect )
    {
      if( startIndex < 0 || endIndex < 0 )
        return false;

      //Apply the selection operation at the current level.
      var rangeWithItem = new SelectionRangeWithItems( new SelectionRange( startIndex, endIndex ), null );
      bool wasAlreadyDone = this.AffectSelection( dataGridContext, rangeWithItem, unselect );

      if( dataGridContext.DetailConfigurations.Count > 0 )
      {
        int firstIndex = Math.Min( startIndex, endIndex );
        int lastIndex = Math.Max( startIndex, endIndex );

        for( int i = firstIndex; i <= lastIndex; i++ )
        {
          // If the items are not provided, extract them from the DataGridContext.
          var item = dataGridContext.Items.GetItemAt( i );
          foreach( var configuration in dataGridContext.DetailConfigurations )
          {
            var subContext = dataGridContext.GetChildContext( item, configuration );
            if( subContext != null )
            {
              wasAlreadyDone = wasAlreadyDone
                && this.AffectSelectionRecursivelyOnDetails( subContext, 0, subContext.Items.Count - 1, unselect );
            }
          }
        }
      }

      return wasAlreadyDone;
    }

    private bool AffectSelection( DataGridContext dataGridContext, SelectionRangeWithItems selectionRange, bool unselect )
    {
      return ( unselect )
        ? this.UnselectItems( dataGridContext, selectionRange )
        : this.SelectItems( dataGridContext, selectionRange );
    }

    private bool IsItemSelected( SelectionRangePoint rangePosition )
    {
      return this.IsItemSelected( rangePosition.DataGridContext, rangePosition.ItemIndex );
    }

    private bool IsItemSelected( DataGridContext context, int itemIndex )
    {
      return context.SelectedItemsStore.Contains( itemIndex );
    }

    private bool IsCellSelected( SelectionRangePoint selectionPoint )
    {
      return this.IsCellSelected( selectionPoint.DataGridContext, selectionPoint.ItemIndex, selectionPoint.ColumnIndex );
    }

    private bool IsCellSelected( DataGridContext context, int itemIndex, int columnIndex )
    {
      return context.SelectedCellsStore.Contains( itemIndex, columnIndex );
    }

    private bool IsItemCellsSelected( SelectionRangePoint selectionPoint )
    {
      SelectedCellsStorage itemCells = this.GetItemCellStorage( selectionPoint.DataGridContext,
        selectionPoint.ItemIndex, selectionPoint.Item );

      return this.IsItemCellsSelected( selectionPoint.DataGridContext, itemCells );
    }

    private bool IsItemCellsSelected( DataGridContext dataGridContext, SelectedCellsStorage itemCellsStorage )
    {
      bool allCellSelected = true;

      foreach( SelectionCellRangeWithItems allCellSelection in itemCellsStorage )
      {
        if( !dataGridContext.SelectedCellsStore.Contains( allCellSelection ) )
        {
          allCellSelected = false;
          break;
        }
      }

      return allCellSelected;
    }

    private SelectedCellsStorage GetItemCellStorage( DataGridContext dataGridContext, int itemIndex, object item )
    {
      SelectedCellsStorage tempStorage = new SelectedCellsStorage( null );
      tempStorage.Add( this.GetItemCellRange( dataGridContext, itemIndex, item ) );

      foreach( ColumnBase column in dataGridContext.ColumnsByVisiblePosition )
      {
        if( !column.Visible )
          tempStorage.Remove( new SelectionCellRangeWithItems( itemIndex, item, column.VisiblePosition ) );
      }

      return tempStorage;
    }

    private SelectionCellRangeWithItems GetItemCellRange( DataGridContext dataGridContext, int itemIndex, object item )
    {
      SelectionRange itemRange = new SelectionRange( itemIndex );

      SelectionRange cellRange = new SelectionRange( 0,
        Math.Max( 0, dataGridContext.Columns.Count - 1 ) );

      // Select all visible cells for this itemIndex
      return new SelectionCellRangeWithItems( itemRange, new object[] { item }, cellRange );
    }

    private int CompareRangePosition( List<SelectionRangePoint> range1Indexes, List<SelectionRangePoint> range2Indexes )
    {
      for( int i = 0; i < range1Indexes.Count && i < range2Indexes.Count; i++ )
      {
        SelectionRangePoint node1 = range1Indexes[ i ];
        SelectionRangePoint node2 = range2Indexes[ i ];

        Debug.Assert( node1.DataGridContext == node2.DataGridContext );
        int node1First, node1Last;
        int node2First, node2Last;

        this.ExtractRangePositionIndexes( node1, out node1First, out node1Last );
        this.ExtractRangePositionIndexes( node2, out node2First, out node2Last );

        if( node1First < node2First )
        {
          return -1;
        }
        else if( node1First > node2First )
        {
          return 1;
        }
        else
        {
          // Both node have the same first index. They may however represent
          // different items (Ex. A the first item or subgroup of a group 
          // in relation to this parent group)
          //
          // In this case, since the group is visually represented as 
          // the "header" of it's children, consider 
          // the parent group to be "before" its children.
          // The the parent group of an item/subgroup will have a
          // higher last index than its children.
          if( node1Last != node2Last )
          {
            return ( node1Last > node2Last )
              ? -1
              : 1;
          }
        }
      }

      // Since the visual representation of the master is before the children,
      // any Master->Children comparaision will identify the master to be "before" 
      // the children.
      if( range1Indexes.Count != range2Indexes.Count )
      {
        return ( range1Indexes.Count < range2Indexes.Count ) ? -1 : 1;
      }

      // Paths are perfectly equals
      return 0;
    }

    private List<SelectionRangePoint> CreatePathForSelectionPoint( SelectionRangePoint rangePoint )
    {
      var indexList = new List<SelectionRangePoint>();

      indexList.Insert( 0, rangePoint );

      var context = rangePoint.DataGridContext;
      var parentContext = context.ParentDataGridContext;

      while( parentContext != null )
      {
        var parentIndex = parentContext.Items.IndexOf( context.ParentItem );

        var parentRangePoint = SelectionRangePoint.TryCreateRangePoint( parentContext, context.ParentItem, parentIndex, -1 );
        if( parentRangePoint != null )
        {
          indexList.Insert( 0, parentRangePoint );
        }

        context = parentContext;
        parentContext = parentContext.ParentDataGridContext;
      }

      return indexList;
    }

    private void ExtractRangePositionIndexes( SelectionRangePoint rangePosition, out int firstIndex, out int lastIndex )
    {
      firstIndex = -1;
      lastIndex = -1;

      if( rangePosition.Item != null )
      {
        Debug.Assert( rangePosition.ItemIndex != -1 );
        firstIndex = rangePosition.ItemIndex;
        lastIndex = firstIndex;
      }
      else if( rangePosition.Group != null )
      {
        var groupRange = rangePosition.Group.GetRange();
        firstIndex = groupRange.StartIndex;
        lastIndex = groupRange.EndIndex;
      }

      Debug.Assert( firstIndex <= lastIndex );
    }

    private SelectionChanger GetSelectionChanger( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return null;

      var selectionChanger = default( SelectionChanger );

      if( !m_activatedSelectionChanger.TryGetValue( dataGridContext, out selectionChanger ) )
      {
        selectionChanger = new SelectionChanger( dataGridContext );
        m_activatedSelectionChanger.Add( dataGridContext, selectionChanger );
      }

      return selectionChanger;
    }

    private void CommitSelectionChanger( SelectionChanger selectionChanger )
    {
      try
      {
        selectionChanger.UpdateSelectedItemsInChangeOfDataGridContext();
        selectionChanger.UpdateSelectedCellsInChangeOfDataGridContext();
      }
      finally
      {
        selectionChanger.Cleanup();
      }

      selectionChanger.Owner.UpdatePublicSelectionProperties();
    }

    private void UpdateCurrentToSelection()
    {
      var currentContext = m_owner.CurrentContext;
      var currentItemIndex = default( int );
      var currentColumn = default( ColumnBase );

      var selectedColumn = default( ColumnBase );
      var selectedColumnIndex = -1;
      var selectedItemIndex = -1;
      var selectedItem = default( object );

      if( ( currentContext == null ) || !m_owner.SelectedContexts.Contains( currentContext ) )
      {
        currentItemIndex = -1;
        currentColumn = null;

        if( m_owner.SelectedContexts.Count > 0 )
        {
          currentContext = m_owner.SelectedContexts[ 0 ];
          currentContext.GetFirstSelectedItemFromStore( true, out selectedItemIndex, out selectedItem, out selectedColumnIndex );
        }
      }
      else
      {
        currentItemIndex = currentContext.CurrentItemIndex;
        currentColumn = currentContext.CurrentColumn;
        currentContext.GetFirstSelectedItemFromStore( true, out selectedItemIndex, out selectedItem, out selectedColumnIndex );
      }

      if( currentContext == null )
        return;

      if( m_owner.SelectionUnit == SelectionUnit.Cell )
      {
        if( ( currentItemIndex != -1 ) && ( currentColumn != null )
          && currentContext.SelectedCellsStore.GetIntersectedColumnRanges( new SelectionCellRange( currentItemIndex, currentColumn.VisiblePosition ) ).Any() )
          return;

        selectedColumn = currentContext.ColumnsByVisiblePosition.ElementAtOrDefault( selectedColumnIndex );
      }
      else
      {
        if( ( currentItemIndex != -1 ) && currentContext.SelectedItemsStore.Contains( currentItemIndex ) )
          return;

        selectedColumn = currentContext.CurrentColumn;
      }

      // No need to change Current when the previous and subsequent current are both null, even if SelectionUnit is set to Cell and columns do not correspond,
      // since there is no current anyway.  It will be correctly updated next time an item becomes current.
      if( selectedItem == currentContext.CurrentItem )
        return;

      currentContext.SetCurrent( selectedItem, null, selectedItemIndex, selectedColumn, false, true, false, AutoScrollCurrentItemSourceTriggers.SelectionChanged );
    }

    private bool m_isActive;
    private readonly Dictionary<DataGridContext, SelectionChanger> m_activatedSelectionChanger = new Dictionary<DataGridContext, SelectionChanger>();
    private readonly DataGridControl m_owner;
    private SelectionRangePoint m_selectionRangeStartPoint;
    private bool m_updateSelectionOnNextMouseUp = true;
    private bool m_fromRowSelector; // = false;

    // = Navigation ?
    private UpdateSelectionSource m_currentUpdateSelectionSource;

    private class UpdateSelectionSourceHelper : IDisposable
    {
      public UpdateSelectionSourceHelper(
        SelectionManager selectionManager,
        UpdateSelectionSource newUpdateSelectionSource )
      {
        m_selectionManager = selectionManager;
        m_oldUpdateSelectionSource = selectionManager.m_currentUpdateSelectionSource;
        selectionManager.m_currentUpdateSelectionSource = newUpdateSelectionSource;
      }

      private UpdateSelectionSource m_oldUpdateSelectionSource;
      private SelectionManager m_selectionManager;

      #region IDisposable Members

      public void Dispose()
      {
        m_selectionManager.m_currentUpdateSelectionSource = m_oldUpdateSelectionSource;
      }

      #endregion IDisposable Members
    }
  }
}
