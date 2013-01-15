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
using System.Windows;
using System.Collections.Generic;

namespace Xceed.Wpf.DataGrid.Views
{
  internal interface IVirtualizingCellsHost
  {
    // Determines if it is possible for this CellsHost to internaly set/clear the LogicalParent
    // to avoid the UIElementCollection to do it in background for each Collection modification
    bool CanModifyLogicalParent
    {
      get;
    }

    void SetLogicalParent( Cell cell );

    void PrepareCellsHost( DataGridContext dataGridContext );

    void ClearCellsHost();

    void InvalidateCellsHostMeasure();

    bool BringIntoView( Cell cell );
  }
}
