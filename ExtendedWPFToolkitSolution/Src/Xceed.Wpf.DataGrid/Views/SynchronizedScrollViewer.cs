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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Utils.Math;

namespace Xceed.Wpf.DataGrid.Views
{
  public class SynchronizedScrollViewer : ScrollViewer, DataGridScrollViewer.IDeferableScrollChanged
  {
    #region Constructors

    static SynchronizedScrollViewer()
    {
      ScrollViewer.HorizontalScrollBarVisibilityProperty.OverrideMetadata( typeof( SynchronizedScrollViewer ), new FrameworkPropertyMetadata( ScrollBarVisibility.Hidden ) );
      ScrollViewer.VerticalScrollBarVisibilityProperty.OverrideMetadata( typeof( SynchronizedScrollViewer ), new FrameworkPropertyMetadata( ScrollBarVisibility.Hidden ) );

      // By default, we never want item scrolling.
      ScrollViewer.CanContentScrollProperty.OverrideMetadata( typeof( SynchronizedScrollViewer ), new FrameworkPropertyMetadata( false ) );
    }

    public SynchronizedScrollViewer()
    {
      // ScrollViewer binds these three properties to the TemplatedParent properties. 
      // Changing their default value using only OverrideMetadata would not work.
      this.HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
    }

    #endregion

    #region DataGridScrollViewer.IDeferableScrollChanged Implementation

    bool DataGridScrollViewer.IDeferableScrollChanged.DeferScrollChanged
    {
      get
      {
        return m_deferScrollChange;
      }
      set
      {

        var valueChanged = ( m_deferScrollChange != value );

        m_deferScrollChange = value;

        if( m_mainScrollViewer == null )
          return;

        if( !m_deferScrollChange && valueChanged )
        {
          var scrollOrientation = this.ScrollOrientation;

          if( ( scrollOrientation & ScrollOrientation.Horizontal ) == ScrollOrientation.Horizontal )
          {
            this.ScrollToHorizontalOffset( m_mainScrollViewer.HorizontalOffset );
          }

          if( ( scrollOrientation & ScrollOrientation.Vertical ) == ScrollOrientation.Vertical )
          {
            this.ScrollToVerticalOffset( m_mainScrollViewer.VerticalOffset );
          }
        }
      }
    }

    #endregion

    #region ScrollOrientation property

    public static readonly DependencyProperty ScrollOrientationProperty = DependencyProperty.Register(
      "ScrollOrientation",
      typeof( ScrollOrientation ),
      typeof( SynchronizedScrollViewer ),
      new PropertyMetadata( ScrollOrientation.Horizontal ) );

    public ScrollOrientation ScrollOrientation
    {
      get
      {
        return ( ScrollOrientation )this.GetValue( SynchronizedScrollViewer.ScrollOrientationProperty );
      }

      set
      {
        this.SetValue( SynchronizedScrollViewer.ScrollOrientationProperty, value );
      }
    }

    #endregion

    #region LimitScrolling Internal Property

    internal static readonly DependencyProperty LimitScrollingProperty = DependencyProperty.Register(
      "LimitScrolling",
      typeof( bool ),
      typeof( SynchronizedScrollViewer ),
      new UIPropertyMetadata( true ) );

    internal bool LimitScrolling
    {
      get
      {
        return ( bool )this.GetValue( SynchronizedScrollViewer.LimitScrollingProperty );
      }
      set
      {
        this.SetValue( SynchronizedScrollViewer.LimitScrollingProperty, value );
      }
    }

    #endregion

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( this.TemplatedParent != m_mainScrollViewer )
      {
        if( m_mainScrollViewer != null )
        {
          m_mainScrollViewer.ScrollChanged -= new ScrollChangedEventHandler( this.OnMainScrollViewer_ScrollChanged );
        }

        m_mainScrollViewer = this.TemplatedParent as ScrollViewer;

        if( m_mainScrollViewer != null )
        {
          m_mainScrollViewer.ScrollChanged += new ScrollChangedEventHandler( this.OnMainScrollViewer_ScrollChanged );
        }
      }
    }

