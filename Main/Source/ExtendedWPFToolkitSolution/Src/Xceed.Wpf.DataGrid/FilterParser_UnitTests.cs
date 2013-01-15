/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

#if DEBUG

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using Xceed.Wpf.DataGrid.FilterCriteria;

namespace Xceed.Wpf.DataGrid
{
  internal static partial class FilterParser
  {
    internal static Action<string> LogMessageCallback
    {
      get
      {
        return s_logMessageCallback;
      }

      set
      {
        if( ( ( value == null ) && ( s_logMessageCallback != null ) ) ||
            ( ( value != null ) && ( s_logMessageCallback == null ) ) )
        {
          s_logMessageCallback = value;
        }
        else
        {
          throw new InvalidOperationException( "Unit test already running." );
        }
      }
    }

    internal static void TestQuoteParser()
    {
      FilterParser.LogMessageCallback( "Testing quote parser..." );

      FilterParser.CheckQuoteParserResult( "", new RawToken( "" ) );
      FilterParser.CheckQuoteParserResult( "test", new RawToken( "test" ) );
      FilterParser.CheckQuoteParserResult( "test test", new RawToken( "test test" ) );
      FilterParser.CheckQuoteParserResult( "\"test\"", new AtomicStringToken( "test" ) );
      FilterParser.CheckQuoteParserResult( "L'idéal du calme \"est dans\" un \"chat assis\"",
        new RawToken( "L'idéal du calme " ),
        new AtomicStringToken( "est dans" ),
        new RawToken( " un " ),
        new AtomicStringToken( "chat assis" ) );


      FilterParser.CheckQuoteParserResult( "\"\"\"L'idéal du calme est dans un chat assis\"\"\"",
        new AtomicStringToken( "\"L'idéal du calme est dans un chat assis\"" ) );


      FilterParser.CheckQuoteParserResult( "L'idéal du calme \"est dans\" un \"\"\"chat assis\"",
        new RawToken( "L'idéal du calme " ),
        new AtomicStringToken( "est dans" ),
        new RawToken( " un " ),
        new AtomicStringToken( "\"chat assis" ) );

      FilterParser.CheckQuoteParserResult( "L'idéal du calme \"est dans\" un \"\"\"chat assis\"\"\"",
        new RawToken( "L'idéal du calme " ),
        new AtomicStringToken( "est dans" ),
        new RawToken( " un " ),
        new AtomicStringToken( "\"chat assis\"" ) );
      FilterParser.CheckQuoteParserResult( "\"\"", new AtomicStringToken( "" ) );
      FilterParser.CheckQuoteParserResult( "\"\"\"\"\"\"", new AtomicStringToken( "\"\"" ) );
      FilterParser.CheckQuoteParserResult( "\"\"\"\"\"\"\"\"", new AtomicStringToken( "\"\"\"" ) );
      FilterParser.CheckQuoteParserResult( "\" \"\"\"\" \"", new AtomicStringToken( " \"\" " ) );
      FilterParser.CheckQuoteParserResult( "\"\"\"", FilterParser.MissingClosingQuotesErrorText );
      FilterParser.CheckQuoteParserResult( "\"\"L'idéal du calme est dans un chat assis\"\"", new AtomicStringToken( "" ),
      new RawToken( "L'idéal du calme est dans un chat assis" ), new AtomicStringToken( "" ) );
      FilterParser.CheckQuoteParserResult( "\"\"L'idéal du calme est dans un chat assis", new AtomicStringToken( "" ),
        new RawToken( "L'idéal du calme est dans un chat assis" ) );
      FilterParser.CheckQuoteParserResult( "\"\"\"L'idéal du calme est dans un chat assis\"", new AtomicStringToken( "\"L'idéal du calme est dans un chat assis" ) );
      FilterParser.CheckQuoteParserResult( "\"\"\"\"L'idéal du calme est dans un chat assis\"", FilterParser.MissingClosingQuotesErrorText );

    }

    internal static void TestCriterionBuilder()
    {
      FilterParser.TestCriterionBuilderString();
      FilterParser.TestCriterionBuilderChar();
      FilterParser.TestCriterionBuilderDateTime();
      FilterParser.TestCriterionBuilderNumber();
    }

