/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Xceed.Utils.Wpf.DragDrop;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_HierarchicalGroupByControlTreeView", Type = typeof( TreeView ) )]
  public class HierarchicalGroupByControl : Control, IDropTarget, INotifyPropertyChanged
  {
    #region CONSTRUCTORS

    static HierarchicalGroupByControl()
    {
      // This DefaultStyleKey will only be used in design-time.
      DefaultStyleKeyProperty.OverrideMetadata( typeof( HierarchicalGroupByControl ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( HierarchicalGroupByControl ) ) ) );

      // Default binding to HierarchicalGroupByControl value
      FrameworkElementFactory staircaseFactory = new FrameworkElementFactory( typeof( StaircasePanel ) );
      ItemsPanelTemplate itemsPanelTemplate = new ItemsPanelTemplate( staircaseFactory );

      Binding binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControl.ConnectionLineAlignmentProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = RelativeSource.Self;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLineAlignmentProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControl.ConnectionLineOffsetProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = RelativeSource.Self;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLineOffsetProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControl.ConnectionLinePenProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = RelativeSource.Self;
      staircaseFactory.SetBinding( StaircasePanel.ConnectionLinePenProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControl.StairHeightProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = RelativeSource.Self;
      staircaseFactory.SetBinding( StaircasePanel.StairHeightProperty, binding );

      binding = new Binding();
      binding.Path = new PropertyPath( HierarchicalGroupByControl.StairSpacingProperty );
      binding.Mode = BindingMode.OneWay;
      binding.RelativeSource = RelativeSource.Self;
      staircaseFactory.SetBinding( StaircasePanel.StairSpacingProperty, binding );

      itemsPanelTemplate.Seal();

      ItemsControl.ItemsPanelProperty.OverrideMetadata( typeof( HierarchicalGroupByControl ), new FrameworkPropertyMetadata( itemsPanelTemplate ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( HierarchicalGroupByControl ), new FrameworkPropertyMetadata( new PropertyChangedCallback( HierarchicalGroupByControl.ParentGridControlChangedCallback ) ) );
      FocusableProperty.OverrideMetadata( typeof( HierarchicalGroupByControl ), new FrameworkPropertyMetadata( false ) );

    }

    #endregion

    #region AllowGroupingModification Property

    public static readonly DependencyProperty AllowGroupingModificationProperty =
      GroupByControl.AllowGroupingModificationProperty.AddOwner( typeof( HierarchicalGroupByControl ), new UIPropertyMetadata( true ) );

    public bool AllowGroupingModification
    {
      get
      {
        return ( bool )this.GetValue( HierarchicalGroupByControl.AllowGroupingModificationProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControl.AllowGroupingModificationProperty, value );
      }
    }

    #endregion AllowGroupingModification Property

    #region AllowSort Property

    public static readonly DependencyProperty AllowSortProperty =
        ColumnManagerRow.AllowSortProperty.AddOwner( typeof( HierarchicalGroupByControl ), new UIPropertyMetadata( true ) );

    public bool AllowSort
    {
      get
      {
        return ( bool )this.GetValue( HierarchicalGroupByControl.AllowSortProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControl.AllowSortProperty, value );
      }
    }

    #endregion AllowSort Property

    #region ConnectionLineAlignment Property

    public static readonly DependencyProperty ConnectionLineAlignmentProperty =
      StaircasePanel.ConnectionLineAlignmentProperty.AddOwner( typeof( HierarchicalGroupByControl ) );

    public ConnectionLineAlignment ConnectionLineAlignment
    {
      get
      {
        return ( ConnectionLineAlignment )this.GetValue( HierarchicalGroupByControl.ConnectionLineAlignmentProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControl.ConnectionLineAlignmentProperty, value );
      }
    }

    #endregion ConnectionLineAlignment Property

    #region ConnectionLineOffset Property

    public static readonly DependencyProperty ConnectionLineOffsetProperty =
      StaircasePanel.ConnectionLineOffsetProperty.AddOwner( typeof( HierarchicalGroupByControl ) );

    public double ConnectionLineOffset
    {
      get
      {
        return ( double )this.GetValue( HierarchicalGroupByControl.ConnectionLineOffsetProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControl.ConnectionLineOffsetProperty, value );
      }
    }

    #endregion ConnectionLineOffset Property

    #region ConnectionLinePen Property

    public static readonly DependencyProperty ConnectionLinePenProperty =
      StaircasePanel.ConnectionLinePenProperty.AddOwner( typeof( HierarchicalGroupByControl ) );

    public Pen ConnectionLinePen
    {
      get
      {
        return ( Pen )this.GetValue( HierarchicalGroupByControl.ConnectionLinePenProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControl.ConnectionLinePenProperty, value );
      }
    }

    #endregion ConnectionLinePen Property

    #region StairHeight Property

    public static readonly DependencyProperty StairHeightProperty =
      StaircasePanel.StairHeightProperty.AddOwner( typeof( HierarchicalGroupByControl ) );

    public double StairHeight
    {
      get
      {
        return ( double )this.GetValue( HierarchicalGroupByControl.StairHeightProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControl.StairHeightProperty, value );
      }
    }

    #endregion StairHeight Property

    #region StairSpacing Property

    public static readonly DependencyProperty StairSpacingProperty =
      StaircasePanel.StairSpacingProperty.AddOwner( typeof( HierarchicalGroupByControl ) );

    public double StairSpacing
    {
      get
      {
        return ( double )this.GetValue( HierarchicalGroupByControl.StairSpacingProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControl.StairSpacingProperty, value );
      }
    }

    #endregion StairSpacing Property

    #region HasGroups

    public bool HasGroups
    {
      get
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext == null )
          return false;

        // We only display the NoGroupContent if the HierarchicalGroupByControl is at the master level
        if( dataGridContext.ParentDataGridContext != null )
          return false;

        // We look for GroupLevelDescription in the DataGridContext and all details
        return ( GroupingHelper.HasGroup( dataGridContext ) == false );
      }
    }

    #endregion

    #region NoGroupContent Property

    public static readonly DependencyProperty NoGroupContentProperty =
      GroupByControl.NoGroupContentProperty.AddOwner(
        typeof( HierarchicalGroupByControl ),
        new PropertyMetadata( "Drag a column header here to group by that column." ) );

    public object NoGroupContent
    {
      get
      {
        return this.GetValue( HierarchicalGroupByControl.NoGroupContentProperty );
      }
      set
      {
        this.SetValue( HierarchicalGroupByControl.NoGroupContentProperty, value );
      }
    }

    #endregion NoGroupContent Property

    #region PROTECTED INTERNAL METHODS

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( HierarchicalGroupByControl ) );
    }

    #endregion

    #region PRIVATE STATIC METHODS

    private static HierarchicalGroupByControlNode GetHierarchicalGroupByControlNode( UIElement element )
    {
      HierarchicalGroupByControlNode hierarchicalGroupByControlNode = element as HierarchicalGroupByControlNode;

      if( hierarchicalGroupByControlNode != null )
        return hierarchicalGroupByControlNode;

      int childCount = VisualTreeHelper.GetChildrenCount( element );

      for( int i = 0; i < childCount; i++ )
      {
        UIElement child = VisualTreeHelper.GetChild( element, i ) as UIElement;

        if( child != null )
          hierarchicalGroupByControlNode = HierarchicalGroupByControl.GetHierarchicalGroupByControlNode( child );

        if( hierarchicalGroupByControlNode != null )
          break;
      }

      return hierarchicalGroupByControlNode;
    }

    private static TreeViewItem GetTreeViewItemFromGroupLevelDescriptionCollection( TreeViewItem rootItem, GroupLevelDescriptionCollection groupLevelDescriptions )
    {
      TreeViewItem returned = null;
      Debug.Assert( rootItem != null );
      Debug.Assert( groupLevelDescriptions != null );

      if( rootItem == null )
        throw new DataGridInternalException( "rootItem is null." );

      foreach( object item in rootItem.Items )
      {
        TreeViewItem child = rootItem.ItemContainerGenerator.ContainerFromItem( item ) as TreeViewItem;

        // It may not be visible
        if( child == null )
          continue;

        DetailConfiguration detailConfiguration = child.Header as DetailConfiguration;

        if( detailConfiguration == null )
          throw new DataGridInternalException( "The item's DataContext must be a DetailConfiguration except for the top-most HierarchicalGroupByControl, which contains a DataGridContext." );

        if( groupLevelDescriptions == detailConfiguration.GroupLevelDescriptions )
        {
          returned = child;
          break;
        }

        returned = HierarchicalGroupByControl.GetTreeViewItemFromGroupLevelDescriptionCollection( child, groupLevelDescriptions );

        if( returned != null )
          break;
      }

      return returned;
    }

    private static TreeViewItem GetTreeViewItemFromGroupLevelDescription( TreeViewItem rootItem, GroupLevelDescription groupLevelDescription )
    {
      TreeViewItem returned = null;
      Debug.Assert( rootItem != null );
      Debug.Assert( groupLevelDescription != null );

      if( rootItem == null )
        throw new DataGridInternalException( "rootItem is null." );

      foreach( object item in rootItem.Items )
      {
        TreeViewItem child = rootItem.ItemContainerGenerator.ContainerFromItem( item ) as TreeViewItem;

        Debug.Assert( child != null );
        if( child == null )
          throw new DataGridInternalException( "An item does not contain a valid item." );

        DetailConfiguration detailConfiguration = child.Header as DetailConfiguration;

        if( detailConfiguration == null )
          throw new DataGridInternalException( "An item's DataContext must be a DetailConfiguration except for the top-most HierarchicalGroupByControl, which contains a DataGridContext." );

        if( detailConfiguration.GroupLevelDescriptions.Contains( groupLevelDescription ) )
        {
          returned = child;
          break;
        }

        returned = HierarchicalGroupByControl.GetTreeViewItemFromGroupLevelDescription( child, groupLevelDescription );

        if( returned != null )
          break;
      }

      return returned;
    }

    private static void ParentGridControlChangedCallback( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      DataGridControl parentDataGrid = e.NewValue as DataGridControl;
      HierarchicalGroupByControl groupByControl = ( HierarchicalGroupByControl )sender;

      if( parentDataGrid != null )
        groupByControl.PrepareDefaultStyleKey( parentDataGrid.GetView() );
    }

    #endregion

    #region INTERNAL METHODS

    internal HierarchicalGroupByControlNode GetHierarchicalGroupByControlNodeFromColumnManagerCell( ColumnManagerCell cell )
    {
      Debug.Assert( cell != null );
      if( cell == null )
        return null;

      DataGridContext cellDataGridContext = DataGridControl.GetDataGridContext( cell );

      if( cellDataGridContext == null )
        throw new DataGridInternalException( "A DataGridContext cannot be null on ColumnManagerCell." );

      TreeView treeView = this.GetTemplateChild( @"PART_HierarchicalGroupByControlTreeView" ) as TreeView;

      if( treeView == null )
        return null;

      if( treeView.Items.Count == 0 )
        throw new DataGridInternalException( "The HierarchicalGroupByControl should contain at least one grouping level." );

      // The first item is always the top level HierarchicalGroupByControlNode
      TreeViewItem rootItem = treeView.ItemContainerGenerator.ContainerFromItem( treeView.Items[ 0 ] ) as TreeViewItem;

      // It may not be visible
      if( rootItem == null )
        return null;

      TreeViewItem dropMarkedTreeViewItem = null;

      DataGridContext rootDataGridContext = rootItem.Header as DataGridContext;

      if( ( rootDataGridContext != null ) && ( rootDataGridContext.GroupLevelDescriptions == cellDataGridContext.GroupLevelDescriptions ) )
      {
        dropMarkedTreeViewItem = rootItem;
      }
      else
      {
        GroupLevelDescriptionCollection groupLevelDescriptions = cellDataGridContext.GroupLevelDescriptions;

        dropMarkedTreeViewItem = HierarchicalGroupByControl.GetTreeViewItemFromGroupLevelDescriptionCollection( rootItem, groupLevelDescriptions );
      }

      // If null, it means the cell does not belong to this detail
      if( dropMarkedTreeViewItem == null )
        return null;

      ContentPresenter treeViewItemHeader = dropMarkedTreeViewItem.Template.FindName( "PART_Header", dropMarkedTreeViewItem ) as ContentPresenter;

      // It may not be visible
      if( treeViewItemHeader == null )
        return null;

      HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByControl.GetHierarchicalGroupByControlNode( treeViewItemHeader );

      return hierarchicalGroupByControlNode;
    }

    internal HierarchicalGroupByControlNode GetHierarchicalGroupByControlNodeFromHierarchicalGroupByItem( HierarchicalGroupByItem hierarchicalGroupByItem )
    {
      Debug.Assert( hierarchicalGroupByItem != null );
      if( hierarchicalGroupByItem == null )
        return null;

      TreeView treeView = this.GetTemplateChild( @"PART_HierarchicalGroupByControlTreeView" ) as TreeView;

      if( treeView == null )
        return null;

      if( treeView.Items.Count == 0 )
        throw new DataGridInternalException( "The HierarchicalGroupByControl should contain at least one grouping level." );

      // The first item is always the top level HierarchicalGroupByControlNode
      TreeViewItem rootItem = treeView.ItemContainerGenerator.ContainerFromItem( treeView.Items[ 0 ] ) as TreeViewItem;

      if( rootItem == null )
        throw new DataGridInternalException( "The root item is null." );

      GroupLevelDescription detailGroupLevelDescription = hierarchicalGroupByItem.Content as GroupLevelDescription;

      Debug.Assert( detailGroupLevelDescription != null );

      TreeViewItem dropMarkedTreeViewItem = null;

      DataGridContext rootDataGridContext = rootItem.Header as DataGridContext;

      Debug.Assert( rootDataGridContext != null );

      if( rootDataGridContext.GroupLevelDescriptions.Contains( detailGroupLevelDescription ) )
      {
        dropMarkedTreeViewItem = rootItem;
      }
      else
      {
        dropMarkedTreeViewItem = HierarchicalGroupByControl.GetTreeViewItemFromGroupLevelDescription( rootItem, detailGroupLevelDescription );
      }


      // If null, it means the cell does not belong to this detail
      if( dropMarkedTreeViewItem == null )
        return null;

      ContentPresenter treeViewItemHeader = dropMarkedTreeViewItem.Template.FindName( "PART_Header", dropMarkedTreeViewItem ) as ContentPresenter;

      Debug.Assert( treeViewItemHeader != null );
      if( treeViewItemHeader == null )
        throw new DataGridInternalException( "An error occurred while retrieving the PART_Header template part of an item containing a HierarchicalGroupByControlNode." );

      HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByControl.GetHierarchicalGroupByControlNode( treeViewItemHeader );

      return hierarchicalGroupByControlNode;
    }

    #endregion

    #region IDropTarget Members


    bool IDropTarget.CanDropElement( UIElement draggedElement, RelativePoint mousePosition )
    {
      bool canDrop = this.AllowGroupingModification;

      ColumnManagerCell cell = null;
      HierarchicalGroupByItem hierarchicalGroupByItem = null;

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
            DataGridContext thisDataGridContext = DataGridControl.GetDataGridContext( this );

            if( thisDataGridContext.Items != null )
              canDrop = thisDataGridContext.Items.CanGroup;

            if( canDrop )
            {
              canDrop = GroupingHelper.IsColumnManagerCellInDataGridContext( thisDataGridContext, cell );

              if( canDrop == true )
                canDrop = GroupingHelper.ValidateMaxGroupDescriptions( DataGridControl.GetDataGridContext( draggedElement ) );
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
            // Try to get the HierarchicalGroupByControlNode in which this HierarchicalGroupByItem can be added using the parent HierarchicalGroupByControl => null it can't
            HierarchicalGroupByControlNode draggedHierarchicalGroupByControlNode = this.GetHierarchicalGroupByControlNodeFromHierarchicalGroupByItem( hierarchicalGroupByItem );

            if( draggedHierarchicalGroupByControlNode == null )
              canDrop = false;
          }
        }
      }

      bool returnedValue = ( ( cell != null ) || ( hierarchicalGroupByItem != null ) ) && // ColumnManagerCell or HierarchicalGroupByItem 
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
        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = this.GetHierarchicalGroupByControlNodeFromColumnManagerCell( cell );

        // It may not be visible
        if( hierarchicalGroupByControlNode != null )
          hierarchicalGroupByControlNode.ShowFarDropMark( cell, mousePosition );
      }
      else
      {
        HierarchicalGroupByItem hierarchicalGroupByItem = draggedElement as HierarchicalGroupByItem;
        if( hierarchicalGroupByItem == null )
          return;

        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = this.GetHierarchicalGroupByControlNodeFromHierarchicalGroupByItem( hierarchicalGroupByItem );

        Debug.Assert( hierarchicalGroupByControlNode != null, "CanDrop should have returned false" );
        if( hierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "A HierarchicalGroupByControlNode must exist for every level." );

        hierarchicalGroupByControlNode.ShowFarDropMark( mousePosition );
      }
    }

    void IDropTarget.DragLeave( UIElement draggedElement )
    {
      ColumnManagerCell cell = draggedElement as ColumnManagerCell;

      if( cell != null )
      {
        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = this.GetHierarchicalGroupByControlNodeFromColumnManagerCell( cell );

        // It may not be visible
        if( hierarchicalGroupByControlNode != null )
          hierarchicalGroupByControlNode.HideFarDropMark();
      }
      else
      {
        HierarchicalGroupByItem hierarchicalGroupByItem = draggedElement as HierarchicalGroupByItem;
        if( hierarchicalGroupByItem == null )
          return;

        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = this.GetHierarchicalGroupByControlNodeFromHierarchicalGroupByItem( hierarchicalGroupByItem );

        Debug.Assert( hierarchicalGroupByControlNode != null, "CanDrop should have returned false" );
        if( hierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "A HierarchicalGroupByControlNode must exist for every level." );

        hierarchicalGroupByControlNode.HideFarDropMark();
      }
    }

    void IDropTarget.Drop( UIElement draggedElement, RelativePoint mousePosition )
    {
      ColumnManagerCell cell = draggedElement as ColumnManagerCell;

      if( cell == null )
        return;

      HierarchicalGroupByControlNode hierarchicalGroupByControlNode = this.GetHierarchicalGroupByControlNodeFromColumnManagerCell( cell );

      // It may not be visible
      if( hierarchicalGroupByControlNode != null )
        hierarchicalGroupByControlNode.HideFarDropMark( cell );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl parentGrid = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      GroupingHelper.AppendNewGroupFromColumnManagerCell( cell, parentGrid );

      // Notify groups have changed for NoGroupContent    
      this.UpdateHasGroups();
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    internal void UpdateHasGroups()
    {
      this.OnPropertyChanged( "HasGroups" );
    }

    #endregion
  }
}
