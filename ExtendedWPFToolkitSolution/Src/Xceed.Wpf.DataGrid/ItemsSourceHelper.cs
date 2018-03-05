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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.Objects.DataClasses;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using Xceed.Wpf.DataGrid.Converters;
using Xceed.Wpf.DataGrid.ValidationRules;

namespace Xceed.Wpf.DataGrid
{
  internal static class ItemsSourceHelper
  {
    #region Static Fields

    private static readonly Type EntityObjectType = Type.GetType(
      "System.Data.Objects.DataClasses.EntityObject, System.Data.Entity, Version=" + _XceedVersionInfo.FrameworkVersion + ", Culture=neutral, PublicKeyToken=b77a5c561934e089",
      false, false );

    #endregion

    internal static bool IsSourceSupportingChangeNotification( object source )
    {
      if( source is INotifyCollectionChanged )
        return true;

      var collection = source as IBindingList;

      return ( collection != null )
          && ( collection.SupportsChangeNotification );
    }

    private static bool IsSupportingDBNull( IEnumerable source )
    {
      // return false because all the DBNull conversion should have been done in the DataGridCollectionView.ItemProperties converter.
      if( source is DataGridCollectionView )
        return false;

      if( source is DataView )
        return true;

      var collectionView = source as CollectionView;
      if( collectionView != null )
        return ItemsSourceHelper.IsSupportingDBNull( collectionView.SourceCollection );

      return false;
    }

    internal static bool IsItemSupportingDBNull( object source )
    {
      return ( source is System.Data.DataRow )
          || ( source is DataRowView );
    }

    internal static bool IsDataView( IEnumerable source )
    {
      if( source is DataGridCollectionView )
        return false;

      if( source is DataView )
        return true;

      var collectionView = source as CollectionView;

      return ( collectionView != null )
          && ( collectionView.SourceCollection is DataView );
    }

    internal static IList TryGetIList( IEnumerable itemsSourceCollection )
    {
      var collectionView = itemsSourceCollection as ItemCollection;
      if( collectionView != null )
      {
        var list = collectionView.SourceCollection as IList;
        if( list != null )
          return list;
      }

      return itemsSourceCollection as IList;
    }

    private static bool IsValueType( Type itemType )
    {
      if( typeof( string ) == itemType )
        return true;

      if( !itemType.IsValueType )
        return false;

      return ( itemType.IsPrimitive )
          || ( typeof( decimal ) == itemType )
          || ( typeof( DateTime ) == itemType )
          || ( typeof( TimeSpan ) == itemType )
          || ( typeof( Guid ) == itemType );
    }

    internal static bool IsEntityObjectLoadable( EntityObject entityObject )
    {
      return ( ( entityObject.EntityState & EntityState.Added ) != EntityState.Added )
          && ( ( entityObject.EntityState & EntityState.Detached ) != EntityState.Detached );
    }

    private static bool IsEntityFramework( Type itemType )
    {
      var entityObjectType = ItemsSourceHelper.EntityObjectType;

      // The EntityFramework assembly is not loaded.  We are running on the client framework.
      return ( entityObjectType != null )
          && ( entityObjectType.IsAssignableFrom( itemType ) );
    }

    internal static bool IsEntityFramework( object source )
    {
      return ( source != null )
          && ( ItemsSourceHelper.IsEntityFramework( source.GetType() ) );
    }

    internal static object GetFirstItemByEnumerable( IEnumerable source )
    {
      if( source == null )
        return null;

      return source.Cast<object>().FirstOrDefault();
    }

    internal static DataGridItemPropertyCollection GetRootCollection( DataGridItemPropertyCollection collection )
    {
      if( collection == null )
        return null;

      var owner = collection.Owner;
      if( owner == null )
        return collection;

      return ItemsSourceHelper.GetRootCollection( owner );
    }

    internal static DataGridItemPropertyCollection GetRootCollection( DataGridItemPropertyBase itemProperty )
    {
      if( itemProperty == null )
        return null;

      return ItemsSourceHelper.GetRootCollection( itemProperty.ContainingCollection );
    }

    internal static object AddNewDataItem( IEnumerable itemsSourceCollection, DataGridControl dataGridControl, out int itemIndex )
    {
      var dataGridCollectionViewBase = itemsSourceCollection as DataGridCollectionViewBase;
      if( dataGridCollectionViewBase != null )
      {
        if( !dataGridCollectionViewBase.CanAddNew )
          throw new InvalidOperationException( "An attempt was made to add a new data item to a source that does not support insertion." );

        itemIndex = dataGridCollectionViewBase.Count;

        return dataGridCollectionViewBase.AddNew();
      }

      var newItem = default( object );

      if( ( dataGridControl != null ) && ( dataGridControl.ItemsSource == null ) )
      {
        //unbound
#pragma warning disable 618
        var eventArgs = new AddingNewDataItemEventArgs();
        dataGridControl.OnAddingNewDataItem( eventArgs );

        newItem = eventArgs.DataItem;
#pragma warning restore 618

        if( newItem == null )
          throw new InvalidOperationException( "The AddingNewDataItem event did not return a new data item because the grid is not bound to a data source." );

        itemIndex = dataGridControl.Items.Add( newItem );

        return newItem;
      }

      var dataView = itemsSourceCollection as DataView;
      if( dataView == null )
      {
        var collectionView = itemsSourceCollection as CollectionView;

        dataView = ( collectionView == null ) ? null : collectionView.SourceCollection as DataView;
      }

      if( dataView != null )
      {
        itemIndex = dataView.Count;

        return dataView.AddNew();
      }

      var bindingList = itemsSourceCollection as IBindingList;
      if( bindingList == null )
      {
        var collectionView = itemsSourceCollection as CollectionView;

        bindingList = ( collectionView == null ) ? null : collectionView.SourceCollection as IBindingList;
      }

      if( ( bindingList != null ) && ( bindingList.AllowNew ) )
      {
        itemIndex = bindingList.Count;
        newItem = bindingList.AddNew();
      }
      else
      {
        var itemType = ItemsSourceHelper.GetItemTypeFromEnumeration( itemsSourceCollection );
        if( itemType == null )
          throw new InvalidOperationException( "An attempt was made to use a source whose item type cannot be determined." );

        try
        {
          itemIndex = -1;
          newItem = Activator.CreateInstance( itemType );
        }
        catch( MissingMethodException exception )
        {
          throw new InvalidOperationException( "An attempt was made to use a source whose item type does not have a default constructor.", exception );
        }
        catch( Exception exception )
        {
          throw new InvalidOperationException( "An unsuccessful attempt was made to create an instance of the source's item type using the item's default constructor.", exception );
        }
      }

      var initializableObject = ItemsSourceHelper.GetSupportInitializeObject( newItem );
      if( initializableObject != null )
      {
        initializableObject.BeginInit();
      }

      var editableObject = ItemsSourceHelper.GetEditableObject( newItem );
      if( editableObject != null )
      {
        editableObject.BeginEdit();
      }

      return newItem;
    }

    internal static void CancelNewDataItem( IEnumerable itemsSourceCollection, DataGridControl dataGridControl, object newItem, int newItemIndex )
    {
      var dataGridCollectionViewBase = itemsSourceCollection as DataGridCollectionViewBase;
      if( dataGridCollectionViewBase != null )
      {
        if( dataGridCollectionViewBase.CurrentAddItem == newItem )
        {
          // The DataGridCollectionViewBase's CancelNew will take care of calling the item's CancelEdit if it must do so.
          dataGridCollectionViewBase.CancelNew();
        }

        return;
      }

      //if unbound
      if( ( dataGridControl != null ) && ( dataGridControl.ItemsSource == null ) )
        return;

      var editableObject = ItemsSourceHelper.GetEditableObject( newItem );
      if( editableObject != null )
      {
        editableObject.CancelEdit();
      }

      var initializableObject = ItemsSourceHelper.GetSupportInitializeObject( newItem );
      if( initializableObject != null )
      {
        initializableObject.EndInit();
      }

      if( ItemsSourceHelper.IsEditableObjectInsertedOrRemovedFromDataSourceAutomatically( editableObject ) )
        return;

      var cancelAddNew = itemsSourceCollection as ICancelAddNew;
      if( cancelAddNew == null )
      {
        var collectionView = itemsSourceCollection as CollectionView;

        cancelAddNew = ( collectionView == null ) ? null : collectionView.SourceCollection as ICancelAddNew;
      }

      if( cancelAddNew != null )
      {
        cancelAddNew.CancelNew( newItemIndex );
        return;
      }

      if( newItemIndex >= 0 )
      {
        var list = itemsSourceCollection as IList;
        if( list == null )
        {
          var collectionView = itemsSourceCollection as CollectionView;

          list = ( collectionView == null ) ? null : collectionView.SourceCollection as IList;
        }

        if( ( list != null ) && ( !list.IsFixedSize ) )
        {
          list.RemoveAt( newItemIndex );
        }
      }
    }

    internal static void EndNewDataItem( IEnumerable itemsSourceCollection, DataGridControl dataGridControl, object newItem, ref int newItemIndex )
    {
      var dataGridCollectionViewBase = itemsSourceCollection as DataGridCollectionViewBase;
      if( dataGridCollectionViewBase != null )
      {
        if( dataGridCollectionViewBase.CurrentAddItem == newItem )
        {
          // The DataGridCollectionViewBase's EndNew will take care of calling the item's EndEdit if it must do so.
          dataGridCollectionViewBase.CommitNew();
        }

        return;
      }

      //if unbound
      if( ( dataGridControl != null ) && ( dataGridControl.ItemsSource == null ) )
        return;

      var editableObject = ItemsSourceHelper.GetEditableObject( newItem );
      if( editableObject != null )
      {
        editableObject.EndEdit();
      }

      var initializableObject = ItemsSourceHelper.GetSupportInitializeObject( newItem );
      if( initializableObject != null )
      {
        initializableObject.EndInit();
      }

      if( ItemsSourceHelper.IsEditableObjectInsertedOrRemovedFromDataSourceAutomatically( editableObject ) )
        return;

      var cancelAddNew = itemsSourceCollection as ICancelAddNew;
      if( cancelAddNew == null )
      {
        var collectionView = itemsSourceCollection as CollectionView;

        cancelAddNew = ( collectionView == null ) ? null : collectionView.SourceCollection as ICancelAddNew;
      }

      if( cancelAddNew != null )
      {
        cancelAddNew.EndNew( newItemIndex );
        return;
      }

      var bindingList = itemsSourceCollection as IBindingList;
      if( bindingList == null )
      {
        var collectionView = itemsSourceCollection as CollectionView;

        bindingList = ( collectionView == null ) ? null : collectionView.SourceCollection as IBindingList;
      }

      // If the item is already added into the list by IBindingList.AddNew.
      if( ( bindingList != null ) && ( bindingList.AllowNew ) )
        return;

      var list = itemsSourceCollection as IList;
      if( list == null )
      {
        var collectionView = itemsSourceCollection as CollectionView;

        list = ( collectionView == null ) ? null : collectionView.SourceCollection as IList;
      }

      if( ( list == null ) || ( list.IsFixedSize ) )
        throw new InvalidOperationException( "An attempt was made to insert an item into a source that does not implement the IList interface or that has a fixed size." );

      newItemIndex = list.Count;
      list.Add( newItem );
    }

