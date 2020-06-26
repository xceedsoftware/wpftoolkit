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

using Xceed.Wpf.Toolkit.Media.Animation;

namespace Xceed.Wpf.Toolkit.Panels
{
  public static class Animators
  {
    #region BackEaseIn Static Property

    public static DoubleAnimator BackEaseIn
    {
      get
      {
        if( _backEaseIn == null )
        {
          _backEaseIn = new DoubleAnimator( PennerEquations.BackEaseIn );
        }
        return _backEaseIn;
      }
    }

    private static DoubleAnimator _backEaseIn;

    #endregion

    #region BackEaseInOut Static Property

    public static DoubleAnimator BackEaseInOut
    {
      get
      {
        if( _backEaseInOut == null )
        {
          _backEaseInOut = new DoubleAnimator( PennerEquations.BackEaseInOut );
        }
        return _backEaseInOut;
      }
    }

    private static DoubleAnimator _backEaseInOut;

    #endregion

    #region BackEaseOut Static Property

    public static DoubleAnimator BackEaseOut
    {
      get
      {
        if( _backEaseOut == null )
        {
          _backEaseOut = new DoubleAnimator( PennerEquations.BackEaseOut );
        }
        return _backEaseOut;
      }
    }

    private static DoubleAnimator _backEaseOut;

    #endregion

    #region BounceEaseIn Static Property

    public static DoubleAnimator BounceEaseIn
    {
      get
      {
        if( _bounceEaseIn == null )
        {
          _bounceEaseIn = new DoubleAnimator( PennerEquations.BounceEaseIn );
        }
        return _bounceEaseIn;
      }
    }

    private static DoubleAnimator _bounceEaseIn;

    #endregion

    #region BounceEaseInOut Static Property

    public static DoubleAnimator BounceEaseInOut
    {
      get
      {
        if( _bounceEaseInOut == null )
        {
          _bounceEaseInOut = new DoubleAnimator( PennerEquations.BounceEaseInOut );
        }
        return _bounceEaseInOut;
      }
    }

    private static DoubleAnimator _bounceEaseInOut;

    #endregion

    #region BounceEaseOut Static Property

    public static DoubleAnimator BounceEaseOut
    {
      get
      {
        if( _bounceEaseOut == null )
        {
          _bounceEaseOut = new DoubleAnimator( PennerEquations.BounceEaseOut );
        }
        return _bounceEaseOut;
      }
    }

    private static DoubleAnimator _bounceEaseOut;

    #endregion

    #region CircEaseIn Static Property

    public static DoubleAnimator CircEaseIn
    {
      get
      {
        if( _circEaseIn == null )
        {
          _circEaseIn = new DoubleAnimator( PennerEquations.CircEaseIn );
        }
        return _circEaseIn;
      }
    }

    private static DoubleAnimator _circEaseIn;

    #endregion

    #region CircEaseInOut Static Property

    public static DoubleAnimator CircEaseInOut
    {
      get
      {
        if( _circEaseInOut == null )
        {
          _circEaseInOut = new DoubleAnimator( PennerEquations.CircEaseInOut );
        }
        return _circEaseInOut;
      }
    }

    private static DoubleAnimator _circEaseInOut;

    #endregion

    #region CircEaseOut Static Property

    public static DoubleAnimator CircEaseOut
    {
      get
      {
        if( _circEaseOut == null )
        {
          _circEaseOut = new DoubleAnimator( PennerEquations.CircEaseOut );
        }
        return _circEaseOut;
      }
    }

    private static DoubleAnimator _circEaseOut;

    #endregion

    #region CubicEaseIn Static Property

    public static DoubleAnimator CubicEaseIn
    {
      get
      {
        if( _cubicEaseIn == null )
        {
          _cubicEaseIn = new DoubleAnimator( PennerEquations.CubicEaseIn );
        }
        return _cubicEaseIn;
      }
    }

    private static DoubleAnimator _cubicEaseIn;

    #endregion

