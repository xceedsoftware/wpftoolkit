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

namespace Xceed.Wpf.Toolkit.Media.Animation
{
  public static class PennerEquations
  {
    #region BackEaseIn Static Property

    public static PennerEquation BackEaseIn
    {
      get
      {
        if( _backEaseIn == null )
        {
          _backEaseIn = new PennerEquation( BackEaseInImpl );
        }
        return _backEaseIn;
      }
    }

    private static PennerEquation _backEaseIn;

    #endregion

    #region BackEaseInOut Static Property

    public static PennerEquation BackEaseInOut
    {
      get
      {
        if( _backEaseInOut == null )
        {
          _backEaseInOut = new PennerEquation( BackEaseInOutImpl );
        }
        return _backEaseInOut;
      }
    }

    private static PennerEquation _backEaseInOut;

    #endregion

    #region BackEaseOut Static Property

    public static PennerEquation BackEaseOut
    {
      get
      {
        if( _backEaseOut == null )
        {
          _backEaseOut = new PennerEquation( BackEaseOutImpl );
        }
        return _backEaseOut;
      }
    }

    private static PennerEquation _backEaseOut;

    #endregion

    #region BounceEaseIn Static Property

    public static PennerEquation BounceEaseIn
    {
      get
      {
        if( _bounceEaseIn == null )
        {
          _bounceEaseIn = new PennerEquation( BounceEaseInImpl );
        }
        return _bounceEaseIn;
      }
    }

    private static PennerEquation _bounceEaseIn;

    #endregion

    #region BounceEaseInOut Static Property

    public static PennerEquation BounceEaseInOut
    {
      get
      {
        if( _bounceEaseInOut == null )
        {
          _bounceEaseInOut = new PennerEquation( BounceEaseInOutImpl );
        }
        return _bounceEaseInOut;
      }
    }

    private static PennerEquation _bounceEaseInOut;

    #endregion

    #region BounceEaseOut Static Property

    public static PennerEquation BounceEaseOut
    {
      get
      {
        if( _bounceEaseOut == null )
        {
          _bounceEaseOut = new PennerEquation( BounceEaseOutImpl );
        }
        return _bounceEaseOut;
      }
    }

    private static PennerEquation _bounceEaseOut;

    #endregion

    #region CircEaseIn Static Property

    public static PennerEquation CircEaseIn
    {
      get
      {
        if( _circEaseIn == null )
        {
          _circEaseIn = new PennerEquation( CircEaseInImpl );
        }
        return _circEaseIn;
      }
    }

    private static PennerEquation _circEaseIn;

    #endregion

    #region CircEaseInOut Static Property

    public static PennerEquation CircEaseInOut
    {
      get
      {
        if( _circEaseInOut == null )
        {
          _circEaseInOut = new PennerEquation( CircEaseInOutImpl );
        }
        return _circEaseInOut;
      }
    }

    private static PennerEquation _circEaseInOut;

    #endregion

    #region CircEaseOut Static Property

    public static PennerEquation CircEaseOut
    {
      get
      {
        if( _circEaseOut == null )
        {
          _circEaseOut = new PennerEquation( CircEaseOutImpl );
        }
        return _circEaseOut;
      }
    }

    private static PennerEquation _circEaseOut;

    #endregion

    #region CubicEaseIn Static Property

    public static PennerEquation CubicEaseIn
    {
      get
      {
        if( _cubicEaseIn == null )
        {
          _cubicEaseIn = new PennerEquation( CubicEaseInImpl );
        }
        return _cubicEaseIn;
      }
    }

    private static PennerEquation _cubicEaseIn;

    #endregion

    #region CubicEaseInOut Static Property

    public static PennerEquation CubicEaseInOut
    {
      get
      {
        if( _cubicEaseInOut == null )
        {
          _cubicEaseInOut = new PennerEquation( CubicEaseInOutImpl );
        }
        return _cubicEaseInOut;
      }
    }

    private static PennerEquation _cubicEaseInOut;

    #endregion

    #region CubicEaseOut Static Property

    public static PennerEquation CubicEaseOut
    {
      get
      {
        if( _cubicEaseOut == null )
        {
          _cubicEaseOut = new PennerEquation( CubicEaseOutImpl );
        }
        return _cubicEaseOut;
      }
    }

    private static PennerEquation _cubicEaseOut;

    #endregion

    #region ElasticEaseIn Static Property

    public static PennerEquation ElasticEaseIn
    {
      get
      {
        if( _elasticEaseIn == null )
        {
          _elasticEaseIn = new PennerEquation( ElasticEaseInImpl );
        }
        return _elasticEaseIn;
      }
    }

    private static PennerEquation _elasticEaseIn;

    #endregion

    #region ElasticEaseInOut Static Property

