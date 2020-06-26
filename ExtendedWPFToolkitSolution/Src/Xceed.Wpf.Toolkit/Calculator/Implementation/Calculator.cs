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
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_CalculatorButtonPanel, Type = typeof( ContentControl ) )]
  public class Calculator : Control
  {
    private const string PART_CalculatorButtonPanel = "PART_CalculatorButtonPanel";

    #region Members

    private ContentControl _buttonPanel;
    private bool _showNewNumber = true;
    private decimal _previousValue;
    private Operation _lastOperation = Operation.None;
    private readonly Dictionary<Button, DispatcherTimer> _timers = new Dictionary<Button, DispatcherTimer>();

    #endregion //Members

    #region Enumerations

    public enum CalculatorButtonType
    {
      Add,
      Back,
      Cancel,
      Clear,
      Decimal,
      Divide,
      Eight,
      Equal,
      Five,
      Four,
      Fraction,
      MAdd,
      MC,
      MR,
      MS,
      MSub,
      Multiply,
      Negate,
      Nine,
      None,
      One,
      Percent,
      Seven,
      Six,
      Sqrt,
      Subtract,
      Three,
      Two,
      Zero
    }

    public enum Operation
    {
      Add,
      Subtract,
      Divide,
      Multiply,
      Percent,
      Sqrt,
      Fraction,
      None,
      Clear,
      Negate
    }

    #endregion //Enumerations

    #region Properties

    #region CalculatorButtonPanelTemplate

    public static readonly DependencyProperty CalculatorButtonPanelTemplateProperty = DependencyProperty.Register( "CalculatorButtonPanelTemplate"
      , typeof( ControlTemplate ), typeof( Calculator ), new UIPropertyMetadata( null ) );
    public ControlTemplate CalculatorButtonPanelTemplate
    {
      get
      {
        return (ControlTemplate)GetValue( CalculatorButtonPanelTemplateProperty );
      }
      set
      {
        SetValue( CalculatorButtonPanelTemplateProperty, value );
      }
    }

    #endregion //CalculatorButtonPanelTemplate

    #region CalculatorButtonType

    public static readonly DependencyProperty CalculatorButtonTypeProperty = DependencyProperty.RegisterAttached( "CalculatorButtonType", typeof( CalculatorButtonType ), typeof( Calculator ), new UIPropertyMetadata( CalculatorButtonType.None, OnCalculatorButtonTypeChanged ) );
    public static CalculatorButtonType GetCalculatorButtonType( DependencyObject target )
    {
      return ( CalculatorButtonType )target.GetValue( CalculatorButtonTypeProperty );
    }
    public static void SetCalculatorButtonType( DependencyObject target, CalculatorButtonType value )
    {
      target.SetValue( CalculatorButtonTypeProperty, value );
    }
    private static void OnCalculatorButtonTypeChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      OnCalculatorButtonTypeChanged( o, ( CalculatorButtonType )e.OldValue, ( CalculatorButtonType )e.NewValue );
    }
    private static void OnCalculatorButtonTypeChanged( DependencyObject o, CalculatorButtonType oldValue, CalculatorButtonType newValue )
    {
      Button button = o as Button;
      button.CommandParameter = newValue;
      if( button.Content == null )
      {
        button.Content = CalculatorUtilities.GetCalculatorButtonContent( newValue );
      }
    }

    #endregion //CalculatorButtonType

    #region DisplayText

    public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register( "DisplayText", typeof( string ), typeof( Calculator ), new UIPropertyMetadata( "0", OnDisplayTextChanged ) );
    public string DisplayText
    {
      get
      {
        return ( string )GetValue( DisplayTextProperty );
      }
      set
      {
        SetValue( DisplayTextProperty, value );
      }
    }

    private static void OnDisplayTextChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Calculator calculator = o as Calculator;
      if( calculator != null )
        calculator.OnDisplayTextChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnDisplayTextChanged( string oldValue, string newValue )
    {
      // TODO: Add your property changed side-effects. Descendants can override as well.
    }

    #endregion //DisplayText

    #region Memory

    public static readonly DependencyProperty MemoryProperty = DependencyProperty.Register( "Memory", typeof( decimal ), typeof( Calculator ), new UIPropertyMetadata( default( decimal ) ) );
    public decimal Memory
    {
      get
      {
        return ( decimal )GetValue( MemoryProperty );
      }
      set
      {
        SetValue( MemoryProperty, value );
      }
    }

    #endregion //Memory

    #region Precision

    public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register( "Precision", typeof( int ), typeof( Calculator ), new UIPropertyMetadata( 6 ) );
    public int Precision
    {
      get
      {
        return ( int )GetValue( PrecisionProperty );
      }
      set
      {
        SetValue( PrecisionProperty, value );
      }
    }

    #endregion //Precision

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( decimal? ), typeof( Calculator ), new FrameworkPropertyMetadata( default( decimal ), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged ) );
    public decimal? Value
    {
      get
      {
        return ( decimal? )GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      Calculator calculator = o as Calculator;
      if( calculator != null )
        calculator.OnValueChanged( ( decimal? )e.OldValue, ( decimal? )e.NewValue );
    }

    protected virtual void OnValueChanged( decimal? oldValue, decimal? newValue )
    {
      SetDisplayText( newValue );

      RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>( oldValue, newValue );
      args.RoutedEvent = ValueChangedEvent;
      RaiseEvent( args );
    }

    #endregion //Value

    #endregion //Properties

    #region Constructors

    static Calculator()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( Calculator ), new FrameworkPropertyMetadata( typeof( Calculator ) ) );
    }

    public Calculator()
    {

      CommandBindings.Add( new CommandBinding( CalculatorCommands.CalculatorButtonClick, ExecuteCalculatorButtonClick ) );
      AddHandler( MouseDownEvent, new MouseButtonEventHandler( Calculator_OnMouseDown ), true );
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      _buttonPanel = GetTemplateChild( PART_CalculatorButtonPanel ) as ContentControl;
    }

    protected override void OnTextInput( TextCompositionEventArgs e )
    {
      var buttonType = CalculatorUtilities.GetCalculatorButtonTypeFromText( e.Text );
      if( buttonType != CalculatorButtonType.None )
      {
        SimulateCalculatorButtonClick( buttonType );
        ProcessCalculatorButton( buttonType );
      }
    }




    #endregion //Base Class Overrides

    #region Event Handlers

    private void Calculator_OnMouseDown( object sender, MouseButtonEventArgs e )
    {
      if( !IsFocused )
      {
        Focus();
        e.Handled = true;
      }
    }

    void Timer_Tick( object sender, EventArgs e )
    {
      DispatcherTimer timer = ( DispatcherTimer )sender;
      timer.Stop();
      timer.Tick -= Timer_Tick;

      if( _timers.ContainsValue( timer ) )
      {
        var button = _timers.Where( x => x.Value == timer ).Select( x => x.Key ).FirstOrDefault();
        if( button != null )
        {
          VisualStateManager.GoToState( button, button.IsMouseOver ? "MouseOver" : "Normal", true );
          _timers.Remove( button );
        }
      }
    }

    #endregion //Event Handlers

    #region Methods

    internal void InitializeToValue(decimal? value)
    {
      _previousValue = 0;
      _lastOperation = Operation.None;
      _showNewNumber = true;
      Value = value;
      // Since the display text may be out of sync
      // with "Value", this call will force the
      // text update if Value was already equal to
      // the value parameter.
      this.SetDisplayText( value );
    }

    private void Calculate()
    {
      if( _lastOperation == Operation.None )
        return;

      try
      {
        Value = Decimal.Round( CalculateValue( _lastOperation ), Precision );
        SetDisplayText( Value ); //Set DisplayText even when Value doesn't change
      }
      catch
      {
        Value = null;
        DisplayText = "ERROR";
      }
    }

    private void SetDisplayText( decimal? newValue )
    {
      if( newValue.HasValue && ( newValue.Value != 0 ) )
        DisplayText = newValue.ToString();
      else
        DisplayText = "0";
    }

    private void Calculate( Operation newOperation )
    {
      if( !_showNewNumber )
        Calculate();

      _lastOperation = newOperation;
    }

    private void Calculate( Operation currentOperation, Operation newOperation )
    {
      _lastOperation = currentOperation;
      Calculate();
      _lastOperation = newOperation;
    }

    private decimal CalculateValue( Operation operation )
    {
      decimal newValue = decimal.Zero;
      decimal currentValue = CalculatorUtilities.ParseDecimal( DisplayText );

      switch( operation )
      {
        case Operation.Add:
          newValue = CalculatorUtilities.Add( _previousValue, currentValue );
          break;
        case Operation.Subtract:
          newValue = CalculatorUtilities.Subtract( _previousValue, currentValue );
          break;
        case Operation.Multiply:
          newValue = CalculatorUtilities.Multiply( _previousValue, currentValue );
          break;
        case Operation.Divide:
          newValue = CalculatorUtilities.Divide( _previousValue, currentValue );
          break;
        //case Operation.Percent:
        //    newValue = CalculatorUtilities.Percent(_previousValue, currentValue);
        //    break;
        case Operation.Sqrt:
          newValue = CalculatorUtilities.SquareRoot( currentValue );
          break;
        case Operation.Fraction:
          newValue = CalculatorUtilities.Fraction( currentValue );
          break;
        case Operation.Negate:
          newValue = CalculatorUtilities.Negate( currentValue );
          break;
        default:
          newValue = decimal.Zero;
          break;
      }

      return newValue;
    }

    void ProcessBackKey()
    {
      string displayText;
      if( DisplayText.Length > 1 && !( DisplayText.Length == 2 && DisplayText[ 0 ] == '-' ) )
      {
        displayText = DisplayText.Remove( DisplayText.Length - 1, 1 );
      }
      else
      {
        displayText = "0";
        _showNewNumber = true;
      }

      DisplayText = displayText;
    }

    private void ProcessCalculatorButton( CalculatorButtonType buttonType )
    {
      if( CalculatorUtilities.IsDigit( buttonType ) )
        ProcessDigitKey( buttonType );
      else if( ( CalculatorUtilities.IsMemory( buttonType ) ) )
        ProcessMemoryKey( buttonType );
      else
        ProcessOperationKey( buttonType );
    }

    private void ProcessDigitKey( CalculatorButtonType buttonType )
    {
      if( _showNewNumber )
        DisplayText = CalculatorUtilities.GetCalculatorButtonContent( buttonType );
      else
        DisplayText += CalculatorUtilities.GetCalculatorButtonContent( buttonType );

      _showNewNumber = false;
    }

    private void ProcessMemoryKey( Calculator.CalculatorButtonType buttonType )
    {
      decimal currentValue = CalculatorUtilities.ParseDecimal( DisplayText );

      _showNewNumber = true;

      switch( buttonType )
      {
        case Calculator.CalculatorButtonType.MAdd:
          Memory += currentValue;
          break;
        case Calculator.CalculatorButtonType.MC:
          Memory = decimal.Zero;
          break;
        case Calculator.CalculatorButtonType.MR:
          DisplayText = Memory.ToString();
          _showNewNumber = false;
          break;
        case Calculator.CalculatorButtonType.MS:
          Memory = currentValue;
          break;
        case Calculator.CalculatorButtonType.MSub:
          Memory -= currentValue;
          break;
        default:
          break;
      }
    }

    private void ProcessOperationKey( CalculatorButtonType buttonType )
    {
      switch( buttonType )
      {
        case CalculatorButtonType.Add:
          Calculate( Operation.Add );
          break;
        case CalculatorButtonType.Subtract:
          Calculate( Operation.Subtract );
          break;
        case CalculatorButtonType.Multiply:
          Calculate( Operation.Multiply );
          break;
        case CalculatorButtonType.Divide:
          Calculate( Operation.Divide );
          break;
        case CalculatorButtonType.Percent:
          if( _lastOperation != Operation.None )
          {
            decimal currentValue = CalculatorUtilities.ParseDecimal( DisplayText );
            decimal newValue = CalculatorUtilities.Percent( _previousValue, currentValue );
            DisplayText = newValue.ToString();
          }
          else
          {
            DisplayText = "0";
            _showNewNumber = true;
          }
          return;
        case CalculatorButtonType.Sqrt:
          Calculate( Operation.Sqrt, Operation.None );
          break;
        case CalculatorButtonType.Fraction:
          Calculate( Operation.Fraction, Operation.None );
          break;
        case CalculatorButtonType.Negate:
          Calculate( Operation.Negate, Operation.None );
          break;
        case CalculatorButtonType.Equal:
          Calculate( Operation.None );
          break;
        case CalculatorButtonType.Clear:
          Calculate( Operation.Clear, Operation.None );
          break;
        case CalculatorButtonType.Cancel:
          DisplayText = _previousValue.ToString();
          _lastOperation = Operation.None;
          _showNewNumber = true;
          return;
        case CalculatorButtonType.Back:
          ProcessBackKey();
          return;
        default:
          break;
      }

      Decimal.TryParse( DisplayText, out _previousValue );
      _showNewNumber = true;
    }

    private void SimulateCalculatorButtonClick( CalculatorButtonType buttonType )
    {
      var button = CalculatorUtilities.FindButtonByCalculatorButtonType( _buttonPanel, buttonType );
      if( button != null )
      {
        VisualStateManager.GoToState( button, "Pressed", true );
        DispatcherTimer timer;
        if( _timers.ContainsKey( button ) )
        {
          timer = _timers[ button ];
          timer.Stop();
        }
        else
        {
          timer = new DispatcherTimer();
          timer.Interval = TimeSpan.FromMilliseconds( 100 );
          timer.Tick += Timer_Tick;
          _timers.Add( button, timer );
        }

        timer.Start();
      }
    }

    #endregion //Methods

    #region Events

    //Due to a bug in Visual Studio, you cannot create event handlers for nullable args in XAML, so I have to use object instead.
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent( "ValueChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<object> ), typeof( Calculator ) );
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

    #endregion //Events

    #region Commands

    private void ExecuteCalculatorButtonClick( object sender, ExecutedRoutedEventArgs e )
    {
      var buttonType = ( CalculatorButtonType )e.Parameter;
      ProcessCalculatorButton( buttonType );
    }

    #endregion //Commands
  }
}
