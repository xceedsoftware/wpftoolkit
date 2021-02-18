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
using System.Windows.Controls;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_LowerRange, Type = typeof( RepeatButton ) )]
  [TemplatePart( Name = PART_HigherRange, Type = typeof( RepeatButton ) )]
  [TemplatePart( Name = PART_HigherSlider, Type = typeof( Slider ) )]
  [TemplatePart( Name = PART_LowerSlider, Type = typeof( Slider ) )]
  [TemplatePart( Name = PART_Track, Type = typeof( Track ) )]

  public class RangeSlider : Control
  {
    #region Members

    private const String PART_LowerRange = "PART_LowerRange";
    private const String PART_Range = "PART_Range";
    private const String PART_HigherRange = "PART_HigherRange";
    private const String PART_HigherSlider = "PART_HigherSlider";
    private const String PART_LowerSlider = "PART_LowerSlider";
    private const String PART_Track = "PART_Track";

    private RepeatButton _lowerRange;
    private RepeatButton _higherRange;
    private Slider _lowerSlider;
    private Slider _higherSlider;
    private Track _lowerTrack;
    private Track _higherTrack;
    private double? _deferredUpdateValue;

    #endregion Members

    #region Constructors

    static RangeSlider()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( RangeSlider ), new FrameworkPropertyMetadata( typeof( RangeSlider ) ) );
    }

    public RangeSlider()
    {
      this.SizeChanged += this.RangeSlider_SizeChanged;
    }

    #endregion Constructors

    #region Properties

    #region AutoToolTipPlacement

    public static readonly DependencyProperty AutoToolTipPlacementProperty = DependencyProperty.Register( "AutoToolTipPlacement", typeof( AutoToolTipPlacement ), typeof( RangeSlider ),
        new FrameworkPropertyMetadata( AutoToolTipPlacement.None, RangeSlider.OnAutoToolTipPlacementChanged ) );

    public AutoToolTipPlacement AutoToolTipPlacement
    {
      get
      {
        return (AutoToolTipPlacement)GetValue( RangeSlider.AutoToolTipPlacementProperty );
      }
      set
      {
        SetValue( RangeSlider.AutoToolTipPlacementProperty, value );
      }
    }

    private static void OnAutoToolTipPlacementChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var rangeSlider = sender as RangeSlider;
      if( rangeSlider != null )
      {
        rangeSlider.OnAutoToolTipPlacementChanged( (AutoToolTipPlacement)e.OldValue, (AutoToolTipPlacement)e.NewValue );
      }
    }

    protected virtual void OnAutoToolTipPlacementChanged( AutoToolTipPlacement oldValue, AutoToolTipPlacement newValue )
    {
    }

    #endregion //AutoToolTipPlacement

    #region AutoToolTipPrecision

    public static readonly DependencyProperty AutoToolTipPrecisionProperty = DependencyProperty.Register( "AutoToolTipPrecision", typeof( int ), typeof( RangeSlider ),
        new FrameworkPropertyMetadata( 0 ) );

    public int AutoToolTipPrecision
    {
      get
      {
        return (int)GetValue( RangeSlider.AutoToolTipPrecisionProperty );
      }
      set
      {
        SetValue( RangeSlider.AutoToolTipPrecisionProperty, value );
      }
    }

    #endregion //AutoToolTipPrecision

    #region HigherRangeBackground
    /// <summary>
    /// Get/Set the Brush for the Range between Higher and Maximum values. (Brush)
    /// </summary>

    public static readonly DependencyProperty HigherRangeBackgroundProperty = DependencyProperty.Register( "HigherRangeBackground", typeof( Brush ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( Brushes.Transparent ) );

    public Brush HigherRangeBackground
    {
      get
      {
        return (Brush)GetValue( RangeSlider.HigherRangeBackgroundProperty );
      }
      set
      {
        SetValue( RangeSlider.HigherRangeBackgroundProperty, value );
      }
    }

    #endregion HigherRangeBackground

    #region HigherRangeStyle
    /// <summary>
    /// Get/Set the Style for the Range between Higher and Maximum values. (Style)
    /// </summary>

    public static readonly DependencyProperty HigherRangeStyleProperty = DependencyProperty.Register( "HigherRangeStyle", typeof( Style ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( null ) );

    public Style HigherRangeStyle
    {
      get
      {
        return (Style)this.GetValue( RangeSlider.HigherRangeStyleProperty );
      }
      set
      {
        this.SetValue( RangeSlider.HigherRangeStyleProperty, value );
      }
    }

    #endregion HigherRangeStyle

    #region HigherRangeWidth
    /// <summary>
    /// HigherRangeWidth property is a readonly property, used to calculate the percentage of the HigherRange within the entire min/max range.
    /// </summary>
    /// 

    private static readonly DependencyPropertyKey HigherRangeWidthPropertyKey = DependencyProperty.RegisterAttachedReadOnly( "HigherRangeWidth", typeof( double )
        , typeof( RangeSlider ), new PropertyMetadata( 0d ) );

    public static readonly DependencyProperty HigherRangeWidthProperty = HigherRangeWidthPropertyKey.DependencyProperty;

    public double HigherRangeWidth
    {
      get
      {
        return (double)GetValue( RangeSlider.HigherRangeWidthProperty );
      }
      private set
      {
        SetValue( RangeSlider.HigherRangeWidthPropertyKey, value );
      }
    }


    #endregion HigherRangeWidth

    #region HigherThumbBackground
    /// <summary>
    /// Get/Set the Brush for the HigherValue thumb's background [active state]. (Brush)
    /// </summary>

    public static readonly DependencyProperty HigherThumbBackgroundProperty = DependencyProperty.Register( "HigherThumbBackground", typeof( Brush ), typeof( RangeSlider ) );

    public Brush HigherThumbBackground
    {
      get
      {
        return (Brush)GetValue( RangeSlider.HigherThumbBackgroundProperty );
      }
      set
      {
        SetValue( RangeSlider.HigherThumbBackgroundProperty, value );
      }
    }

    #endregion HigherThumbBackground

    #region HigherValue
    /// <summary>
    /// HigherValue property represents the higher value within the selected range.
    /// </summary>
    public static readonly DependencyProperty HigherValueProperty = DependencyProperty.Register( "HigherValue", typeof( double ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( 0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RangeSlider.OnHigherValueChanged, RangeSlider.OnCoerceHigherValueChanged ) );

    public double HigherValue
    {
      get
      {
        return (double)GetValue( RangeSlider.HigherValueProperty );
      }
      set
      {
        SetValue( RangeSlider.HigherValueProperty, value );
      }
    }

    private static object OnCoerceHigherValueChanged( DependencyObject d, object basevalue )
    {
      var rangeSlider = (RangeSlider)d;
      if( (rangeSlider == null) || !rangeSlider.IsLoaded)
        return basevalue;

      var min = Math.Min( rangeSlider.Minimum, rangeSlider.Maximum );
      var max = Math.Max( rangeSlider.Minimum, rangeSlider.Maximum );
      var higherValue = Math.Max( rangeSlider.Minimum, Math.Min( rangeSlider.Maximum, (double)basevalue ) );
      higherValue = Math.Max( rangeSlider.LowerValue, ( double)basevalue );

      return higherValue;
    }

    private static void OnHigherValueChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      RangeSlider rangeSlider = sender as RangeSlider;
      if( rangeSlider != null )
      {
        rangeSlider.OnHigherValueChanged( (double)args.OldValue, (double)args.NewValue );
      }
    }

    protected virtual void OnHigherValueChanged( double oldValue, double newValue )
    {
      this.AdjustView();

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = RangeSlider.HigherValueChangedEvent;
      this.RaiseEvent( args );
    }

    #endregion HigherValue

    #region IsDeferredUpdateValues
    /// <summary>
    /// Gets/Sets if the LowerValue and HigherValue should be updated only on mouse up.
    /// </summary>
    public static readonly DependencyProperty IsDeferredUpdateValuesProperty = DependencyProperty.Register( "IsDeferredUpdateValues", typeof( bool ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( false ) );

    public bool IsDeferredUpdateValues
    {
      get
      {
        return (bool)GetValue( RangeSlider.IsDeferredUpdateValuesProperty );
      }
      set
      {
        SetValue( RangeSlider.IsDeferredUpdateValuesProperty, value );
      }
    }

    #endregion IsDeferredUpdateValues

    #region IsSnapToTickEnabled

    public static readonly DependencyProperty IsSnapToTickEnabledProperty = DependencyProperty.Register( "IsSnapToTickEnabled", typeof( bool ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( false ) );

    public bool IsSnapToTickEnabled
    {
      get
      {
        return (bool)GetValue( RangeSlider.IsSnapToTickEnabledProperty );
      }
      set
      {
        SetValue( RangeSlider.IsSnapToTickEnabledProperty, value );
      }
    }

    #endregion IsSnapToTickEnabled

    #region LowerRangeBackground
    /// <summary>
    /// Get/Set the Brush for the Range between Minimum and Lower values. (Brush)
    /// </summary>

    public static readonly DependencyProperty LowerRangeBackgroundProperty = DependencyProperty.Register( "LowerRangeBackground", typeof( Brush ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( Brushes.Transparent ) );

    public Brush LowerRangeBackground
    {
      get
      {
        return (Brush)GetValue( RangeSlider.LowerRangeBackgroundProperty );
      }
      set
      {
        SetValue( RangeSlider.LowerRangeBackgroundProperty, value );
      }
    }

    #endregion LowerRangeBackground

    #region LowerRangeStyle
    /// <summary>
    /// Get/Set the Style for the Range between Minimum and Lower values. (Style)
    /// </summary>

    public static readonly DependencyProperty LowerRangeStyleProperty = DependencyProperty.Register( "LowerRangeStyle", typeof( Style ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( null ) );

    public Style LowerRangeStyle
    {
      get
      {
        return (Style)this.GetValue( RangeSlider.LowerRangeStyleProperty );
      }
      set
      {
        this.SetValue( RangeSlider.LowerRangeStyleProperty, value );
      }
    }

    #endregion LowerRangeStyle

    #region LowerRangeWidth
    /// <summary>
    /// LowerRangeWidth property is a readonly property, used to calculate the percentage of the LowerRange, within the entire min/max range.
    /// </summary>
    /// 

    private static DependencyPropertyKey LowerRangeWidthPropertyKey = DependencyProperty.RegisterAttachedReadOnly( "LowerRangeWidth", typeof( double )
      , typeof( RangeSlider ), new PropertyMetadata( 0d ) );

    public static readonly DependencyProperty LowerRangeWidthProperty = LowerRangeWidthPropertyKey.DependencyProperty;

    public double LowerRangeWidth
    {
      get
      {
        return (double)GetValue( RangeSlider.LowerRangeWidthProperty );
      }
      private set
      {
        SetValue( RangeSlider.LowerRangeWidthPropertyKey, value );
      }
    }

    #endregion LowerRangeWidth

    #region LowerThumbBackground
    /// <summary>
    /// Get/Set the Brush for the LowerValue thumb's background [active state]. (Brush)
    /// </summary>

    public static readonly DependencyProperty LowerThumbBackgroundProperty = DependencyProperty.Register( "LowerThumbBackground", typeof( Brush ), typeof( RangeSlider ) );

    public Brush LowerThumbBackground
    {
      get
      {
        return (Brush)GetValue( RangeSlider.LowerThumbBackgroundProperty );
      }
      set
      {
        SetValue( RangeSlider.LowerThumbBackgroundProperty, value );
      }
    }

    #endregion LowerThumbBackground

    #region LowerValue
    /// <summary>
    /// LowerValue property represents the lower value within the selected range.
    /// </summary>
    public static readonly DependencyProperty LowerValueProperty = DependencyProperty.Register( "LowerValue", typeof( double ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( 0d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RangeSlider.OnLowerValueChanged, RangeSlider.OnCoerceLowerValueChanged ) );

    public double LowerValue
    {
      get
      {
        return (double)GetValue( RangeSlider.LowerValueProperty );
      }
      set
      {
        SetValue( RangeSlider.LowerValueProperty, value );
      }
    }

    private static object OnCoerceLowerValueChanged( DependencyObject d, object basevalue )
    {
      var rangeSlider = (RangeSlider)d;
      if( (rangeSlider == null) || !rangeSlider.IsLoaded )
        return basevalue;

      var min = Math.Min( rangeSlider.Minimum, rangeSlider.Maximum );
      var max = Math.Max( rangeSlider.Minimum, rangeSlider.Maximum );
      var lowerValue = Math.Max( rangeSlider.Minimum, Math.Min( rangeSlider.Maximum, (double)basevalue ) );
      lowerValue = Math.Min( (double)basevalue, rangeSlider.HigherValue );

      return lowerValue;
    }

    private static void OnLowerValueChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      RangeSlider rangeSlider = sender as RangeSlider;
      if( rangeSlider != null )
      {
        rangeSlider.OnLowerValueChanged( (double)args.OldValue, (double)args.NewValue );
      }
    }

    protected virtual void OnLowerValueChanged( double oldValue, double newValue )
    {
      this.AdjustView();

      RoutedEventArgs args = new RoutedEventArgs();
      args.RoutedEvent = RangeSlider.LowerValueChangedEvent;
      this.RaiseEvent( args );
    }

    #endregion LowerValue

    #region Maximum
    /// <summary>
    /// Maximum property represents the maximum value, which can be selected, in a range.
    /// </summary>
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register( "Maximum", typeof( double ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( RangeSlider.OnMaximumChanged ) );

    public double Maximum
    {
      get
      {
        return (double)GetValue( RangeSlider.MaximumProperty );
      }
      set
      {
        SetValue( RangeSlider.MaximumProperty, value );
      }
    }

    private static void OnMaximumChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      RangeSlider rangeSlider = sender as RangeSlider;
      if( rangeSlider != null )
      {
        rangeSlider.OnMaximumChanged( (double)args.OldValue, (double)args.NewValue );
      }
    }

    protected virtual void OnMaximumChanged( double oldValue, double newValue )
    {
      this.AdjustView();
    }

    #endregion Maximum

    #region Minimum
    /// <summary>
    /// Minimum property represents the minimum value, which can be selected, in a range.
    /// </summary>
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register( "Minimum", typeof( double ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( RangeSlider.OnMinimumChanged ) );

    public double Minimum
    {
      get
      {
        return (double)GetValue( RangeSlider.MinimumProperty );
      }
      set
      {
        SetValue( RangeSlider.MinimumProperty, value );
      }
    }

    private static void OnMinimumChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      RangeSlider rangeSlider = sender as RangeSlider;
      if( rangeSlider != null )
      {
        rangeSlider.OnMinimumChanged( (double)args.OldValue, (double)args.NewValue );
      }
    }

    protected virtual void OnMinimumChanged( double oldValue, double newValue )
    {
      // adjust the range width
      this.AdjustView();
    }

    #endregion Minimum

    #region Orientation

    /// <summary>
    /// Get/Set the RangeSlider's orientation.
    /// </summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register( "Orientation", typeof( Orientation ), typeof( RangeSlider ),
        new FrameworkPropertyMetadata( Orientation.Horizontal, RangeSlider.OnOrientationChanged ) );

    public Orientation Orientation
    {
      get
      {
        return (Orientation)GetValue( RangeSlider.OrientationProperty );
      }
      set
      {
        SetValue( RangeSlider.OrientationProperty, value );
      }
    }

    private static void OnOrientationChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      RangeSlider rangeSlider = sender as RangeSlider;
      if( rangeSlider != null )
      {
        rangeSlider.OnOrientationChanged( (Orientation)e.OldValue, (Orientation)e.NewValue );
      }
    }

    protected virtual void OnOrientationChanged( Orientation oldValue, Orientation newValue )
    {
    }

    #endregion //Orientation

    #region RangeBackground
    /// <summary>
    /// Get/Set the Brush for the Range between Lower and Higher values [active state]. (Brush)
    /// </summary>

    public static readonly DependencyProperty RangeBackgroundProperty = DependencyProperty.Register( "RangeBackground", typeof( Brush ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( Brushes.Transparent ) );

    public Brush RangeBackground
    {
      get
      {
        return (Brush)GetValue( RangeSlider.RangeBackgroundProperty );
      }
      set
      {
        SetValue( RangeSlider.RangeBackgroundProperty, value );
      }
    }

    #endregion RangeBackground

    #region RangeStyle
    /// <summary>
    /// Get/Set the Style for the Range between Lower and Higher values. (Style)
    /// </summary>

    public static readonly DependencyProperty RangeStyleProperty = DependencyProperty.Register( "RangeStyle", typeof( Style ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( null ) );

    public Style RangeStyle
    {
      get
      {
        return (Style)this.GetValue( RangeSlider.RangeStyleProperty );
      }
      set
      {
        this.SetValue( RangeSlider.RangeStyleProperty, value );
      }
    }

    #endregion RangeStyle

    #region RangeWidth
    /// <summary>
    /// RangeWidth property is a readonly property, used to calculate the percentage of the range within the entire min/max range.
    /// </summary>

    private static readonly DependencyPropertyKey RangeWidthPropertyKey = DependencyProperty.RegisterAttachedReadOnly( "RangeWidth", typeof( double )
      , typeof( RangeSlider ), new PropertyMetadata( 0d ) );

    public static readonly DependencyProperty RangeWidthProperty = RangeWidthPropertyKey.DependencyProperty;

    public double RangeWidth
    {
      get
      {
        return (double)GetValue( RangeSlider.RangeWidthProperty );
      }
      private set
      {
        SetValue( RangeSlider.RangeWidthPropertyKey, value );
      }
    }

    #endregion RangeWidth

    #region Step
    /// <summary>
    /// Step property is used to identify the RangeSlider's size of individual move, when clicking on the LowerRange, HigherRange, not while scrolling the thumbs.
    /// </summary>
    private static readonly DependencyProperty StepProperty = DependencyProperty.Register( "Step", typeof( double ), typeof( RangeSlider )
      , new PropertyMetadata( 1.0, null, RangeSlider.CoerceStep ) );

    public double Step
    {
      get
      {
        return (double)GetValue( RangeSlider.StepProperty );
      }
      set
      {
        SetValue( RangeSlider.StepProperty, value );
      }
    }

    private static object CoerceStep( DependencyObject sender, object value )
    {
      RangeSlider rangeSlider = sender as RangeSlider;
      double newValue = (double)value;

      return Math.Max( 0.01, newValue );
    }

    #endregion

    #region TickFrequency
    /// <summary>       
    /// Gets or sets the interval between tick marks.
    /// </summary>
    public static readonly DependencyProperty TickFrequencyProperty = DependencyProperty.Register( "TickFrequency", typeof( double ), typeof( RangeSlider )
      , new FrameworkPropertyMetadata( 1d, RangeSlider.OnTickFrequencyChanged ) );

    public double TickFrequency
    {
      get
      {
        return (double)GetValue( RangeSlider.TickFrequencyProperty );
      }
      set
      {
        SetValue( RangeSlider.TickFrequencyProperty, value );
      }
    }

    private static void OnTickFrequencyChanged( DependencyObject sender, DependencyPropertyChangedEventArgs args )
    {
      var rangeSlider = sender as RangeSlider;
      if( rangeSlider != null )
      {
        rangeSlider.OnTickFrequencyChanged( (double)args.OldValue, (double)args.NewValue );
      }
    }

    protected virtual void OnTickFrequencyChanged( double oldValue, double newValue )
    {
    }

    #endregion TickFrequency

    #region TickPlacement

    public static readonly DependencyProperty TickPlacementProperty = DependencyProperty.Register( "TickPlacement", typeof( TickPlacement ), typeof( RangeSlider ),
        new FrameworkPropertyMetadata( TickPlacement.None, RangeSlider.OnTickPlacementChanged ) );

    public TickPlacement TickPlacement
    {
      get
      {
        return (TickPlacement)GetValue( RangeSlider.TickPlacementProperty );
      }
      set
      {
        SetValue( RangeSlider.TickPlacementProperty, value );
      }
    }

    private static void OnTickPlacementChanged( DependencyObject sender, DependencyPropertyChangedEventArgs e )
    {
      var rangeSlider = sender as RangeSlider;
      if( rangeSlider != null )
      {
        rangeSlider.OnTickPlacementChanged( (TickPlacement)e.OldValue, (TickPlacement)e.NewValue );
      }
    }

    protected virtual void OnTickPlacementChanged( TickPlacement oldValue, TickPlacement newValue )
    {
    }

    #endregion //TickPlacement

    #endregion Properties

    #region Override

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _lowerRange != null )
      {
        _lowerRange.Click -= new RoutedEventHandler( this.LowerRange_Click );
      }
      _lowerRange = this.Template.FindName( PART_LowerRange, this ) as RepeatButton;
      if( _lowerRange != null )
      {
        _lowerRange.Click += new RoutedEventHandler( this.LowerRange_Click );
      }

      if( _higherRange != null )
      {
        _higherRange.Click -= new RoutedEventHandler( this.HigherRange_Click );
      }
      _higherRange = this.Template.FindName( PART_HigherRange, this ) as RepeatButton;
      if( _higherRange != null )
      {
        _higherRange.Click += new RoutedEventHandler( this.HigherRange_Click );
      }

      if( _lowerSlider != null )
      {
        _lowerSlider.Loaded -= this.Slider_Loaded;
        _lowerSlider.ValueChanged -= LowerSlider_ValueChanged;
        if( _lowerTrack != null )
        {
          _lowerTrack.Thumb.DragCompleted -= this.LowerSlider_DragCompleted;
        }
      }
      _lowerSlider = this.Template.FindName( PART_LowerSlider, this ) as Slider;
      if( _lowerSlider != null )
      {
        _lowerSlider.Loaded += this.Slider_Loaded;
        _lowerSlider.ValueChanged += LowerSlider_ValueChanged;
        _lowerSlider.ApplyTemplate();
        _lowerTrack = _lowerSlider.Template.FindName( PART_Track, _lowerSlider ) as Track;
        if( _lowerTrack != null )
        {
          _lowerTrack.Thumb.DragCompleted += this.LowerSlider_DragCompleted;
        }
      }

      if( _higherSlider != null )
      {
        _higherSlider.Loaded -= this.Slider_Loaded;
        _higherSlider.ValueChanged -= HigherSlider_ValueChanged;
        if( _higherTrack != null )
        {
          _higherTrack.Thumb.DragCompleted -= this.HigherSlider_DragCompleted;
        }
      }
      _higherSlider = this.Template.FindName( PART_HigherSlider, this ) as Slider;
      if( _higherSlider != null )
      {
        _higherSlider.Loaded += this.Slider_Loaded;
        _higherSlider.ValueChanged += HigherSlider_ValueChanged;
        _higherSlider.ApplyTemplate();
        _higherTrack = _higherSlider.Template.FindName( PART_Track, _higherSlider ) as Track;
        if( _higherTrack != null )
        {
          _higherTrack.Thumb.DragCompleted += this.HigherSlider_DragCompleted;
        }
      }
    }

    public override string ToString()
    {
      return this.LowerValue.ToString() + "-" + this.HigherValue.ToString();
    }


    #endregion Override

    #region Methods

    internal static double GetThumbWidth( Slider slider )
    {
      if( slider != null )
      {
        var track = (Track)slider.Template.FindName( "PART_Track", slider );
        if( track != null )
        {
          var thumb = track.Thumb;
          return thumb.ActualWidth;
        }
      }
      return 0d;
    }

    internal static double GetThumbHeight( Slider slider )
    {
      if( slider != null )
      {
        var track = (Track)slider.Template.FindName( "PART_Track", slider );
        if( track != null )
        {
          var thumb = track.Thumb;
          return thumb.ActualHeight;
        }
      }
      return 0d;
    }

    private void AdjustView( bool isHigherValueChanged = false )
    {
      //Coerce values to make them consistent.
      var cv = this.GetCoercedValues();

      double actualWidth = 0;
      double lowerSliderThumbWidth = 0d;
      double higherSliderThumbWidth = 0d;

      if( this.Orientation == Orientation.Horizontal )
      {
        actualWidth = this.ActualWidth;
        lowerSliderThumbWidth = RangeSlider.GetThumbWidth( _lowerSlider );
        higherSliderThumbWidth = RangeSlider.GetThumbWidth( _higherSlider );
      }
      else if( this.Orientation == Orientation.Vertical )
      {
        actualWidth = this.ActualHeight;
        lowerSliderThumbWidth = RangeSlider.GetThumbHeight( _lowerSlider );
        higherSliderThumbWidth = RangeSlider.GetThumbHeight( _higherSlider );
      }

      actualWidth -= (lowerSliderThumbWidth + higherSliderThumbWidth);

      if( !this.IsDeferredUpdateValues || ( _deferredUpdateValue == null ) )
      {
        this.SetLowerSliderValues( cv.LowerValue, cv.Minimum, cv.Maximum );
        this.SetHigherSliderValues( cv.HigherValue, cv.Minimum, cv.Maximum );
      }

      double entireRange = cv.Maximum - cv.Minimum;

      if( entireRange > 0 )
      {
        var higherValue = this.IsDeferredUpdateValues && isHigherValueChanged && (_deferredUpdateValue != null ) ? _deferredUpdateValue.Value : cv.HigherValue;
        var lowerValue = this.IsDeferredUpdateValues && !isHigherValueChanged && ( _deferredUpdateValue != null ) ? _deferredUpdateValue.Value : cv.LowerValue;

        this.HigherRangeWidth = (actualWidth * (cv.Maximum - higherValue ) ) / entireRange;

        this.RangeWidth = (actualWidth * ( higherValue - lowerValue ) ) / entireRange;

        this.LowerRangeWidth = (actualWidth * ( lowerValue - cv.Minimum)) / entireRange;
      }
      else
      {
        this.HigherRangeWidth = 0d;
        this.RangeWidth = 0d;
        this.LowerRangeWidth = actualWidth;
      }
    }

    private void SetSlidersMargins()
    {
      if( (_lowerSlider != null) && (_higherSlider != null) )
      {
        if( this.Orientation == Orientation.Horizontal )
        {
          double lowerSliderThumbWidth = RangeSlider.GetThumbWidth( _lowerSlider );
          double higherSliderThumbWidth = RangeSlider.GetThumbWidth( _higherSlider );

          _higherSlider.Margin = new Thickness( lowerSliderThumbWidth, 0d, 0d, 0d );
          _lowerSlider.Margin = new Thickness( 0d, 0d, higherSliderThumbWidth, 0d );
        }
        else
        {
          double lowerSliderThumbHeight = RangeSlider.GetThumbHeight( _lowerSlider );
          double higherSliderThumbHeight = RangeSlider.GetThumbHeight( _higherSlider );

          _higherSlider.Margin = new Thickness( 0d, 0d, 0d, lowerSliderThumbHeight );
          _lowerSlider.Margin = new Thickness( 0d, higherSliderThumbHeight, 0d, 0d );
        }
      }
    }

    private CoercedValues GetCoercedValues()
    {
      CoercedValues cv = new CoercedValues();
      cv.Minimum = Math.Min( this.Minimum, this.Maximum );
      cv.Maximum = Math.Max( cv.Minimum, this.Maximum );
      cv.LowerValue = Math.Max( cv.Minimum, Math.Min( cv.Maximum, this.LowerValue ) );
      cv.HigherValue = Math.Max( cv.Minimum, Math.Min( cv.Maximum, this.HigherValue ) );
      cv.HigherValue = Math.Max( cv.LowerValue, cv.HigherValue );

      return cv;
    }

    private void SetLowerSliderValues( double value, double? minimum, double? maximum )
    {
      this.SetSliderValues( _lowerSlider, this.LowerSlider_ValueChanged, value, minimum, maximum );
    }

    private void SetHigherSliderValues( double value, double? minimum, double? maximum )
    {
      this.SetSliderValues( _higherSlider, this.HigherSlider_ValueChanged, value, minimum, maximum );
    }

    private void SetSliderValues(
      Slider slider,
      RoutedPropertyChangedEventHandler<double> handler,
      double value,
      double? minimum,
      double? maximum )
    {
      if( slider != null )
      {
        slider.ValueChanged -= handler;

        slider.Value = value;
        if( minimum != null )
        {
          slider.Minimum = minimum.Value;
        }
        if( maximum != null )
        {
          slider.Maximum = maximum.Value;
        }

        slider.ValueChanged += handler;
      }
    }

    private void UpdateHigherValue( double? value )
    {
      CoercedValues cv = this.GetCoercedValues();
      double newValue = Math.Max( cv.Minimum, Math.Min( cv.Maximum, value.HasValue ? value.Value : 0d ) );
      newValue = Math.Max( newValue, cv.LowerValue );
      this.SetHigherSliderValues( newValue, null, null );
      this.HigherValue = newValue;
    }

    private void UpdateLowerValue( double? value )
    {
      CoercedValues cv = this.GetCoercedValues();
      double newValue = Math.Max( cv.Minimum, Math.Min( cv.Maximum, value.HasValue ? value.Value : 0d ) );
      newValue = Math.Min( newValue, cv.HigherValue );
      this.SetLowerSliderValues( newValue, null, null );
      this.LowerValue = newValue;
    }

    #endregion

    #region Events

    public static readonly RoutedEvent LowerValueChangedEvent = EventManager.RegisterRoutedEvent( "LowerValueChanged", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( RangeSlider ) );
    public event RoutedEventHandler LowerValueChanged
    {
      add
      {
        AddHandler( RangeSlider.LowerValueChangedEvent, value );
      }
      remove
      {
        RemoveHandler( RangeSlider.LowerValueChangedEvent, value );
      }
    }

    public static readonly RoutedEvent HigherValueChangedEvent = EventManager.RegisterRoutedEvent( "HigherValueChanged", RoutingStrategy.Bubble, typeof( RoutedEventHandler ), typeof( RangeSlider ) );
    public event RoutedEventHandler HigherValueChanged
    {
      add
      {
        AddHandler( RangeSlider.HigherValueChangedEvent, value );
      }
      remove
      {
        RemoveHandler( RangeSlider.HigherValueChangedEvent, value );
      }
    }

    #endregion //Events

    #region Events Handlers

    private void LowerRange_Click( object sender, RoutedEventArgs e )
    {
      CoercedValues cv = this.GetCoercedValues();
      //When Maximum is not greater than Minimum, the
      //slider display is in an inconsistant state. Don't 
      //consider any operation from the user
      if( cv.Minimum < cv.Maximum )
      {
        double newValue = cv.LowerValue - this.Step;
        this.LowerValue = Math.Min( cv.Maximum, Math.Max( cv.Minimum, newValue ) );
      }
    }

    private void HigherRange_Click( object sender, RoutedEventArgs e )
    {
      CoercedValues cv = this.GetCoercedValues();
      //When Maximum is not greater than Minimum, the
      //slider display is in an inconsistant state. Don't 
      //consider any operation from the user
      if( cv.Minimum < cv.Maximum )
      {
        double newValue = cv.HigherValue + this.Step;
        this.HigherValue = Math.Min( cv.Maximum, Math.Max( cv.Minimum, newValue ) );
      }
    }

    private void RangeSlider_SizeChanged( object sender, SizeChangedEventArgs e )
    {
      this.AdjustView();
    }

    private void Slider_Loaded( object sender, RoutedEventArgs e )
    {
      this.SetSlidersMargins();
      this.AdjustView();
    }

    private void LowerSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
    {
      if( (_lowerSlider != null) && _lowerSlider.IsLoaded )
      {
        if( !this.IsDeferredUpdateValues )
        {
          this.UpdateLowerValue( e.NewValue );
        }
        else
        {
          _deferredUpdateValue = e.NewValue;
          this.AdjustView( false );
        }
      }
    }

    private void HigherSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
    {
      if( ( _higherSlider != null) && _higherSlider.IsLoaded )
      {
        if( !this.IsDeferredUpdateValues )
        {
          this.UpdateHigherValue( e.NewValue );
        }
        else
        {
          _deferredUpdateValue = e.NewValue;
          this.AdjustView( true );
        }
      }
    }

    private void HigherSlider_DragCompleted( object sender, DragCompletedEventArgs e )
    {
      if( this.IsDeferredUpdateValues )
      {
        this.UpdateHigherValue( _deferredUpdateValue );
        _deferredUpdateValue = null;
        this.AdjustView();
      }
    }

    private void LowerSlider_DragCompleted( object sender, DragCompletedEventArgs e )
    {
      if( this.IsDeferredUpdateValues )
      {
        this.UpdateLowerValue( _deferredUpdateValue );
        _deferredUpdateValue = null;
        this.AdjustView();
      }
    }

    #endregion Events Handlers

    private struct CoercedValues
    {
      public double Minimum;
      public double Maximum;
      public double LowerValue;
      public double HigherValue;
    }
  }
}
