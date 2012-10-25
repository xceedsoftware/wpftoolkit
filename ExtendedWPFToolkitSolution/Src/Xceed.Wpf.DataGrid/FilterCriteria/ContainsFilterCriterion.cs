/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

  **********************************************************************/

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Linq;

namespace Xceed.Wpf.DataGrid.FilterCriteria
{
  [CriterionDescriptor( "", FilterOperatorPrecedence.RelationalOperator, FilterTokenPriority.RelationalOperator )]
  [DebuggerDisplay( "Contains( {Value} )" )]
  public class ContainsFilterCriterion : RelationalFilterCriterion
  {
    public ContainsFilterCriterion()
      : base()
    {
    }

    public ContainsFilterCriterion( object value )
      : base( value )
    {
    }

    public override string ToExpression( CultureInfo culture )
    {
      string strValue = this.GetValueForExpression( culture );

      return strValue ?? "";
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

      string queriedValue = this.Value as string;

      if( queriedValue == null )
        queriedValue = string.Empty;

      return queryable.CreateContainsExpression( parameterExpression, propertyName, queriedValue );
    }

    public override bool IsMatch( object value )
    {
      if( value == null )
        return false;

      if( this.Value == null )
        return true;

      string searchedValue = this.Value.ToString().ToLower();

      if( searchedValue.Length == 0 )
        return true;

      string strValue = value.ToString().ToLower();

      return strValue.Contains( searchedValue );
    }

#if DEBUG
    public override string ToString()
    {
      System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder( "Contains( " );
      stringBuilder.Append( ( this.Value == null ) ? "" : this.Value.ToString() );
      stringBuilder.Append( " )" );

      return stringBuilder.ToString();
    }
#endif
  }
}
