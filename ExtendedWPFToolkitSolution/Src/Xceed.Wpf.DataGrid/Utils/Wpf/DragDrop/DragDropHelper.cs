/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Xceed.Utils.Wpf.DragDrop
{
  internal class DragDropHelper
  {
    public static bool IsMouseMoveDrag( Point initialPosition, Point currentPosition )
    {
      Rect dragRect = new Rect(
        initialPosition.X - SystemParameters.MinimumHorizontalDragDistance,
        initialPosition.Y - SystemParameters.MinimumVerticalDragDistance,
        SystemParameters.MinimumHorizontalDragDistance * 2,
        SystemParameters.MinimumVerticalDragDistance * 2 );

      return !dragRect.Contains( currentPosition );
    }

    public static IDropTarget GetDropTargetAtPoint(
      UIElement draggedElement,
      UIElement dragContainer,
      MouseEventArgs e,
      out Nullable<Point> dropTargetPosition,
      out IDropTarget lastFoundDropTarget )
    {
      dropTargetPosition = null;
      lastFoundDropTarget = null;

      if( dragContainer == null )
        return null;

      IDropTarget dropTarget = null;

      Point pointToDragContainer = e.GetPosition( dragContainer );

      IInputElement hitTest = dragContainer.InputHitTest( pointToDragContainer );

      if( hitTest != null )
      {
        DependencyObject parent = hitTest as DependencyObject;

        while( parent != null )
        {
          dropTarget = parent as IDropTarget;
          if( dropTarget != null )
          {
            lastFoundDropTarget = dropTarget;

            if( dropTarget.CanDropElement( draggedElement ) )
            {
              dropTargetPosition = pointToDragContainer;
              break;
            }
          }
          dropTarget = null;
          parent = Xceed.Utils.Wpf.TreeHelper.GetParent( parent );
        }
      }

      return dropTarget;
    }
  }
}
