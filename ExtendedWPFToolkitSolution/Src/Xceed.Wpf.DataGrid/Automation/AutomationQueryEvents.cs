/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Automation
{
  public static class AutomationQueryEvents
  {
    #region QueryAutomationIdForDetail Event

    public static readonly RoutedEvent QueryAutomationIdForDetailEvent =
      EventManager.RegisterRoutedEvent( "QueryAutomationIdForDetail", RoutingStrategy.Bubble, typeof( QueryAutomationIdRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryAutomationIdForDetailHandler( DependencyObject d, QueryAutomationIdRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryAutomationIdForDetailEvent, handler );
    }

    public static void RemoveQueryAutomationIdForDetailHandler( DependencyObject d, QueryAutomationIdRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryAutomationIdForDetailEvent, handler );
    }

    #endregion QueryAutomationIdForDetail Event

    #region QueryHelpTextForDetail Event

    public static readonly RoutedEvent QueryHelpTextForDetailEvent =
      EventManager.RegisterRoutedEvent( "QueryHelpTextForDetail", RoutingStrategy.Bubble, typeof( QueryHelpTextRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryHelpTextForDetailHandler( DependencyObject d, QueryHelpTextRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryHelpTextForDetailEvent, handler );
    }

    public static void RemoveQueryHelpTextForDetailHandler( DependencyObject d, QueryHelpTextRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryHelpTextForDetailEvent, handler );
    }

    #endregion QueryHelpTextForDetail Event

    #region QueryItemStatusForDetail Event

    public static readonly RoutedEvent QueryItemStatusForDetailEvent =
      EventManager.RegisterRoutedEvent( "QueryItemStatusForDetail", RoutingStrategy.Bubble, typeof( QueryItemStatusRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryItemStatusForDetailHandler( DependencyObject d, QueryItemStatusRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryItemStatusForDetailEvent, handler );
    }

    public static void RemoveQueryItemStatusForDetailHandler( DependencyObject d, QueryItemStatusRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryItemStatusForDetailEvent, handler );
    }

    #endregion QueryItemStatusForDetail Event

    #region QueryItemTypeForDetail Event

    public static readonly RoutedEvent QueryItemTypeForDetailEvent =
      EventManager.RegisterRoutedEvent( "QueryItemTypeForDetail", RoutingStrategy.Bubble, typeof( QueryItemTypeRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryItemTypeForDetailHandler( DependencyObject d, QueryItemTypeRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryItemTypeForDetailEvent, handler );
    }

    public static void RemoveQueryItemTypeForDetailHandler( DependencyObject d, QueryItemTypeRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryItemTypeForDetailEvent, handler );
    }

    #endregion QueryItemTypeForDetail Event

    #region QueryNameForDetail Event

    public static readonly RoutedEvent QueryNameForDetailEvent =
      EventManager.RegisterRoutedEvent( "QueryNameForDetail", RoutingStrategy.Bubble, typeof( QueryNameRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryNameForDetailHandler( DependencyObject d, QueryNameRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryNameForDetailEvent, handler );
    }

    public static void RemoveQueryNameForDetailHandler( DependencyObject d, QueryNameRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryNameForDetailEvent, handler );
    }

    #endregion QueryNameForDetail Event

    #region QueryAutomationIdForGroup Event

    public static readonly RoutedEvent QueryAutomationIdForGroupEvent =
      EventManager.RegisterRoutedEvent( "QueryAutomationIdForGroup", RoutingStrategy.Bubble, typeof( QueryAutomationIdRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryAutomationIdForGroupHandler( DependencyObject d, QueryAutomationIdRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryAutomationIdForGroupEvent, handler );
    }

    public static void RemoveQueryAutomationIdForGroupHandler( DependencyObject d, QueryAutomationIdRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryAutomationIdForGroupEvent, handler );
    }

    #endregion QueryAutomationIdForGroup Event

    #region QueryHelpTextForGroup Event

    public static readonly RoutedEvent QueryHelpTextForGroupEvent =
      EventManager.RegisterRoutedEvent( "QueryHelpTextForGroup", RoutingStrategy.Bubble, typeof( QueryHelpTextRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryHelpTextForGroupHandler( DependencyObject d, QueryHelpTextRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryHelpTextForGroupEvent, handler );
    }

    public static void RemoveQueryHelpTextForGroupHandler( DependencyObject d, QueryHelpTextRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryHelpTextForGroupEvent, handler );
    }

    #endregion QueryHelpTextForGroup Event

    #region QueryItemStatusForGroup Event

    public static readonly RoutedEvent QueryItemStatusForGroupEvent =
      EventManager.RegisterRoutedEvent( "QueryItemStatusForGroup", RoutingStrategy.Bubble, typeof( QueryItemStatusRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryItemStatusForGroupHandler( DependencyObject d, QueryItemStatusRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryItemStatusForGroupEvent, handler );
    }

    public static void RemoveQueryItemStatusForGroupHandler( DependencyObject d, QueryItemStatusRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryItemStatusForGroupEvent, handler );
    }

    #endregion QueryItemStatusForGroup Event

    #region QueryItemTypeForGroup Event

    public static readonly RoutedEvent QueryItemTypeForGroupEvent =
      EventManager.RegisterRoutedEvent( "QueryItemTypeForGroup", RoutingStrategy.Bubble, typeof( QueryItemTypeRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryItemTypeForGroupHandler( DependencyObject d, QueryItemTypeRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryItemTypeForGroupEvent, handler );
    }

    public static void RemoveQueryItemTypeForGroupHandler( DependencyObject d, QueryItemTypeRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryItemTypeForGroupEvent, handler );
    }

    #endregion QueryItemTypeForGroup Event

    #region QueryNameForGroup Event

    public static readonly RoutedEvent QueryNameForGroupEvent =
      EventManager.RegisterRoutedEvent( "QueryNameForGroup", RoutingStrategy.Bubble, typeof( QueryNameRoutedEventHandler ), typeof( AutomationQueryEvents ) );

    public static void AddQueryNameForGroupHandler( DependencyObject d, QueryNameRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.AddHandler( AutomationQueryEvents.QueryNameForGroupEvent, handler );
    }

    public static void RemoveQueryNameForGroupHandler( DependencyObject d, QueryNameRoutedEventHandler handler )
    {
      UIElement element = d as UIElement;

      if( element != null )
        element.RemoveHandler( AutomationQueryEvents.QueryNameForGroupEvent, handler );
    }

    #endregion QueryNameForGroup Event
  }
}
