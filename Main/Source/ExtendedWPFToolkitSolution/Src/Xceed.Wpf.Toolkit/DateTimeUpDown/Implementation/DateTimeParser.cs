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

namespace Xceed.Wpf.Toolkit
{
  internal class DateTimeParser
  {
    public static bool TryParse( string value, string format, DateTime currentDate, CultureInfo cultureInfo, out DateTime result )
    {
      bool success = false;
      result = currentDate;

      if( string.IsNullOrEmpty( value ) || string.IsNullOrEmpty( format ) )
        return false;

      var dateTimeString = ComputeDateTimeString( value, format, currentDate, cultureInfo ).Trim();

      if( !String.IsNullOrEmpty( dateTimeString ) )
        success = DateTime.TryParse( dateTimeString, cultureInfo.DateTimeFormat, DateTimeStyles.None, out result );

      if( !success )
        result = currentDate;

      return success;
    }

    private static string ComputeDateTimeString( string dateTime, string format, DateTime currentDate, CultureInfo cultureInfo )
    {
      Dictionary<string, string> dateParts = GetDateParts( currentDate, cultureInfo );
      string[] timeParts = new string[ 3 ] { currentDate.Hour.ToString(), currentDate.Minute.ToString(), currentDate.Second.ToString() };
      string designator = "";
      string[] dateTimeSeparators = new string[] { ",", " ", "-", ".", "/", cultureInfo.DateTimeFormat.DateSeparator, cultureInfo.DateTimeFormat.TimeSeparator };

      UpdateSortableDateTimeString( ref dateTime, ref format, cultureInfo );

      var dateTimeParts = dateTime.Split( dateTimeSeparators, StringSplitOptions.RemoveEmptyEntries ).ToList();
      var formats = format.Split( dateTimeSeparators, StringSplitOptions.RemoveEmptyEntries ).ToList();

      //something went wrong
      if( dateTimeParts.Count != formats.Count )
        return string.Empty;

      for( int i = 0; i < formats.Count; i++ )
      {
        var f = formats[ i ];
        if( !f.Contains( "ddd" ) && !f.Contains( "GMT" ) )
        {
          if( f.Contains( "M" ) )
            dateParts[ "Month" ] = dateTimeParts[ i ];
          else if( f.Contains( "d" ) )
            dateParts[ "Day" ] = dateTimeParts[ i ];
          else if( f.Contains( "y" ) )
          {
            dateParts[ "Year" ] = dateTimeParts[ i ];

            if( dateParts[ "Year" ].Length == 2 )
              dateParts[ "Year" ] = string.Format( "{0}{1}", currentDate.Year / 100, dateParts[ "Year" ] );
          }
          else if( f.Contains( "h" ) || f.Contains( "H" ) )
            timeParts[ 0 ] = dateTimeParts[ i ];
          else if( f.Contains( "m" ) )
            timeParts[ 1 ] = dateTimeParts[ i ];
          else if( f.Contains( "s" ) )
            timeParts[ 2 ] = dateTimeParts[ i ];
          else if( f.Contains( "t" ) )
            designator = dateTimeParts[ i ];
        }
      }

      var date = string.Join( cultureInfo.DateTimeFormat.DateSeparator, dateParts.Select( x => x.Value ).ToArray() );
      var time = string.Join( cultureInfo.DateTimeFormat.TimeSeparator, timeParts );

      return String.Format( "{0} {1} {2}", date, time, designator );
    }

    private static void UpdateSortableDateTimeString( ref string dateTime, ref string format, CultureInfo cultureInfo )
    {
      if( format == cultureInfo.DateTimeFormat.SortableDateTimePattern )
      {
        format = format.Replace( "'", "" ).Replace( "T", " " );
        dateTime = dateTime.Replace( "'", "" ).Replace( "T", " " );
      }
      else if( format == cultureInfo.DateTimeFormat.UniversalSortableDateTimePattern )
      {
        format = format.Replace( "'", "" ).Replace( "Z", "" );
        dateTime = dateTime.Replace( "'", "" ).Replace( "Z", "" );
      }
    }

    private static Dictionary<string, string> GetDateParts( DateTime currentDate, CultureInfo cultureInfo )
    {
      Dictionary<string, string> dateParts = new Dictionary<string, string>();
      var dateFormatParts = cultureInfo.DateTimeFormat.ShortDatePattern.Split( new string[] { cultureInfo.DateTimeFormat.DateSeparator }, StringSplitOptions.RemoveEmptyEntries ).ToList();
      dateFormatParts.ForEach( item =>
      {
        string key = string.Empty;
        string value = string.Empty;

        if( item.Contains( "M" ) )
        {
          key = "Month";
          value = currentDate.Month.ToString();
        }
        else if( item.Contains( "d" ) )
        {
          key = "Day";
          value = currentDate.Day.ToString();
        }
        else if( item.Contains( "y" ) )
        {
          key = "Year";
          value = currentDate.Year.ToString();
        }
        dateParts.Add( key, value );
      } );
      return dateParts;
    }
  }
}
