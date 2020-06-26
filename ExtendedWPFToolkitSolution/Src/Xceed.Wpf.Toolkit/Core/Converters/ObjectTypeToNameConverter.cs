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
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  public class ObjectTypeToNameConverter : IValueConverter
  {
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value != null )
      {
        if( value is Type )
        {
          var displayNameAttribute = ( ( Type )value ).GetCustomAttributes( false ).OfType<DisplayNameAttribute>().FirstOrDefault();
          return ( displayNameAttribute != null ) ? displayNameAttribute.DisplayName : ( ( Type )value ).Name;
        }

        var type = value.GetType();
        var valueString = value.ToString();
        if( string.IsNullOrEmpty( valueString )
         || ( valueString == type.UnderlyingSystemType.ToString() ) )
        {
          var displayNameAttribute = type.GetCustomAttributes( false ).OfType<DisplayNameAttribute>().FirstOrDefault();
          return ( displayNameAttribute != null ) ? displayNameAttribute.DisplayName : type.Name;
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
