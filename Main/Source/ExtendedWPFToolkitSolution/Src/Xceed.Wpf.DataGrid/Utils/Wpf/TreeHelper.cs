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
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xceed.Utils.Wpf
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
      return TreeHelper.FindParent<T>( startingObject, checkStartingObject, additionalCheck, null );
    }

    /// <summary>
    /// This will search for a parent of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the element to find</typeparam>
    /// <param name="startingObject">The node where the search begins.</param>
    /// <param name="checkStartingObject">Should the specified startingObject be checked first.</param>
    /// <param name="additionalCheck">Provide a callback to check additional properties 
    /// of the found elements. Can be left Null if no additional criteria are needed.</param>
    /// <param name="scope">Should not search behond this parent. Scope value is excluded from the search</param>
    /// <returns>Returns the found element. Null if nothing is found.</returns>
    /// <example>Button button = TreeHelper.FindParent&lt;Button&gt;( this, foundChild => foundChild.Focusable );</example>
    internal static T FindParent<T>( DependencyObject startingObject, bool checkStartingObject, Func<T, bool> additionalCheck, DependencyObject scope ) where T : DependencyObject
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

        if( object.ReferenceEquals( parent, scope ) )
          break;

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
    /// <param name="predicate">Provide a callback to check additional properties 
    /// of the found elements. Can be left Null if no additional criteria are needed.</param>
    /// <returns>Returns the found element. Null if nothing is found.</returns>
    /// <example>Button button = TreeHelper.FindChild&lt;Button&gt;( this, foundChild => foundChild.Focusable );</example>
    public static T FindChild<T>( DependencyObject parent, Func<T, bool> predicate ) where T : DependencyObject
    {
      return TreeHelper.GetDescendants<T>( new BreadthFirstSearchTreeTraversalStrategy(), parent, predicate, null ).FirstOrDefault();
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

    public static IEnumerable<T> GetDescendants<T>( ITreeTraversalStrategy strategy, DependencyObject root ) where T : DependencyObject
    {
      return TreeHelper.GetDescendants<T>( strategy, root, null, null );
    }

    public static IEnumerable<T> GetDescendants<T>( ITreeTraversalStrategy strategy, DependencyObject root, Func<T, bool> predicate ) where T : class
    {
      return TreeHelper.GetDescendants<T>( strategy, root, predicate, null );
    }

    public static IEnumerable<T> GetDescendants<T>(
      ITreeTraversalStrategy strategy,
      DependencyObject root,
      Func<T, bool> predicate,
      Func<DependencyObject, bool> expand ) where T : class
    {
      if( strategy == null )
        throw new ArgumentNullException( "strategy" );

      if( root == null )
        yield break;

      var traversal = strategy.Create();
      Debug.Assert( traversal != null );

      traversal.VisitNodes( TreeHelper.GetChildren( root ) );

      while( traversal.MoveNext() )
      {
        var current = traversal.Current;
        var descendant = current as T;

        if( ( descendant != null ) && ( ( predicate == null ) || predicate.Invoke( descendant ) ) )
        {
          yield return descendant;
        }

        if( ( expand == null ) || expand.Invoke( current ) )
        {
          traversal.VisitNodes( TreeHelper.GetChildren( current ) );
        }
      }
    }

    private static IEnumerable<DependencyObject> GetChildren( DependencyObject parent )
    {
      if( parent == null )
        yield break;

      var childrenCount = VisualTreeHelper.GetChildrenCount( parent );
      for( int i = 0; i < childrenCount; i++ )
      {
        var child = VisualTreeHelper.GetChild( parent, i );
        if( child != null )
        {
          yield return child;
        }
      }
    }
  }
}
