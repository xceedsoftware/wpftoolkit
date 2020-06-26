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

namespace Xceed.Wpf.AvalonDock
{
  internal static class MathHelper
  {
    public static double MinMax( double value, double min, double max )
    {
      if( min > max )
        throw new ArgumentException( "min>max" );

      if( value < min )
        return min;
      if( value > max )
        return max;

      return value;
    }

    public static void AssertIsPositiveOrZero( double value )
    {
      if( value < 0.0 )
        throw new ArgumentException( "Invalid value, must be a positive number or equal to zero" );
    }
  }
}
