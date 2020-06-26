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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal class ListUtilities
  {
    internal static Type GetListItemType( Type listType )
    {
      Type iListOfT = listType.GetInterfaces().FirstOrDefault(
        ( i ) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof( IList<> ) );

      return ( iListOfT != null )
        ? iListOfT.GetGenericArguments()[ 0 ]
        : null;
    }

    internal static Type GetCollectionItemType( Type colType )
    {
      Type iCollectionOfT = null;
      var isCollectionOfT = colType.IsGenericType && (colType.GetGenericTypeDefinition() == typeof( ICollection<> ) );
      if( isCollectionOfT )
      {
        iCollectionOfT = colType;
      }
      else
      {
        iCollectionOfT = colType.GetInterfaces().FirstOrDefault(( i ) => i.IsGenericType && i.GetGenericTypeDefinition() == typeof( ICollection<> ) );
      }

      return (iCollectionOfT != null)
        ? iCollectionOfT.GetGenericArguments()[ 0 ]
        : null;
    }

    internal static Type[] GetDictionaryItemsType( Type dictType )
    {
      var isDict = dictType.IsGenericType 
        && ((dictType.GetGenericTypeDefinition() == typeof( Dictionary<,>) ) || (dictType.GetGenericTypeDefinition() == typeof( IDictionary<,>) ));

      return isDict
        ? new Type[] { dictType.GetGenericArguments()[ 0 ], dictType.GetGenericArguments()[ 1 ] }
        : null;
    }

    internal static object CreateEditableKeyValuePair( object key, Type keyType, object value, Type valueType )
    {
      var itemType = ListUtilities.CreateEditableKeyValuePairType( keyType, valueType );
      return Activator.CreateInstance( itemType, key, value );
    }

    internal static Type CreateEditableKeyValuePairType( Type keyType, Type valueType )
    {
      //return an EditableKeyValuePair< TKey, TValue> Type from keyType and valueType
      var itemGenType = typeof( EditableKeyValuePair<,> );
      Type[] itemGenTypeArgs = { keyType, valueType };
      return itemGenType.MakeGenericType( itemGenTypeArgs );
    }
  }
}
