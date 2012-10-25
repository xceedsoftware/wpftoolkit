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

using System.Windows.Controls;
using System.Globalization;
using System;

namespace Xceed.Wpf.DataGrid.ValidationRules
{
  public class PassthroughCellValidationRule : CellValidationRule
  {
    public PassthroughCellValidationRule()
    {
    }

    public PassthroughCellValidationRule( ValidationRule validationRule )
    {
      m_validationRule = validationRule;
    }

    public override ValidationResult Validate( 
      object value, 
      CultureInfo culture, 
      CellValidationContext context )
    {
      if( m_validationRule == null )
        return ValidationResult.ValidResult;

      return m_validationRule.Validate( value, culture );
    }

    public ValidationRule ValidationRule
    {
      get
      {
        return m_validationRule;
      }

      set
      {
        m_validationRule = value;
      }
    }

    ValidationRule m_validationRule;
  }
}
