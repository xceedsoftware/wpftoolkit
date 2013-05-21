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
  public class DataGridRemovingItemEventArgs : DataGridItemCancelEventArgs
  {
    public DataGridRemovingItemEventArgs( DataGridCollectionViewBase collectionView, object item, int index, bool cancel)
      : base( collectionView, item, cancel )
    {
      m_index = index;
    }

    #region Index Property

    public int Index
    {
      get { return m_index; }
    }

    private int m_index;

    #endregion Index Property
  }
}
