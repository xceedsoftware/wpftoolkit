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
using System.Diagnostics;
using System.Globalization;
using System.Windows.Markup;

namespace Xceed.Wpf.DataGrid.FilterCriteria
{
  [ContentProperty( "Value" )]
  public abstract class RelationalFilterCriterion : FilterCriterion
  {
    public RelationalFilterCriterion()
    {
    }

    public RelationalFilterCriterion( object value )
    {
      this.Value = value;
    }

    #region Value Property

    public object Value
    {
      get
      {
        return m_value;
      }

      set
      {
        if( value != m_value )
        {
          m_value = value;
          m_comparableValue = value as IComparable;
          this.RaisePropertyChanged( "Value" );
        }
      }
    }

    protected IComparable ComparableValue
    {
      get
      {
        return m_comparableValue;
      }
    }

    private object m_value;
    private IComparable m_comparableValue;

    #endregion Value Property

    public override bool Equals( object obj )
    {
      if( !base.Equals( obj ) )
        return false;

      RelationalFilterCriterion criterion = ( RelationalFilterCriterion )obj;

      return object.Equals( this.Value, criterion.Value );
    }

    public override int GetHashCode()
    {
      int hash = base.GetHashCode();

      if( this.Value != null )
        hash ^= this.Value.GetHashCode();

      return hash;
    }

    // Can return null.
    protected string GetValueForExpression( CultureInfo culture )
    {
      string strValue = this.Value as string;

      if( ( this.Value == null ) || ( this.Value == DBNull.Value ) )
      {
        strValue = "NULL";
      }
      else if( strValue == null )
      {
        Type dataType = this.Value.GetType();

        if( dataType == typeof( DateTime ) )
        {
          DateTime dateValue = ( DateTime )this.Value;

          // ISO 8601 format
          if( ( dateValue.Hour > 0 )
            || ( dateValue.Minute > 0 )
            || ( dateValue.Second > 0 )
            || ( dateValue.Millisecond > 0 ) )
          {
            strValue = ( ( DateTime )this.Value ).ToString( "yyyy-MM-ddTHH:mm:ss.FFFFFFF" );
          }
          else
          {
            strValue = ( ( DateTime )this.Value ).ToString( "yyyy-MM-dd" );
          }
        }
        else
        {
          try
          {
            if( culture == null )
            {
              strValue = Convert.ToString( this.Value );
            }
            else
            {
              strValue = Convert.ToString( this.Value, culture );
            }
          }
          catch( OverflowException )
          {
          }
          catch( FormatException )
          {
          }
        }
      }

      if( strValue != null )
      {
        int index = strValue.Length - 1;

        if( strValue == "" )
          strValue = "\"\"";

        while( index >= 0 )
        {
          if( strValue[ index ] == '"' )
          {
            strValue = strValue.Insert( index, "\"" );
          }

          index--;
        }

        if( strValue.Contains( " " ) )
        {
          strValue = "\"" + strValue + "\"";
        }
      }

      return strValue;
    }

    protected internal override void InitializeFrom( object[] parameters, Type defaultComparisonFilterCriterionType )
    {
      if( parameters.Length != 1 )
        throw new DataGridException( string.Format( FilterParser.MissingRightOperandErrorText, this.GetType().Name ) );

      Debug.Assert( !( parameters[ 0 ] is FilterCriterion), "Should have been caught earlier during BuildCriterion." );

      if( parameters[ 0 ] is FilterCriterion )
        throw new DataGridInternalException( "Comparison with the result of a filter criterion is not allowed." ); 

      this.Value = parameters[ 0 ];
    }

    /// <summary>
    /// Compare the specified value (left value) with the Value property (right value).
    /// For instance, this method will return LessThan if value &lt; this.Value.
    /// </summary>
    internal CompareResult Compare( object value )
    {
      object value2 = this.Value;

      if( ( value == null ) || ( value == DBNull.Value ) || (value as string == string.Empty) )
      {
        if( ( value2 == null ) || ( value2 == DBNull.Value ) || ( ( value2 as string ) == string.Empty ) )
        {
          return CompareResult.EqualsTo;
        }
        else
        {
          return CompareResult.LessThan;
        }
      }
      else
      {
        if( ( value2 == null ) || ( value2 == DBNull.Value ) )
        {
          return CompareResult.GreaterThan;
        }
        else if( m_comparableValue == null )
        {
          return object.Equals( value, value2 ) ? CompareResult.EqualsTo : CompareResult.DifferentThan;
        }
        else
        {
          int i = m_comparableValue.CompareTo( value );

          if( i < 0 )
          {
            // value2 is less than value1: value is greater than.
            return CompareResult.GreaterThan;
          }
          else if( i == 0 )
          {
            return CompareResult.EqualsTo;
          }
          else
          {
            // value2 is greater than value1: value is less than.
            return CompareResult.LessThan;
          }
        }
      }
    }
  }
}
