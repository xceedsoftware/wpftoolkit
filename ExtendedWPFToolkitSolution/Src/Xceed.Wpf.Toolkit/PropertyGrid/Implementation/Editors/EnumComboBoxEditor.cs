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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class EnumComboBoxEditor : ComboBoxEditor
  {
    protected override IEnumerable CreateItemsSource( PropertyItem propertyItem )
    {
      return GetValues( propertyItem.PropertyType );
    }

    private static object[] GetValues( Type enumType )
    {
      List<object> values = new List<object>();

      if( enumType != null )
      {
        var fields = enumType.GetFields().Where( x => x.IsLiteral );
        foreach( FieldInfo field in fields )
        {
          // Get array of BrowsableAttribute attributes
          object[] attrs = field.GetCustomAttributes( typeof( BrowsableAttribute ), false );
          if( attrs.Length == 1 )
          {
            // If attribute exists and its value is false continue to the next field...
            BrowsableAttribute brAttr = ( BrowsableAttribute )attrs[ 0 ];
            if( brAttr.Browsable == false )
              continue;
          }

          values.Add( field.GetValue( enumType ) );
        }
      }

      return values.ToArray();
    }
  }
}
