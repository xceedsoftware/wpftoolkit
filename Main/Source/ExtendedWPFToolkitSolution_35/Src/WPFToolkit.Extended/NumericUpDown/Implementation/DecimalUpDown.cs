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

namespace Xceed.Wpf.Toolkit
{
  public class DecimalUpDown : NumericUpDown<decimal?>
  {
    #region Constructors

    static DecimalUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( DecimalUpDown ), new FrameworkPropertyMetadata( typeof( DecimalUpDown ) ) );
      DefaultValueProperty.OverrideMetadata( typeof( DecimalUpDown ), new FrameworkPropertyMetadata( default( decimal ) ) );
      IncrementProperty.OverrideMetadata( typeof( DecimalUpDown ), new FrameworkPropertyMetadata( 1m ) );
      MaximumProperty.OverrideMetadata( typeof( DecimalUpDown ), new FrameworkPropertyMetadata( decimal.MaxValue ) );
      MinimumProperty.OverrideMetadata( typeof( DecimalUpDown ), new FrameworkPropertyMetadata( decimal.MinValue ) );
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override decimal? CoerceValue( decimal? value )
    {
      if( value < Minimum )
        return Minimum;
      else if( value > Maximum )
        return Maximum;
      else
        return value;
    }

    protected override void OnIncrement()
    {
      if( Value.HasValue )
        Value += Increment;
      else
        Value = DefaultValue;
    }

    protected override void OnDecrement()
    {
      if( Value.HasValue )
        Value -= Increment;
      else
        Value = DefaultValue;
    }

    protected override decimal? ConvertTextToValue( string text )
    {
      decimal? result = null;

      if( String.IsNullOrEmpty( text ) )
        return result;

      try
      {
        result = FormatString.Contains( "P" ) ? ParsePercent( text, CultureInfo ) : ParseDecimal( text, CultureInfo );
        result = CoerceValue( result );
      }
      catch
      {
        Text = ConvertValueToText();
        return Value;
      }

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

      if( Value < Maximum || !Value.HasValue )
        validDirections = validDirections | ValidSpinDirections.Increase;

      if( Value > Minimum || !Value.HasValue )
        validDirections = validDirections | ValidSpinDirections.Decrease;

      if( Spinner != null )
        Spinner.ValidSpinDirection = validDirections;
    }

    protected override void ValidateValue( decimal? value )
    {
      if( value < Minimum )
        Value = Minimum;
      else if( value > Maximum )
        Value = Maximum;
    }

    #endregion //Base Class Overrides
  }
}
