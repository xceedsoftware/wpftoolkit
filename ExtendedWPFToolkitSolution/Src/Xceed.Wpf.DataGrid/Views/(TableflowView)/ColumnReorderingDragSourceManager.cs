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
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Xceed.Utils.Wpf.DragDrop;

namespace Xceed.Wpf.DataGrid.Views
{
  internal sealed class ColumnReorderingDragSourceManager : DragSourceManager
  {
    #region Private Fields

    private static readonly Duration s_columnAnimationDuration = new Duration( TimeSpan.FromMilliseconds( 500d ) );
    private static readonly Duration s_draggedElementFadeInDuration = new Duration( TimeSpan.FromMilliseconds( 250d ) );
    private static readonly Duration s_returnToOriginalPositionDuration = new Duration( TimeSpan.FromMilliseconds( 250d ) );

    private static readonly Point s_origin = new Point();

    #endregion

    internal ColumnReorderingDragSourceManager( UIElement draggedElement, AdornerLayer adornerLayer, UIElement container, int level )
      : base( draggedElement, adornerLayer, container )
    {
      m_level = level;
      m_dataGridContext = DataGridControl.GetDataGridContext( draggedElement );

      Debug.Assert( m_dataGridContext != null );

      m_splitterTranslation = TableflowView.GetFixedColumnSplitterTranslation( m_dataGridContext );
      m_columnVirtualizationManager = m_dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManagerBase;
      m_reorderCancelled = false;
    }

    #region IsAnimatedColumnReorderingEnabled Internal Property

    internal bool IsAnimatedColumnReorderingEnabled
    {
      get
      {
        return m_isAnimatedColumnReorderingEnabled;
      }
      private set
      {
        if( value == m_isAnimatedColumnReorderingEnabled )
          return;

        m_isAnimatedColumnReorderingEnabled = value;

        this.OnPropertyChanged( "IsAnimatedColumnReorderingEnabled" );
      }
    }

    private bool m_isAnimatedColumnReorderingEnabled;

    #endregion

    #region AnimatedColumnReorderingTranslation Attached Property

    // This translation will be used by the AnimatedColumnReorderingManager to apply a TranslateTransform
    // to Columns that require it in order to give a preview of the reordering in an animated or live way
    internal static readonly DependencyProperty AnimatedColumnReorderingTranslationProperty = DependencyProperty.RegisterAttached(
      "AnimatedColumnReorderingTranslation",
      typeof( TransformGroup ),
      typeof( ColumnReorderingDragSourceManager ),
      new FrameworkPropertyMetadata( null ) );

    internal static TransformGroup GetAnimatedColumnReorderingTranslation( DependencyObject obj )
    {
      return ( TransformGroup )obj.GetValue( ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty );
    }

    private static void SetAnimatedColumnReorderingTranslation( DependencyObject obj, TransformGroup value )
    {
      obj.SetValue( ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty, value );
    }

    private static void ClearAnimatedColumnReorderingTranslation( DependencyObject obj )
    {
      var group = ColumnReorderingDragSourceManager.GetAnimatedColumnReorderingTranslation( obj );
      if( group != null )
      {
        ColumnReorderingDragSourceManager.ClearTranslateTransformAnimation( ColumnReorderingDragSourceManager.GetColumnPositionTransform( group ) );
        ColumnReorderingDragSourceManager.ClearScaleTransformAnimation( ColumnReorderingDragSourceManager.GetColumnSizeTransform( group ) );
      }

      obj.ClearValue( ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty );
    }

    private static TransformGroup CreateColumnTransformGroup()
    {
      var group = new TransformGroup();
      group.Children.Add( new TranslateTransform() );
      group.Children.Add( new ScaleTransform() );

      return group;
    }

    private static TransformGroup GetColumnTransformGroup( ColumnBase column )
    {
      if( column == null )
        return null;

      var group = ColumnReorderingDragSourceManager.GetAnimatedColumnReorderingTranslation( column );
      if( group == null )
      {
        group = ColumnReorderingDragSourceManager.CreateColumnTransformGroup();
        ColumnReorderingDragSourceManager.SetAnimatedColumnReorderingTranslation( column, group );
      }

      Debug.Assert( group != null );

      return group;
    }

    private static TranslateTransform GetColumnPositionTransform( TransformGroup group )
    {
      if( ( group == null ) || ( group.Children.Count <= 0 ) )
        return null;

      return group.Children[ 0 ] as TranslateTransform;
    }

    private static ScaleTransform GetColumnSizeTransform( TransformGroup group )
    {
      if( ( group == null ) || ( group.Children.Count <= 1 ) )
        return null;

      return group.Children[ 1 ] as ScaleTransform;
    }

    #endregion

    protected override void OnDragStart( Func<IInputElement, Point> getPosition )
    {
      if( m_isDragStarted )
        throw new InvalidOperationException();

      m_isDragStarted = true;

      this.StopAnimationsAndRollback();
      this.UpdateIsAnimatedColumnReorderingEnabled();

      base.OnDragStart( getPosition );

      var draggedElement = this.DraggedElement;

      //When the grid is hosted in a popup window, it is not possible to know the position of the popup in certain scenarios (e.g. fullscreen, popup openning upward).
      //Thus we must use a regular system adorner instead of the ghost window, so that the dragged adorner will appear correctly under the mouse pointer.
      if( this.IsPopup )
      {
        this.SetPopupDragAdorner( draggedElement as ColumnManagerCell );
      }

      if( !this.IsAnimatedColumnReorderingEnabled )
      {
        this.AutoScrollInterval = TimeSpan.FromMilliseconds( 50d );
        return;
      }

      this.AutoScrollInterval = TimeSpan.Zero;

      // Get initial mouse positions
      m_lastDraggedElementOffset = draggedElement.TranslatePoint( getPosition.Invoke( draggedElement ), this.Container ).X;

      var draggedCell = draggedElement as ColumnManagerCell;
      if( draggedCell != null )
      {
        var draggedColumn = draggedCell.ParentColumn;
        Debug.Assert( draggedColumn != null );

        this.TakeColumnsLayoutSnapshot();

        // Affect the column's IsBeingDraggedAnimated to ensure every prepared container calls AddDraggedColumnGhost of this manager.
        draggedColumn.SetColumnReorderingDragSourceManager( this );
        draggedColumn.SetIsBeingDraggedAnimated( true );
      }

      TableflowView.SetAreColumnsBeingReordered( m_dataGridContext, true );
      TableflowView.SetColumnReorderingDragSourceManager( m_dataGridContext, this );

      this.HideDraggedElements();
    }

    protected override void OnDragEnd( Func<IInputElement, Point> getPosition, bool drop )
    {
      if( !m_isDragStarted )
        throw new InvalidOperationException();

      m_isDragStarted = false;

      base.OnDragEnd( getPosition, drop );

      //If in a popup, we are using a system adorner, and we need to hide it when releasing the mouse button.
      if( this.IsPopup )
      {
        m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 0d;
        m_popupDraggedElementAdorner.SetOffset( s_origin );
      }

      m_columnsLayout.Clear();

      if( !this.IsAnimatedColumnReorderingEnabled )
        return;

      this.MoveGhostToTargetAndDetach();
      this.ClearColumnAnimations();
      this.ClearSplitterAnimation();
    }

    protected override void OnDragMove( Func<IInputElement, Point> getPosition )
    {
      if( this.IsAnimatedColumnReorderingEnabled )
      {
        this.UpdateMouseMoveDirection( getPosition );
      }

      base.OnDragMove( getPosition );
    }

    protected override void OnDragOver( IDropTarget target, Func<IInputElement, Point> getPosition )
    {
      base.OnDragOver( target, getPosition );

      if( !this.IsAnimatedColumnReorderingEnabled )
        return;

      // We are reverting every animation before detaching from the manager
      // so do not update the ghost position
      if( m_ghostToTargetAndDetachAnimationClock != null )
        return;

      var draggedCell = this.DraggedElement as ColumnManagerCell;
      if( draggedCell == null )
        return;

      var dropTarget = this.CurrentDropTarget as ColumnManagerCell;
      if( ( dropTarget == null ) || ( dropTarget == draggedCell ) )
        return;

      var draggedOverDataGridContext = DataGridControl.GetDataGridContext( dropTarget );
      if( ( draggedOverDataGridContext == null ) || ( m_dataGridContext != draggedOverDataGridContext ) || !this.CanReorder( draggedCell, dropTarget, this.CurrentDropTargetToContainerPosition ) )
        return;

      this.UpdateColumnsLayout();
      this.ApplyColumnsLayoutAnimations();
      this.ApplySplitterAnimation();
    }

    protected override DropTargetInfo GetDropTarget( Func<IInputElement, Point> getPosition )
    {
      if( !this.IsAnimatedColumnReorderingEnabled )
        return base.GetDropTarget( getPosition );

      var cell = this.DraggedElement as ColumnManagerCell;
      if( cell == null )
        return base.GetDropTarget( getPosition );

      var parentRow = cell.ParentRow as ColumnManagerRow;
      if( parentRow == null )
        return base.GetDropTarget( getPosition );

      foreach( var dropTargetInfo in DragDropHelper.GetDropTargetAtPoint( this.DraggedElement, this.Container, getPosition ) )
      {
        var target = dropTargetInfo.Target;
        if( !( target is ColumnManagerCell ) && !( target is ColumnManagerRow ) )
          return dropTargetInfo;
      }

      var dragContainer = this.Container;
      var dragContainerRect = new Rect( new Point(), dragContainer.RenderSize );

      var draggedElementToMouse = getPosition.Invoke( cell );
      var draggedElementTopLeftToMouse = new Point( draggedElementToMouse.X - this.InitialMousePositionToDraggedElement.Value.X, draggedElementToMouse.Y - this.InitialMousePositionToDraggedElement.Value.Y );
      var draggedElementBottomRightToMouse = new Point( draggedElementTopLeftToMouse.X + cell.ActualWidth, draggedElementTopLeftToMouse.Y + cell.ActualHeight );

      var draggedElementRectInDragContainer = new Rect( cell.TranslatePoint( draggedElementTopLeftToMouse, dragContainer ),
                                                        cell.TranslatePoint( draggedElementBottomRightToMouse, dragContainer ) );
      draggedElementRectInDragContainer.Intersect( dragContainerRect );

      // We are not interested by the y-axis.  We set the y-axis value to 0 so the rectangle use to look for overlap on the x-axis
      // will work no matter where is located the dragged element on the y-axis.
      var cellRectInDragContainer = new Rect( cell.TranslatePoint( new Point( draggedElementTopLeftToMouse.X, 0d ), dragContainer ),
                                              cell.TranslatePoint( new Point( draggedElementBottomRightToMouse.X, cell.ActualHeight ), dragContainer ) );
      var rowRectInDragContainer = parentRow.TransformToVisual( dragContainer ).TransformBounds( VisualTreeHelper.GetDescendantBounds( parentRow ) );

      var dropTargetRectInDragContainer = rowRectInDragContainer;
      dropTargetRectInDragContainer.Intersect( cellRectInDragContainer );

      var result = default( DropTargetInfo );

      // Some part of the dragged element overlaps the container.
      if( !draggedElementRectInDragContainer.IsEmpty && !dropTargetRectInDragContainer.IsEmpty )
      {
        var hitTestPosition = new Point();
        var dropPosition = new Point();

        // We are not interested by the y-axis, but we will set a value that will return a positive result for hit testing.
        hitTestPosition.Y = cellRectInDragContainer.Top + ( cellRectInDragContainer.Height / 2d );
        dropPosition.Y = hitTestPosition.Y;

        switch( m_horizontalMouseDragDirection )
        {
          // Use the dragged element left edge to minimize the size of the blank area during animations.
          case HorizontalMouseDragDirection.Left:
            {
              hitTestPosition.X = dropTargetRectInDragContainer.Left;
              dropPosition.X = cellRectInDragContainer.Left;
            }
            break;

          // Use the dragged element right edge to minimize the size of the blank area during animations.
          case HorizontalMouseDragDirection.Right:
            {
              hitTestPosition.X = dropTargetRectInDragContainer.Right;
              dropPosition.X = cellRectInDragContainer.Right;
            }
            break;

          default:
            {
              var dragContainerToMouse = getPosition.Invoke( dragContainer );

              if( dragContainerToMouse.X <= dropTargetRectInDragContainer.Left )
              {
                hitTestPosition.X = dropTargetRectInDragContainer.Left;
                dropPosition.X = cellRectInDragContainer.Left;
              }
              else if( dragContainerToMouse.X >= dropTargetRectInDragContainer.Right )
              {
                hitTestPosition.X = dropTargetRectInDragContainer.Right;
                dropPosition.X = cellRectInDragContainer.Right;
              }
              else
              {
                hitTestPosition.X = dragContainerToMouse.X;
                dropPosition.X = dragContainerToMouse.X;
              }
            }
            break;
        }

        // Ensure the DraggedElement is not visible to hit test.
        cell.IsHitTestVisible = false;

        try
        {
          result = this.GetDropTargetAtPoint( cell, new RelativePoint( dragContainer, dropPosition ) );
        }
        finally
        {
          cell.ClearValue( UIElement.IsHitTestVisibleProperty );
        }
      }

      if( !( result.Target is ColumnManagerRow ) && !( result.Target is ColumnManagerCell ) )
      {
        result = default( DropTargetInfo );
      }

      // Flag used to reduce the number of column animations, to speed up the scrolling on ColumnManagerCell dragging.
      if( ( result.Target == null ) && ( this.CurrentDropTarget == null ) )
      {
        m_noColumnsReorderingNeeded = true;
        if( this.IsPopup )
        {
          m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 0d;
        }
      }
      else
      {
        m_noColumnsReorderingNeeded = false;
      }

      return result;
    }

