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

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal static class RoutedEventHelper
  {
    internal static void RaiseEvent( DependencyObject target, RoutedEventArgs args )
    {
      if( target is UIElement )
      {
        ( target as UIElement ).RaiseEvent( args );
      }
      else if( target is ContentElement )
      {
        ( target as ContentElement ).RaiseEvent( args );
      }
    }

    internal static void AddHandler( DependencyObject element, RoutedEvent routedEvent, Delegate handler )
    {
      UIElement uie = element as UIElement;
      if( uie != null )
      {
        uie.AddHandler( routedEvent, handler );
      }
      else
      {
        ContentElement ce = element as ContentElement;
        if( ce != null )
        {
          ce.AddHandler( routedEvent, handler );
        }
      }
    }

    internal static void RemoveHandler( DependencyObject element, RoutedEvent routedEvent, Delegate handler )
    {
      UIElement uie = element as UIElement;
      if( uie != null )
      {
        uie.RemoveHandler( routedEvent, handler );
      }
      else
      {
        ContentElement ce = element as ContentElement;
        if( ce != null )
        {
          ce.RemoveHandler( routedEvent, handler );
        }
      }
    }
  }
}
