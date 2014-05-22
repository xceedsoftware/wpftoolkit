/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  public class ObjectTypeToNameConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value != null )
      {
        string valueString = value.ToString();
        if( string.IsNullOrEmpty( valueString )
         || ( valueString == value.GetType().UnderlyingSystemType.ToString() ) )
        {
          return value.GetType().Name;
        }
        return value; 
      }
      return null;
    }
    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
