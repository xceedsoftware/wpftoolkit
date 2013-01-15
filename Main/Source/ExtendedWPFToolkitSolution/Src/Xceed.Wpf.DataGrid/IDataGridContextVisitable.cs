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

namespace Xceed.Wpf.DataGrid
{
  internal interface IDataGridContextVisitable
  {
    void AcceptVisitor( IDataGridContextVisitor visitor, out bool visitWasStopped );
    void AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, out bool visitWasStopped );
    void AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, DataGridContextVisitorType visitorType, out bool visitWasStopped );
    void AcceptVisitor( int minIndex, int maxIndex, IDataGridContextVisitor visitor, DataGridContextVisitorType visitorType, bool visitDetails, out bool visitWasStopped );
  }
}
