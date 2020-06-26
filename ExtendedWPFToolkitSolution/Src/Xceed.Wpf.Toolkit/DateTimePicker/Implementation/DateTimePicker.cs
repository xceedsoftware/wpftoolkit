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
using Xceed.Wpf.Toolkit.Primitives;
using System.Windows.Shapes;
#if VS2008
using Microsoft.Windows.Controls;
using Microsoft.Windows.Controls.Primitives;
#endif

namespace Xceed.Wpf.Toolkit
{
  [TemplatePart( Name = PART_Calendar, Type = typeof( Calendar ) )]
  [TemplatePart( Name = PART_TimeUpDown, Type = typeof( TimePicker ) )]
  public class DateTimePicker : DateTimePickerBase
  {
    private const string PART_Calendar = "PART_Calendar";
    private const string PART_TimeUpDown = "PART_TimeUpDown";

    #region Members

    private Calendar _calendar;
    private TimePicker _timePicker;
    private DateTime? _calendarTemporaryDateTime;
    private DateTime? _calendarIntendedDateTime;

    #endregion //Members

    #region Properties

    #region AutoCloseCalendar

    public static readonly DependencyProperty AutoCloseCalendarProperty = DependencyProperty.Register( "AutoCloseCalendar", typeof( bool ), typeof( DateTimePicker ), new UIPropertyMetadata( false ) );
    public bool AutoCloseCalendar
    {
      get
      {
        return ( bool )GetValue( AutoCloseCalendarProperty );
      }
      set
      {
        SetValue( AutoCloseCalendarProperty, value );
      }
    }

    #endregion //AutoCloseCalendar

    #region CalendarDisplayMode

    public static readonly DependencyProperty CalendarDisplayModeProperty = DependencyProperty.Register( "CalendarDisplayMode", typeof( CalendarMode )
      , typeof( DateTimePicker ), new UIPropertyMetadata( CalendarMode.Month ) );
    public CalendarMode CalendarDisplayMode
    {
      get
      {
        return (CalendarMode)GetValue( CalendarDisplayModeProperty );
      }
      set
      {
        SetValue( CalendarDisplayModeProperty, value );
      }
    }

    #endregion //CalendarDisplayMode

    #region CalendarWidth

    public static readonly DependencyProperty CalendarWidthProperty = DependencyProperty.Register( "CalendarWidth", typeof( double )
      , typeof( DateTimePicker ), new UIPropertyMetadata( 178d ) );
    public double CalendarWidth
    {
      get
      {
        return ( double )GetValue( CalendarWidthProperty );
      }
      set
      {
        SetValue( CalendarWidthProperty, value );
      }
    }

    #endregion //CalendarWidth

    #region TimeFormat

    public static readonly DependencyProperty TimeFormatProperty = DependencyProperty.Register( "TimeFormat", typeof( DateTimeFormat ), typeof( DateTimePicker ), new UIPropertyMetadata( DateTimeFormat.ShortTime ) );
    public DateTimeFormat TimeFormat
    {
      get
      {
        return ( DateTimeFormat )GetValue( TimeFormatProperty );
      }
      set
      {
        SetValue( TimeFormatProperty, value );
      }
    }

    #endregion //TimeFormat

    #region TimeFormatString

    public static readonly DependencyProperty TimeFormatStringProperty = DependencyProperty.Register( "TimeFormatString", typeof( string ), typeof( DateTimePicker ), new UIPropertyMetadata( default( String ) ), IsTimeFormatStringValid );
    public string TimeFormatString
    {
      get
      {
        return ( string )GetValue( TimeFormatStringProperty );
      }
      set
      {
        SetValue( TimeFormatStringProperty, value );
      }
    }

    private static bool IsTimeFormatStringValid(object value)
    {
      return DateTimeUpDown.IsFormatStringValid( value );
    }

    #endregion //TimeFormatString

    #region TimePickerAllowSpin