    public static PennerEquation ElasticEaseInOut
    {
      get
      {
        if( _elasticEaseInOut == null )
        {
          _elasticEaseInOut = new PennerEquation( ElasticEaseInOutImpl );
        }
        return _elasticEaseInOut;
      }
    }

    private static PennerEquation _elasticEaseInOut;

    #endregion

    #region ElasticEaseOut Static Property

    public static PennerEquation ElasticEaseOut
    {
      get
      {
        if( _elasticEaseOut == null )
        {
          _elasticEaseOut = new PennerEquation( ElasticEaseOutImpl );
        }
        return _elasticEaseOut;
      }
    }

    private static PennerEquation _elasticEaseOut;

    #endregion

    #region ExpoEaseIn Static Property

    public static PennerEquation ExpoEaseIn
    {
      get
      {
        if( _expoEaseIn == null )
        {
          _expoEaseIn = new PennerEquation( ExpoEaseInImpl );
        }
        return _expoEaseIn;
      }
    }

    private static PennerEquation _expoEaseIn;

    #endregion

    #region ExpoEaseInOut Static Property

    public static PennerEquation ExpoEaseInOut
    {
      get
      {
        if( _expoEaseInOut == null )
        {
          _expoEaseInOut = new PennerEquation( ExpoEaseInOutImpl );
        }
        return _expoEaseInOut;
      }
    }

    private static PennerEquation _expoEaseInOut;

    #endregion

    #region ExpoEaseOut Static Property

    public static PennerEquation ExpoEaseOut
    {
      get
      {
        if( _expoEaseOut == null )
        {
          _expoEaseOut = new PennerEquation( ExpoEaseOutImpl );
        }
        return _expoEaseOut;
      }
    }

    private static PennerEquation _expoEaseOut;

    #endregion

    #region Linear Static Property

    public static PennerEquation Linear
    {
      get
      {
        if( _linear == null )
        {
          _linear = new PennerEquation( LinearImpl );
        }
        return _linear;
      }
    }

    private static PennerEquation _linear;

    #endregion

    #region QuadEaseIn Static Property

    public static PennerEquation QuadEaseIn
    {
      get
      {
        if( _quadEaseIn == null )
        {
          _quadEaseIn = new PennerEquation( QuadEaseInImpl );
        }
        return _quadEaseIn;
      }
    }

    private static PennerEquation _quadEaseIn;

    #endregion

    #region QuadEaseInOut Static Property

    public static PennerEquation QuadEaseInOut
    {
      get
      {
        if( _quadEaseInOut == null )
        {
          _quadEaseInOut = new PennerEquation( QuadEaseInOutImpl );
        }
        return _quadEaseInOut;
      }
    }

    private static PennerEquation _quadEaseInOut;

    #endregion

    #region QuadEaseOut Static Property

    public static PennerEquation QuadEaseOut
    {
      get
      {
        if( _quadEaseOut == null )
        {
          _quadEaseOut = new PennerEquation( QuadEaseOutImpl );
        }
        return _quadEaseOut;
      }
    }

    private static PennerEquation _quadEaseOut;

    #endregion

    #region QuartEaseIn Static Property

    public static PennerEquation QuartEaseIn
    {
      get
      {
        if( _quartEaseIn == null )
        {
          _quartEaseIn = new PennerEquation( QuartEaseInImpl );
        }
        return _quartEaseIn;
      }
    }

    private static PennerEquation _quartEaseIn;

    #endregion

    #region QuartEaseInOut Static Property

    public static PennerEquation QuartEaseInOut
    {
      get
      {
        if( _quartEaseInOut == null )
        {
          _quartEaseInOut = new PennerEquation( QuartEaseInOutImpl );
        }
        return _quartEaseInOut;
      }
    }

    private static PennerEquation _quartEaseInOut;

    #endregion

    #region QuartEaseOut Static Property

    public static PennerEquation QuartEaseOut
    {
      get
      {
        if( _quartEaseOut == null )
        {
          _quartEaseOut = new PennerEquation( QuartEaseOutImpl );
        }
        return _quartEaseOut;
      }
    }

    private static PennerEquation _quartEaseOut;

    #endregion

    #region QuintEaseIn Static Property

    public static PennerEquation QuintEaseIn
    {
      get
      {
        if( _quintEaseIn == null )
        {
          _quintEaseIn = new PennerEquation( QuintEaseInImpl );
        }
        return _quintEaseIn;
      }
    }

    private static PennerEquation _quintEaseIn;

    #endregion

    #region QuintEaseInOut Static Property

    public static PennerEquation QuintEaseInOut
    {
      get
      {
        if( _quintEaseInOut == null )
        {
          _quintEaseInOut = new PennerEquation( QuintEaseInOutImpl );
        }
        return _quintEaseInOut;
      }
    }

    private static PennerEquation _quintEaseInOut;

    #endregion

    #region QuintEaseOut Static Property

