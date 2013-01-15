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
using System.Windows;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal static class VisualTreeHelperEx
  {
    public static DependencyObject FindAncestorByType( DependencyObject element, Type type, bool specificTypeOnly )
    {
      if( element == null )
        return null;

      if( specificTypeOnly ? ( element.GetType() == type )
          : ( element.GetType() == type ) || ( element.GetType().IsSubclassOf( type ) ) )
        return element;

      return VisualTreeHelperEx.FindAncestorByType( VisualTreeHelper.GetParent( element ), type, specificTypeOnly );
    }

    public static T FindAncestorByType<T>( DependencyObject depObj ) where T : DependencyObject
    {
      if( depObj == null )
      {
        return default( T );
      }
      if( depObj is T )
      {
        return ( T )depObj;
      }

      T parent = default( T );

      parent = VisualTreeHelperEx.FindAncestorByType<T>( VisualTreeHelper.GetParent( depObj ) );

      return parent;
    }

    public static Visual FindDescendantByName( Visual element, string name )
    {
      if( element != null && ( element is FrameworkElement ) && ( element as FrameworkElement ).Name == name )
        return element;

      Visual foundElement = null;
      if( element is FrameworkElement )
        ( element as FrameworkElement ).ApplyTemplate();

      for( int i = 0; i < VisualTreeHelper.GetChildrenCount( element ); i++ )
      {
        Visual visual = VisualTreeHelper.GetChild( element, i ) as Visual;
        foundElement = VisualTreeHelperEx.FindDescendantByName( visual, name );
        if( foundElement != null )
          break;
      }

      return foundElement;
    }

    public static Visual FindDescendantByType( Visual element, Type type )
    {
      return VisualTreeHelperEx.FindDescendantByType( element, type, true );
    }

    public static Visual FindDescendantByType( Visual element, Type type, bool specificTypeOnly )
    {
      if( element == null )
        return null;

      if( specificTypeOnly ? ( element.GetType() == type )
          : ( element.GetType() == type ) || ( element.GetType().IsSubclassOf( type ) ) )
        return element;

      Visual foundElement = null;
      if( element is FrameworkElement )
        ( element as FrameworkElement ).ApplyTemplate();

      for( int i = 0; i < VisualTreeHelper.GetChildrenCount( element ); i++ )
      {
        Visual visual = VisualTreeHelper.GetChild( element, i ) as Visual;
        foundElement = VisualTreeHelperEx.FindDescendantByType( visual, type, specificTypeOnly );
        if( foundElement != null )
          break;
      }

      return foundElement;
    }

    public static T FindDescendantByType<T>( Visual element ) where T : Visual
    {
      Visual temp = VisualTreeHelperEx.FindDescendantByType( element, typeof( T ) );

      return ( T )temp;
    }

    public static Visual FindDescendantWithPropertyValue( Visual element,
        DependencyProperty dp, object value )
    {
      if( element == null )
        return null;

      if( element.GetValue( dp ).Equals( value ) )
        return element;

      Visual foundElement = null;
      if( element is FrameworkElement )
        ( element as FrameworkElement ).ApplyTemplate();

      for( int i = 0; i < VisualTreeHelper.GetChildrenCount( element ); i++ )
      {
        Visual visual = VisualTreeHelper.GetChild( element, i ) as Visual;
        foundElement = VisualTreeHelperEx.FindDescendantWithPropertyValue( visual, dp, value );
        if( foundElement != null )
          break;
      }

      return foundElement;
    }
  }
}