    private static void TestCriterionBuilderString()
    {
      Type type = typeof( string );
      FilterParser.LogMessageCallback( "Testing string filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        new ContainsFilterCriterion( "" ) );

      FilterParser.CheckCriterionBuilderResult( "=\"\"", type, null,
        new EqualToFilterCriterion( "" ) );

      FilterParser.CheckCriterionBuilderResult( "*\"\"", type, null, new EndsWithFilterCriterion( "" ) );

      FilterParser.CheckCriterionBuilderResult( "\"\"*", type, null, new StartsWithFilterCriterion( "" ) );

      FilterParser.CheckCriterionBuilderResult( "*\"Test\"", type, null, new EndsWithFilterCriterion( "Test" ) );

      FilterParser.CheckCriterionBuilderResult( "Test", type, null,
        new ContainsFilterCriterion( "Test" ) );

      FilterParser.CheckCriterionBuilderResult( "Not Test", type, null,
        new ContainsFilterCriterion( "Not Test" ) );

      FilterParser.CheckCriterionBuilderResult( "NOT", type, null,
        new ContainsFilterCriterion( "NOT" ) );

      FilterParser.CheckCriterionBuilderResult( "NOT ", type, null,
        new ContainsFilterCriterion( "NOT" ) );

      FilterParser.CheckCriterionBuilderResult( "AND ", type, null,
        new ContainsFilterCriterion( "AND" ) );

      FilterParser.CheckCriterionBuilderResult( "   AND    ", type, null,
        new ContainsFilterCriterion( "AND" ) );

      FilterParser.CheckCriterionBuilderResult( "OR OR OR OR", type, null,
        new OrFilterCriterion(
          new ContainsFilterCriterion( "OR" ),
          new ContainsFilterCriterion( "OR OR" ) ) );

      FilterParser.CheckCriterionBuilderResult( "NOT OR NOT", type, null,
        new OrFilterCriterion(
          new ContainsFilterCriterion( "NOT" ),
          new ContainsFilterCriterion( "NOT" ) ) );

      FilterParser.CheckCriterionBuilderResult( "NOT Test", type, null,
        new NotFilterCriterion( new ContainsFilterCriterion( "Test" ) ) );

      FilterParser.CheckCriterionBuilderResult( "<10 OR >50", type, null,
        new OrFilterCriterion(
          new LessThanFilterCriterion( "10" ),
          new GreaterThanFilterCriterion( "50" ) ) );

      FilterParser.CheckCriterionBuilderResult( ">=fou AND <gra", type, null,
        new AndFilterCriterion(
          new GreaterThanOrEqualToFilterCriterion( "fou" ),
          new LessThanFilterCriterion( "gra" ) ) );

      FilterParser.CheckCriterionBuilderResult( "NOT =10", type, null,
        new NotFilterCriterion(
          new EqualToFilterCriterion( "10" ) ) );

      FilterParser.CheckCriterionBuilderResult( "AND AND >10 ", type, null,
        new AndFilterCriterion(
          new ContainsFilterCriterion( "AND" ),
          new GreaterThanFilterCriterion( "10" ) ) );

      FilterParser.CheckCriterionBuilderResult( "OR AND 10 ", type, null,
        new AndFilterCriterion(
          new ContainsFilterCriterion( "OR" ),
          new ContainsFilterCriterion( "10" ) ) );

      FilterParser.CheckCriterionBuilderResult( "AND OR =10 ", type, null,
        new OrFilterCriterion(
          new ContainsFilterCriterion( "AND" ),
          new EqualToFilterCriterion( "10" ) ) );

      FilterParser.CheckCriterionBuilderResult( "\"=10 AND 10 OR 10\"", type, null,
        new ContainsFilterCriterion( "=10 AND 10 OR 10" ) );

      FilterParser.CheckCriterionBuilderResult( "=\"10 AND 10 OR 10\"", type, null,
        new EqualToFilterCriterion( "10 AND 10 OR 10" ) );

      FilterParser.CheckCriterionBuilderResult( "=10   AND   \"10 OR 10\"", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( "10" ),
          new ContainsFilterCriterion( "10 OR 10" ) ) );

      FilterParser.CheckCriterionBuilderResult( "*\"Test\" AND \"\"\"Test2\"*", type, null, new AndFilterCriterion( new EndsWithFilterCriterion( "Test" ), new StartsWithFilterCriterion( "\"Test2" ) ) );

      FilterParser.CheckCriterionBuilderResult( "*\"Test\" AND \"Test\"\"2\"\"\"*", type, null, new AndFilterCriterion( new EndsWithFilterCriterion( "Test" ), new StartsWithFilterCriterion( "Test\"2\"" ) ) );

      FilterParser.CheckCriterionBuilderResult( "*Test AND \"Test\"\"2\"\"\"*", type, null, new AndFilterCriterion( new EndsWithFilterCriterion( "Test" ), new StartsWithFilterCriterion( "Test\"2\"" ) ) );

      FilterParser.CheckCriterionBuilderResult( "*\" Test\" AND \"Test\"\"2\"\"\"*", type, null, new AndFilterCriterion( new EndsWithFilterCriterion( " Test" ), new StartsWithFilterCriterion( "Test\"2\"" ) ) );
      FilterParser.CheckCriterionBuilderResult( "=10 AND \"10   OR 10\"", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( "10" ),
          new ContainsFilterCriterion( "10   OR 10" ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new ContainsFilterCriterion( "NULL" ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new ContainsFilterCriterion( "NULL" ) );

      FilterParser.CheckCriterionBuilderResult( "=NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "=\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "=Null", type, null,
        new EqualToFilterCriterion( "Null" ) );

      FilterParser.CheckCriterionBuilderResult( "\"=10 AND <5 OR NOT >2\"", type, null,
        new ContainsFilterCriterion( "=10 AND <5 OR NOT >2" ) );

      FilterParser.CheckCriterionBuilderResult( "=NULL OR =5", type, null, 
        new OrFilterCriterion( 
          new EqualToFilterCriterion( null ), 
          new EqualToFilterCriterion( "5" ) ) );

      FilterParser.CheckCriterionBuilderResult( "  =10   AND   >5  OR 15  ", type, null,
        new OrFilterCriterion(
          new AndFilterCriterion(
            new EqualToFilterCriterion( "10" ),
            new GreaterThanFilterCriterion( "5" ) ),
          new ContainsFilterCriterion( "15" ) ) );

      FilterParser.CheckCriterionBuilderResult( "=NOT 10", type, null,
        new EqualToFilterCriterion( "NOT 10" ) );

      FilterParser.CheckCriterionBuilderResult( "10 =20", type, null,
        new ContainsFilterCriterion( "10 =20" ) );

      FilterParser.CheckCriterionBuilderResult( "<ab >cd", type, null,
        new LessThanFilterCriterion( "ab >cd" ) );

      FilterParser.CheckCriterionBuilderResult( "= chat", type, null,
        new EqualToFilterCriterion( "chat" ) );

      FilterParser.CheckCriterionBuilderResult( "= chat   OR =chien  ", type, null,
        new OrFilterCriterion(
          new EqualToFilterCriterion( "chat" ),
          new EqualToFilterCriterion( "chien" ) ) );

      FilterParser.CheckCriterionBuilderResult( "=chat  chien   OR =lion", type, null,
        new OrFilterCriterion(
          new EqualToFilterCriterion( "chat  chien" ),
          new EqualToFilterCriterion( "lion" ) ) );

      FilterParser.CheckCriterionBuilderResult( "=chat =chien", type, null,
        new EqualToFilterCriterion( "chat =chien" ) );

      FilterParser.CheckCriterionBuilderResult( "*chien", type, null, new EndsWithFilterCriterion( "chien" ) );
      FilterParser.CheckCriterionBuilderResult( "*chien OR chat*", type, null, new OrFilterCriterion( new EndsWithFilterCriterion( "chien" ), new StartsWithFilterCriterion( "chat" ) ) );
      FilterParser.CheckCriterionBuilderResult( "*\"chien\" OR \"chat\"*", type, null, new OrFilterCriterion( new EndsWithFilterCriterion( "chien" ), new StartsWithFilterCriterion( "chat" ) ) );
      FilterParser.CheckCriterionBuilderResult( "\"*chien\" OR \"chat\"*", type, null, new OrFilterCriterion( new ContainsFilterCriterion( "*chien" ), new StartsWithFilterCriterion( "chat" ) ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "=10   \"AND 10 OR 10\"", type, null,
        FilterParser.InvalidExpressionErrorText );

      FilterParser.CheckCriterionBuilderResult( "Johann  \"Sebastian Bach\"", type, null,
        FilterParser.InvalidExpressionErrorText );

      FilterParser.CheckCriterionBuilderResult( "  \"Johann Sebastian\"    Bach  ", type, null,
        FilterParser.InvalidExpressionErrorText );

      FilterParser.CheckCriterionBuilderResult( "\"University Southern\" \"North Dakota\"", type, null,
        FilterParser.InvalidExpressionErrorText );


    }

    private static void TestCriterionBuilderChar()
    {
      Type type = typeof( char );
      FilterParser.LogMessageCallback( "Testing char filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "a", type, null,
        new EqualToFilterCriterion( 'a' ) );

      FilterParser.CheckCriterionBuilderResult( "\" \"", type, null,
        new EqualToFilterCriterion( ' ' ) );

      FilterParser.CheckCriterionBuilderResult( "\u03a0", type, null,
        new EqualToFilterCriterion( '\u03a0' ) ); // Pi

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( " ", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "abc", type, null,
        string.Format( FilterParser.InvalidCharValueErrorText, "abc" ) );

      FilterParser.CheckCriterionBuilderResult( "\u03a0\u03a0", type, null,
        string.Format( FilterParser.InvalidCharValueErrorText, "\u03a0\u03a0" ) );// PiPi

      FilterParser.CheckCriterionBuilderResult( "=Null", type, null,
        string.Format( FilterParser.InvalidCharValueErrorText, "Null" ) );
    }

    private static void TestCriterionBuilderDateTime()
    {
      Type type = typeof( DateTime );
      FilterParser.LogMessageCallback( "Testing DateTime filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0001-01-01", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( DateTime.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "0104-02-29", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 104, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "0204-02-29", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 204, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "02-29-0304", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 304, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "2.29.0400", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 400, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "0504-02-29T00:00", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 504, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "0604-02-29T00:00:00", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 604, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "0704-02-29T00:00:00.000", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 704, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "0800-02-29T23:59:59.999", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 800, 2, 29, 23, 59, 59, 999 ) ) );

      FilterParser.CheckCriterionBuilderResult( "\"02-29-904 23:59:59.999\"", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 904, 2, 29, 23, 59, 59, 999 ) ) );

      FilterParser.CheckCriterionBuilderResult( "\"29.2.1004 23:59:59.999\"", type, IsIsCulture,
        new EqualToFilterCriterion( new DateTime( 1004, 2, 29, 23, 59, 59, 999 ) ) );

      FilterParser.CheckCriterionBuilderResult( "29-Feb-1104", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 1104, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "February/29/1200", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 1200, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "29-février-1304", type, FrFrCulture,
        new EqualToFilterCriterion( new DateTime( 1304, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "29.Ebrel.1404", type, BrFrCulture,
        new EqualToFilterCriterion( new DateTime( 1404, 4, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "29-02-1504", type, FrFrCulture,
        new EqualToFilterCriterion( new DateTime( 1504, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "février-29-1600", type, FrFrCulture,
        new EqualToFilterCriterion( new DateTime( 1600, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "29-february-1704", type, FrFrCulture,
        new EqualToFilterCriterion( new DateTime( 1704, 2, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "29.april.1804", type, BrFrCulture,
        new EqualToFilterCriterion( new DateTime( 1804, 4, 29 ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "02-29-1904 23:59:59.999", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( new DateTime( 1904, 2, 29, 23, 59, 59, 999 ) ) );

      FilterParser.CheckCriterionBuilderResult( "02-29-1904 23:59:59.999 OR 02-29-1908 23:59:59.999", type, CultureInfo.InvariantCulture,
        new OrFilterCriterion(
          new EqualToFilterCriterion( new DateTime( 1904, 2, 29, 23, 59, 59, 999 ) ),
          new EqualToFilterCriterion( new DateTime( 1908, 2, 29, 23, 59, 59, 999 ) ) ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "`0100-02-29", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "0200-01-29T10", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "0300-01-29T24:00", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "0400-01-29T23:60", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "0500-01-29T23:59:60", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "01-avril-0600", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "01/avril/0700", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "01.avril.0904", type, BrFrCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "01-15-1004", type, FrFrCulture,
        FilterParser.InvalidDateTimeValueErrorText );

      FilterParser.CheckCriterionBuilderResult( "=Null", type, null,
        FilterParser.InvalidDateTimeValueErrorText );
    }

    private static void TestCriterionBuilderNumber()
    {
      FilterParser.TestCriterionBuilderByte();
      FilterParser.TestCriterionBuilderInt16();
      FilterParser.TestCriterionBuilderInt32();
      FilterParser.TestCriterionBuilderInt64();
      FilterParser.TestCriterionBuilderSingle();
      FilterParser.TestCriterionBuilderDouble();
      FilterParser.TestCriterionBuilderDecimal();
      FilterParser.TestCriterionBuilderSByte();
      FilterParser.TestCriterionBuilderUInt16();
      FilterParser.TestCriterionBuilderUInt32();
      FilterParser.TestCriterionBuilderUInt64();
    }

    #region Number test methods

    private static void TestCriterionBuilderByte()
    {
      Type type = typeof( byte );
      FilterParser.LogMessageCallback( "Testing byte filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( byte.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "255", type, null,
        new EqualToFilterCriterion( byte.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=0", type, null,
        new GreaterThanOrEqualToFilterCriterion( byte.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "<255", type, null,
        new LessThanFilterCriterion( byte.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( "  0   OR  255  ", type, null,
        new OrFilterCriterion(
          new EqualToFilterCriterion( ( byte )0 ),
          new EqualToFilterCriterion( byte.MaxValue ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "1,1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-1", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "256", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-1", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<=256", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "10 3", type, null,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "10 AND 300", type, null,
        FilterParser.NumberOverflowErrorText );
    }

    private static void TestCriterionBuilderInt16()
    {
      Type type = typeof( short );
      FilterParser.LogMessageCallback( "Testing Int16 filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( ( short )0 ) );

      FilterParser.CheckCriterionBuilderResult( "-32768", type, null,
        new EqualToFilterCriterion( short.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "32767", type, null,
        new EqualToFilterCriterion( short.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=-32768", type, null,
        new GreaterThanOrEqualToFilterCriterion( short.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "<>32767", type, null,
        new DifferentThanFilterCriterion( short.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( "-309 AND 4555", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( ( short )-309 ),
          new EqualToFilterCriterion( ( short )4555 ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 AND >000", type, null,
        new AndFilterCriterion(
          new LessThanFilterCriterion( ( short )0 ),
          new GreaterThanFilterCriterion( ( short )0 ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "1,1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-32769", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "32768", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<>-32769", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<=32768", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "10,000", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "10 000", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "139 AND 40000", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderInt32()
    {
      Type type = typeof( int );
      FilterParser.LogMessageCallback( "Testing Int32 filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( 0 ) );

      FilterParser.CheckCriterionBuilderResult( "-2147483648", type, null,
        new EqualToFilterCriterion( int.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "2147483647", type, null,
        new EqualToFilterCriterion( int.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=-2147483648", type, null,
        new GreaterThanOrEqualToFilterCriterion( int.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "<2147483647", type, null,
        new LessThanFilterCriterion( int.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( "-564654 AND 71207", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( -564654 ),
          new EqualToFilterCriterion( 71207 ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 OR >00000000000000000", type, null,
        new OrFilterCriterion(
          new LessThanFilterCriterion( 0 ),
          new GreaterThanFilterCriterion( 0 ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "1,1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-2147483649", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "2147483648", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-2147483649", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<2147483648", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "483 648 457", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "483,648,457", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "30 AND 21474836480", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderInt64()
    {
      Type type = typeof( long );
      FilterParser.LogMessageCallback( "Testing Int64 filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( 0L ) );

      FilterParser.CheckCriterionBuilderResult( "-9223372036854775808", type, null,
        new EqualToFilterCriterion( long.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "9223372036854775807", type, null,
        new EqualToFilterCriterion( long.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=-9223372036854775808", type, null,
        new GreaterThanOrEqualToFilterCriterion( long.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "<9223372036854775807", type, null,
        new LessThanFilterCriterion( long.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( "-4199873364 AND 127915466", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( -4199873364L ),
          new EqualToFilterCriterion( 127915466L ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 OR >00000000000000000", type, null,
        new OrFilterCriterion(
          new LessThanFilterCriterion( 0L ),
          new GreaterThanFilterCriterion( 0L ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "1,1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-9223372036854775809", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "9223372036854775808", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-9223372036854775809", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<9223372036854775808", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "223 372 036 854 775 807", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "223,372,036,854,775,807", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "10 AND 92233720368547758080", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderSingle()
    {
      Type type = typeof( float );
      FilterParser.LogMessageCallback( "Testing Single filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( 0F ) );

      FilterParser.CheckCriterionBuilderResult( "1E-45", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( 1E-45F ) );

      FilterParser.CheckCriterionBuilderResult( "-1E-45", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( -1E-45F ) );

      FilterParser.CheckCriterionBuilderResult( "3E38 ", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( 3E38F ) );

      FilterParser.CheckCriterionBuilderResult( "-3E38 ", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( -3E38F ) );

      FilterParser.CheckCriterionBuilderResult( ">=-3829.5000", type, CultureInfo.InvariantCulture,
        new GreaterThanOrEqualToFilterCriterion( -3829.5F ) );

      FilterParser.CheckCriterionBuilderResult( "<>11,687.145", type, CultureInfo.InvariantCulture,
        new DifferentThanFilterCriterion( 11687.145F ) );

      FilterParser.CheckCriterionBuilderResult( "<1687,145", type, FrFrCulture,
        new LessThanFilterCriterion( 1687.145F ) );

      FilterParser.CheckCriterionBuilderResult( "<>\"11 687,145\"", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687.145F ) );

      FilterParser.CheckCriterionBuilderResult( "<>11 687,145", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687.145F ) );

      FilterParser.CheckCriterionBuilderResult( "<>11 687", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687F ) );

      FilterParser.CheckCriterionBuilderResult( "<92   233,75", type, FrFrCulture,
        new LessThanFilterCriterion( 92233.75F ) );

      FilterParser.CheckCriterionBuilderResult( ">11 687,145 AND < 12 384,55", type, FrFrCulture,
        new AndFilterCriterion(
          new GreaterThanFilterCriterion( 11687.145F ),
          new LessThanFilterCriterion( 12384.55F ) ) );

      FilterParser.CheckCriterionBuilderResult( "0.0 AND 3.10", type, CultureInfo.InvariantCulture,
        new AndFilterCriterion(
          new EqualToFilterCriterion( 0F ),
          new EqualToFilterCriterion( 3.1F ) ) );

      FilterParser.CheckCriterionBuilderResult( " >  11,687.145  AND <  12,384.55", type, CultureInfo.InvariantCulture,
        new AndFilterCriterion(
          new GreaterThanFilterCriterion( 11687.145F ),
          new LessThanFilterCriterion( 12384.55F ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 OR >000000,00000000000", type, FrFrCulture,
        new OrFilterCriterion(
          new LessThanFilterCriterion( 0F ),
          new GreaterThanFilterCriterion( 0F ) ) );

      FilterParser.CheckCriterionBuilderResult( ">-Infini", type, FrFrCulture,
        new GreaterThanFilterCriterion( float.NegativeInfinity ) );

      FilterParser.CheckCriterionBuilderResult( "<>-Infinity", type, CultureInfo.InvariantCulture,
        new DifferentThanFilterCriterion( float.NegativeInfinity ) );

      FilterParser.CheckCriterionBuilderResult( "<+Infini", type, FrFrCulture,
        new LessThanFilterCriterion( float.PositiveInfinity ) );

      FilterParser.CheckCriterionBuilderResult( "NOT =Infinity", type, CultureInfo.InvariantCulture,
        new NotFilterCriterion(
          new EqualToFilterCriterion( float.PositiveInfinity ) ) );

      FilterParser.CheckCriterionBuilderResult( "\"Non Numérique\"", type, FrFrCulture,
        new EqualToFilterCriterion( float.NaN ) );

      FilterParser.CheckCriterionBuilderResult( "<>NaN", type, CultureInfo.InvariantCulture,
        new DifferentThanFilterCriterion( float.NaN ) );

      // Seems like a bug in the framework number parser. Everything too small will 
      // convert to 0. There doesn't seem to be a limit in the smallness.
      FilterParser.CheckCriterionBuilderResult( "-1E-46", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( 0F ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<92 233.72", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "26681 5989 92,233.72", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "0.0 3.10", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "14.545,1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-\"92 233,72\"", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<\"92 233.72\"", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "4E38", type, CultureInfo.InvariantCulture,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderDouble()
    {
      Type type = typeof( double );
      FilterParser.LogMessageCallback( "Testing Double filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( 0D ) );

      FilterParser.CheckCriterionBuilderResult( "5E-324", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( 5E-324 ) );

      FilterParser.CheckCriterionBuilderResult( "-5E-324", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( -5E-324 ) );

      FilterParser.CheckCriterionBuilderResult( "1E308 ", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( 1E308 ) );

      FilterParser.CheckCriterionBuilderResult( "-1E308 ", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( -1E308 ) );

      FilterParser.CheckCriterionBuilderResult( ">=-3829.5000", type, CultureInfo.InvariantCulture,
        new GreaterThanOrEqualToFilterCriterion( -3829.5 ) );

      FilterParser.CheckCriterionBuilderResult( "<>11,687.145", type, CultureInfo.InvariantCulture,
        new DifferentThanFilterCriterion( 11687.145 ) );

      FilterParser.CheckCriterionBuilderResult( "<1687,145", type, FrFrCulture,
        new LessThanFilterCriterion( 1687.145 ) );

      FilterParser.CheckCriterionBuilderResult( "<>\"11 687,145\"", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687.145 ) );

      FilterParser.CheckCriterionBuilderResult( "<>11 687,145", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687.145 ) );

      FilterParser.CheckCriterionBuilderResult( "<>11 687", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687.0 ) );

      FilterParser.CheckCriterionBuilderResult( "<92   233,75", type, FrFrCulture,
        new LessThanFilterCriterion( 92233.75 ) );

      FilterParser.CheckCriterionBuilderResult( ">11 687,145 AND < 12 384,55", type, FrFrCulture,
        new AndFilterCriterion(
          new GreaterThanFilterCriterion( 11687.145 ),
          new LessThanFilterCriterion( 12384.55 ) ) );

      FilterParser.CheckCriterionBuilderResult( "0.0 AND 3.10", type, CultureInfo.InvariantCulture,
        new AndFilterCriterion(
          new EqualToFilterCriterion( 0D ),
          new EqualToFilterCriterion( 3.1 ) ) );

      FilterParser.CheckCriterionBuilderResult( " >  11,687.145  AND <  12,384.55", type, CultureInfo.InvariantCulture,
        new AndFilterCriterion(
          new GreaterThanFilterCriterion( 11687.145 ),
          new LessThanFilterCriterion( 12384.55 ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 OR >000000,00000000000", type, FrFrCulture,
        new OrFilterCriterion(
          new LessThanFilterCriterion( 0D ),
          new GreaterThanFilterCriterion( 0D ) ) );

      FilterParser.CheckCriterionBuilderResult( ">-Infini", type, FrFrCulture,
        new GreaterThanFilterCriterion( double.NegativeInfinity ) );

      FilterParser.CheckCriterionBuilderResult( "<>-Infinity", type, CultureInfo.InvariantCulture,
        new DifferentThanFilterCriterion( double.NegativeInfinity ) );

      FilterParser.CheckCriterionBuilderResult( "<+Infini", type, FrFrCulture,
        new LessThanFilterCriterion( double.PositiveInfinity ) );

      FilterParser.CheckCriterionBuilderResult( "NOT =Infinity", type, CultureInfo.InvariantCulture,
        new NotFilterCriterion(
          new EqualToFilterCriterion( double.PositiveInfinity ) ) );

      FilterParser.CheckCriterionBuilderResult( "\"Non Numérique\"", type, FrFrCulture,
        new EqualToFilterCriterion( double.NaN ) );

      FilterParser.CheckCriterionBuilderResult( "<>NaN", type, CultureInfo.InvariantCulture,
        new DifferentThanFilterCriterion( double.NaN ) );

      // Seems like a bug in the framework number parser. Everything too small will 
      // convert to 0. There doesn't seem to be a limit in the smallness.
      FilterParser.CheckCriterionBuilderResult( "2E-325", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( 0D ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<\"92 233.72\"", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<92 233.72", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "26681 5989 92,233.72", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "0.0 3.10", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "14.545,1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-\"92 233,72\"", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<\"92 233.72\"", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "2E308", type, CultureInfo.InvariantCulture,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderDecimal()
    {
      Type type = typeof( decimal );
      FilterParser.LogMessageCallback( "Testing Decimal filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( 0M ) );

      FilterParser.CheckCriterionBuilderResult( "-79228162514264337593543950335", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( decimal.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "79228162514264337593543950335 ", type, CultureInfo.InvariantCulture,
        new EqualToFilterCriterion( decimal.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=-3829.5000", type, CultureInfo.InvariantCulture,
        new GreaterThanOrEqualToFilterCriterion( -3829.5M ) );

      FilterParser.CheckCriterionBuilderResult( "<>11,687.145", type, CultureInfo.InvariantCulture,
        new DifferentThanFilterCriterion( 11687.145M ) );

      FilterParser.CheckCriterionBuilderResult( "<1687,145", type, FrFrCulture,
        new LessThanFilterCriterion( 1687.145M ) );

      FilterParser.CheckCriterionBuilderResult( "<>\"11 687,145\"", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687.145M ) );

      FilterParser.CheckCriterionBuilderResult( "<>11 687,145", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687.145M ) );

      FilterParser.CheckCriterionBuilderResult( "<>11 687", type, FrFrCulture,
        new DifferentThanFilterCriterion( 11687M ) );

      FilterParser.CheckCriterionBuilderResult( "<92   233,75", type, FrFrCulture,
        new LessThanFilterCriterion( 92233.75M ) );

      FilterParser.CheckCriterionBuilderResult( ">11 687,145 AND < 12 384,55", type, FrFrCulture,
        new AndFilterCriterion(
          new GreaterThanFilterCriterion( 11687.145M ),
          new LessThanFilterCriterion( 12384.55M ) ) );

      FilterParser.CheckCriterionBuilderResult( "0.0 AND 3.10", type, CultureInfo.InvariantCulture,
        new AndFilterCriterion(
          new EqualToFilterCriterion( 0M ),
          new EqualToFilterCriterion( 3.1M ) ) );

      FilterParser.CheckCriterionBuilderResult( " >  11,687.145  AND <  12,384.55", type, CultureInfo.InvariantCulture,
        new AndFilterCriterion(
          new GreaterThanFilterCriterion( 11687.145M ),
          new LessThanFilterCriterion( 12384.55M ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 OR >000000,00000000000", type, FrFrCulture,
        new OrFilterCriterion(
          new LessThanFilterCriterion( 0M ),
          new GreaterThanFilterCriterion( 0M ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<\"92 233.72\"", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<92 233.72", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "26681 5989 92,233.72", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "0.0 3.10", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "14.545,1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-\"92 233,72\"", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<\"92 233.72\"", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-79228162514264337593543950336", type, CultureInfo.InvariantCulture,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "79228162514264337593543950336", type, CultureInfo.InvariantCulture,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderSByte()
    {
      Type type = typeof( sbyte );
      FilterParser.LogMessageCallback( "Testing SByte filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( ( sbyte )0 ) );

      FilterParser.CheckCriterionBuilderResult( "-128", type, null,
        new EqualToFilterCriterion( sbyte.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "127", type, null,
        new EqualToFilterCriterion( sbyte.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=-128", type, null,
        new GreaterThanOrEqualToFilterCriterion( sbyte.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "<127", type, null,
        new LessThanFilterCriterion( sbyte.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( "10 AND 20", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( ( sbyte )10 ),
          new EqualToFilterCriterion( ( sbyte )20 ) ) );

      FilterParser.CheckCriterionBuilderResult( "  0   OR  127  ", type, null,
        new OrFilterCriterion(
          new EqualToFilterCriterion( ( sbyte )0 ),
          new EqualToFilterCriterion( sbyte.MaxValue ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "1,1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-129", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "128", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-129", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<=128", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "10 3", type, null,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "10 340", type, null,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderUInt16()
    {
      Type type = typeof( ushort );
      FilterParser.LogMessageCallback( "Testing UInt16 filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( ushort.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "65535", type, null,
        new EqualToFilterCriterion( ushort.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=0", type, null,
        new GreaterThanOrEqualToFilterCriterion( ushort.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "<>65535", type, null,
        new DifferentThanFilterCriterion( ushort.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( "309 AND 4555", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( ( ushort )309 ),
          new EqualToFilterCriterion( ( ushort )4555 ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 AND >000", type, null,
        new AndFilterCriterion(
          new LessThanFilterCriterion( ushort.MinValue ),
          new GreaterThanFilterCriterion( ushort.MinValue ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "1,1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-1", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "65536", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<>-1", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<=65536", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "10,000", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "10 000", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "139 AND 66000", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderUInt32()
    {
      Type type = typeof( uint );
      FilterParser.LogMessageCallback( "Testing UInt32 filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( uint.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "4294967295", type, null,
        new EqualToFilterCriterion( uint.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=0", type, null,
        new GreaterThanOrEqualToFilterCriterion( uint.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "<4294967295", type, null,
        new LessThanFilterCriterion( uint.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( "1 AND 71207", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( 1U ),
          new EqualToFilterCriterion( 71207U ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 OR >00000000000000000", type, null,
        new OrFilterCriterion(
          new LessThanFilterCriterion( uint.MinValue ),
          new GreaterThanFilterCriterion( uint.MinValue ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "1,1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-1", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "4294967296", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-1", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<4294967296", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "42 949 672", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "<42,949,672", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "26681 AND 5989 AND 429496729345", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "26681 5989 42949", type, null,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    private static void TestCriterionBuilderUInt64()
    {
      Type type = typeof( ulong );
      FilterParser.LogMessageCallback( "Testing UInt64 filter criteria..." );

      // Valid expressions
      FilterParser.CheckCriterionBuilderResult( "0", type, null,
        new EqualToFilterCriterion( ulong.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "18446744073709551615", type, null,
        new EqualToFilterCriterion( ulong.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( ">=0", type, null,
        new GreaterThanOrEqualToFilterCriterion( ulong.MinValue ) );

      FilterParser.CheckCriterionBuilderResult( "<18446744073709551615", type, null,
        new LessThanFilterCriterion( ulong.MaxValue ) );

      FilterParser.CheckCriterionBuilderResult( "54 AND 127915466", type, null,
        new AndFilterCriterion(
          new EqualToFilterCriterion( 54UL ),
          new EqualToFilterCriterion( 127915466UL ) ) );

      FilterParser.CheckCriterionBuilderResult( "<0 OR >00000000000000000", type, null,
        new OrFilterCriterion(
          new LessThanFilterCriterion( ulong.MinValue ),
          new GreaterThanFilterCriterion( ulong.MinValue ) ) );

      FilterParser.CheckCriterionBuilderResult( "NULL", type, null,
        new EqualToFilterCriterion( null ) );

      FilterParser.CheckCriterionBuilderResult( "\"NULL\"", type, null,
        new EqualToFilterCriterion( null ) );

      // Invalid expressions
      FilterParser.CheckCriterionBuilderResult( "", type, null,
        string.Format( FilterParser.MissingRightOperandErrorText, typeof( EqualToFilterCriterion ).Name ) );

      FilterParser.CheckCriterionBuilderResult( "1.1", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "1,1", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "-1", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "18446744073709551616", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( ">-1", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "<18446744073709551616", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "26681 AND 5989 AND 1844674407370955161455", type, null,
        FilterParser.NumberOverflowErrorText );

      FilterParser.CheckCriterionBuilderResult( "446 744 073 709 551", type, FrFrCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "446,744,073,709,551", type, CultureInfo.InvariantCulture,
        FilterParser.InvalidNumberFormatErrorText );

      FilterParser.CheckCriterionBuilderResult( "Null", type, null,
        FilterParser.InvalidNumberFormatErrorText );
    }

    #endregion Number test methods

    private static void CheckQuoteParserResult( string expression, params object[] expectedTokens )
    {
      List<Token> tokens = new List<Token>();

      string error = null;

      try
      {
        FilterParser.PrepareExpressionTokens( expression, tokens );
      }
      catch( Exception ex )
      {
        error = ex.Message;
      }

      if( error == null )
      {
        if( tokens.Count != expectedTokens.Length )
          FilterParser.LogMessageCallback( "#ERROR# Token count mismatch for expression: " + expression );

        if( tokens.Count == expectedTokens.Length )
        {
          for( int index = 0; index < expectedTokens.Length; index++ )
          {
            ValueToken expectedToken = expectedTokens[ index ] as ValueToken;

            if( expectedToken == null )
            {
              FilterParser.LogMessageCallback( "#ERROR# Missing expected Token in expression: " + expression );
            }
            else
            {
              Token resultToken = tokens[ index ];

              if( expectedToken.GetType() == resultToken.GetType() )
              {
                if( ( ( ValueToken )expectedToken ).Value != ( ( ValueToken )resultToken ).Value )
                  FilterParser.LogMessageCallback( "#ERROR# Value mismatch for Token in expression: " + expression );
              }
              else
              {
                FilterParser.LogMessageCallback( "#ERROR# Missing expected Token in expression: " + expression );
              }
            }
          }
        }
      }
      else
      {
        if( ( expectedTokens.Length == 1 ) && ( expectedTokens[ 0 ] is string ) )
        {
          if( ( string )expectedTokens[ 0 ] != error )
          {
            FilterParser.LogMessageCallback( "#ERROR# Expected <" + ( string )expectedTokens[ 0 ] + "> Got <" + error + "> Expression: " + expression );
          }
        }
        else
        {
          FilterParser.LogMessageCallback( "#ERROR# " + error + " Expression: " + expression );
        }
      }
    }

    private static void CheckCriterionBuilderResult( string expression, Type dataType, CultureInfo culture, object expectedResult )
    {
      FilterCriterion resultFilterCriterion = FilterParser.TryParse( expression, dataType, culture );

      if( resultFilterCriterion == null )
      {
        if( expectedResult is string )
        {
          if( ( string )expectedResult != FilterParser.LastError )
          {
            FilterParser.LogMessageCallback( "#ERROR# Expected <" + ( string )expectedResult + "> Got <" + FilterParser.LastError + "> Expression: " + expression );
          }
        }
        else
        {
          FilterParser.LogMessageCallback( "#ERROR# " + FilterParser.LastError + " Expression: " + expression );
        }
      }
      else
      {
        if( !string.IsNullOrEmpty( FilterParser.LastError ) )
          FilterParser.LogMessageCallback( "#ERROR# Parse succeeded but an error was set: " + FilterParser.LastError );

        if( expectedResult is FilterCriterion )
        {
          if( !resultFilterCriterion.Equals( expectedResult ) )
            FilterParser.LogMessageCallback( "#ERROR# Result mismatch for expression: " + expression + 
            ". Expected <" + ( ( FilterCriterion )expectedResult ).ToString() + "> Got <" + resultFilterCriterion.ToString() + ">" );
        }
        else
        {
          FilterParser.LogMessageCallback( "#ERROR# Unexpected valid expression: " + expression );
        }
      }
    }

    // As standard, use culture that has little chance of being modified by the 
    // developper's regional settings. This allow the use of the default settings
    // without having to manually force any standard value.
    private static CultureInfo FrFrCulture = CultureInfo.GetCultureInfo( "fr-FR" ); // France
    private static CultureInfo IsIsCulture = CultureInfo.GetCultureInfo( "is-IS" ); // Islande
    private static CultureInfo BrFrCulture = CultureInfo.GetCultureInfo( "br-FR" ); // Breton
    private static Action<string> s_logMessageCallback = null;
  }
}

#endif
