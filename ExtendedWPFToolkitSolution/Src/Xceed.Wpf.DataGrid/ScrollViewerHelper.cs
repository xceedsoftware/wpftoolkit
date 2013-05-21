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
using System.Text;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using Xceed.Utils.Math;
using Xceed.Utils.Wpf;
using Xceed.Wpf.DataGrid.Views;

namespace Xceed.Wpf.DataGrid
{
  internal class ScrollViewerHelper
  {
    //-----------------  PROPERTIES -------------------------

    private static ScrollViewerHelper Current
    {
      get
      {
        if( m_singleInstance == null )
        {
          m_singleInstance = new ScrollViewerHelper();
        }

        return m_singleInstance;
      }
    }

    public static readonly int PixelScrollingCount = 16;

    //-----------------  PUBLIC METHODS -------------------------

    public static bool IsFocusInElement( FrameworkElement element )
    {
      if( element != null )
        return element.IsKeyboardFocusWithin;
      else
        return false;
    }

    public static FrameworkElement GetLastVisibleContainer( 
      DataGridControl gridControl,
      FrameworkElement container,
      ScrollViewer scrollViewer )
    {
      FrameworkElement retval = null;

      if( ScrollViewerHelper.IsPixelScrolling( gridControl, container, scrollViewer ) == false )
      {
        //This means that the panel is performing Item Scrolling

        //if the panel is Vertically scrolling the items (means the Horizontal Axis is Pixel scrolling)
        if( ScrollViewerHelper.GetItemScrollingOrientation( gridControl, container, scrollViewer ) == Orientation.Vertical )
        {
          retval = ScrollViewerHelper.ProcessLastVisibleContainer( gridControl,
            scrollViewer.VerticalOffset,
            scrollViewer.ViewportHeight,
            scrollViewer.HorizontalOffset,
            scrollViewer.ViewportWidth,
            Orientation.Vertical );
        }
        //the panel is Horizontally scrolling the items (means the Vertically Axis is Pixel scrolling)
        else
        {
          retval = ScrollViewerHelper.ProcessLastVisibleContainer( gridControl,
            scrollViewer.HorizontalOffset,
            scrollViewer.ViewportWidth,
            scrollViewer.VerticalOffset,
            scrollViewer.ViewportHeight,
            Orientation.Horizontal );
        }
      }
      else
      {
        Point pt = new Point();

        if( ( container is VirtualizingPanel )
            || ( container is DataGridItemsHost )
            || ( scrollViewer == null ) )
        {
          pt.X = 0;
          pt.Y = 0;
        }
        else
        {
          pt.X = scrollViewer.HorizontalOffset;
          pt.Y = scrollViewer.VerticalOffset;
        }

        Size size = new Size( ( scrollViewer != null ) ? scrollViewer.ViewportWidth : container.ActualWidth,
                              ( scrollViewer != null ) ? scrollViewer.ViewportHeight : container.ActualHeight );

        Rect visibleRect = new Rect( pt, size );

        RectangleGeometry geo = new RectangleGeometry( visibleRect );

        lock( ScrollViewerHelper.Current )
        {
          m_sVisibleChildList.Clear();
          m_sGridControl = gridControl;

          VisualTreeHelper.HitTest( container,
                                    new HitTestFilterCallback( ScrollViewerHelper.MyFilterFunct ),
                                    new HitTestResultCallback( ScrollViewerHelper.UselessResultCallback ),
                                    new GeometryHitTestParameters( geo ) );

          m_sGridControl = null;

          FrameworkElement preservedChild = null;
          Nullable<Vector> preservedOffset = null;

          foreach( FrameworkElement child in m_sVisibleChildList )
          {
            Vector itemOffset = VisualTreeHelper.GetOffset( child );

            Rect itemRect = new Rect( itemOffset.X, itemOffset.Y, child.ActualWidth, child.ActualHeight );

            itemRect.Intersect( visibleRect );

            switch( gridControl.ItemsPrimaryAxis )
            {
              case PrimaryAxis.Vertical:
                if( DoubleUtil.AreClose( itemRect.Width, 0 ) == false )
                {
                  if( DoubleUtil.AreClose( itemRect.Height, child.ActualHeight ) == true )
                  {
                    if( ScrollViewerHelper.IsABetterLastRow( preservedChild, preservedOffset, itemOffset ) == true )
                    {
                      preservedChild = child;
                      preservedOffset = itemOffset;
                    }
                  }
                }
                break;
              case PrimaryAxis.Horizontal:
                if( DoubleUtil.AreClose( itemRect.Width, child.ActualWidth ) == true )
                {
                  if( DoubleUtil.AreClose( itemRect.Height, 0 ) == false )
                  {
                    if( ScrollViewerHelper.IsABetterLastRow( preservedChild, preservedOffset, itemOffset ) == true )
                    {
                      preservedChild = child;
                      preservedOffset = itemOffset;
                    }
                  }
                }
                break;
              case PrimaryAxis.Both:
                if( ( DoubleUtil.AreClose( itemRect.Width, child.ActualWidth ) == true )
                  && ( DoubleUtil.AreClose( itemRect.Height, child.ActualHeight ) == true ) )
                {
                  if( ScrollViewerHelper.IsABetterLastRow( preservedChild, preservedOffset, itemOffset ) == true )
                  {
                    preservedChild = child;
                    preservedOffset = itemOffset;
                  }
                }
                break;
              case PrimaryAxis.None:
                if( itemRect.IsEmpty == false )
                {
                  if( ( DoubleUtil.AreClose( itemRect.Height, 0 ) == false ) && ( DoubleUtil.AreClose( itemRect.Width, 0 ) == false ) )
                  {
                    if( ScrollViewerHelper.IsABetterLastRow( preservedChild, preservedOffset, itemOffset ) == true )
                    {
                      preservedChild = child;
                      preservedOffset = itemOffset;
                    }
                  }
                }
                break;
            }
          }

          retval = preservedChild;

        }//end lock (protection of shared static members with GetFirstVisibleItem() )
      }


      return retval;
    }

