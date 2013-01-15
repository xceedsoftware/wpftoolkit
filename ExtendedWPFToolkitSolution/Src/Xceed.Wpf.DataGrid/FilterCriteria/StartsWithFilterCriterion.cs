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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace Xceed.Wpf.DataGrid.FilterCriteria
{
  [CriterionDescriptor( "@*", FilterOperatorPrecedence.RelationalOperator, FilterTokenPriority.RelationalOperator )]
  [DebuggerDisplay( "StartsWith( {Value} )" )]
  public class StartsWithFilterCriterion : RelationalFilterCriterion
  {
    public StartsWithFilterCriterion()
      : base()
    {
    }

    public StartsWithFilterCriterion( object value )
      : base( value )
    {
    }

    public override string ToExpression( CultureInfo culture )
    {
      string strValue = this.GetValueForExpression( culture );

      return ( strValue == null ) ? "" : strValue + "*";
    }

    // TODO: to be tested
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
      else
        queriedValue = queriedValue.Remove( queriedValue.LastIndexOf( '*' ) );

      return queryable.CreateStartsWithExpression( parameterExpression, propertyName, queriedValue );
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
      return strValue.StartsWith( searchedValue, true, CultureInfo.CurrentCulture );
    }
#if DEBUG
    public override string ToString()
    {
      System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder( "StartsWith( " );
      stringBuilder.Append( ( this.Value == null ) ? "#NULL#" : this.Value.ToString() );
      stringBuilder.Append( " )" );

      return stringBuilder.ToString();
    }
#endif
  }
}
