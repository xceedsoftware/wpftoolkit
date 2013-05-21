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
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Converters
{
  public class TypeToVisibilityConverter : IValueConverter
  {
    #region Visibility Property

    public Visibility Visibility
    {
      get
      {
        return m_visibility;
      }
      set
      {
        m_visibility = value;
      }
    }

    private Visibility m_visibility = Visibility.Visible;

    #endregion

    #region SetVisibilityWhenTrue Property

    public bool SetVisibilityWhenTrue
    {
      get
      {
        return m_setVisibilityWhenTrue;
      }
      set
      {
        m_setVisibilityWhenTrue = value;
      }
    }

    private bool m_setVisibilityWhenTrue = false;

    #endregion

    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      Type toCompareWith = parameter as Type;

      if( ( toCompareWith == null )
        || ( value == null ) )
      {
        return DependencyProperty.UnsetValue;
      }

      if( toCompareWith.IsAssignableFrom( value.GetType() ) == m_setVisibilityWhenTrue )
        return m_visibility;

      return DependencyProperty.UnsetValue;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      return Binding.DoNothing;
    }

    #endregion
  }
}
