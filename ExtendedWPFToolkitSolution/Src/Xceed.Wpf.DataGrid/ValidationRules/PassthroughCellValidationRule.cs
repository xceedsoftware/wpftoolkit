/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

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
