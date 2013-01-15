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
