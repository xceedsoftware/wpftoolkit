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
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class ScrollViewerTemplateHelper : IDisposable
  {

    public ScrollViewerTemplateHelper(
      ScrollViewer scrollViewer,
      Action<Orientation> dragScrollBeginCallback,
      Action<Orientation> dragScrollEndCallback )
    {
      if( scrollViewer == null )
        throw new ArgumentNullException( "scrollViewer" );

      m_scrollViewer = scrollViewer;
      m_dragScrollBeginCallback = dragScrollBeginCallback;
      m_dragScrollEndCallback = dragScrollEndCallback;
    }

    #region VerticalScrollBar Property

    public ScrollBar VerticalScrollBar
    {
      get;
      private set;
    }

    #endregion

    #region HorizontalScrollBar Property

    public ScrollBar HorizontalScrollBar
    {
      get;
      private set;
    }

    #endregion

    public ScrollBar GetScrollBar( Orientation orientation )
    {
      return ( orientation == Orientation.Vertical )
        ? this.VerticalScrollBar
        : this.HorizontalScrollBar;
    }

    public void RefreshTemplate()
    {
      this.UnregisterEvents();

      var template = m_scrollViewer.Template;
      if( template != null )
      {
        this.HorizontalScrollBar = template.FindName( "PART_HorizontalScrollBar", m_scrollViewer ) as ScrollBar;
        this.VerticalScrollBar = template.FindName( "PART_VerticalScrollBar", m_scrollViewer ) as ScrollBar;
      }

      if( m_dragScrollBeginCallback != null || m_dragScrollEndCallback != null )
      {
        this.RegisterEvents();
      }
    }

    public void Dispose()
    {
      this.UnregisterEvents();

      m_dragScrollBeginCallback = null;
      m_dragScrollEndCallback = null;
    }

    private void RegisterEvents()
    {
      if( this.VerticalScrollBar != null )
      {
        this.VerticalScrollBar.IsMouseCaptureWithinChanged += ScrollBar_IsMouseCaptureWithinChanged;
      }

      if( this.HorizontalScrollBar != null )
      {
        this.HorizontalScrollBar.IsMouseCaptureWithinChanged += ScrollBar_IsMouseCaptureWithinChanged;
      }
    }

    private void UnregisterEvents()
    {
      if( this.VerticalScrollBar != null )
      {
        this.VerticalScrollBar.IsMouseCaptureWithinChanged -= ScrollBar_IsMouseCaptureWithinChanged;
      }

      if( this.HorizontalScrollBar != null )
      {
        this.HorizontalScrollBar.IsMouseCaptureWithinChanged -= ScrollBar_IsMouseCaptureWithinChanged;
      }
    }

    private void ScrollBar_IsMouseCaptureWithinChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      var scrollBar = sender as ScrollBar;

      if( scrollBar == null )
        return;

      if( scrollBar != this.HorizontalScrollBar
        && scrollBar != this.VerticalScrollBar )
      {
        Debug.Fail( "Unknown scrollbar" );
        return;
      }

      var scrollThumb = Mouse.Captured as Thumb;

      if( scrollThumb == null )
        return;

      if( scrollBar.Track == null )
        return;

      if( scrollBar.Track.Thumb != scrollThumb )
        return;

      m_currentScrollOrientation = ( scrollBar == this.VerticalScrollBar ) ? Orientation.Vertical : Orientation.Horizontal;

      if( m_dragScrollEndCallback != null )
      {
        // Register to LostMouseCapture to be sure to hide the ScrollTip when the ScrollThumb lost the focus
        scrollThumb.LostMouseCapture += new MouseEventHandler( this.ScrollThumb_LostMouseCapture );
      }

      if( m_dragScrollBeginCallback != null )
      {

        m_dragScrollBeginCallback( m_currentScrollOrientation );
      }

    }


    private void ScrollThumb_LostMouseCapture( object sender, MouseEventArgs e )
    {
      Thumb scrollBarThumb = sender as Thumb;

      Debug.Assert( scrollBarThumb != null );

      if( scrollBarThumb != null )
        scrollBarThumb.LostMouseCapture -= new MouseEventHandler( this.ScrollThumb_LostMouseCapture );

      if( m_dragScrollEndCallback != null )
      {
        m_dragScrollEndCallback( m_currentScrollOrientation );
      }
    }

    #region Fields

    private ScrollViewer m_scrollViewer;
    private Action<Orientation> m_dragScrollBeginCallback;
    private Action<Orientation> m_dragScrollEndCallback;
    private Orientation m_currentScrollOrientation;
    #endregion

  }
}
