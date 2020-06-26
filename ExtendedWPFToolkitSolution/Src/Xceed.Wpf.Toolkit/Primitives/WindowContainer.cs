/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core;

namespace Xceed.Wpf.Toolkit.Primitives
{
  public class WindowContainer : Canvas
  {
    #region Constructors

    static WindowContainer()
    {
      // The default background must be transparent in order to be able to trap
      // all mouse events when a modal window is displayed.
      var defaultModalBackgroundBrush = new SolidColorBrush( Colors.Transparent );
      defaultModalBackgroundBrush.Freeze();
      ModalBackgroundBrushProperty = DependencyProperty.Register( "ModalBackgroundBrush", typeof( Brush ), typeof( WindowContainer ), new UIPropertyMetadata( defaultModalBackgroundBrush, OnModalBackgroundBrushChanged ) );
    }


    public WindowContainer()
    {
      this.SizeChanged += new SizeChangedEventHandler( this.WindowContainer_SizeChanged );
      this.LayoutUpdated += new EventHandler( this.WindowContainer_LayoutUpdated );
      this.Loaded += new RoutedEventHandler( WindowContainer_Loaded );
      this.ClipToBounds = true;
    }

    void WindowContainer_Loaded( object sender, RoutedEventArgs e )
    {
      foreach( WindowControl window in this.Children )
      {
        window.SetIsActiveInternal( false );
      }
      this.SetNextActiveWindow( null );
    }

    #endregion //Constructors

    #region Members

    private Brush _defaultBackgroundBrush;
    private bool _isModalBackgroundApplied;

    #endregion

    #region Properties

    #region ModalBackgroundBrush

    /// <summary>
    /// Identifies the ModalBackgroundBrush dependency property.
    /// </summary>
    // Initialized in the static constructor.
    public static readonly DependencyProperty ModalBackgroundBrushProperty;

    /// <summary>
    /// When using a modal window in the WindowContainer, a ModalBackgroundBrush can be set
    /// for the WindowContainer.
    /// </summary>
    public Brush ModalBackgroundBrush
    {
      get
      {
        return ( Brush )GetValue( ModalBackgroundBrushProperty );
      }
      set
      {
        SetValue( ModalBackgroundBrushProperty, value );
      }
    }

    private static void OnModalBackgroundBrushChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      WindowContainer windowContainer = ( WindowContainer )d;
      if( windowContainer != null )
        windowContainer.OnModalBackgroundBrushChanged( ( Brush )e.OldValue, ( Brush )e.NewValue );
    }

    protected virtual void OnModalBackgroundBrushChanged( Brush oldValue, Brush newValue )
    {
      this.SetModalBackground();
    }

    #endregion //ModalBackgroundBrush

    #endregion

    #region Base Class Override

    /// <summary>
    /// Measure the size of the WindowContainer based on its children.
    /// </summary>
    protected override Size MeasureOverride( Size constraint )
    {
      Size size = base.MeasureOverride( constraint );

      if( this.Children.Count > 0 )
      {
        double width = double.IsNaN( this.Width )
                        ? this.Children.OfType<WindowControl>().Max( ( w ) => w.Left + w.DesiredSize.Width )
                        : this.Width;
        double height = double.IsNaN( this.Height )
                        ? this.Children.OfType<WindowControl>().Max( ( w ) => w.Top + w.DesiredSize.Height )
                        : this.Height;
        return new Size( Math.Min( width, constraint.Width), Math.Min( height, constraint.Height) );
      }

      return size;
    }

    /// <summary>
    /// Register and unregister to children events.
    /// </summary>
    protected override void OnVisualChildrenChanged( DependencyObject visualAdded, DependencyObject visualRemoved )
    {
      base.OnVisualChildrenChanged( visualAdded, visualRemoved );

      if( visualAdded != null && !( visualAdded is WindowControl ) )
        throw new InvalidOperationException( "WindowContainer can only contain WindowControl types." );

      if( visualRemoved != null )
      {
        WindowControl removedChild = ( WindowControl )visualRemoved;
        removedChild.LeftChanged -= new EventHandler<EventArgs>( this.Child_LeftChanged );
        removedChild.TopChanged -= new EventHandler<EventArgs>( this.Child_TopChanged );
        removedChild.PreviewMouseLeftButtonDown -= new MouseButtonEventHandler( this.Child_PreviewMouseLeftButtonDown );
        removedChild.IsVisibleChanged -= new DependencyPropertyChangedEventHandler( this.Child_IsVisibleChanged );
        removedChild.IsKeyboardFocusWithinChanged -= new DependencyPropertyChangedEventHandler( this.Child_IsKeyboardFocusWithinChanged );
        if( removedChild is ChildWindow )
        {
          ( ( ChildWindow )removedChild ).IsModalChanged -= new EventHandler<EventArgs>( this.Child_IsModalChanged );
        }
      }

      if( visualAdded != null )
      {
        WindowControl addedChild = ( WindowControl )visualAdded;
        addedChild.LeftChanged += new EventHandler<EventArgs>( this.Child_LeftChanged );
        addedChild.TopChanged += new EventHandler<EventArgs>( this.Child_TopChanged );
        addedChild.PreviewMouseLeftButtonDown += new MouseButtonEventHandler( this.Child_PreviewMouseLeftButtonDown );
        addedChild.IsVisibleChanged += new DependencyPropertyChangedEventHandler( this.Child_IsVisibleChanged );
        addedChild.IsKeyboardFocusWithinChanged += new DependencyPropertyChangedEventHandler( this.Child_IsKeyboardFocusWithinChanged );
        if( addedChild is ChildWindow )
        {
          ( ( ChildWindow )addedChild ).IsModalChanged += new EventHandler<EventArgs>( this.Child_IsModalChanged );
        }
      }
    }

