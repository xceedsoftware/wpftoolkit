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
  internal class LayoutedContainerInfo : IComparable<LayoutedContainerInfo>
  {
    #region CONSTRUCTORS

    public LayoutedContainerInfo( int realizedIndex, UIElement container )
    {
      if( container == null )
        throw new ArgumentNullException( "container" );

      this.RealizedIndex = realizedIndex;
      this.Container = container;
    }

    #endregion CONSTRUCTORS

    #region RealizedIndex Property

    public int RealizedIndex
    {
      get;
      private set;
    }

    #endregion RealizedIndex Property

    #region Container Property

    public UIElement Container
    {
      get;
      private set;
    }

    #endregion Container Property

    #region IComparable<LayoutedContainerInfo> Members

    public int CompareTo( LayoutedContainerInfo other )
    {
      if( other == null )
        return -1;

      return this.RealizedIndex.CompareTo( other.RealizedIndex );
    }

    #endregion IComparable<LayoutedContainerInfo> Members
  }
}
