/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;
using Xceed.Wpf.DataGrid.Views;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Input;

namespace Xceed.Wpf.DataGrid
{
  public class RowSelectorPane : Panel
  {
    private const double SafeOffScreenCoordinate = -1000000.0d;

    private static readonly Rect RecycleReadyRect = new Rect( RowSelectorPane.SafeOffScreenCoordinate, RowSelectorPane.SafeOffScreenCoordinate, 0, 0 );
    private static readonly Point OriginPoint = new Point( 0, 0 );

    static RowSelectorPane()
    {
      ClipToBoundsProperty.OverrideMetadata( typeof( RowSelectorPane ), new FrameworkPropertyMetadata( true ) );

      FocusableProperty.OverrideMetadata( typeof( RowSelectorPane ), new FrameworkPropertyMetadata( false ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( RowSelectorPane ), new FrameworkPropertyMetadata( new PropertyChangedCallback( RowSelectorPane.OnParentGridControlChanged ) ) );
    }

    //------------------------------
    // PROPERTIES
    //------------------------------

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
        DependencyProperty.Register( "Orientation", typeof( Orientation ), typeof( RowSelectorPane ), new UIPropertyMetadata( Orientation.Vertical, new PropertyChangedCallback( OnOrientationChanged ) ) );


    private static void OnOrientationChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {

    }

    #endregion Orientation Property

    internal void SetRowSelectorPosition( DependencyObject container, Rect containerRect, FrameworkElement referenceElement )
    {
      Debug.Assert( ( Visibility )this.GetValue( RowSelectorPane.VisibilityProperty ) == Visibility.Visible, "We should not be creating RowSelectors if we are not Visible." );

      RowSelector rowSelector = null;

      //Check if a RowSelector is already assigned to the container passed.
      if( m_visibleSelectors.TryGetValue( container, out rowSelector ) == false )
      {
        //If no RowSelector exists for this container, try to recycle one.
        if( m_recycleQueue.Count > 0 )
        {
          //get next recyclable RowSelector.
          rowSelector = m_recycleQueue.Dequeue();
        }
        else
        {
          //Recycling pool empty, create a new RowSelector and place it in the visual tree right away.
          rowSelector = new RowSelector();
          this.InternalChildren.Add( rowSelector );
        }

        //Since there was no RowSelector for the container, map the container to this RowSelector
        rowSelector.DataContext = container;
        DataGridControl.SetContainer( rowSelector, container );
        rowSelector.ReferenceElement = referenceElement;
        m_visibleSelectors.Add( container, rowSelector );
      }

      //At this stage, it is not normal that no RowSelector instance is available.
      Debug.Assert( rowSelector != null );

      if( rowSelector == null )
        throw new InvalidOperationException( "An attempt was made to set the position of a row selector that does not exist." );

      rowSelector.ContainerRect = containerRect;

      this.InvalidateMeasure();
    }

    internal void FreeRowSelector( DependencyObject container )
    {
      RowSelector rowSelector = null;

      //Check if a RowSelector is already assigned to the container passed.
      if( m_visibleSelectors.TryGetValue( container, out rowSelector ) == true )
      {
        //Remove from Visible list, add to pending cleanup list
        m_visibleSelectors.Remove( container );
        m_pendingCleanup.Add( rowSelector );

        //clear data context (container) from the RowSelector.
        rowSelector.DataContext = null;
        DataGridControl.ClearContainer( rowSelector );

        //Invalidate Measure since the list of VisibleSelector + PendingCleanup changed
        this.InvalidateMeasure();
      }
      //If there are no RowSelector for container, then do nothing.
    }

    protected override Size MeasureOverride( Size availableSize )
    {
      Size desiredSize = new Size( 0, 0 );

      foreach( RowSelector rowSelector in m_visibleSelectors.Values )
      {
        Size rowSelectorAvaillableSize = this.PrepareAvailableSizeForRowSelector( rowSelector, availableSize );

        rowSelector.Measure( rowSelectorAvaillableSize );
      }

      return desiredSize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      foreach( RowSelector toClean in m_pendingCleanup )
      {
        toClean.Arrange( RecycleReadyRect );
        m_recycleQueue.Enqueue( toClean );
      }

      m_pendingCleanup.Clear();

      foreach( RowSelector visibleRowSelector in m_visibleSelectors.Values )
      {
        Rect rowSelectorArrangeRect = this.PrepareArrangeRectForRowSelector( finalSize, visibleRowSelector );

        visibleRowSelector.Clip = this.PrepareClipForRowSelector( visibleRowSelector, rowSelectorArrangeRect, visibleRowSelector.ReferenceElement );

        visibleRowSelector.Arrange( rowSelectorArrangeRect );
      }

      //Case 117296: update the mouse status to make sure no container has invalid mouse over status. Only do this when the mouse is over the panel, to 
      //prevent unescessary update when scrolling with thumb
      if( this.IsMouseOver == true )
      {
        Mouse.Synchronize();
      }

      return finalSize;
    }

    internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( RowSelectorPane ) );
    }

    private Rect PrepareArrangeRectForRowSelector( Size finalSize, RowSelector visibleRowSelector )
    {
      Rect returnRect;

      FrameworkElement container = visibleRowSelector.DataContext as FrameworkElement;
      Debug.Assert( container != null );

      GeneralTransform coordinatesTransform = container.TransformToVisual( this );
      Point origin = coordinatesTransform.Transform( OriginPoint );

      if( this.Orientation == Orientation.Vertical )
      {
        returnRect = new Rect( 0, origin.Y, finalSize.Width, container.ActualHeight );
      }
      else
      {
        returnRect = new Rect( origin.X, 0, container.ActualWidth, finalSize.Height );
      }

      return returnRect;
    }

