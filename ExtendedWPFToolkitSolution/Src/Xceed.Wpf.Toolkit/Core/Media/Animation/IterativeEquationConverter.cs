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
using System.Globalization;

namespace Xceed.Wpf.Toolkit.Media.Animation
{
  public class IterativeEquationConverter : TypeConverter
  {
    public override bool CanConvertFrom( ITypeDescriptorContext typeDescriptorContext, Type type )
    {
      return type == typeof( string );
    }

    public override bool CanConvertTo( ITypeDescriptorContext typeDescriptorContext, Type type )
    {
      return type == typeof( IterativeEquation<double> );
    }

    public override object ConvertFrom( 
      ITypeDescriptorContext typeDescriptorContext,
      CultureInfo cultureInfo, 
      object value )
    {
      IterativeEquation<double> result = null;

      if( value is string )
      {
        switch( value as string )
        {
          case "BackEaseIn":
            result = PennerEquations.BackEaseIn as IterativeEquation<double>;
            break;
          case "BackEaseInOut":
            result = PennerEquations.BackEaseInOut as IterativeEquation<double>;
            break;
          case "BackEaseOut":
            result = PennerEquations.BackEaseOut as IterativeEquation<double>;
            break;
          case "BounceEaseIn":
            result = PennerEquations.BounceEaseIn as IterativeEquation<double>;
            break;
          case "BounceEaseInOut":
            result = PennerEquations.BounceEaseInOut as IterativeEquation<double>;
            break;
          case "BounceEaseOut":
            result = PennerEquations.BounceEaseOut as IterativeEquation<double>;
            break;
          case "CircEaseIn":
            result = PennerEquations.CircEaseIn as IterativeEquation<double>;
            break;
          case "CircEaseInOut":
            result = PennerEquations.CircEaseInOut as IterativeEquation<double>;
            break;
          case "CircEaseOut":
            result = PennerEquations.CircEaseOut as IterativeEquation<double>;
            break;
          case "CubicEaseIn":
            result = PennerEquations.CubicEaseIn as IterativeEquation<double>;
            break;
          case "CubicEaseInOut":
            result = PennerEquations.CubicEaseInOut as IterativeEquation<double>;
            break;
          case "CubicEaseOut":
            result = PennerEquations.CubicEaseOut as IterativeEquation<double>;
            break;
          case "ElasticEaseIn":
            result = PennerEquations.ElasticEaseIn as IterativeEquation<double>;
            break;
          case "ElasticEaseInOut":
            result = PennerEquations.ElasticEaseInOut as IterativeEquation<double>;
            break;
          case "ElasticEaseOut":
            result = PennerEquations.ElasticEaseOut as IterativeEquation<double>;
            break;
          case "ExpoEaseIn":
            result = PennerEquations.ExpoEaseIn as IterativeEquation<double>;
            break;
          case "ExpoEaseInOut":
            result = PennerEquations.ExpoEaseInOut as IterativeEquation<double>;
            break;
          case "ExpoEaseOut":
            result = PennerEquations.ExpoEaseOut as IterativeEquation<double>;
            break;
          case "Linear":
            result = PennerEquations.Linear as IterativeEquation<double>;
            break;
          case "QuadEaseIn":
            result = PennerEquations.QuadEaseIn as IterativeEquation<double>;
            break;
          case "QuadEaseInOut":
            result = PennerEquations.QuadEaseInOut as IterativeEquation<double>;
            break;
          case "QuadEaseOut":
            result = PennerEquations.QuadEaseOut as IterativeEquation<double>;
            break;
          case "QuartEaseIn":
            result = PennerEquations.QuartEaseIn as IterativeEquation<double>;
            break;
          case "QuartEaseInOut":
            result = PennerEquations.QuartEaseInOut as IterativeEquation<double>;
            break;
          case "QuartEaseOut":
            result = PennerEquations.QuartEaseOut as IterativeEquation<double>;
            break;
          case "QuintEaseIn":
            result = PennerEquations.QuintEaseIn as IterativeEquation<double>;
            break;
          case "QuintEaseInOut":
            result = PennerEquations.QuintEaseInOut as IterativeEquation<double>;
            break;
          case "QuintEaseOut":
            result = PennerEquations.QuintEaseOut as IterativeEquation<double>;
            break;
          case "SineEaseIn":
            result = PennerEquations.SineEaseIn as IterativeEquation<double>;
            break;
          case "SineEaseInOut":
            result = PennerEquations.SineEaseInOut as IterativeEquation<double>;
            break;
          case "SineEaseOut":
            result = PennerEquations.SineEaseOut as IterativeEquation<double>;
            break;
        }
      }

      return result;
    }
  }
}
