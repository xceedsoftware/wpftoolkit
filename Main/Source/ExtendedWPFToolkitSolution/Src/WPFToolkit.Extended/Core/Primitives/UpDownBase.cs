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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core;
using Xceed.Wpf.Toolkit.Core.Input;

namespace Xceed.Wpf.Toolkit.Primitives
{
  [TemplatePart( Name = PART_TextBox, Type = typeof( TextBox ) )]
  [TemplatePart( Name = PART_Spinner, Type = typeof( Spinner ) )]
  public abstract class UpDownBase<T> : InputBase, IValidateInput
  {
    #region Members

    /// <summary>
    /// Name constant for Text template part.
    /// </summary>
    internal const string PART_TextBox = "PART_TextBox";

    /// <summary>
    /// Name constant for Spinner template part.
    /// </summary>
    internal const string PART_Spinner = "PART_Spinner";

    /// <summary>
    /// Flags if the Text and Value properties are in the process of being sync'd
    /// </summary>
    private bool _isSyncingTextAndValueProperties;
    private bool _isTextChangedFromUI;

    #endregion //Members

    #region Properties

    protected Spinner Spinner
    {
      get;
      private set;
    }
    protected TextBox TextBox
    {
      get;
      private set;
    }

    #region AllowSpin

    public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register( "AllowSpin", typeof( bool ), typeof( UpDownBase<T> ), new UIPropertyMetadata( true ) );
    public bool AllowSpin
    {
      get
      {
        return ( bool )GetValue( AllowSpinProperty );
      }
      set
      {
        SetValue( AllowSpinProperty, value );
      }
    }

    #endregion //AllowSpin

    #region MouseWheelActiveOnFocus

    public static readonly DependencyProperty MouseWheelActiveOnFocusProperty = DependencyProperty.Register( "MouseWheelActiveOnFocus", typeof( bool ), typeof( UpDownBase<T> ), new UIPropertyMetadata( true ) );
    public bool MouseWheelActiveOnFocus
    {
      get
      {
        return ( bool )GetValue( MouseWheelActiveOnFocusProperty );
      }
      set
      {
        SetValue( MouseWheelActiveOnFocusProperty, value );
      }
    }

    #endregion //MouseWheelActiveOnFocus

    #region ShowButtonSpinner

    public static readonly DependencyProperty ShowButtonSpinnerProperty = DependencyProperty.Register( "ShowButtonSpinner", typeof( bool ), typeof( UpDownBase<T> ), new UIPropertyMetadata( true ) );
    public bool ShowButtonSpinner
    {
      get
      {
        return ( bool )GetValue( ShowButtonSpinnerProperty );
      }
      set
      {
        SetValue( ShowButtonSpinnerProperty, value );
      }
    }

