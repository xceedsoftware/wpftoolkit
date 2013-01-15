/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Xceed.Wpf.DataGrid
{
  [Obsolete( "The AddingNewDataItem event is obsolete and has been replaced by the DataGridCollectionView.InsertingNewItem and InitializingNewItem events.", false )]
  [Browsable( false )]
  [EditorBrowsable( EditorBrowsableState.Never )]
  public class AddingNewDataItemEventArgs : EventArgs
  {
    public AddingNewDataItemEventArgs()
    {
    }

    public object DataItem
    {
      get
      {
        return m_dataItem;
      }
      set
      {
        m_dataItem = value;
      }
    }

    private object m_dataItem;
  }
}