    public static readonly DependencyProperty TimePickerAllowSpinProperty = DependencyProperty.Register( "TimePickerAllowSpin", typeof( bool ), typeof( DateTimePicker ), new UIPropertyMetadata( true ) );
    public bool TimePickerAllowSpin
    {
      get
      {
        return (bool)GetValue( TimePickerAllowSpinProperty );
      }
      set
      {
        SetValue( TimePickerAllowSpinProperty, value );
      }
    }

    #endregion //TimePickerAllowSpin

    #region TimePickerShowButtonSpinner

    public static readonly DependencyProperty TimePickerShowButtonSpinnerProperty = DependencyProperty.Register( "TimePickerShowButtonSpinner", typeof( bool ), typeof( DateTimePicker ), new UIPropertyMetadata( true ) );
    public bool TimePickerShowButtonSpinner
    {
      get
      {
        return (bool)GetValue( TimePickerShowButtonSpinnerProperty );
      }
      set
      {
        SetValue( TimePickerShowButtonSpinnerProperty, value );
      }
    }

    #endregion //TimePickerShowButtonSpinner

    #region TimePickerVisibility

    public static readonly DependencyProperty TimePickerVisibilityProperty = DependencyProperty.Register( "TimePickerVisibility", typeof( Visibility ), typeof( DateTimePicker ), new UIPropertyMetadata( Visibility.Visible ) );
    public Visibility TimePickerVisibility
    {
      get
      {
        return ( Visibility )GetValue( TimePickerVisibilityProperty );
      }
      set
      {
        SetValue( TimePickerVisibilityProperty, value );
      }
    }

    #endregion //TimePickerVisibility

    #region TimeWatermark

    public static readonly DependencyProperty TimeWatermarkProperty = DependencyProperty.Register( "TimeWatermark", typeof( object ), typeof( DateTimePicker ), new UIPropertyMetadata( null ) );
    public object TimeWatermark
    {
      get
      {
        return ( object )GetValue( TimeWatermarkProperty );
      }
      set
      {
        SetValue( TimeWatermarkProperty, value );
      }
    }

    #endregion //TimeWatermark

    #region TimeWatermarkTemplate

    public static readonly DependencyProperty TimeWatermarkTemplateProperty = DependencyProperty.Register( "TimeWatermarkTemplate", typeof( DataTemplate ), typeof( DateTimePicker ), new UIPropertyMetadata( null ) );
    public DataTemplate TimeWatermarkTemplate
    {
      get
      {
        return ( DataTemplate )GetValue( TimeWatermarkTemplateProperty );
      }
      set
      {
        SetValue( TimeWatermarkTemplateProperty, value );
      }
    }

    #endregion //TimeWatermarkTemplate

    #endregion //Properties

    #region Constructors

