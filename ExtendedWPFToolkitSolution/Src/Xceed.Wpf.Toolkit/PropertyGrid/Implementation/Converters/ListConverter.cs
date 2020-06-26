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
using System.ComponentModel;
using System.Collections;
using System.Collections.ObjectModel;

namespace Xceed.Wpf.Toolkit.PropertyGrid.Converters
{
  internal class ListConverter : TypeConverter
  {
    public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
    {
      return true;
    }

    public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
    {
      return ( destinationType == typeof( string ) );
    }

    public override object ConvertFrom( ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value )
    {
      if( value == null )
        return null;

      string names = value as string;

      var list = new List<object>();
      if( names == null && value != null )
      {
        list.Add( value );
      }
      else
      {
        if( names == null )
          return null;

        foreach( var name in names.Split( ',' ) )
        {
          list.Add( name.Trim() );
        }
      }

      return new ReadOnlyCollection<object>( list );
    }

    public override object ConvertTo( ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType )
    {
      if( destinationType != typeof( string ) )
        throw new InvalidOperationException( "Can only convert to string." );


      IList strs = ( IList )value;

      if( strs == null )
        return null;

      StringBuilder sb = new StringBuilder();
      bool first = true;
      foreach( object o in strs )
      {
        if( o == null )
          throw new InvalidOperationException( "Property names cannot be null." );

        string s = o as string;
        if( s == null )
          throw new InvalidOperationException( "Does not support serialization of non-string property names." );

        if( s.Contains( ',' ) )
          throw new InvalidOperationException( "Property names cannot contain commas." );

        if( s.Trim().Length != s.Length )
          throw new InvalidOperationException( "Property names cannot start or end with whitespace characters." );

        if( !first )
        {
          sb.Append( ", " );
        }
        first = false;

        sb.Append( s );
      }

      return sb.ToString();
    }
  }
}
