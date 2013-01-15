/************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2010-2012 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus edition at http://xceed.com/wpf_toolkit

   Visit http://xceed.com and follow @datagrid on Twitter

  **********************************************************************/

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
    private static readonly string CategoryPropertyName;
    private static readonly string CategoryOrderPropertyName;
    private static readonly string PropertyOrderPropertyName;
    private static readonly string DisplayNamePropertyName;

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

    internal void Update( IEnumerable<PropertyItem> newItems,  bool isCategorized, string filter )
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
          GroupBy( CategoryPropertyName );
          SortBy( CategoryPropertyName, ListSortDirection.Ascending );
        }

        SortBy( PropertyOrderPropertyName, ListSortDirection.Ascending );
        SortBy( DisplayNamePropertyName, ListSortDirection.Ascending );

        Filter( filter );

        _preventNotification = false;
        OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );
      }
    }

    internal void RefreshView()
    {
      GetDefaultView().Refresh();
    }

    internal static bool IsItemOrderingProperty( string propertyName )
    {
      return string.Equals( propertyName, DisplayNamePropertyName )
        || string.Equals( propertyName, CategoryPropertyName )
        || string.Equals( propertyName, CategoryOrderPropertyName )
        || string.Equals( propertyName, PropertyOrderPropertyName );
    }
  }
}
