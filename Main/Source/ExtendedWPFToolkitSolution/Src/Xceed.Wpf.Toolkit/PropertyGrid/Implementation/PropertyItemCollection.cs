/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using System.Linq;
using System.Collections;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace Xceed.Wpf.Toolkit.PropertyGrid
{
  public class PropertyItemCollection : ReadOnlyObservableCollection<PropertyItem>
  {
    internal static readonly string CategoryPropertyName;
    internal static readonly string CategoryOrderPropertyName;
    internal static readonly string PropertyOrderPropertyName;
    internal static readonly string DisplayNamePropertyName;

    private bool _preventNotification;

    static PropertyItemCollection()
    {
      PropertyItem p = null;
      CategoryPropertyName = ReflectionHelper.GetPropertyOrFieldName( () => p.Category );
      CategoryOrderPropertyName = ReflectionHelper.GetPropertyOrFieldName( () => p.CategoryOrder );
      PropertyOrderPropertyName = ReflectionHelper.GetPropertyOrFieldName( () => p.PropertyOrder );
      DisplayNamePropertyName = ReflectionHelper.GetPropertyOrFieldName( () => p.DisplayName );
    }

    public PropertyItemCollection(ObservableCollection<PropertyItem> editableCollection)
      :base(editableCollection)
    {
      EditableCollection = editableCollection;
    }

    internal Predicate<object> FilterPredicate
    {
      get { return GetDefaultView().Filter; }
      set { GetDefaultView().Filter = value; }
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
      Predicate<object> filter = PropertyItemCollection.CreateFilter( text );
      GetDefaultView().Filter = filter;
    }

    protected override void OnCollectionChanged( NotifyCollectionChangedEventArgs args )
    {
      if( _preventNotification )
        return;

      base.OnCollectionChanged( args );
    }

    internal void UpdateItems( IEnumerable<PropertyItem> newItems )
    {
      if( newItems == null )
        throw new ArgumentNullException( "newItems" );

      _preventNotification = true;
      using( GetDefaultView().DeferRefresh() )
      {
        EditableCollection.Clear();
        foreach( var item in newItems )
        {
          this.EditableCollection.Add( item );
        }
      }
      _preventNotification = false;
      OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
    }

    internal void UpdateCategorization( GroupDescription groupDescription )
    {
      // Clear view values
      ICollectionView view = this.GetDefaultView();
      using( view.DeferRefresh() )
      {
        view.GroupDescriptions.Clear();
        view.SortDescriptions.Clear();

        // Update view values
        if( groupDescription != null )
        {
          view.GroupDescriptions.Add( groupDescription );
          SortBy( CategoryPropertyName, ListSortDirection.Ascending );
        }

        SortBy( PropertyOrderPropertyName, ListSortDirection.Ascending );
        SortBy( DisplayNamePropertyName, ListSortDirection.Ascending );
      }
    }

    internal void RefreshView()
    {
      GetDefaultView().Refresh();
    }

    internal static Predicate<object> CreateFilter( string text )
    {
      Predicate<object> filter = null;

      if( !string.IsNullOrEmpty( text ) )
      {
        filter = ( item ) =>
        {
          var property = item as PropertyItem;
          if( property.DisplayName != null )
          {
            return property.DisplayName.ToLower().StartsWith( text.ToLower() );
          }

          return false;
        };
      }

      return filter;
    }
  }
}
