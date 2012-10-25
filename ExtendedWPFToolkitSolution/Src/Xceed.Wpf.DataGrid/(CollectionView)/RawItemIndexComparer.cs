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
