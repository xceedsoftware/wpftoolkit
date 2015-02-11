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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid.Views
{
  public class FixedCellPanel : Panel, IVirtualizingCellsHost, IWeakEventListener
  {
    private static readonly Point EmptyPoint = new Point();

    public FixedCellPanel()
    {
      // None of the Visual children of the FixedCellPanel will scroll. The needed scrolling as demanded by the parent ScrollViewer
      // will be manually transferred to the  content (m_scrollingPanel) of the ScrollingCellsDecorator below.

      m_fixedPanel = new FixedCellSubPanel( this );

      m_splitter = new FixedColumnSplitter();

      m_scrollingPanel = new VirtualizingFixedCellSubPanel( this );

      m_scrollingCellsDecorator = new ScrollingCellsDecorator();
      m_scrollingCellsDecorator.Child = m_scrollingPanel;

      this.AddVisualChild( m_fixedPanel );
      this.AddVisualChild( m_splitter );
      this.AddVisualChild( m_scrollingCellsDecorator );

      // Used to get the correct item depending of the ZIndex
      m_visualChildren.Add( m_fixedPanel );
      m_visualChildren.Add( m_splitter );
      m_visualChildren.Add( m_scrollingCellsDecorator );

    }

    #region ColumnStretchMinWidth Property

    public static readonly DependencyProperty ColumnStretchMinWidthProperty = DependencyProperty.Register(
      "ColumnStretchMinWidth",
      typeof( double ),
      typeof( FixedCellPanel ),
      new UIPropertyMetadata( 50d,
        new PropertyChangedCallback( FixedCellPanel.ColumnStretchMinWidthChanged ) ),
        new ValidateValueCallback( TableView.ValidateColumnStretchMinWidthCallback ) );

    public double ColumnStretchMinWidth
    {
      get
      {
        return ( double )this.GetValue( FixedCellPanel.ColumnStretchMinWidthProperty );
      }
      set
      {
        this.SetValue( FixedCellPanel.ColumnStretchMinWidthProperty, value );
      }
    }

    private static void ColumnStretchMinWidthChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var panel = sender as FixedCellPanel;
      if( panel == null )
        return;

      //Do not process merged headers.  Their width is determined by their child columns.
      if( panel.ParentRow.LevelCache != -1 )
        return;

      var dataGridContext = DataGridControl.GetDataGridContext( panel );
      if( dataGridContext == null )
        return;

      if( ( dataGridContext.VisibleColumns.Count > 0 ) && !dataGridContext.IsAFlattenDetail )
      {
        // When ColumnStretchMinWidth changes trigger an update by clearing all DesiredWidth values.
        foreach( ColumnBase column in dataGridContext.VisibleColumns )
        {
          column.ClearValue( Column.DesiredWidthProperty );
        }
      }
    }

    #endregion ColumnStretchMinWidth Property

    #region ColumnStretchMode Property

    public static readonly DependencyProperty ColumnStretchModeProperty = DependencyProperty.Register(
      "ColumnStretchMode",
      typeof( ColumnStretchMode ),
      typeof( FixedCellPanel ),
      new UIPropertyMetadata( ColumnStretchMode.None ) );

    public ColumnStretchMode ColumnStretchMode
    {
      get
      {
        return ( ColumnStretchMode )this.GetValue( FixedCellPanel.ColumnStretchModeProperty );
      }
      set
      {
        this.SetValue( FixedCellPanel.ColumnStretchModeProperty, value );
      }
    }

    #endregion ColumnStretchMode Property

    #region DataGridContext Property

    // DataGridContext is set when IVirtualizingCellsHost.PrepareCellsHost is called and cleared when IVirtualizingCellsHost.ClearContainer is called.
    // We always use the DataGridContext passed and assert the old one is never referenced
    internal DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    #endregion

    #region Columns Internal Property

    internal ColumnCollection Columns
    {
      get
      {
        return m_dataGridContext.Columns;
      }
    }

    #endregion

    #region ColumnsByVisiblePosition Property

    internal HashedLinkedList<ColumnBase> ColumnsByVisiblePosition
    {
      get
      {
        return m_dataGridContext.ColumnsByVisiblePosition;
      }
    }

    #endregion

    #region FixedCellCount Property

    public static readonly DependencyProperty FixedCellCountProperty = DependencyProperty.Register(
      "FixedCellCount",
      typeof( int ),
      typeof( FixedCellPanel ),
      new UIPropertyMetadata(
        0,
        new PropertyChangedCallback( FixedCellPanel.FixedCellCountChanged ) ),
        new ValidateValueCallback( FixedCellPanel.ValidateFixedCellCountCallback ) );

    public int FixedCellCount
    {
      get
      {
        return ( int )this.GetValue( FixedCellPanel.FixedCellCountProperty );
      }
      set
      {
        this.SetValue( FixedCellPanel.FixedCellCountProperty, value );
      }
    }

    private static void FixedCellCountChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      FixedCellPanel panel = sender as FixedCellPanel;
      if( panel == null )
        return;

      // Invalidate the measure to force a layout pass
      panel.InvalidateMeasure();
    }

    private static bool ValidateFixedCellCountCallback( object value )
    {
      return ( ( int )value >= 0 );
    }

    #endregion FixedCellCount Property

    #region FixedPanel Property

    internal Panel FixedPanel
    {
      get
      {
        return m_fixedPanel;
      }
    }

    #endregion

    #region FixedColumnDropMarkPen Property

    public static readonly DependencyProperty FixedColumnDropMarkPenProperty = DependencyProperty.Register(
      "FixedColumnDropMarkPen",
      typeof( Pen ),
      typeof( FixedCellPanel ),
      new PropertyMetadata( null ) );

    public Pen FixedColumnDropMarkPen
    {
      get
      {
        return ( Pen )this.GetValue( FixedCellPanel.FixedColumnDropMarkPenProperty );
      }
      set
      {
        this.SetValue( FixedCellPanel.FixedColumnDropMarkPenProperty, value );
      }
    }

    #endregion FixedColumnDropMarkPen Property

    #region ParentRow Property

    internal Row ParentRow
    {
      get
      {
        if( m_parentRow == null )
        {
          m_parentRow = Row.FindFromChild( this.DataGridContext, this );
        }

        return m_parentRow;
      }
    }

    #endregion

    #region ParentRowCells Property

    internal VirtualizingCellCollection ParentRowCells
    {
      get
      {
        if( m_parentRowCells == null )
        {
          Row parentRow = this.ParentRow;

          if( parentRow == null )
            return null;

          m_parentRowCells = ( VirtualizingCellCollection )parentRow.Cells;

          Debug.Assert( m_parentRowCells != null );
        }

        return m_parentRowCells;
      }
    }

    #endregion

    #region ParentScrollViewer Property

    internal ScrollViewer ParentScrollViewer
    {
      get
      {
        if( m_parentScrollViewer == null )
        {
          m_parentScrollViewer = TableViewScrollViewer.GetParentScrollViewer( this );
          Debug.Assert( m_parentScrollViewer != null );
        }

        return m_parentScrollViewer;
      }
    }

    #endregion

    #region ScrollingCellsDecorator Property

    internal Decorator ScrollingCellsDecorator
    {
      get
      {
        return m_scrollingCellsDecorator;
      }
    }

    #endregion

    #region ColumnVirtualizationManager Internal Property

    internal TableViewColumnVirtualizationManagerBase ColumnVirtualizationManager
    {
      get
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext == null )
          return null;

        var columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManagerBase;
        if( columnVirtualizationManager == null )
          throw new DataGridInternalException( "Invalid ColumnVirtualizationManager for FixedCellPanel", dataGridContext.DataGridControl );

        return columnVirtualizationManager;
      }
    }

    #endregion

    #region CellsHostPrepared Private Property

    private bool CellsHostPrepared
    {
      get
      {
        return m_flags[ ( int )FixedCellPanelFlags.CellsHostPrepared ];
      }
      set
      {
        m_flags[ ( int )FixedCellPanelFlags.CellsHostPrepared ] = value;
      }
    }

    #endregion

    #region ZOrderDirty Private Property

    private bool ZOrderDirty
    {
      get
      {
        return m_flags[ ( int )FixedCellPanelFlags.ZOrderDirty ];
      }
      set
      {
        m_flags[ ( int )FixedCellPanelFlags.ZOrderDirty ] = value;
      }
    }

    #endregion

    #region SplitterDragCanceled Private Property

    private bool SplitterDragCanceled
    {
      get
      {
        return m_flags[ ( int )FixedCellPanelFlags.SplitterDragCanceled ];
      }
      set
      {
        m_flags[ ( int )FixedCellPanelFlags.SplitterDragCanceled ] = value;
      }
    }

    #endregion

    #region SplitterDragCompleted Private Property

    private bool SplitterDragCompleted
    {
      get
      {
        return m_flags[ ( int )FixedCellPanelFlags.SplitterDragCompleted ];
      }
      set
      {
        m_flags[ ( int )FixedCellPanelFlags.SplitterDragCompleted ] = value;
      }
    }

    #endregion

    #region VisualChildrenCount Property

    protected override int VisualChildrenCount
    {
      get
      {
        return s_visualChildrenCount;
      }
    }

    #endregion

    #region LogicalChildren Property

    protected override IEnumerator LogicalChildren
    {
      get
      {
        return this.Children.GetEnumerator();
      }
    }

    #endregion

    #region CellsInView Private Property

    private IEnumerable<Cell> CellsInView
    {
      get
      {
        VirtualizingCellCollection virtualizingCellCollection = this.ParentRowCells;
        if( virtualizingCellCollection == null )
          return new Cell[ 0 ];

        TableViewColumnVirtualizationManagerBase columnVirtualizationManager = this.ColumnVirtualizationManager;
        if( columnVirtualizationManager == null )
          return new Cell[ 0 ];

        List<Cell> cells = new List<Cell>();
        Cell cell;

        foreach( string fieldName in columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache ) )
        {
          if( virtualizingCellCollection.TryGetBindedCell( fieldName, out cell ) )
          {
            cells.Add( cell );
          }
        }

        foreach( string fieldName in columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ) )
        {
          if( virtualizingCellCollection.TryGetBindedCell( fieldName, out cell ) )
          {
            cells.Add( cell );
          }
        }

        return cells;
      }
    }

    #endregion

    #region PermanentScrollingFieldNames Internal Property

    internal IEnumerable<string> PermanentScrollingFieldNames
    {
      get
      {
        return m_permanentScrollingFieldNames;
      }
    }

    private void AddPermanentScrollingFieldNames( string fieldName )
    {
      if( string.IsNullOrEmpty( fieldName ) || m_permanentScrollingFieldNames.Contains( fieldName ) )
        return;

      m_permanentScrollingFieldNames.Add( fieldName );
    }

    private bool AddPermanentScrollingFieldNamesIfOutOfViewColumn( string fieldName )
    {
      if( string.IsNullOrEmpty( fieldName ) || m_permanentScrollingFieldNames.Contains( fieldName ) )
        return false;

      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( columnVirtualizationManager != null )
      {
        if( columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache ).Contains( fieldName )
          || columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ).Contains( fieldName ) )
          return false;
      }

      m_permanentScrollingFieldNames.Add( fieldName );

      return true;
    }

    private void ClearPermanentScrollingFieldNames()
    {
      m_permanentScrollingFieldNames.Clear();
      m_permanentScrollingFieldNames.TrimExcess();
    }

    private readonly List<string> m_permanentScrollingFieldNames = new List<string>( 0 );

    #endregion

    protected override Visual GetVisualChild( int index )
    {
      if( this.ZOrderDirty )
      {
        this.RecomputeZOrder();
      }

      int correctedIndex = ( m_zIndexMapping == null ) ? index : m_zIndexMapping[ index ];

      switch( correctedIndex )
      {
        case 0:
          return m_fixedPanel;
        case 1:
          return m_splitter;
        case 2:
          return m_scrollingCellsDecorator;
        default:
          DataGridException.ThrowSystemException( "Invalid visual child index.", typeof( IndexOutOfRangeException ), this.DataGridContext.DataGridControl.Name );
          //Simply there to remove compiling error
          return null;
      }
    }

    protected override UIElementCollection CreateUIElementCollection( FrameworkElement logicalParent )
    {
      return new VirtualizingUICellCollection( m_fixedPanel, m_scrollingPanel, this );
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      DataGridContext dataGridContext = this.DataGridContext;

      // DataGridContext can be null when the parent Row is not prepared
      if( dataGridContext == null )
        return this.DesiredSize;

      this.ApplyFixedTranform();

      // If a change occured and did not triggered an UpdateMeasureRequiredEvent by the columnVirtualizationManager, we ensure the Row contains the necessary Cells
      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( m_lastColumnVirtualizationManagerVersion != columnVirtualizationManager.Version )
      {
        this.UpdateChildren();
      }

      double tempDesiredHeight = 0;
      m_desiredSize.Width = 0;

      // Measure the Splitter
      m_splitter.Measure( availableSize );

      // Measure the Panels (visual children) around the cells (logical children).  Logical children will not be measured again during this process.
      m_fixedPanel.Measure( availableSize );

      if( m_fixedPanel.DesiredSize.Height > tempDesiredHeight )
      {
        tempDesiredHeight = m_fixedPanel.DesiredSize.Height;
      }

      // Set the desired size for m_scrollingPanel which will be passed to the Decorated that will allow the first cell to be clipped
      m_desiredSize.Width += columnVirtualizationManager.VisibleColumnsWidth;

      m_scrollingCellsDecorator.Measure( availableSize );

      if( m_scrollingCellsDecorator.DesiredSize.Height > tempDesiredHeight )
      {
        tempDesiredHeight = m_scrollingCellsDecorator.DesiredSize.Height;
      }

      m_desiredSize.Height = tempDesiredHeight;

      if( !double.IsPositiveInfinity( availableSize.Width ) && !dataGridContext.IsAFlattenDetail )
      {
        dataGridContext.ColumnStretchingManager.CalculateColumnStretchWidths( availableSize.Width, this.ColumnStretchMode, this.ColumnStretchMinWidth );
      }

      // Adjust TabIndex of fixed cell and scrolling cell for the tab navigation to be ok
      this.AdjustCellTabIndex( columnVirtualizationManager );

      return m_desiredSize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      // DataGridContext can be null when the parent Row is not prepared
      if( this.DataGridContext == null )
        return this.DesiredSize;

      // Very simple horizontally Arrange implementation.  We use m_visualChildren instead of GetVisualChild(index)
      // to ensure the sub panels are always arranged in the  correct visible order
      int childrenCount = m_visualChildren.Count;
      Rect finalRect = new Rect( finalSize );
      double offset = 0d;

      UIElement element;

      for( int i = 0; i < childrenCount; i++ )
      {
        element = m_visualChildren[ i ];
        if( element != null )
        {
          finalRect.X = offset;
          finalRect.Width = element.DesiredSize.Width;
          finalRect.Height = Math.Max( finalSize.Height, element.DesiredSize.Height );
          element.Arrange( finalRect );

          offset += finalRect.Width;
        }
      }

      return finalSize;
    }

    // Returns the visible index of a column by its visible position
    internal static int CalculateVisibleIndex( int visiblePosition, HashedLinkedList<ColumnBase> columnsByVisiblePosition )
    {
      if( columnsByVisiblePosition == null )
        return -1;

      int visibleIndex = 0;
      LinkedListNode<ColumnBase> tempNode = columnsByVisiblePosition.First;

      for( int i = 0; i < visiblePosition; i++ )
      {
        if( tempNode == null )
          break;

        if( tempNode.Value.Visible )
        {
          visibleIndex++;
        }

        tempNode = tempNode.Next;
      }

      return visibleIndex;
    }

    internal double GetFixedWidth()
    {
      var offset = VisualTreeHelper.GetOffset( m_scrollingCellsDecorator ).X;
      var parent = VisualTreeHelper.GetParent( this );

      while( parent != null )
      {
        if( parent is Row )
        {
          offset = this.TransformToAncestor( parent as Visual ).Transform( new Point( offset, 0 ) ).X;
          break;
        }

        parent = VisualTreeHelper.GetParent( parent );
      }

      return offset;
    }

    internal ScrollViewer GetParentScrollViewer()
    {
      if( m_parentScrollViewer == null )
      {
        m_parentScrollViewer = TableViewScrollViewer.GetParentScrollViewer( this );
      }

      return m_parentScrollViewer;
    }

    internal bool ForceScrollingCellToLayout( Cell cell )
    {
      bool retVal = false;

      if( cell == null )
        return retVal;

      // We do not want any template cells to be added/removed to/from visual tree
      Debug.Assert( !Row.GetIsTemplateCell( cell ), "We should never insert template cells" );

      ColumnBase parentColumn = cell.ParentColumn;
      Debug.Assert( parentColumn != null, "The cell doesn't have a parent column" );

      CellCollection cellCollection = this.ParentRowCells;
      Debug.Assert( cellCollection != null );

      Cell targetCell = cellCollection[ parentColumn ];
      Debug.Assert( targetCell == cell, "The forced cell is inappropriate" );

      retVal = this.MoveCellToScrollingPanel( cell );

      // Keep the field name in order to inform the scrolling sub-panel to measure and arrange the target cell.
      retVal = this.AddPermanentScrollingFieldNamesIfOutOfViewColumn( parentColumn.FieldName ) || retVal;

      return retVal;
    }

    private bool CanReleaseCell( Cell cell )
    {
      DataGridContext dataGridContext = this.DataGridContext;


      //We must never recycle a cell for one of the following reason:
      //  1. Its edit template is currently displayed.  (The typed content must be kept).
      //  2. Is the current cell.
      //  3. Is a templated cell.
      //  4. Is part of a dragged column.
      return ( cell != null )
          && ( dataGridContext != null )
          && ( cell.CanBeRecycled )
          && ( cell != dataGridContext.CurrentCell )
          && ( !Row.GetIsTemplateCell( cell ) )
          && ( ( cell.ParentColumn == null ) || ( !TableflowView.GetIsBeingDraggedAnimated( cell.ParentColumn ) ) )
          && ( !( bool )cell.GetValue( ColumnManagerCell.IsBeingDraggedProperty ) );
    }

    private void AddCellToVisualTree( Cell cellToAdd )
    {
      if( cellToAdd == null )
        return;

      UIElementCollection scrollingChildren = m_scrollingPanel.Children;

      if( scrollingChildren.Contains( cellToAdd ) )
        return;

      UIElementCollection fixedChildren = m_fixedPanel.Children;

      if( m_fixedPanel.Children.Contains( cellToAdd ) )
        return;

      scrollingChildren.Add( cellToAdd );
    }

    private void MoveCellToFixedPanel( Cell movingCell )
    {
      if( movingCell == null )
        return;

      UIElementCollection origin = m_scrollingPanel.Children;
      UIElementCollection destination = m_fixedPanel.Children;

      //The VisualTree is altered only if the cells isn't in the appropriate location.
      if( !destination.Contains( movingCell ) )
      {
        //Transfer the cell from one panel to the other.
        origin.Remove( movingCell );
        destination.Add( movingCell );
      }
    }

    private bool MoveCellToScrollingPanel( Cell movingCell )
    {
      bool retVal = false;

      if( movingCell == null )
        return retVal;

      UIElementCollection origin = m_fixedPanel.Children;
      UIElementCollection destination = m_scrollingPanel.Children;

      //The VisualTree is altered only if the cells isn't in the appropriate location.
      if( !destination.Contains( movingCell ) )
      {
        //Transfer the cell from one panel to the other.
        origin.Remove( movingCell );
        destination.Add( movingCell );
        retVal = true;
      }

      return retVal;
    }

    private void RemoveLeakingCells( ICollection<Cell> collection )
    {
      this.RemoveLeakingCells( collection, m_fixedPanel.Children );
      this.RemoveLeakingCells( collection, m_scrollingPanel.Children );
    }

    private void RemoveLeakingCells( ICollection<Cell> aliveCells, UIElementCollection collection )
    {
      if( ( aliveCells == null ) || ( collection == null ) )
        return;

      //Remove the inaccessible cells from the elements collection.
      for( int i = collection.Count - 1; i >= 0; i-- )
      {
        Cell cell = collection[ i ] as Cell;

        if( ( cell != null ) && ( !aliveCells.Contains( cell ) ) )
        {
          collection.RemoveAt( i );
        }
      }
    }

    private int GetCurrentLevelDropPoint( ColumnBase parentColumn, bool isPassedHalfWidth )
    {
      string dummystring;
      return this.GetCurrentLevelDropPoint( parentColumn, isPassedHalfWidth, out dummystring );
    }

    private int GetCurrentLevelDropPoint( ColumnBase parentColumn, bool isPassedHalfWidth, out string offsetCellFieldName )
    {
      offsetCellFieldName = parentColumn.FieldName;

      if( isPassedHalfWidth )
        return parentColumn.VisiblePosition + 1;

      return parentColumn.VisiblePosition;
    }

    private void UpdateChildren()
    {
      this.UpdateChildren( UpdateMeasureTriggeredAction.Unspecified );
    }

    private void UpdateChildren( UpdateMeasureTriggeredAction action )
    {
      Row parentRow = this.ParentRow;

      // A CurrentItemChanged was received for a non current Row
      if( ( action == UpdateMeasureTriggeredAction.CurrentItemChanged ) && ( parentRow != null ) && ( !parentRow.IsCurrent ) )
        return;

      DataGridContext dataGridContext = this.DataGridContext;

      Debug.Assert( dataGridContext != null, "Should not be called when a container is not prepared" );

      // If the parent Row is not prepared, do not update children
      if( dataGridContext == null )
        return;

      bool currentItemChangedOnCurrentRow = ( action == UpdateMeasureTriggeredAction.CurrentItemChanged ) && ( parentRow != null ) && ( parentRow.IsCurrent );
      var columnVirtualizationManager = this.ColumnVirtualizationManager;

      if( ( m_lastColumnVirtualizationManagerVersion != columnVirtualizationManager.Version ) || ( currentItemChangedOnCurrentRow )
            || ( m_virtualizingCellCollectionChangedDispatcherOperation != null ) )
      {
        this.UpdateChildren( dataGridContext, columnVirtualizationManager, this.ParentRowCells );

        m_lastColumnVirtualizationManagerVersion = columnVirtualizationManager.Version;

        this.InvalidateMeasure();

        m_virtualizingCellCollectionChangedDispatcherOperation = null;
      }
    }

    private void UpdateChildren( DataGridContext dataGridContext, TableViewColumnVirtualizationManagerBase columnVirtualizationManager, VirtualizingCellCollection parentRowCells )
    {
      //Prevent reentrance
      if( ( parentRowCells == null ) || m_parentRowCells.IsUpdating )
        return;

      using( m_parentRowCells.SetIsUpdating() )
      {
        this.ClearPermanentScrollingFieldNames();

        //Retrieve the cells that aren't needed anymore.
        var unusedCells = ( from cell in parentRowCells.BindedCells
                            where !columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ).Contains( cell.FieldName )
                               && !columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache ).Contains( cell.FieldName )
                            select cell ).ToList();

        //Release the unused binded cells now in order to minimize the number of cell's creation.
        foreach( Cell cell in unusedCells )
        {
          this.AddCellToVisualTree( cell );

          if( this.CanReleaseCell( cell ) )
          {
            // Ensure to close the ContextMenu if it is open and the Cell is virtualized to avoid problems with ContextMenus defined as static resources
            // that won't be able to reopen again if the Close private method is called after the PlacementTarget is removed from the VisualTree
            var contextMenu = cell.ContextMenu;
            if( ( contextMenu != null ) && ( contextMenu.IsOpen ) )
            {
              contextMenu.IsOpen = false;
            }

            cell.ClearContainer();
            parentRowCells.Release( cell );
          }
          //Since the cell cannot be released, it will not be collapsed. We must keep the field
          //name in order to let the scrolling sub-panel measure and arrange the cell out of view.
          else
          {
            //Certain non recyclable cells like StatCells needs their content binding to be removed when they become out of view.
            cell.RemoveContentBinding();
            this.AddPermanentScrollingFieldNames( cell.FieldName );
          }
        }

        //Add the missing cells to the fixed region.
        foreach( string fieldName in columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache ) )
        {
          //The cell is created if it is missing.
          Cell cell = parentRowCells[ fieldName ];

          //Certain non recyclable cells like StatCells need their content binding to be updated when they become (or stay) in view.
          cell.AddContentBinding( dataGridContext, this.ParentRow, this.Columns[ cell.FieldName ] );

          //Make sure the cell is in the appropriate panel.
          this.MoveCellToFixedPanel( cell );
        }

        //Add the missing cells to the scrolling region.
        foreach( string fieldName in columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ) )
        {
          //The cell is created if it is missing.
          Cell cell = parentRowCells[ fieldName ];

          //Certain non recyclable cells like StatCells need their content binding to be updated when they become (or stay) in view.
          cell.AddContentBinding( dataGridContext, this.ParentRow, this.Columns[ cell.FieldName ] );

          //Make sure the cell is in the appropriate panel.
          this.MoveCellToScrollingPanel( cell );
        }

        if( m_clearUnusedCellsDispatcherOperation == null )
        {
          m_clearUnusedCellsDispatcherOperation = this.Dispatcher.BeginInvoke( new Action( this.ClearUnusedCells ), DispatcherPriority.ApplicationIdle );
        }

        m_fixedPanel.InvalidateMeasure();
        m_scrollingCellsDecorator.InvalidateMeasure();
        m_scrollingPanel.InvalidateMeasure();
      }
    }

    private void ApplyFixedTranform()
    {
      ScrollViewer parentScrollViewer = this.GetParentScrollViewer();

      if( parentScrollViewer == null )
        return;

      Transform fixedTranslation = TableViewScrollViewer.GetStoredFixedTransform( parentScrollViewer );

      if( m_fixedPanel.RenderTransform != fixedTranslation )
      {
        m_fixedPanel.RenderTransform = fixedTranslation;
      }

      if( m_scrollingCellsDecorator.RenderTransform != fixedTranslation )
      {
        m_scrollingCellsDecorator.RenderTransform = fixedTranslation;
      }
    }

    private void AdjustCellTabIndex( TableViewColumnVirtualizationManagerBase columnVirtualizationManager )
    {
      // The order of the children in the panel is not guaranteed to be good.  We do not reorder children because we will have to remove
      // and add them at a different location.  Doing that will cause more calculations to occur, so we only change the TabIndex.
      VirtualizingCellCollection collection = this.ParentRowCells;
      if( collection == null )
        return;

      var fieldNameToPositionMapping = columnVirtualizationManager.GetFieldNameToPosition( m_parentRow.LevelCache );
      var fieldNames = columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache )
                         .Concat( columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ) )
                         .Concat( m_permanentScrollingFieldNames );

      foreach( var fieldName in fieldNames )
      {
        Cell cell = collection.GetCell( fieldName, false );
        Debug.Assert( cell != null );

        int tabIndex;
        if( fieldNameToPositionMapping.TryGetValue( fieldName, out tabIndex ) )
        {
          KeyboardNavigation.SetTabIndex( cell, tabIndex );
        }
        else
        {
          cell.ClearValue( Cell.TabIndexProperty );
        }
      }
    }

    private void ClearUnusedCells()
    {
      if( ( m_clearUnusedCellsDispatcherOperation == null ) || ( m_clearUnusedCellsDispatcherOperation.Status == DispatcherOperationStatus.Aborted ) )
        return;

      m_clearUnusedCellsDispatcherOperation = null;

      DataGridContext dataGridContext = this.DataGridContext;
      if( dataGridContext == null )
        return;

      VirtualizingCellCollection parentRowCells = this.ParentRowCells;
      if( parentRowCells == null )
        return;

      //Remove the recycling bins that aren't used anymore.
      parentRowCells.ClearUnusedRecycleBins( this.Columns );

      //Remove the inaccessible cells from the panels.
      this.RemoveLeakingCells( parentRowCells.Cells );
    }

    private FrameworkElement GetPreparedContainer( Cell cell )
    {
      if( cell == null )
        return null;

      var parentRow = cell.ParentRow;
      if( parentRow == null )
        return null;

      // Use the DataGridControl.Container attached property to assert a container is always returned even if the container is in FixedHeaders or FixedFooters.
      var container = DataGridControl.GetContainer( parentRow );
      if( container == null )
        return null;

      if( !DataGridControl.GetIsContainerPrepared( container ) )
        return null;

      return container;
    }

    #region ZOrder Management

    private void RecomputeZOrder()
    {
      ZOrderHelper.ComputeZOrder( m_visualChildren, ref m_zIndexMapping );
      this.ZOrderDirty = false;
    }

    private void InvalidateZOrder()
    {
      this.ZOrderDirty = true;
    }

    internal bool ChangeCellZOrder( Cell cell, int zIndex )
    {
      bool zOrderChanged = false;

      UIElement panelToChangeZOrder = null;

      // Ensure the ZOrder is also considered by the panel containing the Cell
      if( m_fixedPanel.Children.Contains( cell ) )
      {
        panelToChangeZOrder = m_fixedPanel;
      }
      else
      {
        panelToChangeZOrder = m_scrollingCellsDecorator;
      }

      if( object.Equals( zIndex, DependencyProperty.UnsetValue ) )
      {
        if( panelToChangeZOrder.ReadLocalValue( Panel.ZIndexProperty ) != DependencyProperty.UnsetValue )
        {
          panelToChangeZOrder.ClearValue( Panel.ZIndexProperty );
          this.InvalidateZOrder();

          zOrderChanged = true;
        }
      }
      else
      {
        if( !object.Equals( panelToChangeZOrder.GetValue( Panel.ZIndexProperty ), zIndex ) )
        {
          panelToChangeZOrder.SetValue( Panel.ZIndexProperty, zIndex );
          this.InvalidateZOrder();

          zOrderChanged = true;
        }
      }

      Panel.SetZIndex( cell, zIndex );

      return zOrderChanged;
    }

    internal void ClearCellZOrder( Cell cell )
    {
      m_fixedPanel.ClearValue( Panel.ZIndexProperty );
      m_scrollingCellsDecorator.ClearValue( Panel.ZIndexProperty );
      cell.ClearValue( Panel.ZIndexProperty );
      this.InvalidateZOrder();
    }

    #endregion ZOrder Management

    #region IVirtualizingCellsHost Members

    bool IVirtualizingCellsHost.CanModifyLogicalParent
    {
      get
      {
        return true;
      }
    }

    void IVirtualizingCellsHost.SetLogicalParent( Cell cell )
    {
      Debug.Assert( cell != null );

      VirtualizingUICellCollection cellCollection = this.Children as VirtualizingUICellCollection;

      Debug.Assert( cellCollection != null );

      cellCollection.SetCellLogicalParent( cell );

      this.MoveCellToScrollingPanel( cell );
    }

    void IVirtualizingCellsHost.PrepareCellsHost( DataGridContext dataGridContext )
    {
      // This method is called in Row.PrepareContainer and Row.OnApplyTemplate, which is also called in Row.PrepareContainer but only if the template
      // was not previously applied, so it is possible that this panel is already prepared. In that case, we only ignore this preparation.
      if( this.CellsHostPrepared )
        return;

      if( dataGridContext != null )
      {
        m_dataGridContext = dataGridContext;

        object dataItem = this.ParentRow.DataContext;

        foreach( Cell cell in this.CellsInView )
        {
          cell.PrepareContainer( dataGridContext, dataItem );
        }

        TableViewColumnVirtualizationManagerBase columnVirtualizationManager = this.ColumnVirtualizationManager;
        UpdateMeasureRequiredEventManager.AddListener( columnVirtualizationManager, this );
        m_previousColumnVirualizationManager = columnVirtualizationManager;

        VirtualizingCellCollection parentRowCells = this.ParentRowCells;
        if( parentRowCells != null )
        {
          VirtualizingCellCollectionChangedEventManager.AddListener( parentRowCells, this );
        }

        this.CellsHostPrepared = true;
      }
    }

    void IVirtualizingCellsHost.ClearCellsHost()
    {
      if( !this.CellsHostPrepared )
        return;

      VirtualizingCellCollection parentRowCells = this.ParentRowCells;
      if( parentRowCells != null )
      {
        VirtualizingCellCollectionChangedEventManager.RemoveListener( parentRowCells, this );
      }

      //Make sure to stop listening from the same manager to which it started to listen.
      UpdateMeasureRequiredEventManager.RemoveListener( m_previousColumnVirualizationManager, this );
      m_previousColumnVirualizationManager = null;

      m_dataGridContext = null;

      this.CellsHostPrepared = false;
    }

    void IVirtualizingCellsHost.InvalidateCellsHostMeasure()
    {
      if( this.DataGridContext == null )
        return;

      this.InvalidateMeasure();
      m_fixedPanel.InvalidateMeasure();
      m_scrollingCellsDecorator.InvalidateMeasure();
      m_scrollingPanel.InvalidateMeasure();
    }

    bool IVirtualizingCellsHost.BringIntoView( Cell cell, RequestBringIntoViewEventArgs e )
    {
      // This cell doesn't seem to be in the grid.  We return true to handle the event that triggered the BringIntoView.
      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return true;

      // If the container is not in use, we return true to handle the event that triggered the BringIntoView.
      var container = this.GetPreparedContainer( cell );
      if( container == null )
        return true;

      // The ScrollViewer can handle the bring into view if the grid isn't virtualized.
      var scrollViewer = this.ParentScrollViewer;
      if( ( scrollViewer == null ) || !scrollViewer.CanContentScroll )
        return false;

      // This cell seems to be dead.  We return true to handle the event that triggered the BringIntoView.
      var row = cell.ParentRow;
      var column = cell.ParentColumn;
      if( ( row == null ) || ( column == null ) )
        return true;

      var columnVirtualizationManager = this.ColumnVirtualizationManager;

      // Fixed cells are always visible.
      if( columnVirtualizationManager.GetFixedFieldNames( row.LevelCache ).Contains( column ) )
        return this.BringIntoViewFixedCell( container );

      return this.BringIntoViewScrollingCell( cell, container, e );
    }

    private bool BringIntoViewFixedCell( FrameworkElement container )
    {
      Debug.Assert( container != null );

      // We always want the horizontal offset to change if Tab, Home, Left or Right is pressed.
      if( Keyboard.IsKeyDown( Key.Tab ) || Keyboard.IsKeyDown( Key.Home ) || Keyboard.IsKeyDown( Key.Left ) || Keyboard.IsKeyDown( Key.Right ) )
      {
        container.BringIntoView( new Rect( 0d, 0d, 0d, 0d ) );
      }
      else
      {
        // Ensure to bring the container into view vertically.
        container.BringIntoView();
      }

      return true;
    }

    private bool BringIntoViewScrollingCell( Cell cell, FrameworkElement container, RequestBringIntoViewEventArgs e )
    {
      Debug.Assert( cell != null );
      Debug.Assert( container != null );

      // The cell must be measured/arranged for the BringIntoView to correctly reacts
      if( this.ForceScrollingCellToLayout( cell ) )
      {
        container.UpdateLayout();
      }

      var fixedWidth = this.GetFixedWidth();
      var scrollingPanel = this.ScrollingCellsDecorator;
      var scrollingArea = new Rect( scrollingPanel.TranslatePoint( FixedCellPanel.EmptyPoint, this ),
                                    new Size( this.ParentScrollViewer.ViewportWidth - fixedWidth, scrollingPanel.ActualHeight ) );
      var cellArea = new Rect( cell.TranslatePoint( FixedCellPanel.EmptyPoint, this ),
                               new Size( cell.ParentColumn.ActualWidth, cell.ActualHeight ) );

      // The cell is larger than the scrolling area.
      if( cellArea.Width > scrollingArea.Width )
      {
        var targetObject = e.TargetObject as UIElement;
        var targetRect = e.TargetRect;

        // Try to narrow the area within the cell that we clearly want to bring into view.
        if( ( targetObject != null ) && ( targetObject != cell || !targetRect.IsEmpty ) )
        {
          Debug.Assert( targetObject.IsDescendantOf( cell ) );

          if( targetRect.IsEmpty )
          {
            targetRect = new Rect( new Point( 0d, 0d ), targetObject.RenderSize );
          }

          if( targetRect.Width <= scrollingArea.Width )
          {
            var offset = targetObject.TranslatePoint( FixedCellPanel.EmptyPoint, cell );
            var location = cellArea.Location;
            location.Offset( offset.X, offset.Y );

            cellArea.Location = location;
            cellArea.Size = targetRect.Size;
          }
        }
      }

      if( ( cellArea.Left <= scrollingArea.Left ) && ( cellArea.Right >= scrollingArea.Right ) )
      {
        // Ensure to bring the container into view vertically.
        container.BringIntoView();
      }
      else if( cellArea.Left < scrollingArea.Left )
      {
        // The ScrollViewer's extent width includes the fixed section.  We must offset the target area or the cell
        // will come into view under the fixed panel.
        cellArea.X -= fixedWidth;

        this.BringIntoView( cellArea );
      }
      else if( cellArea.Right > scrollingArea.Right )
      {
        this.BringIntoView( cellArea );
      }
      // The cell is fully visible.
      else
      {
        // Ensure to bring the container into view vertically.
        container.BringIntoView();
      }

      return true;
    }

    #endregion

    #region IWeakEventListener Members

    bool IWeakEventListener.ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      return this.OnReceiveWeakEvent( managerType, sender, e );
    }

    protected virtual bool OnReceiveWeakEvent( Type managerType, object sender, EventArgs e )
    {
      bool handled = false;

      if( managerType == typeof( UpdateMeasureRequiredEventManager ) )
      {
        UpdateMeasureTriggeredAction action = UpdateMeasureTriggeredAction.Unspecified;
        UpdateMeasureRequiredEventArgs eventArgs = e as UpdateMeasureRequiredEventArgs;

        if( eventArgs != null )
        {
          action = eventArgs.TriggeredAction;
        }

        this.UpdateChildren( action );

        handled = true;
      }
      else if( managerType == typeof( VirtualizingCellCollectionChangedEventManager ) )
      {
        //Changes to the Cells collection (i.e. VirtualizingCellCollection) can come directly from the Row.Cells property, without the FixedCellPanel knowing about it.
        //So it needs to be informed if this is the case, since it is the FixedCellPanel responsablility to correctly manage and recycle cells.
        //However, if changes to Cells come from the FixedCellPanel itself, of course there is nothing to do.
        if( !m_parentRowCells.IsUpdating )
        {
          //Update cells as rarely as possible, so performance is not affected by this (for instance when Row.Cells is updated in a loop, do it once at the end).
          if( m_virtualizingCellCollectionChangedDispatcherOperation == null )
          {
            m_virtualizingCellCollectionChangedDispatcherOperation = this.Dispatcher.BeginInvoke( DispatcherPriority.Render, new Action( this.UpdateChildren ) );
          }
        }
        handled = true;
      }

      return handled;
    }

    #endregion

    private int m_lastColumnVirtualizationManagerVersion = -1;
    private BitVector32 m_flags = new BitVector32();
    private Row m_parentRow; // = null;
    private ScrollViewer m_parentScrollViewer; // = null;

    private Size m_desiredSize = new Size();
    private VirtualizingCellCollection m_parentRowCells; // = null;

    // The immediate visual children of this panel.
    private FixedCellSubPanel m_fixedPanel;
    private FixedColumnSplitter m_splitter;
    private ScrollingCellsDecorator m_scrollingCellsDecorator;
    private VirtualizingFixedCellSubPanel m_scrollingPanel;

    private int[] m_zIndexMapping; // = null;

    private const int s_visualChildrenCount = 3;
    private readonly IList<UIElement> m_visualChildren = new List<UIElement>();

    private DataGridContext m_dataGridContext; // = null;
    private TableViewColumnVirtualizationManagerBase m_previousColumnVirualizationManager; // = null;

    private DispatcherOperation m_clearUnusedCellsDispatcherOperation; // = null;
    private object m_virtualizingCellCollectionChangedDispatcherOperation;

    private enum FixedCellPanelFlags
    {
      CellsHostPrepared = 1,
      HasDataItem = 2,
      ZOrderDirty = 4,
      SplitterDragCanceled = 8,
      SplitterDragCompleted = 16,
    }
  }
}
