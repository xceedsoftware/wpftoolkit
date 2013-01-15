/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_ResizeThumb, Type = typeof( Thumb ) )]
  public class MultiLineTextEditor : ContentControl
  {
    private const string PART_ResizeThumb = "PART_ResizeThumb";

    #region Members

    Thumb _resizeThumb;

    #endregion //Members

    #region Properties

    public static readonly DependencyProperty DropDownHeightProperty = DependencyProperty.Register( "DropDownHeight", typeof( double ), typeof( MultiLineTextEditor ), new UIPropertyMetadata( 150.0 ) );
    public double DropDownHeight
    {
      get
      {
        return ( double )GetValue( DropDownHeightProperty );
      }
      set
      {
        SetValue( DropDownHeightProperty, value );
      }
    }

    public static readonly DependencyProperty DropDownWidthProperty = DependencyProperty.Register( "DropDownWidth", typeof( double ), typeof( MultiLineTextEditor ), new UIPropertyMetadata( 200.0 ) );
    public double DropDownWidth
    {
      get
      {
        return ( double )GetValue( DropDownWidthProperty );
      }
      set
      {
        SetValue( DropDownWidthProperty, value );
      }
    }

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof( bool ), typeof( MultiLineTextEditor ), new UIPropertyMetadata( false, OnIsOpenChanged ) );
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
      MultiLineTextEditor multiLineTextEditor = o as MultiLineTextEditor;
      if( multiLineTextEditor != null )
        multiLineTextEditor.OnIsOpenChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnIsOpenChanged( bool oldValue, bool newValue )
    {

    }

    #endregion //IsOpen

    public static readonly DependencyProperty IsSpellCheckEnabledProperty = DependencyProperty.Register( "IsSpellCheckEnabled", typeof( bool ), typeof( MultiLineTextEditor ), new UIPropertyMetadata( false ) );
    public bool IsSpellCheckEnabled
    {
      get
      {
        return ( bool )GetValue( IsSpellCheckEnabledProperty );
      }
      set
      {
        SetValue( IsSpellCheckEnabledProperty, value );
      }
    }


    #region Text

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register( "Text", typeof( string ), typeof( MultiLineTextEditor ), new FrameworkPropertyMetadata( String.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged ) );
    public string Text
    {
      get
      {
        return ( string )GetValue( TextProperty );
      }
      set
      {
        SetValue( TextProperty, value );
      }
    }

    private static void OnTextChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      MultiLineTextEditor textEditor = o as MultiLineTextEditor;
      if( textEditor != null )
        textEditor.OnTextChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnTextChanged( string oldValue, string newValue )
    {

    }

    #endregion //Text

    public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register( "TextAlignment", typeof( TextAlignment ), typeof( MultiLineTextEditor ), new UIPropertyMetadata( TextAlignment.Left ) );
    public TextAlignment TextAlignment
    {
      get
      {
        return ( TextAlignment )GetValue( TextAlignmentProperty );
      }
      set
      {
        SetValue( TextAlignmentProperty, value );
      }
    }

    public static readonly DependencyProperty TextWrappingProperty = DependencyProperty.Register( "TextWrapping", typeof( TextWrapping ), typeof( MultiLineTextEditor ), new UIPropertyMetadata( TextWrapping.NoWrap ) );
    public TextWrapping TextWrapping
    {
      get
      {
        return ( TextWrapping )GetValue( TextWrappingProperty );
      }
      set
      {
        SetValue( TextWrappingProperty, value );
      }
    }


    #endregion //Properties

    #region Constructors

    static MultiLineTextEditor()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( MultiLineTextEditor ), new FrameworkPropertyMetadata( typeof( MultiLineTextEditor ) ) );
    }

    public MultiLineTextEditor()
    {
      Keyboard.AddKeyDownHandler( this, OnKeyDown );
      Mouse.AddPreviewMouseDownOutsideCapturedElementHandler( this, OnMouseDownOutsideCapturedElement );
    }

    #endregion //Constructors

    #region Bass Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _resizeThumb != null )
        _resizeThumb.DragDelta -= ResizeThumb_DragDelta;

      _resizeThumb = GetTemplateChild( PART_ResizeThumb ) as Thumb;

      if( _resizeThumb != null )
        _resizeThumb.DragDelta += ResizeThumb_DragDelta;
    }

    #endregion //Bass Class Overrides

    #region Event Handlers

    private void OnKeyDown( object sender, KeyEventArgs e )
    {
      if( !IsOpen )
      {
        if( KeyboardUtilities.IsKeyModifyingPopupState( e ) )
        {
          IsOpen = true;
          e.Handled = true;
        }
      }
      else
      {
        if( KeyboardUtilities.IsKeyModifyingPopupState( e )
          || ( e.Key == Key.Escape )
          || ( e.Key == Key.Tab ) )
        {
          CloseEditor();
          e.Handled = true;
        }
      }
    }

    private void OnMouseDownOutsideCapturedElement( object sender, MouseButtonEventArgs e )
    {
      CloseEditor();
    }

    void ResizeThumb_DragDelta( object sender, DragDeltaEventArgs e )
    {
      double yadjust = DropDownHeight + e.VerticalChange;
      double xadjust = DropDownWidth + e.HorizontalChange;

      if( ( xadjust >= 0 ) && ( yadjust >= 0 ) )
      {
        DropDownWidth = xadjust;
        DropDownHeight = yadjust;
      }
    }

    #endregion //Event Handlers

    #region Methods

    private void CloseEditor()
    {
      if( IsOpen )
        IsOpen = false;
      ReleaseMouseCapture();
    }

    #endregion //Methods
  }
}
