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
using System.Collections.ObjectModel;
using System.Text;
using System.Collections.Specialized;

namespace Xceed.Wpf.DataGrid
{
  internal class ReadOnlyColumnCollection : ReadOnlyObservableCollection<ColumnBase>
  {
    public ReadOnlyColumnCollection()
      : base( new ObservableCollection<ColumnBase>() )
    {
    }

    internal void RaiseItemChanged( ColumnBase column )
    {
      this.OnCollectionChanged( 
        new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, column, column, this.IndexOf( column ) ) );
    }

    internal void InternalClear()
    {
      this.Items.Clear();
    }

    internal void InternalAdd( ColumnBase column )
    {
      this.Items.Add( column );
    }

    internal void InternalInsert( int index, ColumnBase column )
    {
      this.Items.Insert( index, column );
    }

    internal bool InternalRemove( ColumnBase column )
    {
      return this.Items.Remove( column );
    }

    internal void InternalRemoveAt( int index )
    {
      this.Items.RemoveAt( index );
    }
  }
}
