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
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Collections.Generic;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_TimeListItems, Type = typeof( ListBox ) )]
  [TemplatePart( Name = PART_Popup, Type = typeof( Popup ) )]
  public class TimePicker : Control
  {
    private const string PART_TimeListItems = "PART_TimeListItems";
    private const string PART_Popup = "PART_Popup";

    #region Members

    private ListBox _timeListBox;
    private Popup _popup;
    private DateTimeFormatInfo DateTimeFormatInfo
    {
      get;
      set;
    }
    private DateTime? _initialValue;
    internal static readonly TimeSpan EndTimeDefaultValue = new TimeSpan( 23, 59, 0 );
    internal static readonly TimeSpan StartTimeDefaultValue = new TimeSpan( 0, 0, 0 );
    internal static readonly TimeSpan TimeIntervalDefaultValue = new TimeSpan( 1, 0, 0 );

    #endregion //Members

    #region Properties

    #region AllowSpin

    public static readonly DependencyProperty AllowSpinProperty = DependencyProperty.Register( "AllowSpin", typeof( bool ), typeof( TimePicker ), new UIPropertyMetadata( true ) );
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

    #region EndTime

    public static readonly DependencyProperty EndTimeProperty = DependencyProperty.Register( "EndTime", typeof( TimeSpan ), typeof( TimePicker ), new UIPropertyMetadata( EndTimeDefaultValue, new PropertyChangedCallback( OnEndTimeChanged ), new CoerceValueCallback( OnCoerceEndTime ) ) );

    private static object OnCoerceEndTime( DependencyObject o, object value )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        return timePicker.OnCoerceEndTime( ( TimeSpan )value );
      else
        return value;
    }

    private static void OnEndTimeChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        timePicker.OnEndTimeChanged( ( TimeSpan )e.OldValue, ( TimeSpan )e.NewValue );
    }

    protected virtual TimeSpan OnCoerceEndTime( TimeSpan value )
    {
      ValidateTime( value );
      return value;
    }

    protected virtual void OnEndTimeChanged( TimeSpan oldValue, TimeSpan newValue )
    {
      UpdateListBoxItems();
    }

    public TimeSpan EndTime
    {
      get
      {
        return ( TimeSpan )GetValue( EndTimeProperty );
      }
      set
      {
        SetValue( EndTimeProperty, value );
      }
    }


    #endregion //EndTime

    #region Format

    public static readonly DependencyProperty FormatProperty = DependencyProperty.Register( "Format", typeof( TimeFormat ), typeof( TimePicker ), new UIPropertyMetadata( TimeFormat.ShortTime, OnFormatChanged ) );
    public TimeFormat Format
    {
      get
      {
        return ( TimeFormat )GetValue( FormatProperty );
      }
      set
      {
        SetValue( FormatProperty, value );
      }
    }

    private static void OnFormatChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        timePicker.OnFormatChanged( ( TimeFormat )e.OldValue, ( TimeFormat )e.NewValue );
    }

    protected virtual void OnFormatChanged( TimeFormat oldValue, TimeFormat newValue )
    {
      UpdateListBoxItems();
    }

    #endregion //Format

    #region FormatString

    public static readonly DependencyProperty FormatStringProperty = DependencyProperty.Register( "FormatString", typeof( string ), typeof( TimePicker ), new UIPropertyMetadata( default( string ), OnFormatStringChanged ), IsFormatStringValid );
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

    internal static bool IsFormatStringValid( object value )
    {
      return DateTimeUpDown.IsFormatStringValid( value );
    }

    private static void OnFormatStringChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        timePicker.OnFormatStringChanged( ( string )e.OldValue, ( string )e.NewValue );
    }

    protected virtual void OnFormatStringChanged( string oldValue, string newValue )
    {
      if( this.Format == TimeFormat.Custom )
      {
        UpdateListBoxItems();
      }
    }

    #endregion //FormatString

    #region IsOpen

    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register( "IsOpen", typeof( bool ), typeof( TimePicker ), new UIPropertyMetadata( false, OnIsOpenChanged ) );
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
      TimePicker timePicker = ( TimePicker )d;
      if( timePicker != null )
        timePicker.OnIsOpenChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    private void OnIsOpenChanged( bool oldValue, bool newValue )
    {
      if( newValue )
        _initialValue = Value;
    }

    #endregion //IsOpen

    #region ShowButtonSpinner

    public static readonly DependencyProperty ShowButtonSpinnerProperty = DependencyProperty.Register( "ShowButtonSpinner", typeof( bool ), typeof( TimePicker ), new UIPropertyMetadata( true ) );
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

    #region StartTime

    public static readonly DependencyProperty StartTimeProperty = DependencyProperty.Register( "StartTime", typeof( TimeSpan ), typeof( TimePicker ), new UIPropertyMetadata( StartTimeDefaultValue, new PropertyChangedCallback( OnStartTimeChanged ), new CoerceValueCallback( OnCoerceStartTime ) ) );

    private static object OnCoerceStartTime( DependencyObject o, object value )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        return timePicker.OnCoerceStartTime( ( TimeSpan )value );
      else
        return value;
    }

    private static void OnStartTimeChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        timePicker.OnStartTimeChanged( ( TimeSpan )e.OldValue, ( TimeSpan )e.NewValue );
    }

    protected virtual TimeSpan OnCoerceStartTime( TimeSpan value )
    {
      ValidateTime( value );
      return value;
    }

    protected virtual void OnStartTimeChanged( TimeSpan oldValue, TimeSpan newValue )
    {
      UpdateListBoxItems();
    }

    public TimeSpan StartTime
    {
      get
      {
        return ( TimeSpan )GetValue( StartTimeProperty );
      }
      set
      {
        SetValue( StartTimeProperty, value );
      }
    }


    #endregion //StartTime

    #region TextAlignment

    public static readonly DependencyProperty TextAlignmentProperty = DependencyProperty.Register( "TextAlignment", typeof( TextAlignment ), typeof( TimePicker ), new UIPropertyMetadata( TextAlignment.Left ) );
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


    #endregion //TextAlignment

    #region TimeInterval

    public static readonly DependencyProperty TimeIntervalProperty = DependencyProperty.Register( "TimeInterval", typeof( TimeSpan ), typeof( TimePicker ), new UIPropertyMetadata( TimeIntervalDefaultValue, OnTimeIntervalChanged ) );
    public TimeSpan TimeInterval
    {
      get
      {
        return ( TimeSpan )GetValue( TimeIntervalProperty );
      }
      set
      {
        SetValue( TimeIntervalProperty, value );
      }
    }

    private static object OnCoerceTimeInterval( DependencyObject o, object value )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        return timePicker.OnCoerceTimeInterval( ( TimeSpan )value );
      else
        return value;
    }

    private static void OnTimeIntervalChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        timePicker.OnTimeIntervalChanged( ( TimeSpan )e.OldValue, ( TimeSpan )e.NewValue );
    }

    protected virtual TimeSpan OnCoerceTimeInterval( TimeSpan value )
    {
      ValidateTime( value );

      if( value.Ticks == 0L )
        throw new ArgumentException( "TimeInterval must be greater than zero" );

      return value;
    }


    protected virtual void OnTimeIntervalChanged( TimeSpan oldValue, TimeSpan newValue )
    {
      UpdateListBoxItems();
    }

    #endregion //TimeInterval

    #region Value

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( DateTime? ), typeof( TimePicker ), new FrameworkPropertyMetadata( null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged ) );
    public DateTime? Value
    {
      get
      {
        return ( DateTime? )GetValue( ValueProperty );
      }
      set
      {
        SetValue( ValueProperty, value );
      }
    }

    private static void OnValueChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        timePicker.OnValueChanged( ( DateTime? )e.OldValue, ( DateTime? )e.NewValue );
    }

    protected virtual void OnValueChanged( DateTime? oldValue, DateTime? newValue )
    {
      UpdateListBoxSelectedItem();

      RoutedPropertyChangedEventArgs<object> args = new RoutedPropertyChangedEventArgs<object>( oldValue, newValue );
      args.RoutedEvent = ValueChangedEvent;
      RaiseEvent( args );
    }

    #endregion //Value

    #region Watermark

    public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register( "Watermark", typeof( object ), typeof( TimePicker ), new UIPropertyMetadata( null ) );
    public object Watermark
    {
      get
      {
        return ( object )GetValue( WatermarkProperty );
      }
      set
      {
        SetValue( WatermarkProperty, value );
      }
    }

    #endregion //Watermark

    #region WatermarkTemplate

    public static readonly DependencyProperty WatermarkTemplateProperty = DependencyProperty.Register( "WatermarkTemplate", typeof( DataTemplate ), typeof( TimePicker ), new UIPropertyMetadata( null ) );
    public DataTemplate WatermarkTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( WatermarkTemplateProperty );
      }
      set
      {
        SetValue( WatermarkTemplateProperty, value );
      }
    }

    #endregion //WatermarkTemplate

    #endregion //Properties

    #region Constructors

    static TimePicker()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( TimePicker ), new FrameworkPropertyMetadata( typeof( TimePicker ) ) );
    }

    public TimePicker()
    {
      DateTimeFormatInfo = DateTimeFormatInfo.GetInstance( CultureInfo.CurrentCulture );
      Keyboard.AddKeyDownHandler( this, OnKeyDown );
      Mouse.AddPreviewMouseDownOutsideCapturedElementHandler( this, OnMouseDownOutsideCapturedElement );
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _popup != null )
        _popup.Opened -= Popup_Opened;

      _popup = GetTemplateChild( PART_Popup ) as Popup;

      if( _popup != null )
        _popup.Opened += Popup_Opened;

      if( _timeListBox != null )
      {
        _timeListBox.SelectionChanged -= TimeListBox_SelectionChanged;
        _timeListBox.MouseUp -= TimeListBox_MouseUp;
      }

      _timeListBox = GetTemplateChild( PART_TimeListItems ) as ListBox;

      if( _timeListBox != null )
      {
        _timeListBox.SelectionChanged += TimeListBox_SelectionChanged;
        _timeListBox.MouseUp += TimeListBox_MouseUp;

        UpdateListBoxItems();
      }
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    private void OnKeyDown( object sender, KeyEventArgs e )
    {
      if( !IsOpen )
      {
        if( KeyboardUtilities.IsKeyModifyingPopupState( e ) )
        {
          IsOpen = true;
          // TimeListBox_Loaded() will focus on ListBoxItem.
          e.Handled = true;
        }
      }
      else
      {
        if( KeyboardUtilities.IsKeyModifyingPopupState( e ) )
        {
          CloseTimePicker( true );
          e.Handled = true;
        }
        else if( e.Key == Key.Enter )
        {
          CloseTimePicker( true );
          e.Handled = true;
        }
        else if( e.Key == Key.Escape )
        {
          Value = _initialValue;
          CloseTimePicker( true );
          e.Handled = true;
        }
      }
    }

    private void OnMouseDownOutsideCapturedElement( object sender, MouseButtonEventArgs e )
    {
      CloseTimePicker( false );
    }

    private void TimeListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      if( e.AddedItems.Count > 0 )
      {
        TimeItem selectedTimeListItem = ( TimeItem )e.AddedItems[ 0 ];
        var time = selectedTimeListItem.Time;
        var date = Value ?? DateTime.MinValue;

        Value = new DateTime( date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds );
      }
    }

    private void TimeListBox_MouseUp( object sender, MouseButtonEventArgs e )
    {
       CloseTimePicker( true );
    }

    private void Popup_Opened( object sender, EventArgs e )
    {
      if( _timeListBox != null )
      {
        TimeSpan time = ( Value != null ) ? Value.Value.TimeOfDay : StartTimeDefaultValue;
        TimeItem nearestItem = this.GetNearestTimeItem( time );
        if( nearestItem != null )
        {
          _timeListBox.ScrollIntoView( nearestItem );
          ListBoxItem listBoxItem = ( ListBoxItem )_timeListBox.ItemContainerGenerator.ContainerFromItem( nearestItem );
          if( listBoxItem != null )
          {
            listBoxItem.Focus();
          }
        }
      }
    }

    #endregion //Event Handlers

    #region Events

    //Due to a bug in Visual Studio, you cannot create event handlers for nullable args in XAML, so I have to use object instead.
    public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent( "ValueChanged", RoutingStrategy.Bubble, typeof( RoutedPropertyChangedEventHandler<object> ), typeof( TimePicker ) );
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

    #region Methods

    private void ValidateTime( TimeSpan time )
    {
      if( time.TotalHours >= 24d )
        throw new ArgumentException( "Time value cannot be greater than or equal to 24 hours." );
    }

    private void CloseTimePicker( bool isFocusOnTimePicker )
    {
      if( IsOpen )
        IsOpen = false;
      ReleaseMouseCapture();

      if( isFocusOnTimePicker )
        Focus();
    }

    public IEnumerable GenerateTimeListItemsSource()
    {
      TimeSpan time = StartTime;
      TimeSpan endTime = EndTime;

      if( endTime <= time )
      {
        endTime = EndTimeDefaultValue;
        time = StartTimeDefaultValue;
      }

      TimeSpan timeInterval = TimeInterval;
      List<TimeItem> timeItemList = new List<TimeItem>();

      if( time != null && endTime != null && timeInterval != null && timeInterval.Ticks > 0 )
      {
        while( time <= endTime )
        {
          timeItemList.Add( this.CreateTimeItem( time ) );
          time = time.Add( timeInterval );
        }
      }
      return timeItemList;
    }

    private TimeItem CreateTimeItem( TimeSpan time )
    {
      return new TimeItem( DateTime.MinValue.Add( time ).ToString( GetTimeFormat(), CultureInfo.CurrentCulture ), time );
    }

    private string GetTimeFormat()
    {
      switch( Format )
      {
        case TimeFormat.Custom:
          return FormatString;
        case TimeFormat.LongTime:
          return DateTimeFormatInfo.LongTimePattern;
        case TimeFormat.ShortTime:
          return DateTimeFormatInfo.ShortTimePattern;
        default:
          return DateTimeFormatInfo.ShortTimePattern;
      }
    }

    private void UpdateListBoxSelectedItem()
    {
      if(_timeListBox != null)
      {
        TimeItem time = null;
        if(Value != null)
        {
          time = this.CreateTimeItem( Value.Value.TimeOfDay );
          if(!_timeListBox.Items.Contains( time ))
          {
            time = null;
          }
        }

        _timeListBox.SelectedItem = time;
      }
    }

    private void UpdateListBoxItems()
    {
      if( _timeListBox != null )
      {
        _timeListBox.ItemsSource = GenerateTimeListItemsSource();
      }
    }

    private TimeItem GetNearestTimeItem(TimeSpan time)
    {
      if( _timeListBox != null )
      {
        int itemCount = _timeListBox.Items.Count;
        for( int i = 0; i < itemCount; i++ )
        {
          TimeItem timeItem = _timeListBox.Items[ i ] as TimeItem;
          if( timeItem != null )
          {
            if( timeItem.Time >= time )
              return timeItem;
          }
        }

        //They are all less than the searched time. 
        //Return the last one. (Should also be the greater one.)
        if( itemCount > 0 )
        {
          return _timeListBox.Items[ itemCount - 1 ] as TimeItem;
        }
      }

      return null;
    }

    #endregion //Methods
  }
}
