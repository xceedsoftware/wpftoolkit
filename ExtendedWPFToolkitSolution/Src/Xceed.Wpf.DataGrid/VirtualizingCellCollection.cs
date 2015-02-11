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
      this.Bind( cell, true );
    }

    internal override void InternalClear()
    {
      m_recyclingBins = null;
      m_freeCells = null;

      m_bindedCells.Clear();
      m_cells.Clear();
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
      ColumnBase column = this.Columns[ index ];
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
      if( ( cell == null ) || ( !VirtualizingCellCollection.IsRecyclableCell( cell ) ) )
        return;

      // Remove the target cell from the binded cells.
      this.Unbind( cell, true );

      cell.Visibility = Visibility.Collapsed;
    }

    internal Cell GetCell( string fieldName, bool prepareCell )
    {
      if( string.IsNullOrEmpty( fieldName ) )
        return null;

      Cell cell;
      if( this.TryGetCell( fieldName, prepareCell, out cell ) )
        return cell;

      return this.CreateOrRecycleCell( fieldName );
    }

    internal Cell GetCell( ColumnBase column, bool prepareCell )
    {
      if( column == null )
        return null;

      Cell cell;
      if( this.TryGetCell( column, prepareCell, out cell ) )
        return cell;

      return this.CreateOrRecycleCell( column, this.DataGridContext );
    }

    internal bool TryGetCell( string fieldName, out Cell cell )
    {
      return this.TryGetCell( fieldName, true, out cell );
    }

    internal bool TryGetCell( ColumnBase column, out Cell cell )
    {
      return this.TryGetCell( column, true, out cell );
    }

    internal bool TryGetCell( string fieldName, bool prepareCell, out Cell cell )
    {
      return this.TryGetCell( this.GetColumn( fieldName ), prepareCell, out cell );
    }

    internal bool TryGetCell( ColumnBase column, bool prepareCell, out Cell cell )
    {
      cell = null;
      if( column == null )
        return false;

      this.MergeFreeCells();

      if( !m_bindedCells.TryGetValue( column.FieldName, out cell ) )
        return false;

      if( prepareCell )
      {
        this.PrepareCell( cell, column, this.DataGridContext );
      }

      return true;
    }

    internal bool TryGetBindedCell( string fieldName, out Cell cell )
    {
      this.MergeFreeCells();

      if( m_bindedCells.TryGetValue( fieldName, out cell ) )
        return true;

      return false;
    }

    internal void ClearUnusedRecycleBins( ColumnCollection columns )
    {
      if( m_recyclingBins == null )
        return;

      var unusedRecyclingGroups = new HashSet<object>( m_recyclingBins.Keys );

      if( columns != null )
      {
        foreach( ColumnBase column in columns )
        {
          if( unusedRecyclingGroups.Count == 0 )
            break;

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

      if( m_recyclingBins.Count == 0 )
      {
        m_recyclingBins = null;
      }
    }

    private void Bind( Cell cell, bool overwrite )
    {
      if( cell == null )
        throw new ArgumentNullException( "cell" );

      if( !VirtualizingCellCollection.IsFreeCell( cell ) )
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
        if( m_freeCells == null )
        {
          m_freeCells = new List<Cell>( 1 );
        }

        m_freeCells.Add( cell );
      }

      m_cells.Add( cell );
    }

    private void Unbind( Cell cell, bool recycle )
    {
      if( cell == null )
        throw new ArgumentNullException( "cell" );

      if( !VirtualizingCellCollection.IsFreeCell( cell ) )
      {
        m_bindedCells.Remove( cell.FieldName );
      }
      else
      {
        Debug.Fail( "Should always have a field name" );
      }

      if( recycle )
      {
        // Put the cell into the appropriate recycle bin.
        var recyclingGroup = this.GetRecyclingGroup( this.GetColumn( cell ) );
        var recycleBin = this.GetRecycleBinOrNew( recyclingGroup );

        // A released cell shouldn't be in the recycle bin already.
        Debug.Assert( !recycleBin.Contains( cell ) );
        recycleBin.Add( cell );
      }
      else
      {
        m_cells.Remove( cell );
      }
    }

    private Cell CreateOrRecycleCell( string fieldName )
    {
      return this.CreateOrRecycleCell( this.GetColumn( fieldName ), this.DataGridContext );
    }

    private Cell CreateOrRecycleCell( ColumnBase column, DataGridContext context )
    {
      // Create a new cell if recycling wasn't possible.
      Cell cell;
      if( !this.TryGetRecycledCell( column, out cell ) )
      {
        cell = m_parentRow.ProvideCell( column );
        Debug.Assert( cell != null );
      }
      else
      {
        cell.ClearValue( Cell.VisibilityProperty );
      }

      // Make sure the cell is initialized and prepared to be used.
      this.PrepareCell( cell, column, context );
      this.InternalAdd( cell );

      //Need to notify only when cells are not already binded, since the binded cells will get properly measured and arranged by the Virtualizing/FixedCellSubPanel(s).
      if( !this.IsUpdating )
      {
        this.NotifyCollectionChanged();
      }

      return cell;
    }

    private void PrepareCell( Cell cell, ColumnBase column, DataGridContext context )
    {
      //context can be null when the DataGridCollectionView directly has a list of Xceed.Wpf.DataGrid.DataRow
      if( ( cell == null ) || ( context == null ) )
        return;

      // Initialize the cell in case it hasn't been initialized or in case of a column change.
      cell.Initialize( context, m_parentRow, column );

      // Certain non recycling cells like StatCell must not be (re)prepared if their binding is not set, for performance reasons.
      if( cell.HasAliveContentBinding )
      {
        // Make sure the cell is prepared before leaving this method.
        cell.PrepareContainer( context, m_parentRow.DataContext );
      }
    }

    private bool TryGetRecycledCell( ColumnBase column, out Cell cell )
    {
      cell = null;

      if( ( column == null ) || ( m_recyclingBins == null ) )
        return false;

      object recyclingGroup = this.GetRecyclingGroup( column );
      LinkedList<Cell> recycleBin;

      if( m_recyclingBins.TryGetValue( recyclingGroup, out recycleBin ) )
      {
        // Try to recycle a cell that has already the appropriate column in order to minimize the cell's initialization time.
        LinkedListNode<Cell> targetNode = null;
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
      if( m_freeCells == null )
        return;

      foreach( Cell cell in m_freeCells )
      {
        this.MergeFreeCell( cell );
      }

      m_freeCells = null;
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

    private void CheckCellBinded( string fieldName )
    {
      if( m_bindedCells.ContainsKey( fieldName ) )
      {
        this.ThrowCellBinded( fieldName );
      }
    }

    private void ThrowCellBinded( string fieldName )
    {
      throw new DataGridInternalException( string.Format( "A cell with FieldName {0} already exists.", fieldName ), this.DataGridContext.DataGridControl );
    }

    private static bool IsRecyclableCell( Cell cell )
    {
      return ( cell != null )
          && !VirtualizingCellCollection.IsTemplatedCell( cell );
    }

    private static bool IsTemplatedCell( Cell cell )
    {
      return ( cell != null )
          && Row.GetIsTemplateCell( cell );
    }

    private static bool IsFreeCell( Cell cell )
    {
      if( cell == null )
        throw new ArgumentNullException( "cell" );

      return string.IsNullOrEmpty( cell.FieldName );
    }

    #region Private Fields

    private readonly Row m_parentRow;
    private readonly ICollection<Cell> m_cells;

    // This Dictionary is used as a caching system to optimize lookup when virtualizing instead of
    // parsing every columns looking for the fieldname. e.g.: looking for the last one when 1000
    // Cells in the parent row. When virtualizing, only the cell visible in the viewport will be contained
    // within this collection speeding the lookup.
    private readonly Dictionary<string, Cell> m_bindedCells = new Dictionary<string, Cell>();

    // This Dictionary contains the recycling queues for the cells ready to be recycled.
    private Dictionary<object, LinkedList<Cell>> m_recyclingBins; //null

    // This collection is used to store cells that are added to the Collection
    // before the FieldName and/or ParentColumn being set. This occurs when
    // defining a Row with Cell in XAML during a BeginInit / EndInit.
    private List<Cell> m_freeCells; //null

    #endregion

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

    #endregion

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
          Cell cell = null;
          ColumnCollection columns = this.Columns;

          if( columns != null )
          {
            ColumnBase column = columns[ index ];

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
        ColumnCollection columns = this.Columns;

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
        if( VirtualizingCellCollection.IsTemplatedCell( targetCell ) )
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
