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
using System.Diagnostics;
using System.Globalization;

namespace Xceed.Utils.Collections
{
  internal sealed class RSTree2D<T> : RSTree<RSTree2D<T>.Area, T>
  {
    internal RSTree2D()
      : base( new Helper() )
    {
    }

    internal RSTree2D( int minNodeSize, int maxNodeSize )
      : base( new Helper(), minNodeSize, maxNodeSize )
    {
    }

    #region Area Internal Struct

    internal struct Area
    {
      internal static readonly Area Empty = new Area( 0, 0, 0, 0 );

      internal Area( int row, int rowLength, int column, int columnLength )
      {
        Debug.Assert( ( rowLength >= 0 ) && ( columnLength >= 0 ) );

        this.Row = row;
        this.RowLength = rowLength;
        this.Column = column;
        this.ColumnLength = columnLength;
      }

      internal bool IsEmpty
      {
        get
        {
          return ( this.RowLength <= 0 )
              || ( this.ColumnLength <= 0 );
        }
      }

      public override string ToString()
      {
        if( this.IsEmpty )
          return "Empty";

        var rs = this.Row;
        var re = this.Row + this.RowLength - 1;
        var cs = this.Column;
        var ce = this.Column + this.ColumnLength - 1;

        return string.Format( CultureInfo.InvariantCulture, "[R({0},{1}),C({2},{3})]", rs, re, cs, ce );
      }

      internal readonly int Row;
      internal readonly int RowLength;
      internal readonly int Column;
      internal readonly int ColumnLength;
    }

    #endregion

    #region Helper Private Class

    private sealed class Helper : RSTreeHelper<Area>
    {
      protected override int Dimensions
      {
        get
        {
          return 2;
        }
      }

      protected override bool AreEqual( Area x, Area y )
      {
        return ( x.Row == y.Row )
            && ( x.RowLength == y.RowLength )
            && ( x.Column == y.Column )
            && ( x.ColumnLength == y.ColumnLength );
      }

      protected override bool AreOverlapping( Area x, Area y )
      {
        return !this.Intersect( x, y ).IsEmpty;
      }

      protected override bool IsWithin( Area source, Area region )
      {
        return ( source.Row < region.Row )
            && ( source.Column < region.Column )
            && ( ( source.Row + source.RowLength ) > ( region.Row + region.RowLength ) )
            && ( ( source.Column + source.ColumnLength ) > ( region.Column + region.ColumnLength ) );
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
        switch( dimension )
        {
          case 0:
            return region.Row;

          case 1:
            return region.Column;

          default:
            throw new InvalidOperationException();
        }
      }

      protected override double GetSize( Area region, int dimension )
      {
        switch( dimension )
        {
          case 0:
            return region.RowLength;

          case 1:
            return region.ColumnLength;

          default:
            throw new InvalidOperationException();
        }
      }

      protected override double GetArea( Area region )
      {
        return ( region.RowLength * region.ColumnLength );
      }

      protected override double GetMargin( Area region )
      {
        return ( region.RowLength + region.ColumnLength );
      }

      protected override double[] GetCenter( Area region )
      {
        return new double[] { region.Row + ( region.RowLength - 1 ) / 2d, region.Column + ( region.ColumnLength - 1 ) / 2d };
      }

      protected override Area UnionWith( Area x, Area y )
      {
        var xrs = x.Row;
        var xrl = x.RowLength;
        var xcs = x.Column;
        var xcl = x.ColumnLength;
        var yrs = y.Row;
        var yrl = y.RowLength;
        var ycs = y.Column;
        var ycl = y.ColumnLength;

        var rs = System.Math.Min( xrs, yrs );
        var cs = System.Math.Min( xcs, ycs );
        var rl = System.Math.Max( xrs + xrl, yrs + yrl ) - rs;
        var cl = System.Math.Max( xcs + xcl, ycs + ycl ) - cs;

        return new Area( rs, rl, cs, cl );
      }

      protected override Area IntersectWith( Area x, Area y )
      {
        var xrs = x.Row;
        var xre = xrs + x.RowLength;
        var xcs = x.Column;
        var xce = xcs + x.ColumnLength;
        var yrs = y.Row;
        var yre = yrs + y.RowLength;
        var ycs = y.Column;
        var yce = ycs + y.ColumnLength;

        int rs, rl, cs, cl;

        if( xrs <= yrs )
        {
          if( xre <= yrs )
            return Area.Empty;

          rs = yrs;

          if( xcs <= ycs )
          {
            if( xce <= ycs )
              return Area.Empty;

            cs = ycs;
          }
          else
          {
            if( yce <= xcs )
              return Area.Empty;

            cs = xcs;
          }
        }
        else
        {
          if( yre <= xrs )
            return Area.Empty;

          rs = xrs;

          if( xcs <= ycs )
          {
            if( xce <= ycs )
              return Area.Empty;

            cs = ycs;
          }
          else
          {
            if( yce <= xcs )
              return Area.Empty;

            cs = xcs;
          }
        }

        rl = System.Math.Min( xre, yre ) - rs;
        cl = System.Math.Min( xce, yce ) - cs;

        return new Area( rs, rl, cs, cl );
      }
    }

    #endregion
  }
}