    private static FrameworkElement ProcessLastVisibleContainer( 
      DataGridControl gridControl,
      double offset,
      double viewportSize,
      double opposedOffset,
      double opposedViewportSize,
      Orientation panelOrientation )
    {
      FrameworkElement retval = null;

      // Only needed if the DataGridControl contains items
      if( ( gridControl != null ) && ( gridControl.Items.Count > 0 ) )
      {
        bool qualifyingContainerFound = false;

        //retrieve the last object index according to item scrolling axis (this covers both the "Vertical and None" primary axis cases... 
        //as well as default if no other row matches the PrimaryAxis criteria)
        int runningIndex = ( int )( offset + viewportSize - 1 );

        //cycle for as long as a qualifying container is not found.
        while( qualifyingContainerFound == false )
        {
          retval = gridControl.GetContainerFromIndex( runningIndex ) as FrameworkElement;

          if( retval != null )
          {
            qualifyingContainerFound = ScrollViewerHelper.IsContainerQualifying( retval,
              gridControl,
              offset,
              viewportSize,
              opposedOffset,
              opposedViewportSize,
              panelOrientation );
          }

          //under all circumstances, if I am back at the original offset (first item visible), then have it qualify.
          if( runningIndex == offset )
            qualifyingContainerFound = true;

          runningIndex--;
        }
      }

      return retval;
    }

    private static FrameworkElement ProcessFirstVisibleContainer( 
      DataGridControl gridControl,
      double offset,
      double viewportSize,
      double opposedOffset,
      double opposedViewportSize,
      Orientation panelOrientation )
    {
      FrameworkElement retval = null;

      // Only needed if the DataGridControl contains items
      if( ( gridControl != null ) && ( gridControl.Items.Count > 0 ) )
      {
        bool qualifyingContainerFound = false;

        //retrieve the last object index according to item scrolling axis (this covers both the "Vertical and None" primary axis cases... 
        //as well as default if no other row matches the PrimaryAxis criteria)
        int runningIndex = ( int )( offset );

        //cycle for as long as a qualifying container is not found.
        while( !qualifyingContainerFound )
        {
          retval = gridControl.GetContainerFromIndex( runningIndex ) as FrameworkElement;

          //will be reverted back if the container does not match a particular condition.
          if( retval != null )
          {
            qualifyingContainerFound = ScrollViewerHelper.IsContainerQualifying( retval,
              gridControl,
              offset,
              viewportSize,
              opposedOffset,
              opposedViewportSize,
              panelOrientation );
          }

          //under all circumstances, if I am back at the end of the viewport ( last item visible), then have it qualify.
          if( runningIndex == ( offset + viewportSize - 1 ) )
            qualifyingContainerFound = true;

          runningIndex++;
        }
      }

      return retval;
    }