    protected override void OnScrollChanged( ScrollChangedEventArgs e )
    {
      e.Handled = true;

      base.OnScrollChanged( e );

      // m_mainScrollViewer will remain null in design mode and if it's null, nothing to update
      if( m_mainScrollViewer == null )
        return;

      // Sometimes, a row in the SynchronizedScrollViewer will scroll by itself,
      // not triggered by the main ScrollViewer scrolling. In that case, we want
      // to update the main ScrollViewer. A typical example is when a Row's cell is 
      // brought into view by, let's say, activating the cell editor.
      var invalidateMainScrollViewerMeasure = false;

      // If the Extent is 0, there is no reason to update the main ScrollViewer offset since this means there are no children.
      if( this.CanScrollHorizontally() )
      {
        invalidateMainScrollViewerMeasure = invalidateMainScrollViewerMeasure || !DoubleUtil.AreClose( 0, e.ExtentWidthChange );

        this.BeginUpdateMainScrollViewer( e );
      }

      // If the Extent is 0, there is no reason to update the main ScrollViewer offset since this means there are no children.
      if( this.CanScrollVertically() )
      {
        invalidateMainScrollViewerMeasure = invalidateMainScrollViewerMeasure || !DoubleUtil.AreClose( 0, e.ExtentHeightChange );

        this.BeginUpdateMainScrollViewer( e );
      }

      // In some situations, the Extent*Change event is received AFTER the 
      // layout pass of the mainScrollViewer is done. Since the measure of the
      // mainScrollViewer uses the SynchronizedScrollViewer Extent size, we must
      // call InvalidateMeasure on the mainScrollViewer to ensure it is correctly
      // layouted
      if( invalidateMainScrollViewerMeasure )
      {
        m_mainScrollViewer.InvalidateMeasure();
      }
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      if( e.Handled )
        return;

      switch( e.Key )
      {
        // Handle the System key definition (basically with ALT key pressed)
        case Key.System:
          this.HandlePreviewSystemKey( e );
          break;

        case Key.Tab:
          this.HandlePreviewTabKey( e );
          break;

        case Key.PageUp:
          this.HandlePreviewPageUpKey( e );
          break;

        case Key.PageDown:
          this.HandlePreviewPageDownKey( e );
          break;

        case Key.Home:
          this.HandlePreviewHomeKey( e );
          break;

        case Key.End:
          this.HandlePreviewEndKey( e );
          break;

        case Key.Up:
          this.HandlePreviewUpKey( e );
          break;

        case Key.Down:
          this.HandlePreviewDownKey( e );
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

    protected virtual void HandlePreviewSystemKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePreviewTabKey( KeyEventArgs e )
    {
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

    protected virtual void HandlePreviewUpKey( KeyEventArgs e )
    {
      DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
      this.UpdateLayout();
    }

    protected virtual void HandlePreviewDownKey( KeyEventArgs e )
    {
      DataGridItemsHost.BringIntoViewKeyboardFocusedElement();
      this.UpdateLayout();
    }

    protected virtual void HandlePreviewPageUpKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePreviewPageDownKey( KeyEventArgs e )
    {
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
        // Handle the System key definition (basically with ALT key pressed)
        case Key.System:
          this.HandleSystemKey( e );
          break;

        case Key.Tab:
          this.HandleTabKey( e );
          break;

        case Key.PageUp:
          this.HandlePageUpKey( e );
          break;

        case Key.PageDown:
          this.HandlePageDownKey( e );
          break;

        case Key.Home:
          this.HandleHomeKey( e );
          break;

        case Key.End:
          this.HandleEndKey( e );
          break;

        case Key.Up:
          this.HandleUpKey( e );
          break;

        case Key.Down:
          this.HandleDownKey( e );
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

    protected virtual void HandleSystemKey( KeyEventArgs e )
    {
    }

    protected virtual void HandleTabKey( KeyEventArgs e )
    {
    }

    protected virtual void HandlePageUpKey( KeyEventArgs e )
    {
      // Ensure to at least process the PageUp as Key.Up
      // to avoid the TableViewScrollViewer to process the key
      // directly without moving the focus.
      e.Handled = DataGridItemsHost.ProcessMoveFocus( Key.Up );

      // We were not able to move focus out of the 
      // SynchronizedScrollViewer but the focus is still inside.
      // Mark the key as handled to avoid the DataGridScrollViewer
      // to process the PageUp.
      if( !e.Handled && this.IsKeyboardFocusWithin )
      {
        e.Handled = true;
      }
    }

    protected virtual void HandlePageDownKey( KeyEventArgs e )
    {
      // Ensure to at least process the PageDown as Key.Down
      // to avoid the TableViewScrollViewer to process the key
      // directly without moving the focus.
      e.Handled = DataGridItemsHost.ProcessMoveFocus( Key.Down );

      // We were not able to move focus out of the 
      // SynchronizedScrollViewer but the focus is still inside.
      // Mark the key as handled to avoid the DataGridScrollViewer
      // to process the PageDown.
      if( !e.Handled && this.IsKeyboardFocusWithin )
      {
        e.Handled = true;
      }
    }

    protected virtual void HandleHomeKey( KeyEventArgs e )
    {
    }

    protected virtual void HandleEndKey( KeyEventArgs e )
    {
    }

    protected virtual void HandleUpKey( KeyEventArgs e )
    {
      e.Handled = DataGridItemsHost.ProcessMoveFocus( e.Key );
    }

    protected virtual void HandleDownKey( KeyEventArgs e )
    {
      e.Handled = DataGridItemsHost.ProcessMoveFocus( e.Key );
    }

    protected virtual void HandleLeftKey( KeyEventArgs e )
    {
      e.Handled = DataGridItemsHost.ProcessMoveFocus( e.Key );
    }

    protected virtual void HandleRightKey( KeyEventArgs e )
    {
      e.Handled = DataGridItemsHost.ProcessMoveFocus( e.Key );
    }

    private bool CanScrollHorizontally()
    {
      return ( ( this.ScrollOrientation & ScrollOrientation.Horizontal ) == ScrollOrientation.Horizontal )
          && ( this.ExtentWidth > 0d );
    }

    private bool CanScrollVertically()
    {
      return ( ( this.ScrollOrientation & ScrollOrientation.Vertical ) == ScrollOrientation.Vertical )
          && ( this.ExtentHeight > 0d );
    }

    private void BeginUpdateMainScrollViewer( ScrollChangedEventArgs e )
    {
      if( m_scrollChangedEventArgs == null )
      {
        m_scrollChangedEventArgs = e;
      }

      if( m_updateMainScrollViewer == null )
      {
        m_updateMainScrollViewer = this.Dispatcher.BeginInvoke( new Action( this.UpdateMainScrollViewer ), DispatcherPriority.Send );
      }
    }

    private void UpdateMainScrollViewer()
    {
      var e = m_scrollChangedEventArgs;

      m_updateMainScrollViewer = null;
      m_scrollChangedEventArgs = null;

      if( ( m_mainScrollViewer == null ) || ( e == null ) )
        return;

      if( this.CanScrollHorizontally() && ( e.HorizontalOffset != m_mainScrollViewer.HorizontalOffset ) )
      {
        m_mainScrollViewer.ScrollToHorizontalOffset( e.HorizontalOffset );
      }

      if( this.CanScrollVertically() && ( e.VerticalOffset != m_mainScrollViewer.VerticalOffset ) )
      {
        m_mainScrollViewer.ScrollToVerticalOffset( e.VerticalOffset );
      }
    }

    private void OnMainScrollViewer_ScrollChanged( object sender, ScrollChangedEventArgs e )
    {
      if( ( e.OriginalSource != m_mainScrollViewer ) || m_deferScrollChange )
        return;

      var orientation = this.ScrollOrientation;
      var limitScrolling = this.LimitScrolling;

      // If the Extent is 0, there is no reason to update the main ScrollViewer offset since this means there are no children.
      if( this.CanScrollHorizontally() )
      {
        var offset = ( limitScrolling ) ? Math.Min( e.HorizontalOffset, this.ExtentWidth - this.ViewportWidth ) : e.HorizontalOffset;

        if( offset != this.HorizontalOffset )
        {
          // We keep the original ScrollChangedEventArgs because when we'll update
          // our offset, it might be less than what was asked in the first place. Even
          // if this doesn't make sense to us, it might be ok for the m_mainScrollViewer
          // to scroll to that value. Since changing our offset will trigger the OnScrollChanged
          // that will update the m_mainScrollViewer, we want to changer it's offset to 
          // the right value.
          m_scrollChangedEventArgs = e;
          this.ScrollToHorizontalOffset( offset );
        }
      }

      // If the Extent is 0, there is no reason to update the main ScrollViewer offset since this means there are no children.
      if( this.CanScrollVertically() )
      {
        var offset = ( limitScrolling ) ? Math.Min( e.VerticalOffset, this.ExtentHeight - this.ViewportHeight ) : e.VerticalOffset;

        if( offset != this.VerticalOffset )
        {
          // We keep the original ScrollChangedEventArgs because when we'll update
          // our offset, it might be less than what was asked in the first place. Even
          // if this doesn't make sense to us, it might be ok for the m_mainScrollViewer
          // to scroll to that value. Since changing our offset will trigger the OnScrollChanged
          // that will update the m_mainScrollViewer, we want to changer it's offset to 
          // the right value.
          m_scrollChangedEventArgs = e;
          this.ScrollToVerticalOffset( offset );
        }
      }
    }

    #region Private Fields

    private ScrollViewer m_mainScrollViewer; //null
    private bool m_deferScrollChange; //false 
    private ScrollChangedEventArgs m_scrollChangedEventArgs; //null
    private DispatcherOperation m_updateMainScrollViewer; //null

    #endregion
  }
}
