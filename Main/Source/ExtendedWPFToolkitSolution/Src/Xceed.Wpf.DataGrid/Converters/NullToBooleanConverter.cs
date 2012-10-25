/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( object ), typeof( bool ) )]
  public class NullToBooleanConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      // If value is null and no parameter is passed to converter, return true by default
      bool defaultNullReturnValue = true;

      if( parameter != null )
      {
        // Define the bool value to return if a null value is passed to the converter
        // this allows a NullToTrue or NullToFalse converter
        Boolean.TryParse( parameter.ToString(), out defaultNullReturnValue );
      }

      if( !targetType.IsAssignableFrom( typeof( bool ) ) )
        return DependencyProperty.UnsetValue;

      if( value == null )
      {
        return defaultNullReturnValue;
      }
      else
      {
        return !defaultNullReturnValue;
      }
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion
  }
}
