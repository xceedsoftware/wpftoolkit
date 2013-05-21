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
using System.ComponentModel;
using System.Globalization;
using System.Windows;

using Xceed.Wpf.DataGrid.Converters;

namespace Xceed.Wpf.DataGrid
{
  [TypeConverter( typeof( ColumnWidthConverter ) )]
  public struct ColumnWidth
  {
    public ColumnWidth( double pixels )
      : this( pixels, ColumnWidthUnitType.Pixel )
    {
    }

    public ColumnWidth( double value, ColumnWidthUnitType type )
    {
      if( double.IsNaN( value ) )
        throw new ArgumentException( "NaN is not a valid value.", "value" ); 

      if( double.IsInfinity( value ) )
        throw new ArgumentException( "Infinity is not a valid value.", "value" );

      if( ( type != ColumnWidthUnitType.Pixel ) && ( type != ColumnWidthUnitType.Star ) )
        throw new InvalidEnumArgumentException( "type", ( int )type, typeof( ColumnWidthUnitType ) ); 

      if( value < 0d )
        throw new ArgumentException( "Negative values are not valid.", "value" );

      if( ( Xceed.Utils.Math.DoubleUtil.AreClose( value, 0d ) ) && ( type == ColumnWidthUnitType.Star ) )
        throw new ArgumentException( "Zero star (0*) is not a valid value.", "value" );

      m_unitValue = value;
      m_unitType = type;
    }

    public double Value
    {
      get
      {
        return m_unitValue;
      }
    }

    public ColumnWidthUnitType UnitType
    {
      get
      {
        return m_unitType;
      }
    }

    public static implicit operator double( ColumnWidth columnWidth )
    {
      return ( ( columnWidth.UnitType == ColumnWidthUnitType.Pixel ) ? columnWidth.Value : double.NaN );
    }

    public static implicit operator ColumnWidth( double columnWidth )
    {
      return new ColumnWidth( columnWidth, ColumnWidthUnitType.Pixel );
    }

    public static bool operator ==( ColumnWidth columnWidth1, ColumnWidth columnWidth2 )
    {
      return 
        ( ( columnWidth1.UnitType == columnWidth2.UnitType ) && 
          ( Xceed.Utils.Math.DoubleUtil.AreClose( columnWidth1.Value, columnWidth2.Value ) ) );
    }

    public static bool operator !=( ColumnWidth columnWidth1, ColumnWidth columnWidth2 )
    {
      if( columnWidth1.UnitType == columnWidth2.UnitType )
        return !Xceed.Utils.Math.DoubleUtil.AreClose( columnWidth1.Value, columnWidth2.Value );

      return true;
    }

    public override bool Equals( object obj )
    {
      if( obj is ColumnWidth )
        return ( this == ( ColumnWidth )obj );

      return false;
    }

    public bool Equals( ColumnWidth columnWidth )
    {
      return ( this == columnWidth );
    }

    public override int GetHashCode()
    {
      return ( Convert.ToInt32( m_unitValue ) + ( int )m_unitType );
    }

    public override string ToString()
    {
      return ColumnWidthConverter.ToString( this, CultureInfo.InvariantCulture );
    }

    private double m_unitValue;
    private ColumnWidthUnitType m_unitType;
  }
}
