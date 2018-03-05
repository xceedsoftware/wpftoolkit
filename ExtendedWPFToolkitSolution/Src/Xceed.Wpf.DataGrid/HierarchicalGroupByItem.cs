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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Utils.Wpf;
using Xceed.Utils.Wpf.DragDrop;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  public class HierarchicalGroupByItem : ButtonBase, INotifyPropertyChanged, IDropTarget
  {
    #region CONSTRUCTORS

    static HierarchicalGroupByItem()
    {
      // This DefaultStyleKey will only be used in design-time.
      DefaultStyleKeyProperty.OverrideMetadata( typeof( HierarchicalGroupByItem ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( HierarchicalGroupByItem ) ) ) );

      HierarchicalGroupByItem.IsBeingDraggedProperty = HierarchicalGroupByItem.IsBeingDraggedPropertyKey.DependencyProperty;

      FocusableProperty.OverrideMetadata( typeof( HierarchicalGroupByItem ), new FrameworkPropertyMetadata( false ) );
    }

    #endregion

    #region IsBeingDragged Read-Only Property

    private static readonly DependencyPropertyKey IsBeingDraggedPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsBeingDragged", typeof( bool ), typeof( HierarchicalGroupByItem ), new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsBeingDraggedProperty;

    public bool IsBeingDragged
    {
      get
      {
        return ( bool )this.GetValue( HierarchicalGroupByItem.IsBeingDraggedProperty );
      }
    }

    private void SetIsBeingDragged( bool value )
    {
      this.SetValue( HierarchicalGroupByItem.IsBeingDraggedPropertyKey, value );
    }

    #endregion IsBeingDragged Read-Only Property

    #region ParentColumns Property

    internal ColumnCollection ParentColumns
    {
      get
      {
        ColumnCollection columnCollection = null;

        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        Debug.Assert( ( hierarchicalGroupByControlNode != null ) || ( DesignerProperties.GetIsInDesignMode( this ) ) );

        if( hierarchicalGroupByControlNode != null )
        {
          columnCollection = hierarchicalGroupByControlNode.Columns;
        }

        if( ( columnCollection == null ) && ( !DesignerProperties.GetIsInDesignMode( this ) ) )
          throw new DataGridInternalException( "The " + typeof( HierarchicalGroupByItem ).Name + "'s ParentColumns cannot be null." );

        return columnCollection;
      }
    }

    #endregion

    #region ParentGroupDescriptions Property

    internal ObservableCollection<GroupDescription> ParentGroupDescriptions
    {
      get
      {
        ObservableCollection<GroupDescription> groupDescriptions = null;
        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        Debug.Assert( hierarchicalGroupByControlNode != null );
        if( hierarchicalGroupByControlNode != null )
        {
          groupDescriptions = hierarchicalGroupByControlNode.GroupDescriptions;
        }

        if( groupDescriptions == null )
          throw new DataGridInternalException( "The " + typeof( HierarchicalGroupByItem ).Name + "'s ParentGroupDescriptions cannot be null." );

        return groupDescriptions;
      }
    }

    #endregion

    #region ParentGroupLevelDescriptions Property

    private GroupLevelDescriptionCollection ParentGroupLevelDescriptions
    {
      get
      {
        GroupLevelDescriptionCollection groupLevelDescriptions = null;
        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        if( hierarchicalGroupByControlNode != null )
        {
          groupLevelDescriptions = hierarchicalGroupByControlNode.GroupLevelDescriptions;
        }
        else
        {
          DataGridContext dataGridContext = this.ParentDataGridContext;
          if( dataGridContext == null )
            throw new DataGridInternalException( "The " + typeof( HierarchicalGroupByItem ).Name + "'s DataGridContext cannot be null." );
          groupLevelDescriptions = dataGridContext.GroupLevelDescriptions;
        }

        if( groupLevelDescriptions == null )
          throw new DataGridInternalException( "The " + typeof( HierarchicalGroupByItem ).Name + "'s ParentGroupLevelDescriptions cannot be null." );

        return groupLevelDescriptions;
      }
    }

    #endregion

    #region ParentSortDescriptions Property

    private SortDescriptionCollection ParentSortDescriptions
    {
      get
      {
        SortDescriptionCollection sortDescriptions = null;

        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        if( hierarchicalGroupByControlNode != null )
        {
          sortDescriptions = hierarchicalGroupByControlNode.SortDescriptions;
        }
        else
        {
          DataGridContext dataGridContext = this.ParentDataGridContext;
          if( dataGridContext == null )
            throw new DataGridInternalException( "The " + typeof( HierarchicalGroupByItem ).Name + "'s DataGridContext cannot be null." );
          sortDescriptions = dataGridContext.Items.SortDescriptions;
        }

        if( sortDescriptions == null )
          throw new DataGridInternalException( "The " + typeof( HierarchicalGroupByItem ).Name + "'s ParentSortDescriptions cannot be null." );

        return sortDescriptions;
      }
    }

    #endregion

    #region SortDirection Property

    // Only used to bind between Column and us, but we don't want to expose it publicly
    private static readonly DependencyProperty SortDirectionInternalProperty =
        DependencyProperty.Register( "SortDirectionInternal", typeof( SortDirection ), typeof( HierarchicalGroupByItem ), new PropertyMetadata( SortDirection.None, new PropertyChangedCallback( HierarchicalGroupByItem.OnSortDirectionInternalChanged ) ) );

    public SortDirection SortDirection
    {
      get
      {
        return ( SortDirection )this.GetValue( HierarchicalGroupByItem.SortDirectionInternalProperty );
      }
    }

    private static void OnSortDirectionInternalChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      HierarchicalGroupByItem groupByItem = ( HierarchicalGroupByItem )sender;
      groupByItem.OnPropertyChanged( new PropertyChangedEventArgs( "SortDirection" ) );
    }

    #endregion SortDirection Property

    #region Drag & Drop Manager METHODS

    private void SetupDragManager()
    {
      // We do not support DragDrop when there are no AdornerLayer because there wouldn't 
      // be any visual feedback for the operation.
      if( AdornerLayer.GetAdornerLayer( this ) == null )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl dataGridControl = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      // Can be null in design-time (edition of a style TargetType HierarchicalGroupByItem ).
      if( dataGridControl == null )
        return;

      Debug.Assert( m_dragSourceManager == null, "There might be problems when there is already a m_dragSourceManager." );

      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.PropertyChanged -= new PropertyChangedEventHandler( DragSourceManager_PropertyChanged );
        m_dragSourceManager.DragOutsideQueryCursor -= new QueryCursorEventHandler( DragSourceManager_DragOutsideQueryCursor );
        m_dragSourceManager.DroppedOutside -= new EventHandler( DragSourceManager_DroppedOutside );
      }

      // The DataGridControl's AdornerDecoratorForDragAndDrop must be used for dragging in order to include the 
      // RenderTransform the DataGridControl may performs. This AdornerDecorator is defined in the ControlTemplate
      // as PART_DragDropAdornerDecorator
      if( ( dataGridControl.DragDropAdornerDecorator != null )
          && ( dataGridControl.DragDropAdornerDecorator.AdornerLayer != null ) )
      {
        m_dragSourceManager = new DragSourceManager( this, dataGridControl.DragDropAdornerDecorator.AdornerLayer, dataGridControl );
      }
      else
      {
        System.Diagnostics.Debug.Assert( false, "The drag and drop functionnality won't be fully working properly: PART_DragDropAdornerDecorator was not found" );
        m_dragSourceManager = new DragSourceManager( this, null, dataGridControl );
      }

      m_dragSourceManager.PropertyChanged += new PropertyChangedEventHandler( DragSourceManager_PropertyChanged );
      m_dragSourceManager.DragOutsideQueryCursor += new QueryCursorEventHandler( DragSourceManager_DragOutsideQueryCursor );
      m_dragSourceManager.DroppedOutside += new EventHandler( DragSourceManager_DroppedOutside );
    }

    private void DragSourceManager_DroppedOutside( object sender, EventArgs e )
    {
      HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

      if( hierarchicalGroupByControlNode == null )
        throw new DataGridInternalException( "hierarchicalGroupByControlNode is null." );

      bool allowGroupingModification = hierarchicalGroupByControlNode.IsGroupingModificationAllowed;

      if( !allowGroupingModification )
        return;

      ObservableCollection<GroupDescription> groupDescriptions = this.ParentGroupDescriptions;

      if( groupDescriptions != null )
      {
        // Get the HierarchicalGroupByControl before removing us from it
        HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

        GroupLevelDescription groupLevelDescription = this.Content as GroupLevelDescription;

        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        DataGridControl parentDataGridControl = ( dataGridContext != null )
          ? dataGridContext.DataGridControl
          : null;

        GroupingHelper.RemoveGroupDescription( groupDescriptions, groupLevelDescription, parentDataGridControl );

        // Notify groups have changed for NoGroupContent    
        parentGBC.UpdateHasGroups();

        // Update the HasGroups property
        Debug.Assert( parentGBC != null );
        if( parentGBC == null )
          throw new DataGridInternalException( "The hierarchical group-by item must be rooted by a HierarchicalGroupByControl." );
      }
    }

    private void DragSourceManager_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if( e.PropertyName == "IsDragging" )
      {
        this.SetIsBeingDragged( m_dragSourceManager.IsDragging );
      }
    }

    private void DragSourceManager_DragOutsideQueryCursor( object sender, QueryCursorEventArgs e )
    {
      HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

      if( ( hierarchicalGroupByControlNode == null )
        || ( !hierarchicalGroupByControlNode.IsGroupingModificationAllowed ) )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( ( dataGridContext != null ) && ( dataGridContext.DataGridControl != null ) )
      {
        UIViewBase uiViewBase = dataGridContext.DataGridControl.GetView() as UIViewBase;

        e.Cursor = ( uiViewBase != null )
          ? uiViewBase.RemovingGroupCursor
          : UIViewBase.DefaultGroupDraggedOutsideCursor;
      }
      else
      {
        e.Cursor = UIViewBase.DefaultGroupDraggedOutsideCursor;
      }
    }

    protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      if( this.CaptureMouse() )
      {
        if( m_dragSourceManager != null )
        {
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

          if( dataGridContext != null )
          {
            // Update the DropOutsideCursor since it is defined on the View
            UIViewBase uiViewBase = dataGridContext.DataGridControl.GetView() as UIViewBase;

            m_dragSourceManager.DropOutsideCursor = ( uiViewBase != null )
             ? uiViewBase.RemovingGroupCursor
             : UIViewBase.DefaultGroupDraggedOutsideCursor;
          }

          m_dragSourceManager.DragStart( e );
        }

        e.Handled = true;
      }

      base.OnMouseLeftButtonDown( e );
    }

    protected override void OnMouseMove( MouseEventArgs e )
    {
      if( ( this.IsMouseCaptured ) && ( e.LeftButton == MouseButtonState.Pressed ) )
      {
        if( m_dragSourceManager != null )
        {
          m_dragSourceManager.DragMove( e );
        }
      }

      base.OnMouseMove( e );
    }

    protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      bool isMouseCaptured = this.IsMouseCaptured;
      bool isPressed = this.IsPressed;

      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.Drop( e );
      }

      if( isMouseCaptured )
      {
        this.ReleaseMouseCapture();

        bool click = isPressed;

        if( click )
        {
          // DataGridCollectionView always return true for CanSort
          bool allowSort = true;

          // Use the ParentDataGridContext to be sure to get a
          // DataGridContext of the correct detail level since
          // all the HierarchicalGroupByItem will share the same DataGridContext
          // which is the one of the level where the HierarchicalGroupByControl
          // is located
          DataGridContext dataGridContext = this.ParentDataGridContext;

          if( ( dataGridContext != null ) && ( dataGridContext.SourceDetailConfiguration == null ) )
          {
            allowSort = dataGridContext.Items.CanSort;
          }

          if( allowSort )
          {
            ColumnCollection columns = this.ParentColumns;
            GroupLevelDescription groupInfo = this.Content as GroupLevelDescription;

            Debug.Assert( ( columns != null ) && ( groupInfo != null ) );

            if( ( columns != null ) && ( groupInfo != null ) )
            {
              ColumnBase column = columns[ groupInfo.FieldName ];

              if( ( column != null ) && ( column.AllowSort ) )
              {
                bool shiftUnpressed = ( ( Keyboard.Modifiers & ModifierKeys.Shift ) != ModifierKeys.Shift );

                var toggleColumnSort = new HierarchicalGroupByItemToggleColumnSortCommand( this );

                if( toggleColumnSort.CanExecute( column, shiftUnpressed ) )
                {
                  toggleColumnSort.Execute( column, shiftUnpressed );
                }

                e.Handled = true;
              }
            }
          }
        }
      }

      base.OnMouseLeftButtonUp( e );
    }

    protected override void OnLostMouseCapture( MouseEventArgs e )
    {
      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.DragCancel( e );
      }

      base.OnLostMouseCapture( e );
    }

    internal void ShowFarDropMark( RelativePoint mousePosition )
    {
      this.ShowDropMark( mousePosition, DropMarkAlignment.Far, true );
    }

    private void ShowDropMark( RelativePoint mousePosition )
    {
      this.ShowDropMark( mousePosition, DropMarkAlignment.Far, false );
    }

    private void ShowDropMark( RelativePoint mousePosition, DropMarkAlignment defaultAlignment, bool forceDefaultAlignment )
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

        DropMarkOrientation orientation = Xceed.Wpf.DataGrid.Views.UIViewBase.GetDropMarkOrientation( this );

        if( ( orientation == DropMarkOrientation.Default ) && ( grid != null ) )
        {
          UIViewBase uiViewBase = grid.GetView() as UIViewBase;

          orientation = uiViewBase.DefaultDropMarkOrientation;
        }

        m_dropMarkAdorner = new DropMarkAdorner( this, pen, orientation );

        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );

        if( adornerLayer != null )
          adornerLayer.Add( m_dropMarkAdorner );
      }

      if( forceDefaultAlignment )
      {
        m_dropMarkAdorner.Alignment = defaultAlignment;
      }
      else
      {
        m_dropMarkAdorner.UpdateAlignment( mousePosition );
      }
    }

    internal void HideDropMark()
    {
      if( m_dropMarkAdorner != null )
      {
        AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer( this );

        if( adornerLayer != null )
          adornerLayer.Remove( m_dropMarkAdorner );

        m_dropMarkAdorner = null;
      }
    }

    #endregion

    #region INotifyPropertyChanged Members

    protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
    {
      if( this.PropertyChanged != null )
        this.PropertyChanged( this, e );
    }

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region PROTECTED METHDOS

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.InitSortDirection();
      this.SetupDragManager();
    }

    #endregion

    #region PROTECTED INTERNAL METHODS

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( HierarchicalGroupByItem ) );
    }

    #endregion

    #region PRIVATE METHODS

    private void InitSortDirection()
    {
      ColumnCollection columns = this.ParentColumns;
      GroupLevelDescription groupInfo = this.Content as GroupLevelDescription;

      Debug.Assert( ( columns != null ) && ( groupInfo != null ) || ( DesignerProperties.GetIsInDesignMode( this ) ) );
      if( ( columns != null ) && ( groupInfo != null ) )
      {
        ColumnBase column = columns[ groupInfo.FieldName ];

        if( column != null )
        {
          Binding sortBinding = new Binding();
          sortBinding.Path = new PropertyPath( ColumnBase.SortDirectionProperty );
          sortBinding.Mode = BindingMode.OneWay;
          sortBinding.NotifyOnSourceUpdated = true;
          sortBinding.Source = column;
          sortBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

          BindingOperations.SetBinding( this, HierarchicalGroupByItem.SortDirectionInternalProperty, sortBinding );
        }
      }
    }

    #endregion

    #region INTERNAL STATIC METHODS

    internal static HierarchicalGroupByControlNode GetParentHierarchicalGroupByControlNode( UIElement element )
    {
      DependencyObject parent = TreeHelper.GetParent( element );

      while( parent != null )
      {
        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = parent as HierarchicalGroupByControlNode;
        if( hierarchicalGroupByControlNode != null )
          break;

        parent = TreeHelper.GetParent( parent );
      }

      return parent as HierarchicalGroupByControlNode;
    }

    #endregion

    #region ParentDataGridContext Property

    internal DataGridContext ParentDataGridContext
    {
      get
      {
        DataGridContext dataGridContext = null;
        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        if( hierarchicalGroupByControlNode != null )
          dataGridContext = hierarchicalGroupByControlNode.DataGridContext;

        return dataGridContext;
      }
    }

    #endregion

    #region ParentDetailConfiguration Property

    internal DetailConfiguration ParentDetailConfiguration
    {
      get
      {
        DetailConfiguration detailConfiguration = null;
        HierarchicalGroupByControlNode hierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        if( hierarchicalGroupByControlNode != null )
          detailConfiguration = hierarchicalGroupByControlNode.DetailConfiguration;

        // Can return null if the master level is reached: the DataContext
        // will be a DataGridContext instead of a DetailConfiguration and
        // the properties must be fetched via the DataGridContext instead
        return detailConfiguration;
      }
    }

    #endregion

    #region IDropTarget Members

    bool IDropTarget.CanDropElement( UIElement draggedElement, RelativePoint mousePosition )
    {
      bool canDrop = true;

      // Check if this HierarchicalGroupByItem parent HierarchicalGroupByControlNode allows grouping modifications, default yes
      HierarchicalGroupByControlNode parentHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

      if( parentHierarchicalGroupByControlNode != null )
        canDrop = parentHierarchicalGroupByControlNode.IsGroupingModificationAllowed;

      ColumnManagerCell cell = draggedElement as ColumnManagerCell;

      HierarchicalGroupByItem hierarchicalGroupByItem = null;

      if( canDrop )
      {
        if( cell != null )
        {
          ColumnBase parentColumn = cell.ParentColumn;

          if( ( parentColumn == null ) || ( !parentColumn.AllowGroup ) )
            return false;

          // Check if already grouped using the cell's DataGridContext
          canDrop = !GroupingHelper.IsAlreadyGroupedBy( cell );

          if( canDrop )
          {
            // Get the HierarchicalGroupByControl for this HierarchicalGroupByItem 
            HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( parentHierarchicalGroupByControlNode );

            if( parentGBC == null )
              throw new DataGridInternalException( "The hierarchical group-by item must be rooted by a HierarchicalGroupByControl." );

            DataGridContext parentGBCDataGridContext = DataGridControl.GetDataGridContext( parentGBC );

            if( parentGBCDataGridContext.Items != null )
              canDrop = parentGBCDataGridContext.Items.CanGroup;

            if( canDrop )
            {
              canDrop = GroupingHelper.IsColumnManagerCellInDataGridContext( parentGBCDataGridContext, cell );

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
            HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( parentHierarchicalGroupByControlNode );

            if( parentGBC == null )
              throw new DataGridInternalException( "The hierarchical group-by item must be rooted by a HierarchicalGroupByControl." );

            // Try to get the HierarchicalGroupByControlNode in which this HierarchicalGroupByItem can be added using the parent HierarchicalGroupByControl => null it can't
            HierarchicalGroupByControlNode draggedHierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromHierarchicalGroupByItem( hierarchicalGroupByItem );

            if( draggedHierarchicalGroupByControlNode == null )
              canDrop = false;
          }
        }
      }


      bool returnedValue = ( ( cell != null ) || ( hierarchicalGroupByItem != null ) ) && // ColumnManagerCell or HierarchicalGroupByItem 
                           ( draggedElement != this ) &&
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
        DataGridContext draggedCellDataGridContext = DataGridControl.GetDataGridContext( cell );

        HierarchicalGroupByControlNode draggedOverHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        if( draggedCellDataGridContext == null )
          throw new DataGridInternalException( "A ColumnManagerCell must have a DataGridContext." );

        if( draggedOverHierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "draggedOverHierarchicalGroupByControlNode is null." );

        if( draggedOverHierarchicalGroupByControlNode.GroupLevelDescriptions == draggedCellDataGridContext.GroupLevelDescriptions )
        {
          this.ShowDropMark( mousePosition );
        }
        else
        {
          // This ColumnManagerCell does not belong this parent HierarchicalGroupByControlNode, display the DropMark on the correct one
          HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( draggedOverHierarchicalGroupByControlNode );

          if( parentGBC == null )
            throw new DataGridInternalException( "The hierarchical group-by item must be rooted by a HierarchicalGroupByControl." );

          HierarchicalGroupByControlNode hierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromColumnManagerCell( cell );

          Debug.Assert( hierarchicalGroupByControlNode != null, "CanDrop should have returned false" );
          if( hierarchicalGroupByControlNode == null )
            throw new DataGridInternalException( "A HierarchicalGroupByControlNode must exist for every level." );

          hierarchicalGroupByControlNode.ShowFarDropMark( cell, mousePosition );
        }
      }
      else
      {
        HierarchicalGroupByItem hierarchicalGroupByItem = draggedElement as HierarchicalGroupByItem;

        if( hierarchicalGroupByItem == null )
          return;

        HierarchicalGroupByControlNode draggedHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( hierarchicalGroupByItem );

        HierarchicalGroupByControlNode draggedOverHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        if( draggedHierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "draggedHierarchicalGroupByControlNode is null." );

        if( draggedOverHierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "draggedOverHierarchicalGroupByControlNode is null." );

        if( draggedHierarchicalGroupByControlNode.GroupLevelDescriptions == draggedOverHierarchicalGroupByControlNode.GroupLevelDescriptions )
        {
          this.ShowDropMark( mousePosition );
        }
        else
        {
          // This HierarchicalGroupByItem does not belong this parent HierarchicalGroupByControlNode, display the DropMark on the correct one
          HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( draggedOverHierarchicalGroupByControlNode );

          if( parentGBC == null )
            throw new DataGridInternalException( "A hierarchical group-by item must be rooted by a HierarchicalGroupByControl." );

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
        DataGridContext draggedCellDataGridContext = DataGridControl.GetDataGridContext( cell );

        HierarchicalGroupByControlNode draggedOverHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        if( draggedOverHierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "We should never be dragged over and not contained inside a HierarchicalGroupByControlNode." );

        if( draggedOverHierarchicalGroupByControlNode.GroupLevelDescriptions == draggedCellDataGridContext.GroupLevelDescriptions )
        {
          this.HideDropMark();
        }
        else
        {
          // This ColumnManagerCell does not belong this parent HierarchicalGroupByControlNode, display the DropMark on the correct one
          HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( draggedOverHierarchicalGroupByControlNode );

          if( parentGBC == null )
            throw new DataGridInternalException( "A hierarchical group-by item must be rooted by a HierarchicalGroupByControl." );

          HierarchicalGroupByControlNode hierarchicalGroupByControlNode = parentGBC.GetHierarchicalGroupByControlNodeFromColumnManagerCell( cell );

          Debug.Assert( hierarchicalGroupByControlNode != null, "CanDrop should have returned false" );
          if( hierarchicalGroupByControlNode == null )
            throw new DataGridInternalException( "A HierarchicalGroupByControlNode must exist for every level." );

          hierarchicalGroupByControlNode.HideFarDropMark( cell );
        }
      }
      else
      {
        HierarchicalGroupByItem hierarchicalGroupByItem = draggedElement as HierarchicalGroupByItem;

        if( hierarchicalGroupByItem == null )
          return;

        HierarchicalGroupByControlNode draggedHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( hierarchicalGroupByItem );

        HierarchicalGroupByControlNode draggedOverHierarchicalGroupByControlNode = HierarchicalGroupByItem.GetParentHierarchicalGroupByControlNode( this );

        if( draggedHierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "draggedHierarchicalGroupByControlNode is null." );

        if( draggedOverHierarchicalGroupByControlNode == null )
          throw new DataGridInternalException( "draggedOverHierarchicalGroupByControlNode is null." );

        if( draggedHierarchicalGroupByControlNode.GroupLevelDescriptions == draggedOverHierarchicalGroupByControlNode.GroupLevelDescriptions )
        {
          this.HideDropMark();
        }
        else
        {
          // This HierarchicalGroupByItem does not belong this parent HierarchicalGroupByControlNode, display the DropMark on the correct one
          HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( draggedOverHierarchicalGroupByControlNode );

          Debug.Assert( parentGBC != null );
          if( parentGBC == null )
            throw new DataGridInternalException( "A hierarchical group-by item must be rooted by a HierarchicalGroupByControl." );

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
      ColumnManagerCell draggedColumnManagerCell = draggedElement as ColumnManagerCell;

      if( m_dropMarkAdorner != null )
      {
        GroupLevelDescription draggedOverGroupLevelDescription = this.Content as GroupLevelDescription;

        DropMarkAlignment alignment = m_dropMarkAdorner.Alignment;
        this.HideDropMark();

        if( draggedColumnManagerCell != null )
        {
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

          DataGridControl parentGrid = ( dataGridContext != null )
            ? dataGridContext.DataGridControl
            : null;

          GroupingHelper.AddNewGroupFromColumnManagerCell( draggedColumnManagerCell, draggedOverGroupLevelDescription, alignment, parentGrid );
        }
        else
        {
          HierarchicalGroupByItem draggedGroupByItem = draggedElement as HierarchicalGroupByItem;

          if( draggedGroupByItem == null )
            return;

          GroupLevelDescription draggedGroupLevelDescription = draggedGroupByItem.Content as GroupLevelDescription;

          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

          DataGridControl parentDataGridControl = ( dataGridContext != null )
            ? dataGridContext.DataGridControl
            : null;

          GroupLevelDescription destinationGroupLevelDescription = this.Content as GroupLevelDescription;

          GroupingHelper.MoveGroupDescription( this.ParentColumns, this.ParentGroupDescriptions,
                                               destinationGroupLevelDescription, alignment,
                                               draggedGroupLevelDescription, parentDataGridControl );
        }
      }
      else
      {
        // We try to add a new Group which is not in the current GroupLevelDescriptions
        if( draggedColumnManagerCell == null )
          return;

        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        DataGridControl parentGrid = ( dataGridContext != null )
          ? dataGridContext.DataGridControl
          : null;

        GroupingHelper.AppendNewGroupFromColumnManagerCell( draggedColumnManagerCell, parentGrid );
      }

      HierarchicalGroupByControl parentGBC = GroupingHelper.GetHierarchicalGroupByControl( this );

      Debug.Assert( parentGBC != null );
      if( parentGBC == null )
        throw new DataGridInternalException( "A hierarchical group-by item must be rooted by a HierarchicalGroupByControl." );

      // Notify groups have changed for NoGroupContent    
      parentGBC.UpdateHasGroups();
    }

    #endregion

    #region PRIVATE FIELDS

    // Will remain null when no AdornerLayer is found.
    private DragSourceManager m_dragSourceManager;
    private DropMarkAdorner m_dropMarkAdorner;

    #endregion

    #region HierarchicalGroupByItemToggleColumnSortCommand Private Class

    private sealed class HierarchicalGroupByItemToggleColumnSortCommand : ToggleColumnSortCommand
    {
      #region Constructor

      internal HierarchicalGroupByItemToggleColumnSortCommand( HierarchicalGroupByItem target )
        : base()
      {
        if( target == null )
          throw new ArgumentNullException( "owner" );

        m_target = new WeakReference( target );

        if( target.ParentDataGridContext != null )
        {
          m_dataGridContext = new WeakReference( target.ParentDataGridContext );
        }

        DataGridContext groupByItemDataGridContext = DataGridControl.GetDataGridContext( target );
        ToggleColumnSortCommand.ThrowIfNull( groupByItemDataGridContext, "groupByItemDataGridContext" );

        DataGridControl dataGridControl = groupByItemDataGridContext.DataGridControl;
        ToggleColumnSortCommand.ThrowIfNull( dataGridControl, "dataGridControl" );

        m_dataGridControl = new WeakReference( dataGridControl );
      }

      #endregion

      #region Properties

      #region CanSort Protected Property

      protected override bool CanSort
      {
        get
        {
          var datagrid = this.DataGridControl;
          if( datagrid == null )
            return false;

          // When details are flatten, only the master may toggle a column.
          if( this.DataGridControl.AreDetailsFlatten )
            return false;

          if( this.DataGridContext != null )
          {
            return this.DataGridContext.Items.CanSort;
          }

          return true;
        }
      }

      #endregion

      #region Columns Protected Property

      protected override ColumnCollection Columns
      {
        get
        {
          var target = this.Target;
          if( target == null )
            return null;

          return target.ParentColumns;
        }
      }

      #endregion

      #region DataGridContext Protected Property

      protected override DataGridContext DataGridContext
      {
        get
        {
          return ( m_dataGridContext != null ) ? m_dataGridContext.Target as DataGridContext : null;
        }
      }

      private readonly WeakReference m_dataGridContext;

      #endregion   

      #region DataGridControl Private Property

      private DataGridControl DataGridControl
      {
        get
        {
          return ( m_dataGridControl != null ) ? m_dataGridControl.Target as DataGridControl : null;
        }
      }

      private readonly WeakReference m_dataGridControl;

      #endregion

      #region MaxSortLevels Protected Property

      protected override int MaxSortLevels
      {
        get
        {
          return -1;
        }
      }

      #endregion

      #region SortDescriptions Protected Property

      protected override SortDescriptionCollection SortDescriptions
      {
        get
        {
          var target = this.Target;
          if( target == null )
            return null;

          return target.ParentSortDescriptions;
        }
      }

      #endregion

      #region Target Private Property

      private HierarchicalGroupByItem Target
      {
        get
        {
          return m_target.Target as HierarchicalGroupByItem;
        }
      }

      private readonly WeakReference m_target;

      #endregion

      #endregion

      #region Methods Override

      protected override void ValidateToggleColumnSort()
      {
        Debug.Assert( !this.DataGridControl.AreDetailsFlatten, "A flatten detail should not be able to toggle the column sort direction." );
      }

      protected override SortDescriptionsSyncContext GetSortDescriptionsSyncContext()
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext != null )
          return dataGridContext.SortDescriptionsSyncContext;

        return this.DataGridControl.DataGridContext.SortDescriptionsSyncContext;
      }

      protected override void ValidateSynchronizationContext( SynchronizationContext synchronizationContext )
      {
        if( !synchronizationContext.Own )
          throw new DataGridInternalException( "The column is already being processed.", this.DataGridControl );
      }

      protected override void DeferRestoreStateOnLevel( Disposer disposer )
      {
        var dataGridContext = this.DataGridContext;
        if( dataGridContext != null )
        {
          ToggleColumnSortCommand.DeferRestoreStateOnLevel( disposer, dataGridContext );
        }
      }

      protected override IDisposable DeferResortHelperItemsSourceCollection()
      {
        var dataGridContext = this.DataGridContext;

        return ( dataGridContext != null )
                ? this.DeferResortHelper( dataGridContext.ItemsSourceCollection, dataGridContext.Items )
                : this.DeferResortHelper( this.DataGridControl.ItemsSource, this.DataGridControl.DataGridContext.Items );
      }

      protected override bool TryDeferResortSourceDetailConfiguration( out IDisposable defer )
      {
        var dataGridContext = this.DataGridContext;

        return ( dataGridContext != null )
                ? this.TryDeferResort( dataGridContext.SourceDetailConfiguration, out defer )
                : this.TryDeferResort( this.DataGridControl.DataGridContext.SourceDetailConfiguration, out defer );
      }

      protected override IDisposable SetQueueBringIntoViewRestrictions( AutoScrollCurrentItemSourceTriggers triggers )
      {
        return this.DataGridControl.SetQueueBringIntoViewRestrictions( triggers );
      }

      #endregion
    }

    #endregion
  }
}
