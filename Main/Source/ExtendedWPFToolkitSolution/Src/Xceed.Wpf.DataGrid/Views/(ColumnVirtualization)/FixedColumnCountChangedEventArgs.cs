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

namespace Xceed.Wpf.DataGrid.Views
{
  internal class FixedColumnCountInfoEventArgs : EventArgs
  {
    internal FixedColumnCountInfoEventArgs( ColumnBase triggeringColumn, FixedColumnUpdateType updateType, int level = 0 )
    {
      this.TriggeringColumn = triggeringColumn;
      this.UpdateType = updateType;
      this.Level = level;
    }

    internal ColumnBase TriggeringColumn
    {
      get;
      private set;
    }

    internal FixedColumnUpdateType UpdateType
    {
      get;
      private set;
    }

    internal int Level
    {
      get;
      private set;
    }
  }

  internal enum FixedColumnUpdateType
  {
    Hide,  //Column.Visible is set to false
    Remove, //The column is removed from a MergedColumnCollection
    Show, //Column.Visible is set to true
    Update //Simply update the FixedColumnCount property after a change in a ChildColumns collection.
  }

}
