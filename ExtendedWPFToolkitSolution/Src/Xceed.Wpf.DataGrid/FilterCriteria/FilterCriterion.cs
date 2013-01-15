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
using System.Globalization;
using System.Linq.Expressions;
using System.Linq;

namespace Xceed.Wpf.DataGrid.FilterCriteria
{
  [ CriterionDescriptor( "", FilterOperatorPrecedence.Default, FilterTokenPriority.Default ) ]
  public abstract class FilterCriterion : INotifyPropertyChanged
  {
#if DEBUG
    [Obsolete( "This method exists for the general public. Consider using the overload receiving the culture." )]
#endif
    public static FilterCriterion Parse( string expression, Type targetDataType )
    {
      return FilterCriterion.Parse( expression, targetDataType, null );
    }

    public static FilterCriterion Parse( string expression, Type targetDataType, CultureInfo culture )
    {
      if( expression == null )
        throw new ArgumentNullException( "expression" );

      if( targetDataType == null )
        throw new ArgumentNullException( "targetDataType" );

      return FilterParser.Parse( expression, targetDataType, culture );
    }

#if DEBUG
    [Obsolete( "This method exists for the general public. Consider using the overload receiving the culture." )]
#endif
    public static bool TryParse( string expression, Type targetDataType, out FilterCriterion filterCriterion )
    {
      return FilterCriterion.TryParse( expression, targetDataType, null, out filterCriterion );
    }

    public static bool TryParse( string expression, Type targetDataType, CultureInfo culture, out FilterCriterion filterCriterion )
    {
      if( expression == null )
        throw new ArgumentNullException( "expression" );

      if( targetDataType == null )
        throw new ArgumentNullException( "targetDataType" );

      filterCriterion = FilterParser.TryParse( expression, targetDataType, culture );

      return ( filterCriterion != null ) || ( FilterParser.LastError.Length == 0 );
    }

    public abstract bool IsMatch( object value );

    public abstract string ToExpression( CultureInfo culture);

    public abstract Expression ToLinqExpression( IQueryable queryable, ParameterExpression parameterExpression, string propertyName );

    public override bool Equals( object obj )
    {
      FilterCriterion filterCriterion = obj as FilterCriterion;

      if( filterCriterion == null )
        return false;

      return this.GetType() == filterCriterion.GetType();
    }

    public override int GetHashCode()
    {
      return this.GetType().GetHashCode();
    }

#if DEBUG
    public override string ToString()
    {
      return "#" + this.GetType().Name + "#";
    }
#endif

    protected internal abstract void InitializeFrom( object[] parameters, Type defaultComparisonFilterCriterionType );

    #region INotifyPropertyChanged Implementation

    public event PropertyChangedEventHandler PropertyChanged;

    protected void RaisePropertyChanged( string name )
    {
      if( this.PropertyChanged != null )
      {
        this.PropertyChanged( this, new PropertyChangedEventArgs( name ) );
      }
    }

    #endregion
  }
}
