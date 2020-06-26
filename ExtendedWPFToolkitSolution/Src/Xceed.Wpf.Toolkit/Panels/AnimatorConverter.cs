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
using Xceed.Wpf.Toolkit.Panels;

namespace Xceed.Wpf.Toolkit.Panels
{
  public sealed class AnimatorConverter : TypeConverter
  {
    public override bool CanConvertFrom( ITypeDescriptorContext typeDescriptorContext, Type type )
    {
      return ( type == typeof( string ) );
    }

    public override bool CanConvertTo( ITypeDescriptorContext typeDescriptorContext, Type type )
    {
      return ( type == typeof( IterativeAnimator ) )
          || ( type == typeof( DoubleAnimator ) );
    }

    public override object ConvertFrom( 
      ITypeDescriptorContext typeDescriptorContext, CultureInfo cultureInfo, object value )
    {
      IterativeAnimator result = null;
      if( value is string )
      {
        switch( value as string )
        {
          case "BackEaseIn":
            result = Animators.BackEaseIn as IterativeAnimator;
            break;
          case "BackEaseInOut":
            result = Animators.BackEaseInOut as IterativeAnimator;
            break;
          case "BackEaseOut":
            result = Animators.BackEaseOut as IterativeAnimator;
            break;
          case "BounceEaseIn":
            result = Animators.BounceEaseIn as IterativeAnimator;
            break;
          case "BounceEaseInOut":
            result = Animators.BounceEaseInOut as IterativeAnimator;
            break;
          case "BounceEaseOut":
            result = Animators.BounceEaseOut as IterativeAnimator;
            break;
          case "CircEaseIn":
            result = Animators.CircEaseIn as IterativeAnimator;
            break;
          case "CircEaseInOut":
            result = Animators.CircEaseInOut as IterativeAnimator;
            break;
          case "CircEaseOut":
            result = Animators.CircEaseOut as IterativeAnimator;
            break;
          case "CubicEaseIn":
            result = Animators.CubicEaseIn as IterativeAnimator;
            break;
          case "CubicEaseInOut":
            result = Animators.CubicEaseInOut as IterativeAnimator;
            break;
          case "CubicEaseOut":
            result = Animators.CubicEaseOut as IterativeAnimator;
            break;
          case "ElasticEaseIn":
            result = Animators.ElasticEaseIn as IterativeAnimator;
            break;
          case "ElasticEaseInOut":
            result = Animators.ElasticEaseInOut as IterativeAnimator;
            break;
          case "ElasticEaseOut":
            result = Animators.ElasticEaseOut as IterativeAnimator;
            break;
          case "ExpoEaseIn":
            result = Animators.ExpoEaseIn as IterativeAnimator;
            break;
          case "ExpoEaseInOut":
            result = Animators.ExpoEaseInOut as IterativeAnimator;
            break;
          case "ExpoEaseOut":
            result = Animators.ExpoEaseOut as IterativeAnimator;
            break;
          case "Linear":
            result = Animators.Linear as IterativeAnimator;
            break;
          case "QuadEaseIn":
            result = Animators.QuadEaseIn as IterativeAnimator;
            break;
          case "QuadEaseInOut":
            result = Animators.QuadEaseInOut as IterativeAnimator;
            break;
          case "QuadEaseOut":
            result = Animators.QuadEaseOut as IterativeAnimator;
            break;
          case "QuartEaseIn":
            result = Animators.QuartEaseIn as IterativeAnimator;
            break;
          case "QuartEaseInOut":
            result = Animators.QuartEaseInOut as IterativeAnimator;
            break;
          case "QuartEaseOut":
            result = Animators.QuartEaseOut as IterativeAnimator;
            break;
          case "QuintEaseIn":
            result = Animators.QuintEaseIn as IterativeAnimator;
            break;
          case "QuintEaseInOut":
            result = Animators.QuintEaseInOut as IterativeAnimator;
            break;
          case "QuintEaseOut":
            result = Animators.QuintEaseOut as IterativeAnimator;
            break;
          case "SineEaseIn":
            result = Animators.SineEaseIn as IterativeAnimator;
            break;
          case "SineEaseInOut":
            result = Animators.SineEaseInOut as IterativeAnimator;
            break;
          case "SineEaseOut":
            result = Animators.SineEaseOut as IterativeAnimator;
            break;
        }
      }

      return result;
    }
  }
}
