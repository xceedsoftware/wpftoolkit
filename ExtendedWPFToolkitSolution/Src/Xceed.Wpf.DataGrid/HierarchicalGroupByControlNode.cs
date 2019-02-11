/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Utils.Wpf.DragDrop;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  public class HierarchicalGroupByControlNode : ItemsControl, IDropTarget
  {
    #region CONSTRUCTORS

    static HierarchicalGroupByControlNode()
    {
      // Default binding to HierarchicalGroupByControlNode value
      FrameworkElementFactory staircaseFactory = new FrameworkElementFactory( typeof( StaircasePanel ) );
      ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate( staircaseFactory );
      RelativeSource ancestorSource = new RelativeSource( RelativeSourceMode.FindAncestor, typeof( HierarchicalGroupByControl ), 1 );

      Binding binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControlNode.ConnectionLineAlignmentProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLineAlignmentProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControlNode.ConnectionLineOffsetProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLineOffsetProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControlNode.ConnectionLinePenProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLinePenProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControlNode.StairHeightProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.StairHeightProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControlNode.StairSpacingProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = ancestorSource;
      staircaseFactory.SetBinding( StaircasePanel.StairSpacingProperty, binding );

      itemsPanelTemplate.Seal();

      ItemsControl.ItemsPanelProperty.OverrideMetadata( typeof( HierarchicalGroupByControlNode ), new FrameworkPropertyMetadata( itemsPanelTemplate ) );
      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( HierarchicalGroupByControlNode ), new FrameworkPropertyMetadata( new PropertyChangedCallback( HierarchicalGroupByControlNode.ParentGridControlChangedCallback ) ) );
      FocusableProperty.OverrideMetadata( typeof( HierarchicalGroupByControlNode ), new FrameworkPropertyMetadata( false ) );
    }

    #endregion

    #region AllowGroupingModification Property

    public static readonly DependencyProperty AllowGroupingModificationProperty =
        GroupByControl.AllowGroupingModificationProperty.AddOwner( typeof( HierarchicalGroupByControlNode ), new UIPropertyMetadata( true ) );

    public bool AllowGroupingModification
    {
      get
      {
        return ( bool )this.GetValue( HierarchicalGroupByControlNode.AllowGroupingModificationProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControlNode.AllowGroupingModificationProperty, value );
      }
    }

    #endregion AllowGroupingModification Property

    #region AllowSort Property

    public static readonly DependencyProperty AllowSortProperty =
        ColumnManagerRow.AllowSortProperty.AddOwner( typeof( HierarchicalGroupByControlNode ), new UIPropertyMetadata( true ) );

    public bool AllowSort
    {
      get
      {
        return ( bool )this.GetValue( HierarchicalGroupByControlNode.AllowSortProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControlNode.AllowSortProperty, value );
      }
    }

    #endregion AllowSort Property

    #region ConnectionLineAlignment Property

    public static readonly DependencyProperty ConnectionLineAlignmentProperty =
      StaircasePanel.ConnectionLineAlignmentProperty.AddOwner( typeof( HierarchicalGroupByControlNode ) );

    public ConnectionLineAlignment ConnectionLineAlignment
    {
      get
      {
        return ( ConnectionLineAlignment )this.GetValue( HierarchicalGroupByControlNode.ConnectionLineAlignmentProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControlNode.ConnectionLineAlignmentProperty, value );
      }
    }

    #endregion ConnectionLineAlignment Property

    #region ConnectionLineOffset Property

    public static readonly DependencyProperty ConnectionLineOffsetProperty =
      StaircasePanel.ConnectionLineOffsetProperty.AddOwner( typeof( HierarchicalGroupByControlNode ) );

    public double ConnectionLineOffset
    {
      get
      {
        return ( double )this.GetValue( HierarchicalGroupByControlNode.ConnectionLineOffsetProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControlNode.ConnectionLineOffsetProperty, value );
      }
    }

    #endregion ConnectionLineOffset Property

    #region ConnectionLinePen Property

    public static readonly DependencyProperty ConnectionLinePenProperty =
      StaircasePanel.ConnectionLinePenProperty.AddOwner( typeof( HierarchicalGroupByControlNode ) );

    public Pen ConnectionLinePen
    {
      get
      {
        return ( Pen )this.GetValue( HierarchicalGroupByControlNode.ConnectionLinePenProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControlNode.ConnectionLinePenProperty, value );
      }
    }

    #endregion ConnectionLinePen Property

    #region StairHeight Property

    public static readonly DependencyProperty StairHeightProperty =
      StaircasePanel.StairHeightProperty.AddOwner( typeof( HierarchicalGroupByControlNode ) );

    public double StairHeight
    {
      get
      {
        return ( double )this.GetValue( HierarchicalGroupByControlNode.StairHeightProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControlNode.StairHeightProperty, value );
      }
    }

    #endregion StairHeight Property

    #region StairSpacing Property

    public static readonly DependencyProperty StairSpacingProperty =
      StaircasePanel.StairSpacingProperty.AddOwner( typeof( HierarchicalGroupByControlNode ) );

    public double StairSpacing
    {
      get
      {
        return ( double )this.GetValue( HierarchicalGroupByControlNode.StairSpacingProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControlNode.StairSpacingProperty, value );
      }
    }

    #endregion StairSpacing Property

    #region NoGroupContent Property

    public static readonly DependencyProperty NoGroupContentProperty =
      GroupByControl.NoGroupContentProperty.AddOwner(
        typeof( HierarchicalGroupByControlNode ),
        new PropertyMetadata( "Drag a column header here to group by that column." ) );

    public object NoGroupContent
    {
      get
      {
        return this.GetValue( HierarchicalGroupByControlNode.NoGroupContentProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControlNode.NoGroupContentProperty, value );
      }
    }

    #endregion NoGroupContent Property

    #region Title

    public object Title
    {
      get
      {
        return ( object )GetValue( TitleProperty );
      }
      set
      {
        SetValue( TitleProperty, value );
      }
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register( "Title", typeof( object ), typeof( HierarchicalGroupByControlNode ) );

    #endregion

    // INTERNAL PROPERTIES

    #region DataGridContext Property

    internal DataGridContext DataGridContext
    {
      get
      {
        DataGridContext dataGridContext = this.DataContext as DataGridContext;

        if( dataGridContext != null )
          return dataGridContext;

        DetailConfiguration configuration = this.DetailConfiguration;

        if( configuration == null )
          return null;

        // Ensure to get a DataGridContext created with the same DetailConfiguration
        return HierarchicalGroupByControlNode.GetDataGridContextFromDetailConfiguration( configuration,
          DataGridControl.GetDataGridContext( this ) );
      }
    }

    #endregion

    #region DetailConfiguration Property

    internal DetailConfiguration DetailConfiguration
    {
      get
      {
        return this.DataContext as DetailConfiguration;
      }
    }

    #endregion

    #region Columns Property

    internal ColumnCollection Columns
    {
      get
      {
        ColumnCollection columnsCollection = null;
        DetailConfiguration detailConfiguration = this.DetailConfiguration;

        if( detailConfiguration != null )
        {
          columnsCollection = detailConfiguration.Columns;
        }
        else
        {
          DataGridContext dataGridContext = this.DataGridContext;

          if( dataGridContext != null )
            columnsCollection = dataGridContext.Columns;
        }
        Debug.Assert( ( columnsCollection != null ) || ( DesignerProperties.GetIsInDesignMode( this ) ) );
        return columnsCollection;
      }
    }

    #endregion

    #region GroupDescriptions Property

    internal ObservableCollection<GroupDescription> GroupDescriptions
    {
      get
      {
        ObservableCollection<GroupDescription> groupDescriptions = null;
        DetailConfiguration detailConfiguration = this.DetailConfiguration;

        if( detailConfiguration != null )
        {
          groupDescriptions = detailConfiguration.GroupDescriptions;
        }
        else
        {
          DataGridContext dataGridContext = this.DataGridContext;

          groupDescriptions = dataGridContext.Items.GroupDescriptions;
        }
        Debug.Assert( groupDescriptions != null );
        return groupDescriptions;
      }
    }

    #endregion

    #region GroupLevelDescriptions Property

    internal GroupLevelDescriptionCollection GroupLevelDescriptions
    {
      get
      {
        GroupLevelDescriptionCollection groupLevelDescriptions = null;
        DetailConfiguration detailConfiguration = this.DetailConfiguration;

        if( detailConfiguration != null )
        {
          groupLevelDescriptions = detailConfiguration.GroupLevelDescriptions;
        }
        else
        {
          DataGridContext dataGridContext = this.DataGridContext;

          groupLevelDescriptions = dataGridContext.GroupLevelDescriptions;
        }

        if( groupLevelDescriptions == null )
          throw new DataGridInternalException( "GroupLevelDescriptions cannot be null on " + typeof( HierarchicalGroupByControlNode ).Name + "." );

        return groupLevelDescriptions;
      }
    }

    #endregion

    #region SortDescriptions Property

    internal SortDescriptionCollection SortDescriptions
    {
      get
      {
        SortDescriptionCollection sortDescriptions = null;
        DetailConfiguration detailConfiguration = this.DetailConfiguration;

        if( detailConfiguration != null )
        {
          sortDescriptions = detailConfiguration.SortDescriptions;
        }
        else
        {
          DataGridContext dataGridContext = this.DataGridContext;

          sortDescriptions = dataGridContext.Items.SortDescriptions;
        }

        if( sortDescriptions == null )
          throw new DataGridInternalException( "GroupLevelDescriptions cannot be null on " + typeof( HierarchicalGroupByControlNode ).Name + "." );

        return sortDescriptions;
      }
    }

    #endregion

    #region PROTECTED METHODS

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( HierarchicalGroupByControlNode ) );
    }

    protected override DependencyObject GetContainerForItemOverride()
    {
      return new HierarchicalGroupByItem();
    }

    protected override bool IsItemItsOwnContainerOverride( object item )
    {
      return item is HierarchicalGroupByItem;
    }

    protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
    {
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl grid = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      base.PrepareContainerForItemOverride( element, item );

      if( grid != null )
      {
        HierarchicalGroupByItem groupByItem = ( HierarchicalGroupByItem )element;
        groupByItem.PrepareDefaultStyleKey( grid.GetView() );
      }
    }

    #endregion

    #region PRIVATE STATIC METHODS

    private static void ParentGridControlChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl parentDataGrid = e.NewValue as DataGridControl;
      HierarchicalGroupByControlNode groupByControl = ( HierarchicalGroupByControlNode )sender;

      if( parentDataGrid != null )
        groupByControl.PrepareDefaultStyleKey( parentDataGrid.GetView() );
    }

    private static DataGridContext GetDataGridContextFromDetailConfiguration(
    DetailConfiguration configuration,
    DataGridContext parentDataGridContext )
    {
      if( ( configuration == null ) || ( parentDataGridContext == null ) )
        return null;

      if( parentDataGridContext.SourceDetailConfiguration == configuration )
        return parentDataGridContext;

      foreach( DataGridContext childContext in parentDataGridContext.GetChildContexts() )
      {
        DataGridContext foundContext =
          HierarchicalGroupByControlNode.GetDataGridContextFromDetailConfiguration( configuration,
            childContext );

        if( foundContext != null )
          return foundContext;
      }

      return null;
    }

    #endregion

    #region INTERNAL METHODS

    internal bool IsGroupingModificationAllowed
    {
      get
      {
        HierarchicalGroupByControl hierarchicalGroupByControl = GroupingHelper.GetHierarchicalGroupByControl( this );

        // By default, we can since DataGridCollectionView.CanGroup always return true
        // but we rely on the HierarchicalGroupByControl.AllowGroupModification value
        bool allowGroupingModification = hierarchicalGroupByControl.AllowGroupingModification;
        if( allowGroupingModification == true )
        {
          DataGridContext dataGridContext = this.DataGridContext;

          Debug.Assert( dataGridContext != null );

          if( ( dataGridContext != null ) && ( dataGridContext.SourceDetailConfiguration == null ) )
          {
            allowGroupingModification = dataGridContext.Items.CanGroup;
          }
        }

        return allowGroupingModification;
      }
    }

    internal void ShowFarDropMark( ColumnManagerCell cell, RelativePoint mousePosition )
    {
      Debug.Assert( cell != null );
      if( cell == null )
        return;

      DataGridContext cellDataGridContext = DataGridControl.GetDataGridContext( cell );

      Debug.Assert( cellDataGridContext != null );
      if( cellDataGridContext == null )
        throw new DataGridInternalException( "DataGridContext cannot be null on ColumnManagerCell." );

      // We already have GroupLevelDescriptions for this level, we should show DropMark on the last HierarchicalGroupByItem
      if( cellDataGridContext.GroupLevelDescriptions.Count > 0 )
      {
        Debug.Assert( cellDataGridContext.GroupLevelDescriptions == this.GroupLevelDescriptions );

        if( cellDataGridContext.GroupLevelDescriptions != this.GroupLevelDescriptions )
          return;

        int lastIndex = this.GroupLevelDescriptions.Count - 1;

        // If there
        if( lastIndex > -1 )
        {
          HierarchicalGroupByItem hierarchicalGroupByItem = this.ItemContainerGenerator.ContainerFromItem( this.GroupLevelDescriptions[ lastIndex ] ) as HierarchicalGroupByItem;

          Debug.Assert( hierarchicalGroupByItem != null );
          if( hierarchicalGroupByItem == null )
            return;

          hierarchicalGroupByItem.ShowFarDropMark( mousePosition );
        }
        else
        {
          this.ShowFarDropMark( mousePosition );
        }
      }
      else
      {
        this.ShowFarDropMark( mousePosition );
      }
    }

    internal void ShowFarDropMark( RelativePoint mousePosition )
    {
      int itemsCount = this.Items.Count;
      if( itemsCount < 1 )
      {
        if( m_dropMarkAdorner == null )
        {
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

          DataGridControl grid = ( dataGridContext != null )
            ? dataGridContext.DataGridControl
            : null;

          Pen pen = UIViewBase.GetDropMarkPen( this );

          if( ( pen == null ) && ( grid != null ) )
          {
            UIViewBase uiViewBase = grid.GetView() as UIViewBase;
            pen = uiViewBase.DefaultDropMarkPen;
          }

          DropMarkOrientation orientation = UIViewBase.GetDropMarkOrientation( this );

          if( ( orientation == DropMarkOrientation.Default ) && ( grid != null ) )
          {
            UIViewBase uiViewBase = grid.GetView() as UIViewBase;

            orientation = uiViewBase.DefaultDropMarkOrientation;
          }

          m_dropMarkAdorner = new DropMarkAdorner( this, pen, orientation );

          AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );

          if( adornerLayer != null )
          {
            adornerLayer.Add( m_dropMarkAdorner );
          }
        }

        // We Only want the drop mark to be displayed at the end of the HierarchicalGroupByControlNode
        m_dropMarkAdorner.Alignment = DropMarkAlignment.Far;
      }
      else
      {
        HierarchicalGroupByItem hierarchicalGroupByItem = this.ItemContainerGenerator.ContainerFromIndex( itemsCount - 1 ) as HierarchicalGroupByItem;

        Debug.Assert( hierarchicalGroupByItem != null );

        GroupLevelDescription groupLevelDescription = hierarchicalGroupByItem.Content as GroupLevelDescription;

        Debug.Assert( groupLevelDescription != null );

        // Show Far DropMark only if not already grouped
        if( !this.Items.Contains( groupLevelDescription ) )
        {
          hierarchicalGroupByItem.ShowFarDropMark( mousePosition );
        }
      }
    }

    internal void HideFarDropMark()
    {
      int itemsCount = this.Items.Count;
      if( itemsCount > 0 )
      {
        HierarchicalGroupByItem hierarchicalGroupByItem = null;
        if( itemsCount > 1 )
        {
          // Hide the before last item's DropMark if any in case
          hierarchicalGroupByItem = this.ItemContainerGenerator.ContainerFromIndex( itemsCount - 2 ) as HierarchicalGroupByItem;

          Debug.Assert( hierarchicalGroupByItem != null );

          if( hierarchicalGroupByItem != null )
            hierarchicalGroupByItem.HideDropMark();
        }

        // Hide last item's DropMark if any
        hierarchicalGroupByItem = this.ItemContainerGenerator.ContainerFromIndex( itemsCount - 1 ) as HierarchicalGroupByItem;

        Debug.Assert( hierarchicalGroupByItem != null );

        if( hierarchicalGroupByItem != null )
          hierarchicalGroupByItem.HideDropMark();
      }

      if( m_dropMarkAdorner != null )
      {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );

        if( adornerLayer != null )
          adornerLayer.Remove( m_dropMarkAdorner );

        m_dropMarkAdorner = null;
      }
    }

    internal void HideFarDropMark( ColumnManagerCell cell )
    {
      Debug.Assert( cell != null );
      if( cell == null )
        return;

      DataGridContext cellDataGridContext = DataGridControl.GetDataGridContext( cell );

      Debug.Assert( cellDataGridContext != null );
      if( cellDataGridContext == null )
        throw new DataGridInternalException( "DataGridContext cannot be null on ColumnManagerCell." );

      this.HideFarDropMark();

      // We already have GroupLevelDescriptions for this level, we should show DropMark on the last HierarchicalGroupByItem
      if( cellDataGridContext.GroupLevelDescriptions.Count > 0 )
      {
        Debug.Assert( cellDataGridContext.GroupLevelDescriptions == this.GroupLevelDescriptions );

        if( cellDataGridContext.GroupLevelDescriptions != this.GroupLevelDescriptions )
          return;

        int lastIndex = this.GroupLevelDescriptions.Count - 1;

        if( lastIndex > -1 )
        {
          HierarchicalGroupByItem hierarchicalGroupByItem = this.ItemContainerGenerator.ContainerFromItem( this.GroupLevelDescriptions[ lastIndex ] ) as HierarchicalGroupByItem;

          Debug.Assert( hierarchicalGroupByItem != null );
          if( hierarchicalGroupByItem == null )
            return;

          hierarchicalGroupByItem.HideDropMark();
        }
      }
    }

    #endregion

    #region IDropTarget Members

    bool IDropTarget.CanDropElement( UIElement draggedElement, RelativePoint mousePosition )
    {
      ColumnManagerCell cell = null;
      HierarchicalGroupByItem hierarchicalGroupByItem = null;
      bool canDrop = this.AllowGroupingModification;

      if( canDrop )
      {
        cell = draggedElement as ColumnManagerCell;

        if( cell != null )
        {
          ColumnBase parentColumn = cell.ParentColumn;

          if( ( parentColumn == null ) || ( !parentColumn.AllowGroup ) )
            return false;

          // Check if already grouped using the cell's DataGridContext
          canDrop = !GroupingHelper.IsAlreadyGroupedBy( cell );

          if( canDrop )
          {

            // Get the HierarchicalGroupByControl for this HierarchicalGroupByControlNode
            HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

            if( parentGBC == null )
              throw new DataGridInternalException( "The hierarchical group-by control node must be rooted by a HierarchicalGroupByControl." );

            DataGridContext parentGBCDataGridContext = DataGridControl.GetDataGridContext( parentGBC );

            Debug.Assert( parentGBCDataGridContext != null );

            if( parentGBCDataGridContext.Items != null )
              canDrop = parentGBCDataGridContext.Items.CanGroup;

            if( canDrop )
            {
              canDrop = GroupingHelper.IsColumnManagerCellInDataGridContext( parentGBCDataGridContext, cell );

              if( canDrop == true )
              {
                canDrop = GroupingHelper.ValidateMaxGroupDescriptions( DataGridControl.GetDataGridContext( draggedElement ) );
              }
            }
          }
        }
        else
        {
          hierarchicalGroupByItem = draggedElement as HierarchicalGroupByItem;

          if( hierarchicalGroupByItem == null )
            canDrop = false;

          if( canDrop )
          {
            HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

            if( parentGBC == null )
              throw new DataGridInternalException( "The hierarchical group-by control node must be rooted by a HierarchicalGroupByControl." );

            // Try to get the HierarchicalGroupByControlNode in which this HierarchicalGroupByItem can be added using the parent HierarchicalGroupByControl => null it can't
            HierarchicalGroupByControlNode draggedHierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromHierarchicalGroupByItem( hierarchicalGroupByItem );

            if( draggedHierarchicalGroupByControlNode == null )
              canDrop = false;
          }
        }
      }

      bool returnedValue = ( ( cell != null ) || ( hierarchicalGroupByItem != null ) ) &&// ColumnManagerCell or HierarchicalGroupByItem
                           ( canDrop );


      return returnedValue;
    }

    void IDropTarget.DragEnter( UIElement draggedElement )
    {
    }

    void IDropTarget.DragOver( UIElement draggedElement, RelativePoint mousePosition )
    {
      ColumnManagerCell cell = draggedElement as ColumnManagerCell;

      if( cell != null )
      {
        HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

        if( parentGBC == null )
          throw new DataGridInternalException( "The hierarchical group-by control node must be rooted by a HierarchicalGroupByControl." );

        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromColumnManagerCell( cell );

        if( hierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "A HierarchicalGroupByControlNode must exist for every level." );

        hierarchicalGroupByControlNode.ShowFarDropMark( cell, mousePosition );
      }
      else
      {
        HierarchicalGroupByItem hierarchicalGroupByItem = draggedElement as HierarchicalGroupByItem;
        if( hierarchicalGroupByItem == null )
          return;

        HierarchicalGroupByControlNode draggedHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( hierarchicalGroupByItem );

        if( draggedHierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "draggedHierarchicalGroupByControlNode is null." );

        if( draggedHierarchicalGroupByControlNode.GroupLevelDescriptions == this.GroupLevelDescriptions )
        {
          this.ShowFarDropMark( mousePosition );
        }
        else
        {
          // This HierarchicalGroupByItem does not belong this parent HierarchicalGroupByControlNode, display the DropMark on the correct one
          HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

          if( parentGBC == null )
            throw new DataGridInternalException( "The hierarchical group-by control node must be rooted by a HierarchicalGroupByControl." );

          HierarchicalGroupByControlNode hierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromHierarchicalGroupByItem( hierarchicalGroupByItem );

          Debug.Assert( hierarchicalGroupByControlNode != null, "CanDrop should have returned false" );
          if( hierarchicalGroupByControlNode == null )
            throw new DataGridInternalException( "A HierarchicalGroupByControlNode must exist for every level." );

          hierarchicalGroupByControlNode.ShowFarDropMark( mousePosition );
        }
      }
    }

    void IDropTarget.DragLeave( UIElement draggedElement )
    {
      ColumnManagerCell cell = draggedElement as ColumnManagerCell;

      if( cell != null )
      {
        HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

        if( parentGBC == null )
          throw new DataGridInternalException( "The hierarchical group-by control node must be rooted by a HierarchicalGroupByControl." );

        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromColumnManagerCell( cell );

        if( hierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "A HierarchicalGroupByControlNode must exist for every level." );

        hierarchicalGroupByControlNode.HideFarDropMark( cell );
      }
      else
      {
        HierarchicalGroupByItem hierarchicalGroupByItem = draggedElement as HierarchicalGroupByItem;
        if( hierarchicalGroupByItem == null )
          return;

        HierarchicalGroupByControlNode draggedHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( hierarchicalGroupByItem );

        if( draggedHierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "draggedHierarchicalGroupByControlNode is null." );

        if( draggedHierarchicalGroupByControlNode.GroupLevelDescriptions == this.GroupLevelDescriptions )
        {
          this.HideFarDropMark();
        }
        else
        {
          // This HierarchicalGroupByItem does not belong this parent HierarchicalGroupByControlNode, display the DropMark on the correct one
          HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

          Debug.Assert( parentGBC != null );
          if( parentGBC == null )
            throw new DataGridInternalException( "The hierarchical group-by control node must be rooted by a HierarchicalGroupByControl" );

          HierarchicalGroupByControlNode hierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromHierarchicalGroupByItem( hierarchicalGroupByItem );

          Debug.Assert( hierarchicalGroupByControlNode != null, "CanDrop should have returned false" );
          if( hierarchicalGroupByControlNode == null )
            throw new DataGridInternalException( "A HierarchicalGroupByControlNode must exist for every level." );

          hierarchicalGroupByControlNode.HideFarDropMark();
        }
      }
    }

    void IDropTarget.Drop( UIElement draggedElement, RelativePoint mousePosition )
    {
      ColumnManagerCell cell = draggedElement as ColumnManagerCell;

      if( cell == null )
        return;

      HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

      if( parentGBC == null )
        throw new DataGridInternalException( "The hierarchical group-by control node must be rooted by a HierarchicalGroupByControl." );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl parentGrid = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      GroupingHelper.AppendNewGroupFromColumnManagerCell( cell, parentGrid );

      // Notify groups have changed for NoGroupContent    
      parentGBC.UpdateHasGroups();

      HierarchicalGroupByControlNode hierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromColumnManagerCell( cell );

      if( hierarchicalGroupByControlNode == null )
        return;

      hierarchicalGroupByControlNode.HideFarDropMark( cell );

      this.HideFarDropMark();
    }

    #endregion

    #region PRIVATE FIELDS

    private DropMarkAdorner m_dropMarkAdorner;

    #endregion
  }
}
