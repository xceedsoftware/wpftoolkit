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
using System.ComponentModel;
using System.Runtime.InteropServices;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.Core;

namespace Xceed.Wpf.Toolkit.Media.Animation
{
  [TypeConverter( typeof( AnimationRateConverter ) )]
  [StructLayout( LayoutKind.Explicit )]
  public struct AnimationRate
  {
    #region Static Fields

    private static AnimationRate _default = new AnimationRate( true );

    #endregion

    #region Constructors

    public AnimationRate( TimeSpan duration )
    {
      if( duration < TimeSpan.Zero )
      {
        throw new ArgumentException( ErrorMessages.GetMessage( ErrorMessages.NegativeTimeSpanNotSupported ) );
      }
      _speed = 0d;
      _duration = duration.Ticks;
      _rateType = RateType.TimeSpan;
    }

    public AnimationRate( double speed )
    {
      if( DoubleHelper.IsNaN( speed ) || speed < 0d )
      {
        throw new ArgumentException( ErrorMessages.GetMessage( ErrorMessages.NegativeSpeedNotSupported ) );
      }
      _duration = 0;
      _speed = speed;
      _rateType = RateType.Speed;
    }

    private AnimationRate( bool ignore )
    {
      _duration = 0;
      _speed = double.NaN;
      _rateType = RateType.Speed;
    }

    #endregion

    #region Default Property

    public static AnimationRate Default
    {
      get
      {
        return _default;
      }
    }

    #endregion

    #region HasDuration Property

    public bool HasDuration
    {
      get
      {
        return ( _rateType == RateType.TimeSpan );
      }
    }

    #endregion

    #region Duration Property

    public TimeSpan Duration
    {
      get
      {
        if( this.HasDuration )
          return TimeSpan.FromTicks( _duration );

        throw new InvalidOperationException( 
          string.Format(
            ErrorMessages.GetMessage( ErrorMessages.InvalidRatePropertyAccessed ),
            "Duration", 
            this, 
            "Speed" ) );
      }
    }

    #endregion

    #region HasSpeed Property

    public bool HasSpeed
    {
      get
      {
        return ( _rateType == RateType.Speed );
      }
    }

    #endregion

    #region Speed Property

    public double Speed
    {
      get
      {
        if( this.HasSpeed )
          return _speed;

        throw new InvalidOperationException( 
          string.Format(
            ErrorMessages.GetMessage( ErrorMessages.InvalidRatePropertyAccessed ),
            "Speed", 
            this, 
            "Duration" ) );
      }
    }

    #endregion

    public AnimationRate Add( AnimationRate animationRate )
    {
      return this + animationRate;
    }

    public override bool Equals( Object value )
    {
      if( value == null )
        return false;

      if( value is AnimationRate )
        return this.Equals( ( AnimationRate )value );

      return false;
    }

    public bool Equals( AnimationRate animationRate )
    {
      if( this.HasDuration )
      {
        if( animationRate.HasDuration )
          return _duration == animationRate._duration;

        return false;
      }
      else // HasSpeed
      {
        if( animationRate.HasSpeed )
        {
          if( DoubleHelper.IsNaN( _speed ) )
            return DoubleHelper.IsNaN( animationRate._speed );

          return _speed == animationRate._speed;
        }

        return false;
      }
    }

    public static bool Equals( AnimationRate t1, AnimationRate t2 )
    {
      return t1.Equals( t2 );
    }

    public override int GetHashCode()
    {
      if( this.HasDuration )
        return _duration.GetHashCode();

      return _speed.GetHashCode();
    }

    public AnimationRate Subtract( AnimationRate animationRate )
    {
      return this - animationRate;
    }

    public override string ToString()
    {
      if( this.HasDuration )
        return TypeDescriptor.GetConverter( _duration ).ConvertToString( _duration );

      return TypeDescriptor.GetConverter( _speed ).ConvertToString( _speed );
    }

