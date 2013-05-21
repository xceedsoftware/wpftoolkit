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
