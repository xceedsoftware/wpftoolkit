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

namespace Xceed.Utils.Collections
{
  internal abstract class RSTreeHelper<T>
  {
    protected abstract int Dimensions
    {
      get;
    }

    protected abstract double GetArea( T region );
    protected abstract double GetMargin( T region );
    protected abstract double[] GetCenter( T region );

    protected abstract T UnionWith( T x, T y );
    protected abstract T IntersectWith( T x, T y );

    protected abstract bool AreEqual( T x, T y );
    protected abstract bool AreOverlapping( T x, T y );

    protected abstract bool IsWithin( T source, T region );
    protected abstract bool IsEmpty( T region );

    protected abstract T CreateEmptyRegion();

    protected abstract double GetValue( T region, int dimension );
    protected abstract double GetSize( T region, int dimension );

    internal int GetDimensions()
    {
      return this.Dimensions;
    }

    internal bool IsEmptyRegion( T region )
    {
      return this.IsEmpty( region );
    }

    internal T GetEmptyRegion()
    {
      return this.CreateEmptyRegion();
    }

    internal double GetValueOf( T region, int dimension )
    {
      Debug.Assert( ( dimension >= 0 ) && ( dimension < this.Dimensions ) );
      return this.GetValue( region, dimension );
    }

    internal double GetSizeOf( T region, int dimension )
    {
      Debug.Assert( ( dimension >= 0 ) && ( dimension < this.Dimensions ) );
      return this.GetSize( region, dimension );
    }

    internal bool AreEquivalent( T x, T y )
    {
      if( this.IsEmpty( x ) )
      {
        return this.IsEmpty( y );
      }
      else if( this.IsEmpty( y ) )
      {
        return this.IsEmpty( x );
      }
      else
      {
        return this.AreEqual( x, y );
      }
    }

    internal bool IsOverlapping( T source, T region )
    {
      if( this.IsEmpty( source ) || this.IsEmpty( region ) )
        return false;

      return this.AreOverlapping( source, region );
    }

    internal bool IsWithinBounds( T source, T region )
    {
      if( this.IsEmpty( source ) || this.IsEmpty( region ) )
        return false;

      return this.IsWithin( source, region );
    }

    internal double[] CenterOf( T region )
    {
      if( this.IsEmpty( region ) )
        throw new InvalidOperationException();

      var center = this.GetCenter( region );
      Debug.Assert( ( center != null ) && ( center.Length == this.Dimensions ) );

      return center;
    }

    internal T Union( T source, T region )
    {
      if( this.IsEmpty( source ) )
      {
        return region;
      }
      else if( this.IsEmpty( region ) )
      {
        return source;
      }
      else
      {
        return this.UnionWith( source, region );
      }
    }

    internal T Intersect( T source, T region )
    {
      if( this.IsEmpty( source ) )
      {
        return source;
      }
      else if( this.IsEmpty( region ) )
      {
        return region;
      }
      else
      {
        return this.IntersectWith( source, region );
      }
    }

    internal double CalculateOverlapEnlargement( T source, T region )
    {
      return this.CalculateArea( this.Intersect( source, region ) );
    }

    internal double CalculateAreaEnlargement( T source, T region )
    {
      var finalArea = this.CalculateArea( this.Union( source, region ) );
      var sourceArea = this.CalculateArea( source );

      Debug.Assert( finalArea >= sourceArea );

      return finalArea - sourceArea;
    }

    internal double CalculateArea( T region )
    {
      if( this.IsEmpty( region ) )
        return 0d;

      return this.GetArea( region );
    }

    internal double CalculateMargin( T region )
    {
      if( this.IsEmpty( region ) )
        return 0d;

      return this.GetMargin( region );
    }
  }
}