    #region Operators Methods

    public static implicit operator AnimationRate( TimeSpan duration )
    {
      if( duration < TimeSpan.Zero )
        throw new ArgumentException( ErrorMessages.GetMessage( ErrorMessages.NegativeTimeSpanNotSupported ) );

      return new AnimationRate( duration );
    }

    public static implicit operator AnimationRate( double speed )
    {
      if( DoubleHelper.IsNaN( speed ) || speed < 0 )
        throw new ArgumentException( ErrorMessages.GetMessage( ErrorMessages.NegativeSpeedNotSupported ) );

      return new AnimationRate( speed );
    }

    public static implicit operator AnimationRate( int speed )
    {
      if( DoubleHelper.IsNaN( speed ) || speed < 0 )
        throw new ArgumentException( ErrorMessages.GetMessage( ErrorMessages.NegativeSpeedNotSupported ) );

      return new AnimationRate( ( double )speed );
    }

    public static AnimationRate operator +( AnimationRate t1, AnimationRate t2 )
    {
      if( t1.HasDuration && t2.HasDuration )
        return new AnimationRate( t1._duration + t2._duration );

      if( t1.HasSpeed && t2.HasSpeed )
        return new AnimationRate( t1._speed + t2._speed );

      return ( AnimationRate )0d;
    }

    public static AnimationRate operator -( AnimationRate t1, AnimationRate t2 )
    {
      if( t1.HasDuration && t2.HasDuration )
        return new AnimationRate( t1._duration - t2._duration );

      if( t1.HasSpeed && t2.HasSpeed )
        return new AnimationRate( t1._speed - t2._speed );

      return ( AnimationRate )0d;
    }

    public static bool operator ==( AnimationRate t1, AnimationRate t2 )
    {
      return t1.Equals( t2 );
    }

    public static bool operator !=( AnimationRate t1, AnimationRate t2 )
    {
      return !( t1.Equals( t2 ) );
    }

    public static bool operator >( AnimationRate t1, AnimationRate t2 )
    {
      if( t1.HasDuration && t2.HasDuration )
        return t1._duration > t2._duration;

      if( t1.HasSpeed && t2.HasSpeed )
        return ( t1._speed > t2._speed ) && !DoubleHelper.AreVirtuallyEqual( t1._speed, t2._speed );

      // arbitrary: assume a Speed is greater than a Duration
      return t1.HasSpeed;
    }

    public static bool operator >=( AnimationRate t1, AnimationRate t2 )
    {
      return !( t1 < t2 );
    }

    public static bool operator <( AnimationRate t1, AnimationRate t2 )
    {
      if( t1.HasDuration && t2.HasDuration )
        return t1._duration < t2._duration;

      if( t1.HasSpeed && t2.HasSpeed )
        return ( t1._speed < t2._speed ) && !DoubleHelper.AreVirtuallyEqual( t1._speed, t2._speed );

      // arbitrary: assume a Speed is greater than a Duration
      return t1.HasDuration;
    }

    public static bool operator <=( AnimationRate t1, AnimationRate t2 )
    {
      return !( t1 > t2 );
    }

    public static int Compare( AnimationRate t1, AnimationRate t2 )
    {
      if( t1 < t2 )
        return -1;

      if( t1 > t2 )
        return 1;

      // Neither is greater than the other
      return 0;
    }

    public static AnimationRate Plus( AnimationRate animationRate )
    {
      return animationRate;
    }

    public static AnimationRate operator +( AnimationRate animationRate )
    {
      return animationRate;
    }

    #endregion

    #region Private Fields

    [FieldOffset( 0 )]
    long _duration;
    [FieldOffset( 0 )]
    double _speed;
    [FieldOffset( 8 )]
    RateType _rateType;

    #endregion

    #region RateType Nested Type

    private enum RateType
    {
      TimeSpan,
      Speed,
    }

    #endregion
  }
}
