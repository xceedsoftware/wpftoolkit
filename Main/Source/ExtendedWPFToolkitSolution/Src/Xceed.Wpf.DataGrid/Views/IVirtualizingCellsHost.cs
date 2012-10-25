/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   This program can be provided to you by Xceed Software Inc. under a
   proprietary commercial license agreement for use in non-Open Source
   projects. The commercial version of Extended WPF Toolkit also includes
   priority technical support, commercial updates, and many additional 
   useful WPF controls if you license Xceed Business Suite for WPF.

   Visit http://xceed.com and follow @datagrid on Twitter.

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
