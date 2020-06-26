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

namespace Xceed.Wpf.Toolkit
{
  public class TimeItem
  {
    public string Display
    {
      get;
      set;
    }
    public TimeSpan Time
    {
      get;
      set;
    }

    public TimeItem( string display, TimeSpan time )
    {
      Display = display;
      Time = time;
    }

    #region Base Class Overrides

    public override bool Equals( object obj )
    {
      var item = obj as TimeItem;
      if( item != null )
        return Time == item.Time;
      else
        return false;
    }

    public override int GetHashCode()
    {
      return Time.GetHashCode();
    }

    #endregion //Base Class Overrides
  }
}
