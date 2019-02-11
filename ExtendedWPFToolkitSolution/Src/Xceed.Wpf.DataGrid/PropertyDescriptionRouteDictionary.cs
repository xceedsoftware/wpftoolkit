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

namespace Xceed.Wpf.DataGrid
{
  internal sealed class PropertyDescriptionRouteDictionary
  {
    internal PropertyDescriptionRoute this[ PropertyRoute key ]
    {
      get
      {
        PropertyDescriptionRoute value;

        if( this.TryGetValue( key, out value ) )
          return value;

        return null;
      }
    }

    internal ICollection<PropertyDescriptionRoute> Values
    {
      get
      {
        return m_collection.Values;
      }
    }

    internal void Clear()
    {
      m_collection.Clear();
    }

    internal void Add( PropertyDescriptionRoute value, bool overwrite )
    {
      PropertyDescriptionRouteDictionary.EnsureValue( value );

      var key = PropertyRouteBuilder.ToPropertyRoute( value );

      this.Add( key, value, overwrite );
    }

    internal void Add( PropertyRoute key, PropertyDescriptionRoute value, bool overwrite )
    {
      PropertyDescriptionRouteDictionary.EnsureKey( key );
      PropertyDescriptionRouteDictionary.EnsureValue( value );

      if( overwrite )
      {
        m_collection[ key ] = value;
      }
      else
      {
        m_collection.Add( key, value );
      }
    }

    internal void Remove( PropertyRoute key )
    {
      PropertyDescriptionRouteDictionary.EnsureKey( key );

      m_collection.Remove( key );
    }

    internal bool Contains( PropertyRoute key )
    {
      PropertyDescriptionRoute unused;

      return this.TryGetValue( key, out unused );
    }

    internal bool TryGetValue( PropertyRoute key, out PropertyDescriptionRoute value )
    {
      PropertyDescriptionRouteDictionary.EnsureKey( key );

      return m_collection.TryGetValue( key, out value );
    }

    private static void EnsureKey( PropertyRoute key )
    {
      if( key == null )
        throw new ArgumentNullException( "key" );
    }

    private static void EnsureValue( PropertyDescriptionRoute value )
    {
      if( value == null )
        throw new ArgumentNullException( "value" );
    }

    private readonly Dictionary<PropertyRoute, PropertyDescriptionRoute> m_collection = new Dictionary<PropertyRoute, PropertyDescriptionRoute>();
  }
}
