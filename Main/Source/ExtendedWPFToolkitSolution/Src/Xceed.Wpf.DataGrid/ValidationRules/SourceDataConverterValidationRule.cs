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
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Globalization;

using Xceed.Wpf.DataGrid.Converters;

namespace Xceed.Wpf.DataGrid.ValidationRules
{
  public class SourceDataConverterValidationRule : ValidationRule
  {
    public SourceDataConverterValidationRule( bool sourceSupportDBNull, Type targetType )
    {
      if( targetType == null )
        throw new ArgumentNullException( "targetType" );

      m_sourceSupportDBNull = sourceSupportDBNull;
      m_targetType = targetType;
    }

    public override ValidationResult Validate( object value, CultureInfo culture )
    {
      SourceDataConverter converter = new SourceDataConverter( m_sourceSupportDBNull );

      Exception exception;
      converter.TryConvertBack( value, m_targetType, CultureInfo.InvariantCulture, culture, out exception );

      if( exception != null )
        return new ValidationResult( false, exception.Message );

      return ValidationResult.ValidResult;
    }

    bool m_sourceSupportDBNull; // = false
    Type m_targetType; // = null
  }
}
