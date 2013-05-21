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
