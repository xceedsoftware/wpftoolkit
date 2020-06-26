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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  public class WindowControlBackgroundConverter : IMultiValueConverter
  {
    /// <summary>
    /// Used in the WindowContainer Template to calculate the resulting background brush
    /// from the WindowBackground (values[0]) and WindowOpacity (values[1]) propreties.
    /// </summary>
    /// <param name="values"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {
      Brush backgroundColor = ( Brush )values[ 0 ];
      double opacity = ( double )values[ 1 ];

      if( backgroundColor != null )
      {
        // Do not override any possible opacity value specifically set by the user.
        // Only use WindowOpacity value if the user did not set an opacity first.
        if( backgroundColor.ReadLocalValue( Brush.OpacityProperty ) == System.Windows.DependencyProperty.UnsetValue )
        {
          backgroundColor = backgroundColor.Clone();
          backgroundColor.Opacity = opacity;
        }
      }
      return backgroundColor;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
