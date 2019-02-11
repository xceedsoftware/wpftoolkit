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
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.DataGrid;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal class DragSourceManager : DragSourceManagerBase
  {
    internal DragSourceManager( UIElement draggedElement, AdornerLayer adornerLayer, UIElement container )
      : base( draggedElement, adornerLayer, container )
    {
    }

    #region CurrentDropTarget Protected Property

    protected IDropTarget CurrentDropTarget
    {
      get
      {
        return m_currentDropTarget;
      }
    }

    private void SetCurrentDropTarget( IDropTarget value, RelativePoint mousePosition, bool raiseDragEvents )
    {
      if( value == m_currentDropTarget )
        return;

      var element = this.DraggedElement;

      if( m_currentDropTarget != null )
      {
        if( raiseDragEvents )
        {
          m_currentDropTarget.DragLeave( element );
        }

        m_currentDropTarget = null;
      }

      if( ( value != null ) && value.CanDropElement( element, mousePosition ) )
      {
        m_currentDropTarget = value;

        if( raiseDragEvents )
        {
          m_currentDropTarget.DragEnter( element );
        }
      }

      this.OnPropertyChanged( "CurrentDropTarget" );
    }

    private IDropTarget m_currentDropTarget; //null

    #endregion

    #region CurrentDropTargetToContainerPosition Protected Property

    protected RelativePoint? CurrentDropTargetToContainerPosition
    {
      get
      {
        return m_currentDropTargetToContainerPosition;
      }
      private set
      {
        if( value == m_currentDropTargetToContainerPosition )
          return;

        m_currentDropTargetToContainerPosition = value;

        this.OnPropertyChanged( "CurrentDropTargetToContainerPosition" );
      }
    }

    private RelativePoint? m_currentDropTargetToContainerPosition; //null

    #endregion

    #region DropOutsideCursor Internal Property

    internal Cursor DropOutsideCursor
    {
      get
      {
        return m_dropOutsideCursor;
      }
      set
      {
        if( value == m_dropOutsideCursor )
          return;

        m_dropOutsideCursor = value;

        this.OnPropertyChanged( "DropOutsideCursor" );
      }
    }

    private Cursor m_dropOutsideCursor; //null

    #endregion

    #region DragOutsideQueryCursor Internal Event

    internal event QueryCursorEventHandler DragOutsideQueryCursor;

    private void OnDragOutsideQueryCursor( QueryCursorEventArgs e )
    {
      if( e.Handled )
        return;

      e.Handled = true;

      e.Cursor = m_dropOutsideCursor ?? Cursors.No;

      var handler = this.DragOutsideQueryCursor;
      if( handler != null )
      {
        handler.Invoke( this, e );
      }
    }

    #endregion

    #region DroppedOutside Internal Event

    internal event EventHandler DroppedOutside;

    protected virtual void OnDroppedOutside()
    {
      var handler = this.DroppedOutside;
      if( handler == null )
        return;

      handler.Invoke( this, EventArgs.Empty );
    }

    #endregion

    protected override void OnDragStart( Func<IInputElement, Point> getPosition )
    {
      base.OnDragStart( getPosition );

      var element = this.DraggedElement;
      element.QueryCursor += new QueryCursorEventHandler( this.OnDraggedElementQueryCursor );

      Debug.Assert( element.IsMouseCaptured, "The mouse should have be captured by the dragged element." );
    }

    protected override void OnDragEnd( Func<IInputElement, Point> getPosition, bool drop )
    {
      var element = this.DraggedElement;
      element.QueryCursor -= new QueryCursorEventHandler( this.OnDraggedElementQueryCursor );

      base.OnDragEnd( getPosition, drop );
    }

    protected override void OnDragMove( Func<IInputElement, Point> getPosition )
    {
      base.OnDragMove( getPosition );

      var dropTargetInfo = this.GetDropTarget( getPosition );

      this.CurrentDropTargetToContainerPosition = dropTargetInfo.Position;
      this.SetCurrentDropTarget( dropTargetInfo.Target, dropTargetInfo.Position.GetValueOrDefault(), true );

      var dropTarget = this.CurrentDropTarget;
      if( dropTarget != null )
      {
        this.OnDragOver( dropTarget, getPosition );
      }
    }

    protected override void OnDragCancel( Func<IInputElement, Point> getPosition )
    {
      var dropTarget = this.CurrentDropTarget;
      if( dropTarget != null )
      {
        dropTarget.DragLeave( this.DraggedElement );
        this.SetCurrentDropTarget( null, default( RelativePoint ), false );
      }
      else
      {
        this.OnDroppedOutside();
      }

      base.OnDragCancel( getPosition );
    }

    protected override void OnDrop( Func<IInputElement, Point> getPosition )
    {
      var dropTarget = this.CurrentDropTarget;
      if( dropTarget != null )
      {
        var element = dropTarget as UIElement;
        if( element != null )
        {
          var mousePosition = new RelativePoint( element, getPosition.Invoke( element ) );

          dropTarget.Drop( this.DraggedElement, mousePosition );
        }

        this.SetCurrentDropTarget( null, default( RelativePoint ), false );
      }
      else
      {
        this.OnDroppedOutside();
      }

      base.OnDrop( getPosition );
    }

    protected virtual void OnDragOver( IDropTarget target, Func<IInputElement, Point> getPosition )
    {
      if( target == null )
        throw new ArgumentNullException( "target" );

      if( getPosition == null )
        throw new ArgumentNullException( "getPosition" );

      var element = target as UIElement;
      if( element == null )
        return;

      target.DragOver( this.DraggedElement, new RelativePoint( element, getPosition.Invoke( element ) ) );
    }

    protected override void ValidateMouseEventArgs( MouseEventArgs e )
    {
      base.ValidateMouseEventArgs( e );

      var element = this.DraggedElement;
      var source = e.Source as DependencyObject;

      while( source != null )
      {
        if( source == element )
          break;

        source = VisualTreeHelper.GetParent( source );
      }

      Debug.Assert( ( source != null ), "The Source of the " + e.RoutedEvent.Name + " event is NOT the UIElement that was passed to the ctor of this DragSourceManager OR one of its children." );
    }

    protected virtual DropTargetInfo GetDropTarget( Func<IInputElement, Point> getPosition )
    {
      foreach( var info in DragDropHelper.GetDropTargetAtPoint( this.DraggedElement, this.Container, getPosition ) )
      {
        if( !info.CanDrop )
          continue;

        // ColumnManagerRow was defined as IDropTarget only because Animated Column Reordering required it, ignore it in base class
        if( info.Target is ColumnManagerRow )
          break;

        return info;
      }

      return new DropTargetInfo( null, null, false );
    }

    private void OnDraggedElementQueryCursor( object sender, QueryCursorEventArgs e )
    {
      if( !this.IsDragging )
        return;

      var dropTarget = this.CurrentDropTarget;
      if( dropTarget != null )
      {
        var element = dropTarget as UIElement;
        if( element != null )
        {
          var dropPoint = e.GetPosition( element );
          if( dropTarget.CanDropElement( this.DraggedElement, new RelativePoint( element, dropPoint ) ) )
            return;
        }
      }

      this.OnDragOutsideQueryCursor( e );
    }
  }
}
