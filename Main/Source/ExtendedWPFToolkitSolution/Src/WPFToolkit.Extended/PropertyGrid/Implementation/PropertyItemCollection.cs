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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Collections.Specialized;
using System.Diagnostics;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyItemCollection : ReadOnlyObservableCollection<PropertyItem>
  {
    private bool _preventNotification;

    public PropertyItemCollection(ObservableCollection<PropertyItem> editableCollection)
      :base(editableCollection)
    {
      EditableCollection = editableCollection;
    }

    public ObservableCollection<PropertyItem> EditableCollection { get; private set; }

    private ICollectionView GetDefaultView()
    {
      return CollectionViewSource.GetDefaultView( this );
    }

    public void GroupBy( string name )
    {
      GetDefaultView().GroupDescriptions.Add( new PropertyGroupDescription( name ) );
    }

    public void SortBy( string name, ListSortDirection sortDirection )
    {
      GetDefaultView().SortDescriptions.Add( new SortDescription( name, sortDirection ) );
    }

    public void Filter( string text )
    {
      if( text == null )
        return;

      GetDefaultView().Filter = ( item ) =>
      {
        var property = item as PropertyItem;
        return property.DisplayName.ToLower().StartsWith( text.ToLower() );
      };
    }

    protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs args )
    {
      if( _preventNotification )
        return;

      base.OnCollectionChanged( args );
    }

    internal void Update( IEnumerable<PropertyItem> newItems, bool isCategorized, string filter )
    {
      using( GetDefaultView().DeferRefresh() )
      {
        _preventNotification = true;

        // Replace the collection content with the new items.
        if( newItems != null )
        {
          EditableCollection.Clear();
          foreach( var item in newItems )
          {
            this.EditableCollection.Add( item );
          }
        }

        // Clear view values
        ICollectionView view = this.GetDefaultView();
        view.GroupDescriptions.Clear();
        view.SortDescriptions.Clear();
        view.Filter = null;

        // Update view values
        if( isCategorized )
        {
          GroupBy( "Category" );
          SortBy( "Category", ListSortDirection.Ascending );
        }

        SortBy( "PropertyOrder", ListSortDirection.Ascending );
        SortBy( "DisplayName", ListSortDirection.Ascending );

        Filter( filter );

        _preventNotification = false;
        OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
      }
    }
  }
}