    #endregion

    #region Event Handler





    private void Child_LeftChanged( object sender, EventArgs e )
    {
      WindowControl windowControl = ( WindowControl )sender;
      if( windowControl != null )
      {
        windowControl.Left = this.GetRestrictedLeft( windowControl );
      }

      Canvas.SetLeft( windowControl, windowControl.Left );
    }

    private void Child_TopChanged( object sender, EventArgs e )
    {
      WindowControl windowControl = ( WindowControl )sender;
      if( windowControl != null )
      {
        windowControl.Top = this.GetRestrictedTop( windowControl );
      }

      Canvas.SetTop( windowControl, windowControl.Top );
    }

    private void Child_PreviewMouseLeftButtonDown( object sender, RoutedEventArgs e )
    {
      WindowControl windowControl = ( WindowControl )sender;

      WindowControl modalWindow = this.GetModalWindow();
      if( modalWindow == null )
      {
        this.SetNextActiveWindow( windowControl );
      }
    }

    private void Child_IsModalChanged( object sender, EventArgs e )
    {
      this.SetModalBackground();
    }

    private void Child_IsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      WindowControl windowControl = ( WindowControl )sender;

      //Do not give access to data behind the WindowContainer as long as any child of WindowContainer is visible.
      WindowControl firstVisibleChild = this.Children.OfType<WindowControl>().FirstOrDefault( ( x ) => x.Visibility == Visibility.Visible );
      this.IsHitTestVisible = ( firstVisibleChild != null );

      if( ( bool )e.NewValue )
      {
        this.SetChildPos( windowControl );
        this.SetNextActiveWindow( windowControl );
      }
      else
      {
        this.SetNextActiveWindow( null );
      }

      WindowControl modalWindow = this.GetModalWindow();
      foreach( WindowControl window in this.Children )
      {
        window.IsBlockMouseInputsPanelActive = ( modalWindow != null ) && !object.Equals( modalWindow, window );
      }

