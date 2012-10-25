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

namespace Xceed.Wpf.DataGrid.Converters
{
  internal class SynchronizedScrollViewerMultiConverter : IMultiValueConverter
  {
    #region IMultiValueConverter Members

    public object Convert( object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      double largestValue = 0d;

      foreach( double value in values )
      {
        if( value > largestValue )
        {
          largestValue = value;
        }
      }

      return largestValue;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture )
    {
      return null;
    }

    #endregion
  }
}
