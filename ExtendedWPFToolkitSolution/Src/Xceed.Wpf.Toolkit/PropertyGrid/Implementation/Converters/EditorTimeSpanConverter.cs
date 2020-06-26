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
using System.Globalization;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Converters
{
  /// <summary>
  /// Converts a TimeSpan value to a DateTime value.
  /// 
  /// This converter can be used in conjunction with a TimePicker in order 
  /// to create a TimeSpan edit control. 
  /// </summary>
  public sealed class EditorTimeSpanConverter : IValueConverter
  {
    public bool AllowNulls { get; set; }

    object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( this.AllowNulls && value == null )
        return null;

      TimeSpan timeSpan = ( value != null ) ? ( TimeSpan )value : TimeSpan.Zero;
      return DateTime.Today + timeSpan;
    }

    object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( this.AllowNulls && value == null )
        return null;

      return ( value != null )
        ? ( ( DateTime )value ).TimeOfDay
        : TimeSpan.Zero;
    }
  }
}