    private static bool IsContainerQualifying(
      FrameworkElement container,
      DataGridControl gridControl,
      double offset,
      double viewportSize,
      double opposedOffset,
      double opposedViewportSize,
      Orientation panelOrientation )
    {
      bool retval = true;

      HeaderFooterItem headerFooterItemContainer = container as HeaderFooterItem;
      Row rowContainer = container as Row;

      //Determine if the Element is Focusable or Navigable
      if( headerFooterItemContainer != null )
      {
        rowContainer = headerFooterItemContainer.AsVisual() as Row;
        if( rowContainer == null )
        {
          //If the HeaderFooter Item is not a Row and is Not focusable, then the item does not qualify.
          UIElement uiElementContainer = headerFooterItemContainer.AsVisual() as UIElement;
          if( ( uiElementContainer == null ) || ( !uiElementContainer.Focusable ) )
          {
            retval = false;
          }
        }
      }

      //If the container is a Row (in the headers footers region or not )
      if( ( retval ) && ( rowContainer != null ) )
      {
        //and the Row is not navigable, then it does not qualify.
        if( rowContainer.NavigationBehavior == NavigationBehavior.None )
        {
          retval = false;
        }
      }


      //If the container still qualifies after first verification, check for the scrolling axis.
      if( retval )
      {
        //if the PrimaryAxis requires the opposed axis to be fully visible.
        if( ( ( panelOrientation == Orientation.Vertical ) && ( ( gridControl.ItemsPrimaryAxis == PrimaryAxis.Horizontal ) || ( gridControl.ItemsPrimaryAxis == PrimaryAxis.Both ) ) )
          || ( ( panelOrientation == Orientation.Horizontal ) && ( ( gridControl.ItemsPrimaryAxis == PrimaryAxis.Vertical ) || ( gridControl.ItemsPrimaryAxis == PrimaryAxis.Both ) ) ) )
        {
          FrameworkElement frameworkElementContainer = container as FrameworkElement;
          //Somehow, I decided that a container that is not a FrameworkElement (extremelly highly unprobable) was automaticaly NOT to qualify if opposed axis was required.
          if( frameworkElementContainer != null )
          {
            Vector rowOffset = VisualTreeHelper.GetOffset( frameworkElementContainer );

            double computedOffset = ( panelOrientation == Orientation.Vertical ) ? rowOffset.X : rowOffset.Y;
            double computedSize = ( panelOrientation == Orientation.Vertical ) ? frameworkElementContainer.ActualWidth : frameworkElementContainer.ActualHeight;

            //if the coordinates of the Row are NOT inside the Scrolling Axis' viewport... then the item is not qualifyable.
            if( ( computedOffset < opposedOffset ) || ( ( computedOffset + computedSize ) > ( opposedViewportSize + opposedOffset ) ) )
            {
              retval = false;
            }
          }
          else
          {
            retval = false;
          }
        }
      }

      return retval;
    }

