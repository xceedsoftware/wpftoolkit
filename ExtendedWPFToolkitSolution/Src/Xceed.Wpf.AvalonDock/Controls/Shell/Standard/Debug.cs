/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

/**************************************************************************\
    Copyright Microsoft Corporation. All Rights Reserved.
\**************************************************************************/

// Conditional to use more aggressive fail-fast behaviors when debugging.
#define DEV_DEBUG

// This file contains general utilities to aid in development.
// It is distinct from unit test Assert classes.
// Classes here generally shouldn't be exposed publicly since
// they're not particular to any library functionality.
// Because the classes here are internal, it's likely this file
// might be included in multiple assemblies.
namespace Standard
{
  using System;
  using System.Diagnostics;
  using System.Threading;

  internal static class Assert
  {
    private static void _Break()
    {
#if DEV_DEBUG
      Debugger.Break();
#else
            Debug.Assert(false);
#endif
    }

    public delegate void EvaluateFunction();

    public delegate bool ImplicationFunction();

    [Conditional( "DEBUG" )]
    public static void Evaluate( EvaluateFunction argument )
    {
      IsNotNull( argument );
      argument();
    }

    [
        Obsolete( "Use Assert.AreEqual instead of Assert.Equals", false ),
        Conditional( "DEBUG" )
    ]
    public static void Equals<T>( T expected, T actual )
    {
      AreEqual( expected, actual );
    }

    [Conditional( "DEBUG" )]
    public static void AreEqual<T>( T expected, T actual )
    {
      if( null == expected )
      {
        // Two nulls are considered equal, regardless of type semantics.
        if( null != actual && !actual.Equals( expected ) )
        {
          _Break();
        }
      }
      else if( !expected.Equals( actual ) )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void AreNotEqual<T>( T notExpected, T actual )
    {
      if( null == notExpected )
      {
        // Two nulls are considered equal, regardless of type semantics.
        if( null == actual || actual.Equals( notExpected ) )
        {
          _Break();
        }
      }
      else if( notExpected.Equals( actual ) )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void Implies( bool condition, bool result )
    {
      if( condition && !result )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void Implies( bool condition, ImplicationFunction result )
    {
      if( condition && !result() )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsNeitherNullNorEmpty( string value )
    {
      IsFalse( string.IsNullOrEmpty( value ) );
    }

    [Conditional( "DEBUG" )]
    public static void IsNeitherNullNorWhitespace( string value )
    {
      if( string.IsNullOrEmpty( value ) )
      {
        _Break();
      }

      if( value.Trim().Length == 0 )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsNotNull<T>( T value ) where T : class
    {
      if( null == value )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsDefault<T>( T value ) where T : struct
    {
      if( !value.Equals( default( T ) ) )
      {
        Assert.Fail();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsNotDefault<T>( T value ) where T : struct
    {
      if( value.Equals( default( T ) ) )
      {
        Assert.Fail();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsFalse( bool condition )
    {
      if( condition )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsFalse( bool condition, string message )
    {
      if( condition )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsTrue( bool condition )
    {
      if( !condition )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsTrue( bool condition, string message )
    {
      if( !condition )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void Fail()
    {
      _Break();
    }

    [Conditional( "DEBUG" )]
    public static void Fail( string message )
    {
      _Break();
    }

    [Conditional( "DEBUG" )]
    public static void IsNull<T>( T item ) where T : class
    {
      if( null != item )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void BoundedDoubleInc( double lowerBoundInclusive, double value, double upperBoundInclusive )
    {
      if( value < lowerBoundInclusive || value > upperBoundInclusive )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void BoundedInteger( int lowerBoundInclusive, int value, int upperBoundExclusive )
    {
      if( value < lowerBoundInclusive || value >= upperBoundExclusive )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsApartmentState( ApartmentState expectedState )
    {
      if( Thread.CurrentThread.GetApartmentState() != expectedState )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void NullableIsNotNull<T>( T? value ) where T : struct
    {
      if( null == value )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void NullableIsNull<T>( T? value ) where T : struct
    {
      if( null != value )
      {
        _Break();
      }
    }

    [Conditional( "DEBUG" )]
    public static void IsNotOnMainThread()
    {
      if( System.Windows.Application.Current.Dispatcher.CheckAccess() )
      {
        _Break();
      }
    }
  }
}