    protected override void UpdateGhost( Func<IInputElement, Point> getPosition )
    {
      //If in TableView and the grid is hosted in a Popup window.
      if( !this.IsAnimatedColumnReorderingEnabled && this.IsPopup )
      {
        this.ShowGhost = false;

        var draggedElementToMouse = getPosition.Invoke( this.DraggedElement );
        var initialMousePosition = this.InitialMousePositionToDraggedElement.GetValueOrDefault();

        // Correct it according to initial mouse position over the dragged element
        draggedElementToMouse.X -= initialMousePosition.X;
        draggedElementToMouse.Y -= initialMousePosition.Y;

        this.ApplyContainerClip( m_popupDraggedElementAdorner );
        m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 1d;
        m_popupDraggedElementAdorner.SetOffset( draggedElementToMouse );
      }
      //If no animation, or if the dragged object is beyond the edge of the grid, no need to do anything in this override, simply call base.
      //This will be sufficient to update the DraggedElementGhost positon. The result is a much faster scrolling of the grid via the CheckForAutoScroll() method.
      else if( this.IsAnimatedColumnReorderingEnabled && !m_noColumnsReorderingNeeded )
      {
        // We are reverting every animation before detaching from the manager
        // so do not update the ghost position
        if( m_ghostToTargetAndDetachAnimationClock != null )
          return;

        var dragOverRowOrCell = ( this.CurrentDropTarget is ColumnManagerRow ) || ( this.CurrentDropTarget is ColumnManagerCell );

        // We are dragging over an object that will handle the drop itself
        if( !dragOverRowOrCell && !this.IsPopup )
        {
          // Ensure to pause every other animations before
          this.PauseGhostToMousePositionAnimation();
          this.PauseDraggedElementFadeInAnimation();

          this.RollbackReordering();
          this.ShowGhost = true;

          this.MoveGhostToTargetColumn( getPosition.Invoke( this.DraggedElement ) );
        }
        //If dragging over an object that will handle the drop itself and the grid is hosted in a Popup window.
        else if( !dragOverRowOrCell && this.IsPopup )
        {
          // Ensure to pause every other animations before
          this.PauseGhostToMousePositionAnimation();
          this.PauseDraggedElementFadeInAnimation();

          this.RollbackReordering();
          this.ShowGhost = false;

          var draggedElementToMouse = getPosition.Invoke( this.DraggedElement );

          this.MoveGhostToTargetColumn( draggedElementToMouse );

          var initialMousePosition = this.InitialMousePositionToDraggedElement.GetValueOrDefault();

          // Correct it according to initial mouse position over the dragged element
          draggedElementToMouse.X -= initialMousePosition.X;
          draggedElementToMouse.Y -= initialMousePosition.Y;

          this.ApplyContainerClip( m_popupDraggedElementAdorner );
          m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 1d;
          m_popupDraggedElementAdorner.SetOffset( draggedElementToMouse );
        }
        //If animations are required.
        else
        {
          //If in a popup, hide the dragged element adorner.
          if( this.IsPopup )
          {
            m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 0d;
            m_popupDraggedElementAdorner.SetOffset( s_origin );
          }

          // Pause animations that are moving ghosts to target Column
          this.PauseMoveGhostToTargetColumnAnimation();
          this.PauseDraggedElementFadeInAnimation();

          this.ShowGhost = false;
          this.ShowDraggedColumnGhosts();
          this.HideDraggedElements();

          var draggedElementToMouse = getPosition.Invoke( this.DraggedElement );
          var initialMousePosition = this.InitialMousePositionToDraggedElement.GetValueOrDefault();

          // Correct it according to initial mouse position over the dragged element
          draggedElementToMouse.X -= initialMousePosition.X;
          draggedElementToMouse.Y -= initialMousePosition.Y;

          // Wait for the animation to complete before explicitly setting the X and Y offsets on the ghost adorners OR if reordering was canceled
          if( ( m_ghostToMousePositionAnimationClock == null ) && ( m_ghostToTargetColumnAnimationClock == null ) && !m_reorderCancelled )
          {
            // Update the position of ghosts for each rows
            foreach( var adorner in this.GetElementAdorners() )
            {
              adorner.ApplyAnimationClock( DraggedElementAdorner.OffsetProperty, null );
              adorner.SetOffset( draggedElementToMouse );
            }
          }
          else
          {
            this.MoveGhostToMousePosition( draggedElementToMouse );
          }
        }
      }

      base.UpdateGhost( getPosition );
    }

    protected override void OnDroppedOutside()
    {
      base.OnDroppedOutside();

      if( !this.IsAnimatedColumnReorderingEnabled )
        return;

      this.RollbackReordering();
    }

    internal void AddDraggedColumnGhost( UIElement element )
    {
      if( ( element == null ) || m_elementToDraggedElementAdorner.ContainsKey( element ) )
        return;

      // Get the Rect for the DataGridControl
      var dataGridControl = m_dataGridContext.DataGridControl;
      var dataGridControlRect = new Rect( 0d, 0d, dataGridControl.ActualWidth, dataGridControl.ActualHeight );
      var elementToDataGridControl = element.TranslatePoint( s_origin, dataGridControl );
      var elementRect = new Rect( elementToDataGridControl, element.RenderSize );

      // This is a special case with the current element that is always layouted, but can be out of view
      if( !elementRect.IntersectsWith( dataGridControlRect ) )
        return;

      var adorner = new AnimatedDraggedElementAdorner( element, this.AdornerLayer, true );

      this.ApplyContainerClip( adorner );
      this.AdornerLayer.Add( adorner );

      m_elementToDraggedElementAdorner.Add( element, adorner );
    }

    internal void RemoveDraggedColumnGhost( UIElement element )
    {
      if( m_elementToDraggedElementAdorner.Count == 0 )
        return;

      DraggedElementAdorner adorner;
      if( !m_elementToDraggedElementAdorner.TryGetValue( element, out adorner ) )
        return;

      Debug.Assert( adorner != null );
      this.AdornerLayer.Remove( adorner );
      m_elementToDraggedElementAdorner.Remove( element );
    }

    internal bool ContainsDraggedColumnGhost( UIElement element )
    {
      if( ( element == null ) || ( m_elementToDraggedElementAdorner.Count == 0 ) )
        return false;

      return m_elementToDraggedElementAdorner.ContainsKey( element );
    }

    internal bool CanReorder( ColumnManagerCell draggedCell, ColumnManagerCell dropTarget, RelativePoint? dropPoint )
    {
      if( ( draggedCell == null )
        || ( dropTarget == null )
        || ( draggedCell == dropTarget )
        || !dropPoint.HasValue
        || ( DataGridControl.GetDataGridContext( draggedCell ) != m_dataGridContext )
        || ( DataGridControl.GetDataGridContext( dropTarget ) != DataGridControl.GetDataGridContext( draggedCell ) ) )
        return false;

      var dropColumn = dropTarget.ParentColumn;
      if( dropColumn == null )
        return false;

      var sourceLayout = m_dataGridContext.ColumnManager;
      var dropColumnLocation = sourceLayout.GetColumnLocationFor( dropColumn );
      if( dropColumnLocation == null )
        return false;

      if( ColumnReorderingDragSourceManager.IsPointLocatedBeforeTarget( dropTarget, dropPoint ) )
      {
        if( !draggedCell.CanMoveBefore( dropColumn ) )
          return false;

        for( var location = dropColumnLocation.GetPreviousSibling(); location != null; location = location.GetPreviousSibling() )
        {
          if( location.Type != LocationType.Column )
            continue;

          var previousColumn = ( ( ColumnHierarchyManager.IColumnLocation )location ).Column;
          if( !draggedCell.CanMoveAfter( previousColumn ) )
            return false;

          break;
        }
      }
      else if( ColumnReorderingDragSourceManager.IsPointLocatedAfterTarget( dropTarget, dropPoint ) )
      {
        if( !draggedCell.CanMoveAfter( dropColumn ) )
          return false;

        for( var location = dropColumnLocation.GetNextSibling(); location != null; location = location.GetNextSibling() )
        {
          if( location.Type != LocationType.Column )
            continue;

          var nextColumn = ( ( ColumnHierarchyManager.IColumnLocation )location ).Column;
          if( !draggedCell.CanMoveBefore( nextColumn ) )
            return false;

          break;
        }
      }
      else
      {
        return false;
      }

      return true;
    }

