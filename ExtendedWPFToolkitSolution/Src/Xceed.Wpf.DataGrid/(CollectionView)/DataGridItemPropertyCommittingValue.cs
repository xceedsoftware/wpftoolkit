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
  public class DataGridItemPropertyCommittingValueEventArgs : EventArgs
  {
    public DataGridItemPropertyCommittingValueEventArgs( object item, object value )
    {
      this.Item = item;
      this.Value = value;
    }

    public object Item { get; private set; }
    public object Value { get; private set; }
  }
}
