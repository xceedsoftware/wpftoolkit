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
using System.Windows;

namespace Xceed.Wpf.Toolkit.Core.Utilities
{
  internal struct Segment
  {
    #region Constructors

    public Segment( Point point )
    {
      _p1 = point;
      _p2 = point;
      _isP1Excluded = false;
      _isP2Excluded = false;
    }

    public Segment( Point p1, Point p2 )
    {
      _p1 = p1;
      _p2 = p2;
      _isP1Excluded = false;
      _isP2Excluded = false;
    }

    public Segment( Point p1, Point p2, bool excludeP1, bool excludeP2 )
    {
      _p1 = p1;
      _p2 = p2;
      _isP1Excluded = excludeP1;
      _isP2Excluded = excludeP2;
    }

    #endregion

    #region Empty Static Properties

    public static Segment Empty
    {
      get
      {
        Segment result = new Segment( new Point( 0, 0 ) );
        result._isP1Excluded = true;
        result._isP2Excluded = true;
        return result;
      }
    }

    #endregion

    #region P1 Property

    public Point P1
    {
      get
      {
        return _p1;
      }
    }

    #endregion

    #region P2 Property

    public Point P2
    {
      get
      {
        return _p2;
      }
    }

    #endregion

    #region IsP1Excluded Property

    public bool IsP1Excluded
    {
      get
      {
        return _isP1Excluded;
      }
    }

    #endregion

    #region IsP2Excluded Property

    public bool IsP2Excluded
    {
      get
      {
        return _isP2Excluded;
      }
    }

    #endregion

    #region IsEmpty Property

    public bool IsEmpty
    {
      get
      {
        return DoubleHelper.AreVirtuallyEqual( _p1, _p2 ) && ( _isP1Excluded || _isP2Excluded );
      }
    }

    #endregion

    #region IsPoint Property

    public bool IsPoint
    {
      get
      {
        return DoubleHelper.AreVirtuallyEqual( _p1, _p2 );
      }
    }

    #endregion

    #region Length Property

    public double Length
    {
      get
      {
        return ( this.P2 - this.P1 ).Length;
      }
    }

    #endregion

    #region Slope Property

    public double Slope
    {
      get
      {
        return ( this.P2.X == this.P1.X ) ? double.NaN : ( this.P2.Y - this.P1.Y ) / ( this.P2.X - this.P1.X );
      }
    }

    #endregion

    public bool Contains( Point point )
    {
      if( IsEmpty )
        return false;

      // if the point is an endpoint, ensure that it is not excluded
      if( DoubleHelper.AreVirtuallyEqual( _p1, point ) )
        return _isP1Excluded;

      if( DoubleHelper.AreVirtuallyEqual( _p2, point ) )
        return _isP2Excluded;

      bool result = false;

      // ensure that a line through P1 and the point is parallel to the current segment
      if( DoubleHelper.AreVirtuallyEqual( Slope, new Segment( _p1, point ).Slope ) )
      {
        // finally, ensure that the point is between the segment's endpoints
        result = ( point.X >= Math.Min( _p1.X, _p2.X ) )
              && ( point.X <= Math.Max( _p1.X, _p2.X ) )
              && ( point.Y >= Math.Min( _p1.Y, _p2.Y ) )
              && ( point.Y <= Math.Max( _p1.Y, _p2.Y ) );
      }
      return result;
    }

    public bool Contains( Segment segment )
    {
      return ( segment == this.Intersection( segment ) );
    }

    public override bool Equals( object o )
    {
      if( !( o is Segment ) )
        return false;

      Segment other = ( Segment )o;

      // empty segments are always considered equal
      if( this.IsEmpty )
        return other.IsEmpty;

      // segments are considered equal if
      //    1) the endpoints are equal and equally excluded
      //    2) the opposing endpoints are equal and equally excluded
      if( DoubleHelper.AreVirtuallyEqual( _p1, other._p1 ) )
      {
        return ( DoubleHelper.AreVirtuallyEqual( _p2, other._p2 )
             && _isP1Excluded == other._isP1Excluded
             && _isP2Excluded == other._isP2Excluded );
      }
      else
      {
        return ( DoubleHelper.AreVirtuallyEqual( _p1, other._p2 )
             && DoubleHelper.AreVirtuallyEqual( _p2, other._p1 )
             && _isP1Excluded == other._isP2Excluded
             && _isP2Excluded == other._isP1Excluded );
      }
    }

    public override int GetHashCode()
    {
      return _p1.GetHashCode() ^ _p2.GetHashCode() ^ _isP1Excluded.GetHashCode() ^ _isP2Excluded.GetHashCode();
    }

