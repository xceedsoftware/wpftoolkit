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
using System.Windows.Data;
using System.Globalization;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.Converters
{
  /// <summary>
  /// This multi-value converter is used in the CollectionControl template
  /// to determine the list of possible new item types that will be shown in the combo box.
  /// 
  /// If the second value (i.e., CollectionControl.NewItemTypes) is not null, this list will be used.
  /// Otherwise, if the first value (i.e., CollectionControl.ItemsSourceType) is a "IList&lt;T&gt;"
  /// type, the new item type list will contain "T".
  /// 
  /// </summary>
  public class NewItemTypesComboBoxConverter : IMultiValueConverter
  {
    public object Convert( object[] values, Type targetType, object parameter, CultureInfo culture )
    {

      if( values.Length != 2 )
        throw new ArgumentException("The 'values' argument should contain 2 objects.");

      if( values[ 1 ] != null )
      {
        if( !values[ 1 ].GetType().IsGenericType || !(values[ 1 ].GetType().GetGenericArguments().First().GetType() is Type) )
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
