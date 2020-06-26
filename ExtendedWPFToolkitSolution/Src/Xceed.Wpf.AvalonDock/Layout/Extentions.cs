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

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Xceed.Wpf.AvalonDock.Layout
{
  public static class Extensions
  {
    #region Public Methods

    public static IEnumerable<ILayoutElement> Descendents( this ILayoutElement element )
    {
      var container = element as ILayoutContainer;
      if( container != null )
      {
        foreach( var childElement in container.Children )
        {
          yield return childElement;
          foreach( var childChildElement in childElement.Descendents() )
            yield return childChildElement;
        }
      }
    }

    public static T FindParent<T>( this ILayoutElement element ) //where T : ILayoutContainer
    {
      var parent = element.Parent;
      while( parent != null &&
          !( parent is T ) )
        parent = parent.Parent;


      return ( T )parent;
    }

    public static ILayoutRoot GetRoot( this ILayoutElement element ) //where T : ILayoutContainer
    {
      if( element is ILayoutRoot )
        return element as ILayoutRoot;

      var parent = element.Parent;
      while( parent != null &&
          !( parent is ILayoutRoot ) )
        parent = parent.Parent;

      return ( ILayoutRoot )parent;
    }

    public static bool ContainsChildOfType<T>( this ILayoutContainer element )
    {
      foreach( var childElement in element.Descendents() )
        if( childElement is T )
          return true;

      return false;
    }

    public static bool ContainsChildOfType<T, S>( this ILayoutContainer container )
    {
      foreach( var childElement in container.Descendents() )
        if( childElement is T || childElement is S )
          return true;

      return false;
    }

    public static bool IsOfType<T, S>( this ILayoutContainer container )
    {
      return container is T || container is S;
    }

    public static AnchorSide GetSide( this ILayoutElement element )
    {
      var parentContainer = element.Parent as ILayoutOrientableGroup;
      if( parentContainer != null )
      {
        var layoutPanel = parentContainer as LayoutPanel;
        if( layoutPanel == null )
        {
          layoutPanel = parentContainer.FindParent<LayoutPanel>();
        }

        if( (layoutPanel != null) && ( layoutPanel.Children.Count > 0 ) )
        {
          var childrenCount = layoutPanel.Children.Count;
          var currentChildID = -1;
          for( int i = 0; i < childrenCount; ++i )
          {
            if( layoutPanel.Children[ i ].Equals( element ) || layoutPanel.Children[ i ].Descendents().Contains( element ) )
            {
              currentChildID = i;
              break;
            }
          }

          if( layoutPanel.Orientation == System.Windows.Controls.Orientation.Horizontal )
          {
            if( currentChildID < 0 )
              return AnchorSide.Right;
            return ( currentChildID <= ( childrenCount / 2 ) ) ? AnchorSide.Left : AnchorSide.Right;
          }
          else
          {
            if( currentChildID < 0 )
              return AnchorSide.Bottom;
            return ( currentChildID <= ( childrenCount / 2 ) ) ? AnchorSide.Top : AnchorSide.Bottom;
          }
        }
      }

      Debug.Fail( "Unable to find the side for an element, possible layout problem!" );
      return AnchorSide.Right;
    }

    #endregion

    #region Internal Methods

    internal static void KeepInsideNearestMonitor( this ILayoutElementForFloatingWindow paneInsideFloatingWindow )
    {
      Win32Helper.RECT r = new Win32Helper.RECT();
      r.Left = ( int )paneInsideFloatingWindow.FloatingLeft;
      r.Top = ( int )paneInsideFloatingWindow.FloatingTop;
      r.Bottom = r.Top + ( int )paneInsideFloatingWindow.FloatingHeight;
      r.Right = r.Left + ( int )paneInsideFloatingWindow.FloatingWidth;

      uint MONITOR_DEFAULTTONEAREST = 0x00000002;
      uint MONITOR_DEFAULTTONULL = 0x00000000;

      System.IntPtr monitor = Win32Helper.MonitorFromRect( ref r, MONITOR_DEFAULTTONULL );
      if( monitor == System.IntPtr.Zero )
      {
        System.IntPtr nearestmonitor = Win32Helper.MonitorFromRect( ref r, MONITOR_DEFAULTTONEAREST );
        if( nearestmonitor != System.IntPtr.Zero )
        {
          Win32Helper.MonitorInfo monitorInfo = new Win32Helper.MonitorInfo();
          monitorInfo.Size = Marshal.SizeOf( monitorInfo );
          Win32Helper.GetMonitorInfo( nearestmonitor, monitorInfo );

          if( paneInsideFloatingWindow.FloatingLeft < monitorInfo.Work.Left )
          {
            paneInsideFloatingWindow.FloatingLeft = monitorInfo.Work.Left + 10;
          }

          if( paneInsideFloatingWindow.FloatingLeft + paneInsideFloatingWindow.FloatingWidth > monitorInfo.Work.Right )
          {
            paneInsideFloatingWindow.FloatingLeft = monitorInfo.Work.Right - ( paneInsideFloatingWindow.FloatingWidth + 10 );
          }

          if( paneInsideFloatingWindow.FloatingTop < monitorInfo.Work.Top )
          {
            paneInsideFloatingWindow.FloatingTop = monitorInfo.Work.Top + 10;
          }

          if( paneInsideFloatingWindow.FloatingTop + paneInsideFloatingWindow.FloatingHeight > monitorInfo.Work.Bottom )
          {
            paneInsideFloatingWindow.FloatingTop = monitorInfo.Work.Bottom - ( paneInsideFloatingWindow.FloatingHeight + 10 );
          }
        }
      }

    }

    #endregion
  }
}
