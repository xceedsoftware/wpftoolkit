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
  public class SolidColorBrushToColorConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      SolidColorBrush brush = value as SolidColorBrush;
      if( brush != null )
        return brush.Color;

      return default( Color );
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value != null )
      {
        Color color = ( Color )value;
        return new SolidColorBrush( color );
      }

      return default( SolidColorBrush );
    }

    #endregion
  }
}
