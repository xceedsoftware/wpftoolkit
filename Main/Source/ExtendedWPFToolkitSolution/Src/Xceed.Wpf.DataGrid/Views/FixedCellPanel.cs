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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Wpf.DataGrid.Print;
using Xceed.Utils.Math;
using System.Windows.Threading;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Animation;

namespace Xceed.Wpf.DataGrid.Views
{
  public class FixedCellPanel : Panel, IPrintInfo, IVirtualizingCellsHost, IWeakEventListener
  {
    #region Static Fields

    private static readonly Point EmptyPoint = new Point();
    private static readonly Rect PositiveInfinityRect = new Rect( new Size( Double.PositiveInfinity, Double.PositiveInfinity ) );

    #endregion Static Fields

    #region Constructors

    public FixedCellPanel()
    {
      // None of the Visual children of the FixedCellPanel will scroll. The needed scrolling
      // as demanded by the parent ScrollViewer will be manually transferred to the 
      // content (m_scrollingPanel) of the ScrollingCellsDecorator below.

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

    #endregion Constructors

    #region ColumnStretchMinWidth Property

    public static readonly DependencyProperty ColumnStretchMinWidthProperty = DependencyProperty.Register(
      "ColumnStretchMinWidth",
      typeof( double ),
      typeof( FixedCellPanel ),
      new UIPropertyMetadata( 50d, new PropertyChangedCallback( FixedCellPanel.ColumnStretchMinWidthChanged ) ),
      new ValidateValueCallback( TableView.ValidateColumnStretchMinWidthCallback ) );

    private double m_columnStretchMinWidth = 50d;

    public double ColumnStretchMinWidth
    {
      get
      {
        return m_columnStretchMinWidth;
      }
      set
      {
        this.SetValue( FixedCellPanel.ColumnStretchMinWidthProperty, value );
      }
    }

    private static void ColumnStretchMinWidthChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      FixedCellPanel panel = sender as FixedCellPanel;

      if( panel == null )
        return;

      if( e.NewValue != null )
        panel.m_columnStretchMinWidth = ( double )e.NewValue;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( panel );

      if( ( dataGridContext != null ) && ( dataGridContext.VisibleColumns.Count > 0 ) )
      {
        // When ColumnStretchMinWidth changes trigger an update by clearing all 
        // DesiredWidth values.
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
      new UIPropertyMetadata( ColumnStretchMode.None, new PropertyChangedCallback( FixedCellPanel.ColumnStretchModeChanged ) ) );

    private ColumnStretchMode m_columnStretchMode = ColumnStretchMode.None;

    public ColumnStretchMode ColumnStretchMode
    {
      get
      {
        return m_columnStretchMode;
      }
      set
      {
        this.SetValue( FixedCellPanel.ColumnStretchModeProperty, value );
      }
    }

    private static void ColumnStretchModeChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      FixedCellPanel panel = sender as FixedCellPanel;

      if( panel == null )
        return;

      if( e.NewValue != null )
        panel.m_columnStretchMode = ( ColumnStretchMode )e.NewValue;
    }

    #endregion ColumnStretchMode Property

    #region DataGridContext Internal Property

    // DataGridContext is set when IVirtualizingCellsHost.PrepareCellsHost is called and cleared when
    // IVirtualizingCellsHost.ClearContainer is called. We always use the DataGridContext passed
    // and assert the old one is never referenced
    internal DataGridContext DataGridContext
    {
      get
      {
        return m_dataGridContext;
      }
    }

    #endregion

    #region FixedCellCount Property

    public static readonly DependencyProperty FixedCellCountProperty = DependencyProperty.Register(
      "FixedCellCount",
      typeof( int ),
      typeof( FixedCellPanel ),
      new UIPropertyMetadata( 0, new PropertyChangedCallback( FixedCellPanel.FixedCellCountChanged ) ),
      new ValidateValueCallback( FixedCellPanel.ValidateFixedCellCountCallback ) );

    private int m_fixedCellCount; // = 0;

