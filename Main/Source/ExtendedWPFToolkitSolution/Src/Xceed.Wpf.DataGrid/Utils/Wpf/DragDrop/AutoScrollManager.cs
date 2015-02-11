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
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal class AutoScrollManager : DependencyObject
  {
    public const int AutoScrollInterval_DefaultValue = 50;
    public const int AutoScrollTreshold_DefaultValue = 5;

    public AutoScrollManager(ScrollViewer scrollViewer)
    {
      if( scrollViewer == null )
        throw new ArgumentNullException( "scrollViewer" );

      m_scrollViewer = scrollViewer;
      m_timer = new System.Windows.Threading.DispatcherTimer();
      m_timer.Interval = new TimeSpan( 0, 0, 0, 0, 0 );
      m_timer.Tick += new EventHandler( this.OnAutoScrollTimer_Tick );

      this.AutoScrollInterval = AutoScrollInterval_DefaultValue;
      this.AutoScrollTreshold = AutoScrollTreshold_DefaultValue;
    }

    #region AutoScrollInterval Property

    public int AutoScrollInterval
    {
      get;
      set;
    }

    #endregion

    #region AutoScrollTreshold Property

    public int AutoScrollTreshold
    {
      get;
      set;
    }

    #endregion

    #region property ScrollViewer Property

    public ScrollViewer ScrollViewer
    {
      get { return m_scrollViewer; }
    }

    #endregion

    public event EventHandler AutoScrolled;

    private void OnAutoScrollTimer_Tick( object sender, EventArgs e )
    {
      this.PerformAutoScroll();
    }

    public void StopAutoScroll()
    {
      m_timer.Stop();
    }

    internal void ProcessMouseMove( MouseEventArgs e )
    {
      Point clientMousePosition = e.GetPosition( m_scrollViewer );

      Size scrollViewerRenderSize = m_scrollViewer.RenderSize;

      m_autoScrollDirection = AutoScrollDirection.None;
      if( m_scrollViewer.ScrollableWidth > 0 && scrollViewerRenderSize.Width > 0)
      {
        double mouseEdgeDistance = 0d;
        if( ( clientMousePosition.X < AutoScrollTreshold ) && ( m_scrollViewer.HorizontalOffset > 0 ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Left;
          mouseEdgeDistance = clientMousePosition.X - AutoScrollTreshold;
        }
        //Scrolling right
        else if( ( clientMousePosition.X > scrollViewerRenderSize.Width - AutoScrollTreshold ) && ( m_scrollViewer.HorizontalOffset < m_scrollViewer.ScrollableWidth ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Right;
          mouseEdgeDistance = ( clientMousePosition.X - ( scrollViewerRenderSize.Width - AutoScrollTreshold ) );
        }

        //We need a scroll value in units that can be pixels or units (eg. rows)
        //Store a distance ratio based mouse edge distance in relation to the view width
        m_horizontalPageScrollRatio = mouseEdgeDistance / scrollViewerRenderSize.Width;
      }

      if( m_scrollViewer.ScrollableHeight > 0 )
      {
        if( ( clientMousePosition.Y < AutoScrollTreshold ) && ( m_scrollViewer.VerticalOffset > 0 ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Up;
        }
        else if( ( clientMousePosition.Y > scrollViewerRenderSize.Height - AutoScrollTreshold )
            && ( m_scrollViewer.VerticalOffset < m_scrollViewer.ScrollableHeight ) )
        {
          m_autoScrollDirection |= AutoScrollDirection.Down;
        }
      }

      if( m_autoScrollDirection == AutoScrollDirection.None )
      {
        this.StopAutoScroll();
      }
      else
      {
        // The DispatcherTimer is not a priority event. The Tick event won't fire while  the user moves the mouse pointer. That's why we manually call the 
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
      if(  m_timer.IsEnabled )
      {
        TimeSpan timeSpanSinceLastScroll = ( TimeSpan )( DateTime.UtcNow - m_lastAutoScrollTime );

        // This method may be called before its time (on the MouseMove event). We make sure that the AutoScroll is not performed before the desired time span has elapsed.
        if( timeSpanSinceLastScroll.Milliseconds >= AutoScrollInterval )
        {
          double scrollUnits = ( m_horizontalPageScrollRatio * m_scrollViewer.ViewportWidth );

          if( ( m_autoScrollDirection & AutoScrollDirection.Left ) == AutoScrollDirection.Left )
          {
            //Minimum 1 unit
            scrollUnits = System.Math.Min( -1d, scrollUnits );
            double scrollOffset = m_scrollViewer.HorizontalOffset + scrollUnits;

            //Maximum 1 page
            if(m_horizontalPageScrollRatio <= -1d)
            {
              m_scrollViewer.PageLeft();
            }
            else
            {
              //Make sure the grid stops scrolling.
              if( scrollOffset < 0 )
              {
                scrollOffset = 0;
              }
              m_scrollViewer.ScrollToHorizontalOffset( scrollOffset );
            }
          }
          else if( ( m_autoScrollDirection & AutoScrollDirection.Right ) == AutoScrollDirection.Right )
          {
            //Minimum 1 unit
            scrollUnits = System.Math.Max( 1d, scrollUnits );
            double scrollOffset = m_scrollViewer.HorizontalOffset + scrollUnits;

            //Maxium 1 page
            if( m_horizontalPageScrollRatio >= 1d )
            {
              m_scrollViewer.PageRight();
            }
            else
            {
              //Make sure the grid does not scroll pass the last right column.
              if( scrollOffset > m_scrollViewer.ScrollableWidth )
              {
                scrollOffset = m_scrollViewer.ScrollableWidth;
              }
              m_scrollViewer.ScrollToHorizontalOffset( scrollOffset );
            }
          }
          else if( ( m_autoScrollDirection & AutoScrollDirection.Up ) == AutoScrollDirection.Up )
          {
            m_scrollViewer.LineUp();
          }
          else if( ( m_autoScrollDirection & AutoScrollDirection.Down ) == AutoScrollDirection.Down )
          {
            m_scrollViewer.LineDown();
          }

          m_lastAutoScrollTime = DateTime.UtcNow;

          if( this.AutoScrolled != null )
          {
            this.AutoScrolled( this, EventArgs.Empty );
          }

        }
      }
    }

    private AutoScrollDirection m_autoScrollDirection = AutoScrollDirection.None;
    private ScrollViewer m_scrollViewer;
    private DispatcherTimer m_timer = null;
    private DateTime m_lastAutoScrollTime;
    private double m_horizontalPageScrollRatio;

    [Flags()]
    private enum AutoScrollDirection
    {
      None = 0,
      Left = 1,
      Right = 2,
      Up = 4,
      Down = 8
    }
  }
}
