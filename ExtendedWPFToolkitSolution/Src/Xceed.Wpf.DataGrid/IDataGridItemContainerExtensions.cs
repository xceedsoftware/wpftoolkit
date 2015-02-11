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
using System.Windows;
using Xceed.Utils.Wpf;

namespace Xceed.Wpf.DataGrid
{
  internal static class IDataGridItemContainerExtensions
  {
    internal static IEnumerable<IDataGridItemContainer> GetTemplatedDescendantDataGridItemContainers( this IDataGridItemContainer source )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      var predicate = IDataGridItemContainerExtensions.GetTemplatedDescendantExpandPredicate( source );

      return IDataGridItemContainerExtensions.GetChildDataGridItemContainers( source, predicate );
    }

    internal static IEnumerable<IDataGridItemContainer> GetTemplatedChildDataGridItemContainers( this IDataGridItemContainer source )
    {
      if( source == null )
        throw new ArgumentNullException( "source" );

      var predicate = IDataGridItemContainerExtensions.GetTemplatedChildExpandPredicate( source );

      return IDataGridItemContainerExtensions.GetChildDataGridItemContainers( source, predicate );
    }

    private static IEnumerable<IDataGridItemContainer> GetChildDataGridItemContainers( IDataGridItemContainer source, Func<DependencyObject, bool> expand )
    {
      Debug.Assert( source != null );

      var root = source as DependencyObject;
      if( root == null )
        throw new ArgumentException( "The source object must be a DependencyObject.", "source" );

      return IDataGridItemContainerExtensions.GetChildDataGridItemContainers( root, expand );
    }

    private static IEnumerable<IDataGridItemContainer> GetChildDataGridItemContainers( DependencyObject source, Func<DependencyObject, bool> expand )
    {
      Debug.Assert( source != null );

      if( expand == null )
        throw new ArgumentNullException( "expand" );

      var strategy = new DepthFirstSearchTreeTraversalStrategy();

      return TreeHelper.GetDescendants<IDataGridItemContainer>( strategy, source, null, expand );
    }

    private static Func<DependencyObject, bool> GetTemplatedDescendantExpandPredicate( IDataGridItemContainer source )
    {
      var fe = source as FrameworkElement;
      if( fe == null )
        throw new ArgumentException( "The source object must be a FrameworkElement.", "source" );

      return ( DependencyObject item ) => IDataGridItemContainerExtensions.IsPartOfTargetTemplate( item, fe );
    }

    private static Func<DependencyObject, bool> GetTemplatedChildExpandPredicate( IDataGridItemContainer source )
    {
      var fe = source as FrameworkElement;
      if( fe == null )
        throw new ArgumentException( "The source object must be a FrameworkElement.", "source" );

      return ( DependencyObject item ) => IDataGridItemContainerExtensions.ExpandChild( item )
                                       && IDataGridItemContainerExtensions.IsPartOfTargetTemplate( item, fe );
    }

    private static bool ExpandChild( DependencyObject container )
    {
      return ( container != null ) && !( container is IDataGridItemContainer );
    }

    private static bool IsPartOfTargetTemplate( DependencyObject element, FrameworkElement target )
    {
      while( element != null )
      {
        var templatedParent = IDataGridItemContainerExtensions.GetTemplatedParent( element );
        if( templatedParent == target )
          return true;

        element = templatedParent;
      }

      return false;
    }

    private static DependencyObject GetTemplatedParent( DependencyObject element )
    {
      var fe = element as FrameworkElement;
      if( fe != null )
        return fe.TemplatedParent;

      var fce = element as FrameworkContentElement;
      if( fce != null )
        return fce.TemplatedParent;

      return null;
    }
  }
}
