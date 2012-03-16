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

namespace Microsoft.Windows.Controls.PropertyGrid.Converters
{
  public class ValueSourceToToolTipConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      BaseValueSource bvs = ( BaseValueSource )value;
      string toolTip = "Advanced Properties";

      switch( bvs )
      {
        case BaseValueSource.Inherited:
        case BaseValueSource.DefaultStyle:
        case BaseValueSource.ImplicitStyleReference:
          toolTip = "Inheritance";
          break;
        case BaseValueSource.Style:
          toolTip = "Style Setter";
          break;

        case BaseValueSource.Local:
          toolTip = "Local";
          break;
      }

      return toolTip;
    }

    public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
    {
      throw new NotImplementedException();
    }

    #endregion
  }
}