    #region CubicEaseInOut Static Property

    public static DoubleAnimator CubicEaseInOut
    {
      get
      {
        if( _cubicEaseInOut == null )
        {
          _cubicEaseInOut = new DoubleAnimator( PennerEquations.CubicEaseInOut );
        }
        return _cubicEaseInOut;
      }
    }

    private static DoubleAnimator _cubicEaseInOut;

    #endregion

    #region CubicEaseOut Static Property

    public static DoubleAnimator CubicEaseOut
    {
      get
      {
        if( _cubicEaseOut == null )
        {
          _cubicEaseOut = new DoubleAnimator( PennerEquations.CubicEaseOut );
        }
        return _cubicEaseOut;
      }
    }

    private static DoubleAnimator _cubicEaseOut;

    #endregion

    #region ElasticEaseIn Static Property

    public static DoubleAnimator ElasticEaseIn
    {
      get
      {
        if( _elasticEaseIn == null )
        {
          _elasticEaseIn = new DoubleAnimator( PennerEquations.ElasticEaseIn );
        }
        return _elasticEaseIn;
      }
    }

    private static DoubleAnimator _elasticEaseIn;

    #endregion

    #region ElasticEaseInOut Static Property

    public static DoubleAnimator ElasticEaseInOut
    {
      get
      {
        if( _elasticEaseInOut == null )
        {
          _elasticEaseInOut = new DoubleAnimator( PennerEquations.ElasticEaseInOut );
        }
        return _elasticEaseInOut;
      }
    }

    private static DoubleAnimator _elasticEaseInOut;

    #endregion

    #region ElasticEaseOut Static Property

    public static DoubleAnimator ElasticEaseOut
    {
      get
      {
        if( _elasticEaseOut == null )
        {
          _elasticEaseOut = new DoubleAnimator( PennerEquations.ElasticEaseOut );
        }
        return _elasticEaseOut;
      }
    }

    private static DoubleAnimator _elasticEaseOut;

    #endregion

    #region ExpoEaseIn Static Property

    public static DoubleAnimator ExpoEaseIn
    {
      get
      {
        if( _expoEaseIn == null )
        {
          _expoEaseIn = new DoubleAnimator( PennerEquations.ExpoEaseIn );
        }
        return _expoEaseIn;
      }
    }

    private static DoubleAnimator _expoEaseIn;

    #endregion

    #region ExpoEaseInOut Static Property

    public static DoubleAnimator ExpoEaseInOut
    {
      get
      {
        if( _expoEaseInOut == null )
        {
          _expoEaseInOut = new DoubleAnimator( PennerEquations.ExpoEaseInOut );
        }
        return _expoEaseInOut;
      }
    }

    private static DoubleAnimator _expoEaseInOut;

    #endregion

    #region ExpoEaseOut Static Property

    public static DoubleAnimator ExpoEaseOut
    {
      get
      {
        if( _expoEaseOut == null )
        {
          _expoEaseOut = new DoubleAnimator( PennerEquations.ExpoEaseOut );
        }
        return _expoEaseOut;
      }
    }

    private static DoubleAnimator _expoEaseOut;

    #endregion

    #region Linear Static Property

    public static DoubleAnimator Linear
    {
      get
      {
        if( _linear == null )
        {
          _linear = new DoubleAnimator( PennerEquations.Linear );
        }
        return _linear;
      }
    }

    private static DoubleAnimator _linear;

    #endregion

    #region QuadEaseIn Static Property

    public static DoubleAnimator QuadEaseIn
    {
      get
      {
        if( _quadEaseIn == null )
        {
          _quadEaseIn = new DoubleAnimator( PennerEquations.QuadEaseIn );
        }
        return _quadEaseIn;
      }
    }

    private static DoubleAnimator _quadEaseIn;

    #endregion

    #region QuadEaseInOut Static Property

    public static DoubleAnimator QuadEaseInOut
    {
      get
      {
        if( _quadEaseInOut == null )
        {
          _quadEaseInOut = new DoubleAnimator( PennerEquations.QuadEaseInOut );
        }
        return _quadEaseInOut;
      }
    }

