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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public static class Extentions
  {
    public static IEnumerable<T> FindVisualChildren<T>( this DependencyObject depObj ) where T : DependencyObject
    {
      if( depObj != null )
      {
        for( int i = 0; i < VisualTreeHelper.GetChildrenCount( depObj ); i++ )
        {
          DependencyObject child = VisualTreeHelper.GetChild( depObj, i );
          if( child != null && child is T )
          {
            yield return ( T )child;
          }

          foreach( T childOfChild in FindVisualChildren<T>( child ) )
          {
            yield return childOfChild;
          }
        }
      }
    }

    public static IEnumerable<T> FindLogicalChildren<T>( this DependencyObject depObj ) where T : DependencyObject
    {
      if( depObj != null )
      {
        foreach( DependencyObject child in LogicalTreeHelper.GetChildren( depObj ).OfType<DependencyObject>() )
        {
          if( child != null && child is T )
          {
            yield return ( T )child;
          }

          foreach( T childOfChild in FindLogicalChildren<T>( child ) )
          {
            yield return childOfChild;
          }
        }
      }
    }

    public static DependencyObject FindVisualTreeRoot( this DependencyObject initial )
    {
      DependencyObject current = initial;
      DependencyObject result = initial;

      while( current != null )
      {
        result = current;
        if( current is Visual || current is Visual3D )
        {
          current = VisualTreeHelper.GetParent( current );
        }
        else
        {
          // If we're in Logical Land then we must walk 
          // up the logical tree until we find a 
          // Visual/Visual3D to get us back to Visual Land.
          current = LogicalTreeHelper.GetParent( current );
        }
      }

      return result;
    }

    public static T FindVisualAncestor<T>( this DependencyObject dependencyObject ) where T : class
    {
      DependencyObject target = dependencyObject;
      do
      {
        target = VisualTreeHelper.GetParent( target );
      }
      while( target != null && !( target is T ) );
      return target as T;
    }

    public static T FindLogicalAncestor<T>( this DependencyObject dependencyObject ) where T : class
    {
      DependencyObject target = dependencyObject;
      do
      {
        var current = target;
        target = LogicalTreeHelper.GetParent( target );
        if( target == null )
          target = VisualTreeHelper.GetParent( current );

      }
      while( target != null && !( target is T ) );
      return target as T;
    }
  }
}
