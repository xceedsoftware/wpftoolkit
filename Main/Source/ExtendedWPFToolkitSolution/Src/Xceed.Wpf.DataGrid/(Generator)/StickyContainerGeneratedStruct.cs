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

namespace Xceed.Wpf.DataGrid
{
  internal struct StickyContainerGenerated
  {
    #region CONSTRUCTORS

    public StickyContainerGenerated( DependencyObject container, int index, bool isNewlyRealized )
      : this()
    {
      this.StickyContainer = container;
      this.Index = index;
      this.IsNewlyRealized = isNewlyRealized;
    }

    #endregion CONSTRUCTORS

    #region StickyContainer Property

    public DependencyObject StickyContainer
    {
      get;
      private set;
    }

    #endregion StickyContainer Property

    #region IsNewlyRealized Property

    public bool IsNewlyRealized
    {
      get;
      private set;
    }

    #endregion IsNewlyRealized Property

    #region Index Property

    public int Index
    {
      get;
      set;
    }

    #endregion Index Property

    #region Overrides

    public override bool Equals( object obj )
    {
      return object.Equals( obj, this.StickyContainer );
    }

    public override int GetHashCode()
    {
      return this.StickyContainer.GetHashCode();
    }

    #endregion Overrides
  }
}