    private static DoubleAnimator _quadEaseInOut;

    #endregion

    #region QuadEaseOut Static Property

    public static DoubleAnimator QuadEaseOut
    {
      get
      {
        if( _quadEaseOut == null )
        {
          _quadEaseOut = new DoubleAnimator( PennerEquations.QuadEaseOut );
        }
        return _quadEaseOut;
      }
    }

    private static DoubleAnimator _quadEaseOut;

    #endregion

    #region QuartEaseIn Static Property

    public static DoubleAnimator QuartEaseIn
    {
      get
      {
        if( _quartEaseIn == null )
        {
          _quartEaseIn = new DoubleAnimator( PennerEquations.QuartEaseIn );
        }
        return _quartEaseIn;
      }
    }

    private static DoubleAnimator _quartEaseIn;

    #endregion

    #region QuartEaseInOut Static Property

    public static DoubleAnimator QuartEaseInOut
    {
      get
      {
        if( _quartEaseInOut == null )
        {
          _quartEaseInOut = new DoubleAnimator( PennerEquations.QuartEaseInOut );
        }
        return _quartEaseInOut;
      }
    }

    private static DoubleAnimator _quartEaseInOut;

    #endregion

    #region QuartEaseOut Static Property

    public static DoubleAnimator QuartEaseOut
    {
      get
      {
        if( _quartEaseOut == null )
        {
          _quartEaseOut = new DoubleAnimator( PennerEquations.QuartEaseOut );
        }
        return _quartEaseOut;
      }
    }

    private static DoubleAnimator _quartEaseOut;

    #endregion

    #region QuintEaseIn Static Property

    public static DoubleAnimator QuintEaseIn
    {
      get
      {
        if( _quintEaseIn == null )
        {
          _quintEaseIn = new DoubleAnimator( PennerEquations.QuintEaseIn );
        }
        return _quintEaseIn;
      }
    }

    private static DoubleAnimator _quintEaseIn;

    #endregion

    #region QuintEaseInOut Static Property

    public static DoubleAnimator QuintEaseInOut
    {
      get
      {
        if( _quintEaseInOut == null )
        {
          _quintEaseInOut = new DoubleAnimator( PennerEquations.QuintEaseInOut );
        }
        return _quintEaseInOut;
      }
    }

    private static DoubleAnimator _quintEaseInOut;

    #endregion

    #region QuintEaseOut Static Property

    public static DoubleAnimator QuintEaseOut
    {
      get
      {
        if( _quintEaseOut == null )
        {
          _quintEaseOut = new DoubleAnimator( PennerEquations.QuintEaseOut );
        }
        return _quintEaseOut;
      }
    }

    private static DoubleAnimator _quintEaseOut;

    #endregion

    #region SineEaseIn Static Property

    public static DoubleAnimator SineEaseIn
    {
      get
      {
        if( _sineEaseIn == null )
        {
          _sineEaseIn = new DoubleAnimator( PennerEquations.SineEaseIn );
        }
        return _sineEaseIn;
      }
    }

    private static DoubleAnimator _sineEaseIn;

    #endregion

    #region SineEaseInOut Static Property

    public static DoubleAnimator SineEaseInOut
    {
      get
      {
        if( _sineEaseInOut == null )
        {
          _sineEaseInOut = new DoubleAnimator( PennerEquations.SineEaseInOut );
        }
        return _sineEaseInOut;
      }
    }

    private static DoubleAnimator _sineEaseInOut;

    #endregion

    #region SineEaseOut Static Property

    public static DoubleAnimator SineEaseOut
    {
      get
      {
        if( _sineEaseOut == null )
        {
          _sineEaseOut = new DoubleAnimator( PennerEquations.SineEaseOut );
        }
        return _sineEaseOut;
      }
    }

    private static DoubleAnimator _sineEaseOut;

    #endregion
  }
}
