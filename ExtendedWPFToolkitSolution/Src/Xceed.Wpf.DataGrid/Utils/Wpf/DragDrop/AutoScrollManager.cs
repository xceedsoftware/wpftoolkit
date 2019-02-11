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
using System.Windows.Media;
using System.Windows.Threading;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal sealed class AutoScrollManager : DependencyObject
  {
    #region Static Fields

    internal static readonly TimeSpan AutoScrollInterval_DefaultValue = TimeSpan.FromMilliseconds( 50d );
    internal const int AutoScrollThreshold_DefaultValue = 5;

    #endregion

    internal AutoScrollManager( ScrollViewer scrollViewer )
    {
      if( scrollViewer == null )
        throw new ArgumentNullException( "scrollViewer" );

      m_scrollViewer = scrollViewer;

      m_timer = new System.Windows.Threading.DispatcherTimer();
      m_timer.Interval = AutoScrollManager.AutoScrollInterval_DefaultValue;
      m_timer.Tick += new EventHandler( this.OnAutoScrollTimer_Tick );

      this.AutoScrollInterval = AutoScrollManager.AutoScrollInterval_DefaultValue;
      this.AutoScrollThreshold = AutoScrollManager.AutoScrollThreshold_DefaultValue;
    }

    #region AutoScrollInterval Property

    internal TimeSpan AutoScrollInterval
    {
      get;
      set;
    }

    #endregion

    #region AutoScrollThreshold Property

    internal int AutoScrollThreshold
    {
      get;
      set;
    }

    #endregion

    #region ScrollViewer Internal Property

    internal ScrollViewer ScrollViewer
    {
      get
      {
        return m_scrollViewer;
      }
    }

    #endregion

    #region AutoScrolled Internal Event

    internal event EventHandler AutoScrolled;

    private void OnAutoScrolled()
    {
      var handler = this.AutoScrolled;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    #endregion

    internal void Start()
    {
      m_isStarted = true;
    }

    internal void Stop()
    {
      m_isStarted = false;
      m_timer.Stop();
    }

    internal void HandleMouseMove( MouseEventArgs e )
    {
      if( !m_isStarted )
        return;

      this.HandleMove( e.GetPosition );
    }

    internal void HandleMove( Func<IInputElement, Point> getCursorPosition )
    {
      if( !m_isStarted )
        return;

      var position = getCursorPosition.Invoke( m_scrollViewer );
      var renderSize = m_scrollViewer.RenderSize;
      var treshold = this.AutoScrollThreshold;
      var horizontalSpeed = 0d;
      var verticalSpeed = 0d;

      m_autoScrollDirection = AutoScrollDirection.None;

      if( ( m_scrollViewer.ScrollableWidth > 0d ) && ( m_scrollViewer.ViewportWidth > 0d ) )
      {
        var distance = 0d;

        if( ( position.X < treshold ) && ( m_scrollViewer.HorizontalOffset > 0d ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Left;
          distance = position.X - treshold;
        }
        else if( ( position.X > renderSize.Width - treshold ) && ( m_scrollViewer.HorizontalOffset < m_scrollViewer.ScrollableWidth ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Right;
          distance = position.X - ( renderSize.Width - treshold );
        }

        horizontalSpeed = distance / m_scrollViewer.ViewportWidth;
      }

      if( ( m_scrollViewer.ScrollableHeight > 0d ) && ( m_scrollViewer.ViewportHeight > 0d ) )
      {
        var distance = 0d;

        if( ( position.Y < treshold ) && ( m_scrollViewer.VerticalOffset > 0d ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Up;
          distance = position.Y - treshold;
        }
        else if( ( position.Y > renderSize.Height - treshold ) && ( m_scrollViewer.VerticalOffset < m_scrollViewer.ScrollableHeight ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Down;
          distance = position.Y - ( renderSize.Height - treshold );
        }

        verticalSpeed = distance / m_scrollViewer.ViewportHeight;
      }

      m_scrollSpeed = new Vector( horizontalSpeed, verticalSpeed );

      if( m_autoScrollDirection == AutoScrollDirection.None )
      {
        m_timer.Stop();
      }
      else
      {
        // The DispatcherTimer is not a priority event. The Tick event won't fire while the user moves the mouse pointer. That's why we manually call the 
        // PerformAutoScroll method (on each MouseMove, i.e. Drag). When the user doesn't move the mouse pointer, the timer will take over to do the AutoScroll.
        if( !m_timer.IsEnabled )
        {
          m_timer.Start();
        }

        this.PerformAutoScroll();
      }
    }

    private void PerformAutoScroll()
    {
      if( !m_timer.IsEnabled )
        return;

      var elapsedTime = DateTime.UtcNow - m_timestamp;
      if( elapsedTime < this.AutoScrollInterval )
        return;

      var scaleFactor = Matrix.Identity;
      scaleFactor.Scale( m_scrollViewer.ViewportWidth, m_scrollViewer.ViewportHeight );

      var scrollUnits = m_scrollSpeed * scaleFactor;

      if( ( m_autoScrollDirection & AutoScrollDirection.Left ) == AutoScrollDirection.Left )
      {
        if( m_scrollSpeed.X <= -1d )
        {
          m_scrollViewer.PageLeft();
        }
        else
        {
          var unit = System.Math.Min( -1d, scrollUnits.X );
          var offset = System.Math.Max( 0d, m_scrollViewer.HorizontalOffset + unit );

          m_scrollViewer.ScrollToHorizontalOffset( offset );
        }
      }
      else if( ( m_autoScrollDirection & AutoScrollDirection.Right ) == AutoScrollDirection.Right )
      {
        if( m_scrollSpeed.X >= 1d )
        {
          m_scrollViewer.PageRight();
        }
        else
        {
          var unit = System.Math.Max( 1d, scrollUnits.X );
          var offset = System.Math.Min( m_scrollViewer.ScrollableWidth, m_scrollViewer.HorizontalOffset + unit );

          m_scrollViewer.ScrollToHorizontalOffset( offset );
        }
      }

      if( ( m_autoScrollDirection & AutoScrollDirection.Up ) == AutoScrollDirection.Up )
      {
        if( m_scrollSpeed.Y <= -1d )
        {
          m_scrollViewer.PageUp();
        }
        else
        {
          var unit = System.Math.Min( -1d, scrollUnits.Y );
          var offset = System.Math.Max( 0d, m_scrollViewer.VerticalOffset + unit );

          m_scrollViewer.ScrollToVerticalOffset( offset );
        }
      }
      else if( ( m_autoScrollDirection & AutoScrollDirection.Down ) == AutoScrollDirection.Down )
      {
        if( m_scrollSpeed.Y >= 1d )
        {
          m_scrollViewer.PageDown();
        }
        else
        {
          var unit = System.Math.Max( 1d, scrollUnits.Y );
          var offset = System.Math.Min( m_scrollViewer.ScrollableHeight, m_scrollViewer.VerticalOffset + unit );

          m_scrollViewer.ScrollToVerticalOffset( offset );
        }
      }

      m_timestamp = DateTime.UtcNow;

      this.OnAutoScrolled();
    }

    private void OnAutoScrollTimer_Tick( object sender, EventArgs e )
    {
      if( m_isStarted )
      {
        this.PerformAutoScroll();
      }
      else
      {
        ( ( DispatcherTimer )sender ).Stop();
      }
    }

    private AutoScrollDirection m_autoScrollDirection = AutoScrollDirection.None;
    private readonly ScrollViewer m_scrollViewer;
    private readonly DispatcherTimer m_timer;
    private DateTime m_timestamp;
    private Vector m_scrollSpeed = new Vector( 0d, 0d );
    private bool m_isStarted; //false

    #region AutoScrollManager Private Enum

    [Flags()]
    private enum AutoScrollDirection
    {
      None = 0,
      Left = 1,
      Right = 2,
      Up = 4,
      Down = 8
    }

    #endregion
  }
}
