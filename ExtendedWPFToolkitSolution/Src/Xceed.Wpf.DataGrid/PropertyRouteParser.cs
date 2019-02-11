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
using System.Diagnostics;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal static class PropertyRouteParser
  {
    #region Static Fields

    private static readonly string ReservedSymbols = ".[]";

    #endregion

    internal static PropertyRoute Parse( string path )
    {
      path = ( path != null ) ? path.Trim() : string.Empty;

      var length = path.Length;
      if( length <= 0 )
        return null;

      var builder = new PropertyRouteBuilder();
      var state = ParserState.Start;
      var index = 0;

      while( index < length )
      {
        var c = path[ index ];

        if( char.IsWhiteSpace( c ) )
        {
          index++;
        }
        else
        {
          switch( state )
          {
            case ParserState.Start:
              {
                switch( c )
                {
                  case '.':
                    index++;
                    break;

                  default:
                    state = ParserState.Property;
                    break;
                }
              }
              break;

            case ParserState.Separator:
              {
                switch( c )
                {
                  case '.':
                    index++;
                    break;

                  case '[':
                    break;

                  default:
                    {
                      PropertyRouteParser.AddOther( path, length, ref index, builder );
                      Debug.Assert( index >= length );
                    }
                    break;
                }

                state = ParserState.Property;
              }
              break;

            case ParserState.Property:
              {
                switch( c )
                {
                  case '[':
                    {
                      if( PropertyRouteParser.AddIndexer( path, length, ref index, builder ) )
                      {
                        state = ParserState.Separator;
                      }
                      else
                      {
                        PropertyRouteParser.AddOther( path, length, ref index, builder );
                        Debug.Assert( index >= length );
                      }
                    }
                    break;

                  default:
                    {
                      if( PropertyRouteParser.AddProperty( path, length, ref index, builder ) )
                      {
                        state = ParserState.Separator;
                      }
                      else
                      {
                        PropertyRouteParser.AddOther( path, length, ref index, builder );
                        Debug.Assert( index >= length );
                      }
                    }
                    break;
                }
              }
              break;

            default:
              throw new NotImplementedException();
          }
        }
      }

      if( builder.IsEmpty )
      {
        builder.PushDescendant( PropertyRouteSegment.Self );
      }

      return builder.ToPropertyRoute();
    }

    internal static string Parse( DataGridItemPropertyBase itemProperty )
    {
      return PropertyRouteParser.Parse( PropertyRouteBuilder.ToPropertyRoute( DataGridItemPropertyRoute.Create( itemProperty ) ) );
    }

    internal static string Parse( PropertyRoute route )
    {
      if( route == null )
        return string.Empty;

      var sb = new StringBuilder();
      var childSegmentType = PropertyRouteSegmentType.Self;

      while( route != null )
      {
        var segment = route.Current;
        if( segment.Type != PropertyRouteSegmentType.Self )
        {
          if( !string.IsNullOrEmpty( segment.Name ) )
          {
            if( sb.Length != 0 )
            {
              switch( childSegmentType )
              {
                // For these type of segment, there is no need to put a separator.
                case PropertyRouteSegmentType.Self:
                case PropertyRouteSegmentType.Indexer:
                  break;

                default:
                  sb.Insert( 0, "." );
                  break;
              }
            }

            if( segment.Type == PropertyRouteSegmentType.Indexer )
            {
              sb.Insert( 0, "]" );
              sb.Insert( 0, segment.Name );
              sb.Insert( 0, "[" );
            }
            else
            {
              sb.Insert( 0, segment.Name );
            }
          }
        }
        else
        {
          if( ( route.Parent == null ) && ( sb.Length == 0 ) )
          {
            sb.Append( segment.Name );
          }
        }

        childSegmentType = segment.Type;
        route = route.Parent;
      }

      return sb.ToString();
    }

    private static bool AddIndexer( string path, int length, ref int index, PropertyRouteBuilder builder )
    {
      if( ( index >= length ) || ( path[ index ] != '[' ) )
        return false;

      var currentIndex = index + 1;
      var startIndex = currentIndex;

      while( currentIndex < length )
      {
        var c = path[ currentIndex ];

        if( c == ']' )
        {
          Debug.Assert( startIndex < length );
          Debug.Assert( startIndex <= currentIndex );

          var value = path.Substring( startIndex, currentIndex - startIndex ).Trim();
          if( value.Length <= 0 )
            return false;

          // The value is parsed twice in order to get a standard name that could be used as a key for further use.
          var parametersList = IndexerParametersParser.Parse( value );
          var parameters = IndexerParametersParser.Parse( parametersList );
          if( string.IsNullOrEmpty( parameters ) )
            return false;

          builder.PushDescendant( new PropertyRouteSegment( PropertyRouteSegmentType.Indexer, parameters ) );
          index = currentIndex + 1;

          return true;
        }
        else if( PropertyRouteParser.ReservedSymbols.IndexOf( c ) >= 0 )
        {
          return false;
        }

        currentIndex++;
      }

      return false;
    }

    private static bool AddProperty( string path, int length, ref int index, PropertyRouteBuilder builder )
    {
      var currentIndex = index;

      while( ( currentIndex < length ) && ( path[ currentIndex ] == '.' ) )
      {
        currentIndex++;
      }

      var startIndex = currentIndex;
      var parens = 0;

      while( currentIndex < length )
      {
        var c = path[ currentIndex ];

        if( PropertyRouteParser.ReservedSymbols.IndexOf( c ) >= 0 )
          break;

        if( c == '(' )
        {
          parens++;
        }
        else if( c == ')' )
        {
          parens--;

          // A closing parenthesis was found before an opening parenthesis.
          if( parens < 0 )
            return false;
        }

        currentIndex++;
      }

      if( ( parens != 0 ) || ( startIndex >= length ) )
        return false;

      var name = path.Substring( startIndex, currentIndex - startIndex ).Trim();
      if( name.Length > 0 )
      {
        builder.PushDescendant( new PropertyRouteSegment( PropertyRouteSegmentType.Property, name ) );
        index = currentIndex;
      }

      return true;
    }

    private static void AddOther( string path, int length, ref int index, PropertyRouteBuilder builder )
    {
      if( index >= length )
        return;

      var value = path.Substring( index, length - index ).Trim();

      builder.PushDescendant( new PropertyRouteSegment( PropertyRouteSegmentType.Other, value ) );
      index = length;
    }

    #region ParserState Private Enum

    private enum ParserState
    {
      Start,
      Separator,
      Property,
    }

    #endregion
  }
}
