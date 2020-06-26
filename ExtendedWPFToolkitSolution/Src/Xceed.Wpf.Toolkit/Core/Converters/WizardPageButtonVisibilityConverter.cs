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
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  public class WizardPageButtonVisibilityConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      if( values == null || values.Length != 2 )
        throw new ArgumentException( "Wrong number of arguments for WizardPageButtonVisibilityConverter." );

      Visibility wizardVisibility = ( (values[ 0 ] == null) || (values[ 0 ] == DependencyProperty.UnsetValue) )
                                    ? Visibility.Hidden
                                    : ( Visibility )values[ 0 ];

      WizardPageButtonVisibility wizardPageVisibility = ( (values[ 1 ] == null) || (values[ 1 ] == DependencyProperty.UnsetValue) )
                                                        ? WizardPageButtonVisibility.Hidden
                                                        : ( WizardPageButtonVisibility )values[ 1 ];

      Visibility visibility = Visibility.Visible;

      switch( wizardPageVisibility )
      {
        case WizardPageButtonVisibility.Inherit:
          visibility = wizardVisibility;
          break;
        case WizardPageButtonVisibility.Collapsed:
          visibility = Visibility.Collapsed;
          break;
        case WizardPageButtonVisibility.Hidden:
          visibility = Visibility.Hidden;
          break;
        case WizardPageButtonVisibility.Visible:
          visibility = Visibility.Visible;
          break;
      }

      return visibility;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
