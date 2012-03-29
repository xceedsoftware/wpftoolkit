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

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Core.Utilities;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit
{
  public class ColorCanvas : Control
  {
    #region Private Members

    private TranslateTransform _colorShadeSelectorTransform = new TranslateTransform();
    private Canvas _colorShadingCanvas;
    private Canvas _colorShadeSelector;
    private ColorSpectrumSlider _spectrumSlider;
    private Point? _currentColorPosition;
    private bool _surpressPropertyChanged;

    #endregion //Private Members

    #region Properties

    #region SelectedColor

    public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register( "SelectedColor", typeof( Color ), typeof( ColorCanvas ), new FrameworkPropertyMetadata( Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedColorChanged ) );
    public Color SelectedColor
    {
      get
      {
        return ( Color )GetValue( SelectedColorProperty );
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
        colorCanvas.OnSelectedColorChanged( ( Color )e.OldValue, ( Color )e.NewValue );
    }

    protected virtual void OnSelectedColorChanged( Color oldValue, Color newValue )
    {
      HexadecimalString = newValue.ToString();
      UpdateRGBValues( newValue );
      UpdateColorShadeSelectorPosition( newValue );

      RoutedPropertyChangedEventArgs<Color> args = new RoutedPropertyChangedEventArgs<Color>( oldValue, newValue );
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

    public static readonly DependencyProperty HexadecimalStringProperty = DependencyProperty.Register( "HexadecimalString", typeof( string ), typeof( ColorCanvas ), new UIPropertyMetadata( "#FFFFFFFF", OnHexadecimalStringChanged ) );
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
      if( !SelectedColor.ToString().Equals( newValue ) )
        UpdateSelectedColor( ( Color )ColorConverter.ConvertFromString( newValue ) );
    }

    #endregion //HexadecimalString

    #endregion //Properties

    #region Constructors

    static ColorCanvas()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( ColorCanvas ), new FrameworkPropertyMetadata( typeof( ColorCanvas ) ) );
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

      _colorShadingCanvas = GetTemplateChild( "PART_ColorShadingCanvas" ) as Canvas;

      if( _colorShadingCanvas != null )
      {
        _colorShadingCanvas.MouseLeftButtonDown += ColorShadingCanvas_MouseLeftButtonDown;
        _colorShadingCanvas.MouseLeftButtonUp += ColorShadingCanvas_MouseLeftButtonUp;
        _colorShadingCanvas.MouseMove += ColorShadingCanvas_MouseMove;
        _colorShadingCanvas.SizeChanged += ColorShadingCanvas_SizeChanged;
      }

      _colorShadeSelector = GetTemplateChild( "PART_ColorShadeSelector" ) as Canvas;

      if( _colorShadeSelector != null )
        _colorShadeSelector.RenderTransform = _colorShadeSelectorTransform;

      if( _spectrumSlider != null )
        _spectrumSlider.ValueChanged -= SpectrumSlider_ValueChanged;

      _spectrumSlider = GetTemplateChild( "PART_SpectrumSlider" ) as ColorSpectrumSlider;

      if( _spectrumSlider != null )
        _spectrumSlider.ValueChanged += SpectrumSlider_ValueChanged;

      UpdateRGBValues( SelectedColor );
      UpdateColorShadeSelectorPosition( SelectedColor );
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      //hitting enter on textbox will update value of underlying source
      if( e.Key == Key.Enter && e.OriginalSource is TextBox )
      {
        BindingExpression be = ( ( TextBox )e.OriginalSource ).GetBindingExpression( TextBox.TextProperty );
        be.UpdateSource();
      }
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    void ColorShadingCanvas_MouseLeftButtonDown( object sender, MouseButtonEventArgs e )
    {
      Point p = e.GetPosition( _colorShadingCanvas );
      UpdateColorShadeSelectorPositionAndCalculateColor( p, true );
      _colorShadingCanvas.CaptureMouse();
    }

    void ColorShadingCanvas_MouseLeftButtonUp( object sender, MouseButtonEventArgs e )
    {
      _colorShadingCanvas.ReleaseMouseCapture();
    }

    void ColorShadingCanvas_MouseMove( object sender, MouseEventArgs e )
    {
      if( e.LeftButton == MouseButtonState.Pressed )
      {
        Point p = e.GetPosition( _colorShadingCanvas );
        UpdateColorShadeSelectorPositionAndCalculateColor( p, true );
        Mouse.Synchronize();
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
      if( _currentColorPosition != null )
      {
        CalculateColor( ( Point )_currentColorPosition );
      }
    }

    #endregion //Event Handlers

    #region Events

    public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent( "SelectedColorChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<Color> ), typeof( ColorCanvas ) );
    public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
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

    private void UpdateSelectedColor( Color color )
    {
      SelectedColor = Color.FromArgb( color.A, color.R, color.G, color.B );
    }

    private void UpdateRGBValues( Color color )
    {
      _surpressPropertyChanged = true;

      A = color.A;
      R = color.R;
      G = color.G;
      B = color.B;

      _surpressPropertyChanged = false;
    }

    private void UpdateColorShadeSelectorPositionAndCalculateColor( Point p, bool calculateColor )
    {
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

    private void UpdateColorShadeSelectorPosition( Color color )
    {
      if( _spectrumSlider == null || _colorShadingCanvas == null )
        return;

      _currentColorPosition = null;

      HsvColor hsv = ColorUtilities.ConvertRgbToHsv( color.R, color.G, color.B );

      if( !( color.R == color.G && color.R == color.B ) )
        _spectrumSlider.Value = hsv.H;

      Point p = new Point( hsv.S, 1 - hsv.V );

      _currentColorPosition = p;

      _colorShadeSelectorTransform.X = ( p.X * _colorShadingCanvas.Width ) - 5;
      _colorShadeSelectorTransform.Y = ( p.Y * _colorShadingCanvas.Height ) - 5;
    }

    private void CalculateColor( Point p )
    {
      HsvColor hsv = new HsvColor( 360 - _spectrumSlider.Value, 1, 1 )
      {
        S = p.X,
        V = 1 - p.Y
      };
      var currentColor = ColorUtilities.ConvertHsvToRgb( hsv.H, hsv.S, hsv.V );
      currentColor.A = A;
      SelectedColor = currentColor;
      HexadecimalString = SelectedColor.ToString();
    }

    #endregion //Methods
  }
}
