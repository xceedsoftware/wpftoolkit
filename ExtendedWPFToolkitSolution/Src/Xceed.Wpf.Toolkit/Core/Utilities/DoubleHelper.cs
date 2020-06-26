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
using System.Runtime.InteropServices;
using System.Windows;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal static class DoubleHelper
  {
    public static bool AreVirtuallyEqual( double d1, double d2 )
    {
      if( double.IsPositiveInfinity( d1 ) )
        return double.IsPositiveInfinity( d2 );

      if( double.IsNegativeInfinity( d1 ) )
        return double.IsNegativeInfinity( d2 );

      if( IsNaN( d1 ) )
        return IsNaN( d2 );

      double n = d1 - d2;
      double d = ( Math.Abs( d1 ) + Math.Abs( d2 ) + 10 ) * 1.0e-15;
      return ( -d < n ) && ( d > n );
    }

    public static bool AreVirtuallyEqual( Size s1, Size s2 )
    {
      return ( AreVirtuallyEqual( s1.Width, s2.Width )
          && AreVirtuallyEqual( s1.Height, s2.Height ) );
    }

    public static bool AreVirtuallyEqual( Point p1, Point p2 )
    {
      return ( AreVirtuallyEqual( p1.X, p2.X )
          && AreVirtuallyEqual( p1.Y, p2.Y ) );
    }

    public static bool AreVirtuallyEqual( Rect r1, Rect r2 )
    {
      return ( AreVirtuallyEqual( r1.TopLeft, r2.TopLeft )
          && AreVirtuallyEqual( r1.BottomRight, r2.BottomRight ) );
    }

    public static bool AreVirtuallyEqual( Vector v1, Vector v2 )
    {
      return ( AreVirtuallyEqual( v1.X, v2.X )
          && AreVirtuallyEqual( v1.Y, v2.Y ) );
    }

    public static bool AreVirtuallyEqual( Segment s1, Segment s2 )
    {
      // note: Segment struct already uses "virtually equal" approach
      return ( s1 == s2 );
    }

    public static bool IsNaN( double value )
    {
      // used reflector to borrow the high performance IsNan function 
      // from the WPF MS.Internal namespace
      NanUnion t = new NanUnion();
      t.DoubleValue = value;

      UInt64 exp = t.UintValue & 0xfff0000000000000;
      UInt64 man = t.UintValue & 0x000fffffffffffff;

      return ( exp == 0x7ff0000000000000 || exp == 0xfff0000000000000 ) && ( man != 0 );
    }

    #region NanUnion Nested Types

    [StructLayout( LayoutKind.Explicit )]
    private struct NanUnion
    {
      [FieldOffset( 0 )]
      internal double DoubleValue;
      [FieldOffset( 0 )]
      internal UInt64 UintValue;
    }

    #endregion
  }
}
