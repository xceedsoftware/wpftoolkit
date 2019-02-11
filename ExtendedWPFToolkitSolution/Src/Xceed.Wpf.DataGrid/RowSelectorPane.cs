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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Xceed.Wpf.DataGrid
{
  public class RowSelectorPane : Panel
  {
    static RowSelectorPane()
    {
      ClipToBoundsProperty.OverrideMetadata( typeof( RowSelectorPane ), new FrameworkPropertyMetadata( true ) );
      FocusableProperty.OverrideMetadata( typeof( RowSelectorPane ), new FrameworkPropertyMetadata( false ) );

      DataGridControl.ParentDataGridControlPropertyKey.OverrideMetadata( typeof( RowSelectorPane ), new FrameworkPropertyMetadata( new PropertyChangedCallback( RowSelectorPane.OnParentGridControlChanged ) ) );
    }

    #region Orientation Dependency Property

    // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
      "Orientation",
      typeof( Orientation ),
      typeof( RowSelectorPane ),
      new UIPropertyMetadata( Orientation.Vertical ) );

    public Orientation Orientation
    {
      get
      {
        return ( Orientation )this.GetValue( RowSelectorPane.OrientationProperty );
      }
      set
      {
        this.SetValue( RowSelectorPane.OrientationProperty, value );
      }
    }

    #endregion

    protected override Size MeasureOverride( Size availableSize )
    {
      foreach( UIElement child in this.InternalChildren )
      {
        var decorator = child as RowSelectorDecorator;
        if( decorator != null )
        {
          if( this.IsRowSelectorVisible( decorator ) )
          {
            decorator.Visibility = Visibility.Visible;
            decorator.Measure( this.GetAvailableSize( decorator, availableSize ) );
          }
          else
          {
            decorator.Visibility = Visibility.Collapsed;

            if( !decorator.IsMeasureValid )
            {
              decorator.Measure( RowSelectorPane.EmptySize );
            }
          }
        }
        else
        {
          child.Measure( RowSelectorPane.EmptySize );
        }
      }

      return RowSelectorPane.EmptySize;
    }

    protected override Size ArrangeOverride( Size finalSize )
    {
      foreach( UIElement child in this.InternalChildren )
      {
        var decorator = child as RowSelectorDecorator;
        if( decorator != null )
        {
          if( this.IsRowSelectorVisible( decorator ) )
          {
            var arrangeRect = this.GetArrangeRect( decorator, finalSize );
            var rowSelector = decorator.RowSelector;
            var referenceElement = ( rowSelector != null ) ? rowSelector.ReferenceElement : null;

            decorator.Clip = this.GetClipRegion( decorator, arrangeRect, referenceElement );
            decorator.Arrange( arrangeRect );
          }
          else if( !decorator.IsArrangeValid )
          {
            decorator.Arrange( RowSelectorPane.OutOfViewRect );
          }
        }
        else
        {
          child.Arrange( RowSelectorPane.OutOfViewRect );
        }
      }

      // The call to Mouse.Synchronize must not start dragging rows.
      // Update the mouse status to make sure no container has invalid mouse over status.
      // Only do this when the mouse is over the panel, to prevent unescessary update when scrolling with thumb.
      if( this.IsMouseOver )
      {
        var dataGridContext = DataGridControl.GetDataGridContext( this );
        var dataGridControl = ( dataGridContext != null ) ? dataGridContext.DataGridControl : null;

        if( dataGridControl != null )
        {
          using( dataGridControl.InhibitDrag() )
          {
            Mouse.Synchronize();
          }
        }
      }

      return finalSize;
    }

    internal virtual void PrepareDefaultStyleKey( Xceed.Wpf.DataGrid.Views.ViewBase view )
    {
      this.DefaultStyleKey = view.GetDefaultStyleKey( typeof( RowSelectorPane ) );
    }

    internal void SetRowSelectorPosition( DependencyObject container, Rect containerRect, FrameworkElement referenceElement )
    {
      Debug.Assert( ( Visibility )this.GetValue( RowSelectorPane.VisibilityProperty ) == Visibility.Visible, "We should not be creating RowSelectors if we are not Visible." );

      // Try to re-use the RowSelector already assigned to the container.
      if( this.TryReUseVisibleRowSelector( container, containerRect ) )
        return;

      var decorator = this.RecycleOrCreateRowSelector( container );
      Debug.Assert( decorator != null, "An attempt was made to set the position of a row selector that does not exist." );

      RowSelectorPane.PrepareRowSelector( decorator, container, containerRect, referenceElement );

      m_visibleSelectors.Add( container, decorator );

      this.InvalidateMeasure();
    }

    internal void FreeRowSelector( DependencyObject container )
    {
      RowSelectorDecorator decorator;
      if( !m_visibleSelectors.TryGetValue( container, out decorator ) )
        return;

      m_visibleSelectors.Remove( container );
      m_pendingCleanup.Add( decorator );

      this.ScheduleUnusedRowSelectorsCleanUp();
      this.InvalidateMeasure();
    }

    private Rect GetArrangeRect( RowSelectorDecorator decorator, Size finalSize )
    {
      var rowSelector = decorator.RowSelector;
      if( rowSelector == null )
        return RowSelectorPane.EmptyRect;

      var container = rowSelector.DataContext as FrameworkElement;
      Debug.Assert( container != null );

      var coordinatesTransform = container.TransformToVisual( this );
      var origin = coordinatesTransform.Transform( RowSelectorPane.OriginPoint );

      if( this.Orientation == Orientation.Vertical )
        return new Rect( 0, origin.Y, finalSize.Width, container.ActualHeight );

      return new Rect( origin.X, 0, container.ActualWidth, finalSize.Height );
    }

    private Size GetAvailableSize( RowSelectorDecorator decorator, Size availableSize )
    {
      var rowSelector = decorator.RowSelector;
      if( rowSelector == null )
        return RowSelectorPane.EmptySize;

      var containerRect = rowSelector.ContainerRect;

      if( this.Orientation == Orientation.Vertical )
        return new Size( availableSize.Width, containerRect.Height );

      return new Size( containerRect.Width, availableSize.Height );
    }

    private Geometry GetClipRegion( RowSelectorDecorator decorator, Rect arrangeRect, FrameworkElement referenceElement )
    {
      if( referenceElement == null )
        return null;

      var rowSelector = decorator.RowSelector;
      if( rowSelector == null )
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

    private bool IsRowSelectorVisible( RowSelectorDecorator decorator )
    {
      Debug.Assert( decorator != null );
      if( decorator.RowSelector == null )
        return false;

      var container = DataGridControl.GetContainer( decorator );
      if( container == null )
        return false;

      return m_visibleSelectors.ContainsKey( container );
    }

    private bool TryReUseVisibleRowSelector( DependencyObject container, Rect containerRect )
    {
      RowSelectorDecorator decorator;
      if( !m_visibleSelectors.TryGetValue( container, out decorator ) )
        return false;

      var rowSelector = decorator.RowSelector;
      if( rowSelector == null )
      {
        this.FreeRowSelector( container );
        return false;
      }

      // Check if the cached position and size of the target container is up to date.
      if( !Rect.Equals( rowSelector.ContainerRect, containerRect ) )
      {
        rowSelector.ContainerRect = containerRect;

        this.InvalidateMeasure();
      }

      return true;
    }

    private RowSelectorDecorator RecycleOrCreateRowSelector( DependencyObject container )
    {
      RowSelectorDecorator decorator;

      // Try to re-use a RowSelector that is schedule to be clean up.
      if( m_pendingCleanup.Count > 0 )
      {
        int index = m_pendingCleanup.Count - 1;

        for( int i = index; i >= 0; i-- )
        {
          decorator = m_pendingCleanup[ i ];
          if( DataGridControl.GetContainer( decorator ) == container )
          {
            index = i;
            break;
          }
        }

        decorator = m_pendingCleanup[ index ];
        m_pendingCleanup.RemoveAt( index );
      }
      // Try to recycle a RowSelector from the recycling pool.
      else if( m_recyclingQueue.Count > 0 )
      {
        decorator = m_recyclingQueue.Dequeue();
      }
      else
      {
        decorator = new RowSelectorDecorator();

        this.InternalChildren.Add( decorator );
      }

      return decorator;
    }

    private void ScheduleUnusedRowSelectorsCleanUp()
    {
      if( m_pendingCleanupOperation != null )
        return;

      m_pendingCleanupOperation = this.Dispatcher.BeginInvoke( DispatcherPriority.ApplicationIdle, new Action( this.CleanUpUnusedRowSelectors ) );
    }

    private void CleanUpUnusedRowSelectors()
    {
      m_pendingCleanupOperation = null;

      for( int i = m_pendingCleanup.Count - 1; i >= 0; i-- )
      {
        var decorator = m_pendingCleanup[ i ];

        RowSelectorPane.ClearRowSelector( decorator );

        if( decorator.RowSelector != null )
        {
          m_recyclingQueue.Enqueue( decorator );
        }
        else
        {
          // The RowSelector has become useless, so we destroy it.
          this.InternalChildren.Remove( decorator );
        }
      }

      m_pendingCleanup.Clear();
    }

    private static void PrepareRowSelector( RowSelectorDecorator decorator, DependencyObject container, Rect containerRect, FrameworkElement referenceElement )
    {
      Debug.Assert( decorator != null );

      DataGridControl.SetContainer( decorator, container );

      var rowSelector = decorator.RowSelector;
      if( rowSelector != null )
      {
        rowSelector.ReferenceElement = referenceElement;
        rowSelector.ContainerRect = containerRect;
      }
    }

    private static void ClearRowSelector( RowSelectorDecorator decorator )
    {
      Debug.Assert( decorator != null );

      DataGridControl.ClearContainer( decorator );

      var rowSelector = decorator.RowSelector;
      if( rowSelector != null )
      {
        rowSelector.ReferenceElement = null;
        rowSelector.ContainerRect = RowSelectorPane.EmptyRect;
      }
    }

    private static void OnParentGridControlChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      RowSelectorPane rowSelectorPane = ( RowSelectorPane )sender;
      DataGridControl newGrid = e.NewValue as DataGridControl;
      DataGridControl oldGrid = e.OldValue as DataGridControl;

      if( oldGrid != null )
      {
        // The RowSelectorPane must clear its internal state when the ParentGridControl changes
        rowSelectorPane.m_pendingCleanup.Clear();
        rowSelectorPane.m_recyclingQueue.Clear();
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

    #region CONSTANTS

    private static readonly Point OriginPoint = new Point( 0d, 0d );
    private static readonly Size EmptySize = new Size( 0d, 0d );
    private static readonly Rect EmptyRect = new Rect( RowSelectorPane.OriginPoint, RowSelectorPane.EmptySize );
    private static readonly Rect OutOfViewRect = new Rect( new Point( -999999d, -999999d ), RowSelectorPane.EmptySize );

    #endregion

    #region PRIVATE FIELDS

    private readonly Queue<RowSelectorDecorator> m_recyclingQueue = new Queue<RowSelectorDecorator>();
    private readonly List<RowSelectorDecorator> m_pendingCleanup = new List<RowSelectorDecorator>();
    private readonly Dictionary<DependencyObject, RowSelectorDecorator> m_visibleSelectors = new Dictionary<DependencyObject, RowSelectorDecorator>();
    private DispatcherOperation m_pendingCleanupOperation; //null

    #endregion

    #region RowSelectorDecorator Private Class

    private sealed class RowSelectorDecorator : Decorator
    {
      internal RowSelectorDecorator()
      {
        this.Focusable = false;
        this.OverridesDefaultStyle = true;

        this.Child = new RowSelector();
      }

      internal RowSelector RowSelector
      {
        get
        {
          return this.Child as RowSelector;
        }
      }
    }

    #endregion
  }
}
