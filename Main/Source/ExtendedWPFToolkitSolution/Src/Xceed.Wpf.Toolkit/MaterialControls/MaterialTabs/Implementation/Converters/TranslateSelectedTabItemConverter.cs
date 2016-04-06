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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit.Converters
{
  public class TranslateSelectedTabItemConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {
      double width = (double)values[0];
      double height = (double)values[1];
      Dock position = (Dock)values[2];

      if( parameter.ToString().Equals( "Previous" ) )
      {
        if( (position == Dock.Top) || (position == Dock.Bottom) )
          return new TranslateTransform( -width, 0 );
        else
          return new TranslateTransform( 0, -height );
      }
      else if( parameter.ToString().Equals( "Next" ) )
      {
        if( (position == Dock.Top) || (position == Dock.Bottom) )
          return new TranslateTransform( width, 0 );
        else
          return new TranslateTransform( 0, height );
      }

      return null;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
