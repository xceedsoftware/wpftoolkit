/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Globalization;
using System.Windows.Data;

using Xceed.Wpf.DataGrid.FilterCriteria;

namespace Xceed.Wpf.DataGrid.Converters
{
  [ValueConversion( typeof( FilterCriterion ), typeof( Nullable<bool> ) )]
  internal class FilterCriterionToForeignKeyConverter : IValueConverter
  {
    #region IValueConverter Members

    public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
    {
      EqualToFilterCriterion filterCriterion = value as EqualToFilterCriterion;

      if( filterCriterion != null )
      {
        return filterCriterion.Value;
      }

      return null;
    }

    public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
    {
      if( ( value == null ) || ( value == DBNull.Value ) )
      {
        return null;
      }
      else
      {
        return new EqualToFilterCriterion( value );
      }
    }

    #endregion
  }
}
