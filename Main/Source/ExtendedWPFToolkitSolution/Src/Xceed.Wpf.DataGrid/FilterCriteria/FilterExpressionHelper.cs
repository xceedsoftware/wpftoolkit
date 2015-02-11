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

namespace Xceed.Wpf.DataGrid.FilterCriteria
{
  internal class FilterExpressionHelper
  {
    #region LastValue Property

    internal static long LastValue
    {
      get;
      set;
    }

    #endregion

    #region IsConsecutive Property

    internal static bool IsConsecutive
    {
      get;
      set;
    }

    #endregion

    #region GreaterThanAdded Property

    internal static bool GreaterThanAdded
    {
      get;
      set;
    }

    #endregion

    internal static string BuildExpressionFromIntValue( string expression, long currentValue )
    {
      //If we have an int value, let's try to find ranges of values, in oreder to build a more efficent expression than having all values separated by an OR criterion.
      if( currentValue - 1 < FilterExpressionHelper.LastValue )
      {
        expression = currentValue.ToString();
      }
      else if( currentValue - 1 > FilterExpressionHelper.LastValue )
      {
        if( FilterExpressionHelper.IsConsecutive )
        {
          expression += " AND <" + ( FilterExpressionHelper.LastValue + 1 ).ToString() + " OR " + currentValue.ToString();
          FilterExpressionHelper.IsConsecutive = false;
        }
        else
        {
          expression += " OR " + currentValue.ToString();
        }
        FilterExpressionHelper.GreaterThanAdded = false;
      }
      else if( currentValue - 1 == FilterExpressionHelper.LastValue )
      {
        if( !FilterExpressionHelper.GreaterThanAdded )
        {
          expression = expression.Remove( expression.IndexOf( FilterExpressionHelper.LastValue.ToString() ) );
          expression += ">" + ( FilterExpressionHelper.LastValue - 1 ).ToString();
          FilterExpressionHelper.GreaterThanAdded = true;
        }
        FilterExpressionHelper.IsConsecutive = true;
      }

      FilterExpressionHelper.LastValue = currentValue;

      return expression;
    }

    internal static bool GetIsIntValuePathType( Type valuePathDataType )
    {
      return valuePathDataType == typeof( sbyte ) ||
             valuePathDataType == typeof( byte ) ||
             valuePathDataType == typeof( short ) ||
             valuePathDataType == typeof( ushort ) ||
             valuePathDataType == typeof( int ) ||
             valuePathDataType == typeof( uint ) ||
             valuePathDataType == typeof( long ) ||
             valuePathDataType == typeof( ulong );
    }
  }

  internal class EmptyCriterionObject
  {
    internal EmptyCriterionObject()
    {
    }
  }
}
