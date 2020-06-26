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

namespace Xceed.Wpf.Toolkit.PropertyGrid.Attributes
{
  public class ItemsSourceAttribute : Attribute
  {
    public Type Type
    {
      get;
      set;
    }

    public ItemsSourceAttribute( Type type )
    {
      var valueSourceInterface = type.GetInterface( typeof( IItemsSource ).FullName );
      if( valueSourceInterface == null )
        throw new ArgumentException( "Type must implement the IItemsSource interface.", "type" );

      Type = type;
    }
  }
}
