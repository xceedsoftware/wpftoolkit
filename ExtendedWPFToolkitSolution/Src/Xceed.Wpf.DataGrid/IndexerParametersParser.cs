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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal static class IndexerParametersParser
  {
    #region Static Fields

    private const char Separator = ',';
    private const char DoubleQuote = '"';

    #endregion

    internal static string Parse( IEnumerable<object> values )
    {
      if( values == null )
        return string.Empty;

      var sb = new StringBuilder();
      var first = true;

      foreach( var value in values )
      {
        if( !first )
        {
          sb.Append( "," );
        }
        else
        {
          first = false;
        }

        if( value == null )
        {
          sb.Append( "null" );
        }
        else
        {
          sb.Append( Convert.ToString( value, CultureInfo.InvariantCulture ) );
        }
      }

      return sb.ToString();
    }

    internal static IEnumerable<string> Parse( string value )
    {
      var parameters = ( value != null ) ? value.Trim() : string.Empty;
      var length = parameters.Length;
      if( length <= 0 )
        return Enumerable.Empty<string>();

      var values = new List<string>();
      var state = ParserState.Start;
      var index = 0;

      while( index < length )
      {
        var c = parameters[ index ];

        switch( state )
        {
          case ParserState.Start:
            {
              if( char.IsWhiteSpace( c ) )
              {
                index++;
              }
              else
              {
                state = ParserState.FindValue;
              }
            }
            break;

          case ParserState.FindValue:
            {
              if( char.IsWhiteSpace( c ) )
              {
                index++;
              }
              else if( c == IndexerParametersParser.Separator )
              {
                // A parameter is missing.
                return Enumerable.Empty<string>();
              }
              else if( c == IndexerParametersParser.DoubleQuote )
              {
                state = ParserState.ExtractString;
              }
              else
              {
                state = ParserState.ExtractValue;
              }
            }
            break;

          case ParserState.FindSeparator:
            {
              if( char.IsWhiteSpace( c ) )
              {
                index++;
              }
              else if( c == IndexerParametersParser.Separator )
              {
                index++;
                state = ParserState.FindValue;
              }
              else
              {
                // Unexpected character.
                return Enumerable.Empty<string>();
              }
            }
            break;

          case ParserState.ExtractString:
            {
              if( !IndexerParametersParser.AddString( parameters, length, ref index, values ) )
                return Enumerable.Empty<string>();

              state = ParserState.FindSeparator;
            }
            break;

          case ParserState.ExtractValue:
            {
              if( !IndexerParametersParser.AddValue( parameters, length, ref index, values ) )
                return Enumerable.Empty<string>();

              state = ParserState.FindSeparator;
            }
            break;

          default:
            throw new NotImplementedException();
        }
      }

      if( state != ParserState.FindSeparator )
        return Enumerable.Empty<string>();

      return values;
    }

    private static bool AddString( string source, int length, ref int index, IList<string> values )
    {
      if( ( index >= length ) || ( source[ index ] != IndexerParametersParser.DoubleQuote ) )
        return false;

      var currentIndex = index + 1;
      var startIndex = currentIndex;
      var containsSeparator = false;

      while( currentIndex < length )
      {
        var c = source[ currentIndex ];
        if( c == IndexerParametersParser.DoubleQuote )
        {
          Debug.Assert( startIndex < length );
          Debug.Assert( startIndex <= currentIndex );

          var value = source.Substring( startIndex, currentIndex - startIndex ).Trim();
          if( value.Length <= 0 )
          {
            return false;
          }
          else if( containsSeparator )
          {
            values.Add( string.Format( "\"{0}\"", value ) );
          }
          else
          {
            values.Add( value );
          }

          index = currentIndex + 1;

          return true;
        }
        else
        {
          containsSeparator = containsSeparator || ( c == IndexerParametersParser.Separator );
          currentIndex++;
        }
      }

      return false;
    }

    private static bool AddValue( string source, int length, ref int index, IList<string> values )
    {
      if( index >= length )
        return false;

      var currentIndex = index;
      var startIndex = currentIndex;

      while( currentIndex < length )
      {
        var c = source[ currentIndex ];

        if( c == IndexerParametersParser.Separator )
          break;

        // Unexpected character.
        if( c == IndexerParametersParser.DoubleQuote )
          return false;

        currentIndex++;
      }

      var value = source.Substring( startIndex, currentIndex - startIndex ).Trim();
      if( value.Length <= 0 )
        return false;

      values.Add( value );

      index = currentIndex;

      return true;
    }

    #region ParserState Private Enum

    private enum ParserState
    {
      Start,
      FindValue,
      FindSeparator,
      ExtractString,
      ExtractValue,
    }

    #endregion
  }
}
