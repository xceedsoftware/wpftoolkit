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
  internal class StatResultComparer : IComparer
  {
    private StatResultComparer()
    {
    }

    #region Singleton Property

    public static StatResultComparer Singleton
    {
      get
      {
        if( _singleton == null )
          _singleton = new StatResultComparer();

        return _singleton;
      }
    }

    private static StatResultComparer _singleton;

    #endregion Singleton Property

    public int Compare( object x, object y )
    {
      bool xIsExceptionOrNull = ( x == null ) || ( x == DBNull.Value ) || ( x is Exception );
      bool yIsExceptionOrNull = ( y == null ) || ( y == DBNull.Value ) || ( y is Exception );

      if( xIsExceptionOrNull )
      {
        if( yIsExceptionOrNull )
        {
          return 0;
        }
        else
        {
          return -1;
        }
      }
      else
      {
        if( yIsExceptionOrNull )
          return 1;

        IComparable xComparer = x as IComparable;

        if( xComparer != null )
          return xComparer.CompareTo( y );

        return 0;
      }
    }
  }
}