    private static PropertyDescription GetPropertyDescriptionFromItemProperty( PropertyDescriptionRouteDictionary propertyDescriptions, DataGridItemPropertyRoute itemPropertyRoute )
    {
      if( ( propertyDescriptions == null ) || ( itemPropertyRoute == null ) )
        return null;

      var propertyRoute = PropertyRouteBuilder.ToPropertyRoute( itemPropertyRoute );
      if( propertyRoute == null )
        return null;

      PropertyDescriptionRoute propertyDescription;
      if( !propertyDescriptions.TryGetValue( propertyRoute, out propertyDescription ) )
        return null;

      return propertyDescription.Current;
    }

    internal static void CreateAndAddItemPropertiesForPropertyDescriptions( DataGridItemPropertyCollection itemProperties, IEnumerable<PropertyDescriptionRoute> propertyDescriptions )
    {
      if( ( itemProperties == null ) || ( propertyDescriptions == null ) )
        return;

      using( itemProperties.DeferCollectionChanged() )
      {
        foreach( var propertyDescription in propertyDescriptions )
        {
          ItemsSourceHelper.CreateAndAddItemPropertyForPropertyDescription( itemProperties, propertyDescription );
        }
      }
    }

    internal static void CreateAndAddItemPropertyForPropertyDescription( DataGridItemPropertyCollection itemProperties, PropertyDescriptionRoute propertyDescription )
    {
      if( ( itemProperties == null ) || ( propertyDescription == null ) )
        return;

      if( !propertyDescription.Current.IsBrowsable )
        return;

      var ancestors = new Stack<PropertyDescription>();
      for( var current = propertyDescription; current != null; current = current.Parent )
      {
        var description = current.Current;
        if( description.IsSubRelationship )
          return;

        ancestors.Push( description );
      }

      var parentItemProperty = default( DataGridItemPropertyBase );
      var isRoot = true;

      while( ancestors.Count > 0 )
      {
        var description = ancestors.Pop();
        var itemProperty = ( isRoot ) ? itemProperties[ description.Name ] : parentItemProperty.ItemProperties[ description.Name ];

        if( itemProperty == null )
        {
          itemProperty = ItemsSourceHelper.CreateItemPropertyFromPropertyDescription( description );
          Debug.Assert( itemProperty != null );

          if( isRoot )
          {
            itemProperties.Add( itemProperty );
          }
          else
          {
            Debug.Assert( parentItemProperty != null );
            parentItemProperty.ItemProperties.Add( itemProperty );
          }
        }

        parentItemProperty = itemProperty;
        isRoot = false;
      }
    }

    internal static void CreateColumnsFromPropertyDescriptions(
      ColumnHierarchyManager columnManager,
      IDictionary<Type, CellEditor> defaultCellEditors,
      PropertyDescriptionRouteDictionary propertyDescriptions,
      bool autoCreateForeignKeyConfigurations )
    {
      Debug.Assert( columnManager != null );

      var columns = columnManager.Columns;

      using( columnManager.DeferUpdate() )
      {
        using( columns.DeferNotifications() )
        {
          foreach( var propertyDescriptionRoute in propertyDescriptions.Values )
          {
            var fieldName = PropertyRouteParser.Parse( PropertyRouteBuilder.ToPropertyRoute( propertyDescriptionRoute ) );
            var columnBase = columns[ fieldName ];
            var column = columnBase as Column;

            if( columnBase == null )
            {
              column = ItemsSourceHelper.CreateColumnFromPropertyDescription( defaultCellEditors, propertyDescriptionRoute, autoCreateForeignKeyConfigurations );

              if( column != null )
              {
                columns.Add( column );
              }
            }
            else if( column != null )
            {
              ItemsSourceHelper.UpdateColumnFromPropertyDescription( column, defaultCellEditors, autoCreateForeignKeyConfigurations, propertyDescriptionRoute );
            }
          }
        }
      }
    }

    internal static void UpdateColumnsFromPropertyDescriptions(
      ColumnHierarchyManager columnManager,
      IDictionary<Type, CellEditor> defaultCellEditors,
      PropertyDescriptionRouteDictionary propertyDescriptions,
      bool autoCreateForeignKeyConfigurations )
    {
      Debug.Assert( columnManager != null );

      var columns = columnManager.Columns;

      using( columnManager.DeferUpdate() )
      {
        using( columns.DeferNotifications() )
        {
          var columnsToRemove = new HashSet<ColumnBase>( ( from columnBase in columns
                                                           let column = columnBase as Column
                                                           where ( column != null ) && column.IsAutoCreated
                                                           select column ) );

          foreach( var propertyDescriptionRoute in propertyDescriptions.Values )
          {
            var fieldName = PropertyRouteParser.Parse( PropertyRouteBuilder.ToPropertyRoute( propertyDescriptionRoute ) );
            var columnBase = columns[ fieldName ];
            var column = columnBase as Column;

            if( columnBase == null )
            {
              column = ItemsSourceHelper.CreateColumnFromPropertyDescription( defaultCellEditors, propertyDescriptionRoute, autoCreateForeignKeyConfigurations );

              if( column != null )
              {
                columns.Add( column );
              }
            }
            else if( column != null )
            {
              columnsToRemove.Remove( column );

              ItemsSourceHelper.UpdateColumnFromPropertyDescription( column, defaultCellEditors, autoCreateForeignKeyConfigurations, propertyDescriptionRoute );
            }
          }

          foreach( var column in columnsToRemove )
          {
            columns.Remove( column );
          }
        }
      }
    }

    private static Column CreateColumnFromPropertyDescription( IDictionary<Type, CellEditor> defaultCellEditors, PropertyDescriptionRoute propertyDescriptionRoute, bool autoCreateForeignKeyConfigurations )
    {
      if( propertyDescriptionRoute == null )
        return null;

      var propertyDescription = propertyDescriptionRoute.Current;
      var dataType = propertyDescription.DataType;

      if( !propertyDescription.IsBrowsable || !propertyDescription.IsDisplayable )
        return null;

      for( var current = propertyDescriptionRoute; current != null; current = current.Parent )
      {
        var description = current.Current;
        if( description.IsSubRelationship )
          return null;
      }

      var name = PropertyRouteParser.Parse( PropertyRouteBuilder.ToPropertyRoute( propertyDescriptionRoute ) );
      if( string.IsNullOrEmpty( name ) )
        return null;

      var column = new Column();
      column.IsAutoCreated = true;
      column.FieldName = name;
      column.Title = propertyDescription.DisplayName;
      column.OverrideReadOnlyForInsertion = propertyDescription.OverrideReadOnlyForInsertion;
      column.SetDisplayMemberBinding( ItemsSourceHelper.CreateDefaultBinding( propertyDescriptionRoute ) );
      column.IsBindingAutoCreated = true;
      column.IsBoundToDataGridUnboundItemProperty = propertyDescription.IsDataGridUnboundItemProperty;

      if( propertyDescription.IsReadOnly )
      {
        column.ReadOnly = true;
      }

      var cellEditor = default( CellEditor );

      if( ( defaultCellEditors != null ) && !defaultCellEditors.TryGetValue( dataType, out cellEditor ) )
      {
        cellEditor = default( CellEditor );
      }

      var foreignKeyDescription = propertyDescription.ForeignKeyDescription;

      if( ( foreignKeyDescription != null ) && ( foreignKeyDescription.ItemsSource != null ) && ( autoCreateForeignKeyConfigurations ) )
      {
        // We will only use the default foreign key CellEditor if:
        // - a ForeignKey ItemsSource was detected
        // - the grid allows the auto-creation of the ForeignKeyConfigurations
        // else, use the default CellEditor
        if( cellEditor == null )
        {
          cellEditor = DefaultCellEditorSelector.ForeignKeyCellEditor;
        }

        // Update the ForeignKeyConfiguration from the ForeignKeyDescription found on the FieldDescriptor
        ForeignKeyConfiguration.SynchronizeForeignKeyConfigurationFromForeignKeyDescription( column, foreignKeyDescription, autoCreateForeignKeyConfigurations );
      }

      if( cellEditor == null )
      {
        cellEditor = DefaultCellEditorSelector.SelectCellEditor( dataType );
      }

      column.CellEditor = cellEditor;
      column.DefaultCellRecyclingGroupDataType = dataType;

      return column;
    }

