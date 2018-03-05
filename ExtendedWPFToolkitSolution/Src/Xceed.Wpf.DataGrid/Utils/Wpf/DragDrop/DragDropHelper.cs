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
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.DataGrid;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal static class DragDropHelper
  {
    internal static bool IsMouseMoveDrag( Point initialPosition, Point currentPosition )
    {
      var dragRect = new Rect(
        initialPosition.X - SystemParameters.MinimumHorizontalDragDistance,
        initialPosition.Y - SystemParameters.MinimumVerticalDragDistance,
        SystemParameters.MinimumHorizontalDragDistance * 2,
        SystemParameters.MinimumVerticalDragDistance * 2 );

      return !dragRect.Contains( currentPosition );
    }

    internal static IEnumerable<DropTargetInfo> GetDropTargetAtPoint(
      UIElement draggedElement,
      UIElement container,
      MouseEventArgs e )
    {
      return DragDropHelper.GetDropTargetAtPoint( draggedElement, container, e.GetPosition );
    }

    internal static IEnumerable<DropTargetInfo> GetDropTargetAtPoint(
      UIElement draggedElement,
      UIElement container,
      Func<IInputElement, Point> getPosition )
    {
      if( container == null )
        yield break;

      var mousePosition = new RelativePoint( container, getPosition.Invoke( container ) );
      var hitTest = container.InputHitTest( mousePosition.GetPoint( container ) );
      if( hitTest == null )
        yield break;

      var parent = hitTest as DependencyObject;

      while( parent != null )
      {
        var dropTarget = parent as IDropTarget;
        if( dropTarget != null )
        {
          var element = parent as UIElement;
          if( element != null )
          {
            var dropTargetPosition = mousePosition.TranslateTo( element );
            var canDrop = dropTarget.CanDropElement( draggedElement, dropTargetPosition );

            yield return new DropTargetInfo( dropTarget, dropTargetPosition, canDrop );
          }
        }

        parent = Xceed.Utils.Wpf.TreeHelper.GetParent( parent );
      }
    }
  }
}