    internal void CommitReordering()
    {
      var draggedCell = this.DraggedElement as Cell;
      if( draggedCell == null )
        return;

      var draggedColumn = draggedCell.ParentColumn;
      if( draggedColumn == null )
        return;

      var draggedColumnReorderLocation = m_columnsLayout[ draggedColumn ];
      if( draggedColumnReorderLocation == null )
        return;

      var sourceLayout = m_dataGridContext.ColumnManager;

      var draggedColumnSourceLocation = sourceLayout.GetColumnLocationFor( draggedColumn );
      Debug.Assert( draggedColumnSourceLocation != null );
      if( draggedColumnSourceLocation == null )
        return;

      var previousLocation = draggedColumnReorderLocation.GetPreviousSibling();
      if( previousLocation != null )
      {
        switch( previousLocation.Type )
        {
          case LocationType.Column:
            {
              var pivotColumn = ( ( ColumnHierarchyModel.IColumnLocation )previousLocation ).Column;
              var pivotLocation = sourceLayout.GetColumnLocationFor( pivotColumn );

              if( pivotLocation != null )
              {
                Debug.Assert( draggedColumnSourceLocation.CanMoveAfter( pivotLocation ) );
                draggedColumnSourceLocation.MoveAfter( pivotLocation );
                return;
              }
            }
            break;

          case LocationType.Splitter:
            {
              var levelMarkers = sourceLayout.GetLevelMarkersFor( draggedColumn.ContainingCollection );
              var pivotLocation = ( levelMarkers != null ) ? levelMarkers.Splitter : null;

              if( ( pivotLocation != null ) && draggedColumnSourceLocation.CanMoveAfter( pivotLocation ) )
              {
                draggedColumnSourceLocation.MoveAfter( pivotLocation );
                return;
              }
            }
            break;

          case LocationType.Start:
            {
              var levelMarkers = sourceLayout.GetLevelMarkersFor( draggedColumn.ContainingCollection );
              var pivotLocation = ( levelMarkers != null ) ? levelMarkers.Start : null;

              if( ( pivotLocation != null ) && draggedColumnSourceLocation.CanMoveAfter( pivotLocation ) )
              {
                draggedColumnSourceLocation.MoveAfter( pivotLocation );
                return;
              }
            }
            break;

          default:
            throw new NotImplementedException( "Unexpected target location." );
        }
      }

      var nextLocation = draggedColumnReorderLocation.GetNextSibling();
      if( nextLocation != null )
      {
        switch( nextLocation.Type )
        {
          case LocationType.Column:
            {
              var pivotColumn = ( ( ColumnHierarchyModel.IColumnLocation )nextLocation ).Column;
              var pivotLocation = sourceLayout.GetColumnLocationFor( pivotColumn );

              if( pivotLocation != null )
              {
                Debug.Assert( draggedColumnSourceLocation.CanMoveBefore( pivotLocation ) );
                draggedColumnSourceLocation.MoveBefore( pivotLocation );
                return;
              }
            }
            break;

          case LocationType.Splitter:
            {
              var levelMarkers = sourceLayout.GetLevelMarkersFor( draggedColumn.ContainingCollection );
              var pivotLocation = ( levelMarkers != null ) ? levelMarkers.Splitter : null;

              if( ( pivotLocation != null ) && draggedColumnSourceLocation.CanMoveBefore( pivotLocation ) )
              {
                draggedColumnSourceLocation.MoveBefore( pivotLocation );
                return;
              }
            }
            break;

          case LocationType.Orphan:
            {
              var levelMarkers = sourceLayout.GetLevelMarkersFor( draggedColumn.ContainingCollection );
              var pivotLocation = ( levelMarkers != null ) ? levelMarkers.Orphan : null;

              if( ( pivotLocation != null ) && draggedColumnSourceLocation.CanMoveBefore( pivotLocation ) )
              {
                draggedColumnSourceLocation.MoveBefore( pivotLocation );
                return;
              }
            }
            break;

          default:
            throw new NotImplementedException( "Unexpected target location." );
        }
      }

      var parentLocation = draggedColumnReorderLocation.GetParent();
      if( parentLocation != null )
      {
        switch( parentLocation.Type )
        {
          case LocationType.Column:
            {
              var pivotColumn = ( ( ColumnHierarchyModel.IColumnLocation )parentLocation ).Column;
              var pivotLocation = sourceLayout.GetColumnLocationFor( pivotColumn );

              if( pivotLocation != null )
              {
                Debug.Assert( draggedColumnSourceLocation.CanMoveUnder( pivotLocation ) );
                draggedColumnSourceLocation.MoveUnder( pivotLocation );
                return;
              }
            }
            break;
        }
      }

      this.ClearColumnAnimations();
      this.ClearSplitterAnimation();
    }

    internal IEnumerable<IDropTarget> GetDropTargetsAtPoint( RelativePoint point )
    {
      var container = this.Container;
      if( container == null )
        yield break;

      for( var target = container.InputHitTest( point.GetPoint( container ) ) as DependencyObject; target != null; target = Xceed.Utils.Wpf.TreeHelper.GetParent( target ) )
      {
        var dropTarget = target as IDropTarget;
        if( dropTarget == null )
          continue;

        yield return dropTarget;
      }
    }

    private static bool IsPointLocatedBeforeTarget( FrameworkElement target, RelativePoint? point )
    {
      if( ( target == null ) || !point.HasValue )
        return false;

      var targetPoint = point.Value.GetPoint( target );

      return ( targetPoint.X <= target.ActualWidth / 2d );
    }

    private static bool IsPointLocatedAfterTarget( FrameworkElement target, RelativePoint? point )
    {
      if( ( target == null ) || !point.HasValue )
        return false;

      var targetPoint = point.Value.GetPoint( target );

      return ( targetPoint.X > target.ActualWidth / 2d );
    }

    private DropTargetInfo GetDropTargetAtPoint( UIElement draggedElement, RelativePoint point )
    {
      foreach( var dropTarget in this.GetDropTargetsAtPoint( point ) )
      {
        if( dropTarget.CanDropElement( draggedElement, point ) )
          return new DropTargetInfo( dropTarget, point, true );
      }

      return default( DropTargetInfo );
    }

    private static void ClearTranslateTransformAnimation( TranslateTransform transform )
    {
      if( transform == null )
        return;

      // Force a local value before removing the potential animation clock of this transform to avoid leaving the current animated value after animation is removed
      transform.X = 0d;
      transform.ApplyAnimationClock( TranslateTransform.XProperty, null );
    }

    private static void ClearScaleTransformAnimation( ScaleTransform transform )
    {
      if( transform == null )
        return;

      // Force a local value before removing the potential animation clock of this transform to avoid leaving the current animated value after animation is removed
      transform.CenterX = 0d;
      transform.ScaleX = 1d;
      transform.ApplyAnimationClock( ScaleTransform.ScaleXProperty, null );
    }

    private void SetPopupDragAdorner( ColumnManagerCell columnManagerCell )
    {
      if( columnManagerCell == null )
        return;

      if( m_popupDraggedElementAdorner != null )
      {
        this.AdornerLayer.Remove( m_popupDraggedElementAdorner );
        m_popupDraggedElementAdorner = null;
      }

      var dataGridControl = m_dataGridContext.DataGridControl;
      var dataGridControlRect = new Rect( 0d, 0d, dataGridControl.ActualWidth, dataGridControl.ActualHeight );
      var elementToDataGridControl = columnManagerCell.TranslatePoint( s_origin, dataGridControl );
      var elementRect = new Rect( elementToDataGridControl, columnManagerCell.RenderSize );

      // This is a special case with the current Element that is always be layouted, but can be out of view
      if( !elementRect.IntersectsWith( dataGridControlRect ) )
        return;

      var adorner = new AnimatedDraggedElementAdorner( columnManagerCell, this.AdornerLayer, true );
      adorner.AdornedElementImage.Opacity = 0d;

      this.ApplyContainerClip( adorner );
      this.AdornerLayer.Add( adorner );

      m_popupDraggedElementAdorner = adorner;
    }

    private void StopAnimationsAndRollback()
    {
      // Ensure to pause any animation that was applied to ghost adorners.
      this.PauseGhostToMousePositionAnimation();
      this.PauseMoveGhostToTargetColumnAnimation();
      this.PauseMoveGhostToTargetAndDetachAnimation();

      m_reorderCancelled = false;

      this.ShowGhost = true;

      this.ClearColumnAnimations();
      this.ClearSplitterAnimation();
    }

    private void UpdateMouseMoveDirection( Func<IInputElement, Point> getPosition )
    {
      var draggedElement = this.DraggedElement;
      var draggedElementToMouse = getPosition.Invoke( draggedElement );
      var currentDraggedElementOffset = draggedElement.TranslatePoint( draggedElementToMouse, this.Container ).X;

      // Verify if there is an horizontal change according to the lastchange
      var dragRect = new Rect( m_lastDraggedElementOffset - SystemParameters.MinimumHorizontalDragDistance, 0d, SystemParameters.MinimumVerticalDragDistance * 2, 0d );
      if( dragRect.Contains( new Point( currentDraggedElementOffset, 0 ) ) )
        return;

      if( currentDraggedElementOffset < m_lastDraggedElementOffset )
      {
        m_horizontalMouseDragDirection = HorizontalMouseDragDirection.Left;
      }
      else if( currentDraggedElementOffset > m_lastDraggedElementOffset )
      {
        m_horizontalMouseDragDirection = HorizontalMouseDragDirection.Right;
      }

      m_lastDraggedElementOffset = currentDraggedElementOffset;
    }

    private void UpdateIsAnimatedColumnReorderingEnabled()
    {
      var tableflowView = m_dataGridContext.DataGridControl.GetView() as TableflowView;

      // Column reordering is not allowed in a detail when details are flatten.
      var isAnimatedColumnReorderingEnabled = ( tableflowView != null )
                                           && ( tableflowView.IsAnimatedColumnReorderingEnabled )
                                           && ( !m_dataGridContext.IsAFlattenDetail );

      // Make sure the dragged cell's parent row allows column reordering.
      if( isAnimatedColumnReorderingEnabled )
      {
        var draggedCell = this.DraggedElement as Cell;
        if( draggedCell != null )
        {
          // We don't want any ghost to be displayed when column reordering is not allowed by the parent row.
          // Setting the IsAnimatedColumnReorderingEnabled property will prevent this and will display the Cursor.No when dragging.
          var parentRow = draggedCell.ParentRow as ColumnManagerRow;
          if( parentRow != null )
          {
            isAnimatedColumnReorderingEnabled = parentRow.AllowColumnReorder;
          }

          // The main column is not allowed to be reorder when details are flatten.
          if( isAnimatedColumnReorderingEnabled && m_dataGridContext.AreDetailsFlatten )
          {
            var parentColumn = draggedCell.ParentColumn;
            if( parentColumn != null )
            {
              isAnimatedColumnReorderingEnabled = ( parentColumn != null ) && !parentColumn.IsMainColumn;
            }
          }
        }
      }

      this.IsAnimatedColumnReorderingEnabled = isAnimatedColumnReorderingEnabled;
    }

