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
using System.Linq;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid.Export
{
  internal class FormatHelper
  {

    public static string FormatCsvData( Type dataType, object dataValue, CsvFormatSettings formatSettings )
    {
      string outputString = null;
      bool checkWhitespace = true;

      if( ( dataValue != null ) && ( !Convert.IsDBNull( dataValue ) ) && ( !( dataValue is Array ) ) )
      {
        if( dataType == null )
          dataType = dataValue.GetType();

        if( dataType == typeof( string ) )
        {
          string textQualifier = formatSettings.TextQualifier.ToString();

          if( textQualifier == "\0" )
          {
            outputString = ( string )dataValue;
          }
          else
          {
            outputString = formatSettings.TextQualifier + ( ( string )dataValue ).Replace( textQualifier, textQualifier + textQualifier ) + formatSettings.TextQualifier;
          }

          checkWhitespace = false;
        }
        else if( dataType == typeof( DateTime ) )
        {
          if( !string.IsNullOrEmpty( formatSettings.DateTimeFormat ) )
          {
            if( formatSettings.Culture == null )
            {
              outputString = ( ( DateTime )dataValue ).ToString( formatSettings.DateTimeFormat, CultureInfo.InvariantCulture );
            }
            else
            {
              outputString = ( ( DateTime )dataValue ).ToString( formatSettings.DateTimeFormat, formatSettings.Culture );
            }
          }
        }
        else if( ( dataType == typeof( double ) ) ||
                 ( dataType == typeof( decimal ) ) ||
                 ( dataType == typeof( float ) ) ||
                 ( dataType == typeof( int ) ) ||
                 ( dataType == typeof( double ) ) ||
                 ( dataType == typeof( decimal ) ) ||
                 ( dataType == typeof( float ) ) ||
                 ( dataType == typeof( short ) ) ||
                 ( dataType == typeof( Single ) ) ||
                 ( dataType == typeof( UInt16 ) ) ||
                 ( dataType == typeof( UInt32 ) ) ||
                 ( dataType == typeof( UInt64 ) ) ||
                 ( dataType == typeof( Int16 ) ) ||
                 ( dataType == typeof( Int64 ) ) )
        {
          string format = formatSettings.NumericFormat;

          if( ( ( dataType == typeof( double ) ) ||
                ( dataType == typeof( decimal ) ) ||
                ( dataType == typeof( float ) ) ) &&
              ( !string.IsNullOrEmpty( formatSettings.FloatingPointFormat ) ) )
            format = formatSettings.FloatingPointFormat;

          if( !string.IsNullOrEmpty( format ) )
          {
            if( formatSettings.Culture == null )
            {
              outputString = string.Format( CultureInfo.InvariantCulture, "{0:" + format + "}", dataValue );
            }
            else
            {
              outputString = string.Format( formatSettings.Culture, "{0:" + format + "}", dataValue );
            }
          }
        }

        if( outputString == null )
        {
          if( formatSettings.Culture == null )
          {
            outputString = string.Format( CultureInfo.InvariantCulture, "{0}", dataValue );
          }
          else
          {
            outputString = string.Format( formatSettings.Culture, "{0}", dataValue );
          }
        }

        // For dates and numbers, as a rule, we never use the TextQualifier. However, the
        // specified format can introduce whitespaces. To better support this case, we add
        // the TextQualifier if needed.
        if( ( checkWhitespace ) && ( formatSettings.TextQualifier != '\0' ) )
        {
          bool useTextQualifier = false;

          // If the output string contains the character used to separate the fields, we
          // don't bother checking for whitespace. TextQualifier will be used.
          if( outputString.IndexOf( formatSettings.Separator ) < 0 )
          {
            for( int i = 0; i < outputString.Length; i++ )
            {
              if( char.IsWhiteSpace( outputString, i ) )
              {
                useTextQualifier = true;
                break;
              }
            }
          }
          else
          {
            useTextQualifier = true;
          }

          if( useTextQualifier )
            outputString = formatSettings.TextQualifier + outputString + formatSettings.TextQualifier;
        }

      }

      return outputString;
    }

    public static string FormatHtmlFieldData( Type dataType, object dataValue, HtmlFormatSettings formatSettings )
    {
      string outputString = null;

      if( ( dataValue != null ) && ( !Convert.IsDBNull( dataValue ) ) && ( !( dataValue is Array ) ) )
      {
        if( dataType == null )
          dataType = dataValue.GetType();

        if( dataType == typeof( string ) )
        {
          outputString = ( string )dataValue;
        }
        else if( dataType == typeof( DateTime ) )
        {
          if( !string.IsNullOrEmpty( formatSettings.DateTimeFormat ) )
          {
            if( formatSettings.Culture == null )
            {
              outputString = ( ( DateTime )dataValue ).ToString( formatSettings.DateTimeFormat, CultureInfo.InvariantCulture );
            }
            else
            {
              outputString = ( ( DateTime )dataValue ).ToString( formatSettings.DateTimeFormat, formatSettings.Culture );
            }
          }
        }
        else if( ( dataType == typeof( double ) ) ||
                 ( dataType == typeof( decimal ) ) ||
                 ( dataType == typeof( float ) ) ||
                 ( dataType == typeof( int ) ) ||
                 ( dataType == typeof( double ) ) ||
                 ( dataType == typeof( decimal ) ) ||
                 ( dataType == typeof( float ) ) ||
                 ( dataType == typeof( short ) ) ||
                 ( dataType == typeof( Single ) ) ||
                 ( dataType == typeof( UInt16 ) ) ||
                 ( dataType == typeof( UInt32 ) ) ||
                 ( dataType == typeof( UInt64 ) ) ||
                 ( dataType == typeof( Int16 ) ) ||
                 ( dataType == typeof( Int64 ) ) )
        {
          string format = formatSettings.NumericFormat;

          if( ( ( dataType == typeof( double ) ) ||
                ( dataType == typeof( decimal ) ) ||
                ( dataType == typeof( float ) ) ) &&
              ( !string.IsNullOrEmpty( formatSettings.FloatingPointFormat ) ) )
            format = formatSettings.FloatingPointFormat;

          if( !string.IsNullOrEmpty( format ) )
          {
            if( formatSettings.Culture == null )
            {
              outputString = string.Format( CultureInfo.InvariantCulture, "{0:" + format + "}", dataValue );
            }
            else
            {
              outputString = string.Format( formatSettings.Culture, "{0:" + format + "}", dataValue );
            }
          }
        }

        if( outputString == null )
        {
          if( formatSettings.Culture == null )
          {
            outputString = string.Format( CultureInfo.InvariantCulture, "{0}", dataValue );
          }
          else
          {
            outputString = string.Format( formatSettings.Culture, "{0}", dataValue );
          }
        }
      }

      return FormatHelper.FormatPlainTextAsHtml( outputString );
    }

    public static void FormatHtmlData( StringBuilder htmlDataStringBuilder )
    {
      string htmlDataString = htmlDataStringBuilder.ToString();

      StringBuilder headerStringBuilder = new StringBuilder();
      headerStringBuilder.Append( "Version:1.0" );
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "StartHTML:-1" ); // This is optional according to MSDN documentation
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "EndHTML:-1" ); // This is optional according to MSDN documentation
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "StartFragment:0000000109" ); // Always 109 bytes from start of Version to the end of <!--StartFragment--> tag
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "EndFragment:{0}" ); // need + 7 to "emulate" the 10 digits ( "{x}" is already 3 of the 10 chars )
      headerStringBuilder.Append( Environment.NewLine );
      headerStringBuilder.Append( "<!--StartFragment-->" );

      // We assure the StartFramgment offset is correct
      string headerString = headerStringBuilder.ToString();
      int startFragmentByteCounts = headerString.Length + 7; //  +7 chars as mentionned above
      Debug.Assert( startFragmentByteCounts == 109 );

      // Compute the EndFragment offset => header size + html data size + final end line (after data) length (\r\n)
      int endFragmentByteCounts = startFragmentByteCounts + htmlDataString.Length + Environment.NewLine.Length;
      string endFragmentOffset = endFragmentByteCounts.ToString( "0000000000", CultureInfo.InvariantCulture );
      headerString = string.Format( CultureInfo.InvariantCulture, headerString, endFragmentOffset );

      // Insert header at the beginning of the htmlDataStringBuilder
      htmlDataStringBuilder.Insert( 0, headerString );

      // Append the final end line and EndFragment tag
      htmlDataStringBuilder.Append( Environment.NewLine );
      htmlDataStringBuilder.Append( "<!--EndFragment-->" );
    }

    public static string FormatPlainTextAsHtml( string plainText )
    {
      StringBuilder htmlStringBuilder = new StringBuilder();
      if( string.IsNullOrEmpty( plainText ) == false )
      {
        int length = plainText.Length;

        for( int i = 0; i < length; i++ )
        {
          char currentChar = plainText[ i ];
          switch( currentChar )
          {
            case '\n':
              htmlStringBuilder.Append( "<br>" );
              break;

            case '\r':
              break;

            case '\u00A0':
              htmlStringBuilder.Append( "&nbsp;" );
              break;

            case '"':
              htmlStringBuilder.Append( "&quot;" );
              break;

            case '&':
              htmlStringBuilder.Append( "&amp;" );
              break;

            case '<':
              htmlStringBuilder.Append( "&lt;" );
              break;

            case '>':
              htmlStringBuilder.Append( "&gt;" );
              break;

            default:
              // For extended ascii table
              if( ( currentChar >= 160 ) && ( currentChar < 256 ) )
              {
                htmlStringBuilder.Append( "&#" );
                htmlStringBuilder.Append( ( ( int )currentChar ).ToString( NumberFormatInfo.InvariantInfo ) );
                htmlStringBuilder.Append( ';' );
              }
              else
              {
                htmlStringBuilder.Append( currentChar );
              }
              break;
          }
        }
      }
      else
      {
        // Add blank space into field when null or empty
        htmlStringBuilder.Append( "&nbsp;" );
      }

      return htmlStringBuilder.ToString();
    }
  }
}
