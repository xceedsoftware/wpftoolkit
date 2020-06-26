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
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Primitives;
using System.Diagnostics;

namespace Xceed.Wpf.Toolkit
{
  public class TimeSpanUpDown : DateTimeUpDownBase<TimeSpan?>
  {
    #region Private Members

    private static int HoursInDay = 24;
    private static int MinutesInDay = 1440;
    private static int MinutesInHour = 60;    
    private static int SecondsInDay = 86400;
    private static int SecondsInHour = 3600;
    private static int SecondsInMinute = 60;
    private static int MilliSecondsInDay = TimeSpanUpDown.SecondsInDay * 1000;
    private static int MilliSecondsInHour = TimeSpanUpDown.SecondsInHour * 1000;
    private static int MilliSecondsInMinute = TimeSpanUpDown.SecondsInMinute * 1000;
    private static int MilliSecondsInSecond = 1000;

    #endregion

    #region Constructors

    static TimeSpanUpDown()
    {
      DefaultStyleKeyProperty.OverrideMetadata( typeof( TimeSpanUpDown ), new FrameworkPropertyMetadata( typeof( TimeSpanUpDown ) ) );
      MaximumProperty.OverrideMetadata( typeof( TimeSpanUpDown ), new FrameworkPropertyMetadata( TimeSpan.MaxValue ) );
      MinimumProperty.OverrideMetadata( typeof( TimeSpanUpDown ), new FrameworkPropertyMetadata( TimeSpan.MinValue ) );
      DefaultValueProperty.OverrideMetadata( typeof( TimeSpanUpDown ), new FrameworkPropertyMetadata( TimeSpan.Zero ) );
    }

    public TimeSpanUpDown()
    {
      DataObject.AddPastingHandler( this, this.OnPasting );
    }

    #endregion //Constructors

    #region Properties

    #region FractionalSecondsDigitsCount

    public static readonly DependencyProperty FractionalSecondsDigitsCountProperty = DependencyProperty.Register( "FractionalSecondsDigitsCount", typeof( int ), typeof( TimeSpanUpDown ), new UIPropertyMetadata( 0, OnFractionalSecondsDigitsCountChanged, OnCoerceFractionalSecondsDigitsCount ) );
    public int FractionalSecondsDigitsCount
    {
      get
      {
        return ( int )GetValue( FractionalSecondsDigitsCountProperty );
      }
      set
      {
        SetValue( FractionalSecondsDigitsCountProperty, value );
      }
    }

    private static object OnCoerceFractionalSecondsDigitsCount( DependencyObject o, object value )
    {
      TimeSpanUpDown timeSpanUpDown = o as TimeSpanUpDown;
      if( timeSpanUpDown != null )
      {
        int digitsCount = (int)value;
        if( digitsCount < 0 || digitsCount > 3 )
          throw new ArgumentException( "Fractional seconds digits count must be between 0 and 3." );
      }
      return value;
    }

    private static void OnFractionalSecondsDigitsCountChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimeSpanUpDown timeSpanUpDown = o as TimeSpanUpDown;
      if( timeSpanUpDown != null )
        timeSpanUpDown.OnFractionalSecondsDigitsCountChanged( ( int )e.OldValue, ( int )e.NewValue );
    }

    protected virtual void OnFractionalSecondsDigitsCountChanged( int oldValue, int newValue )
    {
      this.UpdateValue();
    }

    #endregion //FractionalSecondsDigitsCount

    #region ShowDays

    public static readonly DependencyProperty ShowDaysProperty = DependencyProperty.Register( "ShowDays", typeof( bool ), typeof( TimeSpanUpDown ), new UIPropertyMetadata( true, OnShowDaysChanged ) );
    public bool ShowDays
    {
      get
      {
        return (bool)GetValue( ShowDaysProperty );
      }
      set
      {
        SetValue( ShowDaysProperty, value );
      }
    }

