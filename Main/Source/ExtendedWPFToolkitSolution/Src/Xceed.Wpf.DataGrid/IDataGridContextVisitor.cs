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
using System.Text;
using System.Windows.Data;
using System.Windows;

namespace Xceed.Wpf.DataGrid
{
  internal interface IDataGridContextVisitor
  {
    void Visit( DataGridContext sourceContext, ref bool stopVisit );
    void Visit( DataGridContext sourceContext, int sourceDataItemIndex, object item, ref bool stopVisit );
    void Visit( DataGridContext sourceContext, int startSourceDataItemIndex, int endSourceDataItemIndex, ref bool stopVisit );
    void Visit( DataGridContext sourceContext, CollectionViewGroup group, object[] namesTree, int groupLevel, bool isExpanded, bool isComputedExpanded, ref bool stopVisit );
    void Visit( DataGridContext sourceContext, DataTemplate headerFooter, ref bool stopVisit );
    void Visit( DataGridContext sourceContext, GroupHeaderFooterItem groupHeaderFooter, ref bool stopVisit );
  }
}
