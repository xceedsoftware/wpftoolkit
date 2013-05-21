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
using System.Globalization;
using System.Windows.Controls;
using System.Windows;

namespace Xceed.Wpf.DataGrid.ValidationRules
{
  public class CellEditorErrorValidationRule : CellEditorValidationRule
  {
    public override ValidationResult Validate( object value, CultureInfo culture, CellValidationContext context, FrameworkElement cellEditor )
    {
      bool cellEditorHasError = ( cellEditor == null ) ? false : ( bool )cellEditor.GetValue( CellEditor.HasErrorProperty );

      if( !cellEditorHasError )
        return ValidationResult.ValidResult;

      return new ValidationResult( false, "An invalid or incomplete value was provided." );
    }
  }
}
