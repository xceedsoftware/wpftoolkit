/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.AvalonDock.Converters
{
  [ValueConversion( typeof( bool ), typeof( Visibility ) )]
  public class InverseBoolToVisibilityConverter : IValueConverter
  {

    #region IValueConverter Members 
    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value is bool && targetType == typeof( Visibility ) )
      {
        bool val = !( bool )value;
        if( val )
          return Visibility.Visible;
        else
                if( parameter != null && parameter is Visibility )
          return parameter;
        else
          return Visibility.Collapsed;
      }
      throw new ArgumentException( "Invalid argument/return type. Expected argument: bool and return type: Visibility" );
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( value is Visibility && targetType == typeof( bool ) )
      {
        Visibility val = ( Visibility )value;
        if( val == Visibility.Visible )
          return false;
        else
          return true;
      }
      throw new ArgumentException( "Invalid argument/return type. Expected argument: Visibility and return type: bool" );
    }
    #endregion
  }
}
