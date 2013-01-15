/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

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
      None = 99
    }

    #region CONSTRUCTORS

    public SelectionManager( DataGridControl owner )
    {
      if( owner == null )
        throw new ArgumentNullException( "owner" );

      m_owner = owner;
    }

    #endregion CONSTRUCTORS

    #region PUBLIC PROPERTIES

    public bool IsActive
    {
      get
      {
        return m_isActive;
      }
    }

    #endregion PUBLIC PROPERTIES

    #region PUBLIC METHODS

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

    public void End( bool itemsSourceChanged, bool allowCancelingOfSelection, bool allowSynchronizeSelectionWithCurrent )
    {
      Debug.Assert( m_isActive );

      List<SelectionInfo> activatedSelectionDeltas = new List<SelectionInfo>();
      SelectionInfo selectionInfo;

      try
      {
        foreach( SelectionChanger selectionChanger in m_activatedSelectionChanger.Values )
        {
          selectionInfo = selectionChanger.GetSelectionInfo();

          if( !selectionInfo.IsEmpty )
          {
            activatedSelectionDeltas.Add( selectionInfo );
          }
        }

        if( activatedSelectionDeltas.Count > 0 )
        {
          DataGridSelectionChangingEventArgs eventArgs = new DataGridSelectionChangingEventArgs(
            new ReadOnlyCollection<SelectionInfo>( activatedSelectionDeltas ), allowCancelingOfSelection );

          m_owner.RaiseSelectionChanging( eventArgs );

          if( ( eventArgs.Cancel ) && ( allowCancelingOfSelection ) )
          {
            this.Cancel();
            throw new DataGridException( "Selection modification canceled." );
          }
        }

        foreach( SelectionChanger selectionChanger in m_activatedSelectionChanger.Values )
        {
          this.CommitSelectionChanger( selectionChanger, itemsSourceChanged );
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

        m_owner.RaiseSelectionChanged(
          new DataGridSelectionChangedEventArgs(
            new ReadOnlyCollection<SelectionInfo>( activatedSelectionDeltas ) ) );

        m_owner.NotifyGlobalSelectedItemsChanged();
      }
    }

    public bool ToggleItemSelection( DataGridContext context, int itemIndex, object item )
    {
      if( context.SelectedItemsStore.Contains( itemIndex ) )
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
      if( context.SelectedCellsStore.Contains( itemIndex, columnIndex ) )
      {
        return this.UnselectCells( context, new SelectionCellRangeWithItems( itemIndex, item, columnIndex ) );
      }
      else
      {
        return this.SelectCells( context, new SelectionCellRangeWithItems( itemIndex, item, columnIndex ) );
      }
    }

    public bool ToggleItemCellsSelection(
      DataGridContext dataGridContext,
      int itemIndex,
      object item )
    {
      SelectionRange itemRange = new SelectionRange( itemIndex );

      SelectionRange cellRange = new SelectionRange( 0,
        Math.Max( 0, dataGridContext.Columns.Count - 1 ) );

      // Select all visible cells for this itemIndex
      SelectionCellRangeWithItems selection = new SelectionCellRangeWithItems( itemRange,
        new object[] { item },
        cellRange );

      SelectedCellsStorage tempStorage = new SelectedCellsStorage( null, 1 );
      tempStorage.Add( selection );

      foreach( ColumnBase column in dataGridContext.ColumnsByVisiblePosition )
      {
        if( !column.Visible )
          tempStorage.Remove( new SelectionCellRangeWithItems( itemIndex, item, column.VisiblePosition ) );
      }

      int tempStorageCount = tempStorage.Count;
      bool allCellSelected = true;

      foreach( SelectionCellRangeWithItems allCellSelection in tempStorage )
      {
        if( !dataGridContext.SelectedCellsStore.Contains( allCellSelection ) )
        {
          allCellSelected = false;
          break;
        }
      }

      bool selectionDone = true;

      if( allCellSelected )
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

    public bool SelectItemCells(
      DataGridContext dataGridContext,
      int itemIndex,
      object item,
      bool preserveSelection )
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
            if( ( e.NewItems.Count == 1 ) && ( e.OldItems.Count == 1 ) && ( e.OldItems[ 0 ] == e.NewItems[ 0 ] ) )
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
              this.End( true, false, true );
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

              m_rangeSelectionItemStartAnchor = -1;
              m_rangeSelectionColumnStartAnchor = -1;
              SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
              selectionChanger.UpdateSelectionAfterSourceDataItemAdded( e );
            }
            finally
            {
              this.End( true, false, true );
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

              m_rangeSelectionItemStartAnchor = -1;
              m_rangeSelectionColumnStartAnchor = -1;
              SelectionChanger selectionChanger = this.GetSelectionChanger( dataGridContext );
              selectionChanger.UpdateSelectionAfterSourceDataItemRemoved( e );
            }
            finally
            {
              this.End( true, false, true );
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

              m_rangeSelectionItemStartAnchor = -1;
              m_rangeSelectionColumnStartAnchor = -1;

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
                      int index = sourceItems.IndexOf( item );

                      if( index != -1 )
                      {
                        selectionChanger.SelectItems( new SelectionRangeWithItems( index, item ) );
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
              this.End( true, false, true );
            }

            break;
          }
      }
    }

    public void UpdateSelection(
      DataGridContext oldCurrentDataGridContext,
      object oldCurrentItem,
      ColumnBase oldCurrentColumn,
      DataGridContext dataGridContext,
      bool oldFocusWasInTheGrid,
      bool rowIsBeingEditedAndCurrentRowNotChanged,
      int sourceDataItemIndex,
      object item,
      int columnIndex,
      Nullable<UpdateSelectionSource> updateSelectionSourceParam )
    {
      int oldCurrentColumnIndex = ( oldCurrentColumn == null ) ? -1 : oldCurrentColumn.VisiblePosition;

      if( item == null )
        return;

      UpdateSelectionSource updateSelectionSource = ( updateSelectionSourceParam.HasValue )
        ? updateSelectionSourceParam.Value : m_currentUpdateSelectionSource;

      // Force the node tree to be created since detail can be deleted during that node creation and selection uptade will be call
      // That way we prevent selection code reentrance.
      this.EnsureNodeTreeCreatedOnAllSubContext( m_owner.DataGridContext );

      if( ( updateSelectionSource != UpdateSelectionSource.RowSelector )
          && ( updateSelectionSource != UpdateSelectionSource.MouseUp ) )
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
                dataGridContext, oldFocusWasInTheGrid, rowIsBeingEditedAndCurrentRowNotChanged,
                sourceDataItemIndex, item, columnIndex, updateSelectionSource );

              break;
            }

          case SelectionMode.Multiple:
            {
              this.DoMultipleSelection(
                dataGridContext, rowIsBeingEditedAndCurrentRowNotChanged,
                sourceDataItemIndex, item, columnIndex, updateSelectionSource );

              break;
            }

          case SelectionMode.Extended:
            {
              this.DoExtendedSelection(
                oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                dataGridContext, oldFocusWasInTheGrid, rowIsBeingEditedAndCurrentRowNotChanged,
                sourceDataItemIndex, item, columnIndex, updateSelectionSource );

              break;
            }
        }
      }
      finally
      {
        try
        {
          this.End( false, true, false );
        }
        catch( DataGridException )
        {
          // This is to swallow when selection is aborted
        }
      }
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
      DataGridContext dataGridContext,
      bool oldFocusWasInTheGrid,
      bool rowIsBeingEditedAndCurrentRowNotChanged,
      int sourceDataItemIndex,
      object item,
      int columnIndex,
      UpdateSelectionSource updateSelectionSource )
    {
      bool doSelection = false;

      if( updateSelectionSource == UpdateSelectionSource.Navigation )
      {
        if( oldFocusWasInTheGrid )
        {
          if( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.None )
          {
            doSelection = true;
          }
          else //this means that CTRL was pressed
          {
            //if PageUp or PageDown or Home or End are pressed, it means its page navigation and in this case, I want to do SingleSelection
            if( Keyboard.IsKeyDown( Key.PageUp )
              || Keyboard.IsKeyDown( Key.PageDown )
              || Keyboard.IsKeyDown( Key.End )
              || Keyboard.IsKeyDown( Key.Home ) )
            {
              doSelection = true;
            }
          }
        }
      }
      else
      {
        doSelection = true;
      }

      // Special case for RowSelector : We do not want the
      // MouseUp to clear the Selection when it was just set
      // by the RowSelector
      if( updateSelectionSource == UpdateSelectionSource.RowSelector )
      {
        m_fromRowSelector = true;
      }

      if( !doSelection )
        return;

      if( m_owner.SelectionUnit == SelectionUnit.Row )
      {
        if( sourceDataItemIndex != -1 )
        {
          this.SelectJustThisItem( dataGridContext, sourceDataItemIndex, item );
        }
        else
        {
          this.UnselectAll();
        }
      }
      else
      {
        if( ( sourceDataItemIndex != -1 ) && ( columnIndex != -1 ) )
        {
          this.SelectJustThisCell( dataGridContext, sourceDataItemIndex, item, columnIndex );
        }
        else
        {
          // Do not remove selection when it comes from the RowSelector
          if( !m_fromRowSelector )
          {
            this.UnselectAll();
          }
        }
      }
    }

    private void DoMultipleSelection(
      DataGridContext dataGridContext,
      bool rowIsBeingEditedAndCurrentRowNotChanged,
      int sourceDataItemIndex,
      object item,
      int columnIndex,
      UpdateSelectionSource updateSelectionSource )
    {
      switch( updateSelectionSource )
      {
        case UpdateSelectionSource.MouseDown:
          {
            if( m_owner.SelectionUnit == SelectionUnit.Row )
            {
              if( sourceDataItemIndex != -1 )
              {
                // this.Select return false when the item is already selected.
                m_updateSelectionOnNextMouseUp = !this.SelectItems(
                  dataGridContext, new SelectionRangeWithItems( sourceDataItemIndex, item ) );
              }
            }
            else
            {
              if( ( sourceDataItemIndex != -1 ) && ( columnIndex != -1 ) )
              {
                // this.Select return false when the cell is already selected.
                m_updateSelectionOnNextMouseUp = !this.SelectCells(
                  dataGridContext, new SelectionCellRangeWithItems( sourceDataItemIndex, item, columnIndex ) );
              }
            }

            break;
          }

        case UpdateSelectionSource.MouseUp:
          {
            if( ( m_updateSelectionOnNextMouseUp ) && ( !rowIsBeingEditedAndCurrentRowNotChanged ) )
            {
              if( m_owner.SelectionUnit == SelectionUnit.Row )
              {
                if( sourceDataItemIndex != -1 )
                {
                  this.ToggleItemSelection( dataGridContext, sourceDataItemIndex, item );
                }
              }
              else
              {
                if( sourceDataItemIndex != -1 )
                {
                  if( m_fromRowSelector )
                  {
                    this.ToggleItemCellsSelection( dataGridContext,
                      sourceDataItemIndex,
                      item );
                  }
                  else if( columnIndex != -1 )
                  {
                    this.ToggleCellSelection( dataGridContext,
                      sourceDataItemIndex,
                      item,
                      columnIndex );
                  }
                }
              }
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
            if( m_owner.SelectionUnit == SelectionUnit.Row )
            {
              if( sourceDataItemIndex != -1 )
              {
                this.ToggleItemSelection( dataGridContext, sourceDataItemIndex, item );
              }
            }
            else
            {
              if( ( sourceDataItemIndex != -1 ) && ( columnIndex != -1 ) )
              {
                this.ToggleCellSelection( dataGridContext,
                  sourceDataItemIndex,
                  item,
                  columnIndex );
              }
            }

            break;
          }
        case UpdateSelectionSource.RowSelector:
          {
            m_fromRowSelector = true;

            // We must raise the same flags as in the MouseDown to ensure
            // the MouseUp is correctly handled
            if( m_owner.SelectionUnit == SelectionUnit.Row )
            {
              if( sourceDataItemIndex != -1 )
              {
                // this.Select return false when the item is already selected.
                m_updateSelectionOnNextMouseUp = !this.SelectItems(
                  dataGridContext, new SelectionRangeWithItems( sourceDataItemIndex, item ) );
              }
            }
            else
            {
              if( ( sourceDataItemIndex != -1 ) && ( columnIndex != -1 ) )
              {
                // this.Select return false when the cell is already selected.
                m_updateSelectionOnNextMouseUp = !this.SelectItemCells( dataGridContext,
                  sourceDataItemIndex,
                  item,
                  true );
              }
            }

            break;
          }
      }
    }

    private void DoExtendedSelection(
      DataGridContext oldCurrentDataGridContext,
      object oldCurrentItem,
      int oldCurrentColumnIndex,
      DataGridContext dataGridContext,
      bool oldFocusWasInTheGrid,
      bool rowIsBeingEditedAndCurrentRowNotChanged,
      int sourceDataItemIndex,
      object item,
      int columnIndex,
      UpdateSelectionSource updateSelectionSource )
    {
      switch( updateSelectionSource )
      {
        case UpdateSelectionSource.Navigation:
          {
            if( oldFocusWasInTheGrid )
            {
              if( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
              {
                if( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
                {
                  this.DoRangeSelection(
                    oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                    dataGridContext, sourceDataItemIndex, item, columnIndex, true );
                }
                else
                {
                  m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
                  m_rangeSelectionColumnStartAnchor = columnIndex;

                  if( Keyboard.IsKeyDown( Key.PageUp )
                    || Keyboard.IsKeyDown( Key.PageDown )
                    || Keyboard.IsKeyDown( Key.End )
                    || Keyboard.IsKeyDown( Key.Home ) )
                  {
                    if( m_owner.SelectionUnit == SelectionUnit.Row )
                    {
                      if( sourceDataItemIndex == -1 )
                      {
                        this.UnselectAll();
                      }
                      else
                      {
                        this.SelectJustThisItem( dataGridContext, sourceDataItemIndex, item );
                      }
                    }
                    else
                    {
                      if( ( sourceDataItemIndex == -1 ) || ( columnIndex == -1 ) )
                      {
                        this.UnselectAll();
                      }
                      else
                      {
                        this.SelectJustThisCell( dataGridContext, sourceDataItemIndex, item, columnIndex );
                      }
                    }
                  }
                }
              }
              else if( ( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift ) && ( !Keyboard.IsKeyDown( Key.Tab ) ) )
              {
                this.DoRangeSelection(
                  oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                  dataGridContext, sourceDataItemIndex, item, columnIndex, false );
              }
              else
              {
                m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
                m_rangeSelectionColumnStartAnchor = columnIndex;

                if( m_owner.SelectionUnit == SelectionUnit.Row )
                {
                  if( sourceDataItemIndex == -1 )
                  {
                    this.UnselectAll();
                  }
                  else
                  {
                    if( ( !dataGridContext.SelectedItemsStore.Contains( sourceDataItemIndex ) ) || ( !rowIsBeingEditedAndCurrentRowNotChanged ) )
                    {
                      this.SelectJustThisItem( dataGridContext, sourceDataItemIndex, item );
                    }
                  }
                }
                else
                {
                  if( ( sourceDataItemIndex == -1 ) || ( columnIndex == -1 ) )
                  {
                    this.UnselectAll();
                  }
                  else
                  {
                    if( ( !dataGridContext.SelectedCellsStore.Contains( sourceDataItemIndex, columnIndex ) ) || ( !rowIsBeingEditedAndCurrentRowNotChanged ) )
                    {
                      this.SelectJustThisCell( dataGridContext, sourceDataItemIndex, item, columnIndex );
                    }
                  }
                }
              }
            }

            break;
          }

        case UpdateSelectionSource.MouseDown:
          {
            if( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
            {
              if( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
              {
                this.DoRangeSelection(
                  oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                  dataGridContext, sourceDataItemIndex, item, columnIndex, true );

                m_updateSelectionOnNextMouseUp = false;
              }
              else
              {
                m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
                m_rangeSelectionColumnStartAnchor = columnIndex;

                if( m_owner.SelectionUnit == SelectionUnit.Row )
                {
                  if( sourceDataItemIndex != -1 )
                  {
                    // this.Select return false when the item is already selected.
                    m_updateSelectionOnNextMouseUp = !this.SelectItems(
                      dataGridContext, new SelectionRangeWithItems( sourceDataItemIndex, item ) );

                    m_rowIsBeingEditedAndCurrentRowNotChanged = false;
                  }
                }
                else
                {
                  if( ( sourceDataItemIndex != -1 ) && ( columnIndex != -1 ) )
                  {
                    // this.Select return false when the item is already selected.
                    m_updateSelectionOnNextMouseUp = !this.SelectCells(
                      dataGridContext, new SelectionCellRangeWithItems( sourceDataItemIndex, item, columnIndex ) );
                    m_rowIsBeingEditedAndCurrentRowNotChanged = rowIsBeingEditedAndCurrentRowNotChanged;
                  }
                }
              }
            }
            else if( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
            {
              this.DoRangeSelection(
                oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                dataGridContext, sourceDataItemIndex, item, columnIndex, false );

              m_updateSelectionOnNextMouseUp = false;
            }
            else
            {
              m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
              m_rangeSelectionColumnStartAnchor = columnIndex;

              if( m_owner.SelectionUnit == SelectionUnit.Row )
              {
                if( sourceDataItemIndex == -1 )
                {
                  this.UnselectAll();
                }
                else
                {
                  if( !dataGridContext.SelectedItemsStore.Contains( sourceDataItemIndex ) )
                  {
                    this.SelectJustThisItem( dataGridContext, sourceDataItemIndex, item );
                    m_updateSelectionOnNextMouseUp = false;
                  }
                  else
                  {
                    m_updateSelectionOnNextMouseUp = true;
                    m_rowIsBeingEditedAndCurrentRowNotChanged = rowIsBeingEditedAndCurrentRowNotChanged;
                  }
                }
              }
              else
              {
                if( ( sourceDataItemIndex == -1 ) || ( columnIndex == -1 ) )
                {
                  this.UnselectAll();
                }
                else
                {
                  if( !dataGridContext.SelectedCellsStore.Contains( sourceDataItemIndex, columnIndex ) )
                  {
                    this.SelectJustThisCell( dataGridContext, sourceDataItemIndex, item, columnIndex );
                    m_updateSelectionOnNextMouseUp = false;
                  }
                  else
                  {
                    m_updateSelectionOnNextMouseUp = true;
                    m_rowIsBeingEditedAndCurrentRowNotChanged = rowIsBeingEditedAndCurrentRowNotChanged;
                  }
                }
              }
            }

            break;
          }

        case UpdateSelectionSource.MouseUp:
          {
            if( ( m_updateSelectionOnNextMouseUp ) && ( !m_rowIsBeingEditedAndCurrentRowNotChanged ) )
            {
              if( m_owner.SelectionUnit == SelectionUnit.Row )
              {
                if( sourceDataItemIndex != -1 )
                {
                  if( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
                  {
                    bool keepPreviousSelection = ( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control );

                    this.DoRangeSelection(
                      oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                      dataGridContext, sourceDataItemIndex, item, columnIndex, keepPreviousSelection );
                  }
                  else
                  {
                    m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
                    m_rangeSelectionColumnStartAnchor = columnIndex;

                    if( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
                    {
                      this.ToggleItemSelection( dataGridContext, sourceDataItemIndex, item );
                    }
                    else if( !rowIsBeingEditedAndCurrentRowNotChanged )
                    {
                      this.SelectJustThisItem( dataGridContext, sourceDataItemIndex, item );
                    }
                  }
                }
              }
              else
              {
                if( sourceDataItemIndex != -1 )
                {
                  if( m_fromRowSelector )
                  {
                    if( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
                    {
                      bool keepPreviousSelection = ( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control );

                      this.DoRangeSelection(
                        oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                        dataGridContext, sourceDataItemIndex, item, columnIndex, keepPreviousSelection, true );
                    }
                    else
                    {
                      m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
                      m_rangeSelectionColumnStartAnchor = columnIndex;

                      if( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
                      {
                        this.ToggleItemCellsSelection( dataGridContext,
                          sourceDataItemIndex,
                          item );
                      }
                      else
                      {
                        this.SelectItemCells( dataGridContext,
                          sourceDataItemIndex,
                          item,
                          false );
                      }
                    }
                  }
                  else if( columnIndex != -1 )
                  {
                    if( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
                    {
                      bool keepPreviousSelection = ( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control );

                      this.DoRangeSelection(
                        oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                        dataGridContext, sourceDataItemIndex, item, columnIndex, keepPreviousSelection );
                    }
                    else
                    {
                      m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
                      m_rangeSelectionColumnStartAnchor = columnIndex;

                      if( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
                      {
                        this.ToggleCellSelection( dataGridContext,
                          sourceDataItemIndex,
                          item,
                          columnIndex );
                      }
                      else
                      {
                        this.SelectJustThisCell( dataGridContext,
                          sourceDataItemIndex,
                          item,
                          columnIndex );
                      }
                    }
                  }
                }
              }
            }

            // Reset the fromRowSelector to ensure nothing special is done
            // on next MouseUp (failsafe, should be reset by UpdateSelection)
            m_fromRowSelector = false;

            // We reset the m_updateSelectionOnNextMouseUp to be sure when no focus is moved
            // around that mouse up will toggle selection
            m_updateSelectionOnNextMouseUp = true;

            m_rowIsBeingEditedAndCurrentRowNotChanged = false;

            break;
          }

        case UpdateSelectionSource.SpaceDown:
          {
            if( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
            {
              m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
              m_rangeSelectionColumnStartAnchor = columnIndex;

              if( m_owner.SelectionUnit == SelectionUnit.Row )
              {
                if( sourceDataItemIndex != -1 )
                {
                  this.ToggleItemSelection( dataGridContext, sourceDataItemIndex, item );
                }
              }
              else
              {
                if( ( sourceDataItemIndex != -1 ) && ( columnIndex != -1 ) )
                {
                  this.ToggleCellSelection( dataGridContext, sourceDataItemIndex, item, columnIndex );
                }
              }
            }

            break;
          }

        case UpdateSelectionSource.RowSelector:
          {
            // We must raise the same flags as in the MouseDown to ensure
            // the MouseUp is correctly handled
            if( ( Keyboard.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control )
            {
              if( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
              {
                this.DoRangeSelection(
                  oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                  dataGridContext, sourceDataItemIndex, item, columnIndex, true, true );

                m_updateSelectionOnNextMouseUp = false;
              }
              else
              {
                m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
                m_rangeSelectionColumnStartAnchor = 0;

                if( m_owner.SelectionUnit == SelectionUnit.Row )
                {
                  if( sourceDataItemIndex != -1 )
                  {
                    // this.Select return false when the item is already selected.
                    m_updateSelectionOnNextMouseUp = !this.SelectItems(
                      dataGridContext, new SelectionRangeWithItems( sourceDataItemIndex, item ) );
                  }
                }
                else
                {
                  if( sourceDataItemIndex != -1 )
                  {
                    // this.Select return false when the item is already selected.
                    m_updateSelectionOnNextMouseUp = !this.SelectItemCells( dataGridContext,
                      sourceDataItemIndex,
                      item,
                      true );
                  }
                }
              }
            }
            else if( ( Keyboard.Modifiers & ModifierKeys.Shift ) == ModifierKeys.Shift )
            {
              this.DoRangeSelection(
                oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
                dataGridContext, sourceDataItemIndex, item, columnIndex, false, true );

              m_updateSelectionOnNextMouseUp = false;
            }
            else
            {
              m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
              m_rangeSelectionColumnStartAnchor = 0;

              if( m_owner.SelectionUnit == SelectionUnit.Row )
              {
                if( sourceDataItemIndex == -1 )
                {
                  this.UnselectAll();
                }
                else
                {
                  if( !dataGridContext.SelectedItemsStore.Contains( sourceDataItemIndex ) )
                  {
                    this.SelectJustThisItem( dataGridContext,
                      sourceDataItemIndex,
                      item );

                    m_updateSelectionOnNextMouseUp = false;
                  }
                  else
                  {
                    m_updateSelectionOnNextMouseUp = true;
                  }
                }
              }
              else
              {
                if( ( sourceDataItemIndex == -1 ) || ( columnIndex == -1 ) )
                {
                  this.UnselectAll();
                }
                else
                {
                  m_updateSelectionOnNextMouseUp =
                    !this.SelectItemCells( dataGridContext, sourceDataItemIndex, item, false );
                }
              }
            }
            break;
          }
      }
    }

    private void DoRangeSelection(
      DataGridContext oldCurrentDataGridContext,
      object oldCurrentItem,
      int oldCurrentColumnIndex,
      DataGridContext dataGridContext,
      int sourceDataItemIndex,
      object item,
      int columnIndex,
      bool keepPreviousSelection )
    {
      this.DoRangeSelection(
        oldCurrentDataGridContext, oldCurrentItem, oldCurrentColumnIndex,
        dataGridContext, sourceDataItemIndex, item, columnIndex,
        keepPreviousSelection, false );
    }

    private void DoRangeSelection(
      DataGridContext oldCurrentDataGridContext,
      object oldCurrentItem,
      int oldCurrentColumnIndex,
      DataGridContext dataGridContext,
      int sourceDataItemIndex,
      object item,
      int columnIndex,
      bool keepPreviousSelection,
      bool fromRowSelector )
    {
      if( item == null )
        return;

      IDataGridContextVisitable visitable = ( IDataGridContextVisitable )m_owner.DataGridContext;

      if( m_rangeSelectionItemStartAnchor == -1 )
      {
        if( ( oldCurrentDataGridContext != null ) && ( oldCurrentItem != null ) )
        {
          m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( oldCurrentDataGridContext, oldCurrentItem );
          m_rangeSelectionColumnStartAnchor = oldCurrentColumnIndex;
        }
        else
        {
          m_rangeSelectionItemStartAnchor = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );
          m_rangeSelectionColumnStartAnchor = columnIndex;
        }
      }

      if( !keepPreviousSelection )
      {
        this.UnselectAll();
      }

      int starting_index = m_rangeSelectionItemStartAnchor;
      int ending_index = m_owner.GetGlobalGeneratorIndexFromItem( dataGridContext, item );

      int min = Math.Min( starting_index, ending_index );
      int max = Math.Max( starting_index, ending_index );

      // here I need to normalize the values to ensure that I'm not catching the Fixed Headers or Fixed Footers
      if( ( min != -1 ) && ( max != -1 ) )
      {
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
            m_rangeSelectionColumnStartAnchor = 0;
            fullColumnRange = new SelectionRange( 0, Math.Max( 0, dataGridContext.ColumnsByVisiblePosition.Count - 1 ) );
          }
          else
          {
            if( m_rangeSelectionColumnStartAnchor == -1 )
              m_rangeSelectionColumnStartAnchor = 0;

            if( columnIndex == -1 )
              return;

            fullColumnRange = new SelectionRange( m_rangeSelectionColumnStartAnchor, columnIndex );
          }

          SelectedItemsStorage selectedColumnStore = new SelectedItemsStorage( null, 8 );
          selectedColumnStore.Add( new SelectionRangeWithItems( fullColumnRange, null ) );
          int index = 0;

          foreach( ColumnBase column in dataGridContext.ColumnsByVisiblePosition )
          {
            if( !column.Visible )
            {
              selectedColumnStore.Remove( new SelectionRangeWithItems( new SelectionRange( index ), null ) );
            }

            index++;
          }

          selectedColumns = selectedColumnStore.ToSelectionRangeArray();
        }

        bool visitWasStopped;

        visitable.AcceptVisitor(
          min, max,
          new RangeSelectionVisitor( selectedColumns ),
          DataGridContextVisitorType.ItemsBlock, out visitWasStopped );
      }
    }

    #endregion PUBLIC METHODS

    #region PRIVATE METHODS

    private SelectionChanger GetSelectionChanger( DataGridContext dataGridContext )
    {
      SelectionChanger selectionChanger = m_activatedSelectionChanger[ dataGridContext ] as SelectionChanger;

      if( selectionChanger != null )
        return selectionChanger;

      selectionChanger = new SelectionChanger( dataGridContext );
      m_activatedSelectionChanger[ dataGridContext ] = selectionChanger;
      return selectionChanger;
    }

    private void CommitSelectionChanger(
      SelectionChanger selectionChanger,
      bool itemsSourceChanged )
    {
      try
      {
        selectionChanger.UpdateSelectedItemsInChangeOfDataGridContext( itemsSourceChanged );
        selectionChanger.UpdateSelectedCellsInChangeOfDataGridContext( itemsSourceChanged );
      }
      finally
      {
        selectionChanger.Cleanup();
      }

      selectionChanger.Owner.UpdatePublicSelectionProperties();
    }

    private void UpdateCurrentToSelection()
    {
      DataGridContext currentContext = m_owner.CurrentContext;
      object currentItem;
      int currentItemIndex;
      ColumnBase currentColumn;

      ColumnBase selectedColumn = null;
      int selectedColumnIndex = -1;
      int selectedItemIndex = -1;
      object selectedItem = null;

      if( ( currentContext == null ) || ( !m_owner.SelectedContexts.Contains( currentContext ) ) )
      {
        currentItem = null;
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
        currentItem = currentContext.CurrentItem;
        currentItemIndex = currentContext.CurrentItemIndex;
        currentColumn = currentContext.CurrentColumn;
        currentContext.GetFirstSelectedItemFromStore( true, out selectedItemIndex, out selectedItem, out selectedColumnIndex );
      }

      if( currentContext == null )
        return;

      if( m_owner.SelectionUnit == SelectionUnit.Cell )
      {
        if( ( currentItemIndex != -1 ) && ( currentColumn != null )
          && currentContext.SelectedCellsStore.GetIntersectedColumnRanges(
            new SelectionCellRange( currentItemIndex, currentColumn.VisiblePosition ) ).Count > 0 )
        {
          return;
        }

        selectedColumn = currentContext.ColumnsByVisiblePosition.ElementAtOrDefault( selectedColumnIndex );
      }
      else
      {
        if( ( currentItemIndex != -1 ) && ( currentContext.SelectedItemsStore.Contains( currentItemIndex ) ) )
          return;

        selectedColumn = currentContext.CurrentColumn;
      }

      currentContext.SetCurrent( selectedItem, null, selectedItemIndex, selectedColumn, false, true, false );
    }

    #endregion PRIVATE METHODS

    #region FIELDS

    private bool m_isActive;
    private Hashtable m_activatedSelectionChanger = new Hashtable();
    private DataGridControl m_owner;

    private int m_rangeSelectionItemStartAnchor = -1;
    private int m_rangeSelectionColumnStartAnchor = -1;

    private bool m_updateSelectionOnNextMouseUp = true;
    private bool m_rowIsBeingEditedAndCurrentRowNotChanged; // = false;
    private bool m_fromRowSelector; // = false;
    private UpdateSelectionSource m_currentUpdateSelectionSource;

    #endregion FIELDS

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
