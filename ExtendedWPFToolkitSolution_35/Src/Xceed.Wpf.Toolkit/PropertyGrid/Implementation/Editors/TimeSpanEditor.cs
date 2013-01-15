/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Globalization;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class TimeSpanEditor : DateTimeUpDownEditor
  {
    private sealed class TimeSpanConverter : IValueConverter
    {
      object IValueConverter.Convert( object value, Type targetType, object parameter, CultureInfo culture )
      {
        return DateTime.Today + ( TimeSpan )value;
      }

      object IValueConverter.ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
      {
        return ( ( DateTime )value ).TimeOfDay;
      }
    }

    protected override void SetControlProperties()
    {
      Editor.Format = DateTimeFormat.LongTime;
    }

    protected override IValueConverter CreateValueConverter()
    {
      return new TimeSpanConverter();
    }
  }
}