    private void MoveGhostToMousePosition( Point mousePosition )
    {
      // Ensure the opacity of the ghost is correctly set
      this.ShowDraggedColumnGhosts();

      var currentAdornerPosition = mousePosition;

      if( m_elementToDraggedElementAdorner.Count == 0 )
        return;

      var remainingDuration = s_returnToOriginalPositionDuration;

      if( m_ghostToMousePositionAnimationClock != null )
      {
        var pointAnimation = m_ghostToMousePositionAnimationClock.Timeline as PointAnimation;
        var currentValue = pointAnimation.To.GetValueOrDefault();
        var deltaX = currentValue.X - mousePosition.X;
        var deltaY = currentValue.Y - mousePosition.Y;

        var dragingX = Math.Abs( deltaX ) > SystemParameters.MinimumHorizontalDragDistance;
        var dragingY = Math.Abs( deltaY ) > SystemParameters.MinimumVerticalDragDistance;

        // If the target value is already the correct one, no need to stop animation and create another one
        if( ( pointAnimation != null ) && ( !dragingX && !dragingY ) )
          return;

        if( m_ghostToMousePositionAnimationClock.CurrentState == ClockState.Active )
        {
          // The remaining duration is the Timeline Duration less the elapsed time
          remainingDuration = new Duration( pointAnimation.Duration.TimeSpan - m_ghostToMousePositionAnimationClock.CurrentTime.Value );
        }
      }

      this.PauseGhostToMousePositionAnimation();

      var animation = new PointAnimation( currentAdornerPosition, mousePosition, remainingDuration );

      m_ghostToMousePositionAnimationClock = animation.CreateClock( true ) as AnimationClock;
      m_ghostToMousePositionAnimationClock.Completed += new EventHandler( this.GhostToMousePosition_Completed );

      foreach( var adorner in this.GetElementAdorners() )
      {
        adorner.ApplyAnimationClock( DraggedElementAdorner.OffsetProperty, m_ghostToMousePositionAnimationClock, HandoffBehavior.SnapshotAndReplace );
      }

      m_ghostToMousePositionAnimationClock.Controller.Begin();
    }

    private void GhostToMousePosition_Completed( object sender, EventArgs e )
    {
      if( m_ghostToMousePositionAnimationClock == sender )
      {
        this.PauseGhostToMousePositionAnimation();
      }

      // Ghosts are returned to their original position reordering was successfully cancelled
      m_reorderCancelled = false;
    }

    private void PauseGhostToMousePositionAnimation()
    {
      if( m_ghostToMousePositionAnimationClock == null )
        return;

      // Stop the animation, do not simply pause it, so it resets correctly.
      m_ghostToMousePositionAnimationClock.Controller.Stop();
      m_ghostToMousePositionAnimationClock.Completed -= new EventHandler( this.GhostToMousePosition_Completed );
      m_ghostToMousePositionAnimationClock = null;
    }

    private void MoveGhostToTargetColumn( Point mousePostion )
    {
      if( m_elementToDraggedElementAdorner.Count == 0 )
        return;

      var remainingDuration = s_returnToOriginalPositionDuration;

      if( m_ghostToTargetColumnAnimationClock != null )
      {
        var pointAnimation = m_ghostToTargetColumnAnimationClock.Timeline as PointAnimation;

        // If the target value is already the correct one, no need to stop animation and create another one
        if( ( pointAnimation != null ) && ( pointAnimation.To == s_origin ) )
          return;

        if( m_ghostToTargetColumnAnimationClock.CurrentState == ClockState.Active )
        {
          // The remaining duration is the Timeline Duration less the elapsed time
          remainingDuration = new Duration( pointAnimation.Duration.TimeSpan - m_ghostToTargetColumnAnimationClock.CurrentTime.Value );
        }
      }

      // We must apply the DraggedCell FadeIn animation to let the DraggedCell reappears while the ghosts are moving to their target position
      this.ApplyDraggedElementFadeInAnimation();
      this.PauseMoveGhostToTargetColumnAnimation();

      var animation = new PointAnimation( mousePostion, s_origin, remainingDuration );

      m_ghostToTargetColumnAnimationClock = animation.CreateClock( true ) as AnimationClock;
      m_ghostToTargetColumnAnimationClock.Completed += new EventHandler( this.GhostToTargetAnimation_Completed );

      foreach( var adorner in this.GetElementAdorners() )
      {
        adorner.ApplyAnimationClock( DraggedElementAdorner.OffsetProperty, m_ghostToTargetColumnAnimationClock, HandoffBehavior.SnapshotAndReplace );
      }
    }

    private void GhostToTargetAnimation_Completed( object sender, EventArgs e )
    {
      if( m_ghostToTargetColumnAnimationClock == sender )
      {
        this.PauseMoveGhostToTargetColumnAnimation();
      }
      else if( m_draggedElementFadeInAnimationClock == sender )
      {
        this.PauseDraggedElementFadeInAnimation();
      }

      if( ( m_ghostToTargetColumnAnimationClock == null ) && ( m_draggedElementFadeInAnimationClock == null ) )
      {
        // The ghosts were successfully moved to the target position so we hide the ghosts and display the Cells in order
        // for the DraggedElementGhost, which is a VisualBrush of the DraggedElement that is transparent during the drag.
        this.HideDraggedColumnGhosts();
        this.ShowDraggedElements();
        m_reorderCancelled = false;
      }
    }

    private void PauseMoveGhostToTargetColumnAnimation()
    {
      if( m_ghostToTargetColumnAnimationClock == null )
        return;

      // Stop the animation, do not simply pause it, so it resets correctly.
      m_ghostToTargetColumnAnimationClock.Controller.Stop();
      m_ghostToTargetColumnAnimationClock.Completed -= new EventHandler( this.GhostToTargetAnimation_Completed );
      m_ghostToTargetColumnAnimationClock = null;
    }

    private void MoveGhostToTargetAndDetach()
    {
      if( m_elementToDraggedElementAdorner.Count == 0 )
      {
        this.DetachManager();
        return;
      }

      this.PauseMoveGhostToTargetAndDetachAnimation();

      var draggedCell = this.DraggedElement as ColumnManagerCell;
      if( draggedCell == null )
        return;

      var parentColumn = draggedCell.ParentColumn;
      if( parentColumn == null )
        return;

      var parentRow = draggedCell.ParentRow;
      if( ( parentRow == null ) || ( parentRow.CellsHostPanel == null ) )
        return;

      var cellsHostPanel = parentRow.CellsHostPanel as FixedCellPanel;
      var draggedAdorner = m_elementToDraggedElementAdorner[ draggedCell ];
      var fromPoint = default( Point );

      if( ( draggedAdorner != null ) && ( cellsHostPanel != null ) )
      {
        Debug.Assert( m_columnVirtualizationManager != null );

        // Update the column layout in order to have the right offset.
        m_columnVirtualizationManager.Update();

        var toPoint = default( Point? );
        var fixedPanel = cellsHostPanel.FixedPanel as FixedCellSubPanel;
        var scrollingPanel = cellsHostPanel.ScrollingCellsDecorator.Child as VirtualizingFixedCellSubPanel;

        if( m_columnVirtualizationManager.GetScrollingFieldNames( m_level ).Contains( parentColumn ) )
        {
          if( scrollingPanel != null )
          {
            var targetPosition = scrollingPanel.CalculateCellOffset( parentColumn );

            // The position must be adjusted when the dragged cell is moved from the fixed panel to the scrolling panel.
            targetPosition.Offset( m_columnVirtualizationManager.FixedColumnsWidth - fixedPanel.ActualWidth, 0d );

            toPoint = scrollingPanel.TranslatePoint( targetPosition, draggedAdorner );
          }
        }
        else if( m_columnVirtualizationManager.GetFixedFieldNames( m_level ).Contains( parentColumn ) )
        {
          if( fixedPanel != null )
          {
            var targetPosition = fixedPanel.CalculateCellOffset( parentColumn );
            toPoint = fixedPanel.TranslatePoint( targetPosition, draggedAdorner );
          }
        }

        if( toPoint.HasValue )
        {
          fromPoint = new Point( -toPoint.Value.X, -toPoint.Value.Y );
        }
        else
        {
          fromPoint = draggedAdorner.Offset;
        }
      }

      var animation = new PointAnimation( fromPoint, new Point(), s_columnAnimationDuration );

      m_ghostToTargetAndDetachAnimationClock = ( AnimationClock )animation.CreateClock( true );
      m_ghostToTargetAndDetachAnimationClock.Completed += new EventHandler( this.MoveGhostToTargetAndDetach_Completed );

      //Animate all cells of all columns.
      foreach( var entry in this.GetElementAdornerEntries() )
      {
        var cell = entry.Key as Cell;
        if( cell == null )
        {
          Debug.Assert( false, "Only Cells should be dragged by this manager" );
          continue;
        }

        var adorner = entry.Value;
        adorner.ApplyAnimationClock( DraggedElementAdorner.OffsetProperty, m_ghostToTargetAndDetachAnimationClock, HandoffBehavior.SnapshotAndReplace );
      }

      m_ghostToTargetAndDetachAnimationClock.Controller.Begin();
    }

    private void MoveGhostToTargetAndDetach_Completed( object sender, EventArgs e )
    {
      // Ensure to stop and unregister any event handlers from animations if not null by completed
      this.PauseMoveGhostToTargetAndDetachAnimation();
      this.DetachManager();
    }

    private void PauseMoveGhostToTargetAndDetachAnimation()
    {
      if( m_ghostToTargetAndDetachAnimationClock == null )
        return;

      // Stop the animation, do not simply pause it, so it resets correctly.
      m_ghostToTargetAndDetachAnimationClock.Controller.Stop();
      m_ghostToTargetAndDetachAnimationClock.Completed -= new EventHandler( this.MoveGhostToTargetAndDetach_Completed );
      m_ghostToTargetAndDetachAnimationClock = null;
    }

    private void DetachManager()
    {
      this.ShowDraggedElements();

      var draggedCell = this.DraggedElement as Cell;
      var parentColumn = ( draggedCell != null ) ? draggedCell.ParentColumn : null;

      if( ( parentColumn != null ) && this.OwnElement( draggedCell ) )
      {
        parentColumn.SetIsBeingDraggedAnimated( false );
      }

      // Clear properties on the manager if it is the one currently in use on for this Detail level. This avoids problem with the FixedColumnSplitter
      // when multiple Cells are moved rapidly:
      // The FixedColumnSplitter listens to TableflowView.AreColumnsBeingReordered internal ViewProperty to bind to the TableflowView.FixedColumnSplitterTranslation internal 
      // ViewProperty. The internal ViewProperty ColumnReorderingDragSourceManager is affected when a new Drag begins. The old DragSourceManager is not stopped to allow 
      // any pending animations to complete smoothly. If the first manager clears the TableflowView.AreColumnBeingReordered property, the FixedColumnSplitters
      // just clears its binding to the FixedColumnSplitterTranslation and is no more animated.
      //
      // Ensuring that the current manager for this Detail level ensure that no other drag was initialized since this manager started animations.

      if( this.OwnElement( m_dataGridContext ) )
      {
        TableflowView.SetAreColumnsBeingReordered( m_dataGridContext, false );

        if( ( parentColumn != null ) && this.OwnElement( draggedCell ) )
        {
          parentColumn.ClearColumnReorderingDragSourceManager();
        }

        TableflowView.ClearColumnReorderingDragSourceManager( m_dataGridContext );
      }
    }

