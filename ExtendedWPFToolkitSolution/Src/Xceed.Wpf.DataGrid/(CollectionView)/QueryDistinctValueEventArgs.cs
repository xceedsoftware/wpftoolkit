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
  public class QueryDistinctValueEventArgs : EventArgs
  {
    internal QueryDistinctValueEventArgs( object dataSourceValue )
    {
      this.DataSourceValue = dataSourceValue;

      // Initialize the DistinctValue as the original DataSourceValue
      // in case it is not modified during the QueryDistinctValue event 
      this.DistinctValue = dataSourceValue;
    }

    public object DataSourceValue
    {
      get;
      private set;
    }

    public object DistinctValue
    {
      get;
      set;
    }
  }
}