    public static PennerEquation QuintEaseOut
    {
      get
      {
        if( _quintEaseOut == null )
        {
          _quintEaseOut = new PennerEquation( QuintEaseOutImpl );
        }
        return _quintEaseOut;
      }
    }

    private static PennerEquation _quintEaseOut;

    #endregion

    #region SineEaseIn Static Property

    public static PennerEquation SineEaseIn
    {
      get
      {
        if( _sineEaseIn == null )
        {
          _sineEaseIn = new PennerEquation( SineEaseInImpl );
        }
        return _sineEaseIn;
      }
    }

    private static PennerEquation _sineEaseIn;

    #endregion

    #region SineEaseInOut Static Property

    public static PennerEquation SineEaseInOut
    {
      get
      {
        if( _sineEaseInOut == null )
        {
          _sineEaseInOut = new PennerEquation( SineEaseInOutImpl );
        }
        return _sineEaseInOut;
      }
    }

    private static PennerEquation _sineEaseInOut;

    #endregion

    #region SineEaseOut Static Property

    public static PennerEquation SineEaseOut
    {
      get
      {
        if( _sineEaseOut == null )
        {
          _sineEaseOut = new PennerEquation( SineEaseOutImpl );
        }
        return _sineEaseOut;
      }
    }

    private static PennerEquation _sineEaseOut;

    #endregion

    #region Back Equations Methods

    private static double BackEaseOutImpl( double t, double b, double c, double d )
    {
      return c * ( ( t = t / d - 1 ) * t * ( ( 1.70158 + 1 ) * t + 1.70158 ) + 1 ) + b;
    }

    private static double BackEaseInImpl( double t, double b, double c, double d )
    {
      return c * ( t /= d ) * t * ( ( 1.70158 + 1 ) * t - 1.70158 ) + b;
    }

    private static double BackEaseInOutImpl( double t, double b, double c, double d )
    {
      double s = 1.70158;
      if( ( t /= d / 2 ) < 1 )
        return c / 2 * ( t * t * ( ( ( s *= ( 1.525 ) ) + 1 ) * t - s ) ) + b;
      return c / 2 * ( ( t -= 2 ) * t * ( ( ( s *= ( 1.525 ) ) + 1 ) * t + s ) + 2 ) + b;
    }

    #endregion

    #region Bounce Equations Methods

