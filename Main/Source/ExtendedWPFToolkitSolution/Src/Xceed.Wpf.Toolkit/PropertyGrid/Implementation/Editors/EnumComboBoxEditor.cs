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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Editors
{
  public class EnumComboBoxEditor : ComboBoxEditor
  {
    protected override IList<object> CreateItemsSource( PropertyItem propertyItem )
    {
      return GetValues( propertyItem.PropertyType );
    }

    private static object[] GetValues( Type enumType )
    {
      List<object> values = new List<object>();

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

      return values.ToArray();
    }
  }
}
