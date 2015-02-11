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
using System.Linq.Expressions;

namespace Xceed.Wpf.DataGrid.Utils
{
  internal static class PropertyHelper
  {
    internal static string GetPropertyName<TSource, TResult>( Expression<Func<TSource, TResult>> expression )
    {
      if( expression == null )
        throw new ArgumentNullException( "expression" );

      var memberExpression = expression.Body as MemberExpression;
      if( memberExpression == null )
        throw new ArgumentException( "The body of the expression must be a MemberExpression.", "expression" );

      return memberExpression.Member.Name;
    }
  }
}