    private static double BounceEaseOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d ) < ( 1 / 2.75 ) )
      {
        return c * ( 7.5625 * t * t ) + b;
      }
      else if( t < ( 2 / 2.75 ) )
      {
        return c * ( 7.5625 * ( t -= ( 1.5 / 2.75 ) ) * t + .75 ) + b;
      }
      else if( t < ( 2.5 / 2.75 ) )
      {
        return c * ( 7.5625 * ( t -= ( 2.25 / 2.75 ) ) * t + .9375 ) + b;
      }
      else
      {
        return c * ( 7.5625 * ( t -= ( 2.625 / 2.75 ) ) * t + .984375 ) + b;
      }
    }

    private static double BounceEaseInImpl( double t, double b, double c, double d )
    {
      return c - PennerEquations.BounceEaseOutImpl( d - t, 0, c, d ) + b;
    }

    private static double BounceEaseInOutImpl( double t, double b, double c, double d )
    {
      if( t < d / 2 )
      {
        return PennerEquations.BounceEaseInImpl( t * 2, 0, c, d ) * .5 + b;
      }
      else
      {
        return PennerEquations.BounceEaseOutImpl( t * 2 - d, 0, c, d ) * .5 + c * .5 + b;
      }
    }

    #endregion

    #region Circular Equations Methods

    private static double CircEaseOutImpl( double t, double b, double c, double d )
    {
      return c * Math.Sqrt( 1 - ( t = t / d - 1 ) * t ) + b;
    }

    private static double CircEaseInImpl( double t, double b, double c, double d )
    {
      return -c * ( Math.Sqrt( 1 - ( t /= d ) * t ) - 1 ) + b;
    }

    private static double CircEaseInOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d / 2 ) < 1 )
        return -c / 2 * ( Math.Sqrt( 1 - t * t ) - 1 ) + b;

      return c / 2 * ( Math.Sqrt( 1 - ( t -= 2 ) * t ) + 1 ) + b;
    }

    #endregion

    #region Cubic Equations Methods

    private static double CubicEaseOutImpl( double t, double b, double c, double d )
    {
      return c * ( ( t = t / d - 1 ) * t * t + 1 ) + b;
    }

    private static double CubicEaseInImpl( double t, double b, double c, double d )
    {
      return c * ( t /= d ) * t * t + b;
    }

    private static double CubicEaseInOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d / 2 ) < 1 )
        return c / 2 * t * t * t + b;

      return c / 2 * ( ( t -= 2 ) * t * t + 2 ) + b;
    }

    #endregion

    #region Elastic Equations Methods

    private static double ElasticEaseOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d ) == 1 )
        return b + c;

      double p = d * .3;
      double s = p / 4;

      return ( c * Math.Pow( 2, -10 * t ) * Math.Sin( ( t * d - s ) * ( 2 * Math.PI ) / p ) + c + b );
    }

    private static double ElasticEaseInImpl( double t, double b, double c, double d )
    {
      if( ( t /= d ) == 1 )
        return b + c;

      double p = d * .3;
      double s = p / 4;

      return -( c * Math.Pow( 2, 10 * ( t -= 1 ) ) * Math.Sin( ( t * d - s ) * ( 2 * Math.PI ) / p ) ) + b;
    }

    private static double ElasticEaseInOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d / 2 ) == 2 )
        return b + c;

      double p = d * ( .3 * 1.5 );
      double s = p / 4;

      if( t < 1 )
        return -.5 * ( c * Math.Pow( 2, 10 * ( t -= 1 ) ) * Math.Sin( ( t * d - s ) * ( 2 * Math.PI ) / p ) ) + b;
      return c * Math.Pow( 2, -10 * ( t -= 1 ) ) * Math.Sin( ( t * d - s ) * ( 2 * Math.PI ) / p ) * .5 + c + b;
    }

    #endregion

    #region Expo Equations Methods

    private static double ExpoEaseOutImpl( double t, double b, double c, double d )
    {
      return ( t == d ) ? b + c : c * ( -Math.Pow( 2, -10 * t / d ) + 1 ) + b;
    }

    private static double ExpoEaseInImpl( double t, double b, double c, double d )
    {
      return ( t == 0 ) ? b : c * Math.Pow( 2, 10 * ( t / d - 1 ) ) + b;
    }

    private static double ExpoEaseInOutImpl( double t, double b, double c, double d )
    {
      if( t == 0 )
        return b;

      if( t == d )
        return b + c;

      if( ( t /= d / 2 ) < 1 )
        return c / 2 * Math.Pow( 2, 10 * ( t - 1 ) ) + b;

      return c / 2 * ( -Math.Pow( 2, -10 * --t ) + 2 ) + b;
    }

    #endregion

    #region Linear Equations Methods

    private static double LinearImpl( double t, double b, double c, double d )
    {
      return c * ( t / d ) + b;
    }

    #endregion

    #region Quad Equations Methods

    private static double QuadEaseOutImpl( double t, double b, double c, double d )
    {
      return -c * ( t /= d ) * ( t - 2 ) + b;
    }

    private static double QuadEaseInImpl( double t, double b, double c, double d )
    {
      return c * ( t /= d ) * t + b;
    }

    private static double QuadEaseInOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d / 2 ) < 1 )
        return c / 2 * t * t + b;

      return -c / 2 * ( ( --t ) * ( t - 2 ) - 1 ) + b;
    }

    #endregion

    #region Quartic Equations Methods

    private static double QuartEaseOutImpl( double t, double b, double c, double d )
    {
      return -c * ( ( t = t / d - 1 ) * t * t * t - 1 ) + b;
    }

    private static double QuartEaseInImpl( double t, double b, double c, double d )
    {
      return c * ( t /= d ) * t * t * t + b;
    }

    private static double QuartEaseInOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d / 2 ) < 1 )
        return c / 2 * t * t * t * t + b;

      return -c / 2 * ( ( t -= 2 ) * t * t * t - 2 ) + b;
    }

    #endregion

    #region Quintic Equations Methods

    private static double QuintEaseOutImpl( double t, double b, double c, double d )
    {
      return c * ( ( t = t / d - 1 ) * t * t * t * t + 1 ) + b;
    }

    private static double QuintEaseInImpl( double t, double b, double c, double d )
    {
      return c * ( t /= d ) * t * t * t * t + b;
    }

    private static double QuintEaseInOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d / 2 ) < 1 )
        return c / 2 * t * t * t * t * t + b;
      return c / 2 * ( ( t -= 2 ) * t * t * t * t + 2 ) + b;
    }

    #endregion

    #region Sine Equations Methods

    private static double SineEaseOutImpl( double t, double b, double c, double d )
    {
      return c * Math.Sin( t / d * ( Math.PI / 2 ) ) + b;
    }

    private static double SineEaseInImpl( double t, double b, double c, double d )
    {
      return -c * Math.Cos( t / d * ( Math.PI / 2 ) ) + c + b;
    }

    private static double SineEaseInOutImpl( double t, double b, double c, double d )
    {
      if( ( t /= d / 2 ) < 1 )
        return c / 2 * ( Math.Sin( Math.PI * t / 2 ) ) + b;

      return -c / 2 * ( Math.Cos( Math.PI * --t / 2 ) - 2 ) + b;
    }

    #endregion
  }
}
