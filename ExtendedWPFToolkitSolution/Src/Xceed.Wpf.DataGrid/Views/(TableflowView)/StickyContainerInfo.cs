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
using System.Windows;

namespace Xceed.Wpf.DataGrid.Views
{
  internal class StickyContainerInfo 
  {
    #region CONSTRUCTORS

    public StickyContainerInfo( UIElement container, int containerIndex, int holdingContainerIndex )
    {
      if( container == null )
        throw new ArgumentNullException( "container" );

      this.Container = container;
      this.ContainerIndex = containerIndex;
      this.HoldingContainerIndex = holdingContainerIndex;
    }

    #endregion CONSTRUCTORS

    #region Container Property

    public UIElement Container
    {
      get;
      private set;
    }

    #endregion Container Property

    #region ContainerIndex Property

    public int ContainerIndex
    {
      get;
      private set;
    }

    #endregion ContainerIndex Property

    #region HoldingContainerIndex Property

    public int HoldingContainerIndex
    {
      get;
      private set;
    }

    #endregion HoldingContainerIndex Property
  }
}
