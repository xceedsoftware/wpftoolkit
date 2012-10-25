/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

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