    private void UpdateColumnsLayout()
    {
      var targetCell = this.CurrentDropTarget as ColumnManagerCell;
      var targetPoint = this.CurrentDropTargetToContainerPosition;
      Debug.Assert( targetCell != null );
      Debug.Assert( DataGridControl.GetDataGridContext( targetCell ) == m_dataGridContext );

      var targetColumn = targetCell.ParentColumn;
      Debug.Assert( targetColumn != null );

      var targetLocation = m_columnsLayout[ targetColumn ];
      Debug.Assert( targetLocation != null );
      if( targetLocation == null )
        return;

      var draggedCell = this.DraggedElement as Cell;
      Debug.Assert( draggedCell != null );
      var draggedLocation = m_columnsLayout[ draggedCell.ParentColumn ];
      Debug.Assert( draggedLocation != null );
      if( draggedLocation == null )
        return;

      // Drop before target cell.
      if( ColumnReorderingDragSourceManager.IsPointLocatedBeforeTarget( targetCell, targetPoint ) )
      {
        if( !draggedLocation.CanMoveBefore( targetLocation ) || object.Equals( draggedLocation, targetLocation.GetPreviousSibling() ) )
          return;

        draggedLocation.MoveBefore( targetLocation );
      }
      // Drop after target cell.
      else if( ColumnReorderingDragSourceManager.IsPointLocatedAfterTarget( targetCell, targetPoint ) )
      {
        if( !draggedLocation.CanMoveAfter( targetLocation ) || object.Equals( draggedLocation, targetLocation.GetNextSibling() ) )
          return;

        draggedLocation.MoveAfter( targetLocation );
      }
      else
      {
        return;
      }

      this.UpdateColumnsLayoutAnimations();
    }

    private void UpdateColumnsLayoutAnimations()
    {
      this.ResetColumnsLayoutAnimations();

      var draggedCell = ( Cell )this.DraggedElement;
      var draggedColumn = draggedCell.ParentColumn;

      var draggedColumnReorderLocation = m_columnsLayout[ draggedColumn ];
      Debug.Assert( draggedColumnReorderLocation != null );
      if( draggedColumnReorderLocation == null )
        return;

      var draggedColumnSourceLocation = m_dataGridContext.ColumnManager.GetColumnLocationFor( draggedColumn );
      Debug.Assert( draggedColumnSourceLocation != null );
      if( draggedColumnSourceLocation == null )
        return;

      Debug.Assert( m_columnVirtualizationManager != null );

      var indexOfDraggedColumnInReorderedColumns = this.GetColumnLocations( this.GetPreviousLocationsOf( draggedColumnReorderLocation ) ).Count();
      var indexOfDraggedColumnInSourceColumns = this.GetColumnLocations( this.GetPreviousLocationsOf( draggedColumnSourceLocation ) ).Count();
      var columnsInView = m_columnVirtualizationManager.GetVisibleFieldNames( m_level );
      var isDraggingLeft = ( indexOfDraggedColumnInReorderedColumns < indexOfDraggedColumnInSourceColumns );
      var isDraggingRight = ( indexOfDraggedColumnInReorderedColumns > indexOfDraggedColumnInSourceColumns );

      if( isDraggingLeft )
      {
        var remaining = indexOfDraggedColumnInSourceColumns - indexOfDraggedColumnInReorderedColumns;

        foreach( var columnLocation in this.GetColumnLocations( this.GetNextLocationsOf( draggedColumnReorderLocation ) ) )
        {
          if( remaining <= 0 )
            break;

          if( columnsInView.Contains( columnLocation.Column ) )
          {
            this.AnimateColumn( columnLocation, AnimationType.MoveRight );
            this.AnimateColumnDescendants( columnLocation, AnimationType.MoveRight );
            this.AnimateColumnAncestors( columnLocation, AnimationType.MoveRight );
          }

          remaining--;
        }
      }
      else if( isDraggingRight )
      {
        var remaining = indexOfDraggedColumnInReorderedColumns - indexOfDraggedColumnInSourceColumns;

        foreach( var columnLocation in this.GetColumnLocations( this.GetPreviousLocationsOf( draggedColumnReorderLocation ) ) )
        {
          if( remaining <= 0 )
            break;

          if( columnsInView.Contains( columnLocation.Column ) )
          {
            this.AnimateColumn( columnLocation, AnimationType.MoveLeft );
            this.AnimateColumnDescendants( columnLocation, AnimationType.MoveLeft );
            this.AnimateColumnAncestors( columnLocation, AnimationType.MoveLeft );
          }

          remaining--;
        }
      }

      var commonAncestor = this.GetCommonAncestor( draggedColumnSourceLocation, draggedColumnReorderLocation );

      // Adjust the animation type on the ancestors.
      for( var childLocation = draggedColumnSourceLocation; childLocation != null; )
      {
        var parentLocation = childLocation.GetParent() as ColumnHierarchyManager.IColumnLocation;
        if( ( parentLocation == null ) || ( parentLocation.Column == commonAncestor ) )
          break;

        var columnLocation = m_columnsLayout[ parentLocation.Column ];
        Debug.Assert( columnLocation != null );

        switch( columnLocation.Status )
        {
          case AnimationType.MoveLeft:
            this.AnimateColumn( columnLocation, AnimationType.DecreaseWidth );
            break;

          case AnimationType.MoveRight:
            this.AnimateColumn( columnLocation, AnimationType.MoveRightAndDecreaseWidth );
            break;

          default:
            {
              if( isDraggingLeft )
              {
                this.AnimateColumn( columnLocation, AnimationType.MoveRightAndDecreaseWidth );
              }
              else if( isDraggingRight )
              {
                this.AnimateColumn( columnLocation, AnimationType.DecreaseWidth );
              }
              else
              {
                var wasFirstVisibleChildColumn = !this.GetVisibleLocations( this.GetColumnLocations( this.GetPreviousSiblingLocationsOf( childLocation ) ) ).Any();
                var wasLastVisibleChildColumn = !this.GetVisibleLocations( this.GetColumnLocations( this.GetNextSiblingLocationsOf( childLocation ) ) ).Any();

                // We must check for last first since we want to apply the "last" animation if the column is both
                // the first and last child.
                if( wasLastVisibleChildColumn )
                {
                  this.AnimateColumn( columnLocation, AnimationType.DecreaseWidth );
                }
                else if( wasFirstVisibleChildColumn )
                {
                  this.AnimateColumn( columnLocation, AnimationType.MoveRightAndDecreaseWidth );
                }
              }
            }
            break;
        }

        childLocation = parentLocation;
      }

      for( var childLocation = draggedColumnReorderLocation; childLocation != null; )
      {
        var parentLocation = childLocation.GetParent() as ColumnHierarchyModel.IColumnLocation;
        if( ( parentLocation == null ) || ( parentLocation.Column == commonAncestor ) )
          break;

        switch( parentLocation.Status )
        {
          case AnimationType.MoveLeft:
            this.AnimateColumn( parentLocation, AnimationType.MoveLeftAndIncreaseWidth );
            break;

          case AnimationType.MoveRight:
            this.AnimateColumn( parentLocation, AnimationType.IncreaseWidth );
            break;

          default:
            {
              if( isDraggingLeft )
              {
                this.AnimateColumn( parentLocation, AnimationType.IncreaseWidth );
              }
              else if( isDraggingRight )
              {
                this.AnimateColumn( parentLocation, AnimationType.MoveLeftAndIncreaseWidth );
              }
              else
              {
                var isFirstVisibleChildColumn = !this.GetVisibleLocations( this.GetColumnLocations( this.GetPreviousSiblingLocationsOf( childLocation ) ) ).Any();
                var isLastVisibleChildColumn = !this.GetVisibleLocations( this.GetColumnLocations( this.GetNextSiblingLocationsOf( childLocation ) ) ).Any();

                // We must check for last first since we want to apply the "last" animation if the column is both
                // the first and last child.
                if( isLastVisibleChildColumn )
                {
                  this.AnimateColumn( parentLocation, AnimationType.IncreaseWidth );
                }
                else if( isFirstVisibleChildColumn )
                {
                  this.AnimateColumn( parentLocation, AnimationType.MoveLeftAndIncreaseWidth );
                }
              }
            }
            break;
        }

        childLocation = parentLocation;
      }

      // Since the column is staying under the same ancestor, no animation needs to be done.
      if( commonAncestor != null )
      {
        var columnLocation = m_columnsLayout[ commonAncestor ];
        Debug.Assert( columnLocation != null );

        this.AnimateColumn( columnLocation, AnimationType.Rollback );
        this.AnimateColumnAncestors( columnLocation, AnimationType.Rollback );
      }
    }

    private void ResetColumnsLayoutAnimations()
    {
      for( int i = 0; i < m_columnsLayout.LevelCount; i++ )
      {
        foreach( var location in this.GetColumnLocations( this.GetNextLocationsOf( m_columnsLayout.GetLevelMarkers( i ).Start ) ) )
        {
          location.Status = AnimationType.Rollback;
        }
      }
    }

    private ColumnHierarchyManager.ILevelMarkers GetTopLevelMarkers()
    {
      return m_dataGridContext.ColumnManager.GetLevelMarkersFor(  m_dataGridContext.Columns );
    }

    private ColumnHierarchyManager.ILocation GetTopLevelStartLocation()
    {
      var levelMarkers = this.GetTopLevelMarkers();

      Debug.Assert( levelMarkers != null );
      if( levelMarkers == null )
        return null;

      return levelMarkers.Start;
    }

    private IEnumerable<ColumnHierarchyManager.IColumnLocation> GetColumnLocations( IEnumerable<ColumnHierarchyManager.ILocation> locations )
    {
      if( locations == null )
        return Enumerable.Empty<ColumnHierarchyManager.IColumnLocation>();

      return ( from location in locations
               let columnLocation = location as ColumnHierarchyManager.IColumnLocation
               where ( columnLocation != null )
               select columnLocation );
    }

    private IEnumerable<ColumnHierarchyModel.IColumnLocation> GetColumnLocations( IEnumerable<ColumnHierarchyModel.ILocation> locations )
    {
      if( locations == null )
        return Enumerable.Empty<ColumnHierarchyModel.IColumnLocation>();

      return ( from location in locations
               let columnLocation = location as ColumnHierarchyModel.IColumnLocation
               where ( columnLocation != null )
               select columnLocation );
    }

    private IEnumerable<ColumnHierarchyManager.IColumnLocation> GetVisibleLocations( IEnumerable<ColumnHierarchyManager.IColumnLocation> locations )
    {
      if( locations == null )
        return Enumerable.Empty<ColumnHierarchyManager.IColumnLocation>();

      return ( from location in locations
               where location.Column.Visible
               select location );
    }

    private IEnumerable<ColumnHierarchyModel.IColumnLocation> GetVisibleLocations( IEnumerable<ColumnHierarchyModel.IColumnLocation> locations )
    {
      if( locations == null )
        return Enumerable.Empty<ColumnHierarchyModel.IColumnLocation>();

      return ( from location in locations
               where location.Column.Visible
               select location );
    }

    private IEnumerable<ColumnHierarchyManager.ILocation> GetNextLocationsOf( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetNextSiblingOrCousin();
      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetNextSiblingOrCousin();
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetNextLocationsOf( ColumnHierarchyModel.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetNextSiblingOrCousin();
      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetNextSiblingOrCousin();
      }
    }

    private IEnumerable<ColumnHierarchyManager.ILocation> GetNextSiblingLocationsOf( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetNextSibling();
      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetNextSibling();
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetNextSiblingLocationsOf( ColumnHierarchyModel.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetNextSibling();
      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetNextSibling();
      }
    }

    private IEnumerable<ColumnHierarchyManager.ILocation> GetPreviousLocationsOf( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetPreviousSiblingOrCousin();
      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetPreviousSiblingOrCousin();
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetPreviousLocationsOf( ColumnHierarchyModel.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetPreviousSiblingOrCousin();
      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetPreviousSiblingOrCousin();
      }
    }

