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

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( string ), typeof( bool ) )]
  public class EmptyStringToBooleanConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      string strValue = value as string;

      // Not a mistake: returns true if *value* is null or *strValue* is "".
      return ( value == null ) || ( strValue == string.Empty );
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return Binding.DoNothing;
    }
  }
}
