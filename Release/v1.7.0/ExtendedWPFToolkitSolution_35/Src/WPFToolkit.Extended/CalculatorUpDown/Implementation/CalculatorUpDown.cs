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
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_CalculatorPopup, Type = typeof( Popup ) )]
  [TemplatePart( Name = PART_Calculator, Type = typeof( Calculator ) )]
  public class CalculatorUpDown : DecimalUpDown
  {
    private const string PART_CalculatorPopup = "PART_CalculatorPopup";
    private const string PART_Calculator = "PART_Calculator";

    #region Members

    private Popup _calculatorPopup;
    private Calculator _calculator;

    #endregion //Members

    #region Properties

    #region DisplayText

    public static readonly DependencyProperty DisplayTextProperty = DependencyProperty.Register( "DisplayText", typeof( string ), typeof( CalculatorUpDown ), new UIPropertyMetadata( "0" ) );
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

    #endregion //DisplayText

    #region EnterClosesCalculator

    public static readonly DependencyProperty EnterClosesCalculatorProperty = DependencyProperty.Register( "EnterClosesCalculator", typeof( bool ), typeof( CalculatorUpDown ), new UIPropertyMetadata( false ) );
    public bool EnterClosesCalculator
    {
      get
      {
        return ( bool )GetValue( EnterClosesCalculatorProperty );
      }
      set
      {
        SetValue( EnterClosesCalculatorProperty, value );
      }
    }

    #endregion //EnterClosesCalculator

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof( bool ), typeof( CalculatorUpDown ), new UIPropertyMetadata( false, OnIsOpenChanged ) );
    public bool IsOpen
    {
      get
      {
        return ( bool )GetValue( IsOpenProperty );
      }
      set
      {
        SetValue( IsOpenProperty, value );
      }
    }

    private static void OnIsOpenChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      CalculatorUpDown calculatorUpDown = o as CalculatorUpDown;
      if( calculatorUpDown != null )
        calculatorUpDown.OnIsOpenChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsOpenChanged( bool oldValue, bool newValue )
    {

    }

    #endregion //IsOpen

    #region Memory

    public static readonly DependencyProperty MemoryProperty = DependencyProperty.Register( "Memory", typeof( decimal ), typeof( CalculatorUpDown ), new UIPropertyMetadata( default( decimal ) ) );
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

    public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register( "Precision", typeof( int ), typeof( CalculatorUpDown ), new UIPropertyMetadata( 6 ) );
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

    #endregion //Properties

    #region Constructors

    static CalculatorUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( CalculatorUpDown ), new FrameworkPropertyMetadata( typeof( CalculatorUpDown ) ) );
    }

    public CalculatorUpDown()
    {
      Keyboard.AddKeyDownHandler( this, OnKeyDown );
      Mouse.AddPreviewMouseDownOutsideCapturedElementHandler( this, OnMouseDownOutsideCapturedElement );
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _calculatorPopup != null )
        _calculatorPopup.Opened -= CalculatorPopup_Opened;

      _calculatorPopup = GetTemplateChild( PART_CalculatorPopup ) as Popup;

      if( _calculatorPopup != null )
        _calculatorPopup.Opened += CalculatorPopup_Opened;

      _calculator = GetTemplateChild( PART_Calculator ) as Calculator;
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    void CalculatorPopup_Opened( object sender, EventArgs e )
    {
      _calculator.Focus();
    }

    private void OnKeyDown( object sender, KeyEventArgs e )
    {
      switch( e.Key )
      {
        case Key.Enter:
          {
            if( EnterClosesCalculator && IsOpen )
              CloseCalculatorUpDown();
            break;
          }
        case Key.Escape:
          {
            CloseCalculatorUpDown();
            e.Handled = true;
            break;
          }
        case Key.Tab:
          {
            CloseCalculatorUpDown();
            break;
          }
      }
    }

    private void OnMouseDownOutsideCapturedElement( object sender, MouseButtonEventArgs e )
    {
      CloseCalculatorUpDown();
    }

    #endregion //Event Handlers

    #region Methods

    private void CloseCalculatorUpDown()
    {
      if( IsOpen )
        IsOpen = false;
      ReleaseMouseCapture();
    }

    #endregion //Methods
  }
}
