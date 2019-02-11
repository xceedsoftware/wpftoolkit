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
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal static class ChangeTypeHelper
  {
    internal static object ChangeType( object value, Type conversionType, IFormatProvider provider )
    {
      if( conversionType == null )
      {
        throw new ArgumentNullException( "conversionType" );
      }
      if( conversionType == typeof( Guid ) )
      {
        return new Guid( value.ToString() );
      }
      else if( conversionType == typeof( Guid? ) )
      {
        if( value == null )
          return null;
        return new Guid( value.ToString() );
      }
      else if( conversionType.IsGenericType && conversionType.GetGenericTypeDefinition().Equals( typeof( Nullable<> ) ) )
      {
        if( value == null )
          return null;
        NullableConverter nullableConverter = new NullableConverter( conversionType );
        conversionType = nullableConverter.UnderlyingType;
      }

      return System.Convert.ChangeType( value, conversionType, provider );
    }
  }
}