    static DateTimePicker()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( DateTimePicker ), new FrameworkPropertyMetadata( typeof( DateTimePicker ) ) );
      UpdateValueOnEnterKeyProperty.OverrideMetadata( typeof( DateTimePicker ), new FrameworkPropertyMetadata( true ) );
    }

    public DateTimePicker()
    {
    }

    #endregion //Constructors

    #region Base Class Overrides

    public override void OnApplyTemplate()
    {
      base.OnApplyTemplate();

      if( _calendar != null )
      {
        _calendar.SelectedDatesChanged -= this.Calendar_SelectedDatesChanged;
        _calendar.MouseDoubleClick -= this.Calendar_MouseDoubleClick;
      }

      _calendar = GetTemplateChild( PART_Calendar ) as Calendar;

      if( _calendar != null )
      {
        _calendar.SelectedDatesChanged += this.Calendar_SelectedDatesChanged;
        _calendar.MouseDoubleClick += this.Calendar_MouseDoubleClick;
        _calendar.SelectedDate = Value ?? null;
        _calendar.DisplayDate = Value ?? this.ContextNow;
        this.SetBlackOutDates();
      }

      if( _timePicker != null )
      {
        _timePicker.ValueChanged -= this.TimePicker_ValueChanged;
      }
      _timePicker = GetTemplateChild( PART_TimeUpDown ) as TimePicker;
      if( _timePicker != null )
      {
        _timePicker.ValueChanged += this.TimePicker_ValueChanged;
      }
    }

    protected override void OnPreviewMouseUp( MouseButtonEventArgs e )
    {
      if( Mouse.Captured is CalendarItem)
      {
        Mouse.Capture( null );

        // Do not close calendar on Year/Month Selection. Close only on Day selection.
        if( AutoCloseCalendar && (_calendar != null) && ( _calendar.DisplayMode == CalendarMode.Month ) )
        {
          ClosePopup( true );
        }
      }
      base.OnPreviewMouseUp( e );
    }

    protected override void OnValueChanged( DateTime? oldValue, DateTime? newValue )
    {
      //The calendar only select the Date part, not the time part.
      DateTime? newValueDate = (newValue != null) 
        ? newValue.Value.Date 
        : (DateTime?)null;

      if( _calendar != null && _calendar.SelectedDate != newValueDate)
      {
        _calendar.SelectedDate = newValueDate;
        _calendar.DisplayDate = newValue.GetValueOrDefault( this.ContextNow );
      }

      //If we change any part of the datetime without
      //using the calendar when the actual date is temporary,
      //clear the temporary value. 
      if( (_calendar != null) && (_calendarTemporaryDateTime != null) && (newValue != _calendarTemporaryDateTime ))
      {
        _calendarTemporaryDateTime = null;
        _calendarIntendedDateTime = null;
      }

      if( _timePicker != null )
      {
        // sync TimePicker.TempValue with current DatetimePicker.Value
        _timePicker.UpdateTempValue( newValue );
      }

      base.OnValueChanged( oldValue, newValue );
    }

    protected override void OnIsOpenChanged( bool oldValue, bool newValue )
    {
      base.OnIsOpenChanged( oldValue, newValue );

      if( !newValue )
      {
        _calendarTemporaryDateTime = null;
        _calendarIntendedDateTime = null;
      }
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      //if the calendar is open then we don't want to modify the behavior of navigating the calendar control with the Up/Down keys.
      if( !IsOpen )
        base.OnPreviewKeyDown( e );
    }

    protected override void OnMaximumChanged( DateTime? oldValue, DateTime? newValue )
    {
      base.OnMaximumChanged( oldValue, newValue );

      this.SetBlackOutDates();
    }

    protected override void OnMinimumChanged( DateTime? oldValue, DateTime? newValue )
    {
      base.OnMinimumChanged( oldValue, newValue );

      this.SetBlackOutDates();
    }

    #endregion //Base Class Overrides

    #region Event Handlers

    protected override void HandleKeyDown( object sender, KeyEventArgs e )
    {
      // The base call will handle the Ctrl+Down, Enter and Esc keys
      // in order to open or close the popup.
      // Do not close the Calendar if the call is handled
      // by the TimePicker inside the DateTimePicker template.
      if( IsOpen
        && ( _timePicker != null)
        && _timePicker.IsKeyboardFocusWithin
        && ( _timePicker.IsOpen || e.Handled ) )
        return;

      base.HandleKeyDown( sender, e );
    }

    private void TimePicker_ValueChanged( object sender, RoutedPropertyChangedEventArgs<object> e )
    {
      e.Handled = true;

      // if UpdateValueOnEnterKey is true, 
      // Sync Value on Text only when Enter Key is pressed.
      if( this.UpdateValueOnEnterKey )
      {
        var newTime = e.NewValue as DateTime?;
        if( newTime != null )
        {
          _fireSelectionChangedEvent = false;
          var currentDate = this.ConvertTextToValue( this.TextBox.Text );
          var date = currentDate ?? this.ContextNow;
          var newValue = new DateTime( date.Year, date.Month, date.Day, newTime.Value.Hour, newTime.Value.Minute, newTime.Value.Second, newTime.Value.Millisecond, date.Kind );
          this.TextBox.Text = newValue.ToString( this.GetFormatString( this.Format ), this.CultureInfo );
          _fireSelectionChangedEvent = true;
        }
      }
    }

    private void Calendar_SelectedDatesChanged( object sender, SelectionChangedEventArgs e )
    {
      if( e.AddedItems.Count > 0 )
      {
        var newDate = ( DateTime? )e.AddedItems[ 0 ];

        if( newDate != null )
        {
          //The Calendar will always return a date with an "Unspecified" Kind.
          //Force the expected kind to the value.
          newDate = DateTime.SpecifyKind( newDate.Value, this.Kind );

          // Only change the year, month, and day part of the value. Keep everything to the last "tick."
          // "Milliseconds" aren't precise enough. Use a mathematical scheme instead.
          if( _calendarIntendedDateTime != null )
          {
            newDate = newDate.Value.Date + _calendarIntendedDateTime.Value.TimeOfDay;
            _calendarTemporaryDateTime = null;
            _calendarIntendedDateTime = null;
          }
          else if( ( _timePicker != null ) && _timePicker.TempValue.HasValue )
          {
            newDate = newDate.Value.Date + _timePicker.TempValue.Value.TimeOfDay;
          }
          else if( Value != null )
          {
            newDate = newDate.Value.Date + Value.Value.TimeOfDay;
          }

          // Always be sure that the time part of the selected value is always 
          // within the bound of the min max. The time part could be altered
          // if the calendar's selected date match the Minimum or Maximum date.
          // Keep in memory the intended time of day, in case that the selected
          // calendar date is only transitory (browsing the calendar with the keyboard)
          var limitedDateTime = this.GetClippedMinMaxValue( newDate );

          if( limitedDateTime.Value != newDate.Value )
          {
            _calendarTemporaryDateTime = limitedDateTime;
            _calendarIntendedDateTime = newDate;
            newDate = limitedDateTime;
          }
        }

        //if( this.UpdateValueOnEnterKey )
        //{
        //  _fireSelectionChangedEvent = false;
        //  this.TextBox.Text = newDate.Value.ToString( this.GetFormatString( this.Format ), this.CultureInfo );
        //  if( _timePicker != null )
        //  {
        //    // update TimePicker.TempValue with new Calendar selection.
        //    _timePicker.UpdateTempValue( newDate );
        //  }
        //  _fireSelectionChangedEvent = true;
        //}
        //else
        //{
          if( !object.Equals( newDate, Value ) )
          {
            this.Value = newDate;
          }
        //}
      }
    }

    private void Calendar_MouseDoubleClick( object sender, MouseButtonEventArgs e )
    {
      var source = e.OriginalSource as Shape;
      if( ( source != null ) && ( source.TemplatedParent is CalendarDayButton ) )
      {
        this.ClosePopup( true );
      }
    }

    protected override void Popup_Opened( object sender, EventArgs e )
    {
      base.Popup_Opened( sender, e );

      if( _calendar != null )
        _calendar.Focus();

      if( _timePicker != null )
      {
        if( this.TextBox != null )
        {
          // Set TimePicker.TempValue with current DateTimePicker.TextBox.Text.
          var initialDate = this.ConvertTextToValue( this.TextBox.Text );
          _timePicker.UpdateTempValue( initialDate );
        }
      }
    }

    #endregion //Event Handlers

    #region Methods

    private void SetBlackOutDates()
    {
      if( _calendar != null )
      {
        _calendar.BlackoutDates.Clear();

        if( ( this.Minimum != null ) && this.Minimum.HasValue && ( this.Minimum.Value >= System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MinSupportedDateTime.AddDays(1) ) )
        {
          DateTime minDate = this.Minimum.Value;
          _calendar.BlackoutDates.Add( new CalendarDateRange( System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MinSupportedDateTime, minDate.AddDays( -1 ) ) );
        }
        if( ( this.Maximum != null ) && this.Maximum.HasValue && ( this.Maximum.Value <= System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MaxSupportedDateTime.AddDays(-1)) )
        {
          DateTime maxDate = this.Maximum.Value;
          _calendar.BlackoutDates.Add( new CalendarDateRange( maxDate.AddDays( 1 ), System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.Calendar.MaxSupportedDateTime ) );
        }
      }
    }

    #endregion //Methods
  }
}
