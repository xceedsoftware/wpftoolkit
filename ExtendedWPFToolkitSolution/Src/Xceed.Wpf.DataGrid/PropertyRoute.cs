/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal sealed class PropertyRoute
  {
    internal PropertyRoute( PropertyRouteSegment segment )
      : this( segment, null )
    {
    }

    private PropertyRoute( PropertyRouteSegment segment, PropertyRoute parent )
    {
      m_current = segment;
      m_parent = parent;
    }

    #region Current Property

    internal PropertyRouteSegment Current
    {
      get
      {
        return m_current;
      }
    }

    private readonly PropertyRouteSegment m_current;

    #endregion

    #region Parent Property

    internal PropertyRoute Parent
    {
      get
      {
        return m_parent;
      }
    }

    private readonly PropertyRoute m_parent;

    #endregion

    public static bool operator ==( PropertyRoute x, PropertyRoute y )
    {
      if( object.ReferenceEquals( x, y ) )
        return true;

      if( object.ReferenceEquals( x, null ) || object.ReferenceEquals( y, null ) )
        return false;

      return ( PropertyRouteSegment.Equals( x.m_current, y.m_current ) )
          && ( x.m_parent == y.m_parent );
    }

    public static bool operator !=( PropertyRoute x, PropertyRoute y )
    {
      return !( x == y );
    }

    public override int GetHashCode()
    {
      return m_current.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      var target = obj as PropertyRoute;
      if( object.ReferenceEquals( target, null ) )
        return false;

      return ( target == this );
    }

    internal static PropertyRoute Combine( PropertyRouteSegment segment, PropertyRoute ancestors )
    {
      if( ancestors == null )
        return new PropertyRoute( segment );

      if( ancestors.Current.Type != PropertyRouteSegmentType.Other )
        return new PropertyRoute( segment, ancestors );

      var path = PropertyRouteParser.Parse( new PropertyRoute( segment, new PropertyRoute( ancestors.Current ) ) );
      Debug.Assert( !string.IsNullOrEmpty( path ) );

      return new PropertyRoute( new PropertyRouteSegment( PropertyRouteSegmentType.Other, path ), ancestors.Parent );
    }

    internal static PropertyRoute Combine( PropertyRoute descendants, PropertyRouteSegment segment )
    {
      return PropertyRoute.Combine( descendants, new PropertyRoute( segment ) );
    }

    internal static PropertyRoute Combine( PropertyRoute descendants, PropertyRoute ancestors )
    {
      if( ancestors == null )
        return descendants;

      if( descendants == null )
        return ancestors;

      return PropertyRoute.Combine(
               descendants.Current,
               PropertyRoute.Combine( descendants.Parent, ancestors ) );
    }
  }
}