      this.SetModalBackground();
    }

    private void Child_IsKeyboardFocusWithinChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      WindowControl windowControl = ( WindowControl )sender;
      if( ( bool )e.NewValue )
      {
        this.SetNextActiveWindow( windowControl );
      }
    }

    private void WindowContainer_LayoutUpdated( object sender, EventArgs e )
    {
      foreach( WindowControl windowControl in this.Children )
      {
        //we only want to set the start position if this is the first time the control has bee initialized
        if( !windowControl.IsStartupPositionInitialized && ( windowControl.ActualWidth != 0 ) && ( windowControl.ActualHeight != 0 ) )
        {
          this.SetChildPos( windowControl );
          windowControl.IsStartupPositionInitialized = true;
        }
      }
    }

    private void WindowContainer_SizeChanged( object sender, SizeChangedEventArgs e )
    {
      foreach( WindowControl windowControl in this.Children )
      {
          //reposition our windows
          windowControl.Left = this.GetRestrictedLeft( windowControl );
          windowControl.Top = this.GetRestrictedTop( windowControl );
      }
    }

    private void ExpandWindowControl( WindowControl windowControl )
    {
      if( windowControl != null )
      {
        windowControl.Left = 0;
        windowControl.Top = 0;
        windowControl.Width = Math.Min( this.ActualWidth, windowControl.MaxWidth );
        windowControl.Height = Math.Min( this.ActualHeight, windowControl.MaxHeight );
      }
    }

    #endregion

    #region Private Methods

    private void SetChildPos( WindowControl windowControl )
    {
      // A MessageBox with no X and Y will be centered.
      // A ChildWindow with WindowStartupLocation == Center will be centered.
      if( ( ( windowControl is MessageBox ) && ( windowControl.Left == 0 ) && ( windowControl.Top == 0 ) )
        || ( ( windowControl is ChildWindow ) && ( ( ( ChildWindow )windowControl ).WindowStartupLocation == WindowStartupLocation.Center ) ) )
      {
        this.CenterChild( windowControl );
      }
      else
      {
        Canvas.SetLeft( windowControl, windowControl.Left );
        Canvas.SetTop( windowControl, windowControl.Top );
      }
    }

    private void CenterChild( WindowControl windowControl )
    {
      windowControl.UpdateLayout();

      if( ( windowControl.ActualWidth != 0 ) && ( windowControl.ActualHeight != 0 ) )
      {
        windowControl.Left = ( this.ActualWidth - windowControl.ActualWidth ) / 2.0;
        windowControl.Left += (windowControl.Margin.Left - windowControl.Margin.Right);
        windowControl.Top = ( this.ActualHeight - windowControl.ActualHeight ) / 2.0;
        windowControl.Top += ( windowControl.Margin.Top - windowControl.Margin.Bottom );
      }
    }

    private void SetNextActiveWindow( WindowControl windowControl )
    {
      if( !this.IsLoaded )
        return;

      if( this.IsModalWindow( windowControl ) )
      {
        this.BringToFront( windowControl );
      }
      else
      {
        WindowControl modalWindow = this.GetModalWindow();
        // Modal window is always in front
        if( modalWindow != null )
        {
          this.BringToFront( modalWindow );
        }
        else if( windowControl != null )
        {
          this.BringToFront( windowControl );
        }
        else
        {
          this.BringToFront( this.Children.OfType<WindowControl>()
                            .OrderByDescending( ( x ) => Canvas.GetZIndex( x ) )
                            .FirstOrDefault( ( x ) => x.Visibility == Visibility.Visible ) );
        }
      }
    }

    private void BringToFront( WindowControl windowControl )
    {
      if( windowControl != null )
      {
        int maxZIndez = this.Children.OfType<WindowControl>().Max( ( x ) => Canvas.GetZIndex( x ) );
        Canvas.SetZIndex( windowControl, maxZIndez + 1 );

        this.SetActiveWindow( windowControl );
      }
    }

    private void SetActiveWindow( WindowControl windowControl )
    {
      if( windowControl.IsActive )
        return;

      foreach( WindowControl window in this.Children )
      {
        window.SetIsActiveInternal( false );
      }
      windowControl.SetIsActiveInternal( true );
    }

    private bool IsModalWindow( WindowControl windowControl )
    {
      return ( ( ( windowControl is MessageBox ) && (windowControl.Visibility == Visibility.Visible) )
             || ( ( windowControl is ChildWindow ) && ( ( ChildWindow )windowControl ).IsModal && ( ( ChildWindow )windowControl).WindowState == WindowState.Open ) );
    }

    private WindowControl GetModalWindow()
    {
      return this.Children.OfType<WindowControl>()
                      .OrderByDescending( ( x ) => Canvas.GetZIndex( x ) )
                      .FirstOrDefault( ( x ) => IsModalWindow( x ) && (x.Visibility == Visibility.Visible) );
    }

    private double GetRestrictedLeft( WindowControl windowControl )
    {
      if( windowControl.Left < 0 )
        return 0;

      if( ( ( windowControl.Left + windowControl.ActualWidth ) > this.ActualWidth ) && ( this.ActualWidth != 0 ) )
      {
        double x = this.ActualWidth - windowControl.ActualWidth;
        return x < 0 ? 0 : x;
      }

      return windowControl.Left;
    }

    private double GetRestrictedTop( WindowControl windowControl )
    {
      if( windowControl.Top < 0 )
        return 0;

      if( ( ( windowControl.Top + windowControl.ActualHeight ) > this.ActualHeight ) && ( this.ActualHeight != 0 ) )
      {
        double y = this.ActualHeight - windowControl.ActualHeight;
        return y < 0 ? 0 : y;
      }

      return windowControl.Top;
    }

    private void SetModalBackground()
    {
      // We have a modal window and a ModalBackgroundBrush set.
      if( ( this.GetModalWindow() != null ) && ( this.ModalBackgroundBrush != null ) )
      {
        if( !_isModalBackgroundApplied )
        {
          _defaultBackgroundBrush = this.Background;
          _isModalBackgroundApplied = true;
        }

        this.Background = this.ModalBackgroundBrush;

      }
      else
      {
        if( _isModalBackgroundApplied )
        {
          this.Background = _defaultBackgroundBrush;
          _defaultBackgroundBrush = null;
          _isModalBackgroundApplied = false;
        }
      }
    }





    #endregion
  }


}
