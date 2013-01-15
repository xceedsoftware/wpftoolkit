/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Windows;
using System.Globalization;

namespace Xceed.Wpf.Toolkit
{
  public abstract class CommonNumericUpDown<T> : NumericUpDown<T?> where T : struct, IFormattable, IComparable<T>
  {
    protected delegate T FromText( string s, NumberStyles style, IFormatProvider provider );
    protected delegate T FromDecimal( decimal d );

    private FromText _fromText;
    private FromDecimal _fromDecimal;

    protected CommonNumericUpDown( FromText fromText, FromDecimal fromDecimal )
    {
      if( fromText == null )
        throw new ArgumentNullException( "parseMethod" );

      if( fromDecimal == null )
        throw new ArgumentNullException( "fromDecimal" );

      _fromText = fromText;
      _fromDecimal = fromDecimal;
    }

    protected static void UpdateMetadata( Type type, T? defaultValue, T? increment, T? minValue, T? maxValue )
    {
      DefaultStyleKeyProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( type ) );
      DefaultValueProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( defaultValue ) );
      IncrementProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( increment ) );
      MaximumProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( maxValue ) );
      MinimumProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( minValue ) );
    }

    private bool IsLowerThan( T? value1, T? value2 )
    {
      if( value1 == null || value2 == null )
        return false;

      return ( value1.Value.CompareTo( value2.Value ) < 0 );
    }

    private bool IsGreaterThan( T? value1, T? value2 )
    {
      if( value1 == null || value2 == null )
        return false;

      return ( value1.Value.CompareTo( value2.Value ) > 0 );
    }

    private bool HandleNullSpin()
    {
      if( !Value.HasValue )
      {
        Value = DefaultValue;
        return true;
      }
      else if( !Increment.HasValue )
      {
        return true;
      }

      return false;
    }

    private T? CoerceValue( T value )
    {
      if( IsLowerThan( value, Minimum ) )
        return Minimum;
      else if( IsGreaterThan( value, Maximum ) )
        return Maximum;
      else
        return value;
    }

    #region Base Class Overrides


    protected override object OnCoerceValue( object newValue )
    {
      ValidateMinMax( ( T? )newValue );

      return newValue;
    }



    protected override void OnIncrement()
    {
      if( !HandleNullSpin() )
      {
        T result = IncrementValue( Value.Value, Increment.Value );
        Value = CoerceValue( result );
      }
    }

    protected override void OnDecrement()
    {
      if( !HandleNullSpin() )
      {
        T result = DecrementValue( Value.Value, Increment.Value );
        Value = CoerceValue( result );
      }
    }

    protected override T? ConvertTextToValue( string text )
    {
      T? result = null;

      if( String.IsNullOrEmpty( text ) )
        return result;

      //don't know why someone would format a T as %, but just in case they do.
      result = FormatString.Contains( "P" )
        ? _fromDecimal( ParsePercent( text, CultureInfo ) )
        : _fromText( text, NumberStyles.Any, CultureInfo );

      ValidateMinMax( result );

      return result;
    }

    protected override string ConvertValueToText()
    {
      if( Value == null )
        return string.Empty;

      return Value.Value.ToString( FormatString, CultureInfo );
    }

    protected override void SetValidSpinDirection()
    {
      ValidSpinDirections validDirections = ValidSpinDirections.None;

      if( IsLowerThan( Value, Maximum ) || !Value.HasValue )
        validDirections = validDirections | ValidSpinDirections.Increase;

      if( IsGreaterThan( Value, Minimum ) || !Value.HasValue )
        validDirections = validDirections | ValidSpinDirections.Decrease;

      if( Spinner != null )
        Spinner.ValidSpinDirection = validDirections;
    }

    private void ValidateMinMax( T? value )
    {
      if( IsLowerThan( value, Minimum ) )
        throw new ArgumentOutOfRangeException( "Minimum", String.Format( "Value must be greater than MinValue of {0}", Minimum ) );
      else if( IsGreaterThan( value, Maximum ) )
        throw new ArgumentOutOfRangeException( "Maximum", String.Format( "Value must be less than MaxValue of {0}", Maximum ) );
    }

    #endregion //Base Class Overrides


    #region Abstract Methods

    protected abstract T IncrementValue( T value, T increment );

    protected abstract T DecrementValue( T value, T increment );

    #endregion
  }
}
