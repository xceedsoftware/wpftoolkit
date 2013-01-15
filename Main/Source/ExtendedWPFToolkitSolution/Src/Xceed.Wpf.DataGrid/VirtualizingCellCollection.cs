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
      m_readOnlyCellsCollection = new ReadOnlyCollection<Cell>( m_cells );

      // We must assign the VirtualizingCellCollection of the VirtualizingItemsList manually
      ( ( VirtualizingItemsList )this.Items ).SetParent( this );
    }

    #region Cells Property

    internal ICollection<Cell> Cells
    {
      get
      {
        this.MergeFreeCells();

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
        this.MergeFreeCells();

        return m_bindedCells.Values;
      }
    }

    #endregion

    #region DataGridContext Property (Private)

    private DataGridContext DataGridContext
    {
      get
      {
        return DataGridControl.GetDataGridContext( m_parentRow );
      }
    }

    #endregion

    #region ColumnManager Property (Private)

    private ColumnVirtualizationManager ColumnManager
    {
      get
      {
        DataGridContext context = this.DataGridContext;
        if( context == null )
          throw new DataGridInternalException();

        return context.ColumnVirtualizationManager;
      }
    }

    #endregion

    #region NeedsNotification Property

    internal bool AlreadyUpdating
    {
      get;
      set;
    }

    #endregion

    #region CollectionChanged Event

    internal event EventHandler CollectionChanged;

    private void NotifyCollectionChanged()
    {
      if( this.CollectionChanged != null )
      {
        this.CollectionChanged( this, EventArgs.Empty );
      }
    }

    #endregion

    public override Cell this[ ColumnBase column ]
    {
      get
      {
        return this.GetCell( column );
      }
    }

    public override Cell this[ string fieldName ]
    {
      get
      {
        return this.GetCell( fieldName );
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
      this.Bind( cell, true );
    }

    internal override void InternalClear()
    {
      this.Unregister( m_bindedCells.Values );

      m_bindedCells.Clear();
      m_recyclingBins.Clear();

      m_freeCells.Clear();
      m_freeCells.TrimExcess();

      m_cells.Clear();
      m_cells.TrimExcess();
    }

    internal override void InternalInsert( int index, Cell cell )
    {
      this.Bind( cell, false );
    }

    internal override void InternalRemove( Cell cell )
    {
      this.Unbind( cell, false );
    }

    internal override void InternalRemoveAt( int index )
    {
      ColumnBase column = this.DataGridContext.Columns[ index ];
      if( column == null )
        return;

      Cell cell;
      if( !m_bindedCells.TryGetValue( column.FieldName, out cell ) )
        return;

      this.Unbind( cell, false );
    }

    internal override void InternalSetCell( int index, Cell cell )
    {
      throw new NotSupportedException();
    }

    internal void Release( Cell cell )
    {
      // Make sure the cell is allowed to be recycled.
      if( ( cell == null ) || ( !this.IsRecyclableCell( cell ) ) )
        return;

      // Remove the target cell from the binded cells.
      this.Unbind( cell, true );

      cell.Visibility = Visibility.Collapsed;
    }

    internal bool TryGetCell( ColumnBase column, out Cell cell )
    {
      cell = null;
      if( column == null )
        return false;

      this.MergeFreeCells();

      if( !m_bindedCells.TryGetValue( column.FieldName, out cell ) )
        return false;

      this.PrepareCell( cell, column, this.DataGridContext );

      return true;
    }

    internal void ClearUnusedRecycleBins( IEnumerable<ColumnBase> columns )
    {
      if( m_recyclingBins.Count == 0 )
        return;

      HashSet<object> unusedRecyclingGroups = new HashSet<object>( m_recyclingBins.Keys );

      if( columns != null )
      {
        foreach( ColumnBase column in columns )
        {
          if( unusedRecyclingGroups.Count == 0 )
            return;

          unusedRecyclingGroups.Remove( this.GetRecyclingGroup( column ) );
        }
      }

      foreach( object recyclingGroup in unusedRecyclingGroups )
      {
        LinkedList<Cell> recycleBin = m_recyclingBins[ recyclingGroup ];
        m_recyclingBins.Remove( recyclingGroup );

        foreach( Cell cell in recycleBin )
        {
          m_cells.Remove( cell );
        }
      }
    }

    private void OnCellRequestBringIntoView( object sender, RequestBringIntoViewEventArgs e )
    {
      Cell cell = e.Source as Cell;
      if( cell == null )
        return;

      // Make sure the the current row if the cell's parent row.
      Row parentRow = cell.ParentRow;
      if( parentRow != m_parentRow )
      {
        this.Unregister( cell );
        return;
      }

      // Do not process the bring into view for a templated cell.
      if( this.IsTemplatedCell( cell ) )
        return;

      IVirtualizingCellsHost cellsHost = parentRow.CellsHostPanel as IVirtualizingCellsHost;
      if( cellsHost != null )
      {
        e.Handled = cellsHost.BringIntoView( cell );
      }
    }

    private void Bind( Cell cell, bool overwrite )
    {
      if( cell == null )
        throw new ArgumentNullException( "cell" );

      if( !this.IsFreeCell( cell ) )
      {
        string fieldName = cell.FieldName;

        Cell oldCell;
        if( m_bindedCells.TryGetValue( fieldName, out oldCell ) )
        {
          if( cell == oldCell )
            return;

          if( !overwrite )
            this.ThrowCellBinded( fieldName );

          this.InternalRemove( oldCell );
        }

        m_bindedCells.Add( fieldName, cell );
      }
      else
      {
        m_freeCells.Add( cell );
      }

      this.Register( cell );

      m_cells.Add( cell );
    }

    private void Unbind( Cell cell, bool recycle )
    {
      if( cell == null )
        throw new ArgumentNullException( "cell" );

      if( !this.IsFreeCell( cell ) )
      {
        m_bindedCells.Remove( cell.FieldName );
      }
      else
      {
        Debug.Fail( "Should always have a field name" );
      }

      this.Unregister( cell );

      if( recycle )
      {
        // Put the cell into the appropriate recycle bin.
        object recyclingGroup = this.GetRecyclingGroup( this.GetColumn( cell ) );
        ICollection<Cell> recycleBin = this.GetRecycleBinOrNew( recyclingGroup );

        // A released cell shouldn't be in the recycle bin already.
        Debug.Assert( !recycleBin.Contains( cell ) );
        recycleBin.Add( cell );
      }
      else
      {
        m_cells.Remove( cell );
      }
    }

    private void Register( Cell cell )
    {
      if( cell == null )
        return;

      cell.RequestBringIntoView += new RequestBringIntoViewEventHandler( this.OnCellRequestBringIntoView );
    }

    private void Unregister( Cell cell )
    {
      if( cell == null )
        return;

      cell.RequestBringIntoView -= new RequestBringIntoViewEventHandler( this.OnCellRequestBringIntoView );
    }

    private void Unregister( IEnumerable<Cell> cells )
    {
      if( cells == null )
        return;

      foreach( Cell cell in cells )
      {
        this.Unregister( cell );
      }
    }

    private Cell GetCell( string fieldName )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        return null;

      DataGridContext context = this.DataGridContext;
      if( context == null )
      {
        Debug.Fail( "An unprepared Row was asked a Cell" );
        return null;
      }

      ColumnBase column = this.GetColumn( context, fieldName );
      if( column == null )
        return null;

      return this.GetCell( fieldName, column, context );
    }

    private Cell GetCell( ColumnBase column )
    {
      if( column == null )
        return null;

      return this.GetCell( column.FieldName, column, this.DataGridContext );
    }

    private Cell GetCell( string fieldName, ColumnBase column, DataGridContext context )
    {
      this.CheckFieldName( fieldName );
      this.MergeFreeCells();

      // Try to recycle a cell if it wasn't already binded.
      Cell cell;
      if( !( m_bindedCells.TryGetValue( fieldName, out cell ) ) && ( column != null ) )
      {
        // Create a new cell if recycling wasn't possible.
        if( !this.TryGetRecycledCell( column, out cell ) )
        {
          cell = m_parentRow.ProvideCell( column );

          Debug.Assert( cell != null );
        }
        else
        {
          cell.Visibility = Visibility.Visible;
        }

        // Make sure the cell is initialized and prepared to be used.
        this.PrepareCell( cell, column, context );
        this.InternalAdd( cell );

        //Need to notify only when cells are not already binded, since the binded cells will get properly measured and arranged by the Virtualizing/FixedCellSubPanel(s).
        if( !this.AlreadyUpdating )
        {
          this.NotifyCollectionChanged();
        }
      }
      else if( cell != null )
      {
        // Make sure the cell is initialized and prepared to be used.
        this.PrepareCell( cell, column, context );
      }

      return cell;
    }

    private void PrepareCell( Cell cell, ColumnBase column, DataGridContext context )
    {
      //context can be null when the DataGridCollectionView directly has a list of Xceed.Wpf.DataGrid.DataRow
      //Certain non recycling cells like StatCell must not be (re)intialized at this point if their binding is not set, for performance reasons.
      if( ( cell == null ) || ( context == null ) || ( !cell.HasAliveContentBinding ) )
        return;

      // Initialize the cell in case it hasn't been initialized or in case of a column change.
      cell.Initialize( context, m_parentRow, column );

      // Make sure the cell is prepared before leaving this method.
      cell.PrepareContainer( context, m_parentRow.DataContext );
    }

    private bool TryGetRecycledCell( ColumnBase column, out Cell cell )
    {
      cell = null;

      if( column == null )
        return false;

      object recyclingGroup = this.GetRecyclingGroup( column );
      LinkedList<Cell> recycleBin;

      if( m_recyclingBins.TryGetValue( recyclingGroup, out recycleBin ) )
      {
        // Try to recycle a cell that has already the appropriate column in order to minimize the cell's initialization time.
        LinkedListNode<Cell> targetNode = null;
        for( LinkedListNode<Cell> node = recycleBin.Last; node != null; node = node.Previous )
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

    private void MergeFreeCell( Cell cell )
    {
      string fieldName = cell.FieldName;

      // Make sure the field name is valid and there is no other cell using it.
      this.CheckCellFieldName( fieldName );
      this.CheckCellBinded( fieldName );

      m_bindedCells.Add( fieldName, cell );
    }

    private void MergeFreeCells()
    {
      if( m_freeCells.Count == 0 )
        return;

      foreach( Cell cell in m_freeCells )
      {
        this.MergeFreeCell( cell );
      }

      m_freeCells.Clear();
      m_freeCells.TrimExcess();
    }

    private ColumnBase GetColumn( Cell cell )
    {
      if( cell == null )
        return null;

      ColumnBase column = cell.ParentColumn;
      if( column == null )
      {
        column = this.GetColumn( cell.FieldName );
      }

      return column;
    }

    private ColumnBase GetColumn( string fieldName )
    {
      return this.GetColumn( this.DataGridContext, fieldName );
    }

    private ColumnBase GetColumn( DataGridContext context, string fieldName )
    {
      if( ( context == null ) || ( string.IsNullOrEmpty( fieldName ) ) )
        return null;

      return context.Columns[ fieldName ];
    }

    private object GetRecyclingGroup( ColumnBase column )
    {
      if( column == null )
        throw new ArgumentNullException( "column" );

      return column.GetCellRecyclingGroupOrDefault();
    }

    private ICollection<Cell> GetRecycleBinOrNew( object recyclingGroup )
    {
      LinkedList<Cell> recycleBin;
      if( !m_recyclingBins.TryGetValue( recyclingGroup, out recycleBin ) )
      {
        recycleBin = new LinkedList<Cell>();
        m_recyclingBins.Add( recyclingGroup, recycleBin );
      }

      return recycleBin;
    }

    private bool IsRecyclableCell( Cell cell )
    {
      return ( cell != null ) && ( !this.IsTemplatedCell( cell ) );
    }

    private bool IsTemplatedCell( Cell cell )
    {
      if( cell == null )
        return false;

      return Row.GetIsTemplateCell( cell );
    }

    private bool IsFreeCell( Cell cell )
    {
      if( cell == null )
        throw new ArgumentNullException( "cell" );

      return string.IsNullOrEmpty( cell.FieldName );
    }

    private void CheckFieldName( string fieldName )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        throw new DataGridInternalException( "A field name is required." );
    }

    private void CheckCellFieldName( string fieldName )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        throw new DataGridInternalException( "A Cell should always have a FieldName." );
    }

    private void CheckCellBinded( string fieldName )
    {
      if( m_bindedCells.ContainsKey( fieldName ) )
      {
        this.ThrowCellBinded( fieldName );
      }
    }

    private void ThrowCellBinded( string fieldName )
    {
      throw new DataGridInternalException( string.Format( "A cell with FieldName {0} already exists.", fieldName ) );
    }

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

        int count = m_collection.Count;
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

    #endregion VirtualizingCellCollectionEnumerator Nested Type

    #region VirtualizingItemsList Nested Type

    // This class is used to force the Collection<T>.Items.Count to return
    // the total number of columns. Every other method except this[index] are
    // not supported 
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
          DataGridContext dataGridContext = this.DataGridContext;
          if( dataGridContext != null )
            return dataGridContext.Columns.Count;

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
          Cell cell = null;
          DataGridContext dataGridContext = this.DataGridContext;

          if( dataGridContext != null )
          {
            ColumnBase column = dataGridContext.Columns[ index ];

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

      private DataGridContext DataGridContext
      {
        get
        {
          if( m_collection == null )
            return null;

          return m_collection.DataGridContext;
        }
      }

      int IList<Cell>.IndexOf( Cell item )
      {
        throw new NotSupportedException();
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
        DataGridContext dataGridContext = this.DataGridContext;

        if( dataGridContext != null )
          return ( dataGridContext.Columns[ fieldName ] != null );

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

    #endregion VirtualizingItemsList Nested Type

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

    #endregion ReadOnlyCollection Nested Type

    private readonly Row m_parentRow;
    private readonly HashSet<Cell> m_cells = new HashSet<Cell>();

    // This Dictionary is used as a caching system to optimize research when virtualizing instead of
    // parsing every columns looking for the fieldname. e.g.: looking for the last one when 1000
    // Cells in the parent row. When virtualizing, only the cell visible in the viewport will be contained
    // within this collection speeding the research.
    private readonly Dictionary<string, Cell> m_bindedCells = new Dictionary<string, Cell>();

    // This Dictionary contains the recycling queues for the cells ready to be recycled.
    private readonly Dictionary<object, LinkedList<Cell>> m_recyclingBins = new Dictionary<object, LinkedList<Cell>>();

    // This collection is used to store cells that are added to the Collection
    // before the FieldName and/or ParentColumn being set. This occurs when
    // defining a Row with Cell in XAML during a BeginInit / EndInit.
    private readonly List<Cell> m_freeCells = new List<Cell>( 0 );
  }
}
