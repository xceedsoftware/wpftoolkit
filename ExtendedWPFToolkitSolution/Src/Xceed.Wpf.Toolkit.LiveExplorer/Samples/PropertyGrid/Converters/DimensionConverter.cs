/**************************************************************************************

   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md  

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Views;

namespace Xceed.Wpf.Toolkit.LiveExplorer.Samples.PropertyGrid.Converters
{
  public class DimensionsConverter : IValueConverter
  {
    static Dimension _originalValue; // the static struct that stores original value at the start of editing

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      _originalValue = ( ( Dimension )value );

      if( parameter.ToString() == "Height" )
        return ( ( Dimension )value ).Height;
      if( parameter.ToString() == "Weight" )
        return ( ( Dimension )value ).Weight;

      return _originalValue;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( parameter.ToString() == "Height" )
        _originalValue = new Dimension( double.Parse( value.ToString() ), _originalValue.Weight );
      if( parameter.ToString() == "Weight" )
        _originalValue = new Dimension( _originalValue.Height, double.Parse( value.ToString() ) );

      return _originalValue;
    }
  }
}
