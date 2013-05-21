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
using System.Linq;
using System.Text;
using System.Collections;

namespace Xceed.Wpf.DataGrid
{
  internal class ObjectComparer : IComparer
  {
    private ObjectComparer()
    {
    }

    #region Singleton Property

    public static ObjectComparer Singleton
    {
      get
      {
        if( _singleton == null )
          _singleton = new ObjectComparer();

        return _singleton;
      }
    }

    private static ObjectComparer _singleton;

    #endregion Singleton Property

    public int Compare( object xData, object yData )
    {
      // Code in there should be indentical to ObjectDataStore.CompareData

      if( ( xData == null ) || ( xData == DBNull.Value ) )
      {
        if( ( yData != null ) && ( yData != DBNull.Value ) )
        {
          return -1;
        }
      }
      else
      {
        if( ( yData == null ) || ( yData == DBNull.Value ) )
          return 1;

        IComparable xDataComparer = xData as IComparable;

        if( xDataComparer != null )
          return xDataComparer.CompareTo( yData );
      }

      return 0;
    }
  }
}
