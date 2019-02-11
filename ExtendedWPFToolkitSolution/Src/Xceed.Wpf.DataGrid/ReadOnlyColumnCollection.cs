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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Xceed.Wpf.DataGrid
{
  internal class ReadOnlyColumnCollection : ReadOnlyObservableCollection<ColumnBase>
  {
    internal ReadOnlyColumnCollection( ObservableColumnCollection collection )
      : base( collection )
    {
    }

    #region [] Property

    internal ColumnBase this[ string fieldName ]
    {
      get
      {
        return ( ( ObservableColumnCollection )this.Items )[ fieldName ];
      }
    }

    #endregion

    internal void RaiseItemChanged( ColumnBase column )
    {
      Debug.Assert( column != null );

      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, column, column, this.IndexOf( column ) ) );
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

    internal IDisposable DeferNotifications()
    {
      return ( ( ObservableColumnCollection )this.Items ).DeferNotifications();
    }
  }
}