    #endregion //ShowButtonSpinner

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( T ), typeof( UpDownBase<T> ), new FrameworkPropertyMetadata( default( T ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, OnCoerceValue, false, UpdateSourceTrigger.LostFocus ) );
    public T Value
    {
      get
      {
        return ( T )GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private static object OnCoerceValue( DependencyObject o, object basevalue )
    {
      return ( ( UpDownBase<T> )o ).OnCoerceValue( basevalue );
    }

    protected virtual object OnCoerceValue( object newValue )
    {
      return newValue;
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      UpDownBase<T> upDownBase = o as UpDownBase<T>;
      if( upDownBase != null )
        upDownBase.OnValueChanged( ( T )e.OldValue, ( T )e.NewValue );
    }

    protected virtual void OnValueChanged( T oldValue, T newValue )
    {
      SyncTextAndValueProperties( false, null );

      SetValidSpinDirection();

      RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>( oldValue, newValue );
      args.RoutedEvent = ValueChangedEvent;
      RaiseEvent( args );
    }

    #endregion //Value

    #endregion //Properties

    #region Constructors

    internal UpDownBase()
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override void OnAccessKey( AccessKeyEventArgs e )
    {
      if( TextBox != null )
        TextBox.Focus();

      base.OnAccessKey( e );
    }

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      TextBox = GetTemplateChild( PART_TextBox ) as TextBox;
      if( TextBox != null )
      {
        TextBox.Text = Text;
        TextBox.LostFocus += new RoutedEventHandler( TextBox_LostFocus );
        TextBox.TextChanged += new TextChangedEventHandler( TextBox_TextChanged );
      }

      if( Spinner != null )
        Spinner.Spin -= OnSpinnerSpin;

      Spinner = GetTemplateChild( PART_Spinner ) as Spinner;

      if( Spinner != null )
        Spinner.Spin += OnSpinnerSpin;

      SetValidSpinDirection();
    }

    protected override void OnGotFocus( RoutedEventArgs e )
    {
      if( TextBox != null )
        TextBox.Focus();
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      switch( e.Key )
      {
        case Key.Up:
          {
            if( AllowSpin && !IsReadOnly )
              DoIncrement();
            e.Handled = true;
            break;
          }
        case Key.Down:
          {
            if( AllowSpin && !IsReadOnly )
              DoDecrement();
            e.Handled = true;
            break;
          }
      }
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      switch( e.Key )
      {
        case Key.Enter:
          {
            // Commit Text on "Enter" to raise Error event 
            CommitInput();
            e.Handled = true;
            break;
          }
      }
    }

    protected override void OnMouseWheel( MouseWheelEventArgs e )
    {
      base.OnMouseWheel( e );

      if( !e.Handled && AllowSpin && !IsReadOnly && ( ( TextBox.IsFocused && MouseWheelActiveOnFocus ) || !MouseWheelActiveOnFocus ) )
      {
        if( e.Delta < 0 )
        {
          DoDecrement();
        }
        else if( 0 < e.Delta )
        {
          DoIncrement();
        }

        e.Handled = true;
      }
    }

    protected override void OnTextChanged( string oldValue, string newValue )
    {
      if( !_isTextChangedFromUI )
      {
        SyncTextAndValueProperties( true, Text );
      }
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    private void OnSpinnerSpin( object sender, SpinEventArgs e )
    {
      if( AllowSpin && !IsReadOnly )
        OnSpin( e );
    }

    #endregion //Event Handlers

    #region Events

    public event InputValidationErrorEventHandler InputValidationError;

    #region ValueChanged Event

    //Due to a bug in Visual Studio, you cannot create event handlers for generic T args in XAML, so I have to use object instead.
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent( "ValueChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<object> ), typeof( UpDownBase<T> ) );
    public event RoutedPropertyChangedEventHandler<object> ValueChanged
    {
      add
      {
        AddHandler( ValueChangedEvent, value );
      }
      remove
      {
        RemoveHandler( ValueChangedEvent, value );
      }
    }

    #endregion

    #endregion //Events

    #region Methods

    protected virtual void OnSpin( SpinEventArgs e )
    {
      if( e == null )
        throw new ArgumentNullException( "e" );

      if( e.Direction == SpinDirection.Increase )
        DoIncrement();
      else
        DoDecrement();
    }

    /// <summary>
    /// Performs an increment if conditions allow it.
    /// </summary>
    private void DoDecrement()
    {
      if( Spinner == null || ( Spinner.ValidSpinDirection & ValidSpinDirections.Decrease ) == ValidSpinDirections.Decrease )
      {
        OnDecrement();
      }
    }

    /// <summary>
    /// Performs a decrement if conditions allow it.
    /// </summary>
    private void DoIncrement()
    {
      if( Spinner == null || ( Spinner.ValidSpinDirection & ValidSpinDirections.Increase ) == ValidSpinDirections.Increase )
      {
        OnIncrement();
      }
    }

    private void TextBox_TextChanged( object sender, TextChangedEventArgs e )
    {
      _isTextChangedFromUI = true;

      TextBox textBox = sender as TextBox;
      Text = textBox.Text;

      _isTextChangedFromUI = false;
    }

    private void TextBox_LostFocus( object sender, RoutedEventArgs e )
    {
      CommitInput();
    }

    public void CommitInput()
    {
      this.SyncTextAndValueProperties( true, Text );
    }

    protected void SyncTextAndValueProperties(bool updateValueFromText, string text )
    {
      if( _isSyncingTextAndValueProperties )
        return;

      _isSyncingTextAndValueProperties = true;
      Exception error = null;

      if( updateValueFromText )
      {
        try
        {
          Value = ConvertTextToValue( Text );
        }
        catch( Exception e )
        {
          error = e;
        }
      }

      Text = ConvertValueToText();

      if( TextBox != null )
        TextBox.Text = Text;

      if( updateValueFromText )
      {
        if( ( error != null ) && ( InputValidationError != null ) )
        {
          InputValidationErrorEventArgs args = new InputValidationErrorEventArgs( error.Message );
          InputValidationError( this, args );
        }
      }

      _isSyncingTextAndValueProperties = false;
    }

    #region Abstract

    /// <summary>
    /// Converts the formatted text to a value.
    /// </summary>
    protected abstract T ConvertTextToValue( string text );

    /// <summary>
    /// Converts the value to formatted text.
    /// </summary>
    /// <returns></returns>
    protected abstract string ConvertValueToText();

    /// <summary>
    /// Called by OnSpin when the spin direction is SpinDirection.Increase.
    /// </summary>
    protected abstract void OnIncrement();

    /// <summary>
    /// Called by OnSpin when the spin direction is SpinDirection.Descrease.
    /// </summary>
    protected abstract void OnDecrement();

    /// <summary>
    /// Sets the valid spin directions.
    /// </summary>
    protected abstract void SetValidSpinDirection();

    #endregion //Abstract

    #endregion //Methods
  }
}