    public int FixedCellCount
    {
      get
      {
        return m_fixedCellCount;
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

      if( e.NewValue != null )
        panel.m_fixedCellCount = ( int )e.NewValue;

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

    public static readonly DependencyProperty FixedColumnDropMarkPenProperty =
        DependencyProperty.Register( "FixedColumnDropMarkPen", typeof( Pen ), typeof( FixedCellPanel ), new PropertyMetadata( new PropertyChangedCallback( FixedCellPanel.OnFixedColumnDropMarkPenChanged ) ) );

    private Pen m_fixedColumnDropMarkPen;

    private static void OnFixedColumnDropMarkPenChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      FixedCellPanel panel = o as FixedCellPanel;

      if( panel == null )
        return;

      panel.m_fixedColumnDropMarkPen = e.NewValue as Pen;
    }

    public Pen FixedColumnDropMarkPen
    {
      get
      {
        return m_fixedColumnDropMarkPen;
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
          Debug.Assert( this.DataGridContext != null );
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

    #region NeedToClearAllCreatedCells Private Property

    private bool NeedToClearAllCreatedCells
    {
      get
      {
        return m_flags[ ( int )FixedCellPanelFlags.NeedToClearAllCreatedCells ];
      }
      set
      {
        m_flags[ ( int )FixedCellPanelFlags.NeedToClearAllCreatedCells ] = value;
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
        VirtualizingCellCollection cellCollection = this.ParentRowCells;
        if( cellCollection == null )
          return new Cell[ 0 ];

        return cellCollection.BindedCells;
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
      if( string.IsNullOrEmpty( fieldName ) )
        return;

      if( !m_permanentScrollingFieldNames.Contains( fieldName ) )
      {
        m_permanentScrollingFieldNames.Add( fieldName );
      }
    }

    private bool AddPermanentScrollingFieldNamesIfOutOfViewColumn( string fieldName )
    {
      bool retVal = false;

      if( string.IsNullOrEmpty( fieldName ) )
        return retVal;

      if( this.DataGridContext == null )
      {
        if( !m_permanentScrollingFieldNames.Contains( fieldName ) )
        {
          m_permanentScrollingFieldNames.Add( fieldName );
          retVal = true;
        }

        return retVal;
      }

      TableViewColumnVirtualizationManager columnVirtualizationManager = this.DataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

      if( columnVirtualizationManager == null )
        throw new DataGridInternalException();

      if( !m_permanentScrollingFieldNames.Contains( fieldName ) && !columnVirtualizationManager.FixedFieldNames.Contains( fieldName )
        && !columnVirtualizationManager.ScrollingFieldNames.Contains( fieldName ) )
      {
        m_permanentScrollingFieldNames.Add( fieldName );
        retVal = true;
      }

      return retVal;
    }

    private void ClearPermanentScrollingFieldNames()
    {
      m_permanentScrollingFieldNames.Clear();
      m_permanentScrollingFieldNames.TrimExcess();
    }

    private readonly HashSet<string> m_permanentScrollingFieldNames = new HashSet<string>();

    #endregion

    protected override Visual GetVisualChild( int index )
    {
      if( this.ZOrderDirty )
        this.RecomputeZOrder();

      int correctedIndex = ( m_zIndexMapping == null )
      ? index
      : m_zIndexMapping[ index ];

      switch( correctedIndex )
      {
        case 0:
          return m_fixedPanel;
        case 1:
          return m_splitter;
        case 2:
          return m_scrollingCellsDecorator;
        default:
          throw new ArgumentOutOfRangeException( "index" );
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

      TableViewColumnVirtualizationManager columnVirtualizationManager =
        dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

      if( columnVirtualizationManager == null )
        throw new DataGridInternalException();

      this.ApplyFixedTranform();

      // If a change occured and did not triggered an UpdateMeasureRequiredEvent by the
      // columnVirtualizationManager, we ensure the Row contains the necessary Cells
      if( m_lastColumnVirtualizationManagerVersion != columnVirtualizationManager.Version )
      {
        this.UpdateChildren();
      }

      double tempDesiredHeight = 0;
      m_desiredSize.Width = 0;

      // Measure the Splitter
      m_splitter.Measure( availableSize );

      // Measure the Panels (visual children) around the cells (logical children).
      // Logical children will not be measured again during this process.
      m_fixedPanel.Measure( availableSize );

      if( m_fixedPanel.DesiredSize.Height > tempDesiredHeight )
        tempDesiredHeight = m_fixedPanel.DesiredSize.Height;

      // Set the desired size for m_scrollingPanel which will be passed to the 
      // Decorated that will allow the first cell to be clipped
      m_desiredSize.Width += columnVirtualizationManager.VisibleColumnsTotalWidth;

      m_scrollingCellsDecorator.Measure( availableSize );

      if( m_scrollingCellsDecorator.DesiredSize.Height > tempDesiredHeight )
        tempDesiredHeight = m_scrollingCellsDecorator.DesiredSize.Height;

      m_desiredSize.Height = tempDesiredHeight;

      if( !double.IsPositiveInfinity( availableSize.Width ) )
      {
        dataGridContext.ColumnStretchingManager.CalculateColumnStretchWidths(
          availableSize.Width,
          this.ColumnStretchMode,
          this.ColumnStretchMinWidth );
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

      // Very simple horizontally Arrange implementation
      // We use m_visualChildren instead of GetVisualChild(index)
      // to ensure the sub panels are always arranged in the 
      // correct visible order
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
    internal static int CalculateVisibleIndex( int visiblePosition, DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return -1;

      int visibleIndex = 0;
      LinkedListNode<ColumnBase> tempNode = dataGridContext.ColumnsByVisiblePosition.First;

      for( int i = 0; i < visiblePosition; i++ )
      {
        if( tempNode == null )
          break;

        if( tempNode.Value.Visible )
          visibleIndex++;

        tempNode = tempNode.Next;
      }

      return visibleIndex;
    }

    internal double GetFixedWidth()
    {
      double offset = VisualTreeHelper.GetOffset( m_scrollingCellsDecorator ).X;

      DependencyObject parent = VisualTreeHelper.GetParent( this );

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
        m_parentScrollViewer = TableViewScrollViewer.GetParentScrollViewer( this );

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
          && ( !cell.HasCellEditorError )
          && ( cell != dataGridContext.CurrentCell )
          && ( !Row.GetIsTemplateCell( cell ) )
          && ( !TableflowView.GetIsBeingDraggedAnimated( cell.ParentColumn ) )
          && ( !( bool )cell.GetValue( ColumnManagerCell.IsBeingDraggedProperty ) );
    }

    private static void ClearCellVisualState( Cell cell )
    {
      if( cell == null )
        return;

      ContextMenu contextMenu = cell.ContextMenu;

      // Ensure to close the ContextMenu if it is open and the Cell
      // is virtualized to avoid problems with ContextMenus
      // defined as static resources that won't be able to reopen
      // again if the Close private method is called after the 
      // PlacementTarget is removed from the VisualTree
      if( ( contextMenu != null ) && ( contextMenu.IsOpen ) )
      {
        contextMenu.IsOpen = false;
      }

      cell.ClearContainerVisualState();
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

    private bool MoveCellToScrollingPanel( Cell movingCell, bool release = false )
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

      if( release )
      {
        FixedCellPanel.ClearCellVisualState( movingCell );
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

      TableViewColumnVirtualizationManager columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

      if( columnVirtualizationManager == null )
        throw new DataGridInternalException();

      bool currentItemChangedOnCurrentRow = ( action == UpdateMeasureTriggeredAction.CurrentItemChanged ) && ( parentRow != null ) && ( parentRow.IsCurrent );

      if( ( m_lastColumnVirtualizationManagerVersion != columnVirtualizationManager.Version ) || ( currentItemChangedOnCurrentRow )
            || ( m_virtualizingCellCollectionChangedDispatcherOperation != null ) )
      {
        this.UpdateChildren( dataGridContext, columnVirtualizationManager, this.ParentRowCells );

        m_lastColumnVirtualizationManagerVersion = columnVirtualizationManager.Version;

        this.InvalidateMeasure();

        m_virtualizingCellCollectionChangedDispatcherOperation = null;
      }
    }

    private void UpdateChildren( DataGridContext dataGridContext, TableViewColumnVirtualizationManager columnVirtualizationManager, VirtualizingCellCollection parentRowCells )
    {
      //Prevent reentrance
      if( m_parentRowCells.AlreadyUpdating )
        return;

      m_parentRowCells.AlreadyUpdating = true;

      if( dataGridContext == null )
        throw new DataGridInternalException( "DataGridContext is null for FixedCellPanel" );

      if( columnVirtualizationManager == null )
        throw new DataGridInternalException( "ColumnVirtualizationManager is null for FixedCellPanel" );

      if( parentRowCells == null )
        return;

      this.ClearPermanentScrollingFieldNames();

      List<string> currentFieldsName = new List<string>();
      currentFieldsName.AddRange( columnVirtualizationManager.FixedFieldNames );
      currentFieldsName.AddRange( columnVirtualizationManager.ScrollingFieldNames );

      HashSet<Cell> unusedCells = new HashSet<Cell>( parentRowCells.BindedCells );

      //Idenfity the binded cells that aren't needed anymore.
      foreach( string fieldName in currentFieldsName )
      {
        ColumnBase column = dataGridContext.Columns[ fieldName ];
        Cell cell;

        if( parentRowCells.TryGetCell( column, out cell ) )
        {
          //Certain non recyclable cells like StatCells need their content binding to be updated when they become (or stay) in view.
          cell.AddContentBinding();
          unusedCells.Remove( cell );
        }
      }
      currentFieldsName.Clear();
      currentFieldsName.TrimExcess();

      //Release the unused binded cells now in order to minimize the number of cell's creation.
      foreach( Cell cell in unusedCells )
      {
        bool release = this.CanReleaseCell( cell );

        //We move the unused cell into the scrolling panel since there is more chance it will be reused there in the future.
        this.MoveCellToScrollingPanel( cell, release );

        if( release )
        {
          parentRowCells.Release( cell );
        }
        //Since the cell cannot be released, it will not be collapsed. We must keep the field name in order to let the scrolling sub-panel measure and arrange the cell out of view.
        else
        {
          //Certain non recyclable cells like StatCells needs their content binding to be removed when they become out of view.
          cell.RemoveContentBinding();
          this.AddPermanentScrollingFieldNames( cell.FieldName );
        }
      }
      unusedCells.Clear();
      unusedCells.TrimExcess();

      //Add the missing cells to the fixed region.
      foreach( string fieldName in columnVirtualizationManager.FixedFieldNames )
      {
        //The cell is created if it is missing.
        Cell cell = parentRowCells[ fieldName ];

        //Make sure the cell is in the appropriate panel.
        this.MoveCellToFixedPanel( cell );
      }

      //Add the missing cells to the scrolling region.
      foreach( string fieldName in columnVirtualizationManager.ScrollingFieldNames )
      {
        //The cell is created if it is missing.
        Cell cell = parentRowCells[ fieldName ];

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

      m_parentRowCells.AlreadyUpdating = false;
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

    private void AdjustCellTabIndex( TableViewColumnVirtualizationManager columnVirtualizationManager )
    {
      // The order of the children in the panel is not guaranteed to be good.  We do not
      // reorder children because we will have to remove and add them at a different location.
      // Doing that will cause more calculations to occur, so we only change the TabIndex.
      VirtualizingCellCollection collection = this.ParentRowCells;
      if( collection == null )
        return;

      HashSet<string> bindedFields = new HashSet<string>();
      foreach( Cell cell in collection.BindedCells )
      {
        bindedFields.Add( cell.FieldName );
        this.AdjustCellTabIndex( columnVirtualizationManager, cell );
      }

      List<string> targetFields = new List<string>();
      targetFields.AddRange( columnVirtualizationManager.FixedFieldNames );
      targetFields.AddRange( columnVirtualizationManager.ScrollingFieldNames );
      targetFields.AddRange( m_permanentScrollingFieldNames );

      foreach( string fieldName in targetFields )
      {
        if( bindedFields.Contains( fieldName ) )
          continue;

        Cell cell = collection[ fieldName ];
        Debug.Assert( cell != null );

        this.AdjustCellTabIndex( columnVirtualizationManager, cell );
      }
    }

    private void AdjustCellTabIndex( TableViewColumnVirtualizationManager columnVirtualizationManager, Cell targetCell )
    {
      int tabIndex;
      if( columnVirtualizationManager.FieldNameToPosition.TryGetValue( targetCell.FieldName, out tabIndex ) )
      {
        if( tabIndex != KeyboardNavigation.GetTabIndex( targetCell ) )
        {
          KeyboardNavigation.SetTabIndex( targetCell, tabIndex );
        }
      }
      else
      {
        targetCell.ClearValue( Cell.TabIndexProperty );
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
      parentRowCells.ClearUnusedRecycleBins( dataGridContext.Columns );

      //Remove the inaccessible cells from the panels.
      this.RemoveLeakingCells( parentRowCells.Cells );
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

      // Ensure the ZOrder is also considered
      // by the panel containing the Cell
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

    #region IPrintInfo Members

    double IPrintInfo.GetPageRightOffset( double horizontalOffset, double viewportWidth )
    {
      double nonScrollingWidth = ( ( FixedCellPanel )this ).GetFixedWidth();

      IPrintInfo printInfo = m_scrollingPanel as IPrintInfo;

      return printInfo.GetPageRightOffset( horizontalOffset, viewportWidth - nonScrollingWidth );
    }

    void IPrintInfo.UpdateElementVisibility( double horizontalOffset, double viewportWidth, object state )
    {
      double nonScrollingWidth = ( ( FixedCellPanel )this ).GetFixedWidth();

      IPrintInfo printInfo = m_scrollingPanel as IPrintInfo;
      double compensationOffset = TableView.GetCompensationOffset( this );

      printInfo.UpdateElementVisibility( horizontalOffset - compensationOffset, viewportWidth - nonScrollingWidth, state );
    }

    object IPrintInfo.CreateElementVisibilityState()
    {
      IPrintInfo printInfo = m_scrollingPanel as IPrintInfo;

      return printInfo.CreateElementVisibilityState();
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
      // This method is called in Row.PrepareContainer and Row.OnApplyTemplate, which is also 
      // called in Row.PrepareContainer but only if the template was not previously applied, so 
      // it is possible that this panel is already prepared. In that case, we only ignore this preparation.
      if( this.CellsHostPrepared )
        return;

      Debug.Assert( ( this.DataGridContext == null ) || ( dataGridContext == this.DataGridContext ),
        "DataGridContext must be null otherwise this indicates that the FixedCellPanel was not cleaned properly before it is prepared again." );

      m_dataGridContext = dataGridContext;

      if( dataGridContext != null )
      {
        object dataItem = this.ParentRow.DataContext;

        // We will clear every created Cells when ClearContainer will be called
        // since some Cells may have been virtualized or prepared, but still
        // have some invalid states from previous data item when the data item 
        // is the current item in edition when the container is prepared
        this.NeedToClearAllCreatedCells = ( dataGridContext.DataGridControl.CurrentItemInEdition == dataItem );

        foreach( Cell cell in this.CellsInView )
        {
          cell.PrepareContainer( dataGridContext, dataItem );
        }

        TableViewColumnVirtualizationManager columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

        if( columnVirtualizationManager != null )
        {
          UpdateMeasureRequiredEventManager.AddListener( columnVirtualizationManager, this );
        }

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
      if( m_dataGridContext != null )
      {
        TableViewColumnVirtualizationManager columnVirtualizationManager = m_dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

        if( columnVirtualizationManager != null )
        {
          UpdateMeasureRequiredEventManager.RemoveListener( columnVirtualizationManager, this );
          this.CellsHostPrepared = false;
        }

        VirtualizingCellCollection parentRowCells = this.ParentRowCells;

        if( parentRowCells != null )
        {
          VirtualizingCellCollectionChangedEventManager.RemoveListener( parentRowCells, this );
        }
      }

      Debug.Assert( !this.CellsHostPrepared, "Should always be unregistered from DataGridContext.ColumnVirtualizationManager" );

      m_dataGridContext = null;
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

    bool IVirtualizingCellsHost.BringIntoView( Cell cell )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return false;

      TableViewColumnVirtualizationManager columnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;
      if( columnVirtualizationManager == null )
        return false;

      Rect cellRect = new Rect();
      cellRect.Width = cell.ParentColumn.Width;
      cellRect.Height = cell.ActualHeight;

      string targetFieldName = cell.FieldName;
      DataGridItemsHost itemsHost = dataGridContext.DataGridControl.ItemsHost as DataGridItemsHost;

      IScrollInfo scrollInfo = itemsHost as IScrollInfo;
      Debug.Assert( scrollInfo != null );

      // Fixed Cells are always visible
      if( !columnVirtualizationManager.FixedFieldNames.Contains( targetFieldName ) )
      {
        // The Cell must be measured/arranged for the BringIntoView to correctly reacts
        bool isCellLayouted = !this.ForceScrollingCellToLayout( cell );

        // Use the DataGridControl.Container attached property to assert a container is always returned even if the container is in FixedHeaders or FixedFooters
        FrameworkElement container = DataGridControl.GetContainer( cell.ParentRow );

        // Ensure to call UpdateLayout to be sure the offset returned are the correct ones.
        if( ( !isCellLayouted ) && ( container != null ) )
        {
          container.UpdateLayout();
        }

        double containerToCellsHostWidth = this.TranslatePoint( FixedCellPanel.EmptyPoint, container ).X;
        double viewportWidth = scrollInfo.ViewportWidth;
        double fixedColumnWidth = columnVirtualizationManager.FixedColumnsWidth;

        Point cellToScrollingCellsDecorator = cell.TranslatePoint( FixedCellPanel.EmptyPoint, this.ScrollingCellsDecorator );

        // If the Cell's left edge is not fully visible, but displays its full content in the Viewport because its width is still
        // greater than the Viewport, let the ScrollViewer process any events that would have requested a BringIntoView
        if( ( cellRect.Width - Math.Abs( cellToScrollingCellsDecorator.X ) ) >= ( viewportWidth - fixedColumnWidth ) )
        {
          // The cell can't be more into view horizontally, but ensure to bring into view the container vertically.
          if( container != null )
          {
            container.BringIntoView();
          }
          return true;
        }

        double scrollViewerDesiredOffset = 0;

        if( cellToScrollingCellsDecorator.X < 0 )
        {
          scrollViewerDesiredOffset = cellToScrollingCellsDecorator.X;
        }
        else
        {
          // The Cell's left edge is visible in the Viewport
          double cellRightEdgeOffset = ( cellToScrollingCellsDecorator.X + cellRect.Width );

          // Verify if the Cell's right edge is visible in the ViewPort
          Point cellToItemsHost = cell.TranslatePoint( new Point( cellRect.Width, 0 ), itemsHost );

          double rightSideOutOfViewOffset = ( cellToItemsHost.X - viewportWidth );

          // If the right edge is out of Viewport, ensure to bring into view the out of view part
          if( rightSideOutOfViewOffset > 0 )
          {
            // Ensure to bring into view the right edge, but we don't want to scroll to far and hide the left edge. 
            double avoidHiddingLeftEdgeCorrectedOffset = Math.Max( 0, ( rightSideOutOfViewOffset - cellToScrollingCellsDecorator.X ) );

            // The desired offset of the rectangle is the left edge of the Cell and since we specified the width, IScrollInfo.MakeVisible
            // will ensure to scroll horizontally so this rectangle is fully visible
            scrollViewerDesiredOffset = cellToItemsHost.X - cellRect.Width - avoidHiddingLeftEdgeCorrectedOffset;
          }
          else
          {
            // No need to precise a Rect to bring into view since the Cell is already fully visible. Only call BringIntoView on the container
            // to be sure it will be fully visible vertically.
            if( container != null )
            {
              container.BringIntoView();
            }
            return true;
          }
        }

        // We computed the desired offset according to the ScrollViewer.Content ( ItemsHost in the DataGridControl)
        // translate it to a point accoring to the FixedCellPanel to ensure to bring this point into view
        cellRect.X = itemsHost.TranslatePoint( new Point( scrollViewerDesiredOffset, 0 ), this ).X;
      }
      else
      {
        // We must consider the actual position of the Cell according to the FixedCellPanel in order to be sure we don't force
        // an horizontal change. The fixed Cells are always visible so there is no need for horizontal change.
        double desiredOffset = -1;

        // We always want the horizontal offset to change if Tab, Home, Left or Right is pressed.
        if( Keyboard.IsKeyDown( Key.Tab ) || Keyboard.IsKeyDown( Key.Home ) || Keyboard.IsKeyDown( Key.Left ) || Keyboard.IsKeyDown( Key.Right ) )
        {
          // Fixed Cells are not computed as part of the scrollable offset. Ensure to specify a rectangle that will force the horizontal offset to be 0.
          desiredOffset = -scrollInfo.HorizontalOffset;
        }
        else
        {
          desiredOffset = columnVirtualizationManager.FieldNameToOffset[ targetFieldName ];
        }

        // The Point returned by TranslatePoint will be according to the current layout, meaning the HorizontalOffset
        // will be included. So there won't be any horizontal scrolling when bringing a fixed Cell into view.
        cellRect.X = itemsHost.TranslatePoint( new Point( desiredOffset, 0 ), this ).X;
      }

      this.BringIntoView( cellRect );

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
        if( !m_parentRowCells.AlreadyUpdating )
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

    private DispatcherOperation m_clearUnusedCellsDispatcherOperation; // = null;
    private object m_virtualizingCellCollectionChangedDispatcherOperation;

    private enum FixedCellPanelFlags
    {
      CellsHostPrepared = 1,
      HasDataItem = 2,
      NeedToClearAllCreatedCells = 4,
      ZOrderDirty = 8,
      SplitterDragCanceled = 16,
      SplitterDragCompleted = 32,
    }
  }
}
