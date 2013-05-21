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
  internal class CustomDistinctValueItemFilterEventArgs : EventArgs
  {
    internal CustomDistinctValueItemFilterEventArgs( object title, object value )
    {
      this.Title = title;
      this.Value = value;
    }

    public bool Accepted
    {
      get;
      set;
    }

    public object Title
    {
      get;
      private set;
    }

    public object Value
    {
      get;
      private set;
    }
  }
}