    internal static void UpdateColumnFromPropertyDescription(
      ColumnBase column,
      IDictionary<Type, CellEditor> defaultCellEditors,
      bool autoCreateForeignKeyConfigurations,
      PropertyDescriptionRoute propertyDescription )
    {
      if( ( column == null ) || ( propertyDescription == null ) )
        return;

      var targetColumn = column as Column;
      var targetPropertyDescription = propertyDescription.Current;

      if( targetPropertyDescription.IsReadOnly )
      {
        if( column.ReadLocalValue( Column.ReadOnlyProperty ) == DependencyProperty.UnsetValue )
        {
          column.ReadOnly = targetPropertyDescription.IsReadOnly;
        }
      }

      if( targetPropertyDescription.OverrideReadOnlyForInsertion )
      {
        if( column.ReadLocalValue( ColumnBase.OverrideReadOnlyForInsertionProperty ) == DependencyProperty.UnsetValue )
        {
          column.OverrideReadOnlyForInsertion = targetPropertyDescription.OverrideReadOnlyForInsertion;
        }
      }

      if( column.ReadLocalValue( ColumnBase.TitleProperty ) == DependencyProperty.UnsetValue )
      {
        column.Title = targetPropertyDescription.DisplayName;
      }

      if( column.ReadLocalValue( ColumnBase.CellEditorProperty ) == DependencyProperty.UnsetValue )
      {
        var cellEditor = default( CellEditor );

        if( defaultCellEditors != null )
        {
          defaultCellEditors.TryGetValue( targetPropertyDescription.DataType, out cellEditor );
        }

        if( ( cellEditor == null ) && ( targetColumn != null ) )
        {
          var descriptionItemsSource = default( object );
          var configurationItemsSource = default( object );
          var configuration = targetColumn.ForeignKeyConfiguration;

          if( targetPropertyDescription.ForeignKeyDescription != null )
          {
            descriptionItemsSource = targetPropertyDescription.ForeignKeyDescription.ItemsSource;
          }

          if( configuration != null )
          {
            configurationItemsSource = configuration.ItemsSource;

            if( configurationItemsSource == null )
            {
              configurationItemsSource = targetColumn.ReadLocalValue( Column.ForeignKeyConfigurationProperty );
            }
          }

          // A foreign key ItemsSource is set and we can auto-create configuration, OR,
          // if the foreign key ItemsSource was found in the ForeignKeyConfiguration, use the Default ForeignKey CellEditor.
          if( ( ( descriptionItemsSource != null ) && ( autoCreateForeignKeyConfigurations ) ) || ( configurationItemsSource != null ) )
          {
            cellEditor = DefaultCellEditorSelector.ForeignKeyCellEditor;
          }
        }

        if( cellEditor == null )
        {
          cellEditor = DefaultCellEditorSelector.SelectCellEditor( targetPropertyDescription.DataType );
        }

        column.CellEditor = cellEditor;
      }

      if( targetColumn != null )
      {
        if( ( targetPropertyDescription.ForeignKeyDescription != null ) && ( targetPropertyDescription.ForeignKeyDescription.ItemsSource != null ) && ( autoCreateForeignKeyConfigurations ) )
        {
          // Update the ForeignKeyConfiguration from the ForeignKeyDescription found on the FieldDescriptor
          ForeignKeyConfiguration.SynchronizeForeignKeyConfigurationFromForeignKeyDescription( targetColumn, targetPropertyDescription.ForeignKeyDescription, autoCreateForeignKeyConfigurations );
        }

        if( targetColumn.GetDisplayMemberBinding() == null )
        {
          targetColumn.SetDisplayMemberBinding( ItemsSourceHelper.CreateDefaultBinding( propertyDescription ) );
          targetColumn.IsBindingAutoCreated = true;
          targetColumn.IsBoundToDataGridUnboundItemProperty = targetPropertyDescription.IsDataGridUnboundItemProperty;
        }
      }

      column.DefaultCellRecyclingGroupDataType = targetPropertyDescription.DataType;
    }

    internal static DataGridItemPropertyBase GetItemPropertyFromProperty( DataGridItemPropertyCollection itemProperties, string propertyPath )
    {
      if( itemProperties == null )
        return null;

      return ItemsSourceHelper.GetItemPropertyFromProperty( itemProperties, PropertyRouteParser.Parse( propertyPath ) );
    }

    internal static DataGridItemPropertyBase GetItemPropertyFromProperty( DataGridItemPropertyCollection itemProperties, PropertyRoute propertyRoute )
    {
      if( ( itemProperties == null ) || ( propertyRoute == null ) )
        return null;

      if( propertyRoute.Parent != null )
      {
        var parentItemProperty = ItemsSourceHelper.GetItemPropertyFromProperty( itemProperties, propertyRoute.Parent );
        if( parentItemProperty == null )
          return null;

        itemProperties = parentItemProperty.ItemPropertiesInternal;
        if( itemProperties == null )
          return null;
      }

      return itemProperties[ propertyRoute.Current.Name ];
    }

    internal static object GetValueFromItemProperty( DataGridItemPropertyBase itemProperty, object item )
    {
      return ItemsSourceHelper.GetValueFromItemProperty( itemProperty, item, null );
    }

    internal static object GetValueFromItemProperty( DataGridItemPropertyBase itemProperty, object item, object defaultValue )
    {
      if( itemProperty == null )
        throw new ArgumentNullException( "itemProperty" );

      // Make sure the DataGridItemProperty is still in use.
      var collection = itemProperty.ContainingCollection;
      if( collection == null )
        return defaultValue;

      var parentItemProperty = collection.Owner;
      if( parentItemProperty != null )
      {
        item = ItemsSourceHelper.GetValueFromItemProperty( parentItemProperty, item );
      }

      if( item == null )
        return defaultValue;

      return itemProperty.GetValue( item );
    }

    internal static void SetValueForItemProperty( DataGridItemPropertyBase itemProperty, object item, object value )
    {
      if( itemProperty == null )
        throw new ArgumentNullException( "itemProperty" );

      // Make sure the DataGridItemProperty is still in use.
      var collection = itemProperty.ContainingCollection;
      if( collection == null )
        return;

      var parentItemProperty = collection.Owner;
      if( parentItemProperty != null )
      {
        item = ItemsSourceHelper.GetValueFromItemProperty( parentItemProperty, item );
      }

      if( item == null )
        return;

      itemProperty.SetValue( item, value );
    }

    internal static void AutoDetectSynonyms( DataGridCollectionViewBase collectionView )
    {
      if( collectionView == null )
        return;

      // The root CollectionView does not need to have its ItemProperties binded.
      var masterCollectionView = collectionView.RootDataGridCollectionViewBase;
      if( ( masterCollectionView == null ) || ( masterCollectionView == collectionView ) )
        return;

      ItemsSourceHelper.AutoDetectSynonyms( masterCollectionView.ItemProperties, collectionView.ItemProperties );
    }

    internal static void AutoDetectSynonyms( DataGridCollectionViewBase collectionView, DataGridItemPropertyRoute detailItemPropertyRoute )
    {
      if( ( collectionView == null ) || ( detailItemPropertyRoute == null ) )
        return;

      // The root CollectionView does not need to have its ItemProperties binded.
      var masterCollectionView = collectionView.RootDataGridCollectionViewBase;
      if( ( masterCollectionView == null ) || ( masterCollectionView == collectionView ) )
        return;

      var masterItemProperty = ItemsSourceHelper.AutoDetectSynonyms( masterCollectionView.ItemProperties, detailItemPropertyRoute );
      if( masterItemProperty == null )
        return;

      // Do the auto detection on child properties.
      ItemsSourceHelper.AutoDetectSynonyms( masterItemProperty.ItemPropertiesInternal, detailItemPropertyRoute.Current.ItemPropertiesInternal );
    }

    private static void AutoDetectSynonyms( DataGridItemPropertyCollection masterItemProperties, DataGridItemPropertyCollection detailItemProperties )
    {
      if( ( masterItemProperties == null ) || ( detailItemProperties == null ) )
        return;

      foreach( var detailItemProperty in detailItemProperties )
      {
        ItemsSourceHelper.AutoDetectSynonyms( masterItemProperties, detailItemProperty, true );
      }
    }

    private static DataGridItemPropertyBase AutoDetectSynonyms( DataGridItemPropertyCollection masterItemProperties, DataGridItemPropertyRoute detailItemPropertyRoute )
    {
      if( ( masterItemProperties == null ) || ( detailItemPropertyRoute == null ) )
        return null;

      if( detailItemPropertyRoute.Parent == null )
        return ItemsSourceHelper.AutoDetectSynonyms( masterItemProperties, detailItemPropertyRoute.Current, false );

      var masterParentItemProperty = ItemsSourceHelper.AutoDetectSynonyms( masterItemProperties, detailItemPropertyRoute.Parent );
      if( masterParentItemProperty == null )
        return null;

      return ItemsSourceHelper.AutoDetectSynonyms( masterParentItemProperty.ItemPropertiesInternal, detailItemPropertyRoute.Current, false );
    }

    private static DataGridItemPropertyBase AutoDetectSynonyms( DataGridItemPropertyCollection masterItemProperties, DataGridItemPropertyBase detailItemProperty, bool autoDetectSubProperties )
    {
      if( ( masterItemProperties == null ) || ( detailItemProperty == null ) )
        return null;

      var masterItemProperty = default( DataGridItemPropertyBase );

      if( string.IsNullOrEmpty( detailItemProperty.Synonym ) )
      {
        masterItemProperty = masterItemProperties[ detailItemProperty.Name ];

        if( ( masterItemProperty != null ) && ( masterItemProperty.DataType == detailItemProperty.DataType ) )
        {
          detailItemProperty.SetSynonym( masterItemProperty.Name );
        }
        else
        {
          masterItemProperty = null;
        }
      }
      else if( ( detailItemProperty.ItemPropertiesInternal != null ) && ( detailItemProperty.Synonym == detailItemProperty.Name ) )
      {
        masterItemProperty = masterItemProperties[ detailItemProperty.Name ];
      }

      if( autoDetectSubProperties && ( masterItemProperty != null ) && ( masterItemProperty.ItemPropertiesInternal != null ) && ( detailItemProperty.ItemPropertiesInternal != null ) )
      {
        ItemsSourceHelper.AutoDetectSynonyms( masterItemProperty.ItemPropertiesInternal, detailItemProperty.ItemPropertiesInternal );
      }

      return masterItemProperty;
    }

    internal static ICustomTypeDescriptor GetCustomTypeDescriptorFromItem( object item, Type itemType )
    {
      var descriptionProvider = default( TypeDescriptionProvider );
      if( itemType != null )
      {
        descriptionProvider = TypeDescriptor.GetProvider( itemType );
      }

      if( !( descriptionProvider is DataItemTypeDescriptionProvider ) )
      {
        descriptionProvider = new DataItemTypeDescriptionProvider( descriptionProvider );
      }

      return descriptionProvider.GetTypeDescriptor( itemType, item );
    }

    private static ICustomTypeDescriptor GetCustomTypeDescriptor( IEnumerable itemsSource, Type itemType )
    {
      if( ( itemsSource == null ) && ( itemType == null ) )
        return null;

      var firstItem = ItemsSourceHelper.GetFirstItemByEnumerable( itemsSource );

      return ItemsSourceHelper.GetCustomTypeDescriptor( firstItem, itemType );
    }

    private static ICustomTypeDescriptor GetCustomTypeDescriptor( object item, Type itemType )
    {
      var descriptor = item as ICustomTypeDescriptor;
      if( descriptor == null )
        return ItemsSourceHelper.GetCustomTypeDescriptorFromItem( item, itemType );

      if( !( descriptor is DataItemTypeDescriptor ) )
        return DataItemTypeDescriptionProvider.GetTypeDescriptor( itemType, descriptor );

      return descriptor;
    }

    internal static void ResetPropertyDescriptions( PropertyDescriptionRouteDictionary collection, DataGridItemPropertyMap itemPropertyMap, DataGridControl dataGridControl, IEnumerable itemsSource )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );

      if( dataGridControl == null )
        throw new ArgumentNullException( "dataGridControl" );

      collection.Clear();

      var masterPropertyDescriptions = dataGridControl.ItemsSourcePropertyDescriptions;

