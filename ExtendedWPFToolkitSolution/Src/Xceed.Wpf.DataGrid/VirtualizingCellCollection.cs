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
using System.Diagnostics;
using System.Windows;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal class VirtualizingCellCollection : CellCollection
  {
    internal VirtualizingCellCollection( Row parentRow )
      : base( new VirtualizingItemsList() )
    {
      if( parentRow == null )
        throw new ArgumentNullException( "parentRow" );

      m_parentRow = parentRow;
      m_cells = new CellSet( parentRow );
      m_readOnlyCellsCollection = new ReadOnlyCollection<Cell>( m_cells );

      // We must assign the VirtualizingCellCollection of the VirtualizingItemsList manually
      ( ( VirtualizingItemsList )this.Items ).SetParent( this );
    }

    #region Cells Property

    internal ICollection<Cell> Cells
    {
      get
      {
        return m_readOnlyCellsCollection;
      }
    }

    private readonly ICollection<Cell> m_readOnlyCellsCollection;

    #endregion

    #region BindedCells Property

    internal ICollection<Cell> BindedCells
    {
      get
      {
        return m_bindedCells.Values;
      }
    }

    #endregion

    #region VirtualizedCells Property

    internal ICollection<Cell> VirtualizedCells
    {
      get
      {
        return m_unbindedCells.Values;
      }
    }

    #endregion

    #region DataGridContext Private Property

    private DataGridContext DataGridContext
    {
      get
      {
        return DataGridControl.GetDataGridContext( m_parentRow );
      }
    }

    #endregion

    #region Columns Private Property

    private ColumnCollection Columns
    {
      get
      {
        if( this.DataGridContext == null )
          return null;

        return this.DataGridContext.Columns;
      }
    }

    #endregion

    #region IsUpdating Property

    internal bool IsUpdating
    {
      get
      {
        return m_isUpdating.IsSet;
      }
    }

    internal IDisposable SetIsUpdating()
    {
      return m_isUpdating.Set();
    }

    private readonly AutoResetFlag m_isUpdating = AutoResetFlagFactory.Create();

    #endregion

    #region VirtualizationMode property

    internal ColumnVirtualizationMode VirtualizationMode
    {
      get;
      set;
    }

    #endregion

    #region CollectionChanged Event

    internal event EventHandler CollectionChanged;

    private void NotifyCollectionChanged()
    {
      var handler = this.CollectionChanged;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    #endregion

    public override Cell this[ ColumnBase column ]
    {
      get
      {
        if( column == null )
          return null;

        var columns = this.Columns;
        if( columns == null )
          return null;

        var match = columns[ column.FieldName ];
        if( match == null )
          return null;

        Debug.Assert( match == column );

        return this.GetCell( column, true );
      }
    }

    public override Cell this[ string fieldName ]
    {
      get
      {
        return this.GetCell( this.GetColumn( fieldName ), true );
      }
    }

    protected override void InsertItem( int index, Cell item )
    {
      this.InternalInsert( index, item );
    }

    protected override void RemoveItem( int index )
    {
      this.InternalRemoveAt( index );
    }

    protected override void SetItem( int index, Cell item )
    {
      throw new NotSupportedException();
    }

    protected override void ClearItems()
    {
      this.InternalClear();
    }

    internal override void InternalAdd( Cell cell )
    {
      this.InternalAddCore( cell );
    }

    internal override void InternalClear()
    {
      //Try to be GC friendly
      if( m_recyclingBins != null )
      {
        foreach( var recycleBin in m_recyclingBins.Values )
        {
          recycleBin.Clear();
        }
        m_recyclingBins.Clear();
      }
      m_recyclingBins = null;

      if( m_freeCells != null )
      {
        m_freeCells.Clear();
        m_freeCells.TrimExcess();
      }
      m_freeCells = null;

      m_bindedCells.Clear();
      m_unbindedCells.Clear();
      m_unprocessedCells.Clear();
      m_cells.Clear();
    }

    internal override void InternalInsert( int index, Cell cell )
    {
      this.InternalAddCore( cell );
    }

    internal override void InternalRemove( Cell cell )
    {
      this.InternalRemoveCore( cell, true );
    }

    internal override void InternalRemoveAt( int index )
    {
      ColumnBase column = this.Columns[ index ];
      if( column == null )
        return;

      Cell cell;
      if( !m_bindedCells.TryGetValue( column.FieldName, out cell ) )
        return;

      this.InternalRemoveCore( cell, true );
    }

    internal override void InternalSetCell( int index, Cell cell )
    {
      throw new NotSupportedException();
    }

    internal void Release( Cell cell )
    {
      if( cell == null )
        return;

      // Remove the target cell from the binded cells.
      this.ReleaseCore( cell );

      cell.Visibility = Visibility.Collapsed;
    }

    internal Cell GetCell( string fieldName, bool prepareCell )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        return null;

      Cell cell;
      if( this.TryGetBindedCell( fieldName, prepareCell, out cell ) )
        return cell;

      return this.CreateOrRecoverCell( fieldName );
    }

    internal Cell GetCell( ColumnBase column, bool prepareCell )
    {
      if( column == null )
        return null;

      Cell cell;
      if( this.TryGetBindedCell( column, prepareCell, out cell ) )
        return cell;

      return this.CreateOrRecoverCell( column, this.DataGridContext );
    }

    internal bool TryGetBindedCell( string fieldName, out Cell cell )
    {
      return this.TryGetBindedCell( fieldName, true, out cell );
    }

    internal bool TryGetBindedCell( ColumnBase column, out Cell cell )
    {
      return this.TryGetBindedCell( column, true, out cell );
    }

    internal bool TryGetBindedCell( string fieldName, bool prepareCell, out Cell cell )
    {
      return this.TryGetBindedCell( this.GetColumn( fieldName ), prepareCell, out cell );
    }

    internal bool TryGetBindedCell( ColumnBase column, bool prepareCell, out Cell cell )
    {
      cell = null;
      if( column == null )
        return false;

      if( !m_bindedCells.TryGetValue( column.FieldName, out cell ) )
        return false;

      if( prepareCell )
      {
        this.PrepareCell( cell, column, this.DataGridContext );
      }

      return true;
    }

    internal bool TryGetCreatedCell( string fieldName, out Cell cell )
    {
      if( m_bindedCells.TryGetValue( fieldName, out cell ) )
        return true;

      //The required cell may have been added in xaml, but may still not be added to the binded cells.  If this is the case, add it now.
      if( m_unprocessedCells.TryGetValue( fieldName, out cell ) )
      {
        m_unprocessedCells.Remove( fieldName );
        cell = m_parentRow.PrepareUnbindedCell( this.GetColumn( fieldName ), cell );

        m_bindedCells.Add( fieldName, cell );

        return true;
      }

      return false;
    }

    internal void MergeFreeCells()
    {
      if( m_freeCells == null )
        return;

      var dataGridContext = this.DataGridContext;
      var columns = this.Columns;

      foreach( Cell cell in m_freeCells )
      {
        this.MergeFreeCell( cell, dataGridContext, columns );
      }

      //Try to be GC friendly
      m_freeCells.Clear();
      m_freeCells.TrimExcess();
      m_freeCells = null;

      m_freeCellsHaveBeenMerged = true;
    }

    internal bool HasVirtualizedCell( string fieldName )
    {
      if( string.IsNullOrWhiteSpace( fieldName ) )
        return false;

      if( m_bindedCells.ContainsKey( fieldName ) )
        return true;

      if( m_unbindedCells.ContainsKey( fieldName ) )
        return true;

      return false;
    }

    internal Cell AddVirtualizedCell( ColumnBase column )
    {
      var fieldName = column.FieldName;
      Cell cell;
      var addToCells = true;

      if( m_unprocessedCells.TryGetValue( fieldName, out cell ) )
      {
        //A cell was provided in xaml
        m_unprocessedCells.Remove( fieldName );
        addToCells = false;
      }

      //If no cell was provided, this will generate one.
      cell = m_parentRow.PrepareUnbindedCell( column, cell );

      m_unbindedCells.Add( fieldName, cell );

      //A new generated cell must be added to the CellSet.  If it was provided through xaml, it is already added.
      if( addToCells )
      {
        m_cells.Add( cell );
      }

      return cell;
    }

    internal void RemoveVirtualizedCell( ColumnBase column )
    {
      Cell cell;
      var binded = true;

      if( !this.TryGetBindedCell( column, false, out cell ) )
      {
        binded = false;
        this.TryRecoverCell( column, out cell, false );
      }

      if( cell != null )
      {
        //Only remove cells that can be generated without any unique configuration (e.g. no custom template, no ResultPropertyName, etc..)
        if( this.CanBeRemoved( cell ) )
        {
          this.InternalRemoveCore( cell, binded );
        }
        else
        {
          if( binded )
          {
            m_bindedCells.Remove( cell.FieldName );
            cell.Visibility = Visibility.Collapsed;
            cell.RemoveContentBinding();
          }
          m_unprocessedCells.Add( cell.FieldName, cell );
        }
      }
    }

    internal void BindRecycledCells( ColumnCollection columns )
    {
      if( m_recyclingBins == null )
        return;

      Cell cell;

      if( columns != null )
      {
        foreach( ColumnBase column in columns )
        {
          if( m_bindedCells.ContainsKey( column.FieldName ) )
            continue;

          if( this.TryGetRecycledCell( column, out cell ) )
          {
            cell.ClearValue( Cell.VisibilityProperty );
            m_bindedCells.Add( cell.FieldName, cell );
          }
        }
      }

      this.ClearRecycleBins();
    }

    internal void BindVirtualizedCells()
    {
      foreach( KeyValuePair<string, Cell> item in m_unbindedCells )
      {
        var cell = item.Value;
        cell.ClearValue( Cell.VisibilityProperty );
        m_bindedCells.Add( item.Key, cell );
      }
      m_unbindedCells.Clear();

      foreach( KeyValuePair<string, Cell> item in m_unprocessedCells )
      {
        var cell = m_parentRow.PrepareUnbindedCell( this.GetColumn( item.Key ), item.Value );
        cell.ClearValue( Cell.VisibilityProperty );
        m_bindedCells.Add( item.Key, cell );
      }
      m_unprocessedCells.Clear();
    }

    internal void ClearOutOfViewBindedCells( List<Cell> cells )
    {
      foreach( Cell cell in cells )
      {
        if( this.CanBeRemoved( cell ) )
        {
          this.InternalRemoveCore( cell, true );
          continue;
        }

        //No need to add the cell's fieldName to the FixedCellPanel.PermanentScrollingFieldNames,
        //for FixedCellPanel.UpdateChildren() will take care of it in the next layout pass.
        cell.RemoveContentBinding();
      }
    }

    internal void ClearVirtualizedCells()
    {
      foreach( Cell cell in m_unbindedCells.Values )
      {
        if( this.CanBeRemoved( cell ) )
        {
          this.InternalRemoveCore( cell );
          continue;
        }

        //No need to add the cell's fieldName to the FixedCellPanel.PermanentScrollingFieldNames,
        //for FixedCellPanel.UpdateChildren() will take care of it in the next layout pass.
        cell.RemoveContentBinding();
        cell.Arrange( new Rect( 0, 0, 0, 0 ) );
        cell.ClearValue( Cell.VisibilityProperty );
        m_bindedCells.Add( cell.FieldName, cell );
      }

      m_unbindedCells.Clear();

      foreach( Cell cell in m_unprocessedCells.Values )
      {
        if( this.CanBeRemoved( cell ) )
        {
          this.InternalRemoveCore( cell );
          continue;
        }

        var fieldName = cell.FieldName;
        m_parentRow.PrepareUnbindedCell( this.GetColumn( fieldName ), cell );
        cell.Arrange( new Rect( 0, 0, 0, 0 ) );
        cell.ClearValue( Cell.VisibilityProperty );
        m_bindedCells.Add( fieldName, cell );
      }

      m_unprocessedCells.Clear();
    }

    internal void VirtualizeRecycledCells()
    {
      if( m_recyclingBins == null )
        return;

      foreach( LinkedList<Cell> recycleBin in m_recyclingBins.Values )
      {
        foreach( Cell cell in recycleBin )
        {
          if( !m_unbindedCells.ContainsKey( cell.FieldName ) )
          {
            m_unbindedCells.Add( cell.FieldName, cell );
            continue;
          }

          Debug.Fail( "Why is there a cell with the same fieldname already present in the unbindedCells?" );
          this.InternalRemoveCore( cell );
        }
        recycleBin.Clear();
      }

      m_recyclingBins.Clear();
      m_recyclingBins = null;
    }

    internal void SynchronizeRecyclingBinsWithRecyclingGroups( IEnumerable<ColumnBase> visibleColumns )
    {
      if( m_recyclingBins == null )
        return;

      //Get the recycling groups of visible columns only
      var recyclingGroups = new List<object>();
      foreach( ColumnBase column in visibleColumns )
      {
        var recyclingGroup = this.GetRecyclingGroup( column );
        if( !recyclingGroups.Contains( recyclingGroup ) )
        {
          recyclingGroups.Add( recyclingGroup );
        }
      }

      //Clear cells and remove recycleBins not linked to a currently used recyclingGroup.
      var unusedRecyclingGroups = new List<object>();
      foreach( KeyValuePair<object, LinkedList<Cell>> item in m_recyclingBins )
      {
        var recyclingGroup = item.Key;
        if( recyclingGroups.Contains( recyclingGroup ) )
          continue;

        unusedRecyclingGroups.Add( recyclingGroup );
        var recycleBin = item.Value;
        foreach( Cell cell in recycleBin )
        {
          this.InternalRemoveCore( cell );
        }
        recycleBin.Clear();
      }

      foreach( object recyclingGroup in unusedRecyclingGroups )
      {
        m_recyclingBins.Remove( recyclingGroup );
      }
    }

    private bool CanBeRemoved( Cell cell )
    {
      // Calling this method assumes it is for cells that are handled by the FixedCellPanel, which does not handle template cells,
      // thus no need to check if it is one through Row.GetIsTemplateCell( cell ).
      return ( cell.CanBeRecycled ) && ( cell.CanBeCollapsed );
    }

    private void ReleaseCore( Cell cell )
    {
      Debug.Assert( !VirtualizingCellCollection.IsFreeCell( cell ), "Should always have a field name" );

      m_bindedCells.Remove( cell.FieldName );

      switch( this.VirtualizationMode )
      {
        case ColumnVirtualizationMode.None:
          {
            //Not virtualizing
            this.InternalRemoveCore( cell );
            break;
          }

        case ColumnVirtualizationMode.Recycling:
          {
            // If recycling, put the cell into the appropriate recycle bin.
            var recyclingGroup = this.GetRecyclingGroup( this.GetColumn( cell ) );
            var recycleBin = this.GetRecycleBinOrNew( recyclingGroup );

            // A released cell shouldn't be in the recycle bin already.
            Debug.Assert( !recycleBin.Contains( cell ) );
            recycleBin.Add( cell );
            break;
          }

        case ColumnVirtualizationMode.Virtualizing:
          {
            //Virtualizing with no recycling
            m_unbindedCells.Add( cell.FieldName, cell );
            break;
          }
      }
    }

    private void InternalAddCore( Cell cell )
    {
      //This method should be invoked only to add cells provided by the user through Row.Cells, in xaml for instance.
      //For template cells, an other mechanism is used (See Row.OnApplyTemplate()).
      Debug.Assert( cell != null );

      //If a cell is provided in code behind, the user must provide a fieldname, and therefore it can be merged right away.
      if( m_freeCellsHaveBeenMerged )
      {
        this.MergeFreeCell( cell, this.DataGridContext, this.Columns );
        return;
      }

      if( m_freeCells == null )
      {
        m_freeCells = new List<Cell>( 1 );
      }

      m_freeCells.Add( cell );
      m_cells.Add( cell );
    }

    private void InternalRemoveCore( Cell cell, bool unbind = false )
    {
      Debug.Assert( !VirtualizingCellCollection.IsFreeCell( cell ), "Should always have a field name" );

      if( unbind )
      {
        m_bindedCells.Remove( cell.FieldName );
      }

      m_cells.Remove( cell );
      m_parentRow.RemoveFromVisualTree( cell );

      //This must absolutely be done once every collection has removed the cell, for the ParentColumn will be set to null, thus the FieldName will be lost.
      cell.CleanUpOnRemove();
    }

    private Cell CreateOrRecoverCell( string fieldName )
    {
      return this.CreateOrRecoverCell( this.GetColumn( fieldName ), this.DataGridContext );
    }

    private Cell CreateOrRecoverCell( ColumnBase column, DataGridContext dataGridContext )
    {
      Cell cell;
      if( !this.TryRecoverCell( column, out cell, true ) )
      {
        // Create a new cell if a suitable cell was not found.
        cell = m_parentRow.ProvideCell( column );
        Debug.Assert( cell != null );

        //Add the new cell to the CellSet
        m_cells.Add( cell );
      }
      else
      {
        //The current local value must be cleared instead of setting a new local value to give a chance for a style to set a value.
        cell.ClearValue( Cell.VisibilityProperty );
      }

      // Make sure the cell is initialized and prepared to be used.
      this.PrepareCell( cell, column, dataGridContext );

      //Once the fieldname is updated, add it to the dictionary
      m_bindedCells.Add( cell.FieldName, cell );

      //Need to notify only when cells are not already binded, since the binded cells will get properly measured and arranged by the Virtualizing/FixedCellSubPanel(s).
      if( !this.IsUpdating )
      {
        this.NotifyCollectionChanged();
      }

      return cell;
    }

    private void PrepareCell( Cell cell, ColumnBase column, DataGridContext dataGridContext )
    {
      //context can be null when the DataGridCollectionView directly has a list of Xceed.Wpf.DataGrid.DataRow
      if( ( cell == null ) || ( dataGridContext == null ) )
        return;

      // Initialize the cell in case it hasn't been initialized or in case of a column change.
      cell.Initialize( dataGridContext, m_parentRow, column );

      // Certain non recycling cells like StatCell must not be (re)prepared if their binding is not set, for performance reasons.
      if( cell.HasAliveContentBinding )
      {
        // Make sure the cell is prepared before leaving this method.
        cell.PrepareContainer( dataGridContext, m_parentRow.DataContext );
      }
    }

    private bool TryRecoverCell( ColumnBase column, out Cell cell, bool includeUnprocessedCells )
    {
      cell = null;

      if( column == null )
        return false;

      switch( this.VirtualizationMode )
      {
        case ColumnVirtualizationMode.Recycling:
          {
            return this.TryGetRecycledCell( column, out cell );
          }

        case ColumnVirtualizationMode.Virtualizing:
          {
            //When not recycling, get the cell already created for the required column.
            if( ( m_unbindedCells.Count > 0 ) && ( m_unbindedCells.TryGetValue( column.FieldName, out cell ) ) )
            {
              m_unbindedCells.Remove( column.FieldName );
              return true;
            }

            //It may still be unprocessed (provided in xaml), so process it now.
            if( ( m_unprocessedCells.Count > 0 ) && ( includeUnprocessedCells ) && ( m_unprocessedCells.TryGetValue( column.FieldName, out cell ) ) )
            {
              m_unprocessedCells.Remove( column.FieldName );
              cell = m_parentRow.PrepareUnbindedCell( column, cell );
              return true;
            }

            return false;
          }

        default:
          {
            //Not virtualizing
            return false;
          }
      }
    }

    private bool TryGetRecycledCell( ColumnBase column, out Cell cell )
    {
      cell = null;

      if( m_recyclingBins == null )
        return false;

      // Make sure the cell recycling group is up-to-date.
      var dataGridContext = this.DataGridContext;
      if( dataGridContext != null )
      {
        var propertyDescription = ItemsSourceHelper.CreateOrGetPropertyDescriptionFromColumn( dataGridContext, column, null );
        ItemsSourceHelper.UpdateColumnFromPropertyDescription( column, dataGridContext.DataGridControl.DefaultCellEditors, dataGridContext.AutoCreateForeignKeyConfigurations, propertyDescription );
      }

      var recyclingGroup = this.GetRecyclingGroup( column );
      var recycleBin = default( LinkedList<Cell> );

      if( m_recyclingBins.TryGetValue( recyclingGroup, out recycleBin ) )
      {
        // Try to recycle a cell that has already the appropriate column in order to minimize the cell's initialization time.
        var targetNode = default( LinkedListNode<Cell> );

        for( var node = recycleBin.Last; node != null; node = node.Previous )
        {
          targetNode = node;

          if( targetNode.Value.ParentColumn == column )
            break;
        }

        // From here, the target node is either:
        //   1. The cell with minimal initialization time.
        //   2. The oldest cell.
        //   3. null in case of an empty bin.
        if( targetNode != null )
        {
          cell = targetNode.Value;
          recycleBin.Remove( targetNode );
        }
      }

      return ( cell != null );
    }

    private void MergeFreeCell( Cell cell, DataGridContext dataGridContext, ColumnCollection columns )
    {
      var fieldName = cell.FieldName;

      // Make sure the field name is valid and there is no other cell using it.
      this.CheckCellFieldName( fieldName );
      this.CheckCellAlreadyExists( fieldName );

      // Cells added in xaml (e.g a StatCell) must be added to the visual tree right away, even if they are not visible for the moment,
      // to make sure any binding defined on a property works correctly.
      m_parentRow.AddToVisualTree( cell );

      //These cell will need further processing when not recycling before they can be used as BindedCells.
      if( this.VirtualizationMode == ColumnVirtualizationMode.Virtualizing )
      {
        m_unprocessedCells.Add( fieldName, cell );
        return;
      }

      //This following calls will be done for unprocessedCells when actually using them.
      cell.PrepareDefaultStyleKey( dataGridContext.DataGridControl.GetView() );

      //It is possible a cell provided through Row.Cells does not correspond to any actual column in the grid, so do not prepare it.
      var parentColumn = columns[ cell.FieldName ];
      if( parentColumn != null )
      {
        cell.Initialize( dataGridContext, m_parentRow, parentColumn );
        cell.PrepareContainer( dataGridContext, m_parentRow.DataContext );
      }

      m_bindedCells.Add( fieldName, cell );
    }

    private ColumnBase GetColumn( Cell cell )
    {
      if( cell == null )
        return null;

      var column = cell.ParentColumn;
      if( column == null )
      {
        column = this.GetColumn( cell.FieldName );
      }

      return column;
    }

    private ColumnBase GetColumn( string fieldName )
    {
      if( ( this.Columns == null ) || ( string.IsNullOrEmpty( fieldName ) ) )
        return null;

      return this.Columns[ fieldName ];
    }

    private object GetRecyclingGroup( ColumnBase column )
    {
      return column.GetCellRecyclingGroupOrDefault();
    }

    private ICollection<Cell> GetRecycleBinOrNew( object recyclingGroup )
    {
      if( m_recyclingBins == null )
      {
        m_recyclingBins = new Dictionary<object, LinkedList<Cell>>();
      }

      LinkedList<Cell> recycleBin;

      if( !m_recyclingBins.TryGetValue( recyclingGroup, out recycleBin ) )
      {
        recycleBin = new LinkedList<Cell>();
        m_recyclingBins.Add( recyclingGroup, recycleBin );
      }

      return recycleBin;
    }

    private void ClearRecycleBins()
    {
      foreach( LinkedList<Cell> recycleBin in m_recyclingBins.Values )
      {
        foreach( Cell cell in recycleBin )
        {
          this.InternalRemoveCore( cell );
        }
        recycleBin.Clear();
      }

      m_recyclingBins.Clear();
      m_recyclingBins = null;
    }

    private void CheckFieldName( string fieldName )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        throw new DataGridInternalException( "A field name is required.", this.DataGridContext.DataGridControl );
    }

    private void CheckCellFieldName( string fieldName )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        throw new DataGridInternalException( "A Cell should always have a FieldName.", this.DataGridContext.DataGridControl );
    }

    private void CheckCellAlreadyExists( string fieldName )
    {
      var alreadyExists = false;

      if( this.VirtualizationMode == ColumnVirtualizationMode.Virtualizing )
      {
        alreadyExists = m_unprocessedCells.ContainsKey( fieldName );

        if( !alreadyExists )
        {
          alreadyExists = m_unbindedCells.ContainsKey( fieldName );
        }
      }

      if( !alreadyExists )
      {
        alreadyExists = m_bindedCells.ContainsKey( fieldName );
      }

      if( alreadyExists )
      {
        this.ThrowCellAlreadyExists( fieldName );
      }
    }

    private void ThrowCellAlreadyExists( string fieldName )
    {
      throw new DataGridInternalException( string.Format( "A cell with FieldName {0} already exists.", fieldName ), this.DataGridContext.DataGridControl );
    }

    private static bool IsFreeCell( Cell cell )
    {
      Debug.Assert( cell != null );

      return string.IsNullOrEmpty( cell.FieldName );
    }

    private readonly Row m_parentRow;
    private readonly ICollection<Cell> m_cells;

    // This Dictionary is used as a caching system to optimize lookup when virtualizing instead of parsing every columns looking for the fieldname,
    // e.g.: looking for the last one when 1000 cells in the parent row. When virtualizing, only the cell visible in the viewport will be contained
    // within this collection speeding the lookup.
    private readonly Dictionary<string, Cell> m_bindedCells = new Dictionary<string, Cell>();

    //This Dictionary is used in combinations with m_bindedCells when virtualizing without recycling. It contains cells assigned to a column, but not currently in view.
    private readonly Dictionary<string, Cell> m_unbindedCells = new Dictionary<string, Cell>();

    //This Dictionary is used to temporarily store FreeCells until they are correctly prepared and added to the designated dictionnary (binded or unbinded cells)
    private readonly Dictionary<string, Cell> m_unprocessedCells = new Dictionary<string, Cell>();

    // This Dictionary contains the recycling queues for the cells ready to be recycled.
    private Dictionary<object, LinkedList<Cell>> m_recyclingBins; //null

    // This collection is used to store cells that are added to the Collection before the FieldName and/or ParentColumn being set.
    // This occurs when defining a Row with Cell in XAML during a BeginInit / EndInit.
    private List<Cell> m_freeCells; //null
    private bool m_freeCellsHaveBeenMerged = false;

    #region VirtualizingCellCollectionEnumerator Nested Type

    private class VirtualizingCellCollectionEnumerator : IEnumerator<Cell>, IEnumerator
    {
      internal VirtualizingCellCollectionEnumerator( VirtualizingCellCollection collection )
      {
        if( collection == null )
          throw new ArgumentNullException( "collection" );

        m_collection = collection;

        this.Reset();
      }

      public Cell Current
      {
        get
        {
          return m_current;
        }
      }

      object IEnumerator.Current
      {
        get
        {
          return this.Current;
        }
      }

      public bool MoveNext()
      {
        if( m_collection == null )
          throw new ObjectDisposedException( string.Empty );

        var count = m_collection.Count;
        m_index = Math.Min( m_index + 1, count );

        if( m_index < count )
        {
          m_current = m_collection[ m_index ];
        }
        else
        {
          m_current = null;
        }

        return ( m_current != null );
      }

      public void Reset()
      {
        m_index = -1;
        m_current = null;
      }

      void IDisposable.Dispose()
      {
        m_collection = null;
        m_current = null;
      }

      private VirtualizingCellCollection m_collection;
      private int m_index;
      private Cell m_current; // = null;
    }

    #endregion

    #region VirtualizingItemsList Nested Type

    // This class is used to force the Collection<T>.Items.Count to return the total number of columns. Every other method except this[index] are not supported 
    private class VirtualizingItemsList : IList<Cell>
    {
      internal VirtualizingItemsList()
      {
      }

      internal void SetParent( VirtualizingCellCollection collection )
      {
        if( collection == null )
          throw new ArgumentNullException( "collection" );

        if( m_collection != null )
          throw new InvalidOperationException();

        m_collection = collection;
      }

      int ICollection<Cell>.Count
      {
        get
        {
          ColumnCollection columns = this.Columns;
          if( columns != null )
            return columns.Count;

          return 0;
        }
      }

      bool ICollection<Cell>.IsReadOnly
      {
        get
        {
          return false;
        }
      }

      Cell IList<Cell>.this[ int index ]
      {
        get
        {
          var cell = default( Cell );
          var columns = this.Columns;

          if( columns != null )
          {
            var column = columns[ index ];

            if( column != null )
            {
              cell = m_collection[ column ];
            }
          }
          else
          {
            Debug.Fail( "An unprepared Row was asked for Cell" );
          }

          return cell;
        }
        set
        {
          throw new NotSupportedException();
        }
      }

      private ColumnCollection Columns
      {
        get
        {
          if( m_collection == null )
            return null;

          return m_collection.Columns;
        }
      }

      int IList<Cell>.IndexOf( Cell item )
      {
        if( item == null )
          return -1;

        var columns = this.Columns;

        if( columns == null )
          return -1;

        string fieldName = item.FieldName;
        var column = columns[ fieldName ];

        if( column == null )
          return -1;

        return columns.IndexOf( column );
      }

      void IList<Cell>.Insert( int index, Cell item )
      {
        throw new NotSupportedException();
      }

      void IList<Cell>.RemoveAt( int index )
      {
        throw new NotSupportedException();
      }

      void ICollection<Cell>.Add( Cell item )
      {
        throw new NotSupportedException();
      }

      void ICollection<Cell>.Clear()
      {
        throw new NotSupportedException();
      }

      bool ICollection<Cell>.Contains( Cell item )
      {
        if( item == null )
          return false;

        string fieldName = item.FieldName;
        var columns = this.Columns;

        if( columns != null )
          return ( columns[ fieldName ] != null );

        Debug.Fail( "An unprepared Row was asked for Cell" );

        return false;
      }

      void ICollection<Cell>.CopyTo( Cell[] array, int arrayIndex )
      {
        throw new NotSupportedException();
      }

      bool ICollection<Cell>.Remove( Cell item )
      {
        throw new NotSupportedException();
      }

      IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator()
      {
        return new VirtualizingCellCollectionEnumerator( m_collection );
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return new VirtualizingCellCollectionEnumerator( m_collection );
      }

      private VirtualizingCellCollection m_collection;
    }

    #endregion

    #region ReadOnlyCollection Nested Type

    private sealed class ReadOnlyCollection<T> : ICollection<T>
    {
      internal ReadOnlyCollection( ICollection<T> collection )
      {
        if( collection == null )
          throw new ArgumentNullException( "collection" );

        m_collection = collection;
      }

      int ICollection<T>.Count
      {
        get
        {
          return m_collection.Count;
        }
      }

      bool ICollection<T>.IsReadOnly
      {
        get
        {
          return true;
        }
      }

      void ICollection<T>.Add( T item )
      {
        throw new NotSupportedException();
      }

      void ICollection<T>.Clear()
      {
        throw new NotSupportedException();
      }

      bool ICollection<T>.Contains( T item )
      {
        return m_collection.Contains( item );
      }

      void ICollection<T>.CopyTo( T[] array, int arrayIndex )
      {
        m_collection.CopyTo( array, arrayIndex );
      }

      bool ICollection<T>.Remove( T item )
      {
        throw new NotSupportedException();
      }

      IEnumerator<T> IEnumerable<T>.GetEnumerator()
      {
        return m_collection.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return m_collection.GetEnumerator();
      }

      private readonly ICollection<T> m_collection;
    }

    #endregion

    #region CellSet Nested Type

    private sealed class CellSet : ICollection<Cell>, IWeakEventListener
    {
      internal CellSet( Row parentRow )
      {
        Debug.Assert( parentRow != null );

        m_parentRow = parentRow;
      }

      public int Count
      {
        get
        {
          return m_collection.Count;
        }
      }

      bool ICollection<Cell>.IsReadOnly
      {
        get
        {
          return false;
        }
      }

      public bool Contains( Cell item )
      {
        return m_collection.Contains( item );
      }

      public void Add( Cell item )
      {
        Debug.Assert( item != null );

        m_collection.Add( item );

        this.RegisterBringIntoView( item );
      }

      public bool Remove( Cell item )
      {
        Debug.Assert( item != null );

        if( !m_collection.Remove( item ) )
          return false;

        this.UnregisterBringIntoView( item );

        return true;
      }

      public void Clear()
      {
        foreach( var item in m_collection )
        {
          this.UnregisterBringIntoView( item );
        }

        m_collection.Clear();
        m_collection.TrimExcess();
      }

      private void RegisterBringIntoView( Cell item )
      {
        RequestBringIntoViewWeakEventManager.AddListener( item, this );
      }

      private void UnregisterBringIntoView( Cell item )
      {
        RequestBringIntoViewWeakEventManager.RemoveListener( item, this );
      }

      private void OnRequestBringIntoView( object sender, RequestBringIntoViewEventArgs e )
      {
        if( e.Handled )
          return;

        var targetCell = ( Cell )sender;
        Debug.Assert( targetCell != null );
        Debug.Assert( m_collection.Contains( targetCell ) );

        if( ( targetCell != e.TargetObject ) && !targetCell.IsAncestorOf( e.TargetObject ) )
          return;

        // Do not process the bring into view for a templated cell.
        if( Row.GetIsTemplateCell( targetCell ) )
          return;

        var targetRow = targetCell.ParentRow;
        Debug.Assert( ( targetRow == null ) || ( targetRow == m_parentRow ) );

        var targetHost = ( targetRow != null ) ? targetRow.CellsHostPanel as IVirtualizingCellsHost : null;

        e.Handled = ( targetHost != null )
                 && ( targetHost.BringIntoView( targetCell, e ) );
      }

      void ICollection<Cell>.CopyTo( Cell[] array, int arrayIndex )
      {
        m_collection.CopyTo( array, arrayIndex );
      }

      IEnumerator<Cell> IEnumerable<Cell>.GetEnumerator()
      {
        return m_collection.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return m_collection.GetEnumerator();
      }

      bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
      {
        if( managerType == typeof( RequestBringIntoViewWeakEventManager ) )
        {
          this.OnRequestBringIntoView( sender, ( RequestBringIntoViewEventArgs )e );
          return true;
        }

        return false;
      }

      private readonly HashSet<Cell> m_collection = new HashSet<Cell>();
      private readonly Row m_parentRow;
    }

    #endregion
  }
}
