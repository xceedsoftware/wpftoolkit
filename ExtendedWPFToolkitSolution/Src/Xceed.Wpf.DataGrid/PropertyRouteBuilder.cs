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
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class PropertyRouteBuilder
  {
    internal PropertyRouteBuilder()
    {
    }

    internal PropertyRouteBuilder( PropertyRoute route )
    {
      if( route == null )
        return;

      for( var c = route; c != null; c = c.Parent )
      {
        this.PushAncestor( c.Current );
      }
    }

    internal PropertyRouteBuilder( PropertyDescriptionRoute route )
    {
      if( route == null )
        return;

      for( var c = route; c != null; c = c.Parent )
      {
        this.PushAncestor( c.Current );
      }
    }

    internal PropertyRouteBuilder( DataGridItemPropertyRoute route )
    {
      if( route == null )
        return;

      for( var c = route; c != null; c = c.Parent )
      {
        this.PushAncestor( c.Current );
      }
    }

    internal bool IsEmpty
    {
      get
      {
        return ( m_segments.Count <= 0 );
      }
    }

    internal static PropertyRoute ToPropertyRoute( PropertyRouteSegment segment )
    {
      return new PropertyRoute( segment );
    }

    internal static PropertyRoute ToPropertyRoute( PropertyDescriptionRoute description )
    {
      if( description == null )
        return null;

      return PropertyRoute.Combine( PropertyRouteBuilder.ToSegment( description.Current ), PropertyRouteBuilder.ToPropertyRoute( description.Parent ) );
    }

    internal static PropertyRoute ToPropertyRoute( DataGridItemPropertyRoute itemProperty )
    {
      if( itemProperty == null )
        return null;

      return PropertyRoute.Combine( PropertyRouteBuilder.ToSegment( itemProperty.Current ), PropertyRouteBuilder.ToPropertyRoute( itemProperty.Parent ) );
    }

    internal void PushAncestor( PropertyRouteSegment segment )
    {
      m_segments.Insert( 0, segment );
    }

    internal void PushAncestor( PropertyDescription description )
    {
      if( description == null )
        throw new ArgumentNullException( "description" );

      this.PushAncestor( PropertyRouteBuilder.ToSegment( description ) );
    }

    internal void PushAncestor( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        throw new ArgumentNullException( "itemProperty" );

      this.PushAncestor( PropertyRouteBuilder.ToSegment( itemProperty ) );
    }

    internal void PushDescendant( PropertyRouteSegment segment )
    {
      m_segments.Add( segment );
    }

    internal void PushDescendant( PropertyDescription description )
    {
      if( description == null )
        throw new ArgumentNullException( "description" );

      this.PushDescendant( PropertyRouteBuilder.ToSegment( description ) );
    }

    internal void PushDescendant( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        throw new ArgumentNullException( "itemProperty" );

      this.PushDescendant( PropertyRouteBuilder.ToSegment( itemProperty ) );
    }

    internal void PopAncestor()
    {
      if( m_segments.Count <= 0 )
        throw new InvalidOperationException();

      m_segments.RemoveAt( 0 );
    }

    internal void PopDescendant()
    {
      if( m_segments.Count <= 0 )
        throw new InvalidOperationException();

      m_segments.RemoveAt( m_segments.Count - 1 );
    }

    internal PropertyRoute ToPropertyRoute()
    {
      if( m_segments.Count <= 0 )
        return null;

      var route = new PropertyRoute( m_segments[ 0 ] );

      for( int i = 1; i < m_segments.Count; i++ )
      {
        route = PropertyRoute.Combine( m_segments[ i ], route );
      }

      return route;
    }

    private static PropertyRouteSegment ToSegment( PropertyDescription description )
    {
      Debug.Assert( description != null );

      return description.ToPropertyRouteSegment();
    }

    private static PropertyRouteSegment ToSegment( DataGridItemPropertyBase itemProperty )
    {
      Debug.Assert( itemProperty != null );

      var route = PropertyRouteParser.Parse( itemProperty.Name );
      if( route == null )
        throw new ArgumentException( "Unexpected DataGridItemPropertyBase.Name property value.", "itemProperty" );

      if( route.Parent == null )
        return route.Current;

      return new PropertyRouteSegment( PropertyRouteSegmentType.Other, itemProperty.Name );
    }

    private readonly List<PropertyRouteSegment> m_segments = new List<PropertyRouteSegment>();
  }
}
