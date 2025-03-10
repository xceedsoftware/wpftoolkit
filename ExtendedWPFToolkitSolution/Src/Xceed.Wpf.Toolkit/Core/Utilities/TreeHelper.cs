/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal static class TreeHelper
  {
    public static DependencyObject GetParent( DependencyObject element )
    {
      return TreeHelper.GetParent( element, true );
    }

    private static DependencyObject GetParent( DependencyObject element, bool recurseIntoPopup )
    {
      if( recurseIntoPopup )
      {
        // Case 126732 : To correctly detect parent of a popup we must do that exception case
        Popup popup = element as Popup;

        if( ( popup != null ) && ( popup.PlacementTarget != null ) )
          return popup.PlacementTarget;
      }

      Visual visual = element as Visual;
      DependencyObject parent = ( visual == null ) ? null : VisualTreeHelper.GetParent( visual );

      if( parent == null )
      {
        // No Visual parent. Check in the logical tree.
        FrameworkElement fe = element as FrameworkElement;

        if( fe != null )
        {
          parent = fe.Parent;

          if( parent == null )
          {
            parent = fe.TemplatedParent;
          }
        }
        else
        {
          FrameworkContentElement fce = element as FrameworkContentElement;

          if( fce != null )
          {
            parent = fce.Parent;

            if( parent == null )
            {
              parent = fce.TemplatedParent;
            }
          }
        }
      }

      return parent;
    }

    public static T FindParent<T>( DependencyObject startingObject ) where T : DependencyObject
    {
      return TreeHelper.FindParent<T>( startingObject, false, null );
    }

    public static T FindParent<T>( DependencyObject startingObject, bool checkStartingObject ) where T : DependencyObject
    {
      return TreeHelper.FindParent<T>( startingObject, checkStartingObject, null );
    }

    public static T FindParent<T>( DependencyObject startingObject, bool checkStartingObject, Func<T, bool> additionalCheck ) where T : DependencyObject
    {
      T foundElement;
      DependencyObject parent = ( checkStartingObject ? startingObject : TreeHelper.GetParent( startingObject, true ) );

      while( parent != null )
      {
        foundElement = parent as T;

        if( foundElement != null )
        {
          if( additionalCheck == null )
          {
            return foundElement;
          }
          else
          {
            if( additionalCheck( foundElement ) )
              return foundElement;
          }
        }

        parent = TreeHelper.GetParent( parent, true );
      }

      return null;
    }

    public static T FindChild<T>( DependencyObject parent ) where T : DependencyObject
    {
      return TreeHelper.FindChild<T>( parent, null );
    }

    public static T FindChild<T>( DependencyObject parent, Func<T, bool> additionalCheck ) where T : DependencyObject
    {
      int childrenCount = VisualTreeHelper.GetChildrenCount( parent );
      T child;

      for( int index = 0; index < childrenCount; index++ )
      {
        child = VisualTreeHelper.GetChild( parent, index ) as T;

        if( child != null )
        {
          if( additionalCheck == null )
          {
            return child;
          }
          else
          {
            if( additionalCheck( child ) )
              return child;
          }
        }
      }

      for( int index = 0; index < childrenCount; index++ )
      {
        child = TreeHelper.FindChild<T>( VisualTreeHelper.GetChild( parent, index ), additionalCheck );

        if( child != null )
          return child;
      }

      return null;
    }

    public static bool IsDescendantOf( DependencyObject element, DependencyObject parent )
    {
      return TreeHelper.IsDescendantOf( element, parent, true );
    }

    public static bool IsDescendantOf( DependencyObject element, DependencyObject parent, bool recurseIntoPopup )
    {
      while( element != null )
      {
        if( element == parent )
          return true;

        element = TreeHelper.GetParent( element, recurseIntoPopup );
      }

      return false;
    }
  }
}