    private IEnumerable<ColumnHierarchyManager.ILocation> GetPreviousSiblingLocationsOf( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetPreviousSibling();
      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetPreviousSibling();
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetPreviousSiblingLocationsOf( ColumnHierarchyModel.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetPreviousSibling();
      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetPreviousSibling();
      }
    }

    private IEnumerable<ColumnHierarchyManager.ILocation> GetChildLocationsOf( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetFirstChild();

      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetNextSibling();
      }
    }

    private IEnumerable<ColumnHierarchyModel.ILocation> GetChildLocationsOf( ColumnHierarchyModel.ILocation location )
    {
      if( location == null )
        yield break;

      var targetLocation = location.GetFirstChild();

      while( targetLocation != null )
      {
        yield return targetLocation;
        targetLocation = targetLocation.GetNextSibling();
      }
    }

    private void ApplyColumnsLayoutAnimations()
    {
      var draggedCell = this.DraggedElement as Cell;
      Debug.Assert( draggedCell != null );

      var draggedColumn = draggedCell.ParentColumn;
      Debug.Assert( draggedColumn != null );

      var draggedColumnWidth = draggedColumn.ActualWidth;

      var startLocation = this.GetTopLevelStartLocation();
      Debug.Assert( startLocation != null );

      foreach( var location in this.GetNextLocationsOf( startLocation ) )
      {
        this.ApplyColumnsLayoutAnimations( location, draggedColumnWidth, s_columnAnimationDuration );
      }
    }

    private void ApplyColumnsLayoutAnimations( ColumnHierarchyManager.ILocation location, double draggedColumnWidth, Duration animationDuration )
    {
      if( location == null )
        return;

      var columnLocation = location as ColumnHierarchyManager.IColumnLocation;
      if( columnLocation != null )
      {
        var column = columnLocation.Column;
        var targetLocation = m_columnsLayout[ column ];
        var animationType = ( targetLocation != null ) ? targetLocation.Status : AnimationType.Rollback;

        switch( animationType )
        {
          case AnimationType.MoveLeft:
            {
              this.StartColumnPositionAnimation( column, -draggedColumnWidth, animationDuration );
            }
            break;

          case AnimationType.MoveRight:
            {
              this.StartColumnPositionAnimation( column, draggedColumnWidth, animationDuration );
            }
            break;

          case AnimationType.MoveLeftAndIncreaseWidth:
            {
            }
            break;

          case AnimationType.MoveRightAndDecreaseWidth:
            {
            }
            break;

          case AnimationType.IncreaseWidth:
            {
            }
            break;

          case AnimationType.DecreaseWidth:
            {
            }
            break;

          case AnimationType.Rollback:
            {
              this.StartColumnPositionAnimation( column, 0d, animationDuration );
            }
            break;

          default:
            throw new NotImplementedException();
        }
      }

      foreach( var childLocation in this.GetChildLocationsOf( location ) )
      {
        this.ApplyColumnsLayoutAnimations( childLocation, draggedColumnWidth, animationDuration );
      }
    }

    private void RollbackColumnsLayoutAnimations()
    {
      var startLocation = this.GetTopLevelStartLocation();
      Debug.Assert( startLocation != null );

      foreach( var location in this.GetNextLocationsOf( startLocation ) )
      {
        this.RollbackColumnsLayoutAnimations( location );
      }
    }

    private void RollbackColumnsLayoutAnimations( ColumnHierarchyManager.ILocation location )
    {
      if( location == null )
        return;

      var columnLocation = location as ColumnHierarchyManager.IColumnLocation;
      if( columnLocation != null )
      {
        var column = columnLocation.Column;

        this.StartColumnPositionAnimation( column, 0d, s_columnAnimationDuration );
      }

      foreach( var childLocation in this.GetChildLocationsOf( location ) )
      {
        this.RollbackColumnsLayoutAnimations( childLocation );
      }
    }

    private void ApplySplitterAnimation()
    {
      var draggedCell = this.DraggedElement as Cell;
      var draggedColumn = ( draggedCell != null ) ? draggedCell.ParentColumn : null;
      Debug.Assert( draggedColumn != null );

      var draggedColumnContainingCollection = ( draggedColumn != null ) ? draggedColumn.ContainingCollection : null;
      Debug.Assert( draggedColumnContainingCollection != null );

      var sourceLevelMarkers = ( draggedColumnContainingCollection != null ) ? m_dataGridContext.ColumnManager.GetLevelMarkersFor( draggedColumnContainingCollection ) : null;
      var sourceSplitter = ( sourceLevelMarkers != null ) ? sourceLevelMarkers.Splitter : null;

      var draggedColumnLocation = m_columnsLayout[ draggedColumn ];
      var reorderedLevelMarkers = ( draggedColumnLocation != null ) ? m_columnsLayout.GetLevelMarkers( draggedColumnLocation.Level ) : null;
      var reorderedSplitter = ( reorderedLevelMarkers != null ) ? reorderedLevelMarkers.Splitter : null;

      int oldFixedColumnCount;
      int newFixedColumnCount;

      if( ( sourceSplitter != null ) && ( reorderedSplitter != null ) )
      {
        oldFixedColumnCount = this.GetVisibleLocations( this.GetColumnLocations( this.GetPreviousLocationsOf( sourceSplitter ) ) ).Count();
        newFixedColumnCount = this.GetVisibleLocations( this.GetColumnLocations( this.GetPreviousLocationsOf( reorderedSplitter ) ) ).Count();
      }
      else
      {
        oldFixedColumnCount = 0;
        newFixedColumnCount = 0;
      }

      if( newFixedColumnCount > oldFixedColumnCount )
      {
        Debug.Assert( draggedColumn != null );

        this.StartSplitterPositionAnimation( draggedColumn.ActualWidth );
      }
      else if( newFixedColumnCount < oldFixedColumnCount )
      {
        Debug.Assert( draggedColumn != null );

        this.StartSplitterPositionAnimation( -draggedColumn.ActualWidth );
      }
      else
      {
        this.StartSplitterPositionAnimation( 0d );
      }
    }

    private void RollbackReordering()
    {
      m_reorderCancelled = true;

      this.RollbackColumnsLayoutAnimations();
      this.StartSplitterPositionAnimation( 0d );
    }

    private void StartColumnPositionAnimation( ColumnBase column, double offset, Duration duration )
    {
      if( column == null )
        return;

      var isMergedColumn = false;

      var transformGroup = ColumnReorderingDragSourceManager.GetColumnTransformGroup( column );
      var positionTransform = ColumnReorderingDragSourceManager.GetColumnPositionTransform( transformGroup );
      var sizeTransform = ColumnReorderingDragSourceManager.GetColumnSizeTransform( transformGroup );

      var positionClock = default( AnimationClock );
      var sizeClock = default( AnimationClock );
      var clocks = default( ColumnAnimationClocks );

      // Pause any previously started animation.
      if( m_animationClocks.TryGetValue( column, out clocks ) )
      {
        positionClock = clocks.Position;
        var positionTimeLine = ( positionClock != null ) ? positionClock.Timeline as OffsetAnimation : null;
        var resetPosition = ( positionTimeLine == null ) || ( positionTimeLine.To != offset );

        sizeClock = clocks.Size;
        var sizeTimeLine = ( sizeClock != null ) ? sizeClock.Timeline as OffsetAnimation : null;
        var resetSize = ( isMergedColumn && ( ( sizeTimeLine == null ) || ( sizeTimeLine.To != 1d ) ) )
                     || ( !isMergedColumn && ( sizeClock != null ) );

        // No animation needs to be done.
        if( !resetPosition && !resetSize )
          return;

        if( resetPosition && ( positionClock != null ) )
        {
          var controller = positionClock.Controller;
          Debug.Assert( controller != null );

          // Stop the animation, do not simply pause it, so it resets correctly.
          controller.Stop();
          controller.Remove();

          positionClock.Completed -= new EventHandler( this.OnRollbackColumnAnimationCompleted );
          positionClock = null;
        }

        if( resetSize && ( sizeClock != null ) )
        {
          var controller = sizeClock.Controller;
          Debug.Assert( controller != null );

          // Stop the animation, do not simply pause it, so it resets correctly.
          controller.Stop();
          controller.Remove();

          sizeClock.Completed -= new EventHandler( this.OnRollbackColumnAnimationCompleted );
          sizeClock = null;
        }
      }

      if( positionClock == null )
      {
        var animation = new OffsetAnimation( offset, duration );
        positionClock = animation.CreateClock( true ) as AnimationClock;
        positionTransform.ApplyAnimationClock( TranslateTransform.XProperty, positionClock, HandoffBehavior.SnapshotAndReplace );

        if( offset == 0d )
        {
          positionClock.Completed += new EventHandler( this.OnRollbackColumnAnimationCompleted );
        }

        positionClock.Controller.Begin();
      }

      Debug.Assert( positionClock != null );

      if( isMergedColumn && ( sizeClock == null ) )
      {
        var animation = new OffsetAnimation( 1d, duration );
        sizeClock = animation.CreateClock( true ) as AnimationClock;
        sizeTransform.ApplyAnimationClock( ScaleTransform.ScaleXProperty, sizeClock, HandoffBehavior.SnapshotAndReplace );

        if( offset == 0d )
        {
          sizeClock.Completed += new EventHandler( this.OnRollbackColumnAnimationCompleted );
        }

        sizeClock.Controller.Begin();
      }

      m_animationClocks[ column ] = new ColumnAnimationClocks( positionClock, sizeClock );
    }

    private void StartColumnSizeAnimation( ColumnBase column, double enlarge, Duration duration )
    {
      if( column == null )
        return;

      Debug.Assert( column.Width != 0d );

      //Calculate the resize ratio that needs to be applied to the ScaleTransform (works in %)
      var resizedWith = column.Width + enlarge;
      var resizedFactor = resizedWith / column.Width;

      var transformGroup = ColumnReorderingDragSourceManager.GetColumnTransformGroup( column );
      var positionTransform = ColumnReorderingDragSourceManager.GetColumnPositionTransform( transformGroup );
      var sizeTransform = ColumnReorderingDragSourceManager.GetColumnSizeTransform( transformGroup );

      var positionClock = default( AnimationClock );
      var sizeClock = default( AnimationClock );
      var clocks = default( ColumnAnimationClocks );

      // Pause any previously started animation.
      if( m_animationClocks.TryGetValue( column, out clocks ) )
      {
        positionClock = clocks.Position;
        var positionTimeLine = ( positionClock != null ) ? positionClock.Timeline as OffsetAnimation : null;
        var resetPosition = ( positionTimeLine == null ) || ( positionTimeLine.To != 0d );

        sizeClock = clocks.Size;
        var sizeTimeLine = ( sizeClock != null ) ? sizeClock.Timeline as OffsetAnimation : null;
        var resetSize = ( sizeTimeLine == null ) || ( sizeTimeLine.To != resizedFactor );

        // No animation needs to be done.
        if( !resetPosition && !resetSize )
          return;

        if( resetPosition && ( positionClock != null ) )
        {
          var controller = positionClock.Controller;
          Debug.Assert( controller != null );

          // Stop the animation, do not simply pause it, so it resets correctly.
          controller.Stop();
          controller.Remove();

          positionClock.Completed -= new EventHandler( this.OnRollbackColumnAnimationCompleted );
          positionClock = null;
        }

        if( resetSize && ( sizeClock != null ) )
        {
          var controller = sizeClock.Controller;
          Debug.Assert( controller != null );

          // Stop the animation, do not simply pause it, so it resets correctly.
          controller.Stop();
          controller.Remove();

          sizeClock.Completed -= new EventHandler( this.OnRollbackColumnAnimationCompleted );
          sizeClock = null;
        }
      }

      if( positionClock == null )
      {
        var animation = new OffsetAnimation( 0d, duration );
        positionClock = animation.CreateClock( true ) as AnimationClock;
        positionTransform.ApplyAnimationClock( TranslateTransform.XProperty, positionClock, HandoffBehavior.SnapshotAndReplace );

        if( enlarge == 0d )
        {
          positionClock.Completed += new EventHandler( this.OnRollbackColumnAnimationCompleted );
        }

        positionClock.Controller.Begin();
      }

      Debug.Assert( positionClock != null );

      if( sizeClock == null )
      {
        var animation = new OffsetAnimation( resizedFactor, duration );
        sizeClock = animation.CreateClock( true ) as AnimationClock;
        sizeTransform.ApplyAnimationClock( ScaleTransform.ScaleXProperty, sizeClock, HandoffBehavior.SnapshotAndReplace );

        if( enlarge == 0d )
        {
          sizeClock.Completed += new EventHandler( this.OnRollbackColumnAnimationCompleted );
        }

        sizeClock.Controller.Begin();
      }

      Debug.Assert( sizeClock != null );

      m_animationClocks[ column ] = new ColumnAnimationClocks( positionClock, sizeClock );
    }

