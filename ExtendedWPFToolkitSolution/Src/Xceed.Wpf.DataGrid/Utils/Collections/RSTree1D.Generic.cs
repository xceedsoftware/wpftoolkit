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
using System.Globalization;

namespace Xceed.Utils.Collections
{
  internal sealed class RSTree1D<T> : RSTree<RSTree1D<T>.Area, T>
  {
    internal RSTree1D()
      : base( new Helper() )
    {
    }

    internal RSTree1D( int minNodeSize, int maxNodeSize )
      : base( new Helper(), minNodeSize, maxNodeSize )
    {
    }

    #region Area Internal Struct

    internal struct Area
    {
      internal static readonly Area Empty = new Area( 0, 0 );

      internal Area( int index, int length )
      {
        Debug.Assert( length >= 0 );

        this.Index = index;
        this.Length = length;
      }

      internal bool IsEmpty
      {
        get
        {
          return ( this.Length <= 0 );
        }
      }

      public override string ToString()
      {
        if( this.IsEmpty )
          return "Empty";

        var startIndex = this.Index;
        var endIndex = this.Index + this.Length - 1;

        return string.Format( CultureInfo.InvariantCulture, "[{0},{1}]", startIndex, endIndex );
      }

      internal readonly int Index;
      internal readonly int Length;
    }

    #endregion

    #region Helper Private Class

    private sealed class Helper : RSTreeHelper<Area>
    {
      protected override int Dimensions
      {
        get
        {
          return 1;
        }
      }

      protected override bool AreEqual( Area x, Area y )
      {
        return ( x.Index == y.Index )
            && ( x.Length == y.Length );
      }

      protected override bool AreOverlapping( Area x, Area y )
      {
        return !this.Intersect( x, y ).IsEmpty;
      }

      protected override bool IsWithin( Area source, Area region )
      {
        return ( source.Index < region.Index )
            && ( source.Index + source.Length ) > ( region.Index + region.Length );
      }

      protected override bool IsEmpty( Area region )
      {
        return region.IsEmpty;
      }

      protected override Area CreateEmptyRegion()
      {
        return Area.Empty;
      }

      protected override double GetValue( Area region, int dimension )
      {
        return region.Index;
      }

      protected override double GetSize( Area region, int dimension )
      {
        return region.Length;
      }

      protected override double GetArea( Area region )
      {
        return region.Length;
      }

      protected override double GetMargin( Area region )
      {
        return region.Length;
      }

      protected override double[] GetCenter( Area region )
      {
        return new double[] { region.Index + ( region.Length - 1 ) / 2d };
      }

      protected override Area UnionWith( Area x, Area y )
      {
        var index = System.Math.Min( x.Index, y.Index );
        var length = System.Math.Max( x.Index + x.Length, y.Index + y.Length ) - index;

        return new Area( index, length );
      }

      protected override Area IntersectWith( Area x, Area y )
      {
        var xs = x.Index;
        var xe = xs + x.Length;
        var ys = y.Index;
        var ye = ys + y.Length;

        if( xs <= ys )
        {
          if( xe <= ys )
            return Area.Empty;

          return new Area( ys, System.Math.Min( xe, ye ) - ys );
        }
        else
        {
          if( ye <= xs )
            return Area.Empty;

          return new Area( xs, System.Math.Min( xe, ye ) - xs );
        }
      }
    }

    #endregion
  }
}
