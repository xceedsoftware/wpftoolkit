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
