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
using System.Windows.Data;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  public class ColorToSolidColorBrushConverter : IValueConverter
  {
    #region IValueConverter Members

    /// <summary>
    /// Converts a Color to a SolidColorBrush.
    /// </summary>
    /// <param name="value">The Color produced by the binding source.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A converted SolidColorBrush. If the method returns null, the valid null value is used.
    /// </returns>
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value != null )
        return new SolidColorBrush( ( Color )value );

      return value;
    }


    /// <summary>
    /// Converts a SolidColorBrush to a Color.
    /// </summary>
    /// <remarks>Currently not used in toolkit, but provided for developer use in their own projects</remarks>
    /// <param name="value">The SolidColorBrush that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="culture">The culture to use in the converter.</param>
    /// <returns>
    /// A converted value. If the method returns null, the valid null value is used.
    /// </returns>
    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value != null )
        return ( ( SolidColorBrush )value ).Color;

      return value;
    }

    #endregion
  }
}
