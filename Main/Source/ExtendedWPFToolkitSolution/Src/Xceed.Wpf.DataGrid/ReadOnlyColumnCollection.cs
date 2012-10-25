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
