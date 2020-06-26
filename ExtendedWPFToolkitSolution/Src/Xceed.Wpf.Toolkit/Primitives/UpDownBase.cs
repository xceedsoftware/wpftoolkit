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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core;
using Xceed.Wpf.Toolkit.Core.Input;
using System.Globalization;
using System.Windows.Threading;

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

    internal bool _isTextChangedFromUI;

    /// <summary>
    /// Flags if the Text and Value properties are in the process of being sync'd
    /// </summary>
    private bool _isSyncingTextAndValueProperties;
    private bool _internalValueSet;

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

    public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register( "AllowSpin", typeof( bool ), typeof( UpDownBase<T> ),

new UIPropertyMetadata( true ) );
    public bool AllowSpin
    {
      get
      {
        return (bool)GetValue( AllowSpinProperty );
      }
      set
      {
        SetValue( AllowSpinProperty, value );
      }
    }

    #endregion //AllowSpin

    #region ButtonSpinnerLocation

    public static readonly DependencyProperty ButtonSpinnerLocationProperty = DependencyProperty.Register( "ButtonSpinnerLocation", typeof( Location

), typeof( UpDownBase<T> ), new UIPropertyMetadata( Location.Right ) );
    public Location ButtonSpinnerLocation
    {
      get
      {
        return (Location)GetValue( ButtonSpinnerLocationProperty );
      }
      set
      {
        SetValue( ButtonSpinnerLocationProperty, value );
      }
    }

    #endregion //ButtonSpinnerLocation

    #region ClipValueToMinMax

    public static readonly DependencyProperty ClipValueToMinMaxProperty = DependencyProperty.Register( "ClipValueToMinMax", typeof( bool ), typeof(

UpDownBase<T> ), new UIPropertyMetadata( false ) );
    public bool ClipValueToMinMax
    {
      get
      {
        return (bool)GetValue( ClipValueToMinMaxProperty );
      }
      set
      {
        SetValue( ClipValueToMinMaxProperty, value );
      }
    }

    #endregion //ClipValueToMinMax

    #region DisplayDefaultValueOnEmptyText

    public static readonly DependencyProperty DisplayDefaultValueOnEmptyTextProperty = DependencyProperty.Register( "DisplayDefaultValueOnEmptyText",

typeof( bool ), typeof( UpDownBase<T> ), new UIPropertyMetadata( false, OnDisplayDefaultValueOnEmptyTextChanged ) );
    public bool DisplayDefaultValueOnEmptyText
    {
      get
      {
        return (bool)GetValue( DisplayDefaultValueOnEmptyTextProperty );
      }
      set
      {
        SetValue( DisplayDefaultValueOnEmptyTextProperty, value );
      }
    }

    private static void OnDisplayDefaultValueOnEmptyTextChanged( DependencyObject source, DependencyPropertyChangedEventArgs args )
    {
      ((UpDownBase<T>)source).OnDisplayDefaultValueOnEmptyTextChanged( (bool)args.OldValue, (bool)args.NewValue );
    }

    private void OnDisplayDefaultValueOnEmptyTextChanged( bool oldValue, bool newValue )
    {
      if( this.IsInitialized && string.IsNullOrEmpty( Text ) )
      {
        this.SyncTextAndValueProperties( false, Text );
      }
    }

    #endregion //DisplayDefaultValueOnEmptyText

    #region DefaultValue

    public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register( "DefaultValue", typeof( T ), typeof( UpDownBase<T>

), new UIPropertyMetadata( default( T ), OnDefaultValueChanged ) );
    public T DefaultValue
    {
      get
      {
        return (T)GetValue( DefaultValueProperty );
      }
      set
      {
        SetValue( DefaultValueProperty, value );
      }
    }

    private static void OnDefaultValueChanged( DependencyObject source, DependencyPropertyChangedEventArgs args )
    {
      ((UpDownBase<T>)source).OnDefaultValueChanged( (T)args.OldValue, (T)args.NewValue );
    }

    private void OnDefaultValueChanged( T oldValue, T newValue )
    {
      if( this.IsInitialized && string.IsNullOrEmpty( Text ) )
      {
        this.SyncTextAndValueProperties( true, Text );
      }
    }

    #endregion //DefaultValue

    #region Maximum

    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register( "Maximum", typeof( T ), typeof( UpDownBase<T> ), new

UIPropertyMetadata( default( T ), OnMaximumChanged, OnCoerceMaximum ) );
    public T Maximum
    {
      get
      {
        return (T)GetValue( MaximumProperty );
      }
      set
      {
        SetValue( MaximumProperty, value );
      }
    }

    private static void OnMaximumChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      UpDownBase<T> upDown = o as UpDownBase<T>;
      if( upDown != null )
        upDown.OnMaximumChanged( (T)e.OldValue, (T)e.NewValue );
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
      UpDownBase<T> upDown = d as UpDownBase<T>;
      if( upDown != null )
        return upDown.OnCoerceMaximum( (T)baseValue );

      return baseValue;
    }

    protected virtual T OnCoerceMaximum( T baseValue )
    {
      return baseValue;
    }

    #endregion //Maximum

    #region Minimum

    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register( "Minimum", typeof( T ), typeof( UpDownBase<T> ), new

UIPropertyMetadata( default( T ), OnMinimumChanged, OnCoerceMinimum ) );
    public T Minimum
    {
      get
      {
        return (T)GetValue( MinimumProperty );
      }
      set
      {
        SetValue( MinimumProperty, value );
      }
    }

    private static void OnMinimumChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      UpDownBase<T> upDown = o as UpDownBase<T>;
      if( upDown != null )
        upDown.OnMinimumChanged( (T)e.OldValue, (T)e.NewValue );
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
      UpDownBase<T> upDown = d as UpDownBase<T>;
      if( upDown != null )
        return upDown.OnCoerceMinimum( (T)baseValue );

      return baseValue;
    }

    protected virtual T OnCoerceMinimum( T baseValue )
    {
      return baseValue;
    }

    #endregion //Minimum

    #region MouseWheelActiveTrigger

    /// <summary>
    /// Identifies the MouseWheelActiveTrigger dependency property
    /// </summary>
    public static readonly DependencyProperty MouseWheelActiveTriggerProperty = DependencyProperty.Register( "MouseWheelActiveTrigger", typeof(

MouseWheelActiveTrigger ), typeof( UpDownBase<T> ), new UIPropertyMetadata( MouseWheelActiveTrigger.FocusedMouseOver ) );

    /// <summary>
    /// Get or set when the mouse wheel event should affect the value.
    /// </summary>
    public MouseWheelActiveTrigger MouseWheelActiveTrigger
    {
      get
      {
        return (MouseWheelActiveTrigger)GetValue( MouseWheelActiveTriggerProperty );
      }
      set
      {
        SetValue( MouseWheelActiveTriggerProperty, value );
      }
    }

    #endregion //MouseWheelActiveTrigger

    #region MouseWheelActiveOnFocus

    [Obsolete( "Use MouseWheelActiveTrigger property instead" )]
    public static readonly DependencyProperty MouseWheelActiveOnFocusProperty = DependencyProperty.Register( "MouseWheelActiveOnFocus", typeof( bool

), typeof( UpDownBase<T> ), new UIPropertyMetadata( true, OnMouseWheelActiveOnFocusChanged ) );

    [Obsolete( "Use MouseWheelActiveTrigger property instead" )]
    public bool MouseWheelActiveOnFocus
    {
      get
      {
#pragma warning disable 618
        return (bool)GetValue( MouseWheelActiveOnFocusProperty );
#pragma warning restore 618
      }
      set
      {
#pragma warning disable 618
        SetValue( MouseWheelActiveOnFocusProperty, value );
#pragma warning restore 618
      }
    }

    private static void OnMouseWheelActiveOnFocusChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      UpDownBase<T> upDownBase = o as UpDownBase<T>;
      if( upDownBase != null )
        upDownBase.MouseWheelActiveTrigger = ((bool)e.NewValue)
          ? MouseWheelActiveTrigger.FocusedMouseOver
          : MouseWheelActiveTrigger.MouseOver;
    }

    #endregion //MouseWheelActiveOnFocus

    #region ShowButtonSpinner

    public static readonly DependencyProperty ShowButtonSpinnerProperty = DependencyProperty.Register( "ShowButtonSpinner", typeof( bool ), typeof(

UpDownBase<T> ), new UIPropertyMetadata( true ) );
    public bool ShowButtonSpinner
    {
      get
      {
        return (bool)GetValue( ShowButtonSpinnerProperty );
      }
      set
      {
        SetValue( ShowButtonSpinnerProperty, value );
      }
    }

    #endregion //ShowButtonSpinner

    #region UpdateValueOnEnterKey

    public static readonly DependencyProperty UpdateValueOnEnterKeyProperty = DependencyProperty.Register( "UpdateValueOnEnterKey", typeof( bool ), typeof( UpDownBase<T> ), 
      new FrameworkPropertyMetadata( false, OnUpdateValueOnEnterKeyChanged ) );
    public bool UpdateValueOnEnterKey
    {
      get
      {
        return (bool)GetValue( UpdateValueOnEnterKeyProperty );
      }
      set
      {
        SetValue( UpdateValueOnEnterKeyProperty, value );
      }
    }

    private static void OnUpdateValueOnEnterKeyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var upDownBase = o as UpDownBase<T>;
      if( upDownBase != null )
        upDownBase.OnUpdateValueOnEnterKeyChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnUpdateValueOnEnterKeyChanged( bool oldValue, bool newValue )
    {
    }

    #endregion //UpdateValueOnEnterKey

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( T ), typeof( UpDownBase<T> ), 
      new FrameworkPropertyMetadata( default( T ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged, OnCoerceValue, false, UpdateSourceTrigger.PropertyChanged ) );
    public T Value
    {
      get
      {
        return (T)GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private void SetValueInternal( T value )
    {
      _internalValueSet = true;
      try
      {
        this.Value = value;
      }
      finally
      {
        _internalValueSet = false;
      }
    }

    private static object OnCoerceValue( DependencyObject o, object basevalue )
    {
      return ((UpDownBase<T>)o).OnCoerceValue( basevalue );
    }

    protected virtual object OnCoerceValue( object newValue )
    {
      return newValue;
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      UpDownBase<T> upDownBase = o as UpDownBase<T>;
      if( upDownBase != null )
        upDownBase.OnValueChanged( (T)e.OldValue, (T)e.NewValue );
    }

    protected virtual void OnValueChanged( T oldValue, T newValue )
    {
      if( !_internalValueSet && this.IsInitialized )
      {
        SyncTextAndValueProperties( false, null, true );
      }

      SetValidSpinDirection();

      this.RaiseValueChangedEvent( oldValue, newValue );
    }

    #endregion //Value

    #endregion //Properties

    #region Constructors

    internal UpDownBase()
    {

      this.AddHandler( Mouse.PreviewMouseDownOutsideCapturedElementEvent, new RoutedEventHandler( this.HandleClickOutsideOfControlWithMouseCapture ),

true );
      this.IsKeyboardFocusWithinChanged += this.UpDownBase_IsKeyboardFocusWithinChanged;
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

      if( TextBox != null )
      {
        TextBox.TextChanged -= new TextChangedEventHandler( TextBox_TextChanged );
        TextBox.RemoveHandler( Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler( this.TextBox_PreviewMouseDown ) );
      }

      TextBox = GetTemplateChild( PART_TextBox ) as TextBox;

      if( TextBox != null )
      {
        TextBox.Text = Text;
        TextBox.TextChanged += new TextChangedEventHandler( TextBox_TextChanged );
        TextBox.AddHandler( Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler( this.TextBox_PreviewMouseDown ), true );
      }

      if( Spinner != null )
        Spinner.Spin -= OnSpinnerSpin;

      Spinner = GetTemplateChild( PART_Spinner ) as Spinner;

      if( Spinner != null )
        Spinner.Spin += OnSpinnerSpin;

      SetValidSpinDirection();
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      switch( e.Key )
      {
        case Key.Enter:
          {
            // Commit Text on "Enter" to raise Error event 
            bool commitSuccess = CommitInput();
            //Only handle if an exception is detected (Commit fails)
            e.Handled = !commitSuccess;
            break;
          }
      }
    }

    protected override void OnTextChanged( string oldValue, string newValue )
    {
      if( this.IsInitialized )
      {
        // When text is typed, if UpdateValueOnEnterKey is true, 
        // Sync Value on Text only when Enter Key is pressed.
        if( this.UpdateValueOnEnterKey )
        {
          if( !_isTextChangedFromUI )
          {
            this.SyncTextAndValueProperties( true, Text );
          }
        }
        else
        {
          this.SyncTextAndValueProperties( true, Text );
        }
      }
    }

    protected override void OnCultureInfoChanged( CultureInfo oldValue, CultureInfo newValue )
    {
      if( IsInitialized )
      {
        SyncTextAndValueProperties( false, null );
      }
    }

    protected override void OnReadOnlyChanged( bool oldValue, bool newValue )
    {
      SetValidSpinDirection();
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    private void TextBox_PreviewMouseDown( object sender, RoutedEventArgs e )
    {
      if( this.MouseWheelActiveTrigger == Primitives.MouseWheelActiveTrigger.Focused )
      {
        //Capture the spinner when user clicks on the control.
        if( Mouse.Captured != this.Spinner )
        {
          //Delay the capture to let the DateTimeUpDown select a new DateTime part.
          this.Dispatcher.BeginInvoke( DispatcherPriority.Input, new Action( () =>
          {
            Mouse.Capture( this.Spinner );
          }
          ) );
        }
      }
    }

    private void HandleClickOutsideOfControlWithMouseCapture( object sender, RoutedEventArgs e )
    {
      if( Mouse.Captured is Spinner )
      {
        //Release the captured spinner when user clicks away from spinner.
        this.Spinner.ReleaseMouseCapture();
      }
    }

    private void OnSpinnerSpin( object sender, SpinEventArgs e )
    {
      if( AllowSpin && !IsReadOnly )
      {
        var activeTrigger = this.MouseWheelActiveTrigger;
        bool spin = !e.UsingMouseWheel;
        spin |= (activeTrigger == MouseWheelActiveTrigger.MouseOver);
        spin |= ( (TextBox  != null) && TextBox.IsFocused && (activeTrigger == MouseWheelActiveTrigger.FocusedMouseOver));
        spin |= ( (TextBox != null) && TextBox.IsFocused && (activeTrigger == MouseWheelActiveTrigger.Focused) && (Mouse.Captured is Spinner));

        if( spin )
        {
          e.Handled = true;
          OnSpin( e );
        }
      }
    }

    #endregion //Event Handlers

    #region Events

    public event InputValidationErrorEventHandler InputValidationError;

    public event EventHandler<SpinEventArgs> Spinned;

    #region ValueChanged Event

    //Due to a bug in Visual Studio, you cannot create event handlers for generic T args in XAML, so I have to use object instead.
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent( "ValueChanged", RoutingStrategy.Bubble, typeof(

RoutedPropertyChangedEventHandler<object> ), typeof( UpDownBase<T> ) );
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

      // Raise the Spinned event to user
      EventHandler<SpinEventArgs> handler = this.Spinned;
      if( handler != null )
      {
        handler( this, e );
      }

      if( e.Direction == SpinDirection.Increase )
        DoIncrement();
      else
        DoDecrement();
    }

    protected virtual void RaiseValueChangedEvent( T oldValue, T newValue )
    {
      RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>( oldValue, newValue );
      args.RoutedEvent = ValueChangedEvent;
      RaiseEvent( args );
    }

    protected override void OnInitialized( EventArgs e )
    {
      base.OnInitialized( e );
      // When both Value and Text are initialized, Value has priority.
      // To be sure that the value is not initialized, it should
      // have no local value, no binding, and equal to the default value.
      bool updateValueFromText =
        (this.ReadLocalValue( ValueProperty ) == DependencyProperty.UnsetValue)
        && (BindingOperations.GetBinding( this, ValueProperty ) == null)
        && (object.Equals( this.Value, ValueProperty.DefaultMetadata.DefaultValue ));

      this.SyncTextAndValueProperties( updateValueFromText, Text, !updateValueFromText );
    }

    /// <summary>
    /// Performs an increment if conditions allow it.
    /// </summary>
    internal void DoDecrement()
    {
      if( Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Decrease) == ValidSpinDirections.Decrease )
      {
        OnDecrement();
      }
    }

    /// <summary>
    /// Performs a decrement if conditions allow it.
    /// </summary>
    internal void DoIncrement()
    {
      if( Spinner == null || (Spinner.ValidSpinDirection & ValidSpinDirections.Increase) == ValidSpinDirections.Increase )
      {
        OnIncrement();
      }
    }

    private void TextBox_TextChanged( object sender, TextChangedEventArgs e )
    {
      if( !this.IsKeyboardFocusWithin )
        return;

      try
      {
        _isTextChangedFromUI = true;
        Text = ((TextBox)sender).Text;
      }
      finally
      {
        _isTextChangedFromUI = false;
      }
    }

    private void UpDownBase_IsKeyboardFocusWithinChanged( object sender, DependencyPropertyChangedEventArgs e )
    {
      if( !(bool)e.NewValue )
      {
        this.CommitInput();
      }
    }

    private void RaiseInputValidationError( Exception e )
    {
      if( InputValidationError != null )
      {
        InputValidationErrorEventArgs args = new InputValidationErrorEventArgs( e );
        InputValidationError( this, args );
        if( args.ThrowException )
        {
          throw args.Exception;
        }
      }
    }

    public virtual bool CommitInput()
    {
      return this.SyncTextAndValueProperties( true, Text );
    }

    protected bool SyncTextAndValueProperties( bool updateValueFromText, string text )
    {
      return this.SyncTextAndValueProperties( updateValueFromText, text, false );
    }

    private bool SyncTextAndValueProperties( bool updateValueFromText, string text, bool forceTextUpdate )
    {
      if( _isSyncingTextAndValueProperties )
        return true;

      _isSyncingTextAndValueProperties = true;
      bool parsedTextIsValid = true;
      try
      {
        if( updateValueFromText )
        {
          if( string.IsNullOrEmpty( text ) )
          {
            // An empty input sets the value to the default value.
            this.SetValueInternal( this.DefaultValue );
          }
          else
          {
            try
            {
              T newValue = this.ConvertTextToValue( text );
              if( !object.Equals( newValue, this.Value ) )
              {
                this.SetValueInternal( newValue );
              }
            }
            catch( Exception e )
            {
              parsedTextIsValid = false;

              // From the UI, just allow any input.
              if( !_isTextChangedFromUI )
              {
                // This call may throw an exception. 
                // See RaiseInputValidationError() implementation.
                this.RaiseInputValidationError( e );
              }
            }
          }
        }

        // Do not touch the ongoing text input from user.
        if( !_isTextChangedFromUI )
        {
          // Don't replace the empty Text with the non-empty representation of DefaultValue.
          bool shouldKeepEmpty = !forceTextUpdate && string.IsNullOrEmpty( Text ) && object.Equals( Value, DefaultValue ) && !this.DisplayDefaultValueOnEmptyText;
          if( !shouldKeepEmpty )
          {
            string newText = ConvertValueToText();
            if( !object.Equals( this.Text, newText ) )
            {
              Text = newText;
            }
          }

          // Sync Text and textBox
          if( TextBox != null )
            TextBox.Text = Text;
        }

        if( _isTextChangedFromUI && !parsedTextIsValid )
        {
          // Text input was made from the user and the text
          // repesents an invalid value. Disable the spinner
          // in this case.
          if( Spinner != null )
          {
            Spinner.ValidSpinDirection = ValidSpinDirections.None;
          }
        }
        else
        {
          this.SetValidSpinDirection();
        }
      }
      finally
      {
        _isSyncingTextAndValueProperties = false;
      }
      return parsedTextIsValid;
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
