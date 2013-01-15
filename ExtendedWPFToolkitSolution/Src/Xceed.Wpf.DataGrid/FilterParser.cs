/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;

using Xceed.Wpf.DataGrid.FilterCriteria;

namespace Xceed.Wpf.DataGrid
{
  internal static partial class FilterParser
  {
    static FilterParser()
    {
      RegisteredFilterCriterionTypes.Add( typeof( AndFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( ContainsFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( DifferentThanFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( EqualToFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( GreaterThanOrEqualToFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( GreaterThanFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( LessThanOrEqualToFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( LessThanFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( NotFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( OrFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( EndsWithFilterCriterion ) );
      RegisteredFilterCriterionTypes.Add( typeof( StartsWithFilterCriterion ) );

    }

    public static string LastError
    {
      get
      {
        return FilterParser.LastErrorString;
      }

      set
      {
        // Always keep the error message that triggered the error state for this session.
        if( string.IsNullOrEmpty( value ) || string.IsNullOrEmpty( FilterParser.LastErrorString ) )
          FilterParser.LastErrorString = value;
      }
    }

    public static FilterCriterion ProduceCriterion( object parameterValue, Type defaultComparisonFilterCriterionType )
    {
      if( parameterValue is FilterCriterion )
      {
        return ( FilterCriterion )parameterValue;
      }
      else
      {
        if( !typeof( RelationalFilterCriterion ).IsAssignableFrom( defaultComparisonFilterCriterionType ) )
          throw new DataGridInternalException( "The default FilterCriterion type should derived from RelationalFilterCriterion." );

        return Activator.CreateInstance( defaultComparisonFilterCriterionType, parameterValue ) as RelationalFilterCriterion;
      }
    }

    // If culture is null, CurrentThread.CurrentCulture will be used when necessary.
    public static FilterCriterion Parse( string expression, Type dataType, CultureInfo culture )
    {
     FilterCriterion filterCriterion = null;
      List<Token> tokens = new List<Token>();

      FilterParser.LastError = null;

      FilterParser.PrepareExpressionTokens( expression, tokens );

      Type defaultCriterionType = ( dataType == typeof( string ) ) ? typeof( ContainsFilterCriterion ) : typeof( EqualToFilterCriterion );

      FilterParser.Tokenize( tokens, dataType, defaultCriterionType );
      FilterParser.TrimTokens( tokens );
      filterCriterion = FilterParser.BuildCriterion( tokens, dataType, defaultCriterionType, culture );

      return filterCriterion;
    }

    public static FilterCriterion TryParse( string expression, Type dataType, CultureInfo culture )
    {
      FilterCriterion filterCriterion = null;
      FilterParser.LastError = null;

      try
      {
        filterCriterion = FilterParser.Parse( expression, dataType, culture );
      }
      catch( Exception ex )
      {
        FilterParser.LastError = ex.Message;
      }

      return filterCriterion;
    }

    private static void TrimTokens( List<Token> tokens )
    {
      RawToken rawToken;

      for( int index = tokens.Count - 1; index >= 0; index-- )
      {
        rawToken = tokens[ index ] as RawToken;

        if( rawToken != null )
        {
          rawToken.Value = rawToken.Value.Trim();

          if( rawToken.Value.Length == 0 )
            tokens.RemoveAt( index );
        }
      }
    }

    private static void FillPrecedenceList( List<int> precedences )
    {
      foreach( Type type in FilterParser.RegisteredFilterCriterionTypes )
      {
        CriterionDescriptorAttribute attribute = ( CriterionDescriptorAttribute )type.GetCustomAttributes( typeof( CriterionDescriptorAttribute ), true )[ 0 ];

        if( !precedences.Contains( ( int )attribute.OperatorPrecedence ) )
          precedences.Add( ( int )attribute.OperatorPrecedence );
      }

      precedences.Sort();
    }

    private static void FillParserPriorityList( List<int> parserPriorities )
    {
      foreach( Type type in FilterParser.RegisteredFilterCriterionTypes )
      {
        CriterionDescriptorAttribute attribute = ( CriterionDescriptorAttribute )type.GetCustomAttributes( typeof( CriterionDescriptorAttribute ), true )[ 0 ];

        if( !parserPriorities.Contains( ( int )attribute.ParserPriority ) )
          parserPriorities.Add( ( int )attribute.ParserPriority );
      }

      parserPriorities.Sort();
    }

    /// <summary>
    /// Transforms the specified expression into one or more preliminary Tokens (RawToken
    /// or AtomicStringToken).
    /// </summary>
    private static void PrepareExpressionTokens( string expression, List<Token> tokens )
    {
      // Start index in the expression when we exclude all added tokens so far. This
      // value is only updated after closing an atomic string token.
      int stringStartIndex = 0;
      // count the number of double quotes simplified 
      int quotesCounter = 0;
      // Index of the first found " starting from the current position (0 for the first).
      int quoteIndex = 0;
      // Index of the first non " character after a found ".
      int quotesEnd = 0;
      // Index of the first character after the opening ".
      int quotesStart = 0;
      bool quoteOpened = false;

      expression = expression.Trim();
      quoteIndex = expression.IndexOf( '"' );

      while( quoteIndex >= 0 )
      {
        quotesEnd = quoteIndex + 1;

        // Find the first non " character starting from the index found.
        while( quotesEnd < expression.Length )
        {
          if( expression[ quotesEnd ] != '"' )
            break;

          quotesEnd++;
        }

        if( ( (( quotesEnd - quoteIndex > 1 ) && ( quoteOpened ) ) ) )
        {
          // Transform the " couples to a single ".
          StringBuilder temp = new StringBuilder( expression.Substring( 0, quoteIndex ) );

          for( int i = 0; i < ( quotesEnd - quoteIndex ) / 2; i++ )
          {
            temp.Append( '"' );
            quotesCounter++ ;
          }
          temp.Append( expression.Substring( quotesEnd ) );

          // Manually add the excess " when we're handling an odd number of " (can only 
          // happen when forming an AtomicString). The quotesCounter indicates where the
          // last quotes have to be added (in case of (Starts/Ends)With, the quotes were misplaced).
          if( ( quotesEnd - quoteIndex ) % 2 != 0 )
          {
            temp.Insert( quotesEnd - quotesCounter - 1, "\"" );
            quotesCounter = 0;
          }
          expression = temp.ToString();

          // Continue looking from the first character after the last " couple.
          quoteIndex = quoteIndex + ( quotesEnd - quoteIndex ) / 2;
        }

        else
        {
          if( quoteOpened )
          {
            tokens.Add( new AtomicStringToken( expression.Substring( quotesStart, quoteIndex - quotesStart ) ) );
            stringStartIndex = quoteIndex + 1;
          }
          else
          {
            if( quoteIndex > stringStartIndex )
              tokens.Add( new RawToken( expression.Substring( stringStartIndex, quoteIndex - stringStartIndex ) ) );

            quotesStart = quoteIndex + 1;
          }

          quoteOpened = !quoteOpened;
          quoteIndex++;
        }

        quoteIndex = expression.IndexOf( '"', quoteIndex );
      }

      if( quoteOpened )
        throw new DataGridException( FilterParser.MissingClosingQuotesErrorText );

      if( ( stringStartIndex > 0 ) && ( stringStartIndex < expression.Length ) )
      {
        if( stringStartIndex < expression.Length )
          tokens.Add( new RawToken( expression.Substring( stringStartIndex ) ) );
      }

      if( tokens.Count == 0 )
      {
        tokens.Add( new RawToken( expression ) );
      }
    }

    private static void Tokenize( List<Token> tokens, Type dataType, Type defaultCriterionType )
    {
      if( !typeof( FilterCriterion ).IsAssignableFrom( defaultCriterionType ) )
        throw new DataGridInternalException( "The default criterion type should be derived from the FilterCriterion class." );

      List<int> parserPriorities = new List<int>();
      FilterParser.FillParserPriorityList( parserPriorities );

      FilterParser.Tokenize( parserPriorities, tokens );

      if( ( tokens.Count == 1 ) && ( tokens[ 0 ] is ValueToken ) )
      {
        if( defaultCriterionType == typeof( ContainsFilterCriterion ) )
        {
          // ContainsFilterCriterion has no keyword associated; it won't be handled when 
          // building the criterion. Initialize it with the Value already in it.
          FilterCriterionToken containsToken = new FilterCriterionToken( ( FilterCriterion )Activator.CreateInstance( defaultCriterionType, ( ( ValueToken )tokens[ 0 ] ).Value ) );
          tokens.Clear();
          tokens.Add( containsToken );
        }
        else
        {
          // Insert the default token previous to the value.
          FilterCriterionToken defaultToken = new FilterCriterionToken( ( FilterCriterion )Activator.CreateInstance( defaultCriterionType ) );
          tokens.Insert( 0, defaultToken );
        }
      }
    }

    private static void Tokenize( List<int> parserPriorities, List<Token> tokens )
    {
      for( int index = 0; index < parserPriorities.Count; index++ )
      {
        int curParserPriority = parserPriorities[ index ];
        bool tokenAdded = false;

        do
        {
          tokenAdded = false;

          for( int tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++ )
          {
            RawToken rawToken = tokens[ tokenIndex ] as RawToken;
            Token precedentToken = ( tokenIndex > 0 ) ? tokens[ tokenIndex - 1 ] : null;
            if( rawToken != null )
            {
              FilterCriterionToken filterToken;
              int startIndex;
              int length;


              if( FilterParser.ExtractFirstCriterion( precedentToken, rawToken, curParserPriority, out filterToken, out startIndex, out length ) )
              {
                tokens.RemoveAt( tokenIndex );

                if( startIndex > 0 )
                {
                  tokens.Insert( tokenIndex, new RawToken( rawToken.Value.Substring( 0, startIndex ) ) );
                  tokenIndex++;
                }

                tokens.Insert( tokenIndex, filterToken );
                tokenIndex++;

                if( startIndex + length < rawToken.Value.Length )
                  tokens.Insert( tokenIndex, new RawToken( rawToken.Value.Substring( startIndex + length ) ) );

                tokenAdded = true;
                break;
              }
            }
          }
        }
        while( tokenAdded );
      }
    }

    //private static bool ExtractFirstCriterion( List<Token> rawTokens, int tokenIndex, int parserPriority, out FilterCriterionToken filterToken, out int startIndex, out int length )
    private static bool ExtractFirstCriterion( Token precedentToken, RawToken rawToken, int parserPriority, out FilterCriterionToken filterToken, out int startIndex, out int length )
    {
      //RawToken rawToken = rawTokens[ tokenIndex ] as RawToken;
      Type foundCriterionType = null;
      filterToken = null;
      startIndex = int.MaxValue;
      length = 0;

      foreach( Type type in FilterParser.RegisteredFilterCriterionTypes )
      {
        CriterionDescriptorAttribute attribute = ( CriterionDescriptorAttribute )type.GetCustomAttributes( typeof( CriterionDescriptorAttribute ), true )[ 0 ];

        if( ( int )attribute.ParserPriority == parserPriority )
        {
          if( !string.IsNullOrEmpty( attribute.Pattern ) )
          {
            string separator = attribute.Pattern.Replace( "@", "" );
            int foundIndex = -1;

            if( attribute.Pattern.StartsWith( "@" ) )
            {
              // The keyword can be anywhere in the token (there can be characters 
              // before the token).
              foundIndex = rawToken.Value.IndexOf( separator );
            }
            //else if( ( rawTokens[ tokenIndex + 1 ] as AtomicStringToken ) != null )
            //{

            //}
            else
            {
              // The keyword must be the first non whitespace character.
              if( rawToken.Value.TrimStart( null ).StartsWith( separator ) )
              {
                foundIndex = rawToken.Value.IndexOf( separator );
              }
            }

            if( ( foundIndex >= 0 ) &&
                ( ( foundIndex < startIndex ) ||
                  ( ( foundIndex == startIndex ) && ( separator.Length > length ) ) ) )
            {
              if( type == typeof( EndsWithFilterCriterion ) && ( precedentToken as AtomicStringToken ) != null )
                foundCriterionType = typeof( StartsWithFilterCriterion );
              else
                foundCriterionType = type;
              startIndex = foundIndex;
              length = separator.Length;
            }
          }
          else
          {
            if( type != typeof( ContainsFilterCriterion ) )
              throw new DataGridInternalException( "Missing pattern in attribute: " + type.Name );
          }
        }
      }

      if( foundCriterionType != null )
        filterToken = new FilterCriterionToken( ( FilterCriterion )Activator.CreateInstance( foundCriterionType ) );

      return ( filterToken != null );
    }

    private static FilterCriterion BuildCriterion( List<Token> tokens, Type dataType, Type defaultCriterionType, CultureInfo culture )
    {
      FilterCriterion filterCriterion = null;
      ReferenceList<FilterCriterion> initializedCriteria = new ReferenceList<FilterCriterion>();
      List<int> precedences = new List<int>();
      FilterParser.FillPrecedenceList( precedences );

      for( int index = 0; index < precedences.Count; index++ )
      {
        int curOperatorPrecedence = precedences[ index ];

        foreach( Type type in FilterParser.RegisteredFilterCriterionTypes )
        {
          CriterionDescriptorAttribute attribute = ( CriterionDescriptorAttribute )type.GetCustomAttributes( typeof( CriterionDescriptorAttribute ), true )[ 0 ];

          if( ( int )attribute.OperatorPrecedence == curOperatorPrecedence )
          {
            if( !string.IsNullOrEmpty( attribute.Pattern ) )
            {
              bool tokenConsumed;

              do
              {
                tokenConsumed = false;

                for( int i = 0; i < tokens.Count; i++ )
                {
                  FilterCriterionToken filterToken = tokens[ i ] as FilterCriterionToken;

                  if( ( filterToken != null ) && ( filterToken.FilterCriterion.GetType() == type ) && ( !initializedCriteria.Contains( filterToken.FilterCriterion ) ) )
                  {
                    List<object> parameters = new List<object>();

                    if( attribute.Pattern.StartsWith( "@" ) )
                    {
                      if( i <= 0 )
                        throw new DataGridException( string.Format( FilterParser.MissingLeftOperandErrorText, type.Name ) );

                      if( tokens[ i - 1 ] is ValueToken )
                      {
                        FilterParser.AddValueToParameters( parameters, ( ValueToken )tokens[ i - 1 ], dataType, culture );
                      }
                      else
                      {
                        parameters.Add( ( ( FilterCriterionToken )tokens[ i - 1 ] ).FilterCriterion );
                      }

                      tokens.RemoveAt( i - 1 );
                      i--;
                    }

                    if( attribute.Pattern.EndsWith( "@" ) )
                    {
                      if( i + 1 >= tokens.Count )
                        throw new DataGridException( string.Format( FilterParser.MissingRightOperandErrorText, type.Name ) );

                      if( tokens[ i + 1 ] is ValueToken )
                      {
                        FilterParser.AddValueToParameters( parameters, ( ValueToken )tokens[ i + 1 ], dataType, culture );
                      }
                      else
                      {
                        parameters.Add( ( ( FilterCriterionToken )tokens[ i + 1 ] ).FilterCriterion );
                      }

                      tokens.RemoveAt( i + 1 );
                      i--;
                    }

                    filterToken.FilterCriterion.InitializeFrom( parameters.ToArray(), defaultCriterionType );
                    initializedCriteria.Add( filterToken.FilterCriterion );
                    tokenConsumed = true;
                    break;
                  }
                }
              }
              while( tokenConsumed );
            }
            else
            {
              if( type != typeof( ContainsFilterCriterion ) )
                throw new DataGridInternalException( "Missing pattern in attribute: " + type.Name );
            }
          }
        }
      }

      if( ( tokens.Count == 1 ) && ( tokens[ 0 ] is FilterCriterionToken ) )
      {
        filterCriterion = ( ( FilterCriterionToken )tokens[ 0 ] ).FilterCriterion;
      }
      else
      {
        throw new DataGridException( FilterParser.InvalidExpressionErrorText );
      }

      return filterCriterion;
    }

    private static void AddValueToParameters( List<object> parameters, ValueToken token, Type dataType, CultureInfo culture )
    {
      if( token.Value == "NULL" )
      {
        parameters.Add( null );
      }
      else if( dataType == typeof( string ) )
      {
        parameters.Add( token.Value );
      }
      else if( dataType == typeof( Guid ) )
      {
        System.ComponentModel.GuidConverter converter = new System.ComponentModel.GuidConverter();

        parameters.Add( converter.ConvertFrom( token.Value ) );
      }
      else if( ( dataType == typeof( byte ) )
            || ( dataType == typeof( short ) )
            || ( dataType == typeof( bool ) )
            || ( dataType == typeof( int ) )
            || ( dataType == typeof( long ) )
            || ( dataType == typeof( float ) )
            || ( dataType == typeof( double ) )
            || ( dataType == typeof( decimal ) )
            || ( dataType == typeof( sbyte ) )
            || ( dataType == typeof( ushort ) )
            || ( dataType == typeof( uint ) )
            || ( dataType == typeof( ulong ) ) )
      {
        try
        {
          parameters.Add( Convert.ChangeType( token.Value, dataType, culture ) );
        }
        catch( OverflowException )
        {
          throw new DataGridException( FilterParser.NumberOverflowErrorText );
        }
        catch( FormatException )
        {
          throw new DataGridException( FilterParser.InvalidNumberFormatErrorText );
        }
      }
      else if( dataType == typeof( char ) )
      {
        if( token.Value.Length == 1 )
        {
          parameters.Add( token.Value[ 0 ] );
        }
        else
        {
          throw new DataGridException( string.Format( FilterParser.InvalidCharValueErrorText, token.Value ) );
        }
      }
      else if( dataType == typeof( DateTime ) )
      {
        DateTime dateTime;

        try
        {
          if( culture == null )
          {
            dateTime = DateTime.Parse( token.Value );
          }
          else
          {
            dateTime = DateTime.Parse( token.Value, culture );
          }
        }
        catch( FormatException )
        {
          throw new DataGridException( string.Format( FilterParser.InvalidDateTimeValueErrorText, token.Value ) );
        }

        parameters.Add( dateTime );
      }
    }

    private static CriterionTypeCollection RegisteredFilterCriterionTypes = new CriterionTypeCollection();
    private static string LastErrorString = null;
    internal static string MissingClosingQuotesErrorText = "Missing closing quotes.";
    internal static string MissingRightOperandErrorText = "Missing right operand for filter {0}.";
    internal static string MissingLeftOperandErrorText = "Missing left operand for filter {0}.";
    internal static string InvalidExpressionErrorText = "The expression cannot be resolved. Are you missing a logical operator?";
    internal static string InvalidCharValueErrorText = "{0} is not a valid character value.";
    internal static string InvalidDateTimeValueErrorText = "Invalid date/time value. It is recommended to use the following format: yyyy-MM-ddTHH:mm:ss.fff."; 
    internal static string InvalidNumberFormatErrorText = "Invalid number format.";
    internal static string InvalidEnumValueErrorText = "Invalid enum value."; 
    internal static string NumberOverflowErrorText = "Number value is out of range."; 

    #region CriterionTypeCollection Nested Type

    private class CriterionTypeCollection : Collection<Type>
    {
      protected override void InsertItem( int index, Type item )
      {
        if( !typeof( FilterCriterion ).IsAssignableFrom( item ) )
          throw new ArgumentException( "Must be derived from FilterCriterion.", "item" ); 

        if( item.GetCustomAttributes( typeof( CriterionDescriptorAttribute ), false ).Length == 0 )
          throw new ArgumentException( "Must have the CriterionDescriptor attribute.", "item" );

        base.InsertItem( index, item );
      }

      protected override void SetItem( int index, Type item )
      {
        if( !typeof( FilterCriterion ).IsAssignableFrom( item ) )
          throw new ArgumentException( "Must be derived from FilterCriterion.", "item" ); 

        if( item.GetCustomAttributes( typeof( CriterionDescriptorAttribute ), false ).Length == 0 )
          throw new ArgumentException( "Must have the CriterionDescriptor attribute.", "item" );

        base.SetItem( index, item );
      }
    }

    #endregion CriterionTypeCollection Nested Type

    #region Token Nested Type

    private abstract class Token
    {
    }

    #endregion Token Nested Type

    #region ValueToken Nested Type

    private abstract class ValueToken : Token
    {
      public ValueToken()
      {
      }

      public ValueToken( string value )
      {
        this.Value = value;
      }

      public string Value
      {
        get;
        set;
      }
    }

    #endregion ValueToken Nested Type

    #region RawToken Nested Type

    /// <summary>
    /// A token containing still unprocessed complete or partial expression or an actual
    /// value that is not between "".
    /// </summary>
    private class RawToken : ValueToken
    {
      public RawToken()
      {
      }

      public RawToken( string value )
        : base( value )
      {
      }
    }

    #endregion RawToken Nested Type

    #region AtomicStringToken Nested Type

    /// <summary>
    /// A token containing a value that will not be processed by the parser because it
    /// was between "".
    /// </summary>
    private class AtomicStringToken : ValueToken
    {
      public AtomicStringToken()
      {
      }

      public AtomicStringToken( string value )
        : base( value )
      {
      }
    }

    #endregion AtomicStringToken Nested Type

    #region FilterCriterionToken Nested Type

    private class FilterCriterionToken : Token
    {
      public FilterCriterionToken()
      {
      }

      public FilterCriterionToken( FilterCriterion filterCriterion )
      {
        this.FilterCriterion = filterCriterion;
      }

      public FilterCriterion FilterCriterion
      {
        get;
        set;
      }
    }

    #endregion FilterCriterionToken Nested Type

    #region ReferenceList Nested Type

    // This list will prevent the same object of being added twice.
    // To compare objects (in IndexOf, Add, Remove and Contains), it will use the 
    // ReferenceEquals() method.
    // Naturally, this class won't behave properly with ValueType because
    // of boxing.
    private class ReferenceList<T> : ICollection<T>
    {
      public int IndexOf( T item )
      {
        int foundIndex = -1;

        for( int i = 0; i < m_internalList.Count; i++ )
        {
          if( object.ReferenceEquals( item, m_internalList[ i ] ) )
          {
            foundIndex = i;
            break;
          }
        }

        return foundIndex;
      }

      #region ICollection<T> Members

      public void Add( T item )
      {
        if( this.Contains( item ) )
          throw new InvalidOperationException( "An attempt was made to add an item that is already contained in the list." ); 

        m_internalList.Add( item );
      }

      public void Clear()
      {
        m_internalList.Clear();
      }

      public bool Contains( T item )
      {
        return ( this.IndexOf( item ) > -1 );
      }

      public void CopyTo( T[] array, int arrayIndex )
      {
        m_internalList.CopyTo( array, arrayIndex );
      }

      public int Count
      {
        get
        {
          return m_internalList.Count;
        }
      }

      public bool IsReadOnly
      {
        get
        {
          return false;
        }
      }

      public bool Remove( T item )
      {
        int index = this.IndexOf( item );

        if( index > -1 )
        {
          m_internalList.RemoveAt( index );
        }

        return ( index > -1 );
      }

      #endregion

      #region IEnumerable<T> Members

      public IEnumerator<T> GetEnumerator()
      {
        return m_internalList.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        return m_internalList.GetEnumerator();
      }

      #endregion

      private List<T> m_internalList = new List<T>();
    }

    #endregion ReferenceList Nested Type
  }
}
