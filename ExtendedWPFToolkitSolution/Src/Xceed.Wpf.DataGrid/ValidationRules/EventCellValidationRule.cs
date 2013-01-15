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
  public class EventCellValidationRule : CellValidationRule
  {
    public EventCellValidationRule()
    {
    }

    public override ValidationResult Validate( 
      object value,
      CultureInfo culture, 
      CellValidationContext context )
    {
      if( this.Validating != null )
      {
        CellValidatingEventArgs cellValidatingEventArgs = 
          new CellValidatingEventArgs( value, culture, context );

        this.Validating( this, cellValidatingEventArgs );
        return cellValidatingEventArgs.Result;
      }

      return ValidationResult.ValidResult;
    }

    public event EventHandler<CellValidatingEventArgs> Validating;
  }
}
