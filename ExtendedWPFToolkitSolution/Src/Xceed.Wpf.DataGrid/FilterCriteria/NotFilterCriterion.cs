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
using System.Windows.Markup;
using System.Linq.Expressions;

namespace Xceed.Wpf.DataGrid.FilterCriteria
{
  [ContentProperty( "FilterCriterion" )]
  [CriterionDescriptor( "NOT @", FilterOperatorPrecedence.NotOperator, FilterTokenPriority.NotOperator )]
  [DebuggerDisplay( "NOT( {FilterCriterion} )" )]
  public class NotFilterCriterion : FilterCriterion
  {
    public NotFilterCriterion()
    {
    }

    public NotFilterCriterion( FilterCriterion filterCriterion )
    {
      this.FilterCriterion = filterCriterion;
    }

    #region FilterCriterion Property

    public FilterCriterion FilterCriterion
    {
      get
      {
        return m_filterCriterion;
      }

      set
      {
        if( value == null )
          throw new ArgumentNullException( "FilterCriterion" );

        if( value != m_filterCriterion )
        {
          if( m_filterCriterion != null )
            m_filterCriterion.PropertyChanged -= new PropertyChangedEventHandler( FilterCriterion_PropertyChanged );

          m_filterCriterion = value;
          m_filterCriterion.PropertyChanged += new PropertyChangedEventHandler( FilterCriterion_PropertyChanged );
          this.RaisePropertyChanged( "FilterCriterion" );
        }
      }
    }

    private void FilterCriterion_PropertyChanged( object sender, PropertyChangedEventArgs e )
    {
      this.RaisePropertyChanged( "FilterCriterion" );
    }

    private FilterCriterion m_filterCriterion; // = null;

    #endregion FilterCriterion Property

    public override string ToExpression( CultureInfo culture )
    {
      if( this.FilterCriterion == null )
        return "";

      return "NOT " + this.FilterCriterion.ToExpression( culture );
    }

    public override System.Linq.Expressions.Expression ToLinqExpression( System.Linq.IQueryable queryable, ParameterExpression parameterExpression, string propertyName )
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

      if( m_filterCriterion == null )
        return null;

      Expression expressionToReverse = m_filterCriterion.ToLinqExpression( queryable, parameterExpression, propertyName );

      if( expressionToReverse == null )
        return null;

      return Expression.Not( expressionToReverse );
    }

    public override bool IsMatch( object value )
    {
      if( m_filterCriterion == null )
        return false;

      return !m_filterCriterion.IsMatch( value );
    }

    protected internal override void InitializeFrom( object[] parameters, Type defaultComparisonFilterCriterionType )
    {
      if( parameters.Length != 1 )
        throw new DataGridException( string.Format( FilterParser.MissingRightOperandErrorText, this.GetType().Name ) );

      this.FilterCriterion = FilterParser.ProduceCriterion( parameters[ 0 ], defaultComparisonFilterCriterionType );
    }

    public override bool Equals( object obj )
    {
      if( !base.Equals( obj ) )
        return false;

      NotFilterCriterion criterion = ( NotFilterCriterion )obj;

      return object.Equals( this.FilterCriterion, criterion.FilterCriterion );
    }

    public override int GetHashCode()
    {
      int hash = base.GetHashCode();

      if( this.FilterCriterion != null )
        hash ^= this.FilterCriterion.GetHashCode();

      return hash;
    }

#if DEBUG
    public override string ToString()
    {
      System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder( "NOT( " );
      stringBuilder.Append( ( this.FilterCriterion == null ) ? "#NULL#" : this.FilterCriterion.ToString() );
      stringBuilder.Append( " )" );

      return stringBuilder.ToString();
    }
#endif
  }
}
