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
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Windows;

namespace Xceed.Wpf.DataGrid.Converters
{
  public class ColumnWidthConverter : TypeConverter
  {
    public override bool CanConvertFrom( ITypeDescriptorContext context, Type sourceType )
    {
      switch( Type.GetTypeCode( sourceType ) )
      {
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
        case TypeCode.Single:
        case TypeCode.Double:
        case TypeCode.Decimal:
        case TypeCode.String:
          return true;
      }

      if( typeof( ColumnWidth ).IsAssignableFrom( sourceType ) )
        return true;

      return false;
    }

    public override bool CanConvertTo( ITypeDescriptorContext context, Type destinationType )
    {
      return ( destinationType == typeof( string ) );
    }

    public override object ConvertFrom( ITypeDescriptorContext context, CultureInfo culture, object value )
    {
      ColumnWidthUnitType unitType;

      if( value == null )
        throw new ArgumentNullException( "value" );

      if( value is ColumnWidth )
        return value;

      if( value is string )
        return ColumnWidthConverter.FromString( ( string )value, culture );

      double doubleValue = Convert.ToDouble( value, culture );

      unitType = ColumnWidthUnitType.Pixel;

      return new ColumnWidth( doubleValue, unitType );
    }

    public override object ConvertTo( ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType )
    {
      if( destinationType == null )
        throw new ArgumentNullException( "destinationType" );

      if( ( value != null ) && ( value is ColumnWidth ) )
      {
        ColumnWidth columnWidth = ( ColumnWidth )value;

        if( destinationType == typeof( string ) )
        {
          return ColumnWidthConverter.ToString( columnWidth, culture );
        }

        // If we ever want to add InstanceDescriptor in the list of supported destination 
        // type, we will have to implement it in another method and surround the call
        // with a try/catch. Otherwise, the call to Type.GetConstructor() will fail if 
        // the application does not have reflection permission (typically XBAP).
      }

      throw new ArgumentException( "Cannot convert to type " + destinationType.FullName + ".", "value" ); 
    }

    internal static ColumnWidth FromString( string stringValue, CultureInfo cultureInfo )
    {
      stringValue = stringValue.Trim().ToLowerInvariant();
      double value = 0.0;
      ColumnWidthUnitType unit = ColumnWidthUnitType.Pixel;
      int stringValueLength = stringValue.Length;
      int unitStringLength = 0;
      double factorValue = 1.0;
      int index = 0;

      for( index = 0; index < ColumnWidthConverter.UnitStrings.Length; index++ )
      {
        if( stringValue.EndsWith( ColumnWidthConverter.UnitStrings[ index ], StringComparison.Ordinal ) )
        {
          unitStringLength = ColumnWidthConverter.UnitStrings[ index ].Length;
          unit = ( ColumnWidthUnitType )index;
          break;
        }
      }

      if( index >= ColumnWidthConverter.UnitStrings.Length )
      {
        // The unit type was not recognized so far. Search for pixel unit types.
        for( index = 0; index < ColumnWidthConverter.PixelUnitStrings.Length; index++ )
        {
          if( stringValue.EndsWith( ColumnWidthConverter.PixelUnitStrings[ index ], StringComparison.Ordinal ) )
          {
            unitStringLength = ColumnWidthConverter.PixelUnitStrings[ index ].Length;
            factorValue = ColumnWidthConverter.PixelUnitFactors[ index ];
            break;
          }
        }
      }

      if( ( stringValueLength == unitStringLength ) && ( unit == ColumnWidthUnitType.Star ) )
      {
        // * was specified alone. Use the default value of 1.
        value = 1.0;
      }
      else
      {
        // Extract the numeric part of the string.
        string valuePartString = stringValue.Substring( 0, stringValueLength - unitStringLength );
        value = Convert.ToDouble( valuePartString, cultureInfo ) * factorValue;
      }

      return new ColumnWidth( value, unit );
    }

    internal static string ToString( ColumnWidth columnWidth, CultureInfo cultureInfo )
    {
      if( columnWidth.UnitType == ColumnWidthUnitType.Star )
      {
        if( columnWidth.Value != 1d )
          return ( Convert.ToString( columnWidth.Value, cultureInfo ) + "*" );

        return "*";
      }

      return Convert.ToString( columnWidth.Value, cultureInfo );
    }

    private static string[] UnitStrings = new string[] { "px", "*" };
    private static string[] PixelUnitStrings = new string[] { "in", "cm", "pt" };
    private static double[] PixelUnitFactors = new double[] { 96.0, 37.795275590551178, 1.3333333333333333 };
  }
}
