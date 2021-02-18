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
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Core.Utilities;
using System.Collections.Generic;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_TimeListItems, Type = typeof( ListBox ) )]
  public class TimePicker : DateTimePickerBase
  {
    private const string PART_TimeListItems = "PART_TimeListItems";

    #region Members

    private ListBox _timeListBox;
    private bool _isListBoxInvalid = true;
    internal static readonly TimeSpan EndTimeDefaultValue = new TimeSpan( 23, 59, 0 );
    internal static readonly TimeSpan StartTimeDefaultValue = new TimeSpan( 0, 0, 0 );
    internal static readonly TimeSpan TimeIntervalDefaultValue = new TimeSpan( 1, 0, 0 );

    #endregion //Members

    #region Properties

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
      InvalidateListBoxItems();
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

    protected override void OnFormatChanged( DateTimeFormat oldValue, DateTimeFormat newValue )
    {
      base.OnFormatChanged( oldValue, newValue );
      InvalidateListBoxItems();
    }

    #endregion //Format

    #region MaxDropDownHeight

    public static readonly DependencyProperty MaxDropDownHeightProperty = DependencyProperty.Register( "MaxDropDownHeight", typeof( double ), typeof( TimePicker ), new UIPropertyMetadata( 130d, OnMaxDropDownHeightChanged ) );
    public double MaxDropDownHeight
    {
      get
      {
        return ( double )GetValue( MaxDropDownHeightProperty );
      }
      set
      {
        SetValue( MaxDropDownHeightProperty, value );
      }
    }

    private static void OnMaxDropDownHeightChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimePicker timePicker = o as TimePicker;
      if( timePicker != null )
        timePicker.OnMaxDropDownHeightChanged( ( double )e.OldValue, ( double )e.NewValue );
    }

    protected virtual void OnMaxDropDownHeightChanged( double oldValue, double newValue )
    {
      // TODO: Add your property changed side-effects. Descendants can override as well.
    }

    #endregion

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
      InvalidateListBoxItems();
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
      InvalidateListBoxItems();
    }

    #endregion //TimeInterval

    #endregion //Properties

    #region Constructors

    static TimePicker()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( TimePicker ), new FrameworkPropertyMetadata( typeof( TimePicker ) ) );
      FormatProperty.OverrideMetadata( typeof( TimePicker ), new UIPropertyMetadata( DateTimeFormat.ShortTime ) );
      UpdateValueOnEnterKeyProperty.OverrideMetadata( typeof( TimePicker ), new FrameworkPropertyMetadata( true ) );
    }

    #endregion //Constructors

    #region Base Class Overrides

    protected override void OnFormatStringChanged( string oldValue, string newValue )
    {
      if( this.Format == DateTimeFormat.Custom )
      {
        InvalidateListBoxItems();
      }
      base.OnFormatStringChanged( oldValue, newValue );
    }

    protected override void OnMaximumChanged( DateTime? oldValue, DateTime? newValue )
    {
      base.OnMaximumChanged( oldValue, newValue );
      this.InvalidateListBoxItems();
    }


    protected override void OnMinimumChanged( DateTime? oldValue, DateTime? newValue )
    {
      base.OnMinimumChanged( oldValue, newValue );
      this.InvalidateListBoxItems();
    }

    protected override void OnValueChanged( DateTime? oldValue, DateTime? newValue )
    {
      base.OnValueChanged( oldValue, newValue );

      // ListBox content may be affected if value's date changed and the date was
      // or is equal to Minimum or Maximum value.
      bool invalidate = false;

      if( DateTimeUtilities.IsSameDate( this.Minimum, oldValue )
        != DateTimeUtilities.IsSameDate( this.Minimum, newValue ) )
      {
        invalidate = true;
      }

      if( DateTimeUtilities.IsSameDate( this.Maximum, oldValue )
        != DateTimeUtilities.IsSameDate( this.Maximum, newValue ) )
      {
        invalidate = true;
      }

      // A value change can affect the display of the listbox items
      // if the Date part of the value has changed. This is the case when
      // the display text of the items contains part of the Date values.
      if( oldValue.GetValueOrDefault().Date != newValue.GetValueOrDefault().Date )
      {
        invalidate = true;
      }

      if( invalidate )
      {
        //Invalidate the entire listbox content
        this.InvalidateListBoxItems();
      }
      else
      {
        //Just update the selected item
        this.UpdateListBoxSelectedItem();
      }
    }

    protected override void Popup_Opened( object sender, EventArgs e )
    {
      base.Popup_Opened( sender, e );

      if( _timeListBox != null )
      {
        this.UpdateListBoxItems();

        var time = (this.Value != null) ? this.Value.Value.TimeOfDay : TimePicker.StartTimeDefaultValue;
        var nearestItem = this.GetNearestTimeItem( time );
        if( nearestItem != null )
        {
          _timeListBox.ScrollIntoView( nearestItem );
          this.UpdateListBoxSelectedItem();
        }
        _timeListBox.Focus();
      }
    }

    public override void OnApplyTemplate()
    {
      if( this.TextBox != null )
      {
        this.TextBox.GotKeyboardFocus -= this.TextBoxSpinner_GotKeyboardFocus;
      }
      if( this.Spinner != null )
      {
        this.Spinner.GotKeyboardFocus -= this.TextBoxSpinner_GotKeyboardFocus;
      }

      base.OnApplyTemplate();

      if( this.TextBox != null )
      {
        this.TextBox.GotKeyboardFocus += this.TextBoxSpinner_GotKeyboardFocus;
      }
      if( this.Spinner != null )
      {
        this.Spinner.GotKeyboardFocus += this.TextBoxSpinner_GotKeyboardFocus;
      }

      if( _timeListBox != null )
      {
        _timeListBox.SelectionChanged -= this.TimeListBox_SelectionChanged;
        _timeListBox.MouseUp -= this.TimeListBox_MouseUp;
      }

      _timeListBox = this.GetTemplateChild( PART_TimeListItems ) as ListBox;

      if( _timeListBox != null )
      {
        _timeListBox.SelectionChanged += this.TimeListBox_SelectionChanged;
        _timeListBox.MouseUp += this.TimeListBox_MouseUp;

        this.InvalidateListBoxItems();
      }
    }

    #endregion //Base Class Overrides

    #region Internal Methods

    internal void UpdateTempValue( DateTime? newDate )
    {
      var date = newDate ?? this.ContextNow;
      // Set TimePicker TextBox (not Value) to DatetimePicker TextBox.
      if( this.TextBox != null )
      {
        this.TextBox.Text = date.ToString( this.GetFormatString( this.Format ), this.CultureInfo );
      }
      // Set TimePicker TempValue to the same value. 
      // It will be used when selecting a new date in Calendar (to keep the set time) and
      // and when incrementing the TimePicker inside a DateTimePicker (to get the current time when Value is not up to date).
      this.TempValue = date;
    }

    #endregion

    #region Event Handlers

    private void TimeListBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
    {
      if( e.AddedItems.Count > 0 )
      {
        TimeItem selectedTimeListItem = ( TimeItem )e.AddedItems[ 0 ];
        var time = selectedTimeListItem.Time;

        //if( this.UpdateValueOnEnterKey )
        //{
        //  var currentValue = this.ConvertTextToValue( this.TextBox.Text );
        //  var date = currentValue ?? this.ContextNow;
        //  var newValue = new DateTime( date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds, date.Kind );
        //  this.TextBox.Text = newValue.ToString( this.GetFormatString( this.Format ), this.CultureInfo );
        //}
        //else
        //{
          var date = this.Value ?? this.ContextNow;
          this.Value = new DateTime( date.Year, date.Month, date.Day, time.Hours, time.Minutes, time.Seconds, time.Milliseconds, date.Kind );
        //}
      }
    }

    private void TextBoxSpinner_GotKeyboardFocus( object sender, KeyboardFocusChangedEventArgs e )
    {
      this.ClosePopup( true );
    }

    private void TimeListBox_MouseUp( object sender, MouseButtonEventArgs e )
    {
      this.ClosePopup( true );
    }

    #endregion //Event Handlers

    #region Methods

    private void ValidateTime( TimeSpan time )
    {
      if( time.TotalHours >= 24d )
        throw new ArgumentException( "Time value cannot be greater than or equal to 24 hours." );
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

      // Limit the content of the list to the Minimum or Maximum
      // if the date is set to the Minimum or Maximum.
      if( this.Value.HasValue )
      {
        DateTime date = this.Value.Value;
        DateTime minDate = this.Minimum.GetValueOrDefault( System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MinSupportedDateTime );
        DateTime maxDate = this.Maximum.GetValueOrDefault( System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MaxSupportedDateTime );
        TimeSpan minTime = minDate.TimeOfDay;
        TimeSpan maxTime = maxDate.TimeOfDay;

        if( date.Date == minDate.Date && time.Ticks < minTime.Ticks )
        {
          time = minTime;
        }

        if( date.Date == maxDate.Date && endTime.Ticks > maxTime.Ticks )
        {
          endTime = maxTime;
        }

        if( endTime < time )
        {
          time = endTime;
        }
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

    protected virtual TimeItem CreateTimeItem( TimeSpan time )
    {
      var date = Value ?? this.ContextNow;
      string formatString = this.GetFormatString( (DateTimeFormat)this.Format );
      return new TimeItem( date.Date.Add( time ).ToString( formatString, CultureInfo ), time );
    }

    private void UpdateListBoxSelectedItem()
    {
      if( _timeListBox != null )
      {
        TimeItem time = null;
        if( Value != null )
        {
          time = this.CreateTimeItem( Value.Value.TimeOfDay );
          if( !_timeListBox.Items.Contains( time ) )
          {
            time = null;
          }
        }

        _timeListBox.SelectedItem = time;
      }
    }

    private void InvalidateListBoxItems()
    {
      _isListBoxInvalid = true;
      if( IsOpen )
      {
        UpdateListBoxItems();
      }
    }

    private void UpdateListBoxItems()
    {
      if( _timeListBox != null )
      {
        if( _isListBoxInvalid )
        {
          _timeListBox.ItemsSource = GenerateTimeListItemsSource();
          UpdateListBoxSelectedItem();
          _isListBoxInvalid = false;
        }
      }
    }

    private TimeItem GetNearestTimeItem( TimeSpan time )
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
