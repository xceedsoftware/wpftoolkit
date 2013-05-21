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
using System.Globalization;
using System.Windows;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit
{
  public abstract class NumericUpDown<T> : UpDownBase<T>
  {
    #region Properties

    #region ClipValueToMinMax

    public static readonly DependencyProperty ClipValueToMinMaxProperty = DependencyProperty.Register( "ClipValueToMinMax", typeof( bool ), typeof( NumericUpDown<T> ), new UIPropertyMetadata(false ) );
    public bool ClipValueToMinMax
    {
      get
      {
        return ( bool )GetValue( ClipValueToMinMaxProperty );
      }
      set
      {
        SetValue( ClipValueToMinMaxProperty, value );
      }
    }

    #endregion //ClipValueToMinMax

    #region FormatString

    public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register( "FormatString", typeof( string ), typeof( NumericUpDown<T> ), new UIPropertyMetadata( String.Empty, OnFormatStringChanged ) );
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

    #region Maximum

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register( "Maximum", typeof( T ), typeof( NumericUpDown<T> ), new UIPropertyMetadata( default( T ), OnMaximumChanged, OnCoerceMaximum ) );
    public T Maximum
    {
      get
      {
        return ( T )GetValue( MaximumProperty );
      }
      set
      {
        SetValue( MaximumProperty, value );
      }
    }

    private static void OnMaximumChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      NumericUpDown<T> numericUpDown = o as NumericUpDown<T>;
      if( numericUpDown != null )
        numericUpDown.OnMaximumChanged( ( T )e.OldValue, ( T )e.NewValue );
    }

    protected virtual void OnMaximumChanged( T oldValue, T newValue )
    {
      if( this.IsInitialized )
      {
        SetValidSpinDirection();
      }
    }

    private static object OnCoerceMaximum( DependencyObject d, object baseValue )
    {
      NumericUpDown<T> numericUpDown = d as NumericUpDown<T>;
      if( numericUpDown != null )
        return numericUpDown.OnCoerceMaximum( ( T )baseValue );

      return baseValue;
    }

    protected virtual T OnCoerceMaximum( T baseValue )
    {
      return baseValue;
    }

    #endregion //Maximum

    #region Minimum

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register( "Minimum", typeof( T ), typeof( NumericUpDown<T> ), new UIPropertyMetadata( default( T ), OnMinimumChanged, OnCoerceMinimum ) );
    public T Minimum
    {
      get
      {
        return ( T )GetValue( MinimumProperty );
      }
      set
      {
        SetValue( MinimumProperty, value );
      }
    }

    private static void OnMinimumChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      NumericUpDown<T> numericUpDown = o as NumericUpDown<T>;
      if( numericUpDown != null )
        numericUpDown.OnMinimumChanged( ( T )e.OldValue, ( T )e.NewValue );
    }

    protected virtual void OnMinimumChanged( T oldValue, T newValue )
    {
      if( this.IsInitialized )
      {
        SetValidSpinDirection();
      }
    }

    private static object OnCoerceMinimum( DependencyObject d, object baseValue )
    {
      NumericUpDown<T> numericUpDown = d as NumericUpDown<T>;
      if( numericUpDown != null )
        return numericUpDown.OnCoerceMinimum( ( T )baseValue );

      return baseValue;
    }

    protected virtual T OnCoerceMinimum( T baseValue )
    {
      return baseValue;
    }

    #endregion //Minimum

    #region SelectAllOnGotFocus

    public static readonly DependencyProperty SelectAllOnGotFocusProperty = DependencyProperty.Register( "SelectAllOnGotFocus", typeof( bool ), typeof( NumericUpDown<T> ), new PropertyMetadata( true ) );
    public bool SelectAllOnGotFocus
    {
      get
      {
        return ( bool )GetValue( SelectAllOnGotFocusProperty );
      }
      set
      {
        SetValue( SelectAllOnGotFocusProperty, value );
      }
    }

    #endregion //SelectAllOnGotFocus

    #endregion //Properties

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
}
