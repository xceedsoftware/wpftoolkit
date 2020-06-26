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

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls.Primitives;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal static class TreeHelper
  {
    /// <summary>
    /// Tries its best to return the specified element's parent. It will 
    /// try to find, in this order, the VisualParent, LogicalParent, LogicalTemplatedParent.
    /// It only works for Visual, FrameworkElement or FrameworkContentElement.
    /// </summary>
    /// <param name="element">The element to which to return the parent. It will only 
    /// work if element is a Visual, a FrameworkElement or a FrameworkContentElement.</param>
    /// <remarks>If the logical parent is not found (Parent), we check the TemplatedParent
    /// (see FrameworkElement.Parent documentation). But, we never actually witnessed
    /// this situation.</remarks>
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

    /// <summary>
    /// This will search for a parent of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the element to find</typeparam>
    /// <param name="startingObject">The node where the search begins. This element is not checked.</param>
    /// <returns>Returns the found element. Null if nothing is found.</returns>
    public static T FindParent<T>( DependencyObject startingObject ) where T : DependencyObject
    {
      return TreeHelper.FindParent<T>( startingObject, false, null );
    }

    /// <summary>
    /// This will search for a parent of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the element to find</typeparam>
    /// <param name="startingObject">The node where the search begins.</param>
    /// <param name="checkStartingObject">Should the specified startingObject be checked first.</param>
    /// <returns>Returns the found element. Null if nothing is found.</returns>
    public static T FindParent<T>( DependencyObject startingObject, bool checkStartingObject ) where T : DependencyObject
    {
      return TreeHelper.FindParent<T>( startingObject, checkStartingObject, null );
    }

    /// <summary>
    /// This will search for a parent of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the element to find</typeparam>
    /// <param name="startingObject">The node where the search begins.</param>
    /// <param name="checkStartingObject">Should the specified startingObject be checked first.</param>
    /// <param name="additionalCheck">Provide a callback to check additional properties 
    /// of the found elements. Can be left Null if no additional criteria are needed.</param>
    /// <returns>Returns the found element. Null if nothing is found.</returns>
    /// <example>Button button = TreeHelper.FindParent&lt;Button&gt;( this, foundChild => foundChild.Focusable );</example>
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

    /// <summary>
    /// This will search for a child of the specified type. The search is performed 
    /// hierarchically, breadth first (as opposed to depth first).
    /// </summary>
    /// <typeparam name="T">The type of the element to find</typeparam>
    /// <param name="parent">The root of the tree to search for. This element itself is not checked.</param>
    /// <returns>Returns the found element. Null if nothing is found.</returns>
    public static T FindChild<T>( DependencyObject parent ) where T : DependencyObject
    {
      return TreeHelper.FindChild<T>( parent, null );
    }

    /// <summary>
    /// This will search for a child of the specified type. The search is performed 
    /// hierarchically, breadth first (as opposed to depth first).
    /// </summary>
    /// <typeparam name="T">The type of the element to find</typeparam>
    /// <param name="parent">The root of the tree to search for. This element itself is not checked.</param>
    /// <param name="additionalCheck">Provide a callback to check additional properties 
    /// of the found elements. Can be left Null if no additional criteria are needed.</param>
    /// <returns>Returns the found element. Null if nothing is found.</returns>
    /// <example>Button button = TreeHelper.FindChild&lt;Button&gt;( this, foundChild => foundChild.Focusable );</example>
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

    /// <summary>
    /// Returns true if the specified element is a child of parent somewhere in the visual 
    /// tree. This method will work for Visual, FrameworkElement and FrameworkContentElement.
    /// </summary>
    /// <param name="element">The element that is potentially a child of the specified parent.</param>
    /// <param name="parent">The element that is potentially a parent of the specified element.</param>
    public static bool IsDescendantOf( DependencyObject element, DependencyObject parent )
    {
      return TreeHelper.IsDescendantOf( element, parent, true );
    }

    /// <summary>
    /// Returns true if the specified element is a child of parent somewhere in the visual 
    /// tree. This method will work for Visual, FrameworkElement and FrameworkContentElement.
    /// </summary>
    /// <param name="element">The element that is potentially a child of the specified parent.</param>
    /// <param name="parent">The element that is potentially a parent of the specified element.</param>
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