    private void StartColumnPositionAndSizeAnimations( ColumnBase column, double offset, Duration duration )
    {
      if( column == null )
        return;

      Debug.Assert( column.Width != 0d );

      //Calculate the resize ratio that needs to be applied to the ScaleTransform (works in %)
      var resizedWidth = column.Width - offset;
      var resizedFactor = resizedWidth / column.Width;

      var transformGroup = ColumnReorderingDragSourceManager.GetColumnTransformGroup( column );
      var positionTransform = ColumnReorderingDragSourceManager.GetColumnPositionTransform( transformGroup );
      var sizeTransform = ColumnReorderingDragSourceManager.GetColumnSizeTransform( transformGroup );

      var positionClock = default( AnimationClock );
      var sizeClock = default( AnimationClock );
      var clocks = default( ColumnAnimationClocks );

      // Pause any previously started animation.
      if( m_animationClocks.TryGetValue( column, out clocks ) )
      {
        positionClock = clocks.Position;
        var positionTimeLine = ( positionClock != null ) ? positionClock.Timeline as OffsetAnimation : null;
        var resetPosition = ( positionTimeLine == null ) || ( positionTimeLine.To != offset );

        sizeClock = clocks.Size;
        var sizeTimeLine = ( sizeClock != null ) ? sizeClock.Timeline as OffsetAnimation : null;
        var resetSize = ( sizeTimeLine == null ) || ( sizeTimeLine.To != resizedFactor );

        // No animation needs to be done.
        if( !resetPosition && !resetSize )
          return;

        if( resetPosition && ( positionClock != null ) )
        {
          var controller = positionClock.Controller;
          Debug.Assert( controller != null );

          // Stop the animation, do not simply pause it, so it resets correctly.
          controller.Stop();
          controller.Remove();

          positionClock.Completed -= new EventHandler( this.OnRollbackColumnAnimationCompleted );
          positionClock = null;
        }

        if( resetSize && ( sizeClock != null ) )
        {
          var controller = sizeClock.Controller;
          Debug.Assert( controller != null );

          // Stop the animation, do not simply pause it, so it resets correctly.
          controller.Stop();
          controller.Remove();

          sizeClock.Completed -= new EventHandler( this.OnRollbackColumnAnimationCompleted );
          sizeClock = null;
        }
      }

      if( positionClock == null )
      {
        var animation = new OffsetAnimation( offset, duration );
        positionClock = animation.CreateClock( true ) as AnimationClock;
        positionTransform.ApplyAnimationClock( TranslateTransform.XProperty, positionClock, HandoffBehavior.SnapshotAndReplace );

        if( offset == 0d )
        {
          positionClock.Completed += new EventHandler( this.OnRollbackColumnAnimationCompleted );
        }

        positionClock.Controller.Begin();
      }

      Debug.Assert( positionClock != null );

      if( sizeClock == null )
      {
        var animation = new OffsetAnimation( resizedFactor, duration );
        sizeClock = animation.CreateClock( true ) as AnimationClock;

        // When doing both a position and a resize animation, the CenterX value must be set so the resizing is applied from the new position,
        // or else the offset at which is starts will be wrong.
        sizeTransform.CenterX = offset;
        sizeTransform.ApplyAnimationClock( ScaleTransform.ScaleXProperty, sizeClock, HandoffBehavior.SnapshotAndReplace );

        if( offset == 0d )
        {
          sizeClock.Completed += new EventHandler( this.OnRollbackColumnAnimationCompleted );
        }

        sizeClock.Controller.Begin();
      }

      Debug.Assert( sizeClock != null );

      m_animationClocks[ column ] = new ColumnAnimationClocks( positionClock, sizeClock );
    }

    private void StartSplitterPositionAnimation( double offset )
    {
      if( m_splitterAnimationClock != null )
      {
        var timeLine = m_splitterAnimationClock.Timeline as OffsetAnimation;

        // No animation needs to be done.
        if( ( timeLine != null ) && ( timeLine.To == offset ) )
          return;

        var controller = m_splitterAnimationClock.Controller;
        Debug.Assert( controller != null );

        controller.Stop();
        controller.Remove();

        m_splitterAnimationClock.Completed -= new EventHandler( this.OnRollbackSplitterPositionCompleted );
      }

      var animation = new OffsetAnimation( offset, s_columnAnimationDuration );
      m_splitterAnimationClock = animation.CreateClock( true ) as AnimationClock;
      m_splitterTranslation.ApplyAnimationClock( TranslateTransform.XProperty, m_splitterAnimationClock, HandoffBehavior.SnapshotAndReplace );

      if( m_splitterAnimationClock != null )
      {
        if( offset == 0d )
        {
          m_splitterAnimationClock.Completed += new EventHandler( this.OnRollbackSplitterPositionCompleted );
        }

        m_splitterAnimationClock.Controller.Begin();
      }
    }

    private void OnRollbackColumnAnimationCompleted( object sender, EventArgs e )
    {
      var animationClock = sender as AnimationClock;
      if( animationClock == null )
        return;

      animationClock.Completed -= new EventHandler( this.OnRollbackColumnAnimationCompleted );

      this.ClearAnimationClock( ref animationClock );
    }

    private void OnRollbackSplitterPositionCompleted( object sender, EventArgs e )
    {
      var animationClock = sender as AnimationClock;
      if( animationClock == null )
        return;

      animationClock.Completed -= new EventHandler( this.OnRollbackSplitterPositionCompleted );

      if( m_splitterAnimationClock == animationClock )
      {
        this.ClearAnimationClock( ref m_splitterAnimationClock );

        Debug.Assert( ( m_splitterTranslation == null ) || ( m_splitterTranslation.X == 0d ) );
      }
      else
      {
        this.ClearAnimationClock( ref animationClock );
      }
    }

    private void ClearAnimationClock( ref AnimationClock animationClock )
    {
      if( animationClock == null )
        return;

      var controller = animationClock.Controller;
      Debug.Assert( controller != null );

      controller.Stop();
      controller.Remove();

      animationClock = null;
    }

    private void ClearSplitterAnimationClock()
    {
      if( m_splitterAnimationClock == null )
        return;

      m_splitterAnimationClock.Completed -= new EventHandler( this.OnRollbackSplitterPositionCompleted );

      this.ClearAnimationClock( ref m_splitterAnimationClock );
    }

    private void ClearColumnAnimations()
    {
      foreach( var column in m_dataGridContext.Columns )
      {
        ColumnReorderingDragSourceManager.ClearAnimatedColumnReorderingTranslation( column );
      }

      m_animationClocks.Clear();
    }

    private void ClearSplitterAnimation()
    {
      this.ClearSplitterAnimationClock();

      ColumnReorderingDragSourceManager.ClearTranslateTransformAnimation( TableflowView.GetFixedColumnSplitterTranslation( m_dataGridContext ) );
    }

    private void ApplyContainerClip( DraggedElementAdorner adorner )
    {
      if( adorner == null )
        return;

      var adornedElement = adorner.AdornedElement;
      if( adornedElement == null )
        return;

      // We only want to clip Cells other than the dragged Cell
      if( adornedElement == this.DraggedElement )
        return;

      var dataGridContext = DataGridControl.GetDataGridContext( adornedElement );
      if( dataGridContext == null )
        return;

      var dataItemStore = CustomItemContainerGenerator.GetDataItemProperty( adornedElement );
      if( ( dataItemStore == null ) || dataItemStore.IsEmpty )
        return;

      var dataItem = dataItemStore.Data;
      if( dataItem == null )
        return;

      var container = dataGridContext.GetContainerFromItem( dataItem ) as UIElement;
      if( container != null )
      {
        var containerClip = container.Clip as RectangleGeometry;
        if( containerClip != null )
        {
          // Transform the bounds of the clip region of the container to fit the bounds of the adornedElement
          var transformedBounds = container.TransformToDescendant( adornedElement ).TransformBounds( containerClip.Bounds );

          containerClip = new RectangleGeometry( transformedBounds );
        }

        adorner.AdornedElementImage.Clip = containerClip;
      }
      else
      {
        //This is an item that is in the FixedHeaders/Footers of the gird, do not clip it.
        adorner.AdornedElementImage.Clip = null;
      }
    }

    private void ShowDraggedColumnGhosts()
    {
      foreach( var adorner in this.GetElementAdorners() )
      {
        this.ApplyContainerClip( adorner );
        adorner.AdornedElementImage.Opacity = 1d;
      }
    }

    private void HideDraggedColumnGhosts()
    {
      foreach( var adorner in this.GetElementAdorners() )
      {
        adorner.AdornedElementImage.Opacity = 0d;
      }
    }

    private void ShowDraggedElements()
    {
      this.PauseDraggedElementFadeInAnimation();

      foreach( var element in this.GetElements() )
      {
        element.Opacity = 1d;
      }
    }

    private void HideDraggedElements()
    {
      this.PauseDraggedElementFadeInAnimation();

      foreach( var element in this.GetElements() )
      {
        element.Opacity = 0d;
      }
    }

    private void ApplyDraggedElementFadeInAnimation()
    {
      var remainingOpacityDuration = s_draggedElementFadeInDuration;

      if( m_draggedElementFadeInAnimationClock != null )
      {
        var fadeInAnimation = m_draggedElementFadeInAnimationClock.Timeline as OffsetAnimation;

        if( ( fadeInAnimation != null ) && ( fadeInAnimation.To == 1d ) )
          return;

        if( m_draggedElementFadeInAnimationClock.CurrentState == ClockState.Active )
        {
          // The remaining duration is the Timeline Duration less the elapsed time
          remainingOpacityDuration = new Duration( fadeInAnimation.Duration.TimeSpan - m_draggedElementFadeInAnimationClock.CurrentTime.Value );
        }
      }

      this.PauseDraggedElementFadeInAnimation();

      var draggedElement = this.DraggedElement;

      // Apply an animation on the opacity of the Cell to ensure a nice transition between the ColumnReordering and another IDropTarget that is not handled by this manager
      if( draggedElement != null )
      {
        var opacityAnimation = new OffsetAnimation( 1d, remainingOpacityDuration );

        m_draggedElementFadeInAnimationClock = opacityAnimation.CreateClock( true ) as AnimationClock;

        // We ust the same completed callback as MoveGhostToTarget animation to  be sure that the DraggedElement
        // FadeIn is completed and the ghosts are correctly positioned before displaying the original UIElements
        m_draggedElementFadeInAnimationClock.Completed += new EventHandler( this.GhostToTargetAnimation_Completed );

        draggedElement.ApplyAnimationClock( UIElement.OpacityProperty, m_draggedElementFadeInAnimationClock );

        m_draggedElementFadeInAnimationClock.Controller.Begin();
      }
    }

