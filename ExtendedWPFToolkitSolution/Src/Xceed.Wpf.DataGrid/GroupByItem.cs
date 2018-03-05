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
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Windows.Controls.Primitives;
using System.Windows.Resources;
using System.Security;
using Xceed.Wpf.DataGrid.Views;

using Xceed.Utils.Wpf.DragDrop;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid
{
  public class GroupByItem : ButtonBase, IDropTarget, INotifyPropertyChanged
  {
    #region PUBLIC CONSTRUCTORS

    static GroupByItem()
    {
      // This DefaultStyleKey will only be used in design-time.
      DefaultStyleKeyProperty.OverrideMetadata( typeof( GroupByItem ), new FrameworkPropertyMetadata( new Markup.ThemeKey( typeof( Views.TableView ), typeof( GroupByItem ) ) ) );

      GroupByItem.IsBeingDraggedProperty = GroupByItem.IsBeingDraggedPropertyKey.DependencyProperty;

      FocusableProperty.OverrideMetadata( typeof( GroupByItem ), new FrameworkPropertyMetadata( false ) );
    }

    #endregion

    #region SortDirection Property

    // Only used to bind between Column and us, but we don't want to expose it publicly
    private static readonly DependencyProperty SortDirectionInternalProperty =
        DependencyProperty.Register( "SortDirectionInternal", typeof( SortDirection ), typeof( GroupByItem ), new PropertyMetadata( SortDirection.None, new PropertyChangedCallback( GroupByItem.OnSortDirectionInternalChanged ) ) );

    public SortDirection SortDirection
    {
      get
      {
        return ( SortDirection )this.GetValue( GroupByItem.SortDirectionInternalProperty );
      }
    }

    private static void OnSortDirectionInternalChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      GroupByItem groupByItem = ( GroupByItem )sender;
      groupByItem.OnPropertyChanged( new PropertyChangedEventArgs( "SortDirection" ) );
    }

    #endregion SortDirection Property

    #region IsBeingDragged Read-Only Property

    private static readonly DependencyPropertyKey IsBeingDraggedPropertyKey =
        DependencyProperty.RegisterReadOnly( "IsBeingDragged", typeof( bool ), typeof( GroupByItem ), new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsBeingDraggedProperty;

    public bool IsBeingDragged
    {
      get
      {
        return ( bool )this.GetValue( GroupByItem.IsBeingDraggedProperty );
      }
    }

    private void SetIsBeingDragged( bool value )
    {
      this.SetValue( GroupByItem.IsBeingDraggedPropertyKey, value );
    }

    #endregion IsBeingDragged Read-Only Property

    #region PUBLIC METHODS

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();
      this.InitSortDirection();
      this.SetupDragManager();
    }

    #endregion

    #region PROTECTED METHODS

    protected internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( GroupByItem ) );
    }

    #endregion

    #region PRIVATE METHODS

    private void InitSortDirection()
    {
      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return;

      var groupInfo = this.Content as GroupLevelDescription;
      if( groupInfo == null )
        return;

      var column = dataGridContext.Columns[ groupInfo.FieldName ];
      if( column == null )
        return;

      var sortBinding = new Binding();
      sortBinding.Path = new PropertyPath( ColumnBase.SortDirectionProperty );
      sortBinding.Mode = BindingMode.OneWay;
      sortBinding.NotifyOnSourceUpdated = true;
      sortBinding.Source = column;
      sortBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;

      BindingOperations.SetBinding( this, GroupByItem.SortDirectionInternalProperty, sortBinding );
    }

    private GroupByControl GetParentGroupByControl()
    {
      DependencyObject parent = TreeHelper.GetParent( this );

      while( parent != null )
      {
        if( parent is GroupByControl )
          break;

        parent = TreeHelper.GetParent( parent );
      }

      return parent as GroupByControl;
    }

    #endregion

    #region Drag & Drop Manager

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

      // Can be null in design-time (edition of a style TargetType GroupByItem).
      if( dataGridControl == null )
        return;

      Debug.Assert( m_dragSourceManager == null, "There might be problems when there is already a m_dragSourceManager." );

      if( m_dragSourceManager != null )
      {
        m_dragSourceManager.PropertyChanged -= new PropertyChangedEventHandler( m_dragSourceManager_PropertyChanged );
        m_dragSourceManager.DragOutsideQueryCursor -= new QueryCursorEventHandler( m_dragSourceManager_DragOutsideQueryCursor );
        m_dragSourceManager.DroppedOutside -= new EventHandler( m_dragSourceManager_DroppedOutside );
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

      m_dragSourceManager.PropertyChanged += new PropertyChangedEventHandler( m_dragSourceManager_PropertyChanged );
      m_dragSourceManager.DragOutsideQueryCursor += new QueryCursorEventHandler( m_dragSourceManager_DragOutsideQueryCursor );
      m_dragSourceManager.DroppedOutside += new EventHandler( m_dragSourceManager_DroppedOutside );
    }

    void m_dragSourceManager_DroppedOutside( object sender, EventArgs e )
    {
      bool allowGroupingModification = true;
      GroupByControl parentGBC = this.GetParentGroupByControl();

      if( parentGBC != null )
        allowGroupingModification = parentGBC.IsGroupingModificationAllowed;

      if( allowGroupingModification )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
        Debug.Assert( dataGridContext != null );

        if( dataGridContext != null )
        {
          GroupingHelper.RemoveGroupDescription(
            dataGridContext.Items.GroupDescriptions,
            this.Content as GroupLevelDescription,
            dataGridContext.DataGridControl );
        }
      }
    }

    void m_dragSourceManager_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      if( e.PropertyName == "IsDragging" )
      {
        this.SetIsBeingDragged( m_dragSourceManager.IsDragging );
      }
    }

    private void m_dragSourceManager_DragOutsideQueryCursor( object sender, QueryCursorEventArgs e )
    {
      GroupByControl parentGBC = this.GetParentGroupByControl();

      if( ( parentGBC == null ) || !parentGBC.IsGroupingModificationAllowed )
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
          bool allowSort = true;
          GroupByControl parentGBC = this.GetParentGroupByControl();

          if( parentGBC != null )
            allowSort = parentGBC.AllowSort;

          if( allowSort )
          {
            DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );
            GroupLevelDescription groupInfo = this.Content as GroupLevelDescription;
            Debug.Assert( ( dataGridContext != null ) && ( groupInfo != null ) );

            if( ( dataGridContext != null ) && ( groupInfo != null ) )
            {
              ColumnBase column = dataGridContext.Columns[ groupInfo.FieldName ];

              if( ( column != null ) && ( column.AllowSort ) )
              {
                bool shiftUnpressed = ( ( Keyboard.Modifiers & ModifierKeys.Shift ) != ModifierKeys.Shift );

                var toggleColumnSort = dataGridContext.ToggleColumnSortCommand;
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

    internal void ShowDropMark( RelativePoint mousePosition )
    {
      if( m_dropMarkAdorner == null )
      {
        var dataGridContext = DataGridControl.GetDataGridContext( this );
        var dataGridControl = ( dataGridContext != null ) ? dataGridContext.DataGridControl : null;
        var pen = UIViewBase.GetDropMarkPen( this );

        if( ( pen == null ) && ( dataGridControl != null ) )
        {
          var uiViewBase = dataGridControl.GetView() as UIViewBase;
          if( uiViewBase != null )
          {
            pen = uiViewBase.DefaultDropMarkPen;
          }
        }

        var orientation = UIViewBase.GetDropMarkOrientation( this );

        if( ( orientation == DropMarkOrientation.Default ) && ( dataGridControl != null ) )
        {
          var uiViewBase = dataGridControl.GetView() as UIViewBase;
          if( uiViewBase != null )
          {
            orientation = uiViewBase.DefaultDropMarkOrientation;
          }
        }

        m_dropMarkAdorner = new DropMarkAdorner( this, pen, orientation );

        var adornerLayer = AdornerLayer.GetAdornerLayer( this );
        if( adornerLayer != null )
        {
          adornerLayer.Add( m_dropMarkAdorner );
        }
      }

      m_dropMarkAdorner.UpdateAlignment( mousePosition );
    }

    internal void HideDropMark()
    {
      if( m_dropMarkAdorner == null )
        return;

      var adornerLayer = AdornerLayer.GetAdornerLayer( this );
      if( adornerLayer != null )
      {
        adornerLayer.Remove( m_dropMarkAdorner );
      }

      m_dropMarkAdorner = null;
    }

    #endregion Drag & Drop Manager

    #region IDropTarget Members

    bool IDropTarget.CanDropElement( UIElement draggedElement, RelativePoint mousePosition )
    {
      var allowGroupingModification = true;
      var parentGBC = this.GetParentGroupByControl();

      if( parentGBC != null )
      {
        allowGroupingModification = parentGBC.IsGroupingModificationAllowed;
      }

      // We don't accept any ColumnManagerCell from Details
      var context = DataGridControl.GetDataGridContext( draggedElement );
      var cell = draggedElement as ColumnManagerCell;
      var isAlreadyGroupedBy = false;

      if( cell != null )
      {
        isAlreadyGroupedBy = GroupingHelper.IsAlreadyGroupedBy( cell );

        var parentColumn = cell.ParentColumn;
        if( ( parentColumn == null ) || ( !parentColumn.AllowGroup ) )
          return false;
      }

      var sourceDetailContext = DataGridControl.GetDataGridContext( this );
      Debug.Assert( sourceDetailContext != null );
      var sourceDetailConfig = ( sourceDetailContext != null ) ? sourceDetailContext.SourceDetailConfiguration : null;

      var draggedDetailContext = DataGridControl.GetDataGridContext( draggedElement );
      Debug.Assert( draggedDetailContext != null );
      var draggedDetailConfig = ( draggedDetailContext != null ) ? draggedDetailContext.SourceDetailConfiguration : null;


      var canDrop = ( sourceDetailConfig == draggedDetailConfig ) &&
                    ( allowGroupingModification ) &&
                    ( ( draggedElement is ColumnManagerCell ) || ( draggedElement is GroupByItem ) ) &&
                    ( draggedElement != this ) &&
                    ( isAlreadyGroupedBy == false );

      if( canDrop && ( cell != null ) )
      {
        canDrop = GroupingHelper.ValidateMaxGroupDescriptions( draggedDetailContext );
      }

      return canDrop;
    }

    void IDropTarget.DragEnter( UIElement draggedElement )
    {
    }

    void IDropTarget.DragOver( UIElement draggedElement, RelativePoint mousePosition )
    {
      this.ShowDropMark( mousePosition );
    }

    void IDropTarget.DragLeave( UIElement draggedElement )
    {
      this.HideDropMark();
    }

    void IDropTarget.Drop( UIElement draggedElement, RelativePoint mousePosition )
    {
      if( m_dropMarkAdorner == null )
        return;

      var dataGridContext = DataGridControl.GetDataGridContext( this );
      if( dataGridContext == null )
        return;

      var draggedOverGroupLevelDescription = this.Content as GroupLevelDescription;
      if( draggedOverGroupLevelDescription == null )
        return;

      var alignment = m_dropMarkAdorner.Alignment;

      this.HideDropMark();

      var draggedCell = draggedElement as ColumnManagerCell;
      if( draggedCell != null )
      {
        GroupingHelper.AddNewGroupFromColumnManagerCell( draggedCell, draggedOverGroupLevelDescription, alignment, dataGridContext.DataGridControl );
      }
      else
      {
        var draggedGroupBy = draggedElement as GroupByItem;

        Debug.Assert( draggedGroupBy != null );

        if( draggedGroupBy != null )
        {
          var draggedGroupLevelDescription = draggedGroupBy.Content as GroupLevelDescription;

          GroupingHelper.MoveGroupDescription( dataGridContext.Columns, dataGridContext.Items.GroupDescriptions, draggedOverGroupLevelDescription, alignment, draggedGroupLevelDescription, dataGridContext.DataGridControl );
        }
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

    #region PRIVATE FIELDS

    /// <summary>
    /// Will remain null when no AdornerLayer is found.
    /// </summary>
    private DragSourceManager m_dragSourceManager;
    private DropMarkAdorner m_dropMarkAdorner;

    #endregion
  }
}
