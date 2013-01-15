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
using System.Windows.Controls;

namespace Xceed.Wpf.DataGrid.ValidationRules
{
  internal class FilterExpressionValidationRule : ValidationRule
  {
    public FilterExpressionValidationRule( Type dataType )
    {
      m_dataType = dataType;
    }

    public override ValidationResult Validate( object value, CultureInfo culture )
    {
      string content = value as string;

      if( !string.IsNullOrEmpty( content ) )
      {
        if( FilterParser.TryParse( content, m_dataType, CultureInfo.CurrentCulture ) == null )
          return new ValidationResult( false, FilterParser.LastError );
      }

      return ValidationResult.ValidResult;
    }

    private Type m_dataType;
  }
}
