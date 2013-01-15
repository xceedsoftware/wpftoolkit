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
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class AsyncQueryInfoWeakComparer : IEqualityComparer<AsyncQueryInfo>
  {
    #region IEqualityComparer<AsyncQueryInfo> Members

    public bool Equals( AsyncQueryInfo x, AsyncQueryInfo y )
    {
      if( ( x == null ) && ( y == null ) )
        return true;

      if( ( x == null ) || ( y == null ) )
        return false;


      if( ( x.StartIndex == y.StartIndex ) && ( x.RequestedItemCount == y.RequestedItemCount ) )
        return true;

      return false;
    }

    public int GetHashCode( AsyncQueryInfo obj )
    {
      if( obj == null )
        return 0;

      return obj.StartIndex ^ obj.RequestedItemCount;
    }

    #endregion IEqualityComparer<AsyncQueryInfo> Members
  }
}
