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
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class RawItemIndexComparer : IComparer<RawItem>
  {
    public RawItemIndexComparer()
    {
    }

    #region IComparer<RawItem> Members

    public int Compare( RawItem x, RawItem y )
    {
      if( x == null )
      {
        if( y == null )
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
        if( y == null )
        {
          return 1;
        }
      }

      return x.Index - y.Index;
    }

    #endregion
  }
}