    private void PauseDraggedElementFadeInAnimation()
    {
      // Ensure to stop dragged element FadeIn in order to correctly display the DraggedElementGhost
      var draggedElement = this.DraggedElement;
      if( draggedElement != null )
      {
        draggedElement.ApplyAnimationClock( UIElement.OpacityProperty, null );
      }

      if( m_draggedElementFadeInAnimationClock == null )
        return;

      // Stop the animation, do not simply pause it, so it resets correctly.
      m_draggedElementFadeInAnimationClock.Controller.Stop();
      m_draggedElementFadeInAnimationClock.Completed -= new EventHandler( this.GhostToTargetAnimation_Completed );
      m_draggedElementFadeInAnimationClock = null;
    }

    private void TakeColumnsLayoutSnapshot()
    {
      Debug.Assert( !m_dataGridContext.ColumnManager.IsUpdateDeferred );

      m_columnsLayout.Clear();

      var levelCount = 1;

      for( int i = 0; i < levelCount; i++ )
      {
        m_columnsLayout.AddLevel( i );
      }

      var sourceLevelMarkers = this.GetTopLevelMarkers();
      Debug.Assert( sourceLevelMarkers != null );

      var pivotLocation = m_columnsLayout.GetLevelMarkers( levelCount - 1 ).Splitter;
      Debug.Assert( pivotLocation != null );

      // Clone all column locations.
      for( var currentLocation = sourceLevelMarkers.Start; currentLocation != null; currentLocation = currentLocation.GetNextSibling() )
      {
        var cloneLocation = ColumnReorderingDragSourceManager.CloneLocation( m_columnsLayout, currentLocation, levelCount - 1 );
        if( cloneLocation == null )
          continue;

        Debug.Assert( cloneLocation.CanMoveAfter( pivotLocation ) );
        cloneLocation.MoveAfter( pivotLocation );

        pivotLocation = cloneLocation;
      }

      // Move the splitter to get an exact copy.
      var sourcePivotLocation = sourceLevelMarkers.Splitter.GetPreviousSibling();
      Debug.Assert( sourcePivotLocation != null );

      var levelMarkers = m_columnsLayout.GetLevelMarkers( levelCount - 1 );
      var splitterLocation = levelMarkers.Splitter;

      if( sourcePivotLocation.Type == LocationType.Column )
      {
        pivotLocation = m_columnsLayout[ ( ( ColumnHierarchyManager.IColumnLocation )sourcePivotLocation ).Column ];
      }
      else
      {
        Debug.Assert( sourcePivotLocation.Type == LocationType.Start );
        pivotLocation = levelMarkers.Start;
      }

      Debug.Assert( splitterLocation != null );
      Debug.Assert( pivotLocation != null );

      Debug.Assert( splitterLocation.CanMoveAfter( pivotLocation ) );
      splitterLocation.MoveAfter( pivotLocation );
    }

    private static ColumnHierarchyModel.ILocation CloneLocation( ColumnHierarchyModel columnsLayout, ColumnHierarchyManager.ILocation sourceLocation, int level )
    {
      Debug.Assert( columnsLayout != null );
      Debug.Assert( sourceLocation != null );
      Debug.Assert( level >= 0 );

      var parentLocation = default( ColumnHierarchyModel.ILocation );

      if( sourceLocation.Type == LocationType.Column )
      {
        var columnLocation = columnsLayout.Add( ( ( ColumnHierarchyManager.IColumnLocation )sourceLocation ).Column, level );
        if( columnLocation == null )
          throw new InvalidOperationException();

        columnLocation.Status = AnimationType.Rollback;

        parentLocation = columnLocation;
      }
      else if( sourceLocation.Type == LocationType.Orphan )
      {
        parentLocation = columnsLayout.GetLevelMarkers( level ).Orphan;
        if( parentLocation == null )
          throw new InvalidOperationException();
      }

      if( parentLocation != null )
      {
        var lastLocation = default( ColumnHierarchyModel.ILocation );
        var childLocation = sourceLocation.GetFirstChild();

        while( childLocation != null )
        {
          var cloneLocation = ColumnReorderingDragSourceManager.CloneLocation( columnsLayout, childLocation, level - 1 );
          if( cloneLocation != null )
          {
            if( lastLocation != null )
            {
              Debug.Assert( cloneLocation.CanMoveAfter( lastLocation ) );
              cloneLocation.MoveAfter( lastLocation );
            }
            else
            {
              Debug.Assert( cloneLocation.CanMoveUnder( parentLocation ) );
              cloneLocation.MoveUnder( parentLocation );
            }

            lastLocation = cloneLocation;
          }

          childLocation = childLocation.GetNextSibling();
        }

        if( parentLocation.Type == LocationType.Column )
          return parentLocation;
      }

      return null;
    }

    private ColumnBase GetCommonAncestor( ColumnHierarchyManager.IColumnLocation x, ColumnHierarchyModel.IColumnLocation y )
    {
      Debug.Assert( x != null );
      Debug.Assert( y != null );

      var ancestors = new HashSet<ColumnBase>();

      for( var parentLocation = x.GetParent(); parentLocation != null; parentLocation = parentLocation.GetParent() )
      {
        var columnLocation = parentLocation as ColumnHierarchyManager.IColumnLocation;
        if( columnLocation == null )
          break;

        ancestors.Add( columnLocation.Column );
      }

      for( var parentLocation = y.GetParent(); parentLocation != null; parentLocation = parentLocation.GetParent() )
      {
        var columnLocation = parentLocation as ColumnHierarchyModel.IColumnLocation;
        if( columnLocation == null )
          break;

        var column = columnLocation.Column;
        if( ancestors.Contains( column ) )
          return column;
      }

      return null;
    }

    private void AnimateColumn( ColumnHierarchyModel.IColumnLocation columnLocation, AnimationType animationType )
    {
      Debug.Assert( columnLocation != null );
      columnLocation.Status = animationType;
    }

    private void AnimateColumnDescendants( ColumnHierarchyModel.IColumnLocation columnLocation, AnimationType animationType )
    {
      Debug.Assert( columnLocation != null );

      foreach( var childLocation in this.GetColumnLocations( this.GetChildLocationsOf( columnLocation ) ) )
      {
        this.AnimateColumn( childLocation, animationType );
        this.AnimateColumnDescendants( childLocation, animationType );
      }
    }

    private void AnimateColumnAncestors( ColumnHierarchyModel.IColumnLocation columnLocation, AnimationType animationType )
    {
      Debug.Assert( columnLocation != null );

      var parentColumnLocation = columnLocation.GetParent() as ColumnHierarchyModel.IColumnLocation;
      while( parentColumnLocation != null )
      {
        this.AnimateColumn( parentColumnLocation, animationType );

        parentColumnLocation = parentColumnLocation.GetParent() as ColumnHierarchyModel.IColumnLocation;
      }
    }

    private IEnumerable<UIElement> GetElements()
    {
      return this.GetElementAdornerEntries().Select( item => item.Key );
    }

    private IEnumerable<DraggedElementAdorner> GetElementAdorners()
    {
      return this.GetElementAdornerEntries().Select( item => item.Value );
    }

    private IEnumerable<KeyValuePair<UIElement, DraggedElementAdorner>> GetElementAdornerEntries()
    {
      var entries = m_elementToDraggedElementAdorner.ToList();

      foreach( var entry in entries )
      {
        var container = entry.Key;

        if( this.OwnElement( container ) )
        {
          Debug.Assert( m_elementToDraggedElementAdorner.ContainsKey( container ) );

          yield return entry;
        }
        else
        {
          this.RemoveDraggedColumnGhost( container );
        }
      }
    }

    private bool OwnElement( DependencyObject target )
    {
      if( target == null )
        return false;

      ColumnReorderingDragSourceManager manager;

      var cell = target as Cell;
      if( cell != null )
      {
        manager = cell.ParentColumnReorderingDragSourceManager;
      }
      else
      {
        manager = TableflowView.GetColumnReorderingDragSourceManager( target );
      }

      return ( this == manager );
    }

    private readonly int m_level;
    private readonly DataGridContext m_dataGridContext;
    private readonly TranslateTransform m_splitterTranslation;
    private readonly TableViewColumnVirtualizationManagerBase m_columnVirtualizationManager;
    private readonly ColumnHierarchyModel m_columnsLayout = new ColumnHierarchyModel();
    private readonly Dictionary<ColumnBase, ColumnAnimationClocks> m_animationClocks = new Dictionary<ColumnBase, ColumnAnimationClocks>();
    private readonly Dictionary<UIElement, DraggedElementAdorner> m_elementToDraggedElementAdorner = new Dictionary<UIElement, DraggedElementAdorner>();

    private bool m_isDragStarted; //false
    private bool m_reorderCancelled; //false;
    private bool m_noColumnsReorderingNeeded; //false

    private AnimationClock m_draggedElementFadeInAnimationClock; //null
    private AnimationClock m_ghostToTargetAndDetachAnimationClock; //null;
    private AnimationClock m_ghostToTargetColumnAnimationClock; //null;
    private AnimationClock m_ghostToMousePositionAnimationClock; //null;
    private AnimationClock m_splitterAnimationClock; //null;

    private DraggedElementAdorner m_popupDraggedElementAdorner; //null
    private HorizontalMouseDragDirection m_horizontalMouseDragDirection = HorizontalMouseDragDirection.None;
    private double m_lastDraggedElementOffset; // 0d;

    #region HorizontalMouseDragDirection Private Enum

    private enum HorizontalMouseDragDirection
    {
      None,
      Left,
      Right
    }

    #endregion

    #region AnimationType Private Enum

    private enum AnimationType
    {
      Rollback = 0,
      MoveLeft,
      MoveRight,
      IncreaseWidth,
      DecreaseWidth,
      MoveLeftAndIncreaseWidth,
      MoveRightAndDecreaseWidth,
    }

    #endregion

    #region ColumnAnimationClocks Private Struct

    private struct ColumnAnimationClocks
    {
      internal ColumnAnimationClocks( AnimationClock position, AnimationClock size )
      {
        m_position = position;
        m_size = size;
      }

      internal AnimationClock Position
      {
        get
        {
          return m_position;
        }
      }

      internal AnimationClock Size
      {
        get
        {
          return m_size;
        }
      }

      private readonly AnimationClock m_position;
      private readonly AnimationClock m_size;
    }

    #endregion

    #region ColumnHierarchyModel Private Class

    private sealed class ColumnHierarchyModel : ColumnHierarchyModel<ColumnBase, AnimationType>
    {
    }

    #endregion
  }
}
