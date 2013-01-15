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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Utils.Wpf.DragDrop;
using System.Windows;
using System.Windows.Documents;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.Windows.Controls;
using Xceed.Utils.Math;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class ColumnReorderingDragSourceManager : DragSourceManager
  {
    public ColumnReorderingDragSourceManager( UIElement draggedElement,
                              AdornerLayer adornerLayerInsideDragContainer,
                              UIElement dragContainer )
      : this( draggedElement, adornerLayerInsideDragContainer, dragContainer, true )
    {
    }

    public ColumnReorderingDragSourceManager( UIElement draggedElement,
                              AdornerLayer adornerLayerInsideDragContainer,
                              UIElement dragContainer,
                              bool enableAutoScroll )
      : this( draggedElement, adornerLayerInsideDragContainer, dragContainer, enableAutoScroll, true )
    {

    }

    public ColumnReorderingDragSourceManager( UIElement draggedElement,
                                                      AdornerLayer adornerLayerInsideDragContainer,
                                                      UIElement dragContainer,
                                                      bool enableAutoScroll,
                                                      bool showDraggedElementGhost )
      : base( draggedElement, adornerLayerInsideDragContainer, dragContainer, enableAutoScroll, showDraggedElementGhost )
    {
    }

    #region IsAnimatedColumnReorderingEnabled Public Property

    public bool IsAnimatedColumnReorderingEnabled
    {
      get;
      set;
    }

    #endregion

    #region AnimatedColumnReorderingTranslation Attached Property

    // This translation will be used by the AnimatedColumnReorderingManager to apply
    // a TranslateTransform to Columns that require it in order to give a preview
    // of the reordering in an animated or live way
    internal static readonly DependencyProperty AnimatedColumnReorderingTranslationProperty = DependencyProperty.RegisterAttached(
      "AnimatedColumnReorderingTranslation",
      typeof( TranslateTransform ),
      typeof( ColumnReorderingDragSourceManager ),
      new FrameworkPropertyMetadata( null ) );

    internal static TranslateTransform GetAnimatedColumnReorderingTranslation( DependencyObject obj )
    {
      return ( TranslateTransform )obj.GetValue( ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty );
    }

    internal static void SetAnimatedColumnReorderingTranslation( DependencyObject obj, TranslateTransform value )
    {
      obj.SetValue( ColumnReorderingDragSourceManager.AnimatedColumnReorderingTranslationProperty, value );
    }

    #endregion

    #region ColumnAnimationDuration Public Property

    public int ColumnAnimationDuration
    {
      get
      {
        return m_columnAnimationDurationCache;
      }
      set
      {
        if( m_columnAnimationDurationCache != value )
        {
          m_columnAnimationDurationCache = value;
          // Keep a cache of the duration as a Duration struct for performance
          m_columnAnimationDuration
            = new Duration( TimeSpan.FromMilliseconds( m_columnAnimationDurationCache ) );
        }
      }
    }

    #endregion

    #region DraggedElementFadeInDuration Public Property

    public int DraggedElementFadeInDuration
    {
      get;
      set;
    }

    #endregion

    #region ReturnToOriginalPositionDuration Public Property

    public int ReturnToOriginalPositionDuration
    {
      get;
      set;
    }

    #endregion

    #region DraggedDataGridContext Private Property

    private DataGridContext DraggedDataGridContext
    {
      get;
      set;
    }

    #endregion

    #region FixedColumnSplitterTranslation Private Property

    private TranslateTransform FixedColumnSplitterTranslation
    {
      get;
      set;
    }

    #endregion

    #region ColumnVirtualizationManager Private Property

    private TableViewColumnVirtualizationManager ColumnVirtualizationManager
    {
      get;
      set;
    }

    #endregion

    #region ReorderingInfoManagerInstance Private Read-Only Singleton Property

    private ReorderingInfoManager ReorderingInfoManagerInstance
    {
      get
      {
        if( m_reorderingInfoManager == null )
        {
          m_reorderingInfoManager = new ReorderingInfoManager();
        }

        return m_reorderingInfoManager;
      }
    }

    #endregion

    public void AddDraggedColumnGhost( UIElement element )
    {
      if( element == null )
        return;

      // Get the Rect for the DataGridControl
      DataGridControl dataGridControl = this.DraggedDataGridContext.DataGridControl;

      Rect dataGridControlRect = new Rect( 0,
        0,
        dataGridControl.ActualWidth,
        dataGridControl.ActualHeight );

      Point elementToDataGridControl
        = element.TranslatePoint( ColumnReorderingDragSourceManager.EmptyPoint, dataGridControl );

      // Get the Rect for the element that request a ghost
      Rect elementRect = new Rect( elementToDataGridControl, element.RenderSize );

      // This is a special case with the current Element that is 
      // always be layouted, but can be out of view
      if( !elementRect.IntersectsWith( dataGridControlRect ) )
        return;

      // We already have a DraggedElementAdorner for this Cell
      if( m_elementToDraggedElementAdorner.ContainsKey( element ) )
        return;

      AnimatedDraggedElementAdorner adorner
        = new AnimatedDraggedElementAdorner( element, this.AdornerLayerInsideDragContainer, true );

      this.ApplyContainerClip( adorner );

      this.AdornerLayerInsideDragContainer.Add( adorner );

      m_elementToDraggedElementAdorner.Add( element, adorner );
    }

    public void RemoveDraggedColumnGhost( UIElement element )
    {
      if( m_elementToDraggedElementAdorner.Count == 0 )
        return;

      DraggedElementAdorner adorner = null;

      if( !m_elementToDraggedElementAdorner.TryGetValue( element, out adorner ) )
        return;

      adorner = m_elementToDraggedElementAdorner[ element ];
      Debug.Assert( adorner != null );
      this.AdornerLayerInsideDragContainer.Remove( adorner );
      m_elementToDraggedElementAdorner.Remove( element );
    }

    public override void SetDraggedElement( UIElement newDraggedElement )
    {
      // Ensure to remove all ghost previously
      // registered with the manager
      this.RemoveAllGhosts();

      base.SetDraggedElement( newDraggedElement );

      // No need to initialize if new DraggedElement is null
      if( newDraggedElement != null )
        return;

      // Must re-initialize the manager before processing the new DraggedElement
      this.InitializeManager();
    }

    public override void ProcessMouseMove( MouseEventArgs e )
    {
      if( this.IsAnimatedColumnReorderingEnabled )
      {
        this.UpdateMouseMoveDirection( e );
      }

      base.ProcessMouseMove( e );
    }

    public override void ProcessMouseLeftButtonDown( MouseButtonEventArgs e )
    {
      // Must re-initialize the manager before processing the MouseLeftButtonDown
      // to ensure the internal state is up to date
      this.InitializeManager();

      this.UpdateIsAnimatedColumnReorderingEnabled();

      base.ProcessMouseLeftButtonDown( e );

      //When the grid is hosted in a popup window, it is not possible to know the position of the popup in certain scenarios (e.g. fullscreen, popup openning upward).
      //Thus we must use a regular system adorner instead of the ghost window, so that the dragged adorner will appear correctly under the mouse pointer.
      if( this.ParentWindowIsPopup )
      {
        this.SetPopupDragAdorner( e.Source as ColumnManagerCell );
      }

      if( !this.IsAnimatedColumnReorderingEnabled )
      {
        this.AutoScrollInterval = 50;
        this.TimerInterval = 5;
        return;
      }

      this.AutoScrollInterval = 0;
      this.TimerInterval = 0;

      UIElement draggedElement = this.DraggedElement;
      ReorderingInfoManager reorderingInfoManager = this.ReorderingInfoManagerInstance;
      DataGridContext dataGridContext = this.DraggedDataGridContext;

      Cell draggedCell = draggedElement as Cell;

      if( draggedCell != null )
      {
        reorderingInfoManager.BeginReordering( dataGridContext, draggedCell.ParentColumn.VisiblePosition );
      }

      // Get initial mouse positions
      Point draggedElementToMouse = e.GetPosition( draggedElement );

      m_lastDraggedElementOffset = draggedElement.TranslatePoint(
        draggedElementToMouse,
        this.DragContainer ).X;

      // Affect the manager on the DataGridContext and DraggedColumn
      TableflowView.SetAreColumnsBeingReordered( dataGridContext, true );

      if( draggedCell != null )
      {
        TableflowView.SetColumnReorderingDragSourceManager( draggedCell.ParentColumn, this );
      }

      TableflowView.SetColumnReorderingDragSourceManager( dataGridContext, this );
    }

    public override void ProcessMouseLeftButtonUp( MouseButtonEventArgs e )
    {
      base.ProcessMouseLeftButtonUp( e );

      //If in a popup, we are using a system adorner, and we need to hide it when releasing the mouse button.
      if( this.ParentWindowIsPopup )
      {
        m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 0;
        m_popupDraggedElementAdorner.SetOffset( ColumnReorderingDragSourceManager.EmptyPoint );
      }

      if( !this.IsAnimatedColumnReorderingEnabled )
        return;

      this.ReorderingInfoManagerInstance.EndReordering();
      this.MoveGhostToTargetAndDetach();
      this.CleanUpColumnAnimations();
    }

    public override void ProcessPreviewMouseRightButtonDown( MouseButtonEventArgs e )
    {
      base.ProcessPreviewMouseRightButtonDown( e );

      if( !this.IsAnimatedColumnReorderingEnabled )
        return;

      this.RollbackReordering();
      this.ReorderingInfoManagerInstance.EndReordering();

      // Ensure to rollback and detach the manager when a right click 
      // is detected to ensure the Ghosts are correctly removed
      // from the VisualTree
      this.MoveGhostToTargetAndDetach();
    }

    protected override IDropTarget GetDropTargetOnDrag( MouseEventArgs e, out Nullable<Point> dropTargetPosition, out IDropTarget lastFoundDropTarget )
    {
      IDropTarget returnedDropTarget = base.GetDropTargetOnDrag( e, out dropTargetPosition, out lastFoundDropTarget );

      if( !this.IsAnimatedColumnReorderingEnabled )
        return returnedDropTarget;

      ColumnManagerCell cell = e.OriginalSource as ColumnManagerCell;

      if( cell == null )
        return returnedDropTarget;

      Row parentRow = cell.ParentRow;

      if( parentRow == null )
        return returnedDropTarget;

      // Rollback reordering only if dragging outside or if the base class found a DropTarget that is not a ColumnManagerCell or ColumnManagerRow at the current mouse position
      if( ( returnedDropTarget != null ) && !( returnedDropTarget is ColumnManagerCell ) && !( returnedDropTarget is ColumnManagerRow ) )
      {
        return returnedDropTarget;
      }

      // If there is no IDropTarget currently dragging over but one was found other than ColumnManagerRow or ColumnManagerCell this means this IDropTarget can't receive the drop
      if( ( lastFoundDropTarget != null ) && !( lastFoundDropTarget is ColumnManagerCell ) && !( lastFoundDropTarget is ColumnManagerRow ) )
      {
        return null;
      }

      UIElement dragContainer = this.DragContainer;
      Debug.Assert( dragContainer != null );

      Cell draggedCell = this.DraggedElement as Cell;
      Debug.Assert( draggedCell != null );
      Debug.Assert( draggedCell == this.DraggedElement );

      Point parentRowToDragContainer = parentRow.TranslatePoint( ColumnReorderingDragSourceManager.EmptyPoint, dragContainer );

      Rect parentRowRect = new Rect( parentRowToDragContainer.X, parentRowToDragContainer.Y, parentRow.ActualWidth, parentRow.ActualHeight );

      Point draggedElementToMouse = e.GetPosition( draggedCell );
      draggedElementToMouse.X -= this.InitialMousePositionToDraggedElement.Value.X;

      Point draggedElementToDragContainer = draggedCell.TranslatePoint( draggedElementToMouse, dragContainer );

      double draggedelementWidth = draggedCell.RenderSize.Width;

      // Get the Rect for the DragContainer
      Rect dragContainerRect = new Rect( ColumnReorderingDragSourceManager.EmptyPoint, dragContainer.RenderSize );

      Point dragContainerToMouse = e.GetPosition( dragContainer );

      // If the mouse cursor is currently over the DragContainer
      if( dragContainerRect.Contains( dragContainerToMouse ) )
      {
        // Ensure the DraggedElement is not visible to HitTest
        draggedCell.IsHitTestVisible = false;

        // Correct the Y coordinate of the 
        double correctedY = parentRowRect.Y + ( draggedCell.ActualHeight / 2 );

        // Correct the Y part to ensure it is over the ParentRow
        Point correctedPoint = new Point( draggedElementToDragContainer.X, correctedY );

        if( m_horizontalMouseDragDirection == HorizontalMouseDragDirection.Left )
        {
          double rightEdge = draggedElementToDragContainer.X + draggedelementWidth;

          // When scrolling left, try to find the first IDropTarget from the left edge of the dragged element to the mouse cursor to the mouse cursor over the dragged element
          while( ( returnedDropTarget == null ) && ( correctedPoint.X < rightEdge ) )
          {
            returnedDropTarget = ColumnReorderingDragSourceManager.GetDropTargetAtPoint( dragContainer, draggedCell, correctedPoint );

            if( returnedDropTarget != null )
              break;

            correctedPoint.X += 1;
          }
        }
        else if( m_horizontalMouseDragDirection == HorizontalMouseDragDirection.Right )
        {
          // When scrolling right, try to find the first IDropTarget from the right edge of the dragged element to the mouse cursor over the dragged element
          double leftSide = draggedElementToDragContainer.X;

          correctedPoint.X += draggedCell.ParentColumn.ActualWidth;

          while( ( returnedDropTarget == null ) && ( correctedPoint.X > leftSide ) )
          {
            returnedDropTarget = ColumnReorderingDragSourceManager.GetDropTargetAtPoint( dragContainer, draggedCell, correctedPoint );

            if( returnedDropTarget != null )
              break;

            correctedPoint.X -= 1;
          }
        }
        else
        {
          returnedDropTarget = ColumnReorderingDragSourceManager.GetDropTargetAtPoint( dragContainer, draggedCell, correctedPoint );
        }

        dropTargetPosition = ( returnedDropTarget != null ) ? correctedPoint : ( Point? )null;

        draggedCell.ClearValue( UIElement.IsHitTestVisibleProperty );
      }
      else
      {
        dropTargetPosition = null;
      }

      //Flag used to reduce the number of column animations, to speed up the scrolling on ColumnManagerCell dragging.
      if( returnedDropTarget == null && this.CurrentDropTarget == null )
      {
        m_noColumnsReorderingNeeded = true;
        if( this.ParentWindowIsPopup )
        {
          m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 0;
        }
      }
      else
      {
        m_noColumnsReorderingNeeded = false;
      }

      return returnedDropTarget;
    }

    protected override void ProcessDragOnCurrentDropTarget( IDropTarget dropTarget )
    {
      base.ProcessDragOnCurrentDropTarget( dropTarget );

      if( !this.IsAnimatedColumnReorderingEnabled )
        return;

      // We are reverting every animation before detaching from the manager
      // so do not update the ghost position
      if( m_ghostToTargetAndDetachAnimationClock != null )
        return;

      ColumnManagerCell cell = this.CurrentDropTarget as ColumnManagerCell;

      bool dragOverCell = ( cell != null );

      // We can drag over other type of IDropTarget than Cell
      if( dragOverCell && this.CurrentDropTargetToDragContainerPosition.HasValue )
      {
        this.ProcessDragOverColumnManagerCell();
      }
    }

    protected override void UpdateDraggedElementGhostOnDrag( MouseEventArgs e )
    {
      //If in TableView and the grid is hosted in a Popup window.
      if( !this.IsAnimatedColumnReorderingEnabled && this.ParentWindowIsPopup )
      {
        this.ShowDraggedElementGhost = false;

        Point draggedElementToMouse = e.GetPosition( this.DraggedElement );
        // Correct it according to initial mouse position over the dragged element
        draggedElementToMouse.X -= this.InitialMousePositionToDraggedElement.GetValueOrDefault().X;
        draggedElementToMouse.Y -= this.InitialMousePositionToDraggedElement.GetValueOrDefault().Y;

        this.ApplyContainerClip( m_popupDraggedElementAdorner );
        m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 1;
        m_popupDraggedElementAdorner.SetOffset( draggedElementToMouse );
      }
      //If no animation, or if the dragged object is beyond the edge of the grid, no need to do anything in this override, simply call base.
      //This will be suficient to update the DraggedElementGhost positon. The result is a much faster scrolling of the grid via the CheckForAutoScroll() method.
      else if( this.IsAnimatedColumnReorderingEnabled && !m_noColumnsReorderingNeeded )
      {
        // We are reverting every animation before detaching from the manager
        // so do not update the ghost position
        if( m_ghostToTargetAndDetachAnimationClock != null )
        {
          return;
        }

        bool dragOverRowOrCell = ( this.CurrentDropTarget as ColumnManagerRow ) != null || ( this.CurrentDropTarget as ColumnManagerCell ) != null;

        // We are dragging over an object that will handle the drop itself
        if( !dragOverRowOrCell && !this.ParentWindowIsPopup )
        {
          // Ensure to pause every other animations before
          this.PauseGhostToMousePositionAnimation();
          this.PauseDraggedElementFadeInAnimation();

          this.RollbackReordering();
          this.ShowDraggedElementGhost = true;

          this.MoveGhostToTargetColumn( e.GetPosition( this.DraggedElement ) );
        }
        //If dragging over an object that will handle the drop itself and the grid is hosted in a Popup window.
        else if( !dragOverRowOrCell && this.ParentWindowIsPopup )
        {
          // Ensure to pause every other animations before
          this.PauseGhostToMousePositionAnimation();
          this.PauseDraggedElementFadeInAnimation();

          this.RollbackReordering();
          this.ShowDraggedElementGhost = false;

          this.MoveGhostToTargetColumn( e.GetPosition( this.DraggedElement ) );

          Point draggedElementToMouse = e.GetPosition( this.DraggedElement );
          // Correct it according to initial mouse position over the dragged element
          draggedElementToMouse.X -= this.InitialMousePositionToDraggedElement.GetValueOrDefault().X;
          draggedElementToMouse.Y -= this.InitialMousePositionToDraggedElement.GetValueOrDefault().Y;

          this.ApplyContainerClip( m_popupDraggedElementAdorner );
          m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 1;
          m_popupDraggedElementAdorner.SetOffset( draggedElementToMouse );
        }
        //If animations are required.
        else
        {
          //If in a popup, hide the dragged element adorner.
          if( this.ParentWindowIsPopup )
          {
            m_popupDraggedElementAdorner.AdornedElementImage.Opacity = 0;
            m_popupDraggedElementAdorner.SetOffset( ColumnReorderingDragSourceManager.EmptyPoint );
          }

          // Pause animations that are moving ghosts to target Column
          this.PauseMoveGhostToTargetColumnAnimation();
          this.PauseDraggedElementFadeInAnimation();

          this.ShowDraggedElementGhost = false;
          this.ShowDraggedColumnGhosts();
          this.HideDraggedElements();
          Point draggedElementToMouse = e.GetPosition( this.DraggedElement );

          // Correct it according to initial mouse position over the dragged element
          draggedElementToMouse.X -= this.InitialMousePositionToDraggedElement.GetValueOrDefault().X;
          draggedElementToMouse.Y -= this.InitialMousePositionToDraggedElement.GetValueOrDefault().Y;

          // Wait for the animation to complete before explicitly setting the 
          // X and Y offsets on the ghost adorners
          // OR if reordering was canceled
          if( ( m_ghostToMousePositionAnimationClock == null )
            && ( m_ghostToTargetColumnAnimationClock == null )
            && !m_reorderCancelled )
          {
            // Update the position of ghosts for each rows
            foreach( UIElement element in m_elementToDraggedElementAdorner.Keys )
            {
              DraggedElementAdorner adorner = m_elementToDraggedElementAdorner[ element ];
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

      // Ensure to call base after to correctly update the DraggedElementGhost of the base class correctly
      base.UpdateDraggedElementGhostOnDrag( e );
    }

    protected override void NotifyDropOuside()
    {
      base.NotifyDropOuside();

      if( !this.IsAnimatedColumnReorderingEnabled )
        return;

      this.RollbackReordering();
    }

    protected override void BeginDrag()
    {
      // Create the DraggedElementGhost in base class
      base.BeginDrag();

      if( !this.IsAnimatedColumnReorderingEnabled )
        return;

      Cell draggedCell = this.DraggedElement as Cell;

      if( draggedCell != null )
      {
        // Affect the IsBeingDraggedAnimated in the ParentColumn to ensure every prepared container calls AddDraggedColumnGhost to this manager
        TableflowView.SetIsBeingDraggedAnimated( draggedCell.ParentColumn, true );
      }

      this.HideDraggedElements();
    }

    internal void CommitReordering()
    {
      ReorderingInfoManager reorderingInfoManager = this.ReorderingInfoManagerInstance;
      RequiredAnimationsInfo animationState = reorderingInfoManager.RequiredAnimations;

      // Commiting the reordering will set the new VisiblePosition
      // of the dragged Column. We need to preserve the old values
      // before commiting to correctly commit the new fixed column 
      // count
      this.CommitColumnReordering();

      this.CommitSplitterReordering( reorderingInfoManager.InitialColumnVisiblePosition,
        animationState.DraggedColumnNewVisiblePosition,
        reorderingInfoManager.InitialFixedColumnCount );
    }

    private void SetPopupDragAdorner( ColumnManagerCell columnManagerCell )
    {
      if( columnManagerCell == null )
        return;

      if( m_popupDraggedElementAdorner != null )
      {
        this.AdornerLayerInsideDragContainer.Remove( m_popupDraggedElementAdorner );
        m_popupDraggedElementAdorner = null;
      }

      // Get the Rect for the DataGridControl
      DataGridControl dataGridControl = this.DraggedDataGridContext.DataGridControl;

      Rect dataGridControlRect = new Rect( 0, 0, dataGridControl.ActualWidth, dataGridControl.ActualHeight );

      Point elementToDataGridControl = columnManagerCell.TranslatePoint( ColumnReorderingDragSourceManager.EmptyPoint, dataGridControl );

      // Get the Rect for the element that request a ghost
      Rect elementRect = new Rect( elementToDataGridControl, columnManagerCell.RenderSize );

      // This is a special case with the current Element that is always be layouted, but can be out of view
      if( !elementRect.IntersectsWith( dataGridControlRect ) )
        return;

      AnimatedDraggedElementAdorner adorner = new AnimatedDraggedElementAdorner( columnManagerCell, this.AdornerLayerInsideDragContainer, true );

      adorner.AdornedElementImage.Opacity = 0;

      this.ApplyContainerClip( adorner );

      this.AdornerLayerInsideDragContainer.Add( adorner );

      m_popupDraggedElementAdorner = adorner;
    }

    private static IDropTarget GetDropTargetAtPoint( UIElement dragContainer, UIElement draggedElement, Point point )
    {
      IInputElement hitTest = dragContainer.InputHitTest( point );

      if( hitTest == null )
        return null;

      DependencyObject parent = hitTest as DependencyObject;

      while( parent != null )
      {
        IDropTarget dropTarget = parent as IDropTarget;

        ColumnManagerCell cell = dropTarget as ColumnManagerCell;
        bool isCellHitTestible = true;

        if( cell != null )
        {
          // The Cell could be partially or completely under the fixed region
          // of the FixedCellPanel, so not "really" HitTestible
          isCellHitTestible = ColumnReorderingDragSourceManager.TryHitTestCell( cell, point, dragContainer );
        }

        if( ( isCellHitTestible )
            && ( dropTarget != null )
            && ( dropTarget.CanDropElement( draggedElement ) ) )
        {
          return dropTarget;
        }

        dropTarget = null;
        parent = Xceed.Utils.Wpf.TreeHelper.GetParent( parent );
      }

      return null;
    }

    private static bool TryHitTestCell( ColumnManagerCell cell, Point point, UIElement dragContainer )
    {
      Row parentRow = cell.ParentRow;

      if( parentRow == null )
        return true;

      FixedCellPanel fixedCellPanel = parentRow.CellsHostPanel as FixedCellPanel;

      if( fixedCellPanel == null )
        return true;

      Decorator scrollingCellsDecorator = fixedCellPanel.ScrollingCellsDecorator;

      if( !scrollingCellsDecorator.IsAncestorOf( cell ) )
        return true;

      Point cellPoint = cell.TranslatePoint( ColumnReorderingDragSourceManager.EmptyPoint, dragContainer );

      Rect cellRect = new Rect( cellPoint.X,
                                cellPoint.Y,
                                cell.ActualWidth,
                                cell.ActualHeight );

      Point scrollingCellsDecoratorPoint = scrollingCellsDecorator.TranslatePoint(
        ColumnReorderingDragSourceManager.EmptyPoint,
        dragContainer );

      Rect scrollingCellsDecoratorRect = new Rect( scrollingCellsDecoratorPoint.X,
                                                   scrollingCellsDecoratorPoint.Y,
                                                   scrollingCellsDecorator.ActualWidth,
                                                   scrollingCellsDecorator.ActualHeight );

      if( !cellRect.IntersectsWith( scrollingCellsDecoratorRect ) )
      {
        return false;
      }
      else if( !scrollingCellsDecoratorRect.Contains( point ) )
      {
        return false;
      }

      return true;
    }

    private static void ClearTranslateTransformAnimation( TranslateTransform transform )
    {
      if( transform == null )
        return;

      // Force a local value before removing the potential animation clock
      // of this transform to avoid leaving the current animated value after
      // animation is removed
      transform.X = 0;
      transform.ApplyAnimationClock( TranslateTransform.XProperty, null );
    }

    // Gets the index of a Column present in the 
    // DataGridContext.ColumnsByVisiblePosition from its 
    // index in the DataGridContext.VisibleColumns
    private static int GetColumnByVisiblePositionIndexFromVisibleColumnIndex( int visibleColumnIndex, List<ColumnBase> columnsByVisiblePosition )
    {
      if( columnsByVisiblePosition == null )
        return -1;

      if( visibleColumnIndex == -1 )
        return -1;

      int columnsByVisiblePositionCount = columnsByVisiblePosition.Count;

      int visibleColumnIndexCache = visibleColumnIndex;
      int columnByVisiblePositionIndex = -1;

      for( int i = 0; i < columnsByVisiblePositionCount; i++ )
      {
        if( visibleColumnIndexCache == 0 )
          break;

        columnByVisiblePositionIndex++;

        ColumnBase columnBase = columnsByVisiblePosition[ i ];

        if( ( columnBase != null ) && ( !columnBase.Visible ) )
          continue;

        visibleColumnIndexCache--;
      }

      return columnByVisiblePositionIndex;
    }

    // Ensure to get the current ViewProperties values
    private void InitializeManager()
    {
      UIElement draggedElement = this.DraggedElement;

      if( draggedElement == null )
        return;

      DataGridContext dataGridContext
        = DataGridControl.GetDataGridContext( draggedElement );

      this.DraggedDataGridContext = dataGridContext;

      this.ColumnVirtualizationManager = dataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

      this.ColumnAnimationDuration = 500;
      this.DraggedElementFadeInDuration = 250;
      this.ReturnToOriginalPositionDuration = 250;

      // Get ViewProperties using DataGridContext
      this.FixedColumnSplitterTranslation = TableflowView.GetFixedColumnSplitterTranslation( dataGridContext );

      // Ensure to pause any animation that was applied to ghost adorners.
      this.PauseGhostToMousePositionAnimation();

      this.PauseMoveGhostToTargetColumnAnimation();

      this.PauseMoveGhostToTargetAndDetachAnimation();

      m_reorderCancelled = false;

      this.ShowDraggedElementGhost = true;

      this.ReorderingInfoManagerInstance.RollbackReorderedColumnsAndFixedColumnCount();

      this.RemoveColumnsPreviousAnimations( dataGridContext.VisibleColumns );
      this.RemoveFixedColumnSplitterPreviousAnimation( dataGridContext );
    }

    private void UpdateMouseMoveDirection( MouseEventArgs e )
    {
      UIElement draggedElement = this.DraggedElement;

      Point draggedElementToMouse = e.GetPosition( draggedElement );

      double currentDraggedElementOffset = draggedElement.TranslatePoint( draggedElementToMouse, this.DragContainer ).X;

      // Verify if there is an horizontal change according to the lastchange      
      Rect dragRect = new Rect(
        m_lastDraggedElementOffset - SystemParameters.MinimumHorizontalDragDistance,
        0,
        SystemParameters.MinimumVerticalDragDistance * 2,
        0 );

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
      bool isAnimatedColumnReorderingEnabled = false;

      if( this.DraggedDataGridContext != null )
      {
        TableflowView tableflowView = this.DraggedDataGridContext.DataGridControl.GetView() as TableflowView;

        if( ( tableflowView != null ) && ( tableflowView.IsAnimatedColumnReorderingEnabled ) )
        {
          isAnimatedColumnReorderingEnabled = true;
        }
      }

      Cell draggedCell = this.DraggedElement as Cell;

      // We must ensure the draggedCell's ParentRow allows the Column Reordering
      if( draggedCell != null )
      {
        // We don't want any ghost to be displayed when Column Reordering
        // is not allowed by the ParentRow. Setting the IsAnimatedColumnReorderingEnabled
        // property will prevent this and will display the Cursor.No when dragging.
        ColumnManagerRow parentRow = draggedCell.ParentRow as ColumnManagerRow;

        if( parentRow != null )
        {
          isAnimatedColumnReorderingEnabled &= parentRow.AllowColumnReorder;
        }
      }

      this.IsAnimatedColumnReorderingEnabled = isAnimatedColumnReorderingEnabled;
    }

    private void MoveGhostToMousePosition( Point mousePosition )
    {
      // Ensure the opacity of the ghost is correctly set
      this.ShowDraggedColumnGhosts();

      Point currentAdornerPosition = mousePosition;

      if( m_elementToDraggedElementAdorner.Count == 0 )
        return;

      int remainingDuration = this.ReturnToOriginalPositionDuration;

      if( m_ghostToMousePositionAnimationClock != null )
      {
        PointAnimation pointAnimation = m_ghostToMousePositionAnimationClock.Timeline as PointAnimation;

        Point currentValue = pointAnimation.To.GetValueOrDefault();

        double deltaX = currentValue.X - mousePosition.X;
        double deltaY = currentValue.Y - mousePosition.Y;

        bool dragingX = Math.Abs( deltaX ) > SystemParameters.MinimumHorizontalDragDistance;
        bool dragingY = Math.Abs( deltaY ) > SystemParameters.MinimumVerticalDragDistance;

        // If the target value is already the correct one, no need to stop animation and create another one
        if( ( pointAnimation != null ) && ( !dragingX && !dragingY ) )
        {
          return;
        }
        else
        {
          if( m_ghostToMousePositionAnimationClock.CurrentState == ClockState.Active )
          {
            // The remaining duration is the Timeline Duration less the elapsed time
            remainingDuration = pointAnimation.Duration.TimeSpan.Milliseconds - m_ghostToMousePositionAnimationClock.CurrentTime.Value.Milliseconds;
          }
        }
      }

      this.PauseGhostToMousePositionAnimation();

      PointAnimation animation = new PointAnimation( currentAdornerPosition, mousePosition, new Duration( TimeSpan.FromMilliseconds( remainingDuration ) ) );

      m_ghostToMousePositionAnimationClock = animation.CreateClock( true ) as AnimationClock;
      m_ghostToMousePositionAnimationClock.Completed += this.GhostToMousePosition_Completed;

      foreach( UIElement element in m_elementToDraggedElementAdorner.Keys )
      {
        DraggedElementAdorner adorner = m_elementToDraggedElementAdorner[ element ];

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

      // Ghosts are returned to their original position
      // reordering was successfully cancelled
      m_reorderCancelled = false;
    }

    private void PauseGhostToMousePositionAnimation()
    {
      if( m_ghostToMousePositionAnimationClock == null )
        return;

      // Stop the animation, do not simply pause it, so it resets correctly.
      m_ghostToMousePositionAnimationClock.Controller.Stop();
      m_ghostToMousePositionAnimationClock.Completed -= this.GhostToMousePosition_Completed;
      m_ghostToMousePositionAnimationClock = null;
    }

    private void MoveGhostToTargetColumn( Point mousePostion )
    {
      if( m_elementToDraggedElementAdorner.Count == 0 )
        return;

      int remainingDuration = this.ReturnToOriginalPositionDuration;

      if( m_ghostToTargetColumnAnimationClock != null )
      {
        PointAnimation pointAnimation = m_ghostToTargetColumnAnimationClock.Timeline as PointAnimation;

        // If the target value is already the correct one, no need to stop animation and create another one
        if( ( pointAnimation != null ) && ( pointAnimation.To == ColumnReorderingDragSourceManager.EmptyPoint ) )
        {
          return;
        }
        else
        {
          if( m_ghostToTargetColumnAnimationClock.CurrentState == ClockState.Active )
          {
            // The remaining duration is the Timeline Duration less the elapsed time
            remainingDuration = pointAnimation.Duration.TimeSpan.Milliseconds - m_ghostToTargetColumnAnimationClock.CurrentTime.Value.Milliseconds;
          }
        }
      }

      // We must apply the DraggedCell FadeIn animation to let the DraggedCell reappears while the ghosts are moving to their target position
      this.ApplyDraggedElementFadeInAnimation();

      this.PauseMoveGhostToTargetColumnAnimation();

      PointAnimation animation = new PointAnimation( mousePostion, ColumnReorderingDragSourceManager.EmptyPoint, new Duration( TimeSpan.FromMilliseconds( remainingDuration ) ) );

      m_ghostToTargetColumnAnimationClock = animation.CreateClock( true ) as AnimationClock;
      m_ghostToTargetColumnAnimationClock.Completed += this.GhostToTargetAnimation_Completed;

      foreach( UIElement element in m_elementToDraggedElementAdorner.Keys )
      {
        DraggedElementAdorner adorner = m_elementToDraggedElementAdorner[ element ];

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
        // The ghosts were successfully moved to the target position
        // so we hide the ghosts and display the Cells in order
        // for the DraggedElementGhost, which is a VisulalBrush of
        // the DraggedElement that is transparent during the drag
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
      m_ghostToTargetColumnAnimationClock.Completed -= this.GhostToTargetAnimation_Completed;
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

      Cell draggedCell = this.DraggedElement as Cell;

      if( draggedCell == null )
        return;

      bool animationInitialized = false;

      DraggedElementAdorner draggedAdorner = m_elementToDraggedElementAdorner[ draggedCell ];

      Nullable<Point> currentDraggedAdornerPosition = null;

      if( draggedAdorner != null )
      {
        currentDraggedAdornerPosition = draggedAdorner.Offset;
      }

      foreach( UIElement element in m_elementToDraggedElementAdorner.Keys )
      {
        Cell cell = element as Cell;

        if( cell == null )
        {
          Debug.Assert( false, "Only Cells should be dragged by this manager" );
          continue;
        }

        DraggedElementAdorner adorner = m_elementToDraggedElementAdorner[ element ];

        if( !animationInitialized )
        {
          TableViewColumnVirtualizationManager columnVirtualizationManager = this.DraggedDataGridContext.ColumnVirtualizationManager as TableViewColumnVirtualizationManager;

          if( columnVirtualizationManager == null )
            return;

          double offset = columnVirtualizationManager.FieldNameToOffset[ draggedCell.FieldName ];

          if( ( draggedCell.ParentRow == null ) || ( draggedCell.ParentRow.CellsHostPanel == null ) )
            return;

          // Get the position of the Mouse according to the DragContainer
          Point mouseToDragContainer = Mouse.GetPosition( this.DragContainer );

          // Get the position of the Mouse according to the DraggedElementAdorner
          Point adornerToMouse = Mouse.GetPosition( adorner );

          // Get the position of the Cell according to the ParentRow.CellsHostPanel for the dragged Cell
          Panel cellsHost = cell.ParentRow.CellsHostPanel;
          Point newCellInCellsHostToDragContainer = cellsHost.TranslatePoint( new Point( offset, 0 ), this.DragContainer );

          // Get the position of the Cell according to its CellsHost in order to know
          // if it was dragged from the Fixed to the Scrolling Cells.
          Point cellToCellsHost = cell.TranslatePoint( ColumnReorderingDragSourceManager.EmptyPoint, cell.ParentRow.CellsHostPanel );

          double initialX = mouseToDragContainer.X - newCellInCellsHostToDragContainer.X - adornerToMouse.X;

          // The DraggedCell is a FixedCell and it offset is greater than  the FixedColumnWidth
          if( columnVirtualizationManager.FixedFieldNames.Contains( cell.FieldName ) && ( cellToCellsHost.X > columnVirtualizationManager.FixedColumnsWidth ) )
          {
            // In this case, we must take the HorizontalOffset into consideration
            initialX -= this.DraggedDataGridContext.DataGridControl.ScrollViewer.HorizontalOffset;
          }
          else
          {
            // The offset of the Cell according the CellsHost is less than 0 and
            // there is a Compensation offset to ensure all the ScrollingCells are
            // in view even if the HorizontalOffset would have scrolled them
            // outside the ViewPort
            if( ( ( newCellInCellsHostToDragContainer.X - columnVirtualizationManager.FixedColumnsWidth ) < 0 ) && ( columnVirtualizationManager.FirstColumnCompensationOffset > 0 ) )
            {
              initialX -= columnVirtualizationManager.FirstColumnCompensationOffset;
            }
          }

          Point fromPoint = new Point( initialX, currentDraggedAdornerPosition.GetValueOrDefault().Y );

          PointAnimation animation = new PointAnimation( fromPoint, ColumnReorderingDragSourceManager.EmptyPoint, m_columnAnimationDuration );

          m_ghostToTargetAndDetachAnimationClock = animation.CreateClock( true ) as AnimationClock;
          m_ghostToTargetAndDetachAnimationClock.Completed += this.MoveGhostToTargetAndDetach_Completed;

          animationInitialized = true;
        }

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
      m_ghostToTargetAndDetachAnimationClock.Completed -= this.MoveGhostToTargetAndDetach_Completed;
      m_ghostToTargetAndDetachAnimationClock = null;
    }

    private void DetachManager()
    {
      this.ShowDraggedElements();

      Cell draggedCell = this.DraggedElement as Cell;

      if( draggedCell != null )
      {
        TableflowView.SetIsBeingDraggedAnimated( draggedCell.ParentColumn, false );
      }

      // Get the manager currently in use on the DataGridContext
      ColumnReorderingDragSourceManager currentManager = TableflowView.GetColumnReorderingDragSourceManager( this.DraggedDataGridContext );

      // Clear properties on the manager if it is the one currently in
      // use on for this Detail level. This avoids problem with the FixedColumnSplitter
      // when multiple Cells are moved rapidly:
      // The FixedColumnSplitter listens to TableflowView.AreColumnsBeingReordered internal 
      // ViewProperty to bind to the TableflowView.FixedColumnSplitterTranslation internal 
      // ViewProperty. The internal ViewProeprty ColumnReorderingDragSourceManager is
      // affected when a new Drag begins. The old DragSourceManager is not stopped to allow 
      // any pending animations to complete smoothly. If the first manager clears the
      // TableflowView.AreColumnBeingReordered property, the FixedColumnSplitters
      // just clears its binding to the FixedColumnSplitterTranslation and is no more animated.
      //
      // Ensuring that the current manager for this Detail level ensure that no other
      // drag was initialized since this manager started animations.

      if( currentManager == this )
      {
        TableflowView.SetAreColumnsBeingReordered( this.DraggedDataGridContext, false );

        if( draggedCell != null )
        {
          TableflowView.ClearColumnReorderingDragSourceManager( draggedCell.ParentColumn );
        }

        TableflowView.ClearColumnReorderingDragSourceManager( this.DraggedDataGridContext );
      }
    }

    private void CommitColumnReordering()
    {
      ReorderingInfoManager reorderingInfoManager = this.ReorderingInfoManagerInstance;
      RequiredAnimationsInfo animationState = reorderingInfoManager.RequiredAnimations;

      int newVisiblePosition = ( animationState != null ) ? animationState.DraggedColumnNewVisiblePosition : -1;

      // Affect the new VisiblePosition to the DraggedColumn to allow the Grid to reflect the change
      if( newVisiblePosition > -1 )
      {
        // There is a ReorderVisiblePosition assigned only when a ColumnReordering is processed, not when a FixedColumnSplitter is moved
        ColumnManagerCell draggedCell = this.DraggedElement as ColumnManagerCell;
        Debug.Assert( draggedCell != null );
        if( draggedCell != null )
        {
          ColumnBase draggedColumn = draggedCell.ParentColumn;

          // Affect the new VisiblePosition of the DraggedColumn
          draggedColumn.VisiblePosition = newVisiblePosition;
        }
      }

      DataGridContext dataGridContext = this.DraggedDataGridContext;
      ReadOnlyObservableCollection<ColumnBase> visibleColumns = dataGridContext.VisibleColumns;

      for( int i = 0; i < visibleColumns.Count; i++ )
      {
        ColumnBase column = visibleColumns[ i ];

        TranslateTransform transform = ColumnReorderingDragSourceManager.GetAnimatedColumnReorderingTranslation( column );

        AnimationClock animationClock = null;

        // Get the AnimationClock for this Column
        if( m_fieldNameToClock.TryGetValue( column.FieldName, out animationClock ) )
        {
          m_fieldNameToClock.Remove( column.FieldName );
          m_clockToFieldName.Remove( animationClock );
        }

        if( animationClock != null )
        {
          animationClock.Controller.Remove();
        }

        if( transform != null )
        {
          transform.X = 0;
        }
      }
    }

    private void CommitSplitterReordering( int oldVisiblePosition, int newVisiblePosition, int oldFixedColumnCount )
    {
      if( m_splitterAnimationClock == null )
        return;

      double fixedColumnTranslation = this.FixedColumnSplitterTranslation.X;

      // Try to get the TargetOffset of the FixedColumnSplitter
      // AnimationClock if available to ensure to commit the
      // correct fixed column count
      if( m_splitterAnimationClock != null )
      {
        OffsetAnimation animation = m_splitterAnimationClock.Timeline as OffsetAnimation;

        if( animation != null )
        {
          if( ( animation.To != null ) && ( animation.To.HasValue ) )
          {
            fixedColumnTranslation = animation.To.Value;
          }
        }
      }

      if( fixedColumnTranslation != 0 )
      {
        ReorderingInfoManager reorderingInfoManager = this.ReorderingInfoManagerInstance;
        int fixedColumnCount = reorderingInfoManager.RequiredAnimations.NewFixedColumnCount;

        // Update the FixedColumnCount on the DataGridContext in order to 
        // correctly represent the value modified during animated reordering
        DataGridContext dataGridContext = this.DraggedDataGridContext;

        if( ( dataGridContext != null ) && ( dataGridContext.ParentDataGridContext != null ) )
        {
          foreach( DataGridContext childDataGridContext in dataGridContext.ParentDataGridContext.GetChildContexts() )
          {
            TableView.SetFixedColumnCount( childDataGridContext, fixedColumnCount );
          }
        }
        else
        {
          TableView.SetFixedColumnCount( dataGridContext, fixedColumnCount );
        }
      }

      m_splitterAnimationClock.Controller.Remove();

      // Ensure 0 to move to original position
      this.FixedColumnSplitterTranslation.X = 0;
    }

    private void ComputeReorderingParameters()
    {
      ColumnManagerCell draggedOverCell = this.CurrentDropTarget as ColumnManagerCell;
      Nullable<Point> mousePosition = this.CurrentDropTargetToDragContainerPosition;
      ColumnManagerCell draggedCell = this.DraggedElement as ColumnManagerCell;
      Debug.Assert( draggedCell != null );

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( draggedOverCell );
      Debug.Assert( dataGridContext != null );

      Point draggedOverCellToDragContainer = draggedOverCell.TranslatePoint( ColumnReorderingDragSourceManager.EmptyPoint, this.DragContainer );

      Rect hitTestRect = new Rect( draggedOverCellToDragContainer.X,
        draggedOverCellToDragContainer.Y,
        draggedOverCell.RenderSize.Width / 2,
        draggedOverCell.RenderSize.Height );

      int initialVisiblePosition = draggedCell.ParentColumn.VisiblePosition;
      int initialFixedColumnCount = this.ColumnVirtualizationManager.FixedColumnCount;
      int newFixedColumnCount = initialFixedColumnCount;
      int newTargetVisiblePosition = -1;

      DraggedOverCellRegion draggedOverCellRegion = DraggedOverCellRegion.Unknown;

      int draggedOverCellVisiblePosition = draggedOverCell.ParentColumn.VisiblePosition;

      Dictionary<string, double> fieldNameToOffset = this.ColumnVirtualizationManager.FieldNameToOffset;

      if( mousePosition.HasValue && ( mousePosition.Value.X < ( hitTestRect.X + hitTestRect.Width ) ) )
      {
        draggedOverCellRegion = DraggedOverCellRegion.Left;

        newTargetVisiblePosition = ( initialVisiblePosition >= draggedOverCellVisiblePosition ) ? draggedOverCellVisiblePosition : draggedOverCellVisiblePosition - 1;
      }
      else
      {
        draggedOverCellRegion = DraggedOverCellRegion.Right;

        newTargetVisiblePosition = ( initialVisiblePosition <= draggedOverCellVisiblePosition ) ? draggedOverCellVisiblePosition : draggedOverCellVisiblePosition + 1;
      }

      ReadOnlyObservableCollection<ColumnBase> visibleColumn = this.DraggedDataGridContext.VisibleColumns;

      int lastFixedColumnVisiblePosition = visibleColumn[ Math.Max( 0, Math.Min( initialFixedColumnCount, visibleColumn.Count ) - 1 ) ].VisiblePosition;

      int firstScrollingColumnVisiblePosition = visibleColumn[ Math.Min( initialFixedColumnCount, visibleColumn.Count - 1 ) ].VisiblePosition;

      // Ensure we have fixed and/or scrolling columns
      if( lastFixedColumnVisiblePosition != firstScrollingColumnVisiblePosition )
      {
        // Initially at the left of the splitter
        if( initialVisiblePosition <= lastFixedColumnVisiblePosition )
        {
          // Moving at the right of the splitter or over the left edged of the first visible scrolling Column
          bool movingAtRightOfSplitter = ( newTargetVisiblePosition >= lastFixedColumnVisiblePosition ) && ( newTargetVisiblePosition >= firstScrollingColumnVisiblePosition );

          bool movingOverFirstVisibleColumnLeftEdge = ( newTargetVisiblePosition == lastFixedColumnVisiblePosition ) && ( draggedOverCellRegion == DraggedOverCellRegion.Left );

          if( movingAtRightOfSplitter || movingOverFirstVisibleColumnLeftEdge )
          {
            newFixedColumnCount--;
          }
        }
        // Initially at the right of the splitter
        else if( initialVisiblePosition >= firstScrollingColumnVisiblePosition )
        {
          // Moving at the left of the splitter or over right edge of the last fixed Column
          bool movingAtLeftOfSpliter = ( newTargetVisiblePosition < firstScrollingColumnVisiblePosition );

          bool movingOverLastFixedColumnRightEdged = ( newTargetVisiblePosition == lastFixedColumnVisiblePosition ) && ( draggedOverCellRegion == DraggedOverCellRegion.Right );

          if( movingAtLeftOfSpliter || movingOverLastFixedColumnRightEdged )
          {
            newFixedColumnCount++;
          }
        }
      }

      // Update the VisiblePositions, DraggedOverCellRegion and FixedColumnCount of the ReorderingInfoManager
      ReorderingInfoManager reorderingInfoManager = this.ReorderingInfoManagerInstance;

      reorderingInfoManager.InitalizeInfoManager( newFixedColumnCount );
      reorderingInfoManager.UpdateDraggedColumnVisiblePosition( newTargetVisiblePosition );
      reorderingInfoManager.UpdateReorderingInfo( this.ColumnVirtualizationManager );
    }

    private void ProcessDragOverColumnManagerCell()
    {
      ColumnManagerCell draggedCell = this.DraggedElement as ColumnManagerCell;

      if( draggedCell == null )
        return;

      IDropTarget currentDropTarget = this.CurrentDropTarget;

      if( currentDropTarget == null )
        return;

      if( currentDropTarget == draggedCell )
        return;

      DataGridContext draggedOverDataGridContext = DataGridControl.GetDataGridContext( currentDropTarget as DependencyObject );

      if( ( draggedOverDataGridContext == null ) || ( this.DraggedDataGridContext != draggedOverDataGridContext ) )
        return;

      this.ComputeReorderingParameters();

      this.ApplyColumnReorderingAnimations();
    }

    private void ApplyColumnReorderingAnimations()
    {
      Cell draggedCell = this.DraggedElement as Cell;

      if( draggedCell == null )
        return;

      ReorderingInfoManager reorderingInfoManager = this.ReorderingInfoManagerInstance;
      RequiredAnimationsInfo animationState = reorderingInfoManager.RequiredAnimations;

      double draggedColumnWidth = draggedCell.ParentColumn.ActualWidth;

      foreach( ColumnBase column in animationState.AnimateToLeft )
      {
        this.DoColumnAnimation( column, -draggedColumnWidth, m_columnAnimationDuration, false );
      }

      foreach( ColumnBase column in animationState.AnimateToRight )
      {
        this.DoColumnAnimation( column, draggedColumnWidth, m_columnAnimationDuration, false );
      }

      foreach( ColumnBase column in animationState.RollbackAnimation )
      {
        this.RollbackColumnAnimation( column );
      }

      this.SetSplitterAnimation( reorderingInfoManager.InitialFixedColumnCount, animationState.NewFixedColumnCount, draggedColumnWidth );
    }

    private AnimationClock AnimateColumn( ColumnBase column, double offset, Duration animationDuration )
    {
      return this.DoColumnAnimation( column, offset, animationDuration, false );
    }

    private void RollbackReordering()
    {
      this.ReorderingInfoManagerInstance.RollbackReorderedColumnsAndFixedColumnCount();

      foreach( ColumnBase column in this.DraggedDataGridContext.VisibleColumns )
      {
        this.RollbackColumnAnimation( column );
      }

      this.DoSplitterAnimation( 0, true );

      m_reorderCancelled = true;
    }

    private void RollbackColumnAnimation( ColumnBase column )
    {
      this.DoColumnAnimation( column, 0, m_columnAnimationDuration, true );
    }

    private AnimationClock DoColumnAnimation( ColumnBase column, double toValue, Duration duration, bool isReverting )
    {
      if( column == null )
        return null;

      Cell draggedCell = this.DraggedElement as Cell;

      // Never animate the dragged Column to avoid flickering
      // effects on the ghosts because we compute its position
      // on each MouseMove event
      if( ( draggedCell != null ) && ( column == draggedCell.ParentColumn ) )
        return null;

      TranslateTransform transform = ColumnReorderingDragSourceManager.GetAnimatedColumnReorderingTranslation( column );

      if( transform == null )
      {
        transform = new TranslateTransform();
        ColumnReorderingDragSourceManager.SetAnimatedColumnReorderingTranslation( column, transform );
      }

      string fieldName = column.FieldName;

      AnimationClock animationClock = null;

      // Pause previously applied AnimationClock
      if( m_fieldNameToClock.TryGetValue( fieldName, out animationClock ) )
      {
        OffsetAnimation clockOffsetAnimation = animationClock.Timeline as OffsetAnimation;

        // If the target value is already the correct one, no need to stop animation and create another one
        if( ( clockOffsetAnimation != null ) && ( clockOffsetAnimation.To == toValue ) )
          return animationClock;

        // Stop the animation, do not simply pause it, so it resets correctly.
        animationClock.Controller.Stop();
        animationClock.Completed -= this.ColumnAnimationCompleted;

        m_fieldNameToClock.Remove( fieldName );
        m_clockToFieldName.Remove( animationClock );
      }

      OffsetAnimation animation = new OffsetAnimation( toValue, duration );
      animationClock = animation.CreateClock( true ) as AnimationClock;

      transform.ApplyAnimationClock( TranslateTransform.XProperty, animationClock, HandoffBehavior.SnapshotAndReplace );

      m_fieldNameToClock.Add( fieldName, animationClock );
      m_clockToFieldName.Add( animationClock, fieldName );

      if( isReverting )
      {
        animationClock.Completed += this.ColumnAnimationCompleted;
      }

      animationClock.Controller.Begin();

      return animationClock;
    }

    private void ColumnAnimationCompleted( object sender, EventArgs e )
    {
      AnimationClock animationClock = sender as AnimationClock;

      if( animationClock == null )
        return;

      // Stop the animation, do not simply pause it, so it resets correctly.
      animationClock.Controller.Stop();
    }

    private void CleanUpColumnAnimations()
    {
      foreach( KeyValuePair<string, AnimationClock> fieldNameToClock in m_fieldNameToClock )
      {
        // Using Columns instead of Visible Columns for performance reasons
        ColumnBase column = this.DraggedDataGridContext.Columns[ fieldNameToClock.Key ];

        Debug.Assert( column != null );

        //Remove the animation translate form the column.
        TranslateTransform transform = ColumnReorderingDragSourceManager.GetAnimatedColumnReorderingTranslation( column );
        Debug.Assert( transform != null );
        transform.X = 0;

        //Clear the animation
        fieldNameToClock.Value.Controller.Remove();
      }

      m_fieldNameToClock.Clear();
      m_clockToFieldName.Clear();
    }

    private void RemoveColumnsPreviousAnimations( ReadOnlyObservableCollection<ColumnBase> readOnlyObservableCollection )
    {
      foreach( ColumnBase column in readOnlyObservableCollection )
      {
        TranslateTransform transform = ColumnReorderingDragSourceManager.GetAnimatedColumnReorderingTranslation( column );

        ColumnReorderingDragSourceManager.ClearTranslateTransformAnimation( transform );
      }
    }

    private void SetSplitterAnimation( int oldfixedColumnCount, int newFixedColumnCount, double offset )
    {
      if( oldfixedColumnCount < newFixedColumnCount )
      {
        this.DoSplitterAnimation( offset, false );
      }
      else if( oldfixedColumnCount > newFixedColumnCount )
      {
        this.DoSplitterAnimation( -offset, false );
      }
      else
      {
        this.DoSplitterAnimation( 0, true );
      }
    }

    private void RollbackSplitterAnimationCompleted( object sender, EventArgs e )
    {
      if( m_splitterAnimationClock != null )
      {
        m_splitterAnimationClock.Controller.Remove();
      }

      if( this.FixedColumnSplitterTranslation != null )
      {
        this.FixedColumnSplitterTranslation.X = 0;
      }
    }

    private void DoSplitterAnimation( double offset, bool isReverting )
    {
      if( m_splitterAnimationClock != null )
      {
        OffsetAnimation clockOffsetAnimation = m_splitterAnimationClock.Timeline as OffsetAnimation;

        // If the target value is already the correct one, no need to stop animation and create another one
        if( ( clockOffsetAnimation != null ) && ( clockOffsetAnimation.To == offset ) )
          return;
      }

      this.PauseSplitterAnimation();

      OffsetAnimation animation = new OffsetAnimation( offset, m_columnAnimationDuration );
      m_splitterAnimationClock = ( AnimationClock )animation.CreateClock( true );

      this.FixedColumnSplitterTranslation.ApplyAnimationClock( TranslateTransform.XProperty, m_splitterAnimationClock, HandoffBehavior.SnapshotAndReplace );

      if( isReverting )
      {
        m_splitterAnimationClock.Completed += this.RollbackSplitterAnimationCompleted;
      }

      m_splitterAnimationClock.Controller.Begin();
    }

    private void PauseSplitterAnimation()
    {
      if( m_splitterAnimationClock == null )
        return;

      // Stop the animation, do not simply pause it, so it resets correctly.
      m_splitterAnimationClock.Controller.Stop();
      m_splitterAnimationClock.Completed -= this.RollbackSplitterAnimationCompleted;
    }

    private void RemoveFixedColumnSplitterPreviousAnimation( DataGridContext dataGridContext )
    {
      if( dataGridContext == null )
        return;

      TranslateTransform transform = TableflowView.GetFixedColumnSplitterTranslation( dataGridContext ) as TranslateTransform;

      ColumnReorderingDragSourceManager.ClearTranslateTransformAnimation( transform );
    }

    private void ApplyContainerClip( DraggedElementAdorner adorner )
    {
      if( adorner == null )
        return;

      UIElement adornedElement = adorner.AdornedElement;

      if( adornedElement == null )
        return;

      // We only want to clip Cells other than the dragged Cell
      if( adornedElement == this.DraggedElement )
        return;

      DataGridContext dataGridContext = DataGridControl.GetDataGridContext( adornedElement );

      if( dataGridContext == null )
        return;

      object dataItem = adornedElement.GetValue( CustomItemContainerGenerator.DataItemPropertyProperty );

      if( dataItem == null )
        return;

      UIElement container = dataGridContext.GetContainerFromItem( dataItem ) as UIElement;

      if( container != null )
      {
        RectangleGeometry containerClip = container.Clip as RectangleGeometry;

        if( containerClip != null )
        {
          // Transform the bounds of the clip region of the container to fit the bounds of the adornedElement
          Rect transformedBounds = container.TransformToDescendant( adornedElement ).TransformBounds( containerClip.Bounds );

          containerClip = new RectangleGeometry( transformedBounds );
        }

        adorner.AdornedElementImage.Clip = containerClip;
      }
      else
      {
        // Clip adorner completely
        adorner.AdornedElementImage.Clip = new RectangleGeometry( Rect.Empty );
      }
    }

    private TableflowViewItemsHost GetTableFlowViewItemsHost()
    {
      if( ( this.DraggedDataGridContext != null )
        && ( this.DraggedDataGridContext.DataGridControl != null )
        && ( this.DraggedDataGridContext.DataGridControl.ItemsHost != null ) )
      {
        return this.DraggedDataGridContext.DataGridControl.ItemsHost as TableflowViewItemsHost;
      }

      return null;
    }

    private Rect GetTableflowViewItemsHostToDragContainerRect( TableflowViewItemsHost itemsHost )
    {
      Rect tableflowItemsHostToDragContainerRect = new Rect( this.DragContainer.RenderSize );

      if( itemsHost != null )
      {
        double stickyHeadersHeight = itemsHost.GetStickyHeadersRegionHeight();
        double stickyFootersHeight = itemsHost.GetStickyHeadersRegionHeight();

        Point itemsHostToDragContainer =
          itemsHost.TranslatePoint( ColumnReorderingDragSourceManager.EmptyPoint, this.DragContainer );

        tableflowItemsHostToDragContainerRect = new Rect(
          itemsHostToDragContainer.X,
          itemsHostToDragContainer.Y + stickyHeadersHeight,
          itemsHost.RenderSize.Width,
          Math.Max( 0, itemsHost.RenderSize.Height - stickyHeadersHeight - stickyFootersHeight ) );
      }

      return tableflowItemsHostToDragContainerRect;
    }

    private Dictionary<DataGridContext, Rect> GetColumnManagerCellGhostRects()
    {
      Dictionary<DataGridContext, Rect> dataGridContextToColumnManagerCellRects =
        new Dictionary<DataGridContext, Rect>();

      foreach( Cell cell in m_elementToDraggedElementAdorner.Keys )
      {
        ColumnManagerCell columnManagerCell = cell as ColumnManagerCell;

        if( columnManagerCell != null )
        {
          Point columnManagerCellToDragContainer =
            columnManagerCell.TranslatePoint( ColumnReorderingDragSourceManager.EmptyPoint, this.DragContainer );

          Rect columnManagerCellRect = new Rect(
            columnManagerCellToDragContainer.X,
            columnManagerCellToDragContainer.Y,
            columnManagerCell.RenderSize.Width,
            columnManagerCell.RenderSize.Height );

          DataGridContext columnManagerCellDataGridContext = DataGridControl.GetDataGridContext( columnManagerCell );

          // We will only consider the first ColumnManagerRow added to this DataGridContext
          if( !dataGridContextToColumnManagerCellRects.ContainsKey( columnManagerCellDataGridContext ) )
          {
            dataGridContextToColumnManagerCellRects.Add( columnManagerCellDataGridContext, columnManagerCellRect );
          }
        }
      }

      return dataGridContextToColumnManagerCellRects;
    }

    private void RemoveAllGhosts()
    {
      DraggedElementAdorner adorner = null;

      this.PauseDraggedElementFadeInAnimation();

      foreach( UIElement element in m_elementToDraggedElementAdorner.Keys )
      {
        adorner = m_elementToDraggedElementAdorner[ element ];
        Debug.Assert( adorner != null );
        this.AdornerLayerInsideDragContainer.Remove( adorner );
        element.Opacity = 1;
      }

      m_elementToDraggedElementAdorner.Clear();

      if( m_popupDraggedElementAdorner != null )
      {
        this.AdornerLayerInsideDragContainer.Remove( m_popupDraggedElementAdorner );
        m_popupDraggedElementAdorner = null;
      }
    }

    private void ShowDraggedColumnGhosts()
    {
      foreach( DraggedElementAdorner adorner in m_elementToDraggedElementAdorner.Values )
      {
        this.ApplyContainerClip( adorner );
        adorner.AdornedElementImage.Opacity = 1;
      }
    }

    private void HideDraggedColumnGhosts()
    {
      foreach( DraggedElementAdorner adorner in m_elementToDraggedElementAdorner.Values )
      {
        adorner.AdornedElementImage.Opacity = 0;
      }
    }

    private void ShowDraggedElements()
    {
      this.PauseDraggedElementFadeInAnimation();

      foreach( UIElement element in m_elementToDraggedElementAdorner.Keys )
      {
        element.Opacity = 1;
      }
    }

    private void HideDraggedElements()
    {
      this.PauseDraggedElementFadeInAnimation();

      foreach( UIElement element in m_elementToDraggedElementAdorner.Keys )
      {
        element.Opacity = 0;
      }
    }

    private void ApplyDraggedElementFadeInAnimation()
    {
      int remainingOpacityDuration = this.DraggedElementFadeInDuration;


      if( m_draggedElementFadeInAnimationClock != null )
      {
        OffsetAnimation fadeInAnimation = m_draggedElementFadeInAnimationClock.Timeline as OffsetAnimation;

        if( ( fadeInAnimation != null ) && ( fadeInAnimation.To == 1 ) )
        {
          return;
        }
        else
        {
          if( m_draggedElementFadeInAnimationClock.CurrentState == ClockState.Active )
          {
            // The remaining duration is the Timeline Duration less the elapsed time
            remainingOpacityDuration = fadeInAnimation.Duration.TimeSpan.Milliseconds - m_draggedElementFadeInAnimationClock.CurrentTime.Value.Milliseconds;
          }
        }
      }

      this.PauseDraggedElementFadeInAnimation();

      UIElement draggedElement = this.DraggedElement;

      // Apply an animation on the opacity of the Cell to ensure a nice transition between
      // the ColumnReordering and another IDropTarget that is not handled by this manager
      if( draggedElement != null )
      {
        OffsetAnimation opacityAnimation = new OffsetAnimation( 1, new Duration( TimeSpan.FromMilliseconds( remainingOpacityDuration ) ) );

        m_draggedElementFadeInAnimationClock = opacityAnimation.CreateClock( true ) as AnimationClock;

        // We ust the same completed callback as MoveGhostToTarget animation to 
        // be sure that the DraggedElement FadeIn is completed and the ghosts are
        // correctly positioned before displaying the original UIElements
        m_draggedElementFadeInAnimationClock.Completed += this.GhostToTargetAnimation_Completed;

        draggedElement.ApplyAnimationClock( UIElement.OpacityProperty, m_draggedElementFadeInAnimationClock );

        m_draggedElementFadeInAnimationClock.Controller.Begin();
      }
    }

    private void PauseDraggedElementFadeInAnimation()
    {
      // Ensure to stop dragged element FadeIn in order
      // to correctly display the DraggedElementGhost
      UIElement draggedElement = this.DraggedElement;

      if( draggedElement != null )
      {
        draggedElement.ApplyAnimationClock( UIElement.OpacityProperty, null );
      }

      if( m_draggedElementFadeInAnimationClock == null )
        return;

      // Stop the animation, do not simply pause it, so it resets correctly.
      m_draggedElementFadeInAnimationClock.Controller.Stop();
      m_draggedElementFadeInAnimationClock.Completed -= this.GhostToTargetAnimation_Completed;
      m_draggedElementFadeInAnimationClock = null;
    }


    private bool m_reorderCancelled; // = false;
    private bool m_noColumnsReorderingNeeded = false;

    private AnimationClock m_draggedElementFadeInAnimationClock; // = null
    private AnimationClock m_ghostToTargetAndDetachAnimationClock; // = null;
    private AnimationClock m_ghostToTargetColumnAnimationClock; // = null;
    private AnimationClock m_ghostToMousePositionAnimationClock; // = null;

    private AnimationClock m_splitterAnimationClock; // = null;

    private Dictionary<string, AnimationClock> m_fieldNameToClock = new Dictionary<string, AnimationClock>();

    private Dictionary<AnimationClock, string> m_clockToFieldName = new Dictionary<AnimationClock, string>();

    private Dictionary<UIElement, DraggedElementAdorner> m_elementToDraggedElementAdorner = new Dictionary<UIElement, DraggedElementAdorner>();
    private DraggedElementAdorner m_popupDraggedElementAdorner;

    private HorizontalMouseDragDirection m_horizontalMouseDragDirection = HorizontalMouseDragDirection.None;

    private double m_lastDraggedElementOffset; // = 0d;

    private int m_columnAnimationDurationCache;
    private Duration m_columnAnimationDuration;

    private static Point EmptyPoint = new Point();

    ReorderingInfoManager m_reorderingInfoManager; // = null;

    internal enum DraggedOverCellRegion
    {
      Unknown,
      Left,
      Right
    }

    private enum HorizontalMouseDragDirection
    {
      None,
      Left,
      Right
    }

    private class ReorderingInfoManager
    {
      public ReorderingInfoManager()
      {
      }

      public int InitialColumnVisiblePosition
      {
        get;
        private set;
      }

      public int InitialFixedColumnCount
      {
        get;
        private set;
      }

      public List<ColumnBase> InitialColumnsByVisiblePosition
      {
        get;
        private set;
      }

      // Contains the ColumnsByVisiblePosition of the DataGridContext in the current reordered order
      public List<ColumnBase> ReorderedColumnsByVisiblePosition
      {
        get;
        private set;
      }

      public RequiredAnimationsInfo RequiredAnimations
      {
        get;
        private set;
      }

      internal void InitalizeInfoManager( int newFixedColumnCount )
      {
        this.RequiredAnimations = new RequiredAnimationsInfo();
        this.RequiredAnimations.NewFixedColumnCount = newFixedColumnCount;
      }

      public void BeginReordering( DataGridContext dataGridContext, int initialColumnVisiblePosition )
      {
        if( dataGridContext == null )
          throw new ArgumentNullException( "dataGridContext" );

        if( m_isProcessingReordering )
        {
          Debug.Assert( false, "Already processing Reordering... ensure to call EndReordering" );
        }

        this.InitialColumnVisiblePosition = initialColumnVisiblePosition;
        this.InitialFixedColumnCount = TableflowView.GetFixedColumnCount( dataGridContext );

        int columnsByVisiblePositionCount = dataGridContext.ColumnsByVisiblePosition.Count;
        ColumnBase[] columnsByVisiblePosition = new ColumnBase[ columnsByVisiblePositionCount ];
        dataGridContext.ColumnsByVisiblePosition.CopyTo( columnsByVisiblePosition, 0 );

        this.InitialColumnsByVisiblePosition = new List<ColumnBase>( columnsByVisiblePosition );

        this.ReorderedColumnsByVisiblePosition = new List<ColumnBase>();

        m_isProcessingReordering = true;
      }

      public void EndReordering()
      {
        m_isProcessingReordering = false;
      }

      public void UpdateDraggedColumnVisiblePosition( int newPosition )
      {
        // Reinitialize the ReorderedColumns List
        this.ReorderedColumnsByVisiblePosition = new List<ColumnBase>( this.InitialColumnsByVisiblePosition );

        int initialColumnVisiblePosition = this.InitialColumnVisiblePosition;
        bool fromLeftToRight = initialColumnVisiblePosition < newPosition;
        bool fromRightToLeft = initialColumnVisiblePosition > newPosition;

        if( !fromLeftToRight && !fromRightToLeft )
          return;

        List<ColumnBase> reorderedColumnsByVisiblePosition = this.ReorderedColumnsByVisiblePosition;
        List<ColumnBase> columnsByVisiblePosition = this.InitialColumnsByVisiblePosition;
        int columnsByVisiblePositionCount = columnsByVisiblePosition.Count;

        ColumnBase reorderedColumn = this.InitialColumnsByVisiblePosition[ initialColumnVisiblePosition ];

        reorderedColumnsByVisiblePosition.Remove( reorderedColumn );

        for( int i = 0; i < columnsByVisiblePositionCount; i++ )
        {
          ColumnBase column = columnsByVisiblePosition[ i ];

          if( !column.Visible )
            continue;

          if( column == reorderedColumn )
            continue;

          int visiblePosition = column.VisiblePosition;

          if( fromLeftToRight )
          {
            // Insert the draggedCellParentColumn at the position of the first visible Column for which the VisiblePosition is greater than the new VisiblePosition
            if( visiblePosition > newPosition )
            {
              if( i <= reorderedColumnsByVisiblePosition.Count )
              {
                // The initial position of the Column in the reordering was after the current insertion point
                if( i > 0 )
                {
                  int insertionIndex = i - 1;

                  // move the reorderedColumn to the first column that doesn't have a hidden column to its left starting at i - 1 and moving backwards. 
                  while( insertionIndex > 0 && reorderedColumnsByVisiblePosition[ insertionIndex - 1 ].Visible == false )
                  {
                    insertionIndex--;
                  }
                  reorderedColumnsByVisiblePosition.Insert( insertionIndex, reorderedColumn );
                }
                else
                {
                  reorderedColumnsByVisiblePosition.Insert( i, reorderedColumn );
                }
              }
              else
              {
                reorderedColumnsByVisiblePosition.Add( reorderedColumn );
              }
              break;
            }
          }
          else if( fromRightToLeft )
          {
            // If a column was already at this position insert it directly at the same position
            if( visiblePosition == newPosition )
            {
              if( i <= reorderedColumnsByVisiblePosition.Count )
              {
                reorderedColumnsByVisiblePosition.Insert( i, reorderedColumn );
              }
              else
              {
                reorderedColumnsByVisiblePosition.Add( reorderedColumn );
              }
              break;
            }
            // Insert the draggedCellParentColumn at the position of the first visible Column for which the VisiblePosition is greater than the new VisiblePosition
            else if( visiblePosition > newPosition )
            {
              if( i <= reorderedColumnsByVisiblePosition.Count )
              {
                // The initial position of the Column in the reordering was after the current insertion point
                if( initialColumnVisiblePosition > i )
                {
                  reorderedColumnsByVisiblePosition.Insert( i, reorderedColumn );
                }
                else
                {
                  reorderedColumnsByVisiblePosition.Insert( i - 1, reorderedColumn );
                }
              }
              else
              {
                reorderedColumnsByVisiblePosition.Add( reorderedColumn );
              }
              break;
            }
          }
        }

        // Ensure to add the parent Column at the end if there are more Columns in the reordering
        if( columnsByVisiblePosition.Count > reorderedColumnsByVisiblePosition.Count )
        {
          reorderedColumnsByVisiblePosition.Add( reorderedColumn );
        }
      }

      public void RollbackReorderedColumnsAndFixedColumnCount()
      {
        this.RequiredAnimations = new RequiredAnimationsInfo( this.InitialColumnVisiblePosition,
          this.InitialFixedColumnCount,
          this.InitialColumnsByVisiblePosition );
      }

      internal void UpdateReorderingInfo( TableViewColumnVirtualizationManager columnVirtualizationManager )
      {
        // Do not create another instance since we want to keep the NewFixedColumnCount
        RequiredAnimationsInfo animationState = this.RequiredAnimations;
        animationState.AnimateToLeft.Clear();
        animationState.AnimateToRight.Clear();
        animationState.RollbackAnimation.Clear();

        List<ColumnBase> columnsByVisiblePosition = this.InitialColumnsByVisiblePosition;
        List<ColumnBase> reorderedColumnsByVisiblePosition = this.ReorderedColumnsByVisiblePosition;

        Debug.Assert( this.InitialColumnVisiblePosition < this.InitialColumnsByVisiblePosition.Count );

        ColumnBase draggedColumn = this.InitialColumnsByVisiblePosition[ this.InitialColumnVisiblePosition ];
        bool draggedColumnReached = false;
        int visibleColumnCount = columnsByVisiblePosition.Count;

        Dictionary<string, object> columnsInView = columnVirtualizationManager.FixedAndScrollingFieldNamesLookupDictionary;

        for( int i = 0; i < visibleColumnCount; i++ )
        {
          ColumnBase originalColumn = columnsByVisiblePosition[ i ];

          ColumnBase reorderedColumn = reorderedColumnsByVisiblePosition[ i ];

          if( reorderedColumn == draggedColumn )
          {
            draggedColumnReached = true;
            continue;
          }

          if( !reorderedColumn.Visible )
            continue;

          if( originalColumn == reorderedColumn )
          {
            animationState.RollbackAnimation.Add( reorderedColumn );
            continue;
          }

          if( columnsInView.ContainsKey( reorderedColumn.FieldName ) )
          {
            if( !draggedColumnReached )
            {
              animationState.AnimateToLeft.Add( reorderedColumn );
            }
            else
            {
              animationState.AnimateToRight.Add( reorderedColumn );
            }
          }
        }

        animationState.DraggedColumnNewVisiblePosition = reorderedColumnsByVisiblePosition.IndexOf( draggedColumn );
      }

      private bool m_isProcessingReordering; // = false;
    }

    private class RequiredAnimationsInfo
    {
      public RequiredAnimationsInfo()
        : this( -1, -1, null )
      {
      }

      public RequiredAnimationsInfo( int draggedColumnNewVisiblePosition, int newFixedColumnCount, IList<ColumnBase> initialColumns )
      {
        this.AnimateToLeft = new List<ColumnBase>();
        this.AnimateToRight = new List<ColumnBase>();
        this.RollbackAnimation = new List<ColumnBase>();

        this.DraggedColumnNewVisiblePosition = draggedColumnNewVisiblePosition;
        this.NewFixedColumnCount = newFixedColumnCount;

        if( initialColumns != null )
        {
          this.RollbackAnimation.AddRange( initialColumns );
        }
      }

      public int DraggedColumnNewVisiblePosition
      {
        get;
        set;
      }

      public int NewFixedColumnCount
      {
        get;
        set;
      }

      public List<ColumnBase> AnimateToLeft
      {
        get;
        private set;
      }

      public List<ColumnBase> AnimateToRight
      {
        get;
        private set;
      }

      public List<ColumnBase> RollbackAnimation
      {
        get;
        private set;
      }
    }
  }
}
