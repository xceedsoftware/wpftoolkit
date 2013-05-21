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
using System.Globalization;
using System.IO;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit
{
  public abstract class CommonNumericUpDown<T> : NumericUpDown<T?> where T : struct, IFormattable, IComparable<T>
  {
    protected delegate T FromText( string s, NumberStyles style, IFormatProvider provider );
    protected delegate T FromDecimal( decimal d );

    private FromText _fromText;
    private FromDecimal _fromDecimal;
    private Func<T, T, bool> _fromLowerThan;
    private Func<T, T, bool> _fromGreaterThan;



    #region ParsingNumberStyle

    public static readonly DependencyProperty ParsingNumberStyleProperty =
        DependencyProperty.Register( "ParsingNumberStyle", typeof( NumberStyles ), typeof( CommonNumericUpDown<T> ), new UIPropertyMetadata( NumberStyles.Any ) );

    public NumberStyles ParsingNumberStyle
    {
      get { return ( NumberStyles )GetValue( ParsingNumberStyleProperty ); }
      set { SetValue( ParsingNumberStyleProperty, value ); }
    }

    #endregion //ParsingNumberStyle


    protected CommonNumericUpDown( FromText fromText, FromDecimal fromDecimal, Func<T, T, bool> fromLowerThan, Func<T, T, bool> fromGreaterThan )
    {
      if( fromText == null )
        throw new ArgumentNullException( "parseMethod" );

      if( fromDecimal == null )
        throw new ArgumentNullException( "fromDecimal" );

      if( fromLowerThan == null )
        throw new ArgumentNullException( "fromLowerThan" );

      if( fromGreaterThan == null )
        throw new ArgumentNullException( "fromGreaterThan" );

      _fromText = fromText;
      _fromDecimal = fromDecimal;
      _fromLowerThan = fromLowerThan;
      _fromGreaterThan = fromGreaterThan;
    }

    protected static void UpdateMetadata( Type type, T? increment, T? minValue, T? maxValue )
    {
      DefaultStyleKeyProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( type ) );
      UpdateMetadataCommon( type, increment, minValue, maxValue );
    }

    internal static void UpdateMetadataInternal( Type type, T? increment, T? minValue, T? maxValue )
    {
      // DefaultStyleKey for internal type (eg. UShortUpDown) must be a ComponentResourceKey instead
      // of the type itself to allow external theme (ex. Office2007 theme) to redefine the default
      // style of the control.
      DefaultStyleKeyProperty.OverrideMetadata( type,
        new FrameworkPropertyMetadata( new ComponentResourceKey( typeof( InputBase ), type.Name ) ) );
      UpdateMetadataCommon( type, increment, minValue, maxValue );
    }

    private static void UpdateMetadataCommon( Type type, T? increment, T? minValue, T? maxValue )
    {
      IncrementProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( increment ) );
      MaximumProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( maxValue ) );
      MinimumProperty.OverrideMetadata( type, new FrameworkPropertyMetadata( minValue ) );
    }

    protected void TestInputSpecialValue( AllowedSpecialValues allowedValues, AllowedSpecialValues valueToCompare )
    {
      if( ( allowedValues & valueToCompare ) != valueToCompare )
      {
        switch( valueToCompare )
        {
          case AllowedSpecialValues.NaN :
            throw new InvalidDataException( "Value to parse shouldn't be NaN." );
          case AllowedSpecialValues.PositiveInfinity:
            throw new InvalidDataException( "Value to parse shouldn't be Positive Infinity." );
          case AllowedSpecialValues.NegativeInfinity:
            throw new InvalidDataException( "Value to parse shouldn't be Negative Infinity." );
        }
      }
    }

    private bool IsLowerThan( T? value1, T? value2 )
    {
      if( value1 == null || value2 == null )
        return false;

      return _fromLowerThan( value1.Value, value2.Value );
    }

    private bool IsGreaterThan( T? value1, T? value2 )
    {
      if( value1 == null || value2 == null )
        return false;

      return _fromGreaterThan( value1.Value, value2.Value );
    }

    private bool HandleNullSpin()
    {
      if( !Value.HasValue )
      {
        T forcedValue = ( DefaultValue.HasValue )
          ? DefaultValue.Value
          : default( T );

        Value = CoerceValueMinMax( forcedValue );

        return true;
      }
      else if( !Increment.HasValue )
      {
        return true;
      }

      return false;
    }

    internal bool IsValid( T? value )
    {
      return !IsLowerThan( value, Minimum ) && !IsGreaterThan( value, Maximum );
    }

    private T? CoerceValueMinMax( T value )
    {
      if( IsLowerThan( value, Minimum ) )
        return Minimum;
      else if( IsGreaterThan( value, Maximum ) )
        return Maximum;
      else
        return value;
    }

    #region Base Class Overrides

    protected override void OnIncrement()
    {
      if( !HandleNullSpin() )
      {
        T result = IncrementValue( Value.Value, Increment.Value );
        Value = CoerceValueMinMax( result );
      }
    }

    protected override void OnDecrement()
    {
      if( !HandleNullSpin() )
      {
        T result = DecrementValue( Value.Value, Increment.Value );
        Value = CoerceValueMinMax( result );
      }
    }

    protected override T? ConvertTextToValue( string text )
    {
      T? result = null;

      if( String.IsNullOrEmpty( text ) )
        return result;

      // Since the conversion from Value to text using a FormartString may not be parsable,
      // we verify that the already existing text is not the exact same value.
      string currentValueText = ConvertValueToText();
      if( object.Equals( currentValueText, text ) )
        return this.Value;

      //Don't know why someone would format a T as %, but just in case they do.
      result = FormatString.Contains( "P" )
        ? _fromDecimal( ParsePercent( text, CultureInfo ) )
        : _fromText( text, this.ParsingNumberStyle, CultureInfo );

      if( this.ClipValueToMinMax )
      {
        return this.GetClippedMinMaxValue();
      }

      ValidateDefaultMinMax( result );

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

      // Null increment always prevents spin.
      if( (this.Increment != null) && !IsReadOnly )
      {
        if( IsLowerThan( Value, Maximum ) || !Value.HasValue )
          validDirections = validDirections | ValidSpinDirections.Increase;

        if( IsGreaterThan( Value, Minimum ) || !Value.HasValue )
          validDirections = validDirections | ValidSpinDirections.Decrease;
      }

      if( Spinner != null )
        Spinner.ValidSpinDirection = validDirections;
    }

    private T? GetClippedMinMaxValue()
    {
      T? result = FormatString.Contains( "P" )
                ? _fromDecimal( ParsePercent( this.Text, CultureInfo ) )
                : _fromText( this.Text, this.ParsingNumberStyle, CultureInfo );

      if( this.IsGreaterThan( result, this.Maximum ) )
        return this.Maximum;
      else if( this.IsLowerThan( result, this.Minimum ) )
        return this.Minimum;
      return result;
    }

    private void ValidateDefaultMinMax( T? value )
    {
      // DefaultValue is always accepted.
      if( object.Equals( value, DefaultValue ) )
        return;

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
