/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Reciprocal
   License (Ms-RL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Windows;
using System.Windows.Data;

namespace Xceed.Wpf.Toolkit.Core.Converters
{
  public class WizardPageButtonVisibilityConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      Visibility wizardVisibility = ( Visibility )values[ 0 ];
      WizardPageButtonVisibility wizardPageVisibility = ( WizardPageButtonVisibility )values[ 1 ];

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
