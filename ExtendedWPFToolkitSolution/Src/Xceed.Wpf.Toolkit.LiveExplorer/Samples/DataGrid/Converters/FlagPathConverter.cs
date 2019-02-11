/*************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2017 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Xceed.Wpf.Samples.SampleData;
using System.Globalization;
using System.Windows;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.DataGrid.Converters
{
  public class FlagPathConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      // Use FlagsProvider to get Flags since it caches every BitmapImages it created.
      // This optimizes the converter.
      return FlagsProvider.Instance.GetFlagFromCountryName( value as string );
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return DependencyProperty.UnsetValue;
    }
  }
}
