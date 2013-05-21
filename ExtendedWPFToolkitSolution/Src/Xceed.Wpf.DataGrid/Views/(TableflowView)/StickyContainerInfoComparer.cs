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

namespace Xceed.Wpf.DataGrid.Views
{
  internal class StickyContainerInfoComparer : IComparer<StickyContainerInfo>
  {
    #region Singleton Property

    public static StickyContainerInfoComparer Singleton
    {
      get
      {
        if( _singleton == null )
          _singleton = new StickyContainerInfoComparer();

        return _singleton;
      }
    }

    private static StickyContainerInfoComparer _singleton;

    #endregion Singleton Property

    #region IComparer<StickyContainerInfo> Members

    public int Compare( StickyContainerInfo x, StickyContainerInfo y )
    {
      return x.ContainerIndex.CompareTo( y.ContainerIndex );
    }

    #endregion IComparer<StickyContainerInfo> Members
  }
}
