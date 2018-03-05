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
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal struct RelativePoint : IEquatable<RelativePoint>
  {
    internal RelativePoint( UIElement element, Point relativePoint )
    {
      if( element == null )
        throw new ArgumentNullException( "element" );

      m_element = element;
      m_relativePoint = relativePoint;
    }

    public static bool operator ==( RelativePoint x, RelativePoint y )
    {
      return x.Equals( y );
    }

    public static bool operator !=( RelativePoint x, RelativePoint y )
    {
      return !( x == y );
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    public override bool Equals( object obj )
    {
      if( !( obj is RelativePoint ) )
        return false;

      return this.Equals( ( RelativePoint )obj );
    }

    public bool Equals( RelativePoint obj )
    {
      return ( object.Equals( obj.m_element, m_element ) )
          && ( object.Equals( obj.m_relativePoint, m_relativePoint ) );
    }

    internal Point GetPoint( UIElement relativeTo )
    {
      if( relativeTo == null )
        throw new ArgumentNullException( "element" );

      if( m_element == null )
        throw new InvalidOperationException();

      if( relativeTo == m_element )
        return m_relativePoint;

      return m_element.TranslatePoint( m_relativePoint, relativeTo );
    }

    internal RelativePoint TranslateTo( UIElement relativeTo )
    {
      return new RelativePoint( relativeTo, this.GetPoint( relativeTo ) );
    }

    private readonly UIElement m_element;
    private readonly Point m_relativePoint;
  }
}
