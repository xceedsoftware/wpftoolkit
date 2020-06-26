/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2020 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Globalization;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  internal class CommonPropertyExceptionValidationRule : ValidationRule
  {
    private TypeConverter _propertyTypeConverter;
    private Type _type;

    internal CommonPropertyExceptionValidationRule( Type type )
    {
      _propertyTypeConverter = TypeDescriptor.GetConverter( type );
      _type = type;
    }

    public override ValidationResult Validate( object value, CultureInfo cultureInfo )
    {
      ValidationResult result = new ValidationResult( true, null );

      if( GeneralUtilities.CanConvertValue( value, _type ) )
      {
        try
        {
          _propertyTypeConverter.ConvertFrom( value );
        }
        catch( Exception e )
        {
          // Will display a red border in propertyGrid
          result = new ValidationResult( false, e.Message );
        }
      }
      return result;
    }
  }
}
