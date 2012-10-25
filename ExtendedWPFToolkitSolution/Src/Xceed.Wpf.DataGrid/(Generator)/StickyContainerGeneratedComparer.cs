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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class StickyContainerGeneratedComparer : IComparer<StickyContainerGenerated>
  {
    #region Singleton Property

    public static StickyContainerGeneratedComparer Singleton
    {
      get
      {
        if( _singleton == null )
          _singleton = new StickyContainerGeneratedComparer();

        return _singleton;
      }
    }

    private static StickyContainerGeneratedComparer _singleton;

    #endregion Singleton Property

    #region IComparer<StickyContainerGenerated> Members

    public int Compare( StickyContainerGenerated x, StickyContainerGenerated y )
    {
      return x.Index.CompareTo( y.Index );
    }

    #endregion IComparer<StickyContainerGenerated> Members
  }
}
