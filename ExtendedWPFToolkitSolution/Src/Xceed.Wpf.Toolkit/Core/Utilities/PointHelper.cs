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
  internal static class PointHelper
  {
    public static double DistanceBetween( Point p1, Point p2 )
    {
      return Math.Sqrt( Math.Pow( p1.X - p2.X, 2 ) + Math.Pow( p1.Y - p2.Y, 2 ) );
    }

    public static Point Empty
    {
      get
      {
        return new Point( double.NaN, double.NaN );
      }
    }

    public static bool IsEmpty( Point point )
    {
      return DoubleHelper.IsNaN( point.X ) && DoubleHelper.IsNaN( point.Y );
    }
  }
}
