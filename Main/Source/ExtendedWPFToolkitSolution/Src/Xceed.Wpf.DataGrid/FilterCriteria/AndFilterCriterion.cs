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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Linq.Expressions;
using System.Linq;

namespace Xceed.Wpf.DataGrid.FilterCriteria
{
  [CriterionDescriptor( "@ AND @", FilterOperatorPrecedence.AndOperator, FilterTokenPriority.AndOperator )]
  [DebuggerDisplay( "AND( {FirstFilterCriterion}, {SecondFilterCriterion} )" )]
  public class AndFilterCriterion : FilterCriterion
  {
    public AndFilterCriterion()
    {
    }

    public AndFilterCriterion( FilterCriterion firstFilterCriterion, FilterCriterion secondFilterCriterion )
    {
      this.FirstFilterCriterion = firstFilterCriterion;
      this.SecondFilterCriterion = secondFilterCriterion;
    }

    #region FirstFilterCriterion Property

    public FilterCriterion FirstFilterCriterion
    {
      get
      {
        return m_firstFilterCriterion;
      }

      set
      {
        if( value == null )
          throw new ArgumentNullException( "FirstFilterCriterion" );

        if( value != m_firstFilterCriterion )
        {
          if( m_firstFilterCriterion != null )
            m_firstFilterCriterion.PropertyChanged -= new PropertyChangedEventHandler( FirstFilterCriterion_PropertyChanged );

          m_firstFilterCriterion = value;
          m_firstFilterCriterion.PropertyChanged += new PropertyChangedEventHandler( FirstFilterCriterion_PropertyChanged );
          this.RaisePropertyChanged( "FirstFilterCriterion" );
        }
      }
    }

    private void FirstFilterCriterion_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      this.RaisePropertyChanged( "FirstFilterCriterion" );
    }

    private FilterCriterion m_firstFilterCriterion; // = null;

    #endregion FirstFilterCriterion Property

    #region SecondFilterCriterion Property

    public FilterCriterion SecondFilterCriterion
    {
      get
      {
        return m_secondFilterCriterion;
      }

      set
      {
        if( value == null )
          throw new ArgumentNullException( "SecondFilterCriterion" );

        if( value != m_secondFilterCriterion )
        {
          if( m_secondFilterCriterion != null )
            m_secondFilterCriterion.PropertyChanged -= new PropertyChangedEventHandler( SecondFilterCriterion_PropertyChanged );

          m_secondFilterCriterion = value;
          m_secondFilterCriterion.PropertyChanged += new PropertyChangedEventHandler( SecondFilterCriterion_PropertyChanged );
          this.RaisePropertyChanged( "SecondFilterCriterion" );
        }
      }
    }

    private void SecondFilterCriterion_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      this.RaisePropertyChanged( "SecondFilterCriterion" );
    }

    private FilterCriterion m_secondFilterCriterion; // = null;

    #endregion SecondFilterCriterion Property

    public override string ToExpression( CultureInfo culture )
    {
      if( ( this.FirstFilterCriterion == null ) || ( this.SecondFilterCriterion == null ) )
        return "";

      StringBuilder stringBuilder = new StringBuilder( this.FirstFilterCriterion.ToExpression( culture ) );
      stringBuilder.Append( " AND " );
      stringBuilder.Append( this.SecondFilterCriterion.ToExpression( culture ) );

      return stringBuilder.ToString();
    }

    public override Expression ToLinqExpression( IQueryable queryable, ParameterExpression parameterExpression, string propertyName )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      if( parameterExpression == null )
        throw new ArgumentNullException( "parameterExpression" );

      if( String.IsNullOrEmpty( propertyName ) )
      {
        if( propertyName == null )
          throw new ArgumentNullException( "propertyName" );

        throw new ArgumentException( "PropertyName must not be empty.", "propertyName" );
      }

      Expression firstFilterLINQExpression = ( m_firstFilterCriterion == null ) ? null : m_firstFilterCriterion.ToLinqExpression( queryable, parameterExpression, propertyName );
      Expression secondFilterLINQExpression = ( m_secondFilterCriterion == null ) ? null : m_secondFilterCriterion.ToLinqExpression( queryable, parameterExpression, propertyName );

      if( ( firstFilterLINQExpression != null ) && ( secondFilterLINQExpression != null ) )
        return Expression.And( firstFilterLINQExpression, secondFilterLINQExpression );

      if( firstFilterLINQExpression != null )
        return firstFilterLINQExpression;

      // Will return null if both criterion or resulting LINQ expressions are null;
      return secondFilterLINQExpression;
    }

    public override bool IsMatch( object value )
    {
      if( ( m_firstFilterCriterion == null ) || ( m_secondFilterCriterion == null ) )
        return false;

      return m_firstFilterCriterion.IsMatch( value ) && m_secondFilterCriterion.IsMatch( value );
    }

    public override bool Equals( object obj )
    {
      if( !base.Equals( obj ) )
        return false;

      AndFilterCriterion criterion = ( AndFilterCriterion )obj;

      return object.Equals( this.FirstFilterCriterion, criterion.FirstFilterCriterion )
        && object.Equals( this.SecondFilterCriterion, criterion.SecondFilterCriterion );
    }

    public override int GetHashCode()
    {
      int hash = base.GetHashCode();

      if( this.FirstFilterCriterion != null )
        hash ^= this.FirstFilterCriterion.GetHashCode();

      if( this.SecondFilterCriterion != null )
        hash ^= this.SecondFilterCriterion.GetHashCode();

      return hash;
    }

#if DEBUG
    public override string ToString()
    {
      System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder( "AND( " );
      stringBuilder.Append( this.FirstFilterCriterion.ToString() );
      stringBuilder.Append( ", " );
      stringBuilder.Append( this.SecondFilterCriterion.ToString() );
      stringBuilder.Append( " )" );

      return stringBuilder.ToString();
    }
#endif

    protected internal override void InitializeFrom( object[] parameters, Type defaultComparisonFilterCriterionType )
    {
      Debug.Assert( parameters.Length == 2, "Should have been caught earlier during BuildCriterion." );

      if( parameters.Length != 2 )
        throw new DataGridInternalException( "Missing operand for the AND operator." ); 

      this.FirstFilterCriterion = FilterParser.ProduceCriterion( parameters[ 0 ], defaultComparisonFilterCriterionType );
      this.SecondFilterCriterion = FilterParser.ProduceCriterion( parameters[ 1 ], defaultComparisonFilterCriterionType );
    }

  }
}
