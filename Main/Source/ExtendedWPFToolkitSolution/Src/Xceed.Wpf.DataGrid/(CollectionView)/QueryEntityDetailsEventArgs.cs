﻿/************************************************************************

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
using System.Windows;
using System.Data.Objects.DataClasses;

namespace Xceed.Wpf.DataGrid
{
  public class QueryEntityDetailsEventArgs : EventArgs
  {
    #region CONSTRUCTORS

    internal QueryEntityDetailsEventArgs( EntityObject parentItem )
      : base()
    {
      if( parentItem == null )
        throw new ArgumentNullException( "parentItem" );

      this.ParentItem = parentItem;
    }

    #endregion CONSTRUCTORS

    #region ParentItem Property

    public EntityObject ParentItem
    {
      get;
      private set;
    }

    #endregion ParentItem Property

    #region Handled Property

    public bool Handled
    {
      get;
      set;
    }

    #endregion Handled Property
  }
}