    private static void OnShowDaysChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      var timeSpanUpDown = o as TimeSpanUpDown;
      if( timeSpanUpDown != null )
        timeSpanUpDown.OnShowDaysChanged( (bool)e.OldValue, (bool)e.NewValue );
    }

    protected virtual void OnShowDaysChanged( bool oldValue, bool newValue )
    {
      this.UpdateValue();
    }

    #endregion //ShowDays

    #region ShowSeconds

    public static readonly DependencyProperty ShowSecondsProperty = DependencyProperty.Register( "ShowSeconds", typeof( bool ), typeof( TimeSpanUpDown ), new UIPropertyMetadata( true, OnShowSecondsChanged ) );
    public bool ShowSeconds
    {
      get
      {
        return ( bool )GetValue( ShowSecondsProperty );
      }
      set
      {
        SetValue( ShowSecondsProperty, value );
      }
    }

    private static void OnShowSecondsChanged( DependencyObject o, DependencyPropertyChangedEventArgs e )
    {
      TimeSpanUpDown timeSpanUpDown = o as TimeSpanUpDown;
      if( timeSpanUpDown != null )
        timeSpanUpDown.OnShowSecondsChanged( ( bool )e.OldValue, ( bool )e.NewValue );
    }

    protected virtual void OnShowSecondsChanged( bool oldValue, bool newValue )
    {
      this.UpdateValue();
    }

    #endregion //ShowSeconds

    #endregion

    #region BaseClass Overrides

    public override bool CommitInput()
    {
      var sync = this.SyncTextAndValueProperties( true, Text );

      if( this.UpdateValueOnEnterKey && ( _selectedDateTimeInfo != null ) && ( _dateTimeInfoList != null ) )
      {
        // Update SelectedDateTimeInfo and TextBox selection after sync is done.
        var selectionInfo = _dateTimeInfoList.FirstOrDefault( x => ( x.Type == _selectedDateTimeInfo.Type ) && ( x.Type != DateTimePart.Other ) );
        _selectedDateTimeInfo = ( selectionInfo != null ) ? selectionInfo : _dateTimeInfoList.FirstOrDefault( x => x.Type != DateTimePart.Other );
        if( _selectedDateTimeInfo != null )
        {
          _fireSelectionChangedEvent = false;
          this.TextBox.Select( _selectedDateTimeInfo.StartPosition, _selectedDateTimeInfo.Length );
          _fireSelectionChangedEvent = true;
        }
      }

      return sync;
    }

    protected override void OnCultureInfoChanged( CultureInfo oldValue, CultureInfo newValue )
    {
      var value = this.UpdateValueOnEnterKey
                  ? (this.TextBox != null) ? this.ConvertTextToValue( this.TextBox.Text ) : null
                  : this.Value;
      this.InitializeDateTimeInfoList( value );
    }

    protected override void SetValidSpinDirection()
    {
      ValidSpinDirections validDirections = ValidSpinDirections.None;

      if( !this.IsReadOnly )
      {
        if( this.IsLowerThan( this.Value, this.Maximum ) || !this.Value.HasValue || !this.Maximum.HasValue)
          validDirections = validDirections | ValidSpinDirections.Increase;

        if( this.IsGreaterThan( this.Value, this.Minimum ) || !this.Value.HasValue || !this.Minimum.HasValue )
          validDirections = validDirections | ValidSpinDirections.Decrease;
      }

      if( this.Spinner != null )
        this.Spinner.ValidSpinDirection = validDirections;
    }

    protected override void OnIncrement()
    {
      this.Increment( this.Step );
    }

    protected override void OnDecrement()
    {
      this.Increment( -this.Step );
    }

    protected override string ConvertValueToText()
    {
      if( this.Value == null )
        return string.Empty;

      return this.ParseValueIntoTimeSpanInfo( this.Value, true );
    }

    protected override TimeSpan? ConvertTextToValue( string text )
    {
      if( string.IsNullOrEmpty( text ) )
        return null;

      var timeSpan = TimeSpan.MinValue;

      var separators = text.Where( x => x == ':' || x == '.' ).ToList();
      var stringValues = text.Split( new char[] { ':', '.' } );
      if( ( stringValues.Count() <= 1 ) || stringValues.Any( x => string.IsNullOrEmpty( x ) ) )
      {
        return this.ResetToLastValidValue();
      }

      var intValues = new int[ stringValues.Count() ];
      for( int i = 0; i < stringValues.Count(); ++i )
      {
        if( !int.TryParse( stringValues[ i ].Replace( "-", "" ), out intValues[ i ] ) )
        {
          return this.ResetToLastValidValue();
        }
      }

      if( intValues.Count() >= 2 )
      {
        var haveMS = ( separators.Count() > 1 ) && ( separators.Last() == '.' );
        var haveDays = ( separators.Count() > 1 ) && ( separators.First() == '.' ) && ( intValues.Count() >= 3 );

        if( this.ShowDays )
        {
          var days = haveDays ? intValues[ 0 ] : intValues[ 0 ] / 24;
          if( days > TimeSpan.MaxValue.Days )
            return this.ResetToLastValidValue();
          var hours = haveDays ? intValues[ 1 ] : intValues[ 0 ] % 24;
          if( ( ( days * TimeSpanUpDown.HoursInDay ) + hours ) > TimeSpan.MaxValue.TotalHours )
            return this.ResetToLastValidValue();
          var minutes = haveDays ? intValues[ 2 ] : intValues[ 1 ];
          if( ( ( days * TimeSpanUpDown.MinutesInDay ) + ( hours * TimeSpanUpDown.MinutesInHour ) + minutes ) > TimeSpan.MaxValue.TotalMinutes )
            return this.ResetToLastValidValue();
          var seconds = this.ShowSeconds
                        ? haveDays && ( intValues.Count() >= 4 ) ? intValues[ 3 ] : ( intValues.Count() >= 3 ) ? intValues[ 2 ] : 0
                        : 0;
          if( ( ( days * TimeSpanUpDown.SecondsInDay ) + ( hours * TimeSpanUpDown.SecondsInHour ) + ( minutes * TimeSpanUpDown.SecondsInMinute ) + seconds ) > TimeSpan.MaxValue.TotalSeconds )
            return this.ResetToLastValidValue();
          var milliseconds = haveMS ? intValues.Last() : 0;
          if( ( ( days * TimeSpanUpDown.MilliSecondsInDay ) + ( hours * TimeSpanUpDown.MilliSecondsInHour ) + ( minutes * TimeSpanUpDown.MilliSecondsInMinute ) + ( seconds * TimeSpanUpDown.MilliSecondsInSecond ) + milliseconds ) > TimeSpan.MaxValue.TotalMilliseconds )
            return this.ResetToLastValidValue();

          timeSpan = new TimeSpan( days, hours, minutes, seconds, milliseconds );
        }
        else
        {
          var hours = intValues[ 0 ];
          if( hours > TimeSpan.MaxValue.TotalHours )
            return this.ResetToLastValidValue();
          var minutes = intValues[ 1 ];
          if( ( ( hours * TimeSpanUpDown.MinutesInHour ) + minutes ) > TimeSpan.MaxValue.TotalMinutes )
            return this.ResetToLastValidValue();
          var seconds = this.ShowSeconds && ( intValues.Count() >= 3 ) ? intValues[ 2 ] : 0;
          if( ( ( hours * TimeSpanUpDown.SecondsInHour ) + ( minutes * TimeSpanUpDown.SecondsInMinute ) + seconds ) > TimeSpan.MaxValue.TotalSeconds )
            return this.ResetToLastValidValue();
          var milliseconds = haveMS ? intValues.Last() : 0;
          if( ( ( hours * TimeSpanUpDown.MilliSecondsInHour ) + ( minutes * TimeSpanUpDown.MilliSecondsInMinute ) + ( seconds * TimeSpanUpDown.MilliSecondsInSecond ) + milliseconds ) > TimeSpan.MaxValue.TotalMilliseconds )
            return this.ResetToLastValidValue();

          timeSpan = new TimeSpan( 0, hours, minutes, seconds, milliseconds );
        }

        if( text.StartsWith( "-" ) )
        {
          timeSpan = timeSpan.Negate();
        }
      }

      if( this.ClipValueToMinMax )
      {
        return this.GetClippedMinMaxValue( timeSpan );
      }

      this.ValidateDefaultMinMax( timeSpan );

      return timeSpan;
    }

    protected override void OnPreviewTextInput( TextCompositionEventArgs e )
    {
      e.Handled = !this.IsNumber( e.Text );
      base.OnPreviewTextInput( e );
    }

    protected override void OnPreviewKeyDown( KeyEventArgs e )
    {
      if( e.Key == Key.Space )
      {
        e.Handled = true;
      }
      base.OnPreviewKeyDown( e );
    }

    protected override void OnTextChanged( string previousValue, string currentValue )
    {
      if( !_processTextChanged )
        return;

      if( String.IsNullOrEmpty( currentValue ) )
      {
        if( !this.UpdateValueOnEnterKey )
        {
          this.Value = null;
        }
        return;
      }

      TimeSpan current = this.Value.HasValue ? this.Value.Value : new TimeSpan();
      TimeSpan result;
      // Validate when more than 60 seconds (or more than 60 minutes, or more than 24 hours) are entered.
      var separators = currentValue.Where( x => x == ':' || x == '.' ).ToList();
      var stringValues = currentValue.Split( new char[] { ':', '.' } );
      if( ( stringValues.Count() >= 2 ) && !stringValues.Any( x => string.IsNullOrEmpty( x ) ) )
      {
        bool haveDays = (separators.First() == '.') && ( stringValues.Count() >= 3);
        bool haveMS = ( separators.Count() > 1 ) && ( separators.Last() == '.' );

        var values = new int[ stringValues.Count() ];
        for( int i = 0; i < stringValues.Count(); ++i )
        {
          if( !int.TryParse( stringValues[ i ], out values[ i ] ) )
          {
            return;
          }
        }

        var days = haveDays ? Math.Abs( values[ 0 ] ): 0;
        if( days > TimeSpan.MaxValue.Days )
          return;
        var hours = haveDays ? Math.Abs( values[ 1 ] ) : Math.Abs( values[ 0 ] );
        if( ( ( days * TimeSpanUpDown.HoursInDay ) + hours) > TimeSpan.MaxValue.TotalHours )
          return;
        var minutes = haveDays ? Math.Abs( values[ 2 ] ) : Math.Abs( values[ 1 ] );
        if( ( ( days * TimeSpanUpDown.MinutesInDay ) + ( hours * TimeSpanUpDown.MinutesInHour ) + minutes ) > TimeSpan.MaxValue.TotalMinutes )
          return;
        var seconds = ( haveDays && this.ShowSeconds && ( values.Count() >= 4 ) ) ? Math.Abs( values[ 3 ] ) : this.ShowSeconds && ( values.Count() >= 3 ) ? Math.Abs( values[ 2 ] ) : 0;
        if( ( ( days * TimeSpanUpDown.SecondsInDay ) + ( hours * TimeSpanUpDown.SecondsInHour ) + ( minutes * TimeSpanUpDown.SecondsInMinute ) + seconds ) > TimeSpan.MaxValue.TotalSeconds )
          return;
        var milliseconds = haveMS ? Math.Abs( values.Last() ) : 0;
        if( ( ( days * TimeSpanUpDown.MilliSecondsInDay ) + ( hours * TimeSpanUpDown.MilliSecondsInHour ) + (minutes * TimeSpanUpDown.MilliSecondsInMinute ) + (seconds * TimeSpanUpDown.MilliSecondsInSecond ) + milliseconds ) > TimeSpan.MaxValue.TotalMilliseconds )
          return;

        result = new TimeSpan( days, hours, minutes, seconds, milliseconds );
        if( values[ 0 ] < 0 )
        {
          result = result.Negate();
        }

        currentValue = result.ToString();
      }
      else
      {
        Debug.Assert( false, "Something went wrong when parsing TimeSpan." );
        return;
      }

      var previousValues = ( previousValue != null ) ? previousValue.Split( new char[] { ':', '.' } ) : null;
      var currentValues = ( currentValue != null ) ? currentValue.Split( new char[] { ':', '.' } ) : null;
      var canSync = ( previousValues != null ) 
                    && ( currentValues != null )
                    && ( previousValues.Length == currentValues.Length )  // same number of time parts.
                    && ( currentValue.Length == previousValue.Length );   // same number of digits.
      // When text is typed, Sync Value on Text only when UpdateValueOnEnterKey is false and time format is the same.
      if( ( _isTextChangedFromUI && !this.UpdateValueOnEnterKey && canSync )
        || !_isTextChangedFromUI )
      {
        this.SyncTextAndValueProperties( true, currentValue );
      }
    }

    protected override void OnValueChanged( TimeSpan? oldValue, TimeSpan? newValue )
    {
      //whenever the value changes we need to parse out the value into out DateTimeInfo segments so we can keep track of the individual pieces
      //but only if it is not null
      if( newValue != null )
      {
        var value = this.UpdateValueOnEnterKey
                  ? (this.TextBox != null) ? this.ConvertTextToValue( this.TextBox.Text ) : null
                  : this.Value;
        this.InitializeDateTimeInfoList( value );
      }
      base.OnValueChanged( oldValue, newValue );
    }

    protected override void PerformMouseSelection()
    {
      if( !this.UpdateValueOnEnterKey )
      {
        this.CommitInput();
        this.InitializeDateTimeInfoList( this.Value );
      }

      base.PerformMouseSelection();
    }

    protected override void InitializeDateTimeInfoList( TimeSpan? value )
    {
      var lastDayInfo = _dateTimeInfoList.FirstOrDefault( x => x.Type == DateTimePart.Day );
      bool hasDay = lastDayInfo != null;
      var negInfo = _dateTimeInfoList.FirstOrDefault( x => x.Type == DateTimePart.Other );
      bool hasNegative = (negInfo != null) && (negInfo.Content == "-");

      _dateTimeInfoList.Clear();

      if( value.HasValue && value.Value.TotalMilliseconds < 0 )
      {
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = "-", IsReadOnly = true } );
        // Negative has been added, move TextBox.Selection to keep it on current DateTimeInfo
        if( !hasNegative && (this.TextBox != null) )
        {
          _fireSelectionChangedEvent = false;
          this.TextBox.SelectionStart++;
          _fireSelectionChangedEvent = true;
        }
      }

      if( this.ShowDays )
      {
        if( value.HasValue && value.Value.Days != 0 )
        {
          int dayLength = Math.Abs( value.Value.Days ).ToString().Length;
          _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Day, Length = dayLength, Format = "dd" } );
          _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ".", IsReadOnly = true } );

          if( this.TextBox != null )
          {
            //number of digits for days has changed when selection is not on date part, move TextBox.Selection to keep it on current DateTimeInfo
            if( hasDay && ( dayLength != lastDayInfo.Length ) && ( _selectedDateTimeInfo.Type != DateTimePart.Day ) )
            {
              _fireSelectionChangedEvent = false;
              this.TextBox.SelectionStart = Math.Max( 0, this.TextBox.SelectionStart + ( dayLength - lastDayInfo.Length ) );
              _fireSelectionChangedEvent = true;
            }
            // Day has been added, move TextBox.Selection to keep it on current DateTimeInfo
            else if( !hasDay )
            {
              _fireSelectionChangedEvent = false;
              this.TextBox.SelectionStart += ( dayLength + 1 );
              _fireSelectionChangedEvent = true;
            }
          }
        }
        // Day has been removed, move TextBox.Selection to keep it on current DateTimeInfo
        else if( hasDay )
        {
          _fireSelectionChangedEvent = false;
          this.TextBox.SelectionStart = Math.Max( hasNegative ? 1 : 0, this.TextBox.SelectionStart - ( lastDayInfo.Length + 1 ) );
          _fireSelectionChangedEvent = true;
        }
      }

      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Hour24, Length = 2, Format = "hh" } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ":", IsReadOnly = true } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Minute, Length = 2, Format = "mm" } );
      if( this.ShowSeconds )
      {
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ":", IsReadOnly = true } );
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Second, Length = 2, Format = "ss" } );
      }

      if( this.FractionalSecondsDigitsCount > 0 )
      {
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ".", IsReadOnly = true } );
        string fraction = new string( 'f', this.FractionalSecondsDigitsCount );
        //If the "f" custom format specifier is used alone, specify "%f" so that it is not misinterpreted as a standard format string.
        if( fraction.Length == 1 )
        {
          fraction = "%" + fraction;
        }
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Millisecond, Length = this.FractionalSecondsDigitsCount, Format = fraction } );
      }

      if( value.HasValue )
      {
        this.ParseValueIntoTimeSpanInfo( value, true );
      }
    }

    protected override bool IsLowerThan( TimeSpan? value1, TimeSpan? value2 )
    {
      if( value1 == null || value2 == null )
        return false;

      return (value1.Value < value2.Value);
    }

    protected override bool IsGreaterThan( TimeSpan? value1, TimeSpan? value2 )
    {
      if( value1 == null || value2 == null )
        return false;

      return (value1.Value > value2.Value);
    }

    internal override void Select( DateTimeInfo info )
    {
      if( this.UpdateValueOnEnterKey )
      {
        if( ( info != null ) && !info.Equals( _selectedDateTimeInfo ) && ( this.TextBox != null ) && !string.IsNullOrEmpty( this.TextBox.Text ) )
        {
          var separatorToSkipCount = _dateTimeInfoList.IndexOf( info ) / 2;
          if( separatorToSkipCount < 0 )
          {
            base.Select( info );
          }
          else
          {
            var textValues = this.Text.Split( new char[] { ':', '.' } );
            var selectionStart = textValues.Take( separatorToSkipCount ).Sum( x => x.Length ) + separatorToSkipCount;
            var selectionLength = textValues[ separatorToSkipCount ].Length;
            // Do not select the "-" sign when moving selection with arrows.
            if( ( separatorToSkipCount == 0 ) && ( textValues.First().StartsWith( "-" ) ) )
            {
              selectionStart += 1;
              selectionLength -= 1;
            }

            _fireSelectionChangedEvent = false;
            this.TextBox.Select( selectionStart, selectionLength );
            _fireSelectionChangedEvent = true;
            _selectedDateTimeInfo = info;
#if VS2008
        this.CurrentDateTimePart = info.Type;
#else
            this.SetCurrentValue( DateTimeUpDownBase<TimeSpan?>.CurrentDateTimePartProperty, info.Type );
#endif
          }
        }
      }
      else
      {
        base.Select( info );
      }
    }


    #endregion

    #region Methods

    private string ParseValueIntoTimeSpanInfo( TimeSpan? value, bool modifyInfo )
    {
      string text = string.Empty;

      _dateTimeInfoList.ForEach( info =>
      {
        if( info.Format == null )
        {
          if( modifyInfo )
          {
            info.StartPosition = text.Length;
            info.Length = info.Content.Length;
          }
          text += info.Content;
        }
        else
        {
          TimeSpan span = TimeSpan.Parse( value.ToString() );
          if( modifyInfo )
          {
            info.StartPosition = text.Length;
          }
          var content = "";
#if VS2008
          switch (info.Format)
          {
              case "hh":
                  // Display days and hours or totalHours
                  content = ( !this.ShowDays && ( span.Days != 0 ) && ( info.Format == "hh" ) )
                            ? Math.Truncate( Math.Abs( span.TotalHours ) ).ToString("00")
                            : span.Hours.ToString("00");
                  break;
              case "mm":
                  content = span.Minutes.ToString("00");
                  break;
              case "ss":
                 content = span.Seconds.ToString("00");
                  break;
              case "dd":
                  content = span.Days.ToString();
                  break;
              case "%f":
                  content = (span.Milliseconds / 100).ToString();
                  break;
              case "ff":
                 content = (span.Milliseconds / 10).ToString();
                  break;
              case "fff":
                 content = span.Milliseconds.ToString();
                  break;
              default:
                  throw new InvalidOperationException("Wrong TimeSpan format");
          }
#else
          // Display days and hours or totalHours
          content = ( !this.ShowDays && ( span.Days != 0 ) && ( info.Format == "hh" ) )
                    ? Math.Truncate( Math.Abs( span.TotalHours ) ).ToString()
                    : span.ToString( info.Format, this.CultureInfo.DateTimeFormat );
#endif

          if( modifyInfo )
          {
            if( info.Format == "dd" )
            {
              content = Convert.ToInt32( content ).ToString();
            }
            info.Content = content;
            info.Length = info.Content.Length;
          }
          text += content;
        }
      } );

      return text;
    }

    private TimeSpan? UpdateTimeSpan( TimeSpan? currentValue, int value )
    {
      DateTimeInfo info = _selectedDateTimeInfo;

      //this only occurs when the user manually type in a value for the Value Property
      if( info == null )
      {
        info = (this.CurrentDateTimePart != DateTimePart.Other)
               ? this.GetDateTimeInfo( this.CurrentDateTimePart )
               : (_dateTimeInfoList[ 0 ].Content != "-") ? _dateTimeInfoList[ 0 ] : _dateTimeInfoList[ 1 ]; //Skip negative sign
        if( info == null )
          info = _dateTimeInfoList[ 0 ];
      }

      TimeSpan? result = null;

      try
      {
        switch( info.Type )
        {
          case DateTimePart.Day:
            result = ( ( TimeSpan )currentValue).Add( new TimeSpan( value, 0, 0, 0, 0 ) );
            break;
          case DateTimePart.Hour24:
            result = ( ( TimeSpan )currentValue).Add( new TimeSpan( 0, value, 0, 0, 0 ) );
            break;
          case DateTimePart.Minute:
            result = ( ( TimeSpan )currentValue).Add( new TimeSpan( 0, 0, value, 0, 0 ) );
            break;
          case DateTimePart.Second:
            result = ( ( TimeSpan )currentValue).Add( new TimeSpan( 0, 0, 0, value, 0 ) );
            break;
          case DateTimePart.Millisecond:
            switch( this.FractionalSecondsDigitsCount )
            {
              case 1:
                value = value * 100;
                break;
              case 2:
                value = value * 10;
                break;
              default:
                value = value * 1;
                break;
            }
            result = ( ( TimeSpan )currentValue).Add( new TimeSpan( 0, 0, 0, 0, value ) );
            break;
          default:
            break;
        }
      }
      catch
      {
        //this can occur if the date/time = 1/1/0001 12:00:00 AM which is the smallest date allowed.
        //I could write code that would validate the date each and everytime but I think that it would be more
        //efficient if I just handle the edge case and allow an exeption to occur and swallow it instead.
      }

      result = ( ( result != null ) && result.HasValue )
                ? result.Value
                : result;

      result = this.CoerceValueMinMax( result );

      return result;
    }

    private void Increment( int step )
    {
      // if UpdateValueOnEnterKey is true, 
      // Sync Value on Text only when Enter Key is pressed.
      if( this.UpdateValueOnEnterKey )
      {
        var newTextBoxContent = string.Empty;
        var currentValue = this.ConvertTextToValue( this.TextBox.Text );
        var newValue = currentValue.HasValue
                       ? this.UpdateTimeSpan( currentValue, step )
                       : this.DefaultValue ?? TimeSpan.Zero;

        if( newValue != null && ( _dateTimeInfoList != null) )
        {
          var selectionStart = 0;
          var selectionLength = 0;

          // Start with a negative sign.
          if( ( newValue.Value.TotalMilliseconds < 0 ) && ( _dateTimeInfoList[ 0 ].Content != "-" ) )
          {
            newTextBoxContent = "-";
          }
          for( int i = 0; i < _dateTimeInfoList.Count; ++i )
          {
            var timePart = _dateTimeInfoList[ i ];
            var timePartContentLength = ( timePart.Content != null ) ? timePart.Content.Length : timePart.Length;

            // Current timePart is the selected timePart, TextBox selection will start here.
            if( ( _selectedDateTimeInfo != null ) && ( timePart.Type == _selectedDateTimeInfo.Type ) )
            {
              selectionStart = newTextBoxContent.Length;
            }
            // Adjust time part start and length.
            switch( timePart.Type )
            {
              case DateTimePart.Day:
                var dayText = Math.Abs( newValue.Value.Days ).ToString( new string( '0', timePartContentLength ) );
                timePart.StartPosition = newTextBoxContent.Length;
                timePart.Length = dayText.Length;
                newTextBoxContent += dayText;
                break;
              case DateTimePart.Hour24:
                var hourText = ( i <= 1 ) 
                                ? Math.Truncate( Math.Abs( newValue.Value.TotalHours ) ).ToString( new string( '0', timePartContentLength ) ) 
                                : Math.Abs( newValue.Value.Hours) .ToString( new string( '0', timePartContentLength ) );
                timePart.StartPosition = newTextBoxContent.Length;
                timePart.Length = hourText.Length;
                newTextBoxContent += hourText;
                break;
              case DateTimePart.Minute:
                var minuteText = ( i <= 1 ) 
                                  ? Math.Truncate( Math.Abs( newValue.Value.TotalMinutes ) ).ToString( new string( '0', timePartContentLength ) ) 
                                  : Math.Abs( newValue.Value.Minutes ).ToString( new string( '0', timePartContentLength ) );
                timePart.StartPosition = newTextBoxContent.Length;
                timePart.Length = minuteText.Length;
                newTextBoxContent += minuteText;
                break;
              case DateTimePart.Second:
                var secondText = ( i <= 1 ) 
                                  ? Math.Truncate( Math.Abs( newValue.Value.TotalSeconds ) ).ToString( new string( '0', timePartContentLength ) ) 
                                  : Math.Abs( newValue.Value.Seconds ).ToString( new string( '0', timePartContentLength ) );
                timePart.StartPosition = newTextBoxContent.Length;
                timePart.Length = secondText.Length;
                newTextBoxContent += secondText;
                break;
              case DateTimePart.Millisecond:
                var millisecondText = ( i <= 1 )
                                      ? Math.Truncate( Math.Abs( newValue.Value.TotalMilliseconds ) ).ToString( new string( '0', timePartContentLength ) ) 
                                      : Math.Abs( newValue.Value.Milliseconds ).ToString( new string( '0', timePartContentLength ) );
                timePart.StartPosition = newTextBoxContent.Length;
                timePart.Length = millisecondText.Length;
                newTextBoxContent += millisecondText;
                break;
              case DateTimePart.Other:
                var otherText = ( ( i == 0 ) && ( newValue.Value.TotalMilliseconds >= 0 ) ) ? "" : timePart.Content;
                timePart.StartPosition = newTextBoxContent.Length;
                timePart.Length = otherText.Length;
                newTextBoxContent += otherText;
                break;
            }
            if( ( _selectedDateTimeInfo != null ) && ( timePart.Type == _selectedDateTimeInfo.Type ) )
            {
              selectionLength = newTextBoxContent.Length - selectionStart;
            }
          }
          this.TextBox.Text = newTextBoxContent;
          this.TextBox.Select( selectionStart, selectionLength );
        }
      }
      else
      {
        if( this.Value.HasValue )
        {
          var newValue = this.UpdateTimeSpan( this.Value, step );
          if( newValue != null )
          {
            this.InitializeDateTimeInfoList( newValue );
            var selectionStart = this.TextBox.SelectionStart;
            var selectionLength = this.TextBox.SelectionLength;
            this.Value = newValue;
            this.TextBox.Select( selectionStart, selectionLength );
          }
        }
        else
        {
          this.Value = this.DefaultValue ?? TimeSpan.Zero;
        }
      }
    }

    private bool IsNumber( string str )
    {
      foreach( char c in str )
      {
        if( !char.IsNumber( c ) )
          return false;
      }

      return true;
    }

    private void UpdateValue()
    {
      var value = this.UpdateValueOnEnterKey
                  ? ( this.TextBox != null ) ? this.ConvertTextToValue( this.TextBox.Text ) : null
                  : this.Value;
      this.InitializeDateTimeInfoList( value );
      this.SyncTextAndValueProperties( false, this.Text );
    }

    private TimeSpan? ResetToLastValidValue()
    {
      // Reset DateTimeInfoList with last valid value.
      this.InitializeDateTimeInfoList( this.Value );
      return this.Value;
    }

    #endregion

    #region Event Handlers

    private void OnPasting( object sender, DataObjectPastingEventArgs e )
    {
      if( e.DataObject.GetDataPresent( typeof( string ) ) )
      {
        // Allow pasting only TimeSpan values
        var pasteText = e.DataObject.GetData( typeof( string ) ) as string;
        TimeSpan result;
        var success = TimeSpan.TryParse( pasteText, out result );
        if( !success )
        {
          e.CancelCommand();
        }
      }
    }

    #endregion
  }
}
