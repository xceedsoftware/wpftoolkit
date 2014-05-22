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
using Xceed.Wpf.Toolkit.Core.Primitives;
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
    }

    public TimeSpanUpDown()
    {
    }

    #endregion //Constructors

    #region BaseClass Overrides

    protected override void OnCultureInfoChanged( CultureInfo oldValue, CultureInfo newValue )
    {
      this.InitializeDateTimeInfoList();
    }

    protected override void SetValidSpinDirection()
    {
      ValidSpinDirections validDirections = ValidSpinDirections.None;

      if( !this.IsReadOnly )
      {
        if( this.IsLowerThan( this.Value, this.Maximum ) || !this.Value.HasValue )
          validDirections = validDirections | ValidSpinDirections.Increase;

        if( this.IsGreaterThan( this.Value, this.Minimum ) || !this.Value.HasValue )
          validDirections = validDirections | ValidSpinDirections.Decrease;
      }

      if( this.Spinner != null )
        this.Spinner.ValidSpinDirection = validDirections;
    }

    protected override void OnIncrement()
    {
      if( this.Value.HasValue )
      {
        this.UpdateTimeSpan( 1 );
      }
      else
      {
        this.Value = this.DefaultValue ?? TimeSpan.Zero;
      }
    }

    protected override void OnDecrement()
    {
      if( this.Value.HasValue )
      {
        this.UpdateTimeSpan( -1 );
      }
      else
      {
        this.Value = this.DefaultValue ?? TimeSpan.Zero;
      }
    }

    protected override string ConvertValueToText()
    {
      if( this.Value == null )
        return string.Empty;

      return this.Value.Value.ToString();
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

    protected override void OnTextChanged( string previousValue, string currentValue )
    {
      if( !_processTextChanged )
        return;

      if( String.IsNullOrEmpty( currentValue ) )
      {
        this.Value = null;
        return;
      }

      TimeSpan current = this.Value.HasValue ? this.Value.Value : new TimeSpan();
      TimeSpan result;
      var success = TimeSpan.TryParse( currentValue, out result );
      currentValue = result.ToString();

      this.SyncTextAndValueProperties( true, currentValue );
    }

    protected override void OnValueChanged( TimeSpan? oldValue, TimeSpan? newValue )
    {
      //whenever the value changes we need to parse out the value into out DateTimeInfo segments so we can keep track of the individual pieces
      //but only if it is not null
      if( newValue != null )
      {
        this.ParseValueIntoTimeSpanInfo();
      }
      base.OnValueChanged( oldValue, newValue );
    }

    protected override void PerformMouseSelection()
    {
      this.InitializeDateTimeInfoList();
      base.PerformMouseSelection();
    }

    #endregion

    #region Methods

    protected override void InitializeDateTimeInfoList()
    {
      DateTimeInfo dayInfo = _dateTimeInfoList.FirstOrDefault( x => x.Type == DateTimePart.Day );
      bool hasDay = dayInfo != null;

      _dateTimeInfoList.Clear();

      if( this.Value.HasValue && this.Value.Value.Days != 0 )
      {
        int dayLength = this.Value.Value.Days.ToString().Length;
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Day, Length = dayLength, Format = "dd" } );
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ".", IsReadOnly = true } );

        // Day has been added, move TextBox.Selection to keep it on current DateTimeInfo
        if( !hasDay )
        {
          this.TextBox.SelectionStart += ( dayLength + 1 );
        }
      }
      // Day has been removed, move TextBox.Selection to keep it on current DateTimeInfo
      else if( hasDay )
      {
        this.TextBox.SelectionStart = Math.Max( 0, this.TextBox.SelectionStart - ( dayInfo.Length + 1 ) );
      }

      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Hour24, Length = 2, Format = "hh" } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ":", IsReadOnly = true } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Minute, Length = 2, Format = "mm" } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ":", IsReadOnly = true } );
      _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Second, Length = 2, Format = "ss" } );

      if( this.Value.HasValue && this.Value.Value.Milliseconds != 0 )
      {
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Other, Length = 1, Content = ".", IsReadOnly = true } );
        _dateTimeInfoList.Add( new DateTimeInfo() { Type = DateTimePart.Second, Length = 7, Format = "fffffff" } );
      }

      if( this.Value.HasValue )
      {
        this.ParseValueIntoTimeSpanInfo();
      }
    }

    protected override bool IsLowerThan( TimeSpan? value1, TimeSpan? value2 )
    {
      if( value1 == null || value2 == null )
        return false;

      return ( value1.Value < value2.Value );
    }

    protected override bool IsGreaterThan( TimeSpan? value1, TimeSpan? value2 )
    {
      if( value1 == null || value2 == null )
        return false;

      return ( value1.Value > value2.Value );
    }

    private void ParseValueIntoTimeSpanInfo()
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
          TimeSpan span = TimeSpan.Parse( this.Value.ToString() );
          info.StartPosition = text.Length;
          DateTime tempDate = new DateTime( span.Ticks );
          info.Content = tempDate.ToString( info.Format );
          if( info.Format == "dd" )
          {
            info.Content = Convert.ToInt32( info.Content ).ToString();
          }
          info.Length = info.Content.Length;
          text += info.Content;
        }
      } );
    }

    private void UpdateTimeSpan( int value )
    {
      _fireSelectionChangedEvent = false;
      DateTimeInfo info = _selectedDateTimeInfo;

      //this only occurs when the user manually type in a value for the Value Property
      if( info == null )
      {
        info = _dateTimeInfoList[ 0 ];
      }

      TimeSpan? result = null;

      try
      {
        switch( info.Type )
        {
          case DateTimePart.Day:
            result = ( ( TimeSpan )Value ).Add( new TimeSpan( value, 0, 0, 0, 0 ) );
            break;
          case DateTimePart.Hour24:
            result = ( ( TimeSpan )Value ).Add( new TimeSpan( 0, value, 0, 0, 0 ) );
            break;
          case DateTimePart.Minute:
            result = ( ( TimeSpan )Value ).Add( new TimeSpan( 0, 0, value, 0, 0 ) );
            break;
          case DateTimePart.Second:
            result = ( ( TimeSpan )Value ).Add( new TimeSpan( 0, 0, 0, value, 0 ) );
            break;
          case DateTimePart.Millisecond:
            result = ( ( TimeSpan )Value ).Add( new TimeSpan( 0, 0, 0, 0, value ) );
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
                ? result.Value.TotalMilliseconds > 0 ? result.Value : this.DefaultValue
                : result;

      this.Value = this.CoerceValueMinMax( result );

      //we loose our selection when the Value is set so we need to reselect it without firing the selection changed event
      this.TextBox.Select( info.StartPosition, info.Length );
      _fireSelectionChangedEvent = true;
    }

    #endregion
  }
}