    public static FrameworkElement GetFirstVisibleContainer( 
      DataGridControl gridControl, 
      FrameworkElement container, 
      ScrollViewer scrollViewer )
    {
      FrameworkElement retval = null;

      if( !ScrollViewerHelper.IsPixelScrolling( gridControl, container, scrollViewer ) )
      {
        //This means that the panel is performing Item Scrolling

        //if the panel is Vertically scrolling the items (means the Horizontal Axis is Pixel scrolling)
        if( ScrollViewerHelper.GetItemScrollingOrientation( gridControl, container, scrollViewer ) == Orientation.Vertical )
        {
          retval = ScrollViewerHelper.ProcessFirstVisibleContainer( gridControl,
            scrollViewer.VerticalOffset,
            scrollViewer.ViewportHeight,
            scrollViewer.HorizontalOffset,
            scrollViewer.ViewportWidth,
            Orientation.Vertical );
        }
        //the panel is Horizontally scrolling the items (means the Vertically Axis is Pixel scrolling)
        else
        {
          retval = ScrollViewerHelper.ProcessFirstVisibleContainer( gridControl,
            scrollViewer.HorizontalOffset,
            scrollViewer.ViewportWidth,
            scrollViewer.VerticalOffset,
            scrollViewer.ViewportHeight,
            Orientation.Horizontal );
        }
      }
      else
      {
        Point pt = new Point();

        if( ( container is VirtualizingPanel )
            || ( container is DataGridItemsHost )
            || ( scrollViewer == null ) )
        {
          pt.X = 0;
          pt.Y = 0;
        }
        else
        {
          pt.X = scrollViewer.HorizontalOffset;
          pt.Y = scrollViewer.VerticalOffset;
        }

        Size size = new Size( ( scrollViewer != null ) ? scrollViewer.ViewportWidth : container.ActualWidth,
                              ( scrollViewer != null ) ? scrollViewer.ViewportHeight : container.ActualHeight );

        Rect visibleRect = new Rect( pt, size );

        RectangleGeometry geo = new RectangleGeometry( visibleRect );

        lock( ScrollViewerHelper.Current )
        {
          m_sVisibleChildList.Clear();
          m_sGridControl = gridControl;

          VisualTreeHelper.HitTest( container,
                                    new HitTestFilterCallback( ScrollViewerHelper.MyFilterFunct ),
                                    new HitTestResultCallback( ScrollViewerHelper.UselessResultCallback ),
                                    new GeometryHitTestParameters( geo ) );

          m_sGridControl = null;

          FrameworkElement preservedChild = null;
          Nullable<Vector> preservedOffset = null;

          foreach( FrameworkElement child in m_sVisibleChildList )
          {
            Vector itemOffset = VisualTreeHelper.GetOffset( child );

            Rect itemRect = new Rect( itemOffset.X, itemOffset.Y, child.ActualWidth, child.ActualHeight );

            itemRect.Intersect( visibleRect );

            switch( gridControl.ItemsPrimaryAxis )
            {
              case PrimaryAxis.Vertical:
                if( DoubleUtil.AreClose( itemRect.Width, 0 ) == false )
                {
                  if( DoubleUtil.AreClose( child.ActualHeight, itemRect.Height ) == true )
                  {
                    if( ScrollViewerHelper.IsABetterFirstRow( preservedChild, preservedOffset, itemOffset ) == true )
                    {
                      preservedChild = child;
                      preservedOffset = itemOffset;
                    }
                  }
                }
                break;
              case PrimaryAxis.Horizontal:
                if( DoubleUtil.AreClose( itemRect.Height, 0 ) == false )
                {
                  if( DoubleUtil.AreClose( itemRect.Width, child.ActualWidth ) == true )
                  {
                    if( ScrollViewerHelper.IsABetterFirstRow( preservedChild, preservedOffset, itemOffset ) == true )
                    {
                      preservedChild = child;
                      preservedOffset = itemOffset;
                    }
                  }
                }
                break;
              case PrimaryAxis.Both:
                if( ( DoubleUtil.AreClose( itemRect.Width, child.ActualWidth ) == true ) && ( DoubleUtil.AreClose( itemRect.Height, child.ActualHeight ) == true ) )
                {
                  if( ScrollViewerHelper.IsABetterFirstRow( preservedChild, preservedOffset, itemOffset ) == true )
                  {
                    preservedChild = child;
                    preservedOffset = itemOffset;
                  }
                }
                break;
              case PrimaryAxis.None:
                if( ( DoubleUtil.AreClose( itemRect.Width, 0 ) == false ) && ( DoubleUtil.AreClose( itemRect.Width, 0 ) == false ) )
                {
                  if( itemRect.IsEmpty == false )
                  {
                    if( ScrollViewerHelper.IsABetterFirstRow( preservedChild, preservedOffset, itemOffset ) == true )
                    {
                      preservedChild = child;
                      preservedOffset = itemOffset;
                    }
                  }
                }
                break;
            }
          }

          retval = preservedChild;
        }//end lock
      }

      return retval;
    }

    //-----------------  INTERNAL METHODS -------------------------

    internal static Thumb FindParentThumb( object reference, ScrollViewer scrollViewer )
    {
      Thumb retval = null;
      DependencyObject obj = reference as DependencyObject;

      if( obj != null )
      {
        do
        {
          retval = obj as Thumb;

          if( retval == null )
          {
            obj = TreeHelper.GetParent( obj );

            if( ( obj == scrollViewer ) || ( obj == null ) )
              break;
          }
        } while( retval == null );
      }

      return retval;
    }

