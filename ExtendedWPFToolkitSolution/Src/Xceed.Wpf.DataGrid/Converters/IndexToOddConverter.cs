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
using System.Windows;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( int ), typeof( bool ) )]
  public class IndexToOddConverter : IValueConverter
  {
    #region Singleton Property

    public static IndexToOddConverter Singleton
    {
      get
      {
        if( mg_singleton == null )
          mg_singleton = new IndexToOddConverter();

        return mg_singleton;
      }
    }

    private static IndexToOddConverter mg_singleton;

    #endregion Singleton Property

    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( !targetType.IsAssignableFrom( typeof( bool ) ) )
        || ( value == null )
        || ( value.GetType() != typeof( int ) ) )
      {
        return DependencyProperty.UnsetValue;
      }

      int index = ( int )value;

      return ( ( index % 2 ) == 1 );
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion
  }
}