      if( ( collection != masterPropertyDescriptions ) && dataGridControl.AreDetailsFlatten )
      {
        var collectionView = itemsSource as DataGridCollectionViewBase;
        if( collectionView != null )
        {
          ItemsSourceHelper.SetPropertyDescriptions( collection, masterPropertyDescriptions, itemPropertyMap, collectionView );
        }
      }
      else
      {
        ItemsSourceHelper.SetPropertyDescriptions( collection, null, itemsSource, null, true );
      }
    }

    internal static void SetPropertyDescriptions( PropertyDescriptionRouteDictionary collection, DataTable model, IEnumerable itemsSource, Type itemType, bool displayable )
    {
      if( collection == null )
        throw new ArgumentNullException( "collection" );

      var descriptions = default( IEnumerable<PropertyDescriptionRoute> );
      var supportsDBNull = ItemsSourceHelper.IsSupportingDBNull( itemsSource );

      if( model != null )
      {
        descriptions = ItemsSourceHelper.CreatePropertyDescriptionsFromDataTable( model, displayable );
      }
      else if( itemsSource is DataView )
      {
        descriptions = ItemsSourceHelper.CreatePropertyDescriptionsFromDataView( ( DataView )itemsSource, displayable );
      }
      else if( itemsSource is DataGridCollectionViewBase )
      {
        descriptions = ItemsSourceHelper.CreatePropertyDescriptionsFromDataGridCollectionView( ( DataGridCollectionViewBase )itemsSource );
      }
      else if( itemsSource is ITypedList )
      {
        descriptions = ItemsSourceHelper.CreatePropertyDescriptionsFromPropertyDescriptors( ( ( ITypedList )itemsSource ).GetItemProperties( null ), supportsDBNull, displayable );
      }
      else
      {
        if( itemType == null )
        {
          itemType = ItemsSourceHelper.GetItemTypeFromEnumeration( itemsSource );
        }

        descriptions = ItemsSourceHelper.CreatePropertyDescriptionsFromItemType( itemsSource, itemType, supportsDBNull, displayable );
      }

      if( descriptions != null )
      {
        foreach( var description in descriptions )
        {
          collection.Add( description, true );
        }
      }
    }

    private static void SetPropertyDescriptions( PropertyDescriptionRouteDictionary detailCollection, PropertyDescriptionRouteDictionary masterCollection, DataGridItemPropertyMap itemPropertyMap, DataGridCollectionViewBase collectionView )
    {
      if( collectionView == null )
        return;

      foreach( var itemProperty in collectionView.ItemProperties )
      {
        var masterItemProperty = default( DataGridItemPropertyBase );
        if( !itemPropertyMap.TryGetMasterItemProperty( itemProperty, out masterItemProperty ) )
        {
          Debug.WriteLine( string.Format( "No mapping was found for the item property." ) );
          continue;
        }

        var masterPropertyRoute = PropertyRouteBuilder.ToPropertyRoute( DataGridItemPropertyRoute.Create( masterItemProperty ) );
        if( masterPropertyRoute == null )
        {
          masterPropertyRoute = PropertyRouteBuilder.ToPropertyRoute( new PropertyRouteSegment( PropertyRouteSegmentType.Property, masterItemProperty.Name ) );
        }

        var masterPropertyDescription = masterCollection[ masterPropertyRoute ];
        if( masterPropertyDescription == null )
        {
          Debug.WriteLine( string.Format( "An item property is mapped to a non-existent master field." ) );
          continue;
        }

        var detailPropertyDescription = ItemsSourceHelper.CreatePropertyDescriptionFromDataGridItemProperty( itemProperty );
        if( detailPropertyDescription.Current.DataType != masterPropertyDescription.Current.DataType )
        {
          Debug.WriteLine( string.Format( "The data type of an item property doesn't match the data type of its master field." ) );
          continue;
        }

        if( detailCollection.Contains( masterPropertyRoute ) )
        {
          Debug.WriteLine( string.Format( "Another detail field is already linked to the master field." ) );
          continue;
        }

        detailCollection.Add( masterPropertyRoute, detailPropertyDescription, false );
      }
    }

    internal static void SetPropertyDescriptionsFromItemProperty( PropertyDescriptionRouteDictionary propertyDescriptions, DataTable model, IEnumerable itemsSource, Type itemType, DataGridItemPropertyRoute itemPropertyRoute )
    {
      if( ( propertyDescriptions == null ) || ( itemPropertyRoute == null ) )
        return;

      var key = PropertyRouteBuilder.ToPropertyRoute( itemPropertyRoute );
      ItemsSourceHelper.SetPropertyDescriptions( propertyDescriptions, model, itemsSource, itemType, key );

      var propertyDescription = default( PropertyDescriptionRoute );
      if( !propertyDescriptions.TryGetValue( key, out propertyDescription ) )
        return;

      var itemProperty = itemPropertyRoute.Current;
      if( itemProperty.ItemPropertiesInternal == null )
        return;

      foreach( var childItemProperty in itemProperty.ItemPropertiesInternal )
      {
        ItemsSourceHelper.SetPropertyDescriptionsFromItemProperty(
          propertyDescriptions,
          null,
          null,
          itemProperty.DataType,
          DataGridItemPropertyRoute.Combine( childItemProperty, itemPropertyRoute ) );
      }
    }

    internal static void InitializePropertyDescriptions( PropertyDescriptionRouteDictionary collection, DataGridItemPropertyRoute itemPropertyRoute, Type itemType, bool defaultPropertyDescriptionsCreated )
    {
      if( ( collection == null ) || ( itemPropertyRoute == null ) )
        return;

      var propertyDescription = ItemsSourceHelper.GetPropertyDescriptionFromItemProperty( collection, itemPropertyRoute );
      var itemProperty = itemPropertyRoute.Current;

      itemProperty.SetUnspecifiedPropertiesValues( propertyDescription, itemType, defaultPropertyDescriptionsCreated );

      if( itemProperty.ItemPropertiesInternal != null )
      {
        foreach( var childItemProperty in itemProperty.ItemPropertiesInternal )
        {
          ItemsSourceHelper.InitializePropertyDescriptions( collection, DataGridItemPropertyRoute.Combine( childItemProperty, itemPropertyRoute ), itemProperty.DataType, defaultPropertyDescriptionsCreated );
        }
      }
    }

    internal static Type GetItemTypeFromEnumeration( IEnumerable source )
    {
      if( source == null )
        return null;

      var enumerationType = source.GetType();
      var itemType = ItemsSourceHelper.GetItemTypeFromEnumerationType( enumerationType );

      if( itemType != null )
        return itemType;

      var item = ItemsSourceHelper.GetFirstItemByEnumerable( source );
      if( item != null )
        return item.GetType();

      return typeof( object );
    }

    internal static Type GetItemTypeFromEnumerationType( Type enumerationType )
    {
      if( enumerationType == null )
        return null;

      if( typeof( Array ).IsAssignableFrom( enumerationType ) )
        return enumerationType.GetElementType();

      var itemType = ItemsSourceHelper.GetTypedListIndexerType( enumerationType );
      if( itemType != null )
        return itemType;

      return ItemsSourceHelper.GetTypedEnumerationItemType( enumerationType );
    }

    internal static IEditableObject GetEditableObject( object item )
    {
      var dataRow = item as System.Data.DataRow;
      if( dataRow != null )
        return new DataRowEditableWrapper( dataRow );

      return item as IEditableObject;
    }

    internal static ISupportInitialize GetSupportInitializeObject( object item )
    {
      return item as ISupportInitialize;
    }

    internal static bool IsASubRelationship( Type dataType )
    {
      if( ( dataType == null ) || ( dataType.IsValueType ) || ( typeof( string ).IsAssignableFrom( dataType ) ) )
        return false;

      if( typeof( IEnumerable ).IsAssignableFrom( dataType ) )
        return !typeof( byte[] ).IsAssignableFrom( dataType );

      return typeof( IListSource ).IsAssignableFrom( dataType );
    }

    internal static object TryGetDataRowFromDataItem( object dataItem )
    {
      var dataRowView = dataItem as DataRowView;
      if( dataRowView != null )
        return dataRowView.Row;

      return dataItem;
    }

    internal static DataView TryGetDataViewFromDataGridContext( DataGridContext context )
    {
      if( context == null )
        return null;

      var dataView = context.ItemsSourceCollection as DataView;
      if( dataView != null )
        return dataView;

      var collectionViewBase = context.ItemsSourceCollection as DataGridCollectionViewBase;
      if( collectionViewBase != null )
        return collectionViewBase.SourceCollection as DataView;

      return null;
    }

    internal static Type GetColumnDataType( DataColumn column )
    {
      if( !column.AllowDBNull )
        return column.DataType;

      if( column.DataType == typeof( Boolean ) )
        return typeof( Nullable<Boolean> );

      if( column.DataType == typeof( Byte ) )
        return typeof( Nullable<Byte> );

      if( column.DataType == typeof( Char ) )
        return typeof( Nullable<Char> );

      if( column.DataType == typeof( DateTime ) )
        return typeof( Nullable<DateTime> );

      if( column.DataType == typeof( Decimal ) )
        return typeof( Nullable<Decimal> );

      if( column.DataType == typeof( Double ) )
        return typeof( Nullable<Double> );

      if( column.DataType == typeof( Int16 ) )
        return typeof( Nullable<Int16> );

      if( column.DataType == typeof( Int32 ) )
        return typeof( Nullable<Int32> );

      if( column.DataType == typeof( Int64 ) )
        return typeof( Nullable<Int64> );

      if( column.DataType == typeof( SByte ) )
        return typeof( Nullable<SByte> );

      if( column.DataType == typeof( Single ) )
        return typeof( Nullable<Single> );

      if( column.DataType == typeof( TimeSpan ) )
        return typeof( Nullable<TimeSpan> );

      if( column.DataType == typeof( UInt16 ) )
        return typeof( Nullable<UInt16> );

      if( column.DataType == typeof( UInt32 ) )
        return typeof( Nullable<UInt32> );

      if( column.DataType == typeof( UInt64 ) )
        return typeof( Nullable<UInt64> );

      return column.DataType;
    }

    internal static void CleanUpColumns( ColumnCollection columns, bool deleteAutoCreatedColumn )
    {
      var tempColumns = new ColumnBase[ columns.Count ];
      columns.CopyTo( tempColumns, 0 );

      foreach( ColumnBase column in tempColumns )
      {
        var dataColumn = column as Column;
        if( dataColumn == null )
          continue;

        if( ( deleteAutoCreatedColumn ) && ( dataColumn.IsAutoCreated ) )
        {
          columns.Remove( column );
        }
        else if( dataColumn.IsBindingAutoCreated )
        {
          dataColumn.IsBindingAutoCreated = false;
          dataColumn.IsBoundToDataGridUnboundItemProperty = false;
          dataColumn.SetDisplayMemberBinding( null );
        }
      }
    }

    internal static PropertyDescriptionRoute CreateOrGetPropertyDescriptionFromColumn( DataGridContext dataGridContext, ColumnBase column, Type itemType )
    {
      if( ( dataGridContext == null ) || ( column == null ) || string.IsNullOrEmpty( column.FieldName ) )
        return null;

      var key = PropertyRouteParser.Parse( column.FieldName );
      if( key == null )
        return null;

      var propertyDescriptions = dataGridContext.ItemsSourcePropertyDescriptions;
      if( propertyDescriptions == null )
        return null;

      var propertyDescriptionRoute = default( PropertyDescriptionRoute );
      if( !propertyDescriptions.TryGetValue( key, out propertyDescriptionRoute ) )
      {
        if( ( itemType == null ) || typeof( EmptyDataItem ).IsAssignableFrom( itemType ) )
        {
          var dataGridCollectionView = dataGridContext.ItemsSourceCollection as DataGridCollectionViewBase;
          if( dataGridCollectionView != null )
          {
            itemType = dataGridCollectionView.ItemType;
          }
          else
          {
            var dataItem = ItemsSourceHelper.GetFirstItemByEnumerable( dataGridContext.Items );

            itemType = ( ( dataItem != null ) && !( dataItem is EmptyDataItem ) ) ? dataItem.GetType() : null;
          }
        }

        ItemsSourceHelper.SetPropertyDescriptions( propertyDescriptions, null, dataGridContext.Items, itemType, key );

        if( !propertyDescriptions.TryGetValue( key, out propertyDescriptionRoute ) )
          return null;
      }

      return propertyDescriptionRoute;
    }

    internal static System.Windows.Data.Binding CreateDefaultBinding( PropertyDescriptionRoute propertyDescriptionRoute )
    {
      if( propertyDescriptionRoute == null )
        return null;

      var propertyDescription = propertyDescriptionRoute.Current;
      var bindingXPath = propertyDescription.XPath;
      var bindingPath = new StringBuilder();
      var isPropertyDescriptorFirstAccessor = false;

      var propertyDescriptors = new List<PropertyDescriptor>();
      if( propertyDescription.PropertyDescriptor != null )
      {
        bindingPath.Append( "(" ).Append( propertyDescriptors.Count ).Append( ")" );
        propertyDescriptors.Add( propertyDescription.PropertyDescriptor );
        isPropertyDescriptorFirstAccessor = true;
      }
      else if( !string.IsNullOrEmpty( propertyDescription.Path ) )
      {
        bindingPath.Append( propertyDescription.Path );
      }

      var currentPropertyDescriptionRoute = propertyDescriptionRoute.Parent;

      while( currentPropertyDescriptionRoute != null )
      {
        var currentPropertyDescription = currentPropertyDescriptionRoute.Current;

        if( currentPropertyDescription.SupportDBNull )
          throw new InvalidOperationException( "Cannot create binding when a parent property supports DBNull." );

        if( currentPropertyDescription.IsDataGridUnboundItemProperty )
          throw new InvalidOperationException( "Cannot create binding when a parent property is linked to an UnboundDataGridItemProperty." );

        if( currentPropertyDescription.ForeignKeyDescription != null )
          throw new InvalidOperationException( "Cannot create binding when a parent property is a foreign key." );

        if( !string.IsNullOrEmpty( bindingXPath ) )
          throw new InvalidOperationException( "Cannot create binding when a child property has an XPath." );

        bindingXPath = currentPropertyDescription.XPath;

        if( currentPropertyDescription.PropertyDescriptor != null )
        {
          isPropertyDescriptorFirstAccessor = true;

          if( bindingPath.Length != 0 )
          {
            bindingPath.Insert( 0, "." );
          }

          bindingPath.Insert( 0, ")" ).Insert( 0, propertyDescriptors.Count ).Insert( 0, "(" );
          propertyDescriptors.Add( currentPropertyDescription.PropertyDescriptor );
        }
        else if( !string.IsNullOrEmpty( currentPropertyDescription.Path ) )
        {
          isPropertyDescriptorFirstAccessor = false;

          if( bindingPath.Length != 0 )
          {
            bindingPath.Insert( 0, "." );
          }

          bindingPath.Insert( 0, currentPropertyDescription.Path );
        }

        currentPropertyDescriptionRoute = currentPropertyDescriptionRoute.Parent;
      }

      var bindingInfo = new DataGridBindingInfo();

      if( !string.IsNullOrEmpty( bindingXPath ) )
      {
        bindingInfo.XPath = bindingXPath;
      }
      else
      {
        // We must insert a PropertyDescriptor to manage EmptyDataItem when the accessor is not from a DataGridItemPropertyBase.
        if( isPropertyDescriptorFirstAccessor && !( propertyDescriptors.Last() is DataGridItemPropertyBase.PropertyDescriptorFromItemPropertyBase ) )
        {
          if( bindingPath.Length != 0 )
          {
            bindingPath.Insert( 0, "." );
          }

          bindingPath.Insert( 0, ")" ).Insert( 0, propertyDescriptors.Count ).Insert( 0, "(" );
          propertyDescriptors.Add( EmptyDataItemSafePropertyDescriptor.Singleton );
        }
      }

      bindingInfo.Path = new PropertyPath( bindingPath.ToString(), propertyDescriptors.ToArray() );
      bindingInfo.ReadOnly = ( propertyDescription.IsReadOnly && !propertyDescription.OverrideReadOnlyForInsertion );
      bindingInfo.Converter = new SourceDataConverter( propertyDescription.SupportDBNull );

      bindingInfo.ValidationRules.Add( new SourceDataConverterValidationRule( propertyDescription.SupportDBNull, propertyDescription.DataType ) );

      return bindingInfo.GetBinding();
    }

    private static Type GetTypedListIndexerType( Type listType )
    {
      if( ( !typeof( IList ).IsAssignableFrom( listType ) && !typeof( ITypedList ).IsAssignableFrom( listType ) ) && !typeof( IListSource ).IsAssignableFrom( listType ) )
        return null;

      var info = default( PropertyInfo );
      var propertyInfos = listType.GetProperties( BindingFlags.Public | BindingFlags.Instance );

      foreach( var propertyInfo in propertyInfos )
      {
        if( ( propertyInfo.GetIndexParameters().Length > 0 ) && ( propertyInfo.PropertyType != typeof( object ) ) )
        {
          info = propertyInfo;

          if( info.Name == "Item" )
            return info.PropertyType;
        }
      }

      if( info != null )
        return info.PropertyType;

      return null;
    }

    private static Type GetTypedEnumerationItemType( Type listType )
    {
      if( listType == null )
        return null;

      foreach( Type interfaceType in listType.GetInterfaces() )
      {
        if( ( interfaceType.IsGenericType ) && ( interfaceType.GetGenericTypeDefinition() == typeof( IEnumerable<> ) ) )
          return interfaceType.GetGenericArguments()[ 0 ];
      }

      return null;
    }

    private static void SetPropertyDescriptions( PropertyDescriptionRouteDictionary propertyDescriptions, DataTable model, IEnumerable itemsSource, Type itemType, PropertyRoute propertyRoute )
    {
      if( ( propertyDescriptions == null ) || ( propertyRoute == null ) || propertyDescriptions.Contains( propertyRoute ) )
        return;

      var parentKey = propertyRoute.Parent;
      if( parentKey != null )
      {
        ItemsSourceHelper.SetPropertyDescriptions( propertyDescriptions, model, itemsSource, itemType, parentKey );

        PropertyDescriptionRoute parentPropertyDescriptionRoute;
        if( !propertyDescriptions.TryGetValue( parentKey, out parentPropertyDescriptionRoute ) )
          return;

        var parentItemType = parentPropertyDescriptionRoute.Current.DataType;
        var childPropertyDescriptions = new PropertyDescriptionRouteDictionary();

        ItemsSourceHelper.SetPropertyDescriptions( childPropertyDescriptions, null, null, parentItemType, false );

        foreach( var childPropertyDescriptionRoute in childPropertyDescriptions.Values )
        {
          var newPropertyDescriptionRoute = PropertyDescriptionRoute.Combine( childPropertyDescriptionRoute, parentPropertyDescriptionRoute );
          var newPropertyRoute = PropertyRouteBuilder.ToPropertyRoute( newPropertyDescriptionRoute );

          if( ( newPropertyRoute == null ) || propertyDescriptions.Contains( newPropertyRoute ) )
            continue;

          propertyDescriptions.Add( newPropertyRoute, newPropertyDescriptionRoute, false );
        }

        var currentSegment = propertyRoute.Current;
        if( currentSegment.Type == PropertyRouteSegmentType.Indexer )
        {
          var indexerDescriptionRoute = ItemsSourceHelper.CreateIndexerDescription( itemsSource, parentItemType, currentSegment.Name, false );
          if( indexerDescriptionRoute != null )
          {
            var newIndexerDescriptionRoute = PropertyDescriptionRoute.Combine( indexerDescriptionRoute, parentPropertyDescriptionRoute );
            if( newIndexerDescriptionRoute != null )
            {
              var newIndexerRoute = PropertyRouteBuilder.ToPropertyRoute( newIndexerDescriptionRoute );
              if( ( newIndexerRoute != null ) && !propertyDescriptions.Contains( newIndexerRoute ) )
              {
                propertyDescriptions.Add( newIndexerRoute, newIndexerDescriptionRoute, false );
              }
            }
          }
        }
      }
      else
      {
        ItemsSourceHelper.SetPropertyDescriptions( propertyDescriptions, model, itemsSource, itemType, false );

        var currentSegment = propertyRoute.Current;
        if( currentSegment.Type == PropertyRouteSegmentType.Indexer )
        {
          var newIndexerDescriptionRoute = ItemsSourceHelper.CreateIndexerDescription( itemsSource, itemType, currentSegment.Name, false );
          if( newIndexerDescriptionRoute != null )
          {
            var newIndexerRoute = PropertyRouteBuilder.ToPropertyRoute( newIndexerDescriptionRoute );
            if( ( newIndexerRoute != null ) && !propertyDescriptions.Contains( newIndexerRoute ) )
            {
              propertyDescriptions.Add( newIndexerRoute, newIndexerDescriptionRoute, false );
            }
          }
        }
      }
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromDataTable( DataTable source, bool displayable )
    {
      Debug.Assert( source != null );

      var foreignKeyConstraints = ItemsSourceHelper.GetForeignKeyConstraints( source.Constraints );
      var columns = source.Columns;

      foreach( DataColumn column in columns )
      {
        yield return new PropertyDescriptionRoute(
                           new DataTablePropertyDescription(
                             new DataRowColumnPropertyDescriptor( column ),
                             column,
                             foreignKeyConstraints,
                             displayable ) );
      }
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromDataView( DataView source, bool displayable )
    {
      Debug.Assert( source != null );

      var itemProperties = ( ( ITypedList )source ).GetItemProperties( null );
      var dataTable = source.Table;
      var foreignKeyConstraints = ItemsSourceHelper.GetForeignKeyConstraints( dataTable.Constraints );
      var columns = dataTable.Columns;

      foreach( PropertyDescriptor propertyDescriptor in itemProperties )
      {
        yield return new PropertyDescriptionRoute( new DataTablePropertyDescription( propertyDescriptor, columns[ propertyDescriptor.Name ], foreignKeyConstraints, displayable ) );
      }
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromDataGridCollectionView( DataGridCollectionViewBase source )
    {
      if( source == null )
        return Enumerable.Empty<PropertyDescriptionRoute>();

      return ItemsSourceHelper.CreatePropertyDescriptionsFromItemProperties( source.ItemProperties );
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromItemProperties( IEnumerable<DataGridItemPropertyBase> source )
    {
      if( source == null )
        yield break;

      foreach( var itemProperty in source )
      {
        yield return ItemsSourceHelper.CreatePropertyDescriptionFromDataGridItemProperty( itemProperty );

        foreach( var childItemProperty in ItemsSourceHelper.CreatePropertyDescriptionsFromItemProperties( itemProperty.ItemPropertiesInternal ) )
        {
          yield return childItemProperty;
        }
      }
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromPropertyDescriptors( PropertyDescriptorCollection source, bool supportsDBNull, bool displayable )
    {
      if( source == null )
        yield break;

      foreach( PropertyDescriptor propertyDescriptor in source )
      {
        yield return new PropertyDescriptionRoute( new PropertyDescriptorPropertyDescription( propertyDescriptor, supportsDBNull, displayable ) );
      }
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromItemType( IEnumerable itemsSource, Type itemType, bool supportsDBNull, bool displayable )
    {
      if( itemType == null )
        return Enumerable.Empty<PropertyDescriptionRoute>();

      if( typeof( XmlNode ).IsAssignableFrom( itemType ) )
        return Enumerable.Empty<PropertyDescriptionRoute>();

      if( itemType.IsArray )
        return ItemsSourceHelper.CreatePropertyDescriptionsFromJaggedArray( itemType, itemsSource, displayable );

      if( itemType.IsInterface )
        return ItemsSourceHelper.CreatePropertyDescriptionsFromInterface( itemType, displayable );

      if( ItemsSourceHelper.IsEntityFramework( itemType ) )
        return ItemsSourceHelper.CreatePropertyDescriptionsFromEntityFramework( itemType, displayable );

      if( ItemsSourceHelper.IsValueType( itemType ) )
      {
        var customTypeDescriptor = ItemsSourceHelper.GetCustomTypeDescriptor( itemsSource, itemType );

        return ItemsSourceHelper.CreatePropertyDescriptionsFromValueType( itemType, supportsDBNull, null, displayable ).Concat(
                 ( customTypeDescriptor != null )
                   ? ItemsSourceHelper.CreatePropertyDescriptionsFromCustomTypeDescriptor( customTypeDescriptor, itemType, supportsDBNull, false )
                   : ItemsSourceHelper.CreatePropertyDescriptionsFromPropertyDescriptors( TypeDescriptor.GetProperties( itemType ), supportsDBNull, false ) );
      }
      else
      {
        var customTypeDescriptor = ItemsSourceHelper.GetCustomTypeDescriptor( itemsSource, itemType );
        if( customTypeDescriptor != null )
          return ItemsSourceHelper.CreatePropertyDescriptionsFromCustomTypeDescriptor( customTypeDescriptor, itemType, supportsDBNull, displayable );

        return ItemsSourceHelper.CreatePropertyDescriptionsFromPropertyDescriptors( TypeDescriptor.GetProperties( itemType ), supportsDBNull, displayable );
      }
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromJaggedArray( Type itemType, IEnumerable jaggedArray, bool displayable )
    {
      var firstItem = ItemsSourceHelper.GetFirstItemByEnumerable( jaggedArray ) as Array;
      if( ( firstItem == null ) || ( itemType == null ) )
        yield break;

      var fieldCount = firstItem.GetLength( 0 );
      var fieldType = itemType.GetElementType();

      for( int i = 0; i < fieldCount; i++ )
      {
        yield return new PropertyDescriptionRoute( new JaggedArrayPropertyDescription( i, fieldType, displayable ) );
      }
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromInterface( Type itemType, bool displayable )
    {
      var propertyDescriptions = ItemsSourceHelper.CreatePropertyDescriptionsFromPropertyDescriptors( TypeDescriptor.GetProperties( itemType ), false, displayable ).ToList();
      var names = new HashSet<string>( propertyDescriptions.Select( item => PropertyRouteParser.Parse( PropertyRouteBuilder.ToPropertyRoute( item ) ) ) );

      foreach( var interfaceType in itemType.GetInterfaces() )
      {
        foreach( PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties( interfaceType ) )
        {
          PropertyDescription propertyDescription;

          if( names.Contains( propertyDescriptor.Name ) )
          {
            var newName = string.Format( "{0}.{1}", interfaceType.FullName, propertyDescriptor.Name );
            propertyDescription = new NamedPropertyDescriptorPropertyDescription( newName, propertyDescriptor, false, displayable );
          }
          else
          {
            propertyDescription = new PropertyDescriptorPropertyDescription( propertyDescriptor, false, displayable );
          }

          Debug.Assert( propertyDescription != null );

          names.Add( propertyDescription.Name );
          propertyDescriptions.Add( new PropertyDescriptionRoute( propertyDescription ) );
        }
      }

      return propertyDescriptions;
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromEntityFramework( Type itemType, bool displayable )
    {
      foreach( PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties( itemType ) )
      {
        yield return new PropertyDescriptionRoute( new EntityFrameworkPropertyDescription( propertyDescriptor, displayable ) );
      }
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromValueType( Type itemType, bool supportsDBNull, DataGridForeignKeyDescription foreignKeyDescription, bool displayable )
    {
      yield return new PropertyDescriptionRoute( new ValueTypePropertyDescription( itemType, supportsDBNull, foreignKeyDescription, displayable ) );
    }

    private static IEnumerable<PropertyDescriptionRoute> CreatePropertyDescriptionsFromCustomTypeDescriptor( ICustomTypeDescriptor customTypeDescriptor, Type itemType, bool supportsDBNull, bool displayable )
    {
      if( customTypeDescriptor == null )
        return Enumerable.Empty<PropertyDescriptionRoute>();

      return ItemsSourceHelper.CreatePropertyDescriptionsFromPropertyDescriptors( customTypeDescriptor.GetProperties(), supportsDBNull, displayable );
    }

    private static PropertyDescriptionRoute CreatePropertyDescriptionFromDataGridItemProperty( DataGridItemPropertyBase source )
    {
      if( source == null )
        return null;

      var collection = source.ContainingCollection;
      var ancestors = ( collection != null ) ? ItemsSourceHelper.CreatePropertyDescriptionFromDataGridItemProperty( collection.Owner ) : null;

      return PropertyDescriptionRoute.Combine( new ItemPropertyPropertyDescription( source ), ancestors );
    }

    private static PropertyDescriptionRoute CreateIndexerDescription( IEnumerable itemsSource, Type itemType, string index, bool displayable )
    {
      if( ( itemType == null ) || string.IsNullOrEmpty( index ) )
        return null;

      var typeDescriptor = ItemsSourceHelper.GetCustomTypeDescriptor( itemsSource, itemType ) as DataItemTypeDescriptor;
      if( typeDescriptor == null )
        return null;

      var parameters = IndexerParametersParser.Parse( index ).ToArray();
      if( parameters.Length <= 0 )
        return null;

      var indexerDescriptor = typeDescriptor.GetIndexer( parameters );
      if( indexerDescriptor == null )
        return null;

      var supportsDBNull = ItemsSourceHelper.IsSupportingDBNull( itemsSource );

      return new PropertyDescriptionRoute( new IndexerDescriptorPropertyDescription( indexerDescriptor, supportsDBNull, displayable ) );
    }

    private static Dictionary<string, ForeignKeyConstraint> GetForeignKeyConstraints( ConstraintCollection constraints )
    {
      var foreignKeyConstraints = new Dictionary<string, ForeignKeyConstraint>();

      // Detect every ForeignKeyConstraints
      foreach( Constraint constraint in constraints )
      {
        var foreignKeyConstraint = constraint as ForeignKeyConstraint;
        if( foreignKeyConstraint == null )
          continue;

        // We only support auto-detection when the ForeignKey is composed of a single column
        if( ( foreignKeyConstraint.Columns != null ) && ( foreignKeyConstraint.Columns.Length == 1 ) )
        {
          foreignKeyConstraints.Add( foreignKeyConstraint.Columns[ 0 ].ColumnName, foreignKeyConstraint );
        }
      }

      return foreignKeyConstraints;
    }

    private static DataGridForeignKeyDescription GetDataGridForeignKeyDescriptionForEnum( Type enumType )
    {
      if( ( enumType == null ) || !enumType.IsEnum )
        return null;

      var description = new DataGridForeignKeyDescription();

      // Using "." as default value path will revert to Self when used as SelectedValuePath when bound to a DataGridForeignKeyDictionary or ComboBox (default editor)
      description.ValuePath = ".";
      description.ItemsSource = Enum.GetValues( enumType );
      description.IsAutoCreated = true;

      return description;
    }

    private static DataTableForeignKeyDescription GetDataGridForeignKeyDescriptionForForeignKeyConstraint( ForeignKeyConstraint foreignKeyConstraint )
    {
      if( foreignKeyConstraint == null )
        return null;

      var columns = foreignKeyConstraint.Columns;
      if( ( columns == null ) || ( columns.Length != 1 ) )
        return null;

      var description = new DataTableForeignKeyDescription();
      description.ForeignKeyConstraint = foreignKeyConstraint;
      description.IsAutoCreated = true;

      return description;
    }

    private static DataGridItemPropertyBase CreateItemPropertyFromPropertyDescription( PropertyDescription propertyDescription )
    {
      if( propertyDescription == null )
        return null;

      return new DataGridItemProperty(
                   propertyDescription.Name,
                   propertyDescription.PropertyDescriptor,
                   propertyDescription.DisplayName,
                   propertyDescription.XPath,
                   propertyDescription.Path,
                   propertyDescription.DataType,
                   true,
                   propertyDescription.IsReadOnly,
                   propertyDescription.OverrideReadOnlyForInsertion,
                   propertyDescription.IsDisplayable,
                   propertyDescription.IsSubRelationship,
                   propertyDescription.ForeignKeyDescription );
    }

    internal static List<DataGridDetailDescription> CreateDetailDescriptions( Type itemType, IEnumerable enumeration )
    {
      var dataTable = enumeration as DataTable;
      if( dataTable == null )
      {
        var dataView = enumeration as DataView;
        if( dataView != null )
        {
          dataTable = dataView.Table;
        }
      }

      if( dataTable != null )
        return ItemsSourceHelper.CreateDetailDescriptions( dataTable );

      if( itemType != null )
      {
        //We do not support automatic Master/Detail detection with an Xml source
        if( typeof( XmlNode ).IsAssignableFrom( itemType ) )
          return new List<DataGridDetailDescription>( 0 );

        //Unbound mode, we do not support Master/Detail in this scenario.
        if( typeof( DataRow ).IsAssignableFrom( itemType ) )
          return new List<DataGridDetailDescription>( 0 );

        //we do not support Master/Details when Item is a Value type...
        if( ItemsSourceHelper.IsValueType( itemType ) )
          return new List<DataGridDetailDescription>( 0 );

        //Check if the object is a Entity Framework Entity, before checking for IEnumerable (since Entity Framework does have IEnumerable
        //properties, but require special handling )...
        if( ItemsSourceHelper.IsEntityFramework( itemType ) )
          return ItemsSourceHelper.CreateDetailDescriptionsForEntityFramework( itemType );

        //If the first item maps to an object that implements IEnumerable, expand that as a Relation ( and only that )...
        if( typeof( IEnumerable ).IsAssignableFrom( itemType ) )
          return ItemsSourceHelper.CreateDetailDescriptionsForEnumerable();

        if( typeof( IListSource ).IsAssignableFrom( itemType ) )
          return ItemsSourceHelper.CreateDetailDescriptionsForListSource();
      }

      //If the Source collection implements ITypedList
      var typedList = enumeration as ITypedList;
      if( typedList != null )
        return ItemsSourceHelper.GetDataGridDetailDescriptions( typedList.GetItemProperties( null ) );

      var customTypeDescriptor = ItemsSourceHelper.GetCustomTypeDescriptor( enumeration, itemType );
      if( customTypeDescriptor != null )
        return ItemsSourceHelper.GetDataGridDetailDescriptions( customTypeDescriptor.GetProperties() );

      if( itemType != null )
        return ItemsSourceHelper.GetDataGridDetailDescriptions( TypeDescriptor.GetProperties( itemType ) );

      return new List<DataGridDetailDescription>( 0 );
    }

    internal static List<DataGridDetailDescription> CreateDetailDescriptions( DataTable dataTable )
    {
      if( dataTable == null )
        return new List<DataGridDetailDescription>( 0 );

      return ItemsSourceHelper.CreateDetailDescriptionsForDataTable( dataTable );
    }

    private static List<DataGridDetailDescription> CreateDetailDescriptionsForDataTable( DataTable dataTable )
    {
      var detailDescriptions = new List<DataGridDetailDescription>( dataTable.ChildRelations.Count );

      foreach( DataRelation relation in dataTable.ChildRelations )
      {
        var description = new DataRelationDetailDescription( relation );
        description.IsAutoCreated = true;
        description.IsInitialized = true;
        detailDescriptions.Add( description );
      }

      return detailDescriptions;
    }

    private static List<DataGridDetailDescription> CreateDetailDescriptionsForEntityFramework( Type type )
    {
      var detailDescriptions = new List<DataGridDetailDescription>();

      // Gets all the public properties of the type.
      var propertyInfos = type.GetProperties( BindingFlags.Instance | BindingFlags.Public );

      // Loop throught the properties to build up the detail descriptions.
      foreach( var propertyInfo in propertyInfos )
      {
        // We must use Reflection to check for the EntityFramework types.
        var propertyType = propertyInfo.PropertyType;

        // The property must be of type RelatedEnd and IEnumerable to continue.
        if( ( propertyType.BaseType != null )
          && ( propertyType.BaseType.FullName == "System.Data.Objects.DataClasses.RelatedEnd" )
          && ( typeof( IEnumerable ).IsAssignableFrom( propertyType ) ) )
        {
          detailDescriptions.Add( new EntityDetailDescription( propertyInfo.Name ) );
        }
      }

      return detailDescriptions;
    }

    private static List<DataGridDetailDescription> CreateDetailDescriptionsForEnumerable()
    {
      var detailDescriptions = new List<DataGridDetailDescription>( 1 );

      var description = new EnumerableDetailDescription();
      description.IsAutoCreated = true;
      description.IsInitialized = true;
      detailDescriptions.Add( description );

      return detailDescriptions;
    }

    private static List<DataGridDetailDescription> CreateDetailDescriptionsForListSource()
    {
      var detailDescriptions = new List<DataGridDetailDescription>( 1 );

      var description = new ListSourceDetailDescription();
      description.IsAutoCreated = true;
      description.IsInitialized = true;
      detailDescriptions.Add( description );

      return detailDescriptions;
    }

    private static List<DataGridDetailDescription> GetDataGridDetailDescriptions( PropertyDescriptorCollection properties )
    {
      var detailDescriptions = new List<DataGridDetailDescription>( properties.Count );

      foreach( PropertyDescriptor property in properties )
      {
        // We only create details for properties that are browsable.
        if( !property.IsBrowsable )
          continue;

        if( ItemsSourceHelper.IsASubRelationship( property.PropertyType ) )
        {
          var description = new PropertyDetailDescription( property );
          description.IsAutoCreated = true;
          description.IsInitialized = true;
          detailDescriptions.Add( description );
        }
      }

      return detailDescriptions;
    }

    private static bool IsEditableObjectInsertedOrRemovedFromDataSourceAutomatically( IEditableObject item )
    {
      return ( item is DataRowView )
          || ( item is DataRow );
    }

    #region DataTablePropertyDescription Private Class

    private sealed class DataTablePropertyDescription : PropertyDescription
    {
      internal DataTablePropertyDescription( PropertyDescriptor propertyDescriptor, DataColumn column, IDictionary<string, ForeignKeyConstraint> constraints, bool isDisplayable )
      {
        if( propertyDescriptor == null )
          throw new ArgumentNullException( "propertyDescriptor" );

        m_propertyDescriptor = propertyDescriptor;
        m_column = column;
        m_isDisplayable = isDisplayable;

        if( constraints != null )
        {
          ForeignKeyConstraint constraint;
          if( constraints.TryGetValue( propertyDescriptor.Name, out constraint ) )
          {
            m_foreignKeyDescription = ItemsSourceHelper.GetDataGridForeignKeyDescriptionForForeignKeyConstraint( constraint );
          }
        }
      }

      internal override string Name
      {
        get
        {
          return m_propertyDescriptor.Name;
        }
      }

      internal override string DisplayName
      {
        get
        {
          if( m_column != null )
            return m_column.Caption;

          return m_propertyDescriptor.DisplayName;
        }
      }

      internal override Type DataType
      {
        get
        {
          return m_propertyDescriptor.PropertyType;
        }
      }

      internal override PropertyDescriptor PropertyDescriptor
      {
        get
        {
          return m_propertyDescriptor;
        }
      }

      internal override bool IsReadOnly
      {
        get
        {
          return m_propertyDescriptor.IsReadOnly;
        }
      }

      internal override bool SupportDBNull
      {
        get
        {
          return ( m_column != null )
              && ( m_column.AllowDBNull );
        }
      }

      internal override bool IsBrowsable
      {
        get
        {
          return m_propertyDescriptor.IsBrowsable;
        }
      }

      internal override bool IsDisplayable
      {
        get
        {
          return m_isDisplayable;
        }
      }

      internal override bool IsSubRelationship
      {
        get
        {
          return ( m_column == null )
              && ( ItemsSourceHelper.IsASubRelationship( this.DataType ) );
        }
      }

      internal override DataGridForeignKeyDescription ForeignKeyDescription
      {
        get
        {
          return m_foreignKeyDescription;
        }
      }

      public override int GetHashCode()
      {
        return m_propertyDescriptor.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as DataTablePropertyDescription;
        if( target == null )
          return false;

        return ( object.Equals( target.m_propertyDescriptor, m_propertyDescriptor ) )
            && ( object.Equals( target.m_column, m_column ) );
      }

      private readonly PropertyDescriptor m_propertyDescriptor;
      private readonly DataColumn m_column;
      private readonly bool m_isDisplayable;
      private readonly DataGridForeignKeyDescription m_foreignKeyDescription;
    }

    #endregion

    #region ItemPropertyPropertyDescription Private Class

    private sealed class ItemPropertyPropertyDescription : PropertyDescription
    {
      internal ItemPropertyPropertyDescription( DataGridItemPropertyBase itemProperty )
      {
        if( itemProperty == null )
          throw new ArgumentNullException( "itemProperty" );

        m_itemProperty = itemProperty;
        m_propertyDescriptor = itemProperty.GetPropertyDescriptorForBinding();
        Debug.Assert( m_propertyDescriptor != null );
      }

      internal override string Name
      {
        get
        {
          return m_itemProperty.Name;
        }
      }

      internal override string DisplayName
      {
        get
        {
          return m_propertyDescriptor.DisplayName;
        }
      }

      internal override Type DataType
      {
        get
        {
          return m_propertyDescriptor.PropertyType;
        }
      }

      internal override PropertyDescriptor PropertyDescriptor
      {
        get
        {
          return m_propertyDescriptor;
        }
      }

      internal override bool IsReadOnly
      {
        get
        {
          return m_propertyDescriptor.IsReadOnly;
        }
      }

      internal override bool OverrideReadOnlyForInsertion
      {
        get
        {
          return m_itemProperty.OverrideReadOnlyForInsertion.GetValueOrDefault( false );
        }
      }

      internal override bool IsBrowsable
      {
        get
        {
          return m_propertyDescriptor.IsBrowsable;
        }
      }

      internal override bool IsDisplayable
      {
        get
        {
          return m_itemProperty.IsDisplayable;
        }
      }

      internal override bool IsSubRelationship
      {
        get
        {
          return m_itemProperty.IsASubRelationship;
        }
      }

      internal override DataGridForeignKeyDescription ForeignKeyDescription
      {
        get
        {
          return m_itemProperty.ForeignKeyDescription;
        }
      }

      public override int GetHashCode()
      {
        return m_itemProperty.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as ItemPropertyPropertyDescription;
        if( target == null )
          return false;

        return object.Equals( target.m_itemProperty, m_itemProperty );
      }

      private readonly DataGridItemPropertyBase m_itemProperty;
      private readonly PropertyDescriptor m_propertyDescriptor;
    }

    #endregion

    #region PropertyDescriptorPropertyDescription Private Class

    private class PropertyDescriptorPropertyDescription : PropertyDescription
    {
      internal PropertyDescriptorPropertyDescription( PropertyDescriptor propertyDescriptor, bool supportDBNull, bool isDisplayable )
      {
        if( propertyDescriptor == null )
          throw new ArgumentNullException( "propertyDescriptor" );

        m_propertyDescriptor = propertyDescriptor;
        m_supportDBNull = supportDBNull;
        m_isDisplayable = isDisplayable;
        m_foreignKeyDescription = ItemsSourceHelper.GetDataGridForeignKeyDescriptionForEnum( propertyDescriptor.PropertyType );
      }

      internal override string Name
      {
        get
        {
          return m_propertyDescriptor.Name;
        }
      }

      internal override string DisplayName
      {
        get
        {
          return m_propertyDescriptor.DisplayName;
        }
      }

      internal override Type DataType
      {
        get
        {
          return m_propertyDescriptor.PropertyType;
        }
      }

      internal override PropertyDescriptor PropertyDescriptor
      {
        get
        {
          return m_propertyDescriptor;
        }
      }

      internal override bool IsReadOnly
      {
        get
        {
          return m_propertyDescriptor.IsReadOnly;
        }
      }

      internal override bool SupportDBNull
      {
        get
        {
          return m_supportDBNull;
        }
      }

      internal override bool IsBrowsable
      {
        get
        {
          return m_propertyDescriptor.IsBrowsable;
        }
      }

      internal override bool IsDisplayable
      {
        get
        {
          return m_isDisplayable;
        }
      }

      internal override bool IsSubRelationship
      {
        get
        {
          return ItemsSourceHelper.IsASubRelationship( this.DataType );
        }
      }

      internal override DataGridForeignKeyDescription ForeignKeyDescription
      {
        get
        {
          return m_foreignKeyDescription;
        }
      }

      public override int GetHashCode()
      {
        return m_propertyDescriptor.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as PropertyDescriptorPropertyDescription;
        if( target == null )
          return false;

        return ( object.Equals( target.m_propertyDescriptor, m_propertyDescriptor ) )
            && ( target.m_supportDBNull == m_supportDBNull );
      }

      private readonly PropertyDescriptor m_propertyDescriptor;
      private readonly bool m_supportDBNull;
      private readonly bool m_isDisplayable;
      private readonly DataGridForeignKeyDescription m_foreignKeyDescription;
    }

    #endregion

    #region NamedPropertyDescriptorPropertyDescription Private Class

    private sealed class NamedPropertyDescriptorPropertyDescription : PropertyDescriptorPropertyDescription
    {
      internal NamedPropertyDescriptorPropertyDescription( string name, PropertyDescriptor propertyDescriptor, bool supportDBNull, bool isDisplayable )
        : base( propertyDescriptor, supportDBNull, isDisplayable )
      {
        if( name == null )
          throw new ArgumentNullException( "name" );

        m_name = name;
      }

      internal override string Name
      {
        get
        {
          return m_name;
        }
      }

      public override int GetHashCode()
      {
        return m_name.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as NamedPropertyDescriptorPropertyDescription;
        if( target == null )
          return false;

        return ( target.m_name == m_name )
            && ( base.Equals( target ) );
      }

      private readonly string m_name;
    }

    #endregion

    #region IndexerDescriptorPropertyDescription Private Class

    private class IndexerDescriptorPropertyDescription : PropertyDescriptorPropertyDescription
    {
      internal IndexerDescriptorPropertyDescription( DataItemIndexerDescriptor indexerDescriptor, bool supportDBNull, bool isDisplayable )
        : base( indexerDescriptor, supportDBNull, isDisplayable )
      {
      }

      public override int GetHashCode()
      {
        return base.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        return ( obj is IndexerDescriptorPropertyDescription )
            && ( base.Equals( obj ) );
      }

      internal override PropertyRouteSegment ToPropertyRouteSegment()
      {
        var descriptor = ( DataItemIndexerDescriptor )this.PropertyDescriptor;

        return new PropertyRouteSegment( PropertyRouteSegmentType.Indexer, descriptor.IndexerParameters );
      }
    }

    #endregion

    #region JaggedArrayPropertyDescription Private Class

    private sealed class JaggedArrayPropertyDescription : PropertyDescription
    {
      internal JaggedArrayPropertyDescription( int index, Type dataType, bool isDisplayable )
      {
        if( index < 0 )
          throw new ArgumentException( "index" );

        if( dataType == null )
          throw new ArgumentNullException( "dataType" );

        m_name = ".[" + index.ToString( CultureInfo.InvariantCulture ) + "]";
        m_dataType = dataType;
        m_isDisplayable = isDisplayable;
        m_propertyDescriptor = new JaggedArrayPropertyDescriptor( index, dataType );
        m_foreignKeyDescription = ItemsSourceHelper.GetDataGridForeignKeyDescriptionForEnum( dataType );
      }

      internal override string Name
      {
        get
        {
          return m_name;
        }
      }

      internal override string DisplayName
      {
        get
        {
          return m_name;
        }
      }

      internal override Type DataType
      {
        get
        {
          return m_dataType;
        }
      }

      internal override PropertyDescriptor PropertyDescriptor
      {
        get
        {
          return m_propertyDescriptor;
        }
      }

      internal override bool IsReadOnly
      {
        get
        {
          return false;
        }
      }

      internal override bool IsDisplayable
      {
        get
        {
          return m_isDisplayable;
        }
      }

      internal override bool IsSubRelationship
      {
        get
        {
          return ItemsSourceHelper.IsASubRelationship( this.DataType );
        }
      }

      internal override DataGridForeignKeyDescription ForeignKeyDescription
      {
        get
        {
          return m_foreignKeyDescription;
        }
      }

      public override int GetHashCode()
      {
        return m_name.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as JaggedArrayPropertyDescription;
        if( target == null )
          return false;

        return ( target.m_name == m_name )
            && ( target.m_dataType == m_dataType );
      }

      private readonly string m_name;
      private readonly Type m_dataType;
      private readonly bool m_isDisplayable;
      private readonly PropertyDescriptor m_propertyDescriptor;
      private readonly DataGridForeignKeyDescription m_foreignKeyDescription;
    }

    #endregion

    #region EntityFrameworkPropertyDescription Private Class

    private sealed class EntityFrameworkPropertyDescription : PropertyDescription
    {
      internal EntityFrameworkPropertyDescription( PropertyDescriptor propertyDescriptor, bool isDisplayable )
      {
        if( propertyDescriptor == null )
          throw new ArgumentNullException( "propertyDescriptor" );

        var attribute = propertyDescriptor.Attributes[ typeof( EdmScalarPropertyAttribute ) ] as EdmScalarPropertyAttribute;
        if( attribute != null )
        {
          m_isEntityKey = attribute.EntityKeyProperty;
          m_supportDBNull = attribute.IsNullable;
        }
        else
        {
          m_isEntityKey = false;
          m_supportDBNull = false;
        }

        m_isDisplayable = isDisplayable;
        m_propertyDescriptor = propertyDescriptor;
      }

      internal override string Name
      {
        get
        {
          return m_propertyDescriptor.Name;
        }
      }

      internal override string DisplayName
      {
        get
        {
          return m_propertyDescriptor.DisplayName;
        }
      }

      internal override Type DataType
      {
        get
        {
          return m_propertyDescriptor.PropertyType;
        }
      }

      internal override PropertyDescriptor PropertyDescriptor
      {
        get
        {
          return m_propertyDescriptor;
        }
      }

      internal override bool IsReadOnly
      {
        get
        {
          return ( m_isEntityKey )
              || ( m_propertyDescriptor.IsReadOnly );
        }
      }

      internal override bool OverrideReadOnlyForInsertion
      {
        get
        {
          return ( m_isEntityKey )
              && ( !m_propertyDescriptor.IsReadOnly );
        }
      }

      internal override bool SupportDBNull
      {
        get
        {
          return m_supportDBNull;
        }
      }

      internal override bool IsBrowsable
      {
        get
        {
          return m_propertyDescriptor.IsBrowsable;
        }
      }

      internal override bool IsDisplayable
      {
        get
        {
          return m_isDisplayable;
        }
      }

      internal override bool IsSubRelationship
      {
        get
        {
          return ItemsSourceHelper.IsASubRelationship( this.DataType );
        }
      }

      public override int GetHashCode()
      {
        return m_propertyDescriptor.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as EntityFrameworkPropertyDescription;
        if( target == null )
          return false;

        return object.Equals( target.m_propertyDescriptor, m_propertyDescriptor );
      }

      private readonly PropertyDescriptor m_propertyDescriptor;
      private readonly bool m_isEntityKey;
      private readonly bool m_supportDBNull;
      private readonly bool m_isDisplayable;
    }

    #endregion

    #region ValueTypePropertyDescription Private Class

    private sealed class ValueTypePropertyDescription : PropertyDescription
    {
      internal ValueTypePropertyDescription( Type dataType, bool supportDBNull, DataGridForeignKeyDescription foreignKeyDescription, bool isDisplayable )
      {
        if( dataType == null )
          throw new ArgumentNullException( "dataType" );

        m_dataType = dataType;
        m_supportDBNull = supportDBNull;
        m_isDisplayable = isDisplayable;
        m_foreignKeyDescription = foreignKeyDescription;
      }

      internal override string Name
      {
        get
        {
          return ".";
        }
      }

      internal override string DisplayName
      {
        get
        {
          return string.Empty;
        }
      }

      internal override Type DataType
      {
        get
        {
          return m_dataType;
        }
      }

      internal override string Path
      {
        get
        {
          return this.Name;
        }
      }

      internal override bool SupportDBNull
      {
        get
        {
          return m_supportDBNull;
        }
      }

      internal override bool IsDisplayable
      {
        get
        {
          return m_isDisplayable;
        }
      }

      internal override bool IsSubRelationship
      {
        get
        {
          return ItemsSourceHelper.IsASubRelationship( this.DataType );
        }
      }

      internal override DataGridForeignKeyDescription ForeignKeyDescription
      {
        get
        {
          return m_foreignKeyDescription;
        }
      }

      public override int GetHashCode()
      {
        return m_dataType.GetHashCode();
      }

      public override bool Equals( object obj )
      {
        var target = obj as ValueTypePropertyDescription;
        if( target == null )
          return false;

        return ( target.m_dataType == m_dataType );
      }

      internal override PropertyRouteSegment ToPropertyRouteSegment()
      {
        return PropertyRouteSegment.Self;
      }

      private readonly Type m_dataType;
      private readonly bool m_supportDBNull;
      private readonly bool m_isDisplayable;
      private readonly DataGridForeignKeyDescription m_foreignKeyDescription;
    }

    #endregion
  }
}
