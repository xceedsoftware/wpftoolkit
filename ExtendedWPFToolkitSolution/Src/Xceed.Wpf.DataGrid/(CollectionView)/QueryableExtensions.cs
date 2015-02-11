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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Xceed.Wpf.DataGrid
{
  internal static class QueryableExtensions
  {
    #region REFLECTION METHODS

    private static MemberInfo GetMemberInfo( Type type, string memberName )
    {
      MemberInfo[] members = type.FindMembers( MemberTypes.Property | MemberTypes.Field,
        BindingFlags.Public | BindingFlags.Instance,
        Type.FilterNameIgnoreCase, memberName );

      if( members.Length != 0 )
        return members[ 0 ];

      return null;
    }

    internal static string[] FindPrimaryKeys( this IQueryable queryable )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      Type elementType = queryable.ElementType;

      List<string> primaryKeyNames = new List<string>();

      try
      {
        PropertyInfo[] properties = elementType.GetProperties();

        for( int i = 0; i < properties.Length; i++ )
        {
          PropertyInfo propertyInfo = properties[ i ];

          object[] attributes = propertyInfo.GetCustomAttributes( true );

          for( int j = 0; j < attributes.Length; j++ )
          {
            Attribute attribute = attributes[ j ] as Attribute;

            if( attribute == null )
              continue;

            Type attributeType = attribute.GetType();

            if( attributeType.FullName == "System.Data.Linq.Mapping.ColumnAttribute" )
            {
              // LINQ to SQL support.
              PropertyInfo isPrimaryKeyPropertyInfo = attributeType.GetProperty( "IsPrimaryKey" );

              if( isPrimaryKeyPropertyInfo != null )
              {
                bool isPrimaryKey = ( bool )isPrimaryKeyPropertyInfo.GetValue( attribute, null );

                if( isPrimaryKey )
                  primaryKeyNames.Add( propertyInfo.Name );
              }
            }
            else if( attributeType.FullName == "System.Data.Objects.DataClasses.EdmScalarPropertyAttribute" )
            {
              // LINQ to Entity support.
              PropertyInfo entityKeyPropertyInfo = attributeType.GetProperty( "EntityKeyProperty" );

              if( entityKeyPropertyInfo != null )
              {
                bool isPrimaryKey = ( bool )entityKeyPropertyInfo.GetValue( attribute, null );

                if( isPrimaryKey )
                  primaryKeyNames.Add( propertyInfo.Name );
              }
            }
          }
        }
      }
      catch
      {
      }

      return primaryKeyNames.ToArray();
    }


    #endregion REFLECTION METHODS

    #region ORDERING

    internal static IQueryable OrderBy(
      this IQueryable queryable,
      SortDescriptionCollection implicitSortDescriptions,
      SortDescriptionCollection explicitSortDescriptions,
      bool reverse )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      if( ( implicitSortDescriptions == null ) && ( explicitSortDescriptions == null ) )
        return queryable;

      Expression orderedExpression = queryable.Expression;

      Type queryableElementType = queryable.ElementType;

      ParameterExpression[] parameters = new ParameterExpression[] { Expression.Parameter( queryableElementType, "" ) };

      string ascendingOrderByMethodName = "OrderBy";
      string descendingOrderByMethodName = "OrderByDescending";

      if( explicitSortDescriptions != null )
      {
        foreach( SortDescription sortDescription in explicitSortDescriptions )
        {
          MemberExpression sortMemberExpression = QueryableExtensions.GenerateMemberExpression( parameters[ 0 ], sortDescription.PropertyName );

          string methodNameToUse;

          if( ( sortDescription.Direction == ListSortDirection.Ascending && !reverse ) || ( sortDescription.Direction == ListSortDirection.Descending && reverse ) )
          {
            methodNameToUse = ascendingOrderByMethodName;
          }
          else
          {
            methodNameToUse = descendingOrderByMethodName;
          }

          orderedExpression = Expression.Call( typeof( Queryable ),
            methodNameToUse,
            new Type[] { queryableElementType, sortMemberExpression.Type },
            orderedExpression, Expression.Quote( Expression.Lambda( sortMemberExpression, parameters ) ) );


          ascendingOrderByMethodName = "ThenBy";
          descendingOrderByMethodName = "ThenByDescending";
        }
      }

      if( implicitSortDescriptions != null )
      {
        foreach( SortDescription sortDescription in implicitSortDescriptions )
        {
          MemberExpression sortMemberExpression = QueryableExtensions.GenerateMemberExpression( parameters[ 0 ], sortDescription.PropertyName );

          string methodNameToUse;

          if( ( sortDescription.Direction == ListSortDirection.Ascending && !reverse ) || ( sortDescription.Direction == ListSortDirection.Descending && reverse ) )
          {
            methodNameToUse = ascendingOrderByMethodName;
          }
          else
          {
            methodNameToUse = descendingOrderByMethodName;
          }

          orderedExpression = Expression.Call( typeof( Queryable ),
            methodNameToUse,
            new Type[] { queryableElementType, sortMemberExpression.Type },
            orderedExpression, Expression.Quote( Expression.Lambda( sortMemberExpression, parameters ) ) );


          ascendingOrderByMethodName = "ThenBy";
          descendingOrderByMethodName = "ThenByDescending";
        }
      }

      return queryable.Provider.CreateQuery( orderedExpression );
    }

    #endregion ORDERING

    #region FILTERING

    internal static Expression CreateStartsWithExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params string[] values )
    {
      return QueryableExtensions.FilterString( queryable, sharedParameterExpression, propertyName, values, StringFilterMode.StartsWith );
    }

    internal static Expression CreateEndsWithExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params string[] values )
    {
      return QueryableExtensions.FilterString( queryable, sharedParameterExpression, propertyName, values, StringFilterMode.EndsWith );
    }

    internal static Expression CreateContainsExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params string[] values )
    {
      return QueryableExtensions.FilterString( queryable, sharedParameterExpression, propertyName, values, StringFilterMode.Contains );
    }


    internal static Expression CreateGreaterThanExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params object[] values )
    {
      return QueryableExtensions.CreateBinaryComparison(
        queryable,
        sharedParameterExpression,
        propertyName,
        values,
        BinaryExpression.GreaterThan );
    }

    internal static Expression CreateGreaterThanOrEqualExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params object[] values )
    {
      return QueryableExtensions.CreateBinaryComparison(
        queryable,
        sharedParameterExpression,
        propertyName,
        values,
        BinaryExpression.GreaterThanOrEqual );
    }


    internal static Expression CreateLesserThanExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params object[] values )
    {
      return QueryableExtensions.CreateBinaryComparison(
        queryable,
        sharedParameterExpression,
        propertyName,
        values,
        BinaryExpression.LessThan );
    }

    internal static Expression CreateLesserThanOrEqualExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params object[] values )
    {
      return QueryableExtensions.CreateBinaryComparison(
        queryable,
        sharedParameterExpression,
        propertyName,
        values,
        BinaryExpression.LessThanOrEqual );
    }

    internal static Expression CreateEqualExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params object[] values )
    {
      return QueryableExtensions.CreateBinaryComparison(
        queryable,
        sharedParameterExpression,
        propertyName,
        values,
        BinaryExpression.Equal );
    }

    internal static Expression CreateDifferentThanExpression(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      params object[] values )
    {
      return QueryableExtensions.CreateBinaryComparison(
        queryable,
        sharedParameterExpression,
        propertyName,
        values,
        BinaryExpression.NotEqual );
    }


    internal static ParameterExpression CreateParameterExpression( this IQueryable queryable )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      return Expression.Parameter( queryable.ElementType, "" );
    }

    private static Expression CreateBinaryComparison(
      this IQueryable queryable,
      ParameterExpression sharedParameterExpression,
      string propertyName,
      object[] values,
      BinaryComparisonDelegate binaryComparisonDelegate )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      if( ( values == null ) || ( values.Length == 0 ) )
        throw new ArgumentNullException( "values" );

      if( binaryComparisonDelegate == null )
        throw new ArgumentNullException( "binaryComparisonDelegate" );

      MemberExpression memberExpression = QueryableExtensions.GenerateMemberExpression( sharedParameterExpression, propertyName );

      Expression mergedFilterExpression = null;

      for( int i = 0; i < values.Length; i++ )
      {
        ConstantExpression valueExpression = Expression.Constant( values[ i ] );

        Expression newFilterExpression = binaryComparisonDelegate( memberExpression,
          Expression.Convert( valueExpression, memberExpression.Type ) );

        if( mergedFilterExpression == null )
        {
          mergedFilterExpression = newFilterExpression;
        }
        else
        {
          mergedFilterExpression = Expression.Or( mergedFilterExpression, newFilterExpression );
        }
      }

      return mergedFilterExpression;
    }


    internal static IQueryable WhereFilter( this IQueryable queryable, ParameterExpression sharedParameterExpression, Expression expression )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      Type queryableElementType = queryable.ElementType;

      LambdaExpression whereLambda = Expression.Lambda( expression, sharedParameterExpression );

      MethodCallExpression whereCall =
        Expression.Call(
        typeof( Queryable ), "Where",
        new Type[] { queryableElementType },
        queryable.Expression,
        Expression.Quote( whereLambda ) );

      return queryable.Provider.CreateQuery( whereCall );
    }



    #endregion FILTERING

    #region SELECTING

    internal static IQueryable Select( this IQueryable queryable, string propertyName )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      if( string.IsNullOrEmpty( propertyName ) )
      {
        if( propertyName == null )
          throw new ArgumentNullException( "propertyName" );

        throw new ArgumentException( "A property name must be provided.", "propertyName" );
      }

      Type queryableElementType = queryable.ElementType;
      ParameterExpression[] parameters = new ParameterExpression[] { Expression.Parameter( queryableElementType, "" ) };

      MemberExpression selectMemberExpression = QueryableExtensions.GenerateMemberExpression( parameters[ 0 ], propertyName );

      LambdaExpression selectLambdaExpression = Expression.Lambda( selectMemberExpression, parameters );

      return queryable.Provider.CreateQuery(
        Expression.Call( typeof( Queryable ), "Select",
        new Type[] { queryableElementType, selectLambdaExpression.Body.Type },
        queryable.Expression, Expression.Quote( selectLambdaExpression ) ) );
    }

    internal static IQueryable GetSubGroupsAndCountsQueryable( this IQueryable queryable, string subGroupBy, bool sortGroupBy, ListSortDirection direction )
    {
      // Create GroupBy
      Type queryableElementType = queryable.ElementType;

      ParameterExpression[] parameters = new ParameterExpression[] { Expression.Parameter( queryableElementType, "" ) };
      MemberExpression memberExpression = QueryableExtensions.GenerateMemberExpression( parameters[ 0 ], subGroupBy );
      LambdaExpression groupByLambdaExpression = Expression.Lambda( memberExpression, parameters );

      MethodCallExpression groupByMethodExpression =
        Expression.Call(
        typeof( Queryable ), "GroupBy",
        new Type[] { queryableElementType, groupByLambdaExpression.Body.Type },
        new Expression[] { queryable.Expression, Expression.Quote( groupByLambdaExpression ) } );

      IQueryable groupedResult = queryable.Provider.CreateQuery( groupByMethodExpression );

      if( sortGroupBy )
        groupedResult = groupedResult.OrderByKey( direction == ListSortDirection.Ascending );

      ParameterExpression[] groupedParameters = new ParameterExpression[] { System.Linq.Expressions.Expression.Parameter( groupedResult.ElementType, "" ) };

      MemberExpression keyMemberExpression = MemberExpression.Property( groupedParameters[ 0 ], "Key" );
      MethodCallExpression countCallExpression = MethodCallExpression.Call( typeof( Enumerable ), "Count", new Type[] { queryableElementType }, groupedParameters );

      QueryableGroupNameCountPairInfo queryableGroupNameCountPairInfo =
        QueryableExtensions.QueryableGroupNameCountPairInfos.GetInfosForType( memberExpression.Type );

      Expression[] newExpressionArguments = new Expression[ 2 ] { keyMemberExpression, countCallExpression };

      NewExpression newExpression =
        NewExpression.New(
        queryableGroupNameCountPairInfo.ConstructorInfo,
        newExpressionArguments,
        new MemberInfo[] { queryableGroupNameCountPairInfo.KeyPropertyInfo, queryableGroupNameCountPairInfo.CountPropertyInfo } );

      LambdaExpression finalLambdaExpression = System.Linq.Expressions.Expression.Lambda( newExpression, groupedParameters );

      MethodCallExpression finalSelectExpression =
        System.Linq.Expressions.Expression.Call(
        typeof( Queryable ), "Select",
        new Type[] { groupedResult.ElementType, finalLambdaExpression.Body.Type },
        new System.Linq.Expressions.Expression[] { groupedResult.Expression, System.Linq.Expressions.Expression.Quote( finalLambdaExpression ) } );

      return groupedResult.Provider.CreateQuery( finalSelectExpression );
    }

    internal static IQueryable Slice( this IQueryable queryable, int skip, int take )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      Expression skipAndTakeExpression = queryable.Expression;

      Type queryableElementType = queryable.ElementType;

      skipAndTakeExpression = QueryableExtensions.Skip( queryableElementType, skipAndTakeExpression, skip );
      skipAndTakeExpression = QueryableExtensions.Take( queryableElementType, skipAndTakeExpression, take );

      return queryable.Provider.CreateQuery( skipAndTakeExpression );
    }

    internal static int Count( this IQueryable queryable )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      Type queryableElementType = queryable.ElementType;
      Expression expressionToCount = queryable.Expression;

      MethodCallExpression countExpression = Expression.Call(
        typeof( Queryable ), "Count",
        new Type[] { queryableElementType }, expressionToCount );

      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Beginning Provider Execute for total count." );
      int count = ( int )queryable.Provider.Execute( countExpression );
      Debug.WriteLineIf( VirtualPageManager.DebugDataVirtualization, "Ended Provider Execute for total count." );
      return count;
    }

    #endregion SELECTING

    #region PRIVATE METHODS

    private static IQueryable OrderByKey( this IQueryable queryable, bool ascending )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      Expression orderedExpression = queryable.Expression;

      Type queryableElementType = queryable.ElementType;

      ParameterExpression parameter = queryable.CreateParameterExpression();

      MemberExpression sortMemberExpression = QueryableExtensions.GenerateMemberExpression( parameter, "Key" );

      string methodName = ( ascending ) ? "OrderBy" : "OrderByDescending";

      orderedExpression = Expression.Call( typeof( Queryable ),
            methodName,
            new Type[] { queryableElementType, sortMemberExpression.Type },
            orderedExpression, Expression.Quote( Expression.Lambda( sortMemberExpression, parameter ) ) );

      return queryable.Provider.CreateQuery( orderedExpression );
    }


    internal static MemberExpression GenerateMemberExpression( ParameterExpression typeExpression, string name )
    {
      if( string.IsNullOrEmpty( name ) )
      {
        if( name == null )
          throw new ArgumentNullException( "name" );

        throw new ArgumentException( "A name must be provided.", "name" );
      }

      MemberExpression memberExpression = null;

      MemberInfo member = QueryableExtensions.GetMemberInfo( typeExpression.Type, name );

      if( member == null )
        throw new DataGridInternalException( "MemberInfo is null." );

      if( member is PropertyInfo )
      {
        // Member is a property.
        memberExpression = Expression.Property( typeExpression, ( PropertyInfo )member );
      }
      else
      {
        // Member is a field.
        memberExpression = Expression.Field( typeExpression, ( FieldInfo )member );
      }

      return memberExpression;
    }

    private static MethodCallExpression Take( Type queryableElementType, Expression expression, int count )
    {
      if( queryableElementType == null )
        throw new ArgumentNullException( "queryableElementType" );

      if( expression == null )
        throw new ArgumentNullException( "expression" );

      return Expression.Call( typeof( Queryable ), "Take", new Type[] { queryableElementType }, expression, Expression.Constant( count ) );
    }

    private static MethodCallExpression Skip( Type queryableElementType, Expression expression, int count )
    {
      if( queryableElementType == null )
        throw new ArgumentNullException( "queryableElementType" );

      if( expression == null )
        throw new ArgumentNullException( "expression" );

      return Expression.Call( typeof( Queryable ), "Skip", new Type[] { queryableElementType }, expression, Expression.Constant( count ) );
    }

    private static string GetMethodNameFromStringFilterMode( StringFilterMode stringFilterMode )
    {
      switch( stringFilterMode )
      {
        case StringFilterMode.StartsWith:
          {
            return "StartsWith";
          }

        case StringFilterMode.Contains:
          {
            return "Contains";
          }

        case StringFilterMode.EndsWith:
          {
            return "EndsWith";
          }
      }

      Debug.Fail( "Should have handled the new StringFilterMode." );
      return string.Empty;
    }

    private static Expression FilterString(
     this IQueryable queryable,
     ParameterExpression sharedParameterExpression,
     string propertyName,
     string[] values,
     StringFilterMode stringFilterMode )
    {
      if( queryable == null )
        throw new ArgumentNullException( "queryable" );

      if( ( values == null ) || ( values.Length == 0 ) )
        throw new ArgumentNullException( "values" );

      string methodName = QueryableExtensions.GetMethodNameFromStringFilterMode( stringFilterMode );

      Debug.Assert( !String.IsNullOrEmpty( methodName ) );

      MemberExpression memberExpression = QueryableExtensions.GenerateMemberExpression( sharedParameterExpression, propertyName );

      Expression mergedFilterCall = null;

      for( int i = 0; i < values.Length; i++ )
      {
        ConstantExpression valueExpression = Expression.Constant( values[ i ] );

        MethodCallExpression newFilterCall = Expression.Call(
          memberExpression,
          typeof( string ).GetMethod( methodName, new Type[] { typeof( string ) } ),
          Expression.Convert( valueExpression, memberExpression.Type ) );

        if( mergedFilterCall == null )
        {
          mergedFilterCall = newFilterCall;
        }
        else
        {
          mergedFilterCall = MethodCallExpression.Or( mergedFilterCall, newFilterCall );
        }
      }

      return mergedFilterCall;
    }

    #endregion PRIVATE METHODS

    #region PRIVATE FIELDS

    private delegate Expression BinaryComparisonDelegate( Expression left, Expression right );




    #endregion PRIVATE FIELDS

    #region INTERNAL NESTED CLASSES

    internal interface IQueryableGroupNameCountPair
    {
      object GroupName
      {
        get;
      }

      Type GroupNameType
      {
        get;
      }

      int Count
      {
        get;
      }
    }

    #endregion INTERNAL NESTED CLASSES

    #region PRIVATE NESTED CLASSES

    private class QueryableGroupNameCountPair<T> : IQueryableGroupNameCountPair
    {
      public QueryableGroupNameCountPair( T groupName, int count )
      {
        this.GroupName = groupName;
        this.Count = count;
      }

      public T GroupName
      {
        get;
        set;
      }

      public int Count
      {
        get;
        set;
      }

      #region QueryableGroupNameCountPair Members

      object IQueryableGroupNameCountPair.GroupName
      {
        get
        {
          return this.GroupName;
        }
      }

      public Type GroupNameType
      {
        get
        {
          return typeof( T );
        }
      }

      int IQueryableGroupNameCountPair.Count
      {
        get
        {
          return this.Count;
        }
      }

      #endregion
    }

    private static class QueryableGroupNameCountPairInfos
    {
      public static QueryableGroupNameCountPairInfo GetInfosForType( Type sourceType )
      {
        if( QueryableGroupNameCountPairInfos.ReflectionInfoDictionary == null )
          QueryableGroupNameCountPairInfos.ReflectionInfoDictionary = new Dictionary<Type, QueryableGroupNameCountPairInfo>();

        QueryableGroupNameCountPairInfo queryableGroupNameCountPairInfo;

        if( !QueryableGroupNameCountPairInfos.ReflectionInfoDictionary.TryGetValue( sourceType, out queryableGroupNameCountPairInfo ) )
        {
          Type queryableGroupNameCountPairType = typeof( QueryableGroupNameCountPair<> ).MakeGenericType( sourceType );

          ConstructorInfo constructorInfo =
            queryableGroupNameCountPairType.GetConstructor( new Type[] { sourceType, typeof( int ) } );

          PropertyInfo keyPropertyInfo = queryableGroupNameCountPairType.GetProperty( "GroupName" );
          PropertyInfo countPropertyInfo = queryableGroupNameCountPairType.GetProperty( "Count" );

          queryableGroupNameCountPairInfo = new QueryableGroupNameCountPairInfo()
          {
            Type = queryableGroupNameCountPairType,
            ConstructorInfo = constructorInfo,
            KeyPropertyInfo = keyPropertyInfo,
            CountPropertyInfo = countPropertyInfo
          };

          QueryableGroupNameCountPairInfos.ReflectionInfoDictionary.Add( sourceType, queryableGroupNameCountPairInfo );
        }

        return queryableGroupNameCountPairInfo;
      }

      private static Dictionary<Type, QueryableGroupNameCountPairInfo> ReflectionInfoDictionary;
    }

    private struct QueryableGroupNameCountPairInfo
    {
      public Type Type
      {
        get;
        set;
      }

      public ConstructorInfo ConstructorInfo
      {
        get;
        set;
      }

      public PropertyInfo KeyPropertyInfo
      {
        get;
        set;
      }

      public PropertyInfo CountPropertyInfo
      {
        get;
        set;
      }
    }

    #endregion PRIVATE NESTED CLASSES

    #region PRIVATE NESTED ENUMS

    private enum StringFilterMode
    {
      StartsWith = 0,
      Contains = 1,
      EndsWith = 2
    }

    #endregion PRIVATE NESTED ENUMS
  }
}
