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
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  /// <summary>
  /// This Control is intended to be used in the template of the 
  /// PropertyItemBase and PropertyGrid classes to contain the
  /// sub-children properties.
  /// </summary>
  public class PropertyItemsControl : ItemsControl
  {
    public PropertyItemsControl()
    {
      var propertyItemsControlProperties = TypeDescriptor.GetProperties( this, new Attribute[] { new PropertyFilterAttribute( PropertyFilterOptions.All ) } );
      var prop1 = propertyItemsControlProperties.Find( "VirtualizingPanel.IsVirtualizingWhenGrouping", false );
      if( prop1 != null )
      {
        prop1.SetValue( this, true );
      }
      var prop2 = propertyItemsControlProperties.Find( "VirtualizingPanel.CacheLengthUnit", false );
      if( prop2 != null )
      {
        prop2.SetValue( this, Enum.ToObject( prop2.PropertyType, 1 ) );
      }
    }

    #region PreparePropertyItemEvent Attached Routed Event

    internal static readonly RoutedEvent PreparePropertyItemEvent = EventManager.RegisterRoutedEvent( "PreparePropertyItem", RoutingStrategy.Bubble, typeof( PropertyItemEventHandler ), typeof( PropertyItemsControl ) );
    internal event PropertyItemEventHandler PreparePropertyItem
    {
      add
      {
        AddHandler( PropertyItemsControl.PreparePropertyItemEvent, value );
      }
      remove
      {
        RemoveHandler( PropertyItemsControl.PreparePropertyItemEvent, value );
      }
    }

    private void RaisePreparePropertyItemEvent( PropertyItemBase propertyItem, object item )
    {
      this.RaiseEvent( new PropertyItemEventArgs( PropertyItemsControl.PreparePropertyItemEvent, this, propertyItem, item ) );
    }

    #endregion

    #region ClearPropertyItemEvent Attached Routed Event

    internal static readonly RoutedEvent ClearPropertyItemEvent = EventManager.RegisterRoutedEvent( "ClearPropertyItem", RoutingStrategy.Bubble, typeof( PropertyItemEventHandler ), typeof( PropertyItemsControl ) );
    internal event PropertyItemEventHandler ClearPropertyItem
    {
      add
      {
        AddHandler( PropertyItemsControl.ClearPropertyItemEvent, value );
      }
      remove
      {
        RemoveHandler( PropertyItemsControl.ClearPropertyItemEvent, value );
      }
    }

    private void RaiseClearPropertyItemEvent( PropertyItemBase propertyItem, object item )
    {
      this.RaiseEvent( new PropertyItemEventArgs( PropertyItemsControl.ClearPropertyItemEvent, this, propertyItem, item ) );
    }

    #endregion

    protected override bool IsItemItsOwnContainerOverride( object item )
    {
      return ( item is PropertyItemBase );
    }


    protected override void PrepareContainerForItemOverride( DependencyObject element, object item )
    {
      base.PrepareContainerForItemOverride( element, item );
      this.RaisePreparePropertyItemEvent( ( PropertyItemBase )element, item );
    }

    protected override void ClearContainerForItemOverride( DependencyObject element, object item )
    {
      this.RaiseClearPropertyItemEvent( ( PropertyItemBase )element, item );
      base.ClearContainerForItemOverride( element, item );
    }

  }
}
