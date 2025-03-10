/*************************************************************************************
   
   Toolkit for WPF

   Copyright (C) 2007-2025 Xceed Software Inc.

   This program is provided to you under the terms of the XCEED SOFTWARE, INC.
   COMMUNITY LICENSE AGREEMENT (for non-commercial use) as published at 
   https://github.com/xceedsoftware/wpftoolkit/blob/master/license.md 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at https://xceed.com/xceed-toolkit-plus-for-wpf/

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Converters
{
  public class NewItemTypesComboBoxConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {

      if( values.Length != 2 )
        throw new ArgumentException( "The 'values' argument should contain 2 objects." );

      if( values[ 1 ] != null )
      {
        if( !values[ 1 ].GetType().IsGenericType || !( values[ 1 ].GetType().GetGenericArguments().First().GetType() is Type ) )
          throw new ArgumentException( "The 'value' argument is not of the correct type." );

        return values[ 1 ];
      }
      else if( values[ 0 ] != null )
      {
        if( !( values[ 0 ].GetType() is Type ) )
          throw new ArgumentException( "The 'value' argument is not of the correct type." );

        List<Type> types = new List<Type>();
        Type listType = ListUtilities.GetListItemType( ( Type )values[ 0 ] );
        if( listType != null )
        {
          types.Add( listType );
        }

        return types;
      }

      return null;
    }

    public object[] ConvertBack( object value, Type[] targetTypes, object parameter, CultureInfo culture )
    {
      throw new NotImplementedException();
    }
  }
}
