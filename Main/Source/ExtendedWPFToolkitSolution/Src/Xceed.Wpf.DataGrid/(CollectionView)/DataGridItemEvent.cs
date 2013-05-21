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
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  public class DataGridItemEventArgs : EventArgs
  {
    public DataGridItemEventArgs(
      DataGridCollectionViewBase collectionView,
      object item )
    {
      m_collectionView = collectionView;
      m_item = item;
    }

    #region CollectionView Property

    public DataGridCollectionViewBase CollectionView
    {
      get { return m_collectionView; }
    }

    private DataGridCollectionViewBase m_collectionView;

    #endregion CollectionView Property

    #region Item Property

    public object Item
    {
      get
      {
        return m_item;
      }
    }

    private object m_item;

    #endregion Item Property
  }
}