    private Size PrepareAvailableSizeForRowSelector( RowSelector rowSelector, Size availableSize )
    {
      Rect containerRect = rowSelector.ContainerRect;
      Size returnSize;

      if( this.Orientation == Orientation.Vertical )
      {
        returnSize = new Size( availableSize.Width, containerRect.Height );
      }
      else
      {
        returnSize = new Size( containerRect.Width, availableSize.Height );
      }

      return returnSize;
    }

    private Geometry PrepareClipForRowSelector( RowSelector rowSelector, Rect arrangeRect, FrameworkElement referenceElement )
    {
      if( referenceElement == null )
        return null;

      GeneralTransform referenceElementToRowSelectorPaneTransform = referenceElement.TransformToVisual( this );

      Rect referenceElementRegion = referenceElementToRowSelectorPaneTransform.TransformBounds( new Rect( 0, 0, referenceElement.ActualWidth, referenceElement.ActualHeight ) );

      RectangleGeometry clipRegion = null;

      if( this.Orientation == Orientation.Vertical )
      {
        UIElement container = rowSelector.DataContext as UIElement;

        if( ( container != null ) && ( container.Clip != null ) )
        {
          Rect containerClipBounds = container.Clip.Bounds;

          // In this case, we will use the container's clip properties (Top and Bottom).
          clipRegion = new RectangleGeometry( new Rect( 0d, containerClipBounds.Y, arrangeRect.Width, containerClipBounds.Height ) );
        }
        else if( ( arrangeRect.Top < referenceElementRegion.Top ) || ( arrangeRect.Bottom > referenceElementRegion.Bottom ) )
        {
          double x = 0d;
          double y = Math.Max( referenceElementRegion.Top - arrangeRect.Top, 0 );

          double width = arrangeRect.Width;
          double height = Math.Max( 0, arrangeRect.Height - y - Math.Max( 0, arrangeRect.Bottom - referenceElementRegion.Bottom ) );


          clipRegion = new RectangleGeometry( new Rect( x, y, width, height ) );
        }
      }
      else
      {
        UIElement container = rowSelector.DataContext as UIElement;

        if( ( container != null ) && ( container.Clip != null ) )
        {
          Rect containerClipBounds = container.Clip.Bounds;

          // In this case, we will use the container's clip properties (Left and Right).
          clipRegion = new RectangleGeometry( new Rect( containerClipBounds.X, 0d, containerClipBounds.Width, arrangeRect.Height ) );
        }
        else if( ( arrangeRect.Left < referenceElementRegion.Left ) || ( arrangeRect.Right > referenceElementRegion.Right ) )
        {
          double x = Math.Max( referenceElementRegion.Left - arrangeRect.Left, 0 );
          double y = 0d;

          double width = arrangeRect.Width - x - Math.Max( 0, arrangeRect.Right - referenceElementRegion.Right );
          double height = arrangeRect.Height;

          clipRegion = new RectangleGeometry( new Rect( x, y, width, height ) );
        }
      }

      return clipRegion;
    }

    //------------------------------
    // DP CHANGED HANDLERS
    //------------------------------

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      RowSelectorPane rowSelectorPane = ( RowSelectorPane )sender;
      DataGridControl newGrid = e.NewValue as DataGridControl;
      DataGridControl oldGrid = e.OldValue as DataGridControl;

      if( oldGrid != null )
      {
        // The RowSelectorPane must clear its internal state when the ParentGridControl changes
        rowSelectorPane.m_pendingCleanup.Clear();
        rowSelectorPane.m_recycleQueue.Clear();
        rowSelectorPane.m_visibleSelectors.Clear();
        rowSelectorPane.InternalChildren.Clear();
      }

      if( newGrid != null )
      {
        rowSelectorPane.PrepareDefaultStyleKey( newGrid.GetView() );
      }

      rowSelectorPane.InvalidateMeasure();
    }

    #region Obsolete Members

    [Obsolete( "The ItemsPlacementReference property is obsolete and should no longer be used.", true )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public FrameworkElement ItemsPlacementReference
    {
      get
      {
        return null;
      }
      set
      {
      }
    }

    [Obsolete( "The ItemsPlacementReferenceProperty dependency property is obsolete and should no longer be used.", true )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty ItemsPlacementReferenceProperty;


    [Obsolete( "The RowSelectorStyle attached property is obsolete and has been replaced by the RowSelector.RowSelectorStyle attached property.", true )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static Style GetRowSelectorStyle( DependencyObject obj )
    {
      return null;
    }

    [Obsolete( "The RowSelectorStyle attached property is obsolete and has been replaced by the RowSelector.RowSelectorStyle attached property.", true )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static void SetRowSelectorStyle( DependencyObject obj, Style value )
    {
    }

    [Obsolete( "The RowSelectorStyleProperty dependency property is obsolete and has been replaced by the RowSelector.RowSelectorStyleProperty dependency property.", true )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty RowSelectorStyleProperty;

    [Obsolete( "The ScrollViewer property is obsolete and should no longer be used.", true )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public ScrollViewer ScrollViewer
    {
      get
      {
        return null;
      }
      set
      {
      }
    }

    [Obsolete( "The ScrollViewerProperty dependency property is obsolete and should no longer be used.", true )]
    [EditorBrowsable( EditorBrowsableState.Never )]
    public static readonly DependencyProperty ScrollViewerProperty;

    #endregion Obsolete Members


    private Queue<RowSelector> m_recycleQueue = new Queue<RowSelector>();
    private List<RowSelector> m_pendingCleanup = new List<RowSelector>();
    Dictionary<DependencyObject, RowSelector> m_visibleSelectors = new Dictionary<DependencyObject, RowSelector>();

  }
}
