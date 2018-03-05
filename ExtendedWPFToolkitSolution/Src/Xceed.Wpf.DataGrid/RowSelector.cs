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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  [TemplatePart( Name = "PART_RowResizerThumb", Type = typeof( Thumb ) )]
  public class RowSelector : ContentControl, INotifyPropertyChanged
  {
    static RowSelector()
    {
      RowSelector.IsPressedProperty = RowSelector.IsPressedPropertyKey.DependencyProperty;
      RowSelector.RowTypeProperty = RowSelector.RowTypePropertyKey.DependencyProperty;

      UIElement.FocusableProperty.OverrideMetadata( typeof( RowSelector ), new FrameworkPropertyMetadata( false ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( RowSelector ),
        new FrameworkPropertyMetadata( new PropertyChangedCallback( RowSelector.OnParentGridControlChanged ) ) );

      m_sDataContextBinding = new Binding();
      m_sDataContextBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );
      m_sDataContextBinding.Path = new PropertyPath( DataGridControl.ContainerProperty );
      m_sDataContextBinding.Mode = BindingMode.OneWay;

      m_sItemIndexBinding = new Binding();
      m_sItemIndexBinding.Path = new PropertyPath( DataGridVirtualizingPanel.ItemIndexProperty );
      m_sItemIndexBinding.Mode = BindingMode.OneWay;

      m_sIsCurrentBinding = new Binding();
      m_sIsCurrentBinding.Path = new PropertyPath( Row.IsCurrentProperty );
      m_sIsCurrentBinding.Mode = BindingMode.OneWay;

      m_sIsBeingEditedBinding = new Binding();
      m_sIsBeingEditedBinding.Path = new PropertyPath( Row.IsBeingEditedProperty );
      m_sIsBeingEditedBinding.Mode = BindingMode.OneWay;

      m_sHasValidationErrorBinding = new Binding();
      m_sHasValidationErrorBinding.Path = new PropertyPath( Row.HasValidationErrorProperty );
      m_sHasValidationErrorBinding.Mode = BindingMode.OneWay;

      m_sRowSelectorVisibleBinding = new Binding();
      m_sRowSelectorVisibleBinding.Path = new PropertyPath( RowSelector.VisibleProperty );
      m_sRowSelectorVisibleBinding.Mode = BindingMode.OneWay;
      m_sRowSelectorVisibleBinding.Converter = new BooleanToVisibilityConverter();
      m_sRowSelectorVisibleBinding.ConverterParameter = Visibility.Collapsed;

      m_sRowSelectorStyleBinding = new Binding();
      m_sRowSelectorStyleBinding.Path = new PropertyPath( RowSelector.RowSelectorStyleProperty );
      m_sRowSelectorStyleBinding.Mode = BindingMode.OneWay;

      m_sAllowResizeBinding = new Binding();
      m_sAllowResizeBinding.RelativeSource = new RelativeSource( RelativeSourceMode.Self );
      m_sAllowResizeBinding.Path = new PropertyPath( "(0).(1)", DataGridControl.DataGridContextProperty, TableView.AllowRowResizeProperty );

      m_sAllowResizeBinding.Mode = BindingMode.OneWay;
    }

    internal RowSelector()
    {
      BindingOperations.SetBinding( this, RowSelector.DataContextProperty, m_sDataContextBinding );
      BindingOperations.SetBinding( this, RowSelector.IsCurrentProperty, m_sIsCurrentBinding );
      BindingOperations.SetBinding( this, RowSelector.IsBeingEditedProperty, m_sIsBeingEditedBinding );
      BindingOperations.SetBinding( this, RowSelector.ItemIndexProperty, m_sItemIndexBinding );
      BindingOperations.SetBinding( this, RowSelector.HasValidationErrorProperty, m_sHasValidationErrorBinding );
      BindingOperations.SetBinding( this, RowSelector.VisibilityProperty, m_sRowSelectorVisibleBinding );
      BindingOperations.SetBinding( this, RowSelector.RowSelectorStyleProperty, m_sRowSelectorStyleBinding );
      BindingOperations.SetBinding( this, RowSelector.AllowResizeProperty, m_sAllowResizeBinding );
    }

    #region AllowResize Property

    internal static readonly DependencyProperty AllowResizeProperty = DependencyProperty.Register(
      "AllowResize",
      typeof( bool ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( true ) );

    internal bool AllowResize
    {
      get
      {
        return ( bool )this.GetValue( RowSelector.AllowResizeProperty );
      }
      set
      {
        this.SetValue( RowSelector.AllowResizeProperty, value );
      }
    }

    private bool CanBeResized( Orientation orientation )
    {
      if( this.AllowResize )
      {
        FrameworkElement rowItem = this.DataContext as FrameworkElement;
        if( rowItem != null )
        {
          if( orientation == Orientation.Vertical )
          {
            object minHeight = rowItem.ReadLocalValue( FrameworkElement.MinHeightProperty );
            object maxHeight = rowItem.ReadLocalValue( FrameworkElement.MaxHeightProperty );

            if( ( minHeight == DependencyProperty.UnsetValue )
              || ( maxHeight == DependencyProperty.UnsetValue )
              || ( !object.Equals( minHeight, maxHeight ) ) )
            {
              return true;
            }
          }
          else
          {
            object minWidth = rowItem.ReadLocalValue( FrameworkElement.MinWidthProperty );
            object maxWidth = rowItem.ReadLocalValue( FrameworkElement.MaxWidthProperty );

            if( ( minWidth == DependencyProperty.UnsetValue )
              || ( maxWidth == DependencyProperty.UnsetValue )
              || ( !object.Equals( minWidth, maxWidth ) ) )
            {
              return true;
            }
          }
        }
      }

      return false;
    }

    #endregion AllowResize Property

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged( string propertyName )
    {
      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
      }
    }

    #endregion

    #region Orientation Property

    public Orientation Orientation
    {
      get
      {
        return ( Orientation )GetValue( OrientationProperty );
      }
      set
      {
        SetValue( OrientationProperty, value );
      }
    }

    // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register( "Orientation", typeof( Orientation ), typeof( RowSelector ), new UIPropertyMetadata( Orientation.Vertical ) );

    #endregion

    #region IsPressed Read-Only Property

    private static readonly DependencyPropertyKey IsPressedPropertyKey =
      DependencyProperty.RegisterReadOnly( "IsPressed", typeof( bool ), typeof( RowSelector ), new PropertyMetadata( false ) );

    public static readonly DependencyProperty IsPressedProperty;

    public bool IsPressed
    {
      get
      {
        return ( bool )this.GetValue( RowSelector.IsPressedProperty );
      }
    }

    internal void SetIsPressed( bool value )
    {
      this.SetValue( RowSelector.IsPressedPropertyKey, value );
    }

    #endregion IsPressed Read-Only Property

    #region IsCurrent

    public static readonly DependencyProperty IsCurrentProperty = DependencyProperty.Register(
      "IsCurrent",
      typeof( bool ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( false ) );

    public bool IsCurrent
    {
      get
      {
        return ( bool )this.GetValue( IsCurrentProperty );
      }
    }

    #endregion

    #region IsBeingEdited

    public static readonly DependencyProperty IsBeingEditedProperty = DependencyProperty.Register(
      "IsBeingEdited",
      typeof( bool ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( false ) );

    public bool IsBeingEdited
    {
      get
      {
        return ( bool )this.GetValue( IsBeingEditedProperty );
      }
    }

    #endregion

    #region ItemIndex

    public static readonly DependencyProperty ItemIndexProperty = DataGridVirtualizingPanel.ItemIndexProperty.AddOwner( typeof( RowSelector ) );

    public int ItemIndex
    {
      get
      {
        return ( int )this.GetValue( ItemIndexProperty );
      }
    }

    #endregion

    #region HasValidationError

    public static readonly DependencyProperty HasValidationErrorProperty = DependencyProperty.Register(
      "HasValidationError",
      typeof( bool ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( false ) );

    public bool HasValidationError
    {
      get
      {
        return ( bool )this.GetValue( HasValidationErrorProperty );
      }
    }

    #endregion

    #region RowType

    private static readonly DependencyPropertyKey RowTypePropertyKey = DependencyProperty.RegisterReadOnly(
      "RowType",
      typeof( Type ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( null ) );

    public static readonly DependencyProperty RowTypeProperty;

    public Type RowType
    {
      get
      {
        return ( Type )this.GetValue( RowSelector.RowTypeProperty );
      }
    }

    private void SetRowType( Type value )
    {
      this.SetValue( RowSelector.RowTypePropertyKey, value );
    }

    #endregion

    #region ContainerRect

    public static readonly DependencyProperty ContainerRectProperty = DependencyProperty.Register(
      "ContainerRect",
      typeof( Rect ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( null ) );

    public Rect ContainerRect
    {
      get
      {
        return ( Rect )this.GetValue( ContainerRectProperty );
      }
      internal set
      {
        this.SetValue( ContainerRectProperty, value );
      }
    }

    #endregion

    #region Visible Attached Property

    public static readonly DependencyProperty VisibleProperty = DependencyProperty.RegisterAttached(
      "Visible",
      typeof( bool ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( true ) );

    public static bool GetVisible( DependencyObject d )
    {
      return ( bool )d.GetValue( RowSelector.VisibleProperty );
    }

    public static void SetVisible( DependencyObject d, bool value )
    {
      d.SetValue( RowSelector.VisibleProperty, value );
    }

    #endregion

    #region RowSelectorStyle Attached Property

    public static readonly DependencyProperty RowSelectorStyleProperty = DependencyProperty.RegisterAttached(
      "RowSelectorStyle",
      typeof( Style ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( null, new PropertyChangedCallback( OnRowSelectorStyleChanged ) ) );

    public static Style GetRowSelectorStyle( DependencyObject obj )
    {
      return ( Style )obj.GetValue( RowSelectorStyleProperty );
    }

    public static void SetRowSelectorStyle( DependencyObject obj, Style value )
    {
      obj.SetValue( RowSelectorStyleProperty, value );
    }

    private static void OnRowSelectorStyleChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      RowSelector rowSelector = sender as RowSelector;

      //Here, since the property is an Attached propety, the type of sender can be anything, but in this particular case, I want
      //to filter out everything but RowSelector since this is my queue to set a local style on the RowSelector ( or clear the local
      //value).
      if( rowSelector == null )
        return;

      if( e.NewValue == null )
      {
        rowSelector.ClearValue( RowSelector.StyleProperty );
      }
      else
      {
        rowSelector.SetValue( RowSelector.StyleProperty, e.NewValue );
      }
    }

    #endregion

    #region ReferenceElement Internal Property

    internal static readonly DependencyProperty ReferenceElementProperty = DependencyProperty.Register(
      "ReferenceElement",
      typeof( FrameworkElement ),
      typeof( RowSelector ),
      new FrameworkPropertyMetadata( null ) );

    internal FrameworkElement ReferenceElement
    {
      get
      {
        return ( FrameworkElement )this.GetValue( ReferenceElementProperty );
      }
      set
      {
        this.SetValue( ReferenceElementProperty, value );
      }
    }

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      this.SetupColumnResizerThumb();
    }

    protected override void OnPropertyChanged( DependencyPropertyChangedEventArgs e )
    {
      base.OnPropertyChanged( e );

      if( e.Property == RowSelector.DataContextProperty )
      {
        Type rowType = null;

        Row row = this.DataContext as Row;
        if( row != null )
        {
          rowType = row.GetType();
        }
        else
        {
          HeaderFooterItem hfi = this.DataContext as HeaderFooterItem;
          if( hfi != null )
          {
            row = hfi.AsVisual() as Row;
            if( row != null )
            {
              rowType = row.GetType();
            }
          }
        }

        this.SetRowType( rowType );

      }
    }

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      RowSelector rowSelector = ( RowSelector )sender;
      DataGridControl newGrid = e.NewValue as DataGridControl;

      if( newGrid != null )
      {
        rowSelector.PrepareDefaultStyleKey( newGrid.GetView() );
      }
    }

    internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( RowSelector ) );
    }

    #region Row Resizer Code

    private void SetupColumnResizerThumb()
    {
      if( m_rowResizerThumb != null )
      {
        m_rowResizerThumb.DragStarted -= new DragStartedEventHandler( m_rowResizerThumb_DragStarted );
        m_rowResizerThumb.DragDelta -= new DragDeltaEventHandler( m_rowResizerThumb_DragDelta );
        m_rowResizerThumb.DragCompleted -= new DragCompletedEventHandler( m_rowResizerThumb_DragCompleted );
        m_rowResizerThumb.QueryCursor -= new QueryCursorEventHandler( m_rowResizerThumb_QueryCursor );
        m_rowResizerThumb.MouseDoubleClick -= new MouseButtonEventHandler( m_rowResizerThumb_MouseDoubleClick );

        m_rowResizerThumb = null;
      }

      m_rowResizerThumb = this.GetTemplateChild( "PART_RowResizerThumb" ) as Thumb;

      if( m_rowResizerThumb != null )
      {
        m_rowResizerThumb.DragStarted += new DragStartedEventHandler( m_rowResizerThumb_DragStarted );
        m_rowResizerThumb.DragDelta += new DragDeltaEventHandler( m_rowResizerThumb_DragDelta );
        m_rowResizerThumb.DragCompleted += new DragCompletedEventHandler( m_rowResizerThumb_DragCompleted );
        m_rowResizerThumb.QueryCursor += new QueryCursorEventHandler( m_rowResizerThumb_QueryCursor );
        m_rowResizerThumb.MouseDoubleClick += new MouseButtonEventHandler( m_rowResizerThumb_MouseDoubleClick );
      }
    }

    private void m_rowResizerThumb_MouseDoubleClick( object sender, MouseButtonEventArgs e )
    {
      FrameworkElement rowItem = ( FrameworkElement )this.DataContext;

      if( rowItem == null )
        return;

      if( !this.CanBeResized( this.Orientation ) )
        return;

      // Resets the row width or height depending on the orientation.
      DependencyProperty property = ( this.Orientation == Orientation.Vertical ) ? FrameworkElement.HeightProperty : FrameworkElement.WidthProperty;

      rowItem.ClearValue( property );
    }

    private void m_rowResizerThumb_QueryCursor( object sender, QueryCursorEventArgs e )
    {
      FrameworkElement rowItem = ( FrameworkElement )this.DataContext;

      if( rowItem == null )
        return;

      if( this.CanBeResized( this.Orientation ) )
      {
        DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

        if( dataGridContext != null )
        {
          TableView tableView = dataGridContext.DataGridControl.GetView() as TableView;

          e.Cursor = ( ( tableView != null ) && ( this.Orientation == Orientation.Vertical ) )
            ? tableView.RowSelectorResizeNorthSouthCursor
            : tableView.RowSelectorResizeWestEastCursor;
        }

        if( e.Cursor == null )
        {
          e.Cursor = ( this.Orientation == Orientation.Vertical ) ? TableView.DefaultRowSelectorResizeNorthSouthCursor : TableView.DefaultRowSelectorResizeWestEastCursor;
        }
      }

      e.Handled = true;
    }

    private void m_rowResizerThumb_DragStarted( object sender, DragStartedEventArgs e )
    {
      if( !this.CanBeResized( this.Orientation ) )
        return;

      FrameworkElement rowItem = ( FrameworkElement )this.DataContext;

      if( rowItem == null )
        return;

      if( this.Orientation == Orientation.Vertical )
      {
        m_originalSize = rowItem.ActualHeight;
      }
      else
      {
        m_originalSize = rowItem.ActualWidth;
      }
    }

    private void m_rowResizerThumb_DragDelta( object sender, DragDeltaEventArgs e )
    {
      if( !this.CanBeResized( this.Orientation ) )
        return;

      FrameworkElement rowItem = ( FrameworkElement )this.DataContext;

      if( rowItem == null )
        return;

      double newSize;

      if( this.Orientation == Orientation.Vertical )
      {
        newSize = rowItem.ActualHeight + e.VerticalChange;
      }
      else
      {
        newSize = rowItem.ActualWidth + e.HorizontalChange;
      }

      if( newSize < MIN_SIZE )
      {
        newSize = MIN_SIZE;
      }

      if( this.Orientation == Orientation.Vertical )
      {
        if( newSize < rowItem.MinHeight )
        {
          newSize = rowItem.MinHeight;
        }
        else if( newSize > rowItem.MaxHeight )
        {
          newSize = rowItem.MaxHeight;
        }

        rowItem.Height = newSize;
      }
      else
      {
        if( newSize < rowItem.MinWidth )
        {
          newSize = rowItem.MinWidth;
        }
        else if( newSize > rowItem.MaxWidth )
        {
          newSize = rowItem.MaxWidth;
        }

        rowItem.Width = newSize;
      }
    }

    private void m_rowResizerThumb_DragCompleted( object sender, DragCompletedEventArgs e )
    {
      if( !this.CanBeResized( this.Orientation ) )
        return;

      FrameworkElement rowItem = ( FrameworkElement )this.DataContext;

      if( rowItem == null )
        return;

      if( e.Canceled )
      {
        if( this.Orientation == Orientation.Vertical )
        {
          rowItem.Height = m_originalSize;
        }
        else
        {
          rowItem.Width = m_originalSize;
        }
      }

      m_originalSize = -1d;
    }

    private const double MIN_SIZE = 4d;

    private double m_originalSize = -1d;
    private Thumb m_rowResizerThumb; // = null

    #endregion Column Resizer Code

    #region Button Behavior Code

    protected override void OnMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      m_isFromMouseButtonDown = true;

      if( this.CaptureMouse() )
      {
        this.SetIsPressed( true );
        e.Handled = true;

        FrameworkElement rowItemContainer = this.DataContext as FrameworkElement;

        if( rowItemContainer != null )
        {
          DataGridContext dataGridContext = DataGridControl.GetDataGridContext( rowItemContainer );

          if( dataGridContext != null )
          {
            DataGridControl dataGridControl = dataGridContext.DataGridControl;

            // Ensure to push the RowSelector source to the SelectionChangeManager to be able to
            // detect that a RowSelector was used to perform a selection of Row(s) and/or Cell(s)
            using( dataGridControl.SelectionChangerManager.PushUpdateSelectionSource( SelectionManager.UpdateSelectionSource.RowSelector ) )
            {
              if( dataGridControl.SetFocusHelper( rowItemContainer, dataGridContext.CurrentColumn, true, true, true ) )
              {
                // Keep a reference to the mouse position so we can calculate when a drag operation is actually started.
                dataGridControl.InitializeDragPostion( e );
              }

              if( dataGridControl.NavigationBehavior == NavigationBehavior.RowOrCell )
              {
                try
                {
                  dataGridContext.SetCurrentColumnCore( null, false, false, AutoScrollCurrentItemSourceTriggers.Navigation );
                }
                catch( DataGridException )
                {
                  // We swallow the exception if it occurs because of a validation error or Cell was read-only or any other GridException.
                }
              }
            }
          }
        }
      }

      base.OnMouseLeftButtonDown( e );

      m_isFromMouseButtonDown = false;
    }

    protected override void OnMouseMove( MouseEventArgs e )
    {
      bool doDrag = false;

      if( e.LeftButton == MouseButtonState.Pressed )
      {
        if( this.IsMouseCaptured )
        {
          Rect bounds = new Rect( 0d, 0d, this.ActualWidth, this.ActualHeight );
          this.SetIsPressed( bounds.Contains( e.GetPosition( this ) ) );

          e.Handled = true;
        }

        doDrag = true;
      }

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( dataGridContext != null )
      {
        //Do not start the drag operation until we have an actual mouse mouve.
        if( doDrag && !m_isFromMouseButtonDown )
        {
          dataGridContext.DataGridControl.DoDrag( e );
        }
        else
        {
          dataGridContext.DataGridControl.ResetDragDataObject();
        }
      }

      base.OnMouseMove( e );
    }

    protected override void OnMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      bool isMouseCaptured = this.IsMouseCaptured;
      bool isPressed = this.IsPressed;

      if( isMouseCaptured )
      {
        this.ReleaseMouseCapture();
        this.SetIsPressed( false );
        e.Handled = true;
      }

      base.OnMouseLeftButtonUp( e );
    }

    protected override void OnLostMouseCapture( MouseEventArgs e )
    {
      if( this.IsPressed )
      {
        this.SetIsPressed( false );
      }

      base.OnLostMouseCapture( e );
    }

    private bool m_isFromMouseButtonDown = false;

    #endregion Button Behavior Code

    private static readonly Binding m_sDataContextBinding;
    private static readonly Binding m_sItemIndexBinding;
    private static readonly Binding m_sIsCurrentBinding;
    private static readonly Binding m_sIsBeingEditedBinding;
    private static readonly Binding m_sHasValidationErrorBinding;
    private static readonly Binding m_sRowSelectorVisibleBinding;
    private static readonly Binding m_sRowSelectorStyleBinding;
    private static readonly Binding m_sAllowResizeBinding;
  }
}