    internal static Orientation GetItemScrollingOrientation( 
      DataGridControl dataGridControl, 
      FrameworkElement container, 
      ScrollViewer scrollViewer )
    {
      int itemsCount = dataGridControl.Items.Count;

      //if the Panel passed supports the CustomItemContainerGenerator, then retrieve the itemcount from the customItemContainerGenerator
      if( ( container is DataGridVirtualizingPanel ) || ( container is DataGridItemsHost ) )
      {
        itemsCount = dataGridControl.CustomItemContainerGenerator.ItemCount;
      }

      if( scrollViewer.ExtentHeight == itemsCount )
        return Orientation.Vertical;

      return Orientation.Horizontal;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Performance", "CA1800:DoNotCastUnnecessarily" )]
    internal static Orientation GetItemScrollingOrientation( DataGridControl dataGridControl )
    {
      int itemsCount = dataGridControl.Items.Count;

      ScrollViewer scrollViewer = dataGridControl.ScrollViewer;
      FrameworkElement container = dataGridControl.ItemsHost;

      //if the Panel passed supports the CustomItemContainerGenerator, then retrieve the itemcount from the customItemContainerGenerator
      if( ( container is DataGridVirtualizingPanel ) || ( container is DataGridItemsHost ) )
      {
        itemsCount = dataGridControl.CustomItemContainerGenerator.ItemCount;
      }

      Orientation foundOrientation = Orientation.Horizontal;
      if( scrollViewer.ExtentHeight == itemsCount )
      {
        foundOrientation = Orientation.Vertical;
      }
      else if( container is StackPanel )
      {
        foundOrientation = ( ( StackPanel )container ).Orientation;
      }
      else if( container is WrapPanel )
      {
        foundOrientation = ( ( WrapPanel )container ).Orientation;
      }
      else if( container is IAnimatedScrollInfo )
      {
        foundOrientation = Orientation.Vertical;
      }
      else
      {
        foundOrientation = Orientation.Horizontal;
      }
      return foundOrientation;
    }

    internal static bool IsPixelScrolling( DataGridControl dataGridControl, FrameworkElement container, ScrollViewer scrollViewer )
    {
      int itemsCount = dataGridControl.Items.Count;

      //if the Panel passed supports the CustomItemContainerGenerator, then retrieve the itemcount from the customItemContainerGenerator
      if( ( container is DataGridVirtualizingPanel ) || ( container is DataGridItemsHost ) )
      {
        itemsCount = dataGridControl.CustomItemContainerGenerator.ItemCount;
      }

      //This means that the ScrollViewer performs Items Scrolling
      if( ( scrollViewer != null )
        && ( ( scrollViewer.ExtentHeight == itemsCount ) || ( scrollViewer.ExtentWidth == itemsCount ) )
        && ( container is IScrollInfo ) && ( scrollViewer.CanContentScroll == true ) )
      {
        return false;
      }

      return true;
    }

    internal static void ResetScrollPositions( ScrollViewer scrollViewer )
    {
      if( scrollViewer == null )
      {
        throw new ArgumentNullException( "scrollViewer" );
      }

      DataGridScrollViewer dataGridScrollViewer = scrollViewer as DataGridScrollViewer;
      if( dataGridScrollViewer != null )
      {
        foreach( SynchronizedScrollViewer ssv in dataGridScrollViewer.SynchronizedScrollViewers )
        {
          ssv.ScrollToTop();
          ssv.ScrollToLeftEnd();
        }
      }

      scrollViewer.ScrollToTop();
      scrollViewer.ScrollToLeftEnd();

    }

    //-----------------  PRIVATE METHODS -------------------------

    private static HitTestFilterBehavior MyFilterFunct( DependencyObject obj )
    {
      Panel panel = m_sGridControl.ItemsHost as Panel;

      //if the object passed is a direct child of the ItemsHostPanel; keep it
      if( ( panel != null ) 
        && ( panel.Children.Contains( ( UIElement )obj ) == true ) )
      {
        m_sVisibleChildList.Add( obj );

        return HitTestFilterBehavior.ContinueSkipChildren;
      }
      else //otherwise, discard it.
      {
        return HitTestFilterBehavior.ContinueSkipSelf;
      }
    }

    private static HitTestResultBehavior UselessResultCallback( HitTestResult htr )
    {
      return HitTestResultBehavior.Continue;
    }

    private static bool IsABetterFirstRow( DependencyObject referenceChild, Nullable<Vector> referenceOffset, Vector newOffset )
    {
      bool retval = false;

      if( referenceChild != null )
      {
        if( ( DoubleUtil.AreClose( referenceOffset.Value.Y, newOffset.Y ) == true ) && ( newOffset.X < referenceOffset.Value.X ) )
        {
          retval = true;
        }
        else if( referenceOffset.Value.Y > newOffset.Y )
        {
          retval = true;
        }
      }
      else
      {
        retval = true;
      }

      return retval;
    }

    private static bool IsABetterLastRow( DependencyObject referenceChild, Nullable<Vector> referenceOffset, Vector newOffset )
    {
      bool retval = false;

      if( referenceChild != null )
      {
        if( ( DoubleUtil.AreClose( referenceOffset.Value.Y, newOffset.Y ) == true ) && ( newOffset.X > referenceOffset.Value.X ) )
        {
          retval = true;
        }
        else if( referenceOffset.Value.Y < newOffset.Y )
        {
          retval = true;
        }
      }
      else
      {
        retval = true;
      }

      return retval;
    }

    //-----------------  FIELDS -------------------------

    private static ScrollViewerHelper m_singleInstance; // = null;
    private static List<DependencyObject> m_sVisibleChildList = new List<DependencyObject>();
    private static DataGridControl m_sGridControl; // = null;
  }
}
