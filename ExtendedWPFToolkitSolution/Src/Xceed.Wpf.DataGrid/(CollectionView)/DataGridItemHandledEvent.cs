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

namespace Xceed.Wpf.DataGrid
{
  public class DataGridItemHandledEventArgs : DataGridItemEventArgs
  {
    public DataGridItemHandledEventArgs(
      DataGridCollectionViewBase collectionView,
      object item )
      : base( collectionView, item )
    {
    }

    #region Handled Property

    public bool Handled
    {
      get
      {
        return m_handled;
      }
      set
      {
        m_handled = value;
      }
    }

    private bool m_handled;

    #endregion Handled Property
  }
}
