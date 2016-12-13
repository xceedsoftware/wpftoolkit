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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Primitives;

namespace Xceed.Wpf.Toolkit
{
  public class TimeSpanUpDown : DateTimeUpDownBase<TimeSpan?>
  {
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
      var value = this.UpdateValueOnEnterKey
                  ? (this.TextBox != null) ? this.ConvertTextToValue( this.TextBox.Text ) : null
                  : this.Value;
      this.InitializeDateTimeInfoList( value );
      this.SyncTextAndValueProperties( false, this.Text );
    }

    #endregion //FractionalSecondsDigitsCount

    #endregion

    #region BaseClass Overrides

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

      return this.ParseValueIntoTimeSpanInfo( this.Value );
    }

    protected override TimeSpan? ConvertTextToValue( string text )
    {
      if( string.IsNullOrEmpty( text ) )
        return null;

      TimeSpan timeSpan = TimeSpan.Parse( text );

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
      var success = TimeSpan.TryParse( currentValue, out result );
      currentValue = result.ToString();

      // When text is typed, if UpdateValueOnEnterKey is true, 
      // Sync Value on Text only when Enter Key is pressed.
      if( ( _isTextChangedFromUI && !this.UpdateValueOnEnterKey )
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
      var value = this.UpdateValueOnEnterKey
                  ? (this.TextBox != null) ? this.ConvertTextToValue( this.TextBox.Text ) : null
                  : this.Value;
      this.InitializeDateTimeInfoList( value );
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

      if( value.HasValue && value.Value.Days != 0 )
      {
        int dayLength = Math.Abs( value.Value.Days ).ToString().Length;
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Day, Length = dayLength, Format = "dd" } );
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ".", IsReadOnly = true } );

        if( this.TextBox != null )
        {
          //number of digits for days has changed when selection is not on date part, move TextBox.Selection to keep it on current DateTimeInfo
          if( hasDay && (dayLength != lastDayInfo.Length) && (_selectedDateTimeInfo.Type != DateTimePart.Day) )
          {
            _fireSelectionChangedEvent = false;
            this.TextBox.SelectionStart = Math.Max( 0, this.TextBox.SelectionStart + (dayLength - lastDayInfo.Length) );
            _fireSelectionChangedEvent = true;
          }
          // Day has been added, move TextBox.Selection to keep it on current DateTimeInfo
          else if( !hasDay )
          {
            _fireSelectionChangedEvent = false;
            this.TextBox.SelectionStart += (dayLength + 1);
            _fireSelectionChangedEvent = true;
          }
        }
      }
      // Day has been removed, move TextBox.Selection to keep it on current DateTimeInfo
      else if( hasDay )
      {
        _fireSelectionChangedEvent = false;
        this.TextBox.SelectionStart = Math.Max( hasNegative ? 1 : 0, this.TextBox.SelectionStart - (lastDayInfo.Length + 1) );
        _fireSelectionChangedEvent = true;
      }

      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Hour24, Length = 2, Format = "hh" } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ":", IsReadOnly = true } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Minute, Length = 2, Format = "mm" } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ":", IsReadOnly = true } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Second, Length = 2, Format = "ss" } );

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
        this.ParseValueIntoTimeSpanInfo( value );
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

    #endregion

    #region Methods

    private string ParseValueIntoTimeSpanInfo( TimeSpan? value )
    {
      string text = string.Empty;

      _dateTimeInfoList.ForEach( info =>
      {
        if( info.Format == null )
        {
          info.StartPosition = text.Length;
          info.Length = info.Content.Length;
          text += info.Content;
        }
        else
        {
          TimeSpan span = TimeSpan.Parse( value.ToString() );
          info.StartPosition = text.Length;
#if VS2008
          switch (info.Format)
          {
              case "hh":
                  info.Content = span.Hours.ToString("00");
                  break;
              case "mm":
                  info.Content = span.Minutes.ToString("00");
                  break;
              case "ss":
                  info.Content = span.Seconds.ToString("00");
                  break;
              case "dd":
                  info.Content = span.Days.ToString();
                  break;
              case "%f":
                  info.Content = (span.Milliseconds / 100).ToString();
                  break;
              case "ff":
                  info.Content = (span.Milliseconds / 10).ToString();
                  break;
              case "fff":
                  info.Content = span.Milliseconds.ToString();
                  break;
              default:
                  throw new InvalidOperationException("Wrong TimeSpan format");
          }
#else
          info.Content = span.ToString( info.Format, this.CultureInfo.DateTimeFormat );
#endif
          if( info.Format == "dd" )
          {
            info.Content = Convert.ToInt32( info.Content ).ToString();
          }
          info.Length = info.Content.Length;
          text += info.Content;
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
        var currentValue = this.ConvertTextToValue( this.TextBox.Text );
        var newValue = currentValue.HasValue
                       ? this.UpdateTimeSpan( currentValue, step )
                       : this.DefaultValue ?? TimeSpan.Zero;

        if( newValue != null )
        {
          this.InitializeDateTimeInfoList( newValue );
          var selectionStart = this.TextBox.SelectionStart;
          var selectionLength = this.TextBox.SelectionLength;
#if VS2008
          this.TextBox.Text = newValue.Value.ToString();
#else
          this.TextBox.Text = newValue.Value.ToString( this.GetTimeSpanFormat() );
#endif
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

#if !VS2008
    private string GetTimeSpanFormat()
    {
      //use this format : "d.hh\:mm\:ss\.fff"
      var formatParts = _dateTimeInfoList.Select( part =>
      {
        if( part.Format == null )
          return '\\' + part.Content;

        if( part.Format.Contains( 'd' ) )
        {
          var dayInfo = _dateTimeInfoList.FirstOrDefault( info => info.Type == DateTimePart.Day );
          part.Format = string.Join( "", Enumerable.Repeat( "d", dayInfo.Length ) );
        }
        return part.Format;
      } ).ToList();

      return string.Join( "", formatParts );
    }
#endif

    private bool IsNumber( string str )
    {
      foreach( char c in str )
      {
        if( !char.IsNumber( c ) )
          return false;
      }

      return true;
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
