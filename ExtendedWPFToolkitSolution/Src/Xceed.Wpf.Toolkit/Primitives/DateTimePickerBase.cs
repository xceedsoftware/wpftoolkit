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
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit.Core.Utilities;
#if VS2008
using Microsoft.Windows.Controls;
using Microsoft.Windows.Controls.Primitives;
#endif

namespace Xceed.Wpf.Toolkit.Primitives
{
  [TemplatePart( Name = PART_Popup, Type = typeof( Popup ) )]
  public class DateTimePickerBase : DateTimeUpDown
  {
    private const string PART_Popup = "PART_Popup";

    #region Members

    private Popup _popup;
    private DateTime? _initialValue;

    #endregion //Members

    #region Properties

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof( bool ), typeof( DateTimePickerBase ), new UIPropertyMetadata( false, OnIsOpenChanged ) );
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

    private static void OnIsOpenChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
    {
      DateTimePickerBase dateTimePicker = ( DateTimePickerBase )d;
      if( dateTimePicker != null )
        dateTimePicker.OnIsOpenChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsOpenChanged( bool oldValue, bool newValue )
    {
      if( newValue )
        _initialValue = Value;
    }

    #endregion //IsOpen

    #region ShowDropDownButton

    public static readonly DependencyProperty ShowDropDownButtonProperty = DependencyProperty.Register( "ShowDropDownButton", typeof( bool ), typeof( DateTimePickerBase ), new UIPropertyMetadata( true ) );
    public bool ShowDropDownButton
    {
      get
      {
        return ( bool )GetValue( ShowDropDownButtonProperty );
      }
      set
      {
        SetValue( ShowDropDownButtonProperty, value );
      }
    }

    #endregion //ShowDropDownButton

    #endregion //Properties

    #region Constructors

    public DateTimePickerBase()
    {
      AddHandler( UIElement.KeyDownEvent, new KeyEventHandler( HandleKeyDown ), true );
      Mouse.AddPreviewMouseDownOutsideCapturedElementHandler( this, OnMouseDownOutsideCapturedElement );
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _popup != null )
      {
        _popup.Opened -= this.Popup_Opened;
      }

      _popup = this.GetTemplateChild( PART_Popup ) as Popup;

      if( _popup != null )
      {
        _popup.Opened += this.Popup_Opened;
      }
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    protected virtual void HandleKeyDown( object sender, KeyEventArgs e )
    {
      if( !IsOpen )
      {
        if( KeyboardUtilities.IsKeyModifyingPopupState( e ) )
        {
          IsOpen = true;
          // Calendar will get focus in Calendar_Loaded().
          e.Handled = true;
        }
      }
      else
      {
        if( KeyboardUtilities.IsKeyModifyingPopupState( e ) )
        {
          ClosePopup( true );
          e.Handled = true;
        }
        else if( e.Key == Key.Enter )
        {
          ClosePopup( true );
          e.Handled = true;
        }
        else if( e.Key == Key.Escape )
        {
          // Avoid setting the "Value" property when no change has occurred.
          // The original value may not be a local value. Setting
          // it, even with the same value, will override a one-way binding.
          if( !object.Equals( this.Value, _initialValue ) )
          {
            this.Value = _initialValue;
          }
          ClosePopup( true );
          e.Handled = true;
        }
      }
    }

    private void OnMouseDownOutsideCapturedElement( object sender, MouseButtonEventArgs e )
    {
      ClosePopup( true );
    }

    protected virtual void Popup_Opened( object sender, EventArgs e )
    {
    }

    #endregion //Event Handlers

    #region Methods

    protected void ClosePopup( bool isFocusOnTextBox )
    {
      if( IsOpen )
      {
        IsOpen = false;
      }

      ReleaseMouseCapture();

      if( isFocusOnTextBox && ( TextBox != null ) )
        TextBox.Focus();
    }

    #endregion //Methods
  }
}
