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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Xceed.Wpf.DataGrid.Utils;

namespace Xceed.Wpf.DataGrid
{
  public class SortDirectionCycleCollection : Collection<SortDirection>, INotifyCollectionChanged, INotifyPropertyChanged
  {
    #region Static Fields

    internal static readonly string CountPropertyName = PropertyHelper.GetPropertyName( ( SortDirectionCycleCollection s ) => s.Count );
    internal static readonly string IndexerName = "Item[]";

    #endregion

    public SortDirectionCycleCollection()
      : base( new List<SortDirection>() )
    {
    }

    internal SortDirectionCycleCollection( IList<SortDirection> list )
      : base( list )
    {
    }

    protected override void ClearItems()
    {
      if( this.Count == 0 )
        return;

      base.ClearItems();
      this.OnPropertyChanged( SortDirectionCycleCollection.CountPropertyName );
      this.OnPropertyChanged( SortDirectionCycleCollection.IndexerName );
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    protected override void InsertItem( int index, SortDirection item )
    {
      this.EnsureUnique( item );
      base.InsertItem( index, item );
      this.OnPropertyChanged( SortDirectionCycleCollection.CountPropertyName );
      this.OnPropertyChanged( SortDirectionCycleCollection.IndexerName );
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Add, item, index ) );
    }

    protected override void RemoveItem( int index )
    {
      var item = base[ index ];

      base.RemoveItem( index );
      this.OnPropertyChanged( SortDirectionCycleCollection.CountPropertyName );
      this.OnPropertyChanged( SortDirectionCycleCollection.IndexerName );
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Remove, item, index ) );
    }

    protected override void SetItem( int index, SortDirection item )
    {
      var oldItem = base[ index ];
      if( oldItem == item )
        return;

      this.EnsureUnique( item );
      base.SetItem( index, item );
      this.OnPropertyChanged( SortDirectionCycleCollection.IndexerName );
      this.OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Replace, item, oldItem, index ) );
    }

    private void EnsureUnique( SortDirection item )
    {
      if( this.Contains( item ) )
        throw new ArgumentException( "The SortDirection is already contained in the collection and cannot be added again.", "item" );
    }

    #region INotifyCollectionChanged Members

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    protected virtual void OnCollectionChanged( NotifyCollectionChangedEventArgs e )
    {
      var handler = this.CollectionChanged;
      if( handler == null )
        return;

      handler.Invoke( this, e );
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged( string propertyName )
    {
      var handler = this.PropertyChanged;
      if( handler == null )
        return;

      handler.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
    }

    #endregion
  }
}