    public Segment Intersection( Segment segment )
    {
      // if either segment is empty, the intersection is also empty
      if( this.IsEmpty || segment.IsEmpty )
        return Segment.Empty;

      // if the segments are equal, just return a new equal segment
      if( this == segment )
        return new Segment( this._p1, this._p2, this._isP1Excluded, this._isP2Excluded );

      // if either segment is a Point, just see if the point is contained in the other segment
      if( this.IsPoint )
        return segment.Contains( this._p1 ) ? new Segment( this._p1 ) : Segment.Empty;

      if( segment.IsPoint )
        return this.Contains( segment._p1 ) ? new Segment( segment._p1 ) : Segment.Empty;

      // okay, no easy answer, so let's do the math...
      Point p1 = this._p1;
      Vector v1 = this._p2 - this._p1;
      Point p2 = segment._p1;
      Vector v2 = segment._p2 - segment._p1;
      Vector endpointVector = p2 - p1;

      double xProd = Vector.CrossProduct( v1, v2 );

      // if segments are not parallel, then look for intersection on each segment
      if( !DoubleHelper.AreVirtuallyEqual( Slope, segment.Slope ) )
      {
        // check for intersection on other segment
        double s = ( Vector.CrossProduct( endpointVector, v1 ) ) / xProd;
        if( s < 0 || s > 1 )
          return Segment.Empty;

        // check for intersection on this segment
        s = ( Vector.CrossProduct( endpointVector, v2 ) ) / xProd;
        if( s < 0 || s > 1 )
          return Segment.Empty;

        // intersection of segments is a point
        return new Segment( p1 + s * v1 );
      }

      // segments are parallel
      xProd = Vector.CrossProduct( endpointVector, v1 );
      if( xProd * xProd > 1.0e-06 * v1.LengthSquared * endpointVector.LengthSquared )
      {
        // segments do not intersect
        return Segment.Empty;
      }

      // intersection is overlapping segment
      Segment result = new Segment();

      // to determine the overlapping segment, create reference segments where the endpoints are *not* excluded
      Segment refThis = new Segment( this._p1, this._p2 );
      Segment refSegment = new Segment( segment._p1, segment._p2 );

      // check whether this segment is contained in the other segment
      bool includeThisP1 = refSegment.Contains( refThis._p1 );
      bool includeThisP2 = refSegment.Contains( refThis._p2 );
      if( includeThisP1 && includeThisP2 )
      {
        result._p1 = this._p1;
        result._p2 = this._p2;
        result._isP1Excluded = this._isP1Excluded || !segment.Contains( this._p1 );
        result._isP2Excluded = this._isP2Excluded || !segment.Contains( this._p2 );
        return result;
      }

      // check whether the other segment is contained in this segment
      bool includeSegmentP1 = refThis.Contains( refSegment._p1 );
      bool includeSegmentP2 = refThis.Contains( refSegment._p2 );
      if( includeSegmentP1 && includeSegmentP2 )
      {
        result._p1 = segment._p1;
        result._p2 = segment._p2;
        result._isP1Excluded = segment._isP1Excluded || !this.Contains( segment._p1 );
        result._isP2Excluded = segment._isP2Excluded || !this.Contains( segment._p2 );
        return result;
      }

      // the intersection must include one endpoint from this segment and one endpoint from the other segment
      if( includeThisP1 )
      {
        result._p1 = this._p1;
        result._isP1Excluded = this._isP1Excluded || !segment.Contains( this._p1 );
      }
      else
      {
        result._p1 = this._p2;
        result._isP1Excluded = this._isP2Excluded || !segment.Contains( this._p2 );
      }
      if( includeSegmentP1 )
      {
        result._p2 = segment._p1;
        result._isP2Excluded = segment._isP1Excluded || !this.Contains( segment._p1 );
      }
      else
      {
        result._p2 = segment._p2;
        result._isP2Excluded = segment._isP2Excluded || !this.Contains( segment._p2 );
      }
      return result;
    }

    public override string ToString()
    {
      string s = base.ToString();

      if( this.IsEmpty )
      {
        s = s + ": {Empty}";
      }
      else if( this.IsPoint )
      {
        s = s + ", Point: " + _p1.ToString();
      }
      else
      {
        s = s + ": " + _p1.ToString() + ( _isP1Excluded ? " (excl)" : " (incl)" )
            + " to " + _p2.ToString() + ( _isP2Excluded ? " (excl)" : " (incl)" );
      }

      return s;
    }

    #region Operators Methods

    public static bool operator ==( Segment s1, Segment s2 )
    {
      if( ( object )s1 == null )
        return ( object )s2 == null;

      if( ( object )s2 == null )
        return ( object )s1 == null;

      return s1.Equals( s2 );
    }

    public static bool operator !=( Segment s1, Segment s2 )
    {
      return !( s1 == s2 );
    }

    #endregion

    #region Private Fields

    private bool _isP1Excluded;
    private bool _isP2Excluded;
    private Point _p1;
    private Point _p2;

    #endregion
  }
}
