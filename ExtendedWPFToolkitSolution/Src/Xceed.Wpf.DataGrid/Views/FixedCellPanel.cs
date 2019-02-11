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
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid.Views
{
  public class FixedCellPanel : Panel, IVirtualizingCellsHost, IWeakEventListener
  {
    private static readonly TimeSpan MaxProcessDurationTime = TimeSpan.FromMilliseconds( 25d );

    static FixedCellPanel()
    {
      FixedCellPanel.MinHeightProperty.OverrideMetadata( typeof( FixedCellPanel ), new FrameworkPropertyMetadata( null, new CoerceValueCallback( FixedCellPanel.CoerceMinHeight ) ) );
      TextElement.FontFamilyProperty.OverrideMetadata( typeof( FixedCellPanel ), new FrameworkPropertyMetadata( new PropertyChangedCallback( FixedCellPanel.InvalidateMinHeight ) ) );
      TextElement.FontSizeProperty.OverrideMetadata( typeof( FixedCellPanel ), new FrameworkPropertyMetadata( new PropertyChangedCallback( FixedCellPanel.InvalidateMinHeight ) ) );
      TextElement.FontStretchProperty.OverrideMetadata( typeof( FixedCellPanel ), new FrameworkPropertyMetadata( new PropertyChangedCallback( FixedCellPanel.InvalidateMinHeight ) ) );
      TextElement.FontStyleProperty.OverrideMetadata( typeof( FixedCellPanel ), new FrameworkPropertyMetadata( new PropertyChangedCallback( FixedCellPanel.InvalidateMinHeight ) ) );
      TextElement.FontWeightProperty.OverrideMetadata( typeof( FixedCellPanel ), new FrameworkPropertyMetadata( new PropertyChangedCallback( FixedCellPanel.InvalidateMinHeight ) ) );
    }

    public FixedCellPanel()
    {
      // None of the Visual children of the FixedCellPanel will scroll. The needed scrolling as demanded by the parent ScrollViewer
      // will be manually transferred to the  content (m_scrollingPanel) of the ScrollingCellsDecorator below.

      m_fixedPanel = new FixedCellSubPanel( this );

      m_scrollingPanel = new VirtualizingFixedCellSubPanel( this );

      m_scrollingCellsDecorator = new ScrollingCellsDecorator();
      m_scrollingCellsDecorator.Child = m_scrollingPanel;

      this.AddVisualChild( m_fixedPanel );
      this.AddVisualChild( null );
      this.AddVisualChild( m_scrollingCellsDecorator );

      // Used to get the correct item depending of the ZIndex
      m_visualChildren.Add( m_fixedPanel );
      m_visualChildren.Add( null );
      m_visualChildren.Add( m_scrollingCellsDecorator );

      this.SetCurrentValue( FixedCellPanel.MinHeightProperty, 0d );

      this.LayoutUpdated += new EventHandler( FixedCellPanelLayoutUpdated );
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

    #endregion

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

    #endregion

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

    #region VisibleColumns Property

    private ReadOnlyObservableCollection<ColumnBase> VisibleColumns
    {
      get
      {
        return m_dataGridContext.VisibleColumns;
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

    #endregion

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

    #endregion

    #region ParentRow Property

    internal Row ParentRow
    {
      get
      {
        if( m_parentRow == null )
        {
          m_parentRow = Row.FindFromChild( m_dataGridContext, this );
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
        if( m_dataGridContext == null )
          return null;

        var columnVirtualizationManager = m_dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManagerBase;
        if( columnVirtualizationManager == null )
          throw new DataGridInternalException( "Invalid ColumnVirtualizationManager for FixedCellPanel", m_dataGridContext.DataGridControl );

        return columnVirtualizationManager;
      }
    }

    #endregion

    #region VisualChildrenCount Property

    protected override int VisualChildrenCount
    {
      get
      {
        return c_visualChildrenCount;
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
        var parentRowCells = this.ParentRowCells;
        if( parentRowCells == null )
          return new Cell[ 0 ];

        TableViewColumnVirtualizationManagerBase columnVirtualizationManager = this.ColumnVirtualizationManager;
        if( columnVirtualizationManager == null )
          return new Cell[ 0 ];

        List<Cell> cells = new List<Cell>();
        Cell cell;

        var fieldNames = columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache )
                           .Concat( columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ) );

        foreach( string fieldName in fieldNames )
        {
          if( parentRowCells.TryGetCreatedCell( fieldName, out cell ) )
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
      if( m_zOrderDirty )
      {
        this.RecomputeZOrder();
      }

      var correctedIndex = ( m_zIndexMapping == null ) ? index : m_zIndexMapping[ index ];

      switch( correctedIndex )
      {
        case 0:
          return m_fixedPanel;
        case 1:
          return null;
        case 2:
          return m_scrollingCellsDecorator;
        default:
          throw DataGridException.Create<IndexOutOfRangeException>( "Invalid visual child index.", ( m_dataGridContext != null ? m_dataGridContext.DataGridControl : null ) );
      }
    }

    protected override UIElementCollection CreateUIElementCollection( FrameworkElement logicalParent )
    {
      return new VirtualizingUICellCollection( m_fixedPanel, m_scrollingPanel, this );
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      // DataGridContext can be null when the parent Row is not prepared
      if( m_dataGridContext == null )
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

      if( !double.IsPositiveInfinity( availableSize.Width ) && !m_dataGridContext.IsAFlattenDetail )
      {
        m_dataGridContext.ColumnStretchingManager.CalculateColumnStretchWidths( availableSize.Width, this.ColumnStretchMode,
                                                                                this.ColumnStretchMinWidth, m_dataGridContext.CanIncreaseColumnWidth );

        // Since a panel can make a column change its width, and since this will invalidate every panel that has already been measured, only a decreasing value
        // is allowd to be set from one panel to another, so the layout pass does not fall into an infinite loop of increasing and decreasing values.
        m_dataGridContext.CanIncreaseColumnWidth = false;
      }

      // Adjust TabIndex of fixed cell and scrolling cell for the tab navigation to be ok
      this.AdjustCellTabIndex( columnVirtualizationManager );

      return m_desiredSize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      // DataGridContext can be null when the parent Row is not prepared
      if( m_dataGridContext == null )
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

      // We do not want any template cells to be added to/removed from visual tree
      Debug.Assert( !Row.GetIsTemplateCell( cell ), "We should never insert template cells" );

      ColumnBase parentColumn = cell.ParentColumn;
      Debug.Assert( parentColumn != null, "The cell doesn't have a parent column" );

      CellCollection parentRowCells = this.ParentRowCells;
      Debug.Assert( parentRowCells != null );

      Cell targetCell = parentRowCells[ parentColumn ];
      Debug.Assert( targetCell == cell, "The forced cell is inappropriate" );

      retVal = this.MoveCellToScrollingPanel( cell );

      // Keep the field name in order to inform the scrolling sub-panel to measure and arrange the target cell.
      retVal = this.AddPermanentScrollingFieldNamesIfOutOfViewColumn( parentColumn.FieldName ) || retVal;

      return retVal;
    }

    internal void PrepareVirtualizationMode( DataGridContext dataGridContext )
    {
      var columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManagerBase;
      if( columnVirtualizationManager == null )
        return;

      var parentRowCells = this.ParentRowCells;
      m_virtualizationMode = columnVirtualizationManager.VirtualizationMode;
      parentRowCells.VirtualizationMode = m_virtualizationMode;

      //Cells provided through Row.Cells (in xaml for instance) must be merged now.
      parentRowCells.MergeFreeCells();

      //Check if the mode is virutalizing without cell recycling
      if( m_virtualizationMode != ColumnVirtualizationMode.Virtualizing )
        return;

      //In this mode, cells for all visible columns are automatically generated (speeds up first time horizontal scrolling).
      m_generateMissingCellsDispatcherOperation = this.Dispatcher.BeginInvoke( new Action<int>( this.GenerateMissingCells ), DispatcherPriority.Input, 0 );
    }

    private void FixedCellPanelLayoutUpdated( object sender, EventArgs e )
    {
      if( m_dataGridContext == null )
        return;

      // Once the layout pass is done, the flag must be reset so the width of columns can be resized.
      m_dataGridContext.CanIncreaseColumnWidth = true;
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

    private void RemoveCellFromFixedOrScrollingPanel( Cell cell )
    {
      if( cell == null )
        return;

      m_fixedPanel.Children.Remove( cell );
      m_scrollingPanel.Children.Remove( cell );
    }

    private IEnumerable<ColumnHierarchyManager.ILocation> GetPreviousLocationsOf( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        yield break;

      var previousLocation = location.GetPreviousSiblingOrCousin();
      while( previousLocation != null )
      {
        yield return previousLocation;
        previousLocation = previousLocation.GetPreviousSiblingOrCousin();
      }
    }

    private IEnumerable<ColumnHierarchyManager.ILocation> GetNextLocationsOf( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        yield break;

      var nextLocation = location.GetNextSiblingOrCousin();
      while( nextLocation != null )
      {
        yield return nextLocation;
        nextLocation = nextLocation.GetNextSiblingOrCousin();
      }
    }

    private IEnumerable<ColumnHierarchyManager.IColumnLocation> GetColumnLocations( IEnumerable<ColumnHierarchyManager.ILocation> locations )
    {
      if( locations == null )
        return Enumerable.Empty<ColumnHierarchyManager.IColumnLocation>();

      return ( from location in locations
               let columnLocation = location as ColumnHierarchyManager.IColumnLocation
               where ( columnLocation != null )
               select columnLocation );
    }

    private bool IsDropMarkAtDropTarget( DropTarget dropTarget )
    {
      if( ( dropTarget == null ) || ( m_dataGridContext == null ) )
        return false;

      var columnManager = m_dataGridContext.ColumnManager;
      var levelMarkers = columnManager.GetLevelMarkersFor( dropTarget.Columns );
      if( levelMarkers == null )
        return false;

      var splitterLocation = levelMarkers.Splitter;
      Debug.Assert( splitterLocation != null );
      if( splitterLocation == null )
        return false;

      // The drop mark location should not be adjusted when the only thing that is between the splitter and
      // the target location are hidden columns.
      if( dropTarget.Type != DropTargetType.Column )
      {
        var locations = ( dropTarget.Before ) ? this.GetNextLocationsOf( splitterLocation ) : this.GetPreviousLocationsOf( splitterLocation );

        return this.GetColumnLocations( locations ).All( columnLocation => !columnLocation.Column.Visible );
      }
      else
      {
        Debug.Assert( dropTarget.Column != null );

        foreach( var columnLocation in this.GetColumnLocations( this.GetPreviousLocationsOf( splitterLocation ) ) )
        {
          var column = columnLocation.Column;
          if( column == dropTarget.Column )
          {
            if( dropTarget.Before )
              return !column.Visible;

            return true;
          }

          if( column.Visible )
            break;
        }

        foreach( var columnLocation in this.GetColumnLocations( this.GetNextLocationsOf( splitterLocation ) ) )
        {
          var column = columnLocation.Column;
          if( column == dropTarget.Column )
          {
            if( !dropTarget.Before )
              return !column.Visible;

            return true;
          }

          if( column.Visible )
            break;
        }
      }

      return false;
    }

    private bool ShowDropMark()
    {
      if( ( m_dragDropState == null ) || ( m_dataGridContext == null ) )
        return false;

      var scrollViewer = TableViewScrollViewer.GetParentTableViewScrollViewer( this );
      if( scrollViewer == null )
        return false;

      var parentRowCells = this.ParentRowCells;
      if( parentRowCells == null )
        return false;

      var dropTarget = m_dragDropState.DropTarget;
      var targetCell = default( Cell );
      var showDropMarkBeforeTargetCell = ( dropTarget != null ) ? dropTarget.Before : false;

      if( ( dropTarget != null ) && !this.IsDropMarkAtDropTarget( dropTarget ) )
      {
        var dropTargetLocation = default( ColumnHierarchyManager.ILocation );

        if( dropTarget.Type == DropTargetType.Column )
        {
          Debug.Assert( dropTarget.Column != null );
          dropTargetLocation = m_dataGridContext.ColumnManager.GetColumnLocationFor( dropTarget.Column );
        }
        else
        {
          var levelMarkers = m_dataGridContext.ColumnManager.GetLevelMarkersFor( dropTarget.Columns );
          if( levelMarkers == null )
            return false;

          switch( dropTarget.Type )
          {
            case DropTargetType.Start:
              dropTargetLocation = levelMarkers.Start;
              break;

            case DropTargetType.Orphan:
              dropTargetLocation = levelMarkers.Orphan;
              break;

            default:
              throw new NotImplementedException();
          }
        }

        if( dropTargetLocation == null )
          return false;

        // Since the drop target may be a hidden column, we must look for the first visible column
        // next to it since the calculation requires a cell.
        {
          var columnLocation = dropTargetLocation as ColumnHierarchyManager.IColumnLocation;
          if( ( columnLocation != null ) && columnLocation.Column.Visible )
          {
            if( !parentRowCells.TryGetBindedCell( columnLocation.Column, false, out targetCell ) )
              return false;
          }
        }

        if( targetCell == null )
        {
          foreach( var columnLocation in this.GetColumnLocations( this.GetPreviousLocationsOf( dropTargetLocation ) ) )
          {
            if( !columnLocation.Column.Visible )
              continue;

            if( !parentRowCells.TryGetBindedCell( columnLocation.Column, false, out targetCell ) )
              continue;

            showDropMarkBeforeTargetCell = false;
            break;
          }
        }

        if( targetCell == null )
        {
          foreach( var columnLocation in this.GetColumnLocations( this.GetNextLocationsOf( dropTargetLocation ) ) )
          {
            if( !columnLocation.Column.Visible )
              continue;

            if( !parentRowCells.TryGetBindedCell( columnLocation.Column, false, out targetCell ) )
              continue;

            showDropMarkBeforeTargetCell = true;
            break;
          }
        }
      }

      double dropMarkOffset = 0;

      if( targetCell != null )
      {
        if( showDropMarkBeforeTargetCell )
        {
          dropMarkOffset = targetCell.TranslatePoint( new Point(), scrollViewer ).X;
        }
        else
        {
          dropMarkOffset = targetCell.TranslatePoint( new Point( targetCell.ActualWidth, 0d ), scrollViewer ).X;
        }
      }
      else
      {
        return false;
      }

      var adorner = m_dragDropState.DropMarkAdorner;
      if( adorner == null )
      {
        // FixedCellPanel can only be used in a Horizontal manner. We can afford to plug this vertical handling of drop mark.
        var pen = this.FixedColumnDropMarkPen;
        if( pen == null )
        {
          var dropMarkWidth = 1d;

          pen = new Pen( Brushes.Gray, dropMarkWidth );
        }

        adorner = new DropMarkAdorner( scrollViewer, pen, DropMarkOrientation.Vertical );
        adorner.HorizontalPosition = dropMarkOffset;

        var adornerLayer = AdornerLayer.GetAdornerLayer( scrollViewer );
        if( adornerLayer != null )
        {
          adornerLayer.Add( adorner );
        }

        m_dragDropState.DropMarkAdorner = adorner;
      }
      else
      {
        adorner.HorizontalPosition = dropMarkOffset;
      }

      return true;
    }

    private void HideDropMark()
    {
      if( m_dragDropState == null )
        return;

      var adorner = m_dragDropState.DropMarkAdorner;
      if( adorner == null )
        return;

      var adornerLayer = AdornerLayer.GetAdornerLayer( adorner.AdornedElement );
      if( adornerLayer != null )
      {
        adornerLayer.Remove( adorner );
      }

      m_dragDropState.DropMarkAdorner = null;
    }

    private void UpdateChildren()
    {
      // If the parent Row is not prepared, do not update children.
      // And contrary to this.UpdateChildren( UpdateMeasureTriggeredAction action ), m_dataGridContext can be null here, since it is from a dispatched call.
      if( m_dataGridContext == null )
        return;

      this.UpdateChildren( UpdateMeasureTriggeredAction.Unspecified );
    }

    private void UpdateChildren( UpdateMeasureTriggeredAction action )
    {
      Debug.Assert( m_dataGridContext != null, "Should not be called when a container is not prepared" );

      // If the parent Row is not prepared, do not update children
      if( m_dataGridContext == null )
        return;

      Row parentRow = this.ParentRow;

      // A CurrentItemChanged was received for a non current Row
      if( ( action == UpdateMeasureTriggeredAction.CurrentItemChanged ) && ( parentRow != null ) && ( !parentRow.IsCurrent ) )
        return;

      bool currentItemChangedOnCurrentRow = ( action == UpdateMeasureTriggeredAction.CurrentItemChanged ) && ( parentRow != null ) && ( parentRow.IsCurrent );
      var columnVirtualizationManager = this.ColumnVirtualizationManager;

      if( ( m_lastColumnVirtualizationManagerVersion != columnVirtualizationManager.Version ) || ( currentItemChangedOnCurrentRow )
            || ( m_virtualizingCellCollectionChangedDispatcherOperation != null ) )
      {
        this.UpdateChildren( columnVirtualizationManager );

        m_lastColumnVirtualizationManagerVersion = columnVirtualizationManager.Version;

        this.InvalidateMeasure();

        m_virtualizingCellCollectionChangedDispatcherOperation = null;
      }
    }

    private void UpdateChildren( TableViewColumnVirtualizationManagerBase columnVirtualizationManager )
    {
      var parentRowCells = this.ParentRowCells;

      //Prevent reentrance
      if( ( parentRowCells == null ) || parentRowCells.IsUpdating )
        return;

      using( parentRowCells.SetIsUpdating() )
      {
        //If in virtualizing mode but not recycling cells.
        if( m_virtualizationMode == ColumnVirtualizationMode.Virtualizing )
        {
          //Retrieve the cells that aren't needed anymore.
          var cellsToCollapse = this.GetCellsToCollapse( columnVirtualizationManager, parentRowCells );

          //Simply hide these cells for now.
          foreach( var cell in cellsToCollapse )
          {
            if( cell.CanBeCollapsed )
            {
              cell.Visibility = Visibility.Collapsed;
            }
            else
            {
              this.AddPermanentScrollingFieldNames( cell.FieldName );
            }
          }

          //And do the actual processing after scrolling has stopped.
          if( m_processUnusedCellsDispatcherOperation == null )
          {
            m_processUnusedCellsDispatcherOperation = this.Dispatcher.BeginInvoke( new Action<TableViewColumnVirtualizationManagerBase>( this.ProcessCollapsedCells ),
                                                                                   DispatcherPriority.Background, columnVirtualizationManager );
          }
        }
        else
        {
          //If recycling cells, we need to process them now, in order to minimize cells' creation.
          this.ProcessCollapsedCells( columnVirtualizationManager );
        }

        var fixedFieldNames = columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache ).ToList();
        var scrollingFieldNames = columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ).ToList();

        //Add the missing cells to the fixed region.
        foreach( string fieldName in fixedFieldNames )
        {
          var cell = this.GetCell( fieldName, parentRowCells );

          //Make sure the cell is in the appropriate panel.
          this.MoveCellToFixedPanel( cell );

        }

        //Add the missing cells to the scrolling region.
        foreach( string fieldName in scrollingFieldNames )
        {
          var cell = this.GetCell( fieldName, parentRowCells );

          //Make sure the cell is in the appropriate panel.
          this.MoveCellToScrollingPanel( cell );
        }

        m_fixedPanel.InvalidateMeasure();
        m_scrollingCellsDecorator.InvalidateMeasure();
        m_scrollingPanel.InvalidateMeasure();
      }
    }

    private void ProcessCollapsedCells( TableViewColumnVirtualizationManagerBase columnVirtualizationManager )
    {
      m_processUnusedCellsDispatcherOperation = null;

      if( m_dataGridContext == null )
        return;

      this.ClearPermanentScrollingFieldNames();

      //Retrieve the cells that aren't needed anymore.
      var collapsedCells = this.GetCellsToCollapse( columnVirtualizationManager, m_parentRowCells );

      foreach( Cell cell in collapsedCells )
      {
        var isCollapsible = cell.CanBeCollapsed;

        if( isCollapsible && cell.CanBeRecycled )
        {
          // Ensure to close the ContextMenu if it is open and the Cell is virtualized to avoid problems with ContextMenus defined as static resources
          // that won't be able to reopen again if the Close private method is called after the PlacementTarget is removed from the VisualTree
          var contextMenu = cell.ContextMenu;
          if( ( contextMenu != null ) && ( contextMenu.IsOpen ) )
          {
            contextMenu.IsOpen = false;
          }

          cell.ClearContainer();
          m_parentRowCells.Release( cell );
        }
        else
        {
          //Certain non recyclable cells like StatCells needs their content binding to be removed when they become out of view.
          cell.RemoveContentBinding();

          //If in virtualizing mode but not recycling cells, release the cell so it will be collapsed, so no need to measure and arrange it.
          if( isCollapsible && m_virtualizationMode == ColumnVirtualizationMode.Virtualizing )
          {
            m_parentRowCells.Release( cell );
          }
          else
          {
            //Since the cell cannot be released, it will not be collapsed. We must keep its fieldname
            //in order to let the scrolling sub-panel measure and arrange the cell out of view.
            this.AddPermanentScrollingFieldNames( cell.FieldName );
          }
        }
      }
    }

    private List<Cell> GetCellsToCollapse( TableViewColumnVirtualizationManagerBase columnVirtualizationManager, VirtualizingCellCollection parentRowCells )
    {
      return ( from cell in parentRowCells.BindedCells
               where !columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ).Contains( cell.FieldName )
                 && !columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache ).Contains( cell.FieldName )
               select cell ).ToList();
    }

    private Cell GetCell( string fieldName, VirtualizingCellCollection parentRowCells )
    {
      //The cell is created if it is missing.
      var cell = parentRowCells[ fieldName ];

      //Certain non recyclable cells like StatCells need their content binding to be updated when they become (or stay) in view.
      cell.AddContentBinding( m_dataGridContext, m_parentRow, this.Columns[ fieldName ] );

      //Only when in Virtualizing mode can the cell still be collapsed at this point, since parentRowCells[ fieldName ] will have uncollapsed it in other modes.
      if( m_virtualizationMode == ColumnVirtualizationMode.Virtualizing )
      {
        //The current local value must be cleared instead of setting a new local value to give a chance for a style to set a value.
        cell.ClearValue( Cell.VisibilityProperty );
      }

      return cell;
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
      var parentRowCells = this.ParentRowCells;
      if( parentRowCells == null )
        return;

      var fieldNameToPositionMapping = columnVirtualizationManager.GetFieldNameToPosition( m_parentRow.LevelCache );
      var fieldNames = columnVirtualizationManager.GetFixedFieldNames( m_parentRow.LevelCache )
                         .Concat( columnVirtualizationManager.GetScrollingFieldNames( m_parentRow.LevelCache ) )
                         .Concat( m_permanentScrollingFieldNames );

      foreach( var fieldName in fieldNames )
      {
        Cell cell = parentRowCells.GetCell( fieldName, false );
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

    private void GenerateMissingCells( int index )
    {
      m_generateMissingCellsDispatcherOperation = null;

      if( m_dataGridContext == null )
      {
        // The row is being recycled without having been prepared properly.  By setting these properties,
        // it will be correctly prepare when used again (through IVirtualizingCellsHost.PrepareCellsHost()).
        m_virtualizationModeChanged = true;
        m_virtualizingCellCollectionUpdateRequired = true;
        return;
      }

      DateTime startTime = DateTime.UtcNow;

      //Generate cells only for visible columns
      var columns = this.VisibleColumns;
      var parentRowCells = this.ParentRowCells;
      var count = columns.Count;
      var actualIndex = count;

      for( int i = index; i < count; i++ )
      {
        var column = columns[ i ];

        if( parentRowCells.HasVirtualizedCell( column.FieldName ) )
          continue;

        var cell = parentRowCells.AddVirtualizedCell( column );

        //Though the cell has been newly prepared to provide better first time scrolling, it needs to be cleared like any other out of view cell.
        //For non releasable cells (like StatCell), it's been properly processed in Row.PrepareUnbindedCell(), in the previous call.
        if( cell.CanBeRecycled )
        {
          cell.ClearContainer();
        }

        if( DateTime.UtcNow.Subtract( startTime ) > FixedCellPanel.MaxProcessDurationTime )
        {
          actualIndex = i + 1;
          break;
        }
      }

      if( actualIndex == count )
        return;

      //If there is still cells to generate, dispatch again.
      m_generateMissingCellsDispatcherOperation = this.Dispatcher.BeginInvoke( new Action<int>( this.GenerateMissingCells ), DispatcherPriority.Input, actualIndex );
    }

    private void GenerateOrRemoveCell( ColumnBase column )
    {
      Debug.Assert( column != null );

      var parentRowCells = this.ParentRowCells;

      switch( m_virtualizationMode )
      {
        case ColumnVirtualizationMode.None:
          {
            if( !column.Visible )
            {
              Cell cell;
              if( parentRowCells.TryGetBindedCell( column, false, out cell ) )
              {
                //Only remove cells that can be generated without any unique configuration (e.g. no custom template, no ResultPropertyName, etc..)
                if( cell.CanBeRecycled )
                {
                  parentRowCells.InternalRemove( cell );
                }
              }
            }
            break;
          }

        case ColumnVirtualizationMode.Recycling:
          {
            if( !column.Visible && ( m_synchronizeRecyclingBinsDispatcherOperation == null ) )
            {
              m_synchronizeRecyclingBinsDispatcherOperation = this.Dispatcher.BeginInvoke( DispatcherPriority.Background,
                                                                                           new Action( this.SynchronizeRecyclingBinsWithRecyclingGroups ) );
            }
            break;
          }

        case ColumnVirtualizationMode.Virtualizing:
          {
            if( column.Visible )
            {
              if( !parentRowCells.HasVirtualizedCell( column.FieldName ) )
              {
                parentRowCells.AddVirtualizedCell( column );
              }
              break;
            }

            parentRowCells.RemoveVirtualizedCell( column );
            break;
          }
      }
    }

    private void UpdateVirtualizationMode()
    {
      var columnVirtualizationManager = this.ColumnVirtualizationManager;
      if( columnVirtualizationManager == null )
        return;

      var parentRowCells = this.ParentRowCells;
      m_virtualizationMode = columnVirtualizationManager.VirtualizationMode;
      parentRowCells.VirtualizationMode = m_virtualizationMode;

      switch( m_virtualizationMode )
      {
        case ColumnVirtualizationMode.None:
          {
            //Not virtualizing
            parentRowCells.BindRecycledCells( this.Columns );
            parentRowCells.BindVirtualizedCells();
            break;
          }

        case ColumnVirtualizationMode.Recycling:
          {
            //Virtualizing and recycling
            parentRowCells.ClearOutOfViewBindedCells( this.GetCellsToCollapse( columnVirtualizationManager, parentRowCells ) );
            parentRowCells.ClearVirtualizedCells();
            break;
          }

        case ColumnVirtualizationMode.Virtualizing:
          {
            //Virtualizing with no recycling
            this.ProcessCollapsedCells( columnVirtualizationManager );
            parentRowCells.VirtualizeRecycledCells();
            if( m_generateMissingCellsDispatcherOperation == null )
            {
              this.GenerateMissingCells( 0 );
            }
            break;
          }
      }
    }

    private void SynchronizeRecyclingBinsWithRecyclingGroups()
    {
      m_synchronizeRecyclingBinsDispatcherOperation = null;

      if( m_dataGridContext == null )
        return;

      m_parentRowCells.SynchronizeRecyclingBinsWithRecyclingGroups( this.VisibleColumns );
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

    private static object CoerceMinHeight( DependencyObject sender, object value )
    {
      var self = sender as FixedCellPanel;
      if( self == null )
        return value;

      return self.CoerceMinHeight( new Thickness(), value );
    }

    private static void InvalidateMinHeight( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var self = sender as FixedCellPanel;
      if( self == null )
        return;

      self.CoerceValue( FixedCellPanel.MinHeightProperty );
    }

    #region ZOrder Management

    private void RecomputeZOrder()
    {
      ZOrderHelper.ComputeZOrder( m_visualChildren, ref m_zIndexMapping );
      m_zOrderDirty = false;
    }

    private void InvalidateZOrder()
    {
      m_zOrderDirty = true;
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

    #endregion

    #region IVirtualizingCellsHost Members

    bool IVirtualizingCellsHost.CanModifyLogicalParent
    {
      get
      {
        return true;
      }
    }

    void IVirtualizingCellsHost.ClearLogicalParent( Cell cell )
    {
      Debug.Assert( cell != null );

      VirtualizingUICellCollection cellCollection = this.Children as VirtualizingUICellCollection;

      Debug.Assert( cellCollection != null );

      cellCollection.ClearCellLogicalParent( cell );

      this.RemoveCellFromFixedOrScrollingPanel( cell );
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
      if( m_cellsHostPrepared )
        return;

      if( dataGridContext != null )
      {
        m_dataGridContext = dataGridContext;

        object dataItem = this.ParentRow.DataContext;

        foreach( Cell cell in this.CellsInView )
        {
          cell.PrepareContainer( dataGridContext, dataItem );
        }

        var columnVirtualizationManager = this.ColumnVirtualizationManager;

        //If the manager has changed, make sure to stop listening to it.
        if( columnVirtualizationManager != m_previousColumnVirtualizationManager )
        {
          VirtualizingCellCollectionUpdateRequiredEventManager.RemoveListener( m_previousColumnVirtualizationManager, this );
          VirtualizingCellCollectionUpdateRequiredEventManager.AddListener( columnVirtualizationManager, this );
        }

        UpdateMeasureRequiredEventManager.AddListener( columnVirtualizationManager, this );
        m_previousColumnVirtualizationManager = columnVirtualizationManager;

        var parentRowCells = this.ParentRowCells;
        if( parentRowCells != null )
        {
          VirtualizingCellCollectionChangedEventManager.AddListener( parentRowCells, this );
        }

        //When pulling a row from the recycling pool, need to make sure it is in the correct virtualization mode and its VirtualzingCellCollection is update to date.
        if( m_virtualizingCellCollectionUpdateRequired )
        {
          m_virtualizingCellCollectionUpdateRequired = false;

          if( m_virtualizationModeChanged )
          {
            m_virtualizationModeChanged = false;
            this.UpdateVirtualizationMode();
          }

          if( m_visibleColumnsChanged )
          {
            m_visibleColumnsChanged = false;
            var columns = this.Columns;
            foreach( WeakReference weakRef in m_changedVisibleColumns )
            {
              var column = weakRef.Target as ColumnBase;

              if( ( column == null ) || !columns.Contains( column ) )
                continue;

              this.GenerateOrRemoveCell( column );
            }
            m_changedVisibleColumns.Clear();
            m_changedVisibleColumns.TrimExcess();
          }

          if( m_visibleColumnsAdded )
          {
            m_visibleColumnsAdded = false;
            this.GenerateMissingCells( 0 );
          }
        }

        m_cellsHostPrepared = true;
      }
    }

    void IVirtualizingCellsHost.ClearCellsHost()
    {
      if( !m_cellsHostPrepared )
        return;

      var parentRowCells = this.ParentRowCells;
      if( parentRowCells != null )
      {
        VirtualizingCellCollectionChangedEventManager.RemoveListener( parentRowCells, this );
      }

      //Make sure to stop listening from the same manager it started to listen from.
      UpdateMeasureRequiredEventManager.RemoveListener( m_previousColumnVirtualizationManager, this );

      m_dataGridContext = null;
      m_cellsHostPrepared = false;
    }

    void IVirtualizingCellsHost.InvalidateCellsHostMeasure()
    {
      if( m_dataGridContext == null )
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
      var viewportWidth = this.ParentScrollViewer.ViewportWidth;

      //If fixed cells fill the entire viewport, then there is no scrolling cell that can be brought in to view, simply return.
      if( fixedWidth >= viewportWidth )
        return true;

      var scrollingPanel = this.ScrollingCellsDecorator;
      var scrollingArea = new Rect( scrollingPanel.TranslatePoint( new Point(), this ), new Size( viewportWidth - fixedWidth, scrollingPanel.ActualHeight ) );
      var cellArea = new Rect( cell.TranslatePoint( new Point(), this ), new Size( cell.ParentColumn.ActualWidth, cell.ActualHeight ) );

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
            var offset = targetObject.TranslatePoint( new Point(), cell );
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
      if( managerType == typeof( UpdateMeasureRequiredEventManager ) )
      {
        var action = UpdateMeasureTriggeredAction.Unspecified;
        var eventArgs = e as UpdateMeasureRequiredEventArgs;

        if( eventArgs != null )
        {
          action = eventArgs.TriggeredAction;
        }

        this.UpdateChildren( action );
      }
      else if( managerType == typeof( VirtualizingCellCollectionUpdateRequiredEventManager ) )
      {
        var eventArgs = e as VirtualizingCellCollectionUpdateRequiredEventArgs;

        switch( eventArgs.TriggeredAction )
        {
          case VirtualizingCellCollectionUpdateTriggeredAction.VirtualizationModeChanged:
            {
              if( !m_cellsHostPrepared )
              {
                m_virtualizationModeChanged = true;
                m_virtualizingCellCollectionUpdateRequired = true;
                break;
              }

              this.UpdateVirtualizationMode();
              break;
            }

          case VirtualizingCellCollectionUpdateTriggeredAction.VisibleColumnsChanged:
            {
              var columns = eventArgs.Columns;
              if( ( columns == null ) || ( columns.Count <= 0 ) )
                break;

              if( !m_cellsHostPrepared )
              {
                foreach( var column in columns )
                {
                  m_changedVisibleColumns.Add( new WeakReference( column ) );
                }

                m_visibleColumnsChanged = true;
                m_virtualizingCellCollectionUpdateRequired = true;
                break;
              }

              //A column was added or removed from VisibleColumns.
              foreach( var column in columns )
              {
                this.GenerateOrRemoveCell( column );
              }
              break;
            }

          //This action will be triggered only when in Virtualizing mode, so no need to check it.
          //No need to handle Remove, Replace, Reset, since every row will be flushed and recreated, and rows present in recycle bins will simply be flushed.
          case VirtualizingCellCollectionUpdateTriggeredAction.VisibleColumnsAdded:
            {
              if( !m_cellsHostPrepared )
              {
                m_visibleColumnsAdded = true;
                m_virtualizingCellCollectionUpdateRequired = true;
                break;
              }

              if( m_generateMissingCellsDispatcherOperation == null )
              {
                this.GenerateMissingCells( 0 );
              }
              break;
            }
        }
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
      }
      else
      {
        return false;
      }

      return true;
    }

    #endregion

    private int m_lastColumnVirtualizationManagerVersion = -1;
    private Row m_parentRow; // = null;
    private ScrollViewer m_parentScrollViewer; // = null;
    private bool m_cellsHostPrepared; //false
    private bool m_zOrderDirty; //false

    private Size m_desiredSize = new Size();
    private VirtualizingCellCollection m_parentRowCells; // = null;

    private DragDropState m_dragDropState = null;

    // The immediate visual children of this panel.
    private FixedCellSubPanel m_fixedPanel;
    private ScrollingCellsDecorator m_scrollingCellsDecorator;
    private VirtualizingFixedCellSubPanel m_scrollingPanel;

    private int[] m_zIndexMapping; // = null;

    private const int c_visualChildrenCount = 3;
    private readonly IList<UIElement> m_visualChildren = new List<UIElement>();

    private DataGridContext m_dataGridContext; // = null;
    private TableViewColumnVirtualizationManagerBase m_previousColumnVirtualizationManager; // = null;

    private DispatcherOperation m_processUnusedCellsDispatcherOperation; // = null;
    private DispatcherOperation m_virtualizingCellCollectionChangedDispatcherOperation;
    private DispatcherOperation m_generateMissingCellsDispatcherOperation;
    private DispatcherOperation m_synchronizeRecyclingBinsDispatcherOperation;
    private ColumnVirtualizationMode m_virtualizationMode;
    private List<WeakReference> m_changedVisibleColumns = new List<WeakReference>();
    private bool m_virtualizingCellCollectionUpdateRequired;
    private bool m_virtualizationModeChanged;
    private bool m_visibleColumnsChanged;
    private bool m_visibleColumnsAdded;

    #region DragDropState Private Class

    private sealed class DragDropState
    {
      internal DragDropState( double splitterOffset )
      {
        m_initialSplitterOffset = splitterOffset;
      }

      internal double InitialSplitterOffset
      {
        get
        {
          return m_initialSplitterOffset;
        }
      }

      internal DropMarkAdorner DropMarkAdorner
      {
        get;
        set;
      }

      internal DropTarget DropTarget
      {
        get;
        set;
      }

      private readonly double m_initialSplitterOffset;
    }

    #endregion

    #region DropTarget Private Class

    private sealed class DropTarget
    {
      internal DropTarget( DropTargetType type, ColumnCollection columns )
      {
        if( ( type != DropTargetType.Start ) && ( type != DropTargetType.Orphan ) )
          throw new ArgumentException( "The drop target must be a Start or Orphan target type.", "type" );

        m_type = type;
        m_column = default( ColumnBase );
        m_before = ( type == DropTargetType.Orphan );
        m_columns = columns;
      }

      internal DropTarget( ColumnBase column, bool before, ColumnCollection columns )
      {
        if( column == null )
          throw new ArgumentNullException( "column" );

        m_type = DropTargetType.Column;
        m_column = column;
        m_before = before;
        m_columns = columns;
      }

      internal DropTargetType Type
      {
        get
        {
          return m_type;
        }
      }

      internal ColumnBase Column
      {
        get
        {
          return m_column;
        }
      }

      internal bool Before
      {
        get
        {
          return m_before;
        }
      }

      internal ColumnCollection Columns
      {
        get
        {
          return m_columns;
        }
      }

      private readonly DropTargetType m_type;
      private readonly ColumnBase m_column;
      private readonly ColumnCollection m_columns;
      private readonly bool m_before;
    }

    #endregion

    #region DropTargetType Private Enum

    private enum DropTargetType
    {
      Column = 0,
      Start,
      Orphan
    }

    #endregion
  }
}
