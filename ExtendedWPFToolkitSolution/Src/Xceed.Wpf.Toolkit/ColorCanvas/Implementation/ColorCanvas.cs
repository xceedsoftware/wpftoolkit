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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.Primitives;
using System.IO;
using System;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_ColorShadingCanvas, Type = typeof( Canvas ) )]
  [TemplatePart( Name = PART_ColorShadeSelector, Type = typeof( Canvas ) )]
  [TemplatePart( Name = PART_SpectrumSlider, Type = typeof( ColorSpectrumSlider ) )]
  [TemplatePart( Name = PART_HexadecimalTextBox, Type = typeof( TextBox ) )]
  public class ColorCanvas : Control
  {
    private const string PART_ColorShadingCanvas = "PART_ColorShadingCanvas";
    private const string PART_ColorShadeSelector = "PART_ColorShadeSelector";
    private const string PART_SpectrumSlider = "PART_SpectrumSlider";
    private const string PART_HexadecimalTextBox = "PART_HexadecimalTextBox";

    #region Private Members

    private TranslateTransform _colorShadeSelectorTransform = new TranslateTransform();
    private Canvas _colorShadingCanvas;
    private Canvas _colorShadeSelector;
    private ColorSpectrumSlider _spectrumSlider;
    private TextBox _hexadecimalTextBox;
    private Point? _currentColorPosition;
    private bool _surpressPropertyChanged;
    private bool _updateSpectrumSliderValue = true;

    #endregion //Private Members

    #region Properties

    #region SelectedColor

    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register( "SelectedColor", typeof( Color? ), typeof( ColorCanvas ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged ) );
    public Color? SelectedColor
    {
      get
      {
        return ( Color? )GetValue( SelectedColorProperty );
      }
      set
      {
        SetValue( SelectedColorProperty, value );
      }
    }

    private static void OnSelectedColorChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColorCanvas colorCanvas = o as ColorCanvas;
      if( colorCanvas != null )
        colorCanvas.OnSelectedColorChanged( ( Color? )e.OldValue, ( Color? )e.NewValue );
    }

    protected virtual void OnSelectedColorChanged( Color? oldValue, Color? newValue )
    {
      SetHexadecimalStringProperty( GetFormatedColorString( newValue ), false );
      UpdateRGBValues( newValue );
      UpdateColorShadeSelectorPosition( newValue );

      RoutedPropertyChangedEventArgs<Color?> args = new RoutedPropertyChangedEventArgs<Color?>( oldValue, newValue );
      args.RoutedEvent = SelectedColorChangedEvent;
      RaiseEvent( args );
    }

    #endregion //SelectedColor

    #region RGB

    #region A

    public static readonly DependencyProperty AProperty = DependencyProperty.Register( "A", typeof( byte ), typeof( ColorCanvas ), new UIPropertyMetadata( ( byte )255, OnAChanged ) );
    public byte A
    {
      get
      {
        return ( byte )GetValue( AProperty );
      }
      set
      {
        SetValue( AProperty, value );
      }
    }

    private static void OnAChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColorCanvas colorCanvas = o as ColorCanvas;
      if( colorCanvas != null )
        colorCanvas.OnAChanged( ( byte )e.OldValue, ( byte )e.NewValue );
    }

    protected virtual void OnAChanged( byte oldValue, byte newValue )
    {
      if( !_surpressPropertyChanged )
        UpdateSelectedColor();
    }

    #endregion //A

    #region R

    public static readonly DependencyProperty RProperty = DependencyProperty.Register( "R", typeof( byte ), typeof( ColorCanvas ), new UIPropertyMetadata( ( byte )0, OnRChanged ) );
    public byte R
    {
      get
      {
        return ( byte )GetValue( RProperty );
      }
      set
      {
        SetValue( RProperty, value );
      }
    }

    private static void OnRChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColorCanvas colorCanvas = o as ColorCanvas;
      if( colorCanvas != null )
        colorCanvas.OnRChanged( ( byte )e.OldValue, ( byte )e.NewValue );
    }

    protected virtual void OnRChanged( byte oldValue, byte newValue )
    {
      if( !_surpressPropertyChanged )
        UpdateSelectedColor();
    }

    #endregion //R

    #region G

    public static readonly DependencyProperty GProperty = DependencyProperty.Register( "G", typeof( byte ), typeof( ColorCanvas ), new UIPropertyMetadata( ( byte )0, OnGChanged ) );
    public byte G
    {
      get
      {
        return ( byte )GetValue( GProperty );
      }
      set
      {
        SetValue( GProperty, value );
      }
    }

    private static void OnGChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColorCanvas colorCanvas = o as ColorCanvas;
      if( colorCanvas != null )
        colorCanvas.OnGChanged( ( byte )e.OldValue, ( byte )e.NewValue );
    }

    protected virtual void OnGChanged( byte oldValue, byte newValue )
    {
      if( !_surpressPropertyChanged )
        UpdateSelectedColor();
    }

    #endregion //G

    #region B

    public static readonly DependencyProperty BProperty = DependencyProperty.Register( "B", typeof( byte ), typeof( ColorCanvas ), new UIPropertyMetadata( ( byte )0, OnBChanged ) );
    public byte B
    {
      get
      {
        return ( byte )GetValue( BProperty );
      }
      set
      {
        SetValue( BProperty, value );
      }
    }

    private static void OnBChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColorCanvas colorCanvas = o as ColorCanvas;
      if( colorCanvas != null )
        colorCanvas.OnBChanged( ( byte )e.OldValue, ( byte )e.NewValue );
    }

    protected virtual void OnBChanged( byte oldValue, byte newValue )
    {
      if( !_surpressPropertyChanged )
        UpdateSelectedColor();
    }

    #endregion //B

    #endregion //RGB

    #region HexadecimalString

    public static readonly DependencyProperty HexadecimalStringProperty = DependencyProperty.Register( "HexadecimalString", typeof( string ), typeof( ColorCanvas ), new UIPropertyMetadata( "", OnHexadecimalStringChanged, OnCoerceHexadecimalString ) );
    public string HexadecimalString
    {
      get
      {
        return ( string )GetValue( HexadecimalStringProperty );
      }
      set
      {
        SetValue( HexadecimalStringProperty, value );
      }
    }

    private static void OnHexadecimalStringChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColorCanvas colorCanvas = o as ColorCanvas;
      if( colorCanvas != null )
        colorCanvas.OnHexadecimalStringChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnHexadecimalStringChanged( string oldValue, string newValue )
    {
      string newColorString = GetFormatedColorString( newValue );
      string currentColorString = GetFormatedColorString( SelectedColor );
      if( !currentColorString.Equals( newColorString ) )
      {
        Color? col = null;
        if( !string.IsNullOrEmpty( newColorString ) )
        {
          col = ( Color )ColorConverter.ConvertFromString( newColorString );      
        }
        UpdateSelectedColor( col );
      }

      SetHexadecimalTextBoxTextProperty( newValue );
    }

    private static object OnCoerceHexadecimalString( DependencyObject d, object basevalue )
    {
      var colorCanvas = ( ColorCanvas )d;
      if( colorCanvas == null )
        return basevalue;

      return colorCanvas.OnCoerceHexadecimalString( basevalue );
    }

    private object OnCoerceHexadecimalString( object newValue )
    {
      var value = newValue as string;
      string retValue = value;

      try
      {
        if( !string.IsNullOrEmpty( retValue ) )
        {
          int outValue;
          // User has entered an hexadecimal value (without the "#" character)... add it.
          if( Int32.TryParse( retValue, System.Globalization.NumberStyles.HexNumber, null, out outValue ) )
          {
            retValue = "#" + retValue;
          }
          ColorConverter.ConvertFromString( retValue );
        }
      }
      catch
      {
        //When HexadecimalString is changed via Code-Behind and hexadecimal format is bad, throw.
        throw new InvalidDataException( "Color provided is not in the correct format." );
      }

      return retValue;
    }

    #endregion //HexadecimalString

    #region UsingAlphaChannel

    public static readonly DependencyProperty UsingAlphaChannelProperty = DependencyProperty.Register( "UsingAlphaChannel", typeof( bool ), typeof( ColorCanvas ), new FrameworkPropertyMetadata( true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback( OnUsingAlphaChannelPropertyChanged ) ) );
    public bool UsingAlphaChannel
    {
      get
      {
        return ( bool )GetValue( UsingAlphaChannelProperty );
      }
      set
      {
        SetValue( UsingAlphaChannelProperty, value );
      }
    }

    private static void OnUsingAlphaChannelPropertyChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      ColorCanvas colorCanvas = o as ColorCanvas;
      if( colorCanvas != null )
        colorCanvas.OnUsingAlphaChannelChanged();
    }

    protected virtual void OnUsingAlphaChannelChanged()
    {
      SetHexadecimalStringProperty( GetFormatedColorString( SelectedColor ), false );
    }

    #endregion //UsingAlphaChannel

    #endregion //Properties

    #region Constructors

    static ColorCanvas()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( ColorCanvas ), new FrameworkPropertyMetadata( typeof( ColorCanvas ) ) );
    }

    public ColorCanvas()
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _colorShadingCanvas != null )
      {
        _colorShadingCanvas.MouseLeftButtonDown -= ColorShadingCanvas_MouseLeftButtonDown;
        _colorShadingCanvas.MouseLeftButtonUp -= ColorShadingCanvas_MouseLeftButtonUp;
        _colorShadingCanvas.MouseMove -= ColorShadingCanvas_MouseMove;
        _colorShadingCanvas.SizeChanged -= ColorShadingCanvas_SizeChanged;
      }

      _colorShadingCanvas = GetTemplateChild( PART_ColorShadingCanvas ) as Canvas;

      if( _colorShadingCanvas != null )
      {
        _colorShadingCanvas.MouseLeftButtonDown += ColorShadingCanvas_MouseLeftButtonDown;
        _colorShadingCanvas.MouseLeftButtonUp += ColorShadingCanvas_MouseLeftButtonUp;
        _colorShadingCanvas.MouseMove += ColorShadingCanvas_MouseMove;
        _colorShadingCanvas.SizeChanged += ColorShadingCanvas_SizeChanged;
      }

      _colorShadeSelector = GetTemplateChild( PART_ColorShadeSelector ) as Canvas;

      if( _colorShadeSelector != null )
        _colorShadeSelector.RenderTransform = _colorShadeSelectorTransform;

      if( _spectrumSlider != null )
        _spectrumSlider.ValueChanged -= SpectrumSlider_ValueChanged;

      _spectrumSlider = GetTemplateChild( PART_SpectrumSlider ) as ColorSpectrumSlider;

      if( _spectrumSlider != null )
        _spectrumSlider.ValueChanged += SpectrumSlider_ValueChanged;

      if( _hexadecimalTextBox != null )
        _hexadecimalTextBox.LostFocus -= new RoutedEventHandler( HexadecimalTextBox_LostFocus );

      _hexadecimalTextBox = GetTemplateChild( PART_HexadecimalTextBox ) as TextBox;

      if( _hexadecimalTextBox != null )
        _hexadecimalTextBox.LostFocus += new RoutedEventHandler( HexadecimalTextBox_LostFocus );

      UpdateRGBValues( SelectedColor );
      UpdateColorShadeSelectorPosition( SelectedColor );

      // When changing theme, HexadecimalString needs to be set since it is not binded.
      SetHexadecimalTextBoxTextProperty( GetFormatedColorString( SelectedColor ) );
    }

    protected override void OnKeyDown( KeyEventArgs e )
    {
      base.OnKeyDown( e );

      //hitting enter on textbox will update Hexadecimal string
      if( e.Key == Key.Enter && e.OriginalSource is TextBox )
      {
        TextBox textBox = ( TextBox )e.OriginalSource;
        if( textBox.Name == PART_HexadecimalTextBox )
          SetHexadecimalStringProperty( textBox.Text, true );
      }
    }




    #endregion //Base Class Overrides

    #region Event Handlers

    void ColorShadingCanvas_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
    {
      if( _colorShadingCanvas != null )
      {
        Point p = e.GetPosition( _colorShadingCanvas );
        UpdateColorShadeSelectorPositionAndCalculateColor( p, true );
        _colorShadingCanvas.CaptureMouse();
        //Prevent from closing ColorCanvas after mouseDown in ListView
        e.Handled = true;
      }
    }

    void ColorShadingCanvas_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
    {
      if( _colorShadingCanvas != null )
      {
        _colorShadingCanvas.ReleaseMouseCapture();
      }
    }

    void ColorShadingCanvas_MouseMove( object sender, MouseEventArgs e )
    {
      if( _colorShadingCanvas != null )
      {
        if( e.LeftButton == MouseButtonState.Pressed )
        {
          Point p = e.GetPosition( _colorShadingCanvas );
          UpdateColorShadeSelectorPositionAndCalculateColor( p, true );
          Mouse.Synchronize();
        }
      }
    }

    void ColorShadingCanvas_SizeChanged( object sender, SizeChangedEventArgs e )
    {
      if( _currentColorPosition != null )
      {
        Point _newPoint = new Point
        {
          X = ( ( Point )_currentColorPosition ).X * e.NewSize.Width,
          Y = ( ( Point )_currentColorPosition ).Y * e.NewSize.Height
        };

        UpdateColorShadeSelectorPositionAndCalculateColor( _newPoint, false );
      }
    }

    void SpectrumSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
    {
      if( (_currentColorPosition != null) && (this.SelectedColor != null) )
      {
        CalculateColor( ( Point )_currentColorPosition );
      }
    }

    void HexadecimalTextBox_LostFocus( object sender, RoutedEventArgs e )
    {
      TextBox textbox = sender as TextBox;
      SetHexadecimalStringProperty( textbox.Text, true );
    }

    #endregion //Event Handlers

    #region Events

    public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent( "SelectedColorChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<Color?> ), typeof( ColorCanvas ) );
    public event RoutedPropertyChangedEventHandler<Color?> SelectedColorChanged
    {
      add
      {
        AddHandler( SelectedColorChangedEvent, value );
      }
      remove
      {
        RemoveHandler( SelectedColorChangedEvent, value );
      }
    }

    #endregion //Events

    #region Methods

    private void UpdateSelectedColor()
    {
      SelectedColor = Color.FromArgb( A, R, G, B );
    }

    private void UpdateSelectedColor( Color? color )
    {
      SelectedColor = ( ( color != null ) && color.HasValue ) 
                      ? (Color?)Color.FromArgb( color.Value.A, color.Value.R, color.Value.G, color.Value.B )
                      : null;
    }

    private void UpdateRGBValues( Color? color )
    {
      if( ( color == null ) || !color.HasValue )
        return;

      _surpressPropertyChanged = true;

      A = color.Value.A;
      R = color.Value.R;
      G = color.Value.G;
      B = color.Value.B;

      _surpressPropertyChanged = false;
    }

    private void UpdateColorShadeSelectorPositionAndCalculateColor( Point p, bool calculateColor )
    {
      if( (_colorShadingCanvas == null) || ( _colorShadeSelector == null) )
        return;

        if( p.Y < 0 )
        p.Y = 0;

      if( p.X < 0 )
        p.X = 0;

      if( p.X > _colorShadingCanvas.ActualWidth )
        p.X = _colorShadingCanvas.ActualWidth;

      if( p.Y > _colorShadingCanvas.ActualHeight )
        p.Y = _colorShadingCanvas.ActualHeight;

      _colorShadeSelectorTransform.X = p.X - ( _colorShadeSelector.Width / 2 );
      _colorShadeSelectorTransform.Y = p.Y - ( _colorShadeSelector.Height / 2 );

      p.X = p.X / _colorShadingCanvas.ActualWidth;
      p.Y = p.Y / _colorShadingCanvas.ActualHeight;

      _currentColorPosition = p;

      if( calculateColor )
        CalculateColor( p );
    }

    private void UpdateColorShadeSelectorPosition( Color? color )
    {
      if( (_spectrumSlider == null) || (_colorShadingCanvas == null) || (color == null) || !color.HasValue)
        return;

      _currentColorPosition = null;

      var hsv = ColorUtilities.ConvertRgbToHsv( color.Value.R, color.Value.G, color.Value.B );

      if( _updateSpectrumSliderValue )
      {
        _spectrumSlider.Value = 360 - hsv.H;
      }

      Point p = new Point( hsv.S, 1 - hsv.V );

      _currentColorPosition = p;

      _colorShadeSelectorTransform.X = ( p.X * _colorShadingCanvas.Width ) - 5;
      _colorShadeSelectorTransform.Y = ( p.Y * _colorShadingCanvas.Height ) - 5;
    }

    private void CalculateColor( Point p )
    {
      if( _spectrumSlider == null )
        return;

      HsvColor hsv = new HsvColor( 360 - _spectrumSlider.Value, 1, 1 )
      {
        S = p.X,
        V = 1 - p.Y
      };
      var currentColor = ColorUtilities.ConvertHsvToRgb( hsv.H, hsv.S, hsv.V );
      currentColor.A = A;
      _updateSpectrumSliderValue = false;
      SelectedColor = currentColor;
      _updateSpectrumSliderValue = true;
      SetHexadecimalStringProperty( GetFormatedColorString( SelectedColor ), false );
    }

    private string GetFormatedColorString( Color? colorToFormat )
    {
      if( ( colorToFormat == null ) || !colorToFormat.HasValue )
        return string.Empty;
      return ColorUtilities.FormatColorString( colorToFormat.ToString(), UsingAlphaChannel );
    }

    private string GetFormatedColorString( string stringToFormat )
    {
      return ColorUtilities.FormatColorString( stringToFormat, UsingAlphaChannel );
    }

    private void SetHexadecimalStringProperty( string newValue, bool modifyFromUI )
    {
      if( modifyFromUI )
      {
        try
        {
          if( !string.IsNullOrEmpty( newValue ) )
          {
            int outValue;
            // User has entered an hexadecimal value (without the "#" character)... add it.
            if( Int32.TryParse( newValue, System.Globalization.NumberStyles.HexNumber, null, out outValue ) )
            {
              newValue = "#" + newValue;
            }
            ColorConverter.ConvertFromString( newValue );
          }
          HexadecimalString = newValue;
        }
        catch
        {
          //When HexadecimalString is changed via UI and hexadecimal format is bad, keep the previous HexadecimalString.
          SetHexadecimalTextBoxTextProperty( HexadecimalString );
        }
      }
      else
      {
        //When HexadecimalString is changed via Code-Behind, hexadecimal format will be evaluated in OnCoerceHexadecimalString()
        HexadecimalString = newValue;
      }
    }

    private void SetHexadecimalTextBoxTextProperty( string newValue )
    {
      if( _hexadecimalTextBox != null )
        _hexadecimalTextBox.Text = newValue;
    }

    #endregion //Methods
  }
}
