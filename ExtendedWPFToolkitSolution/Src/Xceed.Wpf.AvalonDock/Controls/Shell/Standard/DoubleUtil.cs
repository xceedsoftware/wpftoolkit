/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2024 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/


namespace Standard
{
  using System.Diagnostics.CodeAnalysis;

  internal static class DoubleUtilities
  {
    private const double Epsilon = 0.00000153;

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static bool AreClose( double value1, double value2 )
    {
      if( value1 == value2 )
      {
        return true;
      }

      double delta = value1 - value2;
      return ( delta < Epsilon ) && ( delta > -Epsilon );
    }

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static bool LessThan( double value1, double value2 )
    {
      return ( value1 < value2 ) && !AreClose( value1, value2 );
    }

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static bool GreaterThan( double value1, double value2 )
    {
      return ( value1 > value2 ) && !AreClose( value1, value2 );
    }

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static bool LessThanOrClose( double value1, double value2 )
    {
      return ( value1 < value2 ) || AreClose( value1, value2 );
    }

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static bool GreaterThanOrClose( double value1, double value2 )
    {
      return ( value1 > value2 ) || AreClose( value1, value2 );
    }

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static bool IsFinite( double value )
    {
      return !double.IsNaN( value ) && !double.IsInfinity( value );
    }

    [SuppressMessage( "Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode" )]
    public static bool IsValidSize( double value )
    {
      return IsFinite( value ) && GreaterThanOrClose( value, 0 );
    }
  }
}
