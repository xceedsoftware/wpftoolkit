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
using System.Globalization;
using System.Windows;
using Xceed.Wpf.Toolkit.Primitives;


namespace Xceed.Wpf.Toolkit
{
  public abstract class NumericUpDown<T> : UpDownBase<T>
  {
#pragma warning disable 0618

    #region Properties

    #region AutoMoveFocus

    public bool AutoMoveFocus
    {
      get
      {
        return ( bool )GetValue( AutoMoveFocusProperty );
      }
      set
      {
        SetValue( AutoMoveFocusProperty, value );
      }
    }

    public static readonly DependencyProperty AutoMoveFocusProperty =
        DependencyProperty.Register( "AutoMoveFocus", typeof( bool ), typeof( NumericUpDown<T> ), new UIPropertyMetadata( false ) );

    #endregion AutoMoveFocus

    #region AutoSelectBehavior

    public AutoSelectBehavior AutoSelectBehavior
    {
      get
      {
        return ( AutoSelectBehavior )GetValue( AutoSelectBehaviorProperty );
      }
      set
      {
        SetValue( AutoSelectBehaviorProperty, value );
      }
    }

    public static readonly DependencyProperty AutoSelectBehaviorProperty =
        DependencyProperty.Register( "AutoSelectBehavior", typeof( AutoSelectBehavior ), typeof( NumericUpDown<T> ),
      new UIPropertyMetadata( AutoSelectBehavior.OnFocus ) );

    #endregion AutoSelectBehavior PROPERTY

    #region FormatString

    public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register( "FormatString", typeof( string ), typeof( NumericUpDown<T> ), new UIPropertyMetadata( String.Empty, OnFormatStringChanged, OnCoerceFormatString ) );
    public string FormatString
    {
      get
      {
        return ( string )GetValue( FormatStringProperty );
      }
      set
      {
        SetValue( FormatStringProperty, value );
      }
    }

    private static object OnCoerceFormatString( DependencyObject o, object baseValue )
    {
      NumericUpDown<T> numericUpDown = o as NumericUpDown<T>;
      if( numericUpDown != null )
        return numericUpDown.OnCoerceFormatString( (string)baseValue );

      return baseValue;
    }

    protected virtual string OnCoerceFormatString( string baseValue )
    {
      return baseValue ?? string.Empty;
    }

    private static void OnFormatStringChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      NumericUpDown<T> numericUpDown = o as NumericUpDown<T>;
      if( numericUpDown != null )
        numericUpDown.OnFormatStringChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnFormatStringChanged( string oldValue, string newValue )
    {
      if( IsInitialized )
      {
        this.SyncTextAndValueProperties( false, null );
      }
    }

    #endregion //FormatString

    #region Increment

    public static readonly DependencyProperty IncrementProperty = DependencyProperty.Register( "Increment", typeof( T ), typeof( NumericUpDown<T> ), new PropertyMetadata( default( T ), OnIncrementChanged, OnCoerceIncrement ) );
    public T Increment
    {
      get
      {
        return ( T )GetValue( IncrementProperty );
      }
      set
      {
        SetValue( IncrementProperty, value );
      }
    }

    private static void OnIncrementChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      NumericUpDown<T> numericUpDown = o as NumericUpDown<T>;
      if( numericUpDown != null )
        numericUpDown.OnIncrementChanged( ( T )e.OldValue, ( T )e.NewValue );
    }

    protected virtual void OnIncrementChanged( T oldValue, T newValue )
    {
      if( this.IsInitialized )
      {
        SetValidSpinDirection();
      }
    }

    private static object OnCoerceIncrement( DependencyObject d, object baseValue )
    {
      NumericUpDown<T> numericUpDown = d as NumericUpDown<T>;
      if( numericUpDown != null )
        return numericUpDown.OnCoerceIncrement( ( T )baseValue );

      return baseValue;
    }

    protected virtual T OnCoerceIncrement( T baseValue )
    {
      return baseValue;
    }

    #endregion

    #region MaxLength

    public static readonly DependencyProperty MaxLengthProperty = DependencyProperty.Register( "MaxLength", typeof( int ), typeof( NumericUpDown<T> ), new UIPropertyMetadata( 0 ) );
    public int MaxLength
    {
      get
      {
        return ( int )GetValue( MaxLengthProperty );
      }
      set
      {
        SetValue( MaxLengthProperty, value );
      }
    }

    #endregion //MaxLength

    #endregion //Properties

    #region Overrides


    #endregion

    #region Methods

    protected static decimal ParsePercent( string text, IFormatProvider cultureInfo )
    {
      NumberFormatInfo info = NumberFormatInfo.GetInstance( cultureInfo );

      text = text.Replace( info.PercentSymbol, null );

      decimal result = Decimal.Parse( text, NumberStyles.Any, info );
      result = result / 100;

      return result;
    }

    #endregion //Methods
  }

#pragma warning restore 0618
}
