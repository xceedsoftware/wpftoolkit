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
using System.Globalization;
using System.Windows.Data;


namespace Xceed.Wpf.DataGrid.Converters
{
  public class TypeToBooleanConverter : IValueConverter
  {
    #region Singleton Property

    public static TypeToBooleanConverter Singleton
    {
      get
      {
        if( mg_singleton == null )
          mg_singleton = new TypeToBooleanConverter();

        return mg_singleton;
      }
    }

    private static TypeToBooleanConverter mg_singleton;

    #endregion Singleton Property

    #region IValueConverter Implementation

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( value == null ) || ( parameter == null ) )
        return false;

      Type typeToVerify = (Type)parameter;

      Type valueType = value.GetType();

      if( typeof(Type).IsAssignableFrom( valueType ) == true )
      {
        valueType = (Type) value;
      }

      bool retval = typeToVerify.IsAssignableFrom( valueType );

      return retval;
    }

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion IValueConverter Implementation
  }
}
