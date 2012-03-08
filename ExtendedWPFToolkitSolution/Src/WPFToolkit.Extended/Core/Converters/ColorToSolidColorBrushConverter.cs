/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Windows.Controls.Core.Converters
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
