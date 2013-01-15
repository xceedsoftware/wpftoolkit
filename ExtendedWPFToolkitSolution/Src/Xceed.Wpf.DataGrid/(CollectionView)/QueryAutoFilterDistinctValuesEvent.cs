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
using System.Linq;
using System.Text;

namespace Xceed.Wpf.DataGrid
{
  internal class QueryAutoFilterDistinctValuesEventArgs : EventArgs
  {
    internal QueryAutoFilterDistinctValuesEventArgs( DataGridItemPropertyBase itemProperty )
    {
      m_itemProperty = itemProperty;
      m_distinctValues = new List<object>();
    }

    public DataGridItemPropertyBase ItemProperty
    {
      get
      {
        return m_itemProperty;
      }
    }

    public List<object> DistinctValues
    {
      get
      {
        return m_distinctValues;
      }
    }

    #region PRIVATE FIELDS

    private DataGridItemPropertyBase m_itemProperty;
    private List<object> m_distinctValues;

    #endregion PRIVATE FIELDS
  }
}
