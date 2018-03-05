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

namespace Xceed.Wpf.DataGrid
{
  internal struct PropertyRouteSegment : IEquatable<PropertyRouteSegment>
  {
    #region Static Fields

    internal static readonly PropertyRouteSegment Self = new PropertyRouteSegment( PropertyRouteSegmentType.Self, "." );

    #endregion

    internal PropertyRouteSegment( PropertyRouteSegmentType type, string name )
    {
      if( name == null )
        throw new ArgumentNullException( "name" );

      m_type = type;
      m_name = name;
    }

    #region Type Property

    internal PropertyRouteSegmentType Type
    {
      get
      {
        return m_type;
      }
    }

    private readonly PropertyRouteSegmentType m_type;

    #endregion

    #region Name Property

    internal string Name
    {
      get
      {
        return m_name;
      }
    }

    private readonly string m_name;

    #endregion

    public static bool Equals( PropertyRouteSegment x, PropertyRouteSegment y )
    {
      return x.Equals( y );
    }

    public override int GetHashCode()
    {
      return m_name.GetHashCode() ^ m_type.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      return ( obj is PropertyRouteSegment )
          && ( this.Equals( ( PropertyRouteSegment )obj ) );
    }

    public bool Equals( PropertyRouteSegment obj )
    {
      return ( obj.m_name == m_name )
          && ( obj.m_type == m_type );
    }
  }
}
