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
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid.Views
{
  public class TableViewHeaderFooterPanel : StackPanel
  {
    #region Constructor

    public TableViewHeaderFooterPanel()
    {
      this.AddHandler( FrameworkElement.RequestBringIntoViewEvent, new RequestBringIntoViewEventHandler( this.OnRequestBringIntoView ) );
    }

    #endregion

    #region ParentDataGridControl Private Property

    private DataGridControl ParentDataGridControl
    {
      get
      {
        var dataGridContext = DataGridControl.GetDataGridContext( this );
        if( dataGridContext == null )
          return null;

        return dataGridContext.DataGridControl;
      }
    }

    #endregion

    #region PreviewKeyDown and KeyDown handling overrides

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      switch( e.Key )
      {
        case Key.Tab:
          this.HandlePreviewTabKey( e );
          break;

        case Key.Home:
          this.HandlePreviewHomeKey( e );
          break;

        case Key.End:
          this.HandlePreviewEndKey( e );
          break;

        case Key.Left:
          this.HandlePreviewLeftKey( e );
          break;

        case Key.Right:
          this.HandlePreviewRightKey( e );
          break;

        default:
          base.OnPreviewKeyDown( e );
          break;
      }
    }

    protected virtual void HandlePreviewTabKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      var dataGridContext = dataGridControl.CurrentContext;
      if( dataGridContext == null )
        return;

      var container = dataGridContext.GetContainerFromItem( dataGridContext.InternalCurrentItem );
      if( container == null )
        return;

      var tabbingMode = KeyboardNavigation.GetTabNavigation( container );
      if( tabbingMode == KeyboardNavigationMode.None )
        return;

      if( ( Keyboard.Modifiers == ModifierKeys.None ) || ( Keyboard.Modifiers == ModifierKeys.Shift ) )
      {
        DataGridItemsHost.BringIntoViewKeyboardFocusedElement();

        //Force the "inline" relayout of the panel
        //This has no effect if the panel do not have to be updated.
        this.UpdateLayout();
      }
    }

    protected virtual void HandlePreviewLeftKey( KeyEventArgs e )
    {
      DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
      this.UpdateLayout();
    }

    protected virtual void HandlePreviewRightKey( KeyEventArgs e )
    {
      DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
      this.UpdateLayout();
    }

    protected virtual void HandlePreviewHomeKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePreviewEndKey( KeyEventArgs e )
    {
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      switch( e.Key )
      {
        case Key.Tab:
          this.HandleTabKey( e );
          break;

        case Key.Home:
          this.HandleHomeKey( e );
          break;

        case Key.End:
          this.HandleEndKey( e );
          break;

        case Key.Left:
          this.HandleLeftKey( e );
          break;

        case Key.Right:
          this.HandleRightKey( e );
          break;

        default:
          base.OnKeyDown( e );
          break;
      }
    }

    protected virtual void HandleTabKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      var dataGridContext = dataGridControl.CurrentContext;
      if( dataGridContext == null )
        return;

      e.Handled = NavigationHelper.HandleTabKey( dataGridControl, dataGridContext, e.OriginalSource as FrameworkElement, e.KeyboardDevice );
    }

    protected virtual void HandleHomeKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      bool ctrlPressed = ( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control );
      if( !ctrlPressed )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( ( dataGridControl == null ) || ( dataGridControl.NavigationBehavior != NavigationBehavior.CellOnly ) )
        return;

      NavigationHelper.MoveFocusToFirstVisibleColumn( dataGridControl.CurrentContext );

      e.Handled = true;
    }

    protected virtual void HandleEndKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      bool ctrlPressed = ( ( e.KeyboardDevice.Modifiers & ModifierKeys.Control ) == ModifierKeys.Control );
      if( !ctrlPressed )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( ( dataGridControl == null ) || ( dataGridControl.NavigationBehavior != NavigationBehavior.CellOnly ) )
        return;

      NavigationHelper.MoveFocusToLastVisibleColumn( dataGridControl.CurrentContext );

      e.Handled = true;
    }

    protected virtual void HandleLeftKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      e.Handled = NavigationHelper.MoveFocusLeft( dataGridControl.CurrentContext );
    }

    protected virtual void HandleRightKey( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      var dataGridControl = this.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      e.Handled = NavigationHelper.MoveFocusRight( dataGridControl.CurrentContext );
    }

    #endregion

    #region BringIntoView Methods

    private void OnRequestBringIntoView( object sender, RequestBringIntoViewEventArgs e )
    {
      if( e.Handled )
        return;

      Debug.Assert( this == sender );

      var dataGridControl = this.ParentDataGridControl;
      if( dataGridControl == null )
        return;

      // Prevent a BringIntoView to be requested twice when setting the focus on a cell.
      if( dataGridControl.SettingFocusOnCell )
      {
        e.Handled = true;
        return;
      }

      // Let the ScrollViewer handle this if the grid isn't virtualized.
      var scrollViewer = TreeHelper.FindParent<ScrollViewer>( this );
      if( ( scrollViewer == null ) || !scrollViewer.CanContentScroll )
        return;

      var targetVisual = e.TargetObject as Visual;
      if( targetVisual == null )
        return;

      // Let the ScrollViewer handle this if the target is the container (i.e. HeaderFooterItem).
      var container = this.GetContainerFromElement( targetVisual );
      if( ( container == null ) || ( container == targetVisual ) )
        return;

      Debug.Assert( this.Children.Contains( container ) );

      e.Handled = true;

      var targetRect = e.TargetRect;
      if( targetRect.IsEmpty )
      {
        var uiElement = TreeHelper.FindParent<UIElement>( targetVisual, true );

        // If we have a container, we surely have a UIElement.
        Debug.Assert( uiElement != null );

        targetVisual = uiElement;
        targetRect = new Rect( 0d, 0d, Math.Min( uiElement.RenderSize.Width, scrollViewer.ViewportWidth ), 0d );
      }

      var area = targetVisual.TransformToAncestor( container ).TransformBounds( targetRect );

      if( area.Left < scrollViewer.HorizontalOffset )
      {
        scrollViewer.ScrollToHorizontalOffset( area.Left );
      }
      else if( area.Right > scrollViewer.HorizontalOffset + scrollViewer.ViewportWidth )
      {
        scrollViewer.ScrollToHorizontalOffset( Math.Min( area.Left, ( area.Right - scrollViewer.ViewportWidth ) ) );
      }
    }

    #endregion

    protected override Size ArrangeOverride( Size arrangeSize )
    {
      Size finalSize = base.ArrangeOverride( arrangeSize );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      DataGridControl dataGridControl = ( dataGridContext != null )
        ? dataGridContext.DataGridControl
        : null;

      RowSelectorPane rowSelectorPane = null;

      // dataGridControl can be null in design-time
      if( dataGridControl != null )
      {
        TableViewScrollViewer scrollViewer = dataGridControl.ScrollViewer as TableViewScrollViewer;
        rowSelectorPane = ( scrollViewer != null ) ? scrollViewer.RowSelectorPane : null;
      }

      if( rowSelectorPane == null )
        return finalSize;

      Visibility rowSelectorPaneVisibility = ( Visibility )rowSelectorPane.GetValue( RowSelectorPane.VisibilityProperty );

      if( rowSelectorPaneVisibility != Visibility.Visible )
        return finalSize;

      foreach( UIElement child in this.InternalChildren )
      {
        Vector offset = VisualTreeHelper.GetOffset( child );
        Size desiredSize = child.DesiredSize;

        rowSelectorPane.SetRowSelectorPosition( child, new Rect( offset.X, offset.Y, desiredSize.Width, desiredSize.Height ), this );
      }

      return finalSize;
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      Size restrictedMeasureSize;
      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( this );

      if( this.Orientation == Orientation.Vertical )
      {
        restrictedMeasureSize = new Size( availableSize.Width, double.PositiveInfinity );

        if( availableSize.Width != Double.PositiveInfinity )
          dataGridContext.FixedHeaderFooterViewPortSize = restrictedMeasureSize;
      }
      else
      {
        restrictedMeasureSize = new Size( double.PositiveInfinity, availableSize.Height );

        if( availableSize.Height != Double.PositiveInfinity )
          dataGridContext.FixedHeaderFooterViewPortSize = restrictedMeasureSize;
      }

      foreach( UIElement child in this.Children )
      {
        if( ( child is HeaderFooterItem ) && ( typeof( Row ).IsAssignableFrom( ( ( HeaderFooterItem )child ).VisualRootElementType ) ) )
        {
          dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated = false;

          // Calling Measure with the Viewport's width will have the effect of 
          // distributing the extra space (see FixedCellPanel's MeasureOverride). 
          // Eventually, the FixedCellPanel will receive an adjusted viewport 
          // width (where GroupLevelIndicator's width et al will be substracted).
          child.Measure( restrictedMeasureSize );

          if( dataGridContext.ColumnStretchingManager.ColumnStretchingCalculated )
            break;
        }
      }

      return base.MeasureOverride( availableSize );
    }

    private UIElement GetContainerFromElement( DependencyObject element )
    {
      if( element == null )
        return null;

      var uiElement = TreeHelper.FindParent<UIElement>( element, true );
      if( uiElement == null )
        return null;

      if( this.Children.Contains( uiElement ) )
        return uiElement;

      foreach( UIElement container in this.Children )
      {
        if( container.IsAncestorOf( uiElement ) )
          return container;
      }

      return null;
    }
  }
}
